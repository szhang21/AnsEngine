# Plan Archive: PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE

## PlanId

`PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE`

## Status

Closed

## CreatedAt

2026-05-27

## UpdatedAt

2026-05-13

## ChangeReason

Closeout after human signoff

## GoalSummary

- 本轮目标：修正当前 runtime 职责分散问题，把脚本从 App 外部调度的行为升级为真正的 runtime update component，并为后续脚本、物理、动画、信号系统打地基。
- 成功标准：
  - `Transform` 正式进入 runtime component container，`GetComponent<IRuntimeTransformComponent>()` 返回真实组件。
  - 新增可更新组件生命周期接口，Runtime 只调度实现 update lifecycle 的组件。
  - 脚本实例通过 runtime owner/component lookup 更新自身对象，不再依赖 `SceneScriptObjectHandle -> SceneScriptSelfObject -> SceneScriptTransformComponent` 专用链路。
  - 新增 `Engine.Runtime` 模块承载 scene/script/physics runtime update pipeline。
  - `Engine.App` 收敛为 platform/app host，不再直接拥有 script update 和 physics writeback 编排。
  - 现有 script -> physics -> scene state -> render 主路径不退化。

## Context

- M22 建立了 `Engine.Runtime.Abstractions`，但该模块只定义 object/component 窄接口，不负责 update/lifecycle dispatch。
- M22 后 `SceneRuntimeObject` 已实现 `IRuntimeObject` 和 typed component lookup，但 `Transform` 仍是特殊 core field，并未进入 component lookup。
- M17/M18 的 scripting runtime 通过 `ScriptContext.Self` 和 App 侧 self-object adapter 访问 Transform，脚本不是 runtime component lifecycle 的一部分。
- M20/M23 的 physics writeback 由 App 调度，App 已经承担 scene/script/physics update 顺序，逐步变成伪 runtime。
- 如果继续在 App 中追加 animation、audio、signal、fixed update 或 richer script lifecycle，App 会成为上帝对象，runtime ownership 会继续分裂。

## Scope

- In scope:
  - 扩展 `Engine.Runtime.Abstractions` 的 update component lifecycle public shape。
  - 将 `SceneTransformComponent` 纳入 `SceneRuntimeObject` component collection。
  - 调整 scripting update 模型，使 script behavior 作为 runtime update component 被驱动。
  - 新增 `Engine.Runtime` 模块和测试项目。
  - 将 runtime update pipeline 从 `Engine.App` 收敛到 `Engine.Runtime`。
  - 将 physics state sync/writeback orchestration 迁出 App。
  - App loop 改为调用 runtime tick。
  - 更新相关边界合同与测试。
- Out of scope:
  - external DLL loading
  - source compilation
  - hot reload
  - Editor Script UI
  - cross-object query
  - signal/event system
  - animation system
  - audio system
  - `IRuntimeFixedUpdateComponent`
  - dynamic physics solver changes
  - render pipeline redesign

## TechnicalDesign

### Dependency Direction

- `Engine.Runtime.Abstractions`:
  - allowed: `Engine.Contracts`, .NET standard library.
  - forbidden: `Engine.Scene`, `Engine.Scripting`, `Engine.App`, `Engine.Physics`, `Engine.Render`, `Engine.SceneData`, `Engine.Editor`, `Engine.Editor.App`.
- `Engine.Scene`:
  - may depend on `Engine.Runtime.Abstractions`.
  - must not depend on `Engine.Scripting` or `Engine.Runtime`.
  - owns concrete runtime object/component storage and render snapshot output.
- `Engine.Scripting`:
  - may depend on `Engine.Runtime.Abstractions` and `Engine.Contracts`.
  - must not depend on `Engine.Scene` or `Engine.App`.
  - owns script registry/factory/binding diagnostics and built-in script behavior types.
- `Engine.Runtime`:
  - may depend on `Engine.Scene`, `Engine.Scripting`, `Engine.Physics`, `Engine.SceneData`, `Engine.Runtime.Abstractions`, and `Engine.Contracts`.
  - owns runtime session and update pipeline orchestration.
- `Engine.App`:
  - remains composition root and platform host.
  - may depend on `Engine.Runtime`.
  - should no longer directly own script update or physics writeback ordering.

### Runtime Abstractions API Shape

Add:

