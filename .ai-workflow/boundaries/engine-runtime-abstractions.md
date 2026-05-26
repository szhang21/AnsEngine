# Engine.Runtime.Abstractions 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Runtime.Abstractions`
- 版本：`v0.1`
- 负责人：`待指定`
- 生效日期：`2026-05-11`
- 关联任务卡：`TASK-RABS-001`

## 2) 目标与范围

- 模块目标：提供 runtime object/component 的最小公共抽象，使后续 Scene runtime 与 Scripting 能共享窄接口。
- 适用范围：runtime object identity、typed component lookup、runtime transform component abstraction。
- 非适用范围：scene traversal、component collection mutation、update 生命周期、script loading、physics、render submission、editor authoring。

## 3) 职责（Responsibilities）

- 定义 `IRuntimeComponent` marker interface。
- 定义 `IRuntimeObject` 的 `ObjectId`、`ObjectName` 与 typed component lookup。
- 定义 `IRuntimeTransformComponent`，使用 `Engine.Contracts.SceneTransform` 表达 local transform。
- 保持公共 API 最小，避免提前承诺 M23+ runtime lifecycle。

## 4) 非职责（Non-Responsibilities）

- 不负责 concrete runtime object/component storage。
- 不负责 update、fixed update、script execution 或 lifecycle dispatch。
- 不负责跨对象查询、parent/children traversal、scene graph traversal。
- 不负责 add/remove component mutation API。
- 不负责 SceneData DTO、Editor GUI、Render、Physics 或 App composition。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Contracts`
- 可使用基础库/第三方：
  - `.NET` 标准库

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.Scene`
  - `Engine.Scripting`
  - `Engine.App`
  - `Engine.Render`
  - `Engine.Physics`
  - `Engine.SceneData`
  - `Engine.Editor`
  - `Engine.Editor.App`
- 禁止跨层调用模式：
  - 在抽象层暴露 scene traversal、update 或 component mutation 行为
  - 在抽象层引用 concrete Scene runtime object/component 类型
  - 在抽象层引用 Scripting context 或 App composition root

## 7) 公开接口（Public Interfaces）

- `IRuntimeComponent`
  - 用途：runtime component marker。

- `IRuntimeObject`
  - 用途：脚本和 runtime 共享的对象窄接口。
  - 成员：`ObjectId`、`ObjectName`、`GetComponent<T>()`、`HasComponent<T>()`。
  - 约束：`T : class, IRuntimeComponent`。

- `IRuntimeTransformComponent`
  - 用途：runtime transform component 窄接口。
  - 成员：`LocalTransform`、`SetLocalTransform(SceneTransform transform)`。
  - 约束：transform 类型来自 `Engine.Contracts`。

## 8) 数据与状态边界

- 模块内部可变状态：无。
- 外部可观察状态：仅接口 public shape。
- 线程模型与并发约束：不定义运行时线程模型。
- 资源生命周期：不拥有资源生命周期。

## 9) 质量门禁与验收

- Build 验收：`dotnet build AnsEngine.sln` 通过。
- Test 验收：`Engine.Runtime.Abstractions.Tests` 覆盖 API shape、依赖方向和禁止 API 名称。
- Smoke 验收：solution 可加载模块，接口 public shape 与 M22.1 计划一致。
- Perf 验收：仅新增接口模块，不引入 runtime 主路径成本。
- 边界验证项（必须逐条通过）：
  - [ ] `Engine.Runtime.Abstractions` 只引用 `Engine.Contracts`
  - [ ] 不引用 Scene/Scripting/App/Render/Physics/SceneData/Editor/Editor.App
  - [ ] 不暴露 update、traversal 或 add/remove component mutation API

## M24.1 Boundary Update

- Scope addition: `Engine.Runtime.Abstractions` may define runtime update lifecycle abstractions only.
- Public additions:
  - `IRuntimeUpdateComponent : IRuntimeComponent`
  - `RuntimeUpdateContext`
  - `RuntimeInputSnapshot`
  - `RuntimeKey`
  - `RuntimeUpdateResult`
  - `RuntimeUpdateFailure`
- Responsibility rule: this module defines lifecycle shape and frame input/result contracts, but does not own traversal, ordering, scheduler, component storage, script binding, physics, rendering, platform input, or app composition.
- Dependency rule remains unchanged: direct dependency is limited to `Engine.Contracts` and .NET standard library.
- Forbidden concrete dependencies remain: `Engine.Scene`, `Engine.Scripting`, `Engine.App`, `Engine.Physics`, `Engine.Runtime`, `Engine.Render`, `Engine.SceneData`, `Engine.Editor`, and `Engine.Editor.App`.
- Boundary verification for M24.1 must confirm update lifecycle APIs exist while fixed update, scheduler, traversal, scene query, and add/remove mutation APIs remain absent.

## 10) 变更记录（Boundary Change Log）
- 2026-05-13
  - Changed by: Execution-Agent
  - Task: `TASK-QA-025`
  - Change: M24 QA gate verified `Engine.Runtime.Abstractions` remains scheduler-free API shape only, with no concrete Scene/Scripting/App/Physics/Runtime/Render/SceneData dependencies.
  - Reason: Records M24 runtime component lifecycle gate evidence.
  - Risk and rollback: Low. MustFixCount=0; future lifecycle expansion must stay shape-only here unless a new boundary task approves concrete ownership changes.
- 2026-05-13
  - Changed by: Execution-Agent
  - Task: `TASK-RABS-002`
  - Change: Added M24 runtime update component lifecycle shape to the Runtime.Abstractions boundary, including update component, context, runtime input snapshot, runtime key, and fail-fast result/failure contracts.
  - Reason: Supports `PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE` / `M24.1` so downstream Scene, Scripting, Runtime, and App cards can share one scheduler-free update component contract.
  - Risk and rollback: Low. The change is public API shape only, keeps dependency direction unchanged, and introduces no traversal/scheduler/mutation implementation. Rollback is limited to removing the M24 API files and associated tests if downstream M24 cards are cancelled.

- 2026-05-11
  - 变更人：Execution-Agent
  - 变更内容：完成 M22 QA gate review，复验 `Engine.Runtime.Abstractions` 仅引用 `Engine.Contracts`，public API 仍限于 marker、object identity/typed lookup 与 transform abstraction。
  - 变更原因：支撑 `TASK-QA-023`，确认 Runtime.Abstractions foundation 可进入归档准备状态，未引入 update/traversal/component mutation 或跨对象查询能力。
  - 风险与回滚方案：当前未发现 MustFix；后续如扩展 updateable component lifecycle 或 hierarchy traversal，必须另立 M23+ 任务并重新审查边界。
- 2026-05-11
  - 变更人：Execution-Agent
  - 变更内容：新增 `Engine.Runtime.Abstractions` 边界合同，定义 runtime object/component 最小抽象、允许依赖、禁止依赖和禁止 API 面。
  - 变更原因：支撑 `TASK-RABS-001`，为 M22 Runtime Abstractions And Component Container 建立共享抽象地基。
  - 风险与回滚方案：当前仅暴露 marker、object lookup 与 transform abstraction；若后续需要 update/traversal/mutation，应另立任务卡并重新评审边界。
