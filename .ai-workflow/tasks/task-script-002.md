# 任务: TASK-SCRIPT-002 M17.F1 Script SelfObject/Transform 解耦收敛

## TaskId
`TASK-SCRIPT-002`

## 目标（Goal）
将脚本访问形状从 `IScriptSelfTransform` 收敛为 `SelfObject + Transform`，并移除 `Engine.Scripting` 对 `Engine.Scene` 的残余项目级依赖，使 `Transform` 继续作为 `Scene` 原生组件存在，而 `Script` 只通过统一 self-object 访问面消费它。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M17-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M17.F1`

## 执行代理（ExecutionAgent）
Exec-Scripting

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Scripting

## 次级模块（SecondaryModules）
- Engine.App
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scripting.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M17-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-APP-011`

## 里程碑上下文（MilestoneContext）
- 这张卡是 M17 scripting foundation 落地后的结构收敛迭代，不是新能力扩张。当前主链路已经能跑，但脚本侧仍以 `IScriptSelfTransform` 暴露 self 访问面，而且 `Engine.Scripting.csproj` 还残留对 `Engine.Scene.csproj` 的直接引用。
- 本卡承担的是“像 Unity 一样按原生 component 视角消费 Transform”的接口收敛，以及残余依赖回收；不承担通用 `TryGetComponent<T>` 系统、不扩到 `MeshRenderer/Camera`、不改 Scene bridge 的跨对象边界。
- 直接影响本卡实现的背景包括：`Transform` 应继续归 `Engine.Scene` 作为原生 runtime component；`Engine.Scene` 不应知道 `Engine.Scripting`；脚本世界应该通过宿主对象拿原生 `Transform`，而不是把 `Transform` 变成脚本实现。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.Scripting -> Engine.Scene` 原本是 M17 的允许依赖方向，但当前代码形态已接近只消费抽象，具备进一步回收具体项目引用的条件。
  - `Engine.Scene` 只暴露窄的 self-object bind/transform bridge，不允许跨对象查询。
  - `RotateSelf` 作为内置 sample，继续通过自身对象的 Transform 驱动 rotation。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 `Transform` 变成脚本实现对象或把组件所有权从 `Scene` 挪给 `Script`。
  - 不允许引入通用组件查询系统或 `TryGetComponent<T>` 风格的更大抽象。
  - 不允许恢复或保留 `Engine.Scripting` 对 `Engine.Scene` 的直接项目依赖。
  - 不允许扩大脚本访问面到 `MeshRenderer`、`Camera`、跨对象查询或任意 runtime 集合。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - 基于当前仓库已落地的 `IScriptSelfTransform`、`ScriptContext`、`ScriptBindingDescription` 和 `SceneScriptObjectHandle` 形状，这张卡明确要求把脚本访问面升级为 `SelfObject + Transform` 两层抽象：
    - `IScriptSelfObject`
    - `IScriptTransformComponent`
  - `ScriptContext` 应改为暴露 `Self`，并通过 `Self.Transform` 访问原生 transform；执行时不得继续把 `SelfTransform` 作为公开主入口保留。

## 实施说明（ImplementationNotes）
- 先在 `Engine.Scripting` 内收敛公开抽象：
  - 新增 `IScriptSelfObject`
  - 新增 `IScriptTransformComponent`
  - 让 `ScriptContext` 从 `SelfTransform` 改为 `Self`
  - 让 `ScriptBindingDescription` 绑定 `IScriptSelfObject`
- 再删除 `Engine.Scripting.csproj` 对 `Engine.Scene.csproj` 的直接引用，并确认 `Engine.Scripting` 源码不再依赖 `Scene` 命名类型。
- 然后在 `Engine.App` 适配层把 `SceneScriptObjectHandle` 包装成新的 self-object/transform 抽象，保持 Scene handle 只留在 App/Scene 边界。
- 最后迁移 `RotateSelfScript` 和测试：
  - 通过 `context.Self.Transform` 修改 rotation
  - 现有 bind/initialize/update 顺序保持不变
  - unknown script / script exception / shutdown-dispose 语义不退化

## 设计约束（DesignConstraints）
- 不允许修改 `Engine.Scene` 的“窄 self-object bind”方向，更不能把它扩成通用组件查询或跨对象访问 API。
- 不允许顺手重构为完整 `GameObject/Component` 脚本 API。
- 不允许在本卡中扩展 `MeshRenderer`、`Camera`、`Rigidbody` 等组件访问。
- 不允许改动 Editor Script UI、外部 DLL 加载、源码编译、热重载或 Play Mode。

## 失败与降级策略（FallbackBehavior）
- 若命名需要贴合现有仓库风格，允许在 `IScriptSelfObject` / `IScriptTransformComponent` 命名上做等价微调，但对外必须保持“self object 拿 transform”的两层语义。
- 若 `SceneScriptObjectHandle` 仍需要暂时留在 `App/Scene` 边界层，允许保留，但不得再进入 `Engine.Scripting` 公共 API 或项目依赖。
- 若实现中发现删除 `Engine.Scripting -> Engine.Scene` 项目引用后仍有隐藏编译依赖，必须先收敛到抽象层，不得重新加回 project reference 交差。
- 若为了图省事需要引入更重的泛化组件访问系统，必须停工回退修卡，因为这超出了这张卡的收敛范围。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scripting/Engine.Scripting.csproj`
  - `src/Engine.Scripting/IScriptSelfTransform.cs`
  - `src/Engine.Scripting/ScriptContext.cs`
  - `src/Engine.Scripting/ScriptBindingDescription.cs`
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/SceneRuntimeContracts.cs`
  - `src/Engine.Scene/Runtime/SceneScriptObjectHandle.cs`
- 相关测试入口：
  - `tests/Engine.Scripting.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Scene.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-script-001.md`
  - `.ai-workflow/tasks/task-scene-019.md`
  - `.ai-workflow/tasks/task-app-011.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M17-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M17-2026-05-02 > PlanningDecisions`
  - `PLAN-M17-2026-05-02 > SceneBridgeDesign`
  - `PLAN-M17-2026-05-02 > AppIntegrationDesign`
  - `PLAN-M17-2026-05-02 > Risks`
  - 上述引用在本卡属于“字段关系已定的参考实现约束”；本卡不是新设计一套完整脚本组件系统，而是把现有语义收敛到更像 Unity 的 self-object/component 访问面。

## 范围（Scope）
- AllowedModules:
  - Engine.Scripting
  - Engine.App
- AllowedFiles:
  - script self-object / transform abstractions
  - script context / binding model
  - App adapter wrapper
  - Scripting/App tests
- AllowedPaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不改 `Engine.Scene` 公开 bridge 范围
- 不引入通用组件查询系统
- 不扩展到 `MeshRenderer` / `Camera`
- 不做 Editor Script UI
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - self-object / transform 抽象的具体类型命名可以按仓库风格微调，但不能回退为单一 `SelfTransform` 专用入口。
- 处理规则：
  - 若问题影响 `Transform` 原生组件归属、`Engine.Scripting` 项目依赖回收或脚本访问边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 目标、抽象方向、边界限制、测试口径和不做事项都已经明确。
  - 执行者无需回看完整讨论也能知道这张卡是在做“接口收敛”，不是更大范围的脚本系统升级。
  - 失败时该停工还是可局部调整已经写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡同时触碰脚本公开 API、App 适配层和边界收敛，做偏就会重新引入 `Scene` 具体类型泄漏。
  - 还要在“更像 Unity”与“不引入更重组件查询系统”之间精确控范围。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scripting -> Engine.Core`
  - `Engine.Scripting -> Engine.Contracts`
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Scripting`
- ForbiddenDependsOn:
  - `Engine.Scripting -> Engine.Scene`
  - `Engine.Scripting -> Engine.Render`
  - `Engine.Scripting -> Engine.Editor`
  - `Engine.Scripting -> Engine.Editor.App`
  - `Engine.App` 直接依赖 Scene runtime object/component 集合

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal` 与 `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` 通过
- Smoke: `RotateSelf` 仍在 render 前通过 `Self.Transform` 驱动 rotation；`Engine.Scripting` 项目与源码不再引用 `Engine.Scene`
- Perf: 不新增逐帧对象查询、反射式组件分发、额外脚本绑定轮询或 render side effect

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Done