```csharp
public interface IRuntimeUpdateComponent : IRuntimeComponent
{
    RuntimeUpdateResult Update(RuntimeUpdateContext context);
}
```

Add:

```csharp
public sealed record RuntimeUpdateContext(
    IRuntimeObject Owner,
    double DeltaSeconds,
    double TotalSeconds,
    RuntimeInputSnapshot Input);
```

Add:

```csharp
public readonly record struct RuntimeInputSnapshot
{
    public static RuntimeInputSnapshot Empty { get; }
    public bool AnyInputDetected { get; }
    public static RuntimeInputSnapshot FromKeys(params RuntimeKey[] keys);
    public bool IsKeyDown(RuntimeKey key);
}

public enum RuntimeKey
{
    W,
    A,
    S,
    D
}
```

Add a small fail-fast result:

```csharp
public sealed record RuntimeUpdateResult
{
    public bool IsSuccess { get; }
    public RuntimeUpdateFailure? Failure { get; }
}

public sealed record RuntimeUpdateFailure(
    string Message,
    string? ObjectId = null,
    string? ComponentType = null);
```

Design constraints:

- `IRuntimeComponent` remains a marker.
- Transform and MeshRenderer do not implement update just to fit the traversal.
- `RuntimeUpdateContext.Owner` is the only object entry for per-object update.
- No cross-object query in M24.
- No fixed update API in M24, but names must leave room for a future `IRuntimeFixedUpdateComponent`.

### Scene Component Container

- `SceneRuntimeObject` must construct one component collection containing:
  - `SceneTransformComponent` when present.
  - `SceneMeshRendererComponent` when present.
  - script/update components produced during runtime binding.
  - any future runtime components.
- `SceneRuntimeObject.Transform` may remain as a convenience property during migration, but it must reference the exact same `SceneTransformComponent` instance returned by `GetComponent<IRuntimeTransformComponent>()`.
- Snapshot and render item construction must read from the same transform instance.
- Remove or update tests that assert Transform is not part of typed component lookup.

### Scripting As Runtime Update Component

- Built-in scripts should update through `IRuntimeUpdateComponent`.
- `RotateSelf`:
  - reads speed property during bind/initialize.
  - on update, gets `IRuntimeTransformComponent` from `context.Owner`.
  - updates rotation through `SetLocalTransform`.
- `MoveOnInput`:
  - reads speed property during bind/initialize.
  - on update, reads `RuntimeInputSnapshot`.
  - updates position through `context.Owner.GetComponent<IRuntimeTransformComponent>()`.
- Missing Transform must produce deterministic runtime update failure before render.
- Script behavior must not receive a Scene-specific handle.
- `ScriptRuntime` should keep registry/factory/bind responsibility, but no longer own an independent per-frame update loop once `Engine.Runtime` drives components.
- During migration, an adapter layer is acceptable only if it does not preserve `SceneScriptSelfObject` as the long-term public path.

### Engine.Runtime Module

Introduce a runtime session, for example:

```csharp
public sealed class EngineRuntimeSession
{
    public RuntimeInitializationResult Initialize(SceneDescription sceneDescription);
    public RuntimeTickResult Tick(RuntimeTickContext context);
}
```

Recommended tick context:

```csharp
public sealed record RuntimeTickContext(
    double DeltaSeconds,
    double TotalSeconds,
    RuntimeInputSnapshot Input);
```

Responsibilities:

- Initialize Scene runtime from `SceneDescription`.
- Create `PhysicsWorld` from scene physics data.
- Bind Script components into runtime update components.
- Drive frame pipeline.
- Return deterministic diagnostics on script/physics failures.

First pipeline:

```text
Scene base update/statistics
  -> Runtime update components
  -> Physics apply/writeback
  -> Scene state ready for render
```

Runtime should own the moved equivalents of:

- `RuntimePhysicsOrchestrator`
- `ScenePhysicsWorldDefinitionBridge`
- script component binding from scene descriptions to runtime components

### App Host

- `ApplicationHost.Run()` should keep:
  - renderer initialize/shutdown
  - scene file load
  - asset/bootstrap mesh warmup
  - window event processing
  - input/time polling
  - render frame
  - present
  - exit code and shutdown behavior
