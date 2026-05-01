# 任务: TASK-SCENE-015 M15 Runtime update context 与统计地基

## TaskId
`TASK-SCENE-015`

## 目标（Goal）
在 `Engine.Scene` 建立 Scene 自有 `SceneUpdateContext`、`SceneGraphService.UpdateRuntime(...)` 和 `RuntimeScene` update 统计地基，使 runtime update 可以被安全调用、可诊断且不破坏现有边界。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M15-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M15.1`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Core
- Engine.Contracts
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M15-G1`
- CanRunParallel: `false`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M15.1 是整个 runtime update pipeline 的地基，没有 Scene 自有 update context 和 runtime 统计，后续默认旋转行为、App 主循环接线、snapshot 诊断都没有稳定落点。
- 本卡承担的是 Scene 内 update 生命周期入口、统计字段和参数语义，不承担可见行为验证、App 主循环接线或 QA 收口。
- 直接影响本卡实现的上游背景包括：`Update` 是 runtime/game loop 核心机制，不建模为 component 或 system；`Engine.Scene` 不能依赖 `Engine.Platform` 的时间/输入类型；`BuildRenderFrame()` 只能读取当前 runtime state，不得推进 update。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `SceneUpdateContext` 位置固定在 `Engine.Scene`。
  - `SceneGraphService` 新增 `public void UpdateRuntime(SceneUpdateContext context)`，只承担参数转发与 runtime ownership 边界。
  - `RuntimeScene` 新增 `UpdateFrameCount`、`AccumulatedUpdateSeconds`、`Update(SceneUpdateContext context)`。
  - `Clear()` / `LoadFromDescription(...)` 必须重置 update 统计。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 update context 放进 `Engine.Contracts`。
  - 不允许让 `Engine.Scene` 直接依赖 `TimeSnapshot`、`InputSnapshot` 或 `Engine.Platform`。
  - 不允许在本卡内把 `Update` 抽象成正式 component/system。
  - 不允许让 `BuildRenderFrame()` 隐式推进 update。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M15-2026-05-01 > CodeDesignNotes` 已定稿 `SceneUpdateContext` 形状为 `public readonly record struct SceneUpdateContext(double DeltaSeconds, double TotalSeconds, bool AnyInputDetected)`，执行时不得改写字段名、字段顺序或迁移模块。
  - `PLAN-M15-2026-05-01 > CodeDesignNotes` 已定稿 `DeltaSeconds < 0` 抛 `ArgumentOutOfRangeException`、`DeltaSeconds == 0` 合法且会增加 update count 的语义，本卡不得自行改成静默截断或忽略本次 update。

## 实施说明（ImplementationNotes）
- 先在 `Engine.Scene` 增加 Scene 自有 update context 类型，并把参数校验封装在类型或其最近邻入口，保证负 delta 有可诊断异常。
- 再给 `RuntimeScene` 增加 update 统计状态和 `Update(...)` 入口，但本卡只建立“可被调用且正确记账”的基础语义，不抢做旋转烟雾行为。
- 然后在 `SceneGraphService` 增加 `UpdateRuntime(...)` 作为 facade 转发入口，保持 Scene runtime ownership 不外泄。
- 最后补 Scene 测试，覆盖：
  - 第一次 update 后 `UpdateFrameCount` 递增。
  - 多次 update 后 `AccumulatedUpdateSeconds` 累加。
  - `DeltaSeconds == 0` 合法且统计递增。
  - `DeltaSeconds < 0` 抛 `ArgumentOutOfRangeException`。
  - 空 scene update 不崩溃。
  - `LoadSceneDescription` 或清空后统计归零。

## 设计约束（DesignConstraints）
- 不允许在本卡中引入 `Engine.Scene -> Engine.Platform`、`Engine.Scene -> Engine.App`、`Engine.Scene -> Engine.Render` 依赖。
- 不允许让 `SceneGraphService.UpdateRuntime(...)` 直接写 transform 细节或塞入可见行为。
- 不允许为了省事把 update 统计挂到 render frame number 上。
- 不允许顺手扩展 SceneData schema、Render contract 或 editor/runtime query 面。

## 失败与降级策略（FallbackBehavior）
- 若参数校验入口选择上存在实现差异，可以放在 `SceneUpdateContext` 或 `RuntimeScene.Update(...)` 最近邻，但必须保持外部可观察到相同异常语义。
- 若空 scene 没有对象可更新，应允许无报错返回，同时正常更新统计。
- 若实现过程中发现需要 `Engine.Platform` 类型才能表达时间/输入，必须停工回退修卡，不得跨边界借型。
- 若统计只能通过修改 render contract 或 render frame number 复用实现，必须显式回退，不得继续。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Scene/**`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-010.md`
  - `.ai-workflow/tasks/task-scene-012.md`
  - `.ai-workflow/tasks/task-scene-014.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M15-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M15-2026-05-01 > CodeDesignNotes`
  - `PLAN-M15-2026-05-01 > Milestones > M15.1`
  - `PLAN-M15-2026-05-01 > TestPlan`
  - `PLAN-M15-2026-05-01 > PlanningDecisions`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - Scene update context
  - runtime update statistics
  - Scene tests
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现默认旋转 smoke behavior
- 不修改 `Engine.App` 主循环
- 不扩展 snapshot 诊断字段
- 不修改 `Engine.Contracts.SceneRenderFrame`
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `SceneUpdateContext` 的参数校验是放在 record struct 构造路径还是 update 入口邻近层；两种都可，但对外异常语义必须一致。
- 处理规则：
  - 若问题影响异常语义、模块边界或统计重置语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - update context 形状、统计语义、边界约束和测试口径都已下沉到卡内。
  - 执行者无需回看里程碑全文也能知道哪些是本卡地基、哪些必须留给后续卡。
  - 失败时可继续、必须报错和必须停工的分流已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 runtime update pipeline 的基础卡，若上下文或统计语义做偏，后续 Scene/App/QA 全会漂移。
  - 同时涉及边界、异常语义、生命周期重置和验证口径，信息密度必须高。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Platform`
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.App`
  - `Engine.Scene -> Engine.Editor`
  - `Engine.Scene -> Engine.Editor.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 update 统计、zero delta、negative delta、空 scene 和 reset 语义
