# 任务: TASK-APP-010 M15 App 主循环 runtime update 接线

## TaskId
`TASK-APP-010`

## 目标（Goal）
让 `Engine.App` 在每帧 `RenderFrame()` 前稳定调用 scene runtime update，并由 App adapter 负责把时间与输入快照翻译成 `SceneUpdateContext`，从而打通 M15 的 runtime update 主链路。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M15-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M15.3`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Platform

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M15-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-016`

## 里程碑上下文（MilestoneContext）
- M15.3 的职责是让已有 App 主循环真正驱动 Scene runtime update，而不是让 Scene 只在初始化时拥有一次性状态。
- 本卡承担的是 App 层接口扩展、adapter 翻译和固定调用顺序，不承担 Scene 内部旋转逻辑、snapshot 诊断扩展或 QA 收口。
- 直接影响本卡实现的上游背景包括：`Engine.Scene` 定义自有 update context；App 只负责把 `TimeSnapshot` / `InputSnapshot` 翻译给 Scene；`RenderFrame()` 前必须完成 update；loader failure 和 render failure 仍要保持既有生命周期语义。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `ISceneRuntime` 保留 `InitializeScene(SceneDescription sceneDescription)`，并新增 `Update(SceneUpdateContext context)` 或等价入口。
  - `SceneRuntimeAdapter.Update` 调用 `SceneGraphService.UpdateRuntime(...)`。
  - `ApplicationHost.Run()` 每帧顺序固定为：
    1. `mWindowService.ProcessEvents()`
    2. `var input = mInputService.GetSnapshot()`
    3. `var time = mTimeService.Current`
    4. `mSceneRuntime.Update(new SceneUpdateContext(time.DeltaSeconds, time.TotalSeconds, input.AnyInputDetected))`
    5. `mRenderer.RenderFrame()`
    6. `mWindowService.Present()`
- 本卡执行时不得推翻的既定取舍：
  - 不允许让 `Engine.Scene` 直接引用 `TimeSnapshot` 或 `InputSnapshot`。
  - 不允许把 update 接线塞进 renderer 或 window service。
  - 不允许改变 loader failure、render failure 的既有收口行为。
  - 不允许把 update 调用藏进 `BuildRenderFrame()` 或其他隐式路径。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M15-2026-05-01 > CodeDesignNotes > App integration` 已定稿主循环顺序与 `new SceneUpdateContext(time.DeltaSeconds, time.TotalSeconds, input.AnyInputDetected)` 的翻译形状，执行时不得改成别的参数来源或顺序。

## 实施说明（ImplementationNotes）
- 先在 App 侧 runtime abstraction 与 adapter 上增加 update 入口，并保持 initialize 入口不变。
- 然后在 `SceneRuntimeAdapter` 做唯一的类型翻译：从 App 的时间/输入快照构造 `SceneUpdateContext`，再转发给 Scene。
- 再修改 `ApplicationHost.Run()` 成功路径顺序，确保 update 发生在 render 前，present 仍在 render 后。
- 最后补 App 测试，覆盖：
  - 成功路径至少调用一次 `sceneRuntime.Update(...)`。
  - update 发生在 render 前。
  - adapter 正确传递 `DeltaSeconds`、`TotalSeconds`、`AnyInputDetected`。
  - loader failure 不调用 initialize/update/render。
  - render failure 仍 request close、shutdown、dispose。

## 设计约束（DesignConstraints）
- 不允许在 App 卡里回头改 Scene 内部 update 语义。
- 不允许让 App 引入对 runtime object/component 的直接依赖。
- 不允许改变 `ApplicationHost.Run()` 既有的异常处理、关闭请求和 dispose 责任归属。
- 不允许顺手扩到 editor、play mode 或新的主循环阶段。

## 失败与降级策略（FallbackBehavior）
- 若 App 现有 runtime abstraction 名称与计划建议略有差异，可保持等价接口命名，但必须仍满足“App 侧有显式 update 入口，Scene 自有 context，不直接泄露 Platform 类型给 Scene”。
- 若 loader 失败路径已在现有测试里固定，应遵守原语义，不能为了插入 update 调整失败分支顺序。
- 若实现中发现必须让 Scene 接受 `TimeSnapshot` / `InputSnapshot` 才能接线，必须停工回退修卡，不得破边界。
- 若 update 只能放到 render 内部或 present 后才能通过测试，也必须回退，因为这违反计划固定顺序。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/ApplicationHost.cs`
  - `src/Engine.App/**`
  - `src/Engine.Scene/SceneGraphService.cs`
- 相关测试入口：
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Scene.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-app-009.md`
  - `.ai-workflow/tasks/task-scene-015.md`
  - `.ai-workflow/tasks/task-scene-016.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M15-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M15-2026-05-01 > CodeDesignNotes > App integration`
  - `PLAN-M15-2026-05-01 > Milestones > M15.3`
  - `PLAN-M15-2026-05-01 > TestPlan`
  - `PLAN-M15-2026-05-01 > PlanningDecisions`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - App runtime adapter
  - main loop orchestration
  - App tests
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Scene 内部默认旋转逻辑
- 不扩展 snapshot 诊断字段
- 不修改 Render public contract
- 不引入 editor play mode、脚本、物理、动画
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `ISceneRuntime` 新增入口的具体命名可以与现有接口风格保持一致，但其职责必须仍是显式 runtime update。
- 处理规则：
  - 若问题影响主循环顺序、边界翻译责任或失败路径语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 主循环固定顺序、adapter 翻译内容、失败路径要求和测试口径都已明确。
  - 执行者无需回看计划也能知道 update 接线只能在 App 层完成。
  - 高风险边界漂移和错误放置点都已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡同时约束主循环顺序、跨模块翻译责任和失败路径稳定性，做偏会直接破坏 App/Scene 边界。
  - 需要多入口联动验证，不能写成短卡。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Contracts`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.App -> Engine.Editor`
  - `Engine.App -> Engine.Editor.App`
  - `Engine.App` 直接依赖 runtime object/component 内部类型

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 update-before-render、adapter 翻译和失败路径
- Smoke: 成功路径至少调用一次 scene runtime update；update 明确发生在 render 前；render failure 仍按既有路径关闭与释放
- Perf: 不引入额外逐帧文件 IO、重复 scene initialize 或 render 前后双重 update

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-APP-010.md`
- ClosedAt: `2026-05-01 20:33`
- Summary:
  - Verify: 已完成 `ISceneRuntime` update 入口、`SceneRuntimeAdapter` 时间/输入翻译和 `ApplicationHost.Run()` update-before-render 接线；等待 Build/Test/Smoke/Perf 门禁。
  - GateFailure: `2026-05-01` 首轮 App.Tests 失败，adapter rotation 验证使用精确 quaternion 比较，实际仅存在浮点尾差；回退 InProgress 修正测试容差。
  - GateRetry: 已将 adapter rotation 验证改为 5 位精度容差比较；重新进入 Verify。
  - Review: Build/Test/Smoke/Boundary/Perf 通过，归档三件套已准备，等待 Human 复验。
- FilesChanged:
  - `src/Engine.App/SceneRuntimeContracts.cs`
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-010.md`
  - `.ai-workflow/archive/2026-05/TASK-APP-010.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`；App.Tests 8 条通过）
  - Smoke: `pass`（headless app run 退出码 0）
  - Boundary: `pass`（仅改 AllowedPaths 与边界/任务/归档文档；未引用 Scene runtime internal 类型）
  - Perf: `pass`（无逐帧文件 IO、重复 scene initialize 或双重 update）
- ModuleAttributionCheck: pass