- `ApplicationHost.Run()` should delegate runtime behavior to `EngineRuntimeSession`.
- App converts `Engine.Platform.InputSnapshot` to `RuntimeInputSnapshot`.
- App should no longer convert platform input directly to scripting input.
- App should not directly call `ScriptRuntime.Update(...)` or `RuntimePhysicsOrchestrator.ResolveAndWriteBack(...)`.

## Milestones

### M24.1 Runtime Abstractions Lifecycle Shape

- Primary module: `Engine.Runtime.Abstractions`
- Priority: P0
- Deliverables:
  - Add `IRuntimeUpdateComponent`.
  - Add `RuntimeUpdateContext`.
  - Add `RuntimeInputSnapshot` / `RuntimeKey`.
  - Add runtime update result/failure shape.
  - Update `engine-runtime-abstractions.md`.
- Completion definition:
  - API shape tests pass.
  - Dependency scan confirms no Scene/Scripting/App/Physics dependency.
  - No traversal, mutation, or scheduler implementation leaks into abstractions.

### M24.2 Transform Component Container Alignment

- Primary module: `Engine.Scene`
- Priority: P0
- Deliverables:
  - `SceneRuntimeObject.GetComponent<IRuntimeTransformComponent>()` returns the true transform.
  - Existing `Transform` convenience property and typed component lookup point to the same instance.
  - Snapshot/render paths read the same transform state.
  - Update scene boundary docs.
- Completion definition:
  - Transform component lookup tests pass.
  - Existing render/snapshot tests stay green.
  - No `Engine.Scene -> Engine.Scripting` or `Engine.Scene -> Engine.Runtime` dependency introduced.

### M24.3 Scripting Runtime Component Conversion

- Primary module: `Engine.Scripting`
- Priority: P0
- Deliverables:
  - Built-in script behavior uses runtime owner/component lookup.
  - `RotateSelf` and `MoveOnInput` work through `IRuntimeUpdateComponent`.
  - Remove long-term dependency on `SceneScriptSelfObject` adapter path.
  - Script binding returns runtime update components or equivalent bind output consumable by `Engine.Runtime`.
  - Update scripting boundary docs.
- Completion definition:
  - Scripts update Transform through `Owner.GetComponent<IRuntimeTransformComponent>()`.
  - Missing Transform returns deterministic failure.
  - Script order remains scene file order.
  - `Engine.Scripting` still does not reference `Engine.Scene`.

### M24.4 Engine.Runtime Module And Tick Pipeline

- Primary module: `Engine.Runtime`
- Priority: P0
- Deliverables:
  - New `Engine.Runtime` and test project.
  - Runtime session initialization from `SceneDescription`.
  - Runtime tick context/result.
  - Component update traversal.
  - Physics state sync/writeback migrated from App.
  - Runtime boundary contract.
- Completion definition:
  - Runtime tick drives Scene base update -> script/update components -> physics writeback.
  - Script and physics failures fail before render.
  - Consecutive frame script/physics state sync remains green.
  - Dependency direction matches TechnicalDesign.

### M24.5 App Host Contraction And QA Close

- Primary module: `Engine.App`
- Priority: P1
- Deliverables:
  - App delegates runtime behavior to `EngineRuntimeSession`.
  - App no longer owns script update or physics writeback pipeline.
  - Built-in script placement moved out of `ApplicationBootstrap.cs`.
  - Full build/test/smoke and boundary review.
  - Archive and close M24.
- Completion definition:
  - App loop order remains process events -> input/time -> runtime tick -> render -> present.
  - Existing headless smoke passes.
  - App tests prove no direct script/physics update orchestration remains.
  - M24 no-goals did not slip in.

## RuntimeRealityCheck

M24 changes the real runtime path.

Required real path:

```text
SceneData loads objects and script components
  -> Engine.Runtime initializes Scene runtime, PhysicsWorld, and script update components
  -> App polls Platform input/time
  -> App converts to RuntimeInputSnapshot
  -> Engine.Runtime.Tick updates Scene statistics
  -> Engine.Runtime.Tick calls update components with RuntimeUpdateContext.Owner
  -> script update mutates Transform through component lookup
  -> Engine.Runtime.Tick applies physics sync/writeback
  -> App renders the post-runtime-tick Scene state
```

Unit tests are required, but M24 is not complete unless App-level headless smoke proves the post-refactor runtime path still drives script movement/rotation before render.