- Smoke: `SceneGraphService.UpdateRuntime(...)` 可被调用；runtime update count/seconds 能从 scene 侧查询或后续 snapshot 路径稳定观察；空 scene 不崩溃
- Perf: 不引入逐帧文件 IO、scene rebuild 或对 render frame number 的隐式依赖

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Review

## 完成度（Completion）
`95`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-015.md`
- ClosedAt: `2026-05-01 20:16`
- Summary:
  - Verify: 已新增 `SceneUpdateContext`、`SceneGraphService.UpdateRuntime(...)` 与 `RuntimeScene.Update(...)` 统计地基；等待 Build/Test/Smoke/Perf 门禁。
  - GateFailure: `2026-05-01` 首轮 Build/Test 失败，`SceneUpdateContext` primary record struct constructor 与自定义校验构造同签名冲突（CS0111）；回退 InProgress 修复。
  - GateRetry: 已改为显式属性的 `readonly record struct`，保留计划要求的构造参数顺序与负 delta 异常语义；重新进入 Verify。
  - Review: Build/Test/Smoke/Boundary/Perf 通过，归档三件套已准备，等待 Human 复验。
- FilesChanged:
  - `src/Engine.Scene/SceneUpdateContext.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-015.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-015.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 36 条通过）
  - Smoke: `pass`（`SceneGraphService.UpdateRuntime(...)` 可调用；空 scene update 不崩溃；统计可观察）
  - Boundary: `pass`（仅改 AllowedPaths 与边界/任务/归档文档；未新增禁止依赖）
  - Perf: `pass`（无逐帧文件 IO、scene rebuild 或 render frame number 隐式依赖）
- ModuleAttributionCheck: pass