## 完成度（Completion）
`100`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCRIPT-002.md`
- ClosedAt: `2026-05-03 13:41`
- Summary:
  - `Engine.Scripting` script access now exposes `ScriptContext.Self.Transform` through `IScriptSelfObject` and `IScriptTransformComponent`.
  - Removed the `Engine.Scripting -> Engine.Scene` project reference while keeping Transform as a contracts-level value consumed by Scripting.
  - App now adapts `SceneScriptObjectHandle` into self-object/transform wrappers at the Scene/Scripting bridge.
  - `RotateSelf` and tests now use `context.Self.Transform` without changing bind/update/fail-fast behavior.
- FilesChanged:
  - `src/Engine.Scripting/Engine.Scripting.csproj`
  - `src/Engine.Scripting/IScriptSelfObject.cs`
  - `src/Engine.Scripting/IScriptTransformComponent.cs`
  - `src/Engine.Scripting/IScriptSelfTransform.cs`
  - `src/Engine.Scripting/ScriptBindingDescription.cs`
  - `src/Engine.Scripting/ScriptContext.cs`
  - `src/Engine.Scripting/ScriptRuntime.cs`
  - `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-script-002.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/2026-05/TASK-SCRIPT-002.md`
  - `.ai-workflow/archive/archive-index.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Test: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal` passed, 11/11 tests.
  - Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` passed, 12/12 tests.
  - Smoke: `ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=0.05 dotnet run --project src/Engine.App/Engine.App.csproj --no-restore --nologo` exited 0.
  - Boundary: `Engine.Scripting` no longer has actual `Engine.Scene` project/source dependency; only boundary test assertions mention `Engine.Scene`.
  - Perf: pass; no per-frame object query, reflection component dispatch, extra script binding polling, or render side effect introduced.
- ModuleAttributionCheck: pass