## PriorityOrder

- P0: M24.1, M24.2, M24.3, M24.4
  - Reason: lifecycle interface, true component container, script component conversion, and runtime tick module are one architectural chain.
- P1: M24.5
  - Reason: App contraction depends on the runtime module being ready.
- P2: QA/archive polish after implementation cards pass.

## PlanningDecisions

- Do not make every `IRuntimeComponent` updateable.
- Use `IRuntimeUpdateComponent` for behavior components.
- Transform becomes a true component lookup result.
- Script instances become runtime update components.
- App is not the runtime scheduler.
- `Engine.Runtime.Abstractions` remains interface-only and scheduler-free.
- `Engine.Runtime` is a new concrete orchestration module.
- Do not implement external scripting, hot reload, Editor Script UI, signal system, or cross-object query in M24.

## HandoffToDispatch

Suggested task cards:

- `TASK-RABS-002`: M24 Runtime update component lifecycle shape
- `TASK-SCENE-024`: M24 Transform component container alignment
- `TASK-SCRIPT-005`: M24 Scripting runtime update component conversion
- `TASK-RUNTIME-001`: M24 Engine.Runtime module and tick pipeline
- `TASK-APP-023`: M24 App host contraction to Runtime session
- `TASK-QA-025`: M24 Runtime component lifecycle gate review and archive

Suggested dependencies:

- `TASK-RABS-002` first.
- `TASK-SCENE-024` depends on `TASK-RABS-002`.
- `TASK-SCRIPT-005` depends on `TASK-RABS-002` and `TASK-SCENE-024`.
- `TASK-RUNTIME-001` depends on `TASK-SCRIPT-005`.
- `TASK-APP-023` depends on `TASK-RUNTIME-001`.
- `TASK-QA-025` depends on all implementation cards.

Dispatch constraints:

- Keep each card to a single primary module.
- Do not expand into Editor UI or external script loading.
- Ensure new `Engine.Runtime` boundary is created before implementation touches App contraction.
- Any new source/test files must update matching boundary docs.

## TestPlan

- `Engine.Runtime.Abstractions.Tests`:
  - API shape for update component lifecycle.
  - runtime input snapshot W/A/S/D behavior.
  - dependency boundary scan.
- `Engine.Scene.Tests`:
  - Transform component lookup.
  - Transform convenience property and lookup share instance.
  - snapshot/render observe component-updated transform.
- `Engine.Scripting.Tests`:
  - built-in scripts consume `RuntimeUpdateContext`.
  - missing Transform failure.
  - multi-script update order.
  - no Scene dependency.
- `Engine.Runtime.Tests`:
  - runtime session initialization.
  - tick order.
  - update component traversal.
  - script failure before render.
  - physics sync after script update.
- `Engine.App.Tests`:
  - App calls runtime tick.
  - App no longer directly calls script update/physics orchestrator.
  - headless valid script scene updates before render.
  - failure shutdown behavior remains stable.
- Full gate:
  - `dotnet build AnsEngine.sln --nologo -v minimal`
  - `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`

## Risks

- Risk: high
  - Description: A shallow `Engine.Runtime` extraction could just move App spaghetti into a new project.
  - Mitigation: First fix component lifecycle and Transform lookup, then move scheduling.
- Risk: high
  - Description: Transform may accidentally exist in two states during migration.
  - Mitigation: Require convenience property and component lookup to reference the same instance.
- Risk: medium
  - Description: ScriptRuntime and Engine.Runtime responsibilities may overlap.
  - Mitigation: ScriptRuntime owns factory/bind diagnostics; Engine.Runtime owns per-frame traversal.
- Risk: medium
  - Description: App tests may overfit old internal ordering.
  - Mitigation: Move ordering assertions to Runtime tests and keep App tests at host boundary.
- Risk: low
  - Description: Runtime input duplicates ScriptInputSnapshot temporarily.
  - Mitigation: Treat ScriptInputSnapshot as migration target; RuntimeInputSnapshot becomes the frame input model.

## Assumptions

- M24 starts after M23 is closed.
- Multi Script component model remains.
- Existing SceneData Script schema remains `scriptId + properties`.
- Render remains App-owned for now.
- Physics state sync remains kinematic/static MVP behavior from M23.
- `net7.0` EOL warnings are existing residual warnings, not M24 blockers.
