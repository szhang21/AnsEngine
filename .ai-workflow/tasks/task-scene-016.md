# 任务: TASK-SCENE-016 M15 Runtime update 默认旋转 smoke behavior

## TaskId
`TASK-SCENE-016`

## 目标（Goal）
在 `RuntimeScene.Update(...)` 内落地最小默认旋转 smoke behavior，让 update 后的 runtime transform 变化能同时被 snapshot 与 `BuildRenderFrame()` 观察到，同时保持 position/scale、资源引用和边界不变。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M15-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M15.2`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P1

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
  - `TASK-SCENE-015`

## 里程碑上下文（MilestoneContext）
- M15.2 的职责是提供一个最小、可见、可测试的 runtime update 行为，证明 M15 不只是有统计，而是真的会推进 runtime state。
- 本卡承担的是默认旋转 smoke behavior，不承担 App 主循环接线、snapshot 诊断字段扩展或正式动画/脚本体系设计。
- 上游直接影响本卡的背景包括：默认行为只用于验证 update 正在推进；旋转必须 deterministic，只依赖传入的 `DeltaSeconds`；`BuildRenderFrame()` 只能读取 update 后 state，不能自己推进时间。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 每次 update 找到第一个同时具备 `Transform` 与 `MeshRenderer` 的 runtime object。
  - 只修改该 object 的 `SceneTransformComponent.LocalRotation`。
  - 绕 Y 轴按固定角速度推进，建议 `MathF.PI * 0.5f` radians/sec。
  - 位置和缩放保持不变。
  - 没有可渲染对象时不报错，只更新诊断状态。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把默认旋转行为升级成正式 animation/script/component/system 抽象。
  - 不允许改 mesh/material/camera 或创建/删除对象。
  - 不允许通过 wall clock、renderer frame number 或随机量驱动旋转。
  - 不允许修改 SceneData schema，也不要在 JSON 里加旋转行为字段。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M15-2026-05-01 > CodeDesignNotes` 已定稿旋转实现必须复用 `SceneTransformComponent.SetLocalTransform(...)`，执行时不得绕过该入口直接拆写内部字段。
  - `PLAN-M15-2026-05-01 > CodeDesignNotes` 已定稿“第一个同时具备 `Transform` 与 `MeshRenderer` 的 object”这一选择规则，不得改成“所有对象都转”或“按名称筛选”。

## 实施说明（ImplementationNotes）
- 以 `TASK-SCENE-015` 已建立的 `RuntimeScene.Update(...)` 为入口，在其中添加最小旋转行为。
- 明确分两个阶段：
  - 先定位第一个可渲染 runtime object，并安全读取其当前 local transform。
  - 再仅更新 local rotation，复用 `SetLocalTransform(...)` 写回，保持 position/scale 原值不变。
- 确保零 delta 时 update count 增加但 rotation 不前进；非零 delta 时旋转增量仅由 `DeltaSeconds * MathF.PI * 0.5f` 决定。
- 补 Scene 测试，至少覆盖：
  - 有可渲染 object 时 rotation 变化。
  - position/scale 不变。
  - `BuildRenderFrame()` 输出的 rotation 与 runtime snapshot 一致。
  - 没有可渲染 object 时不抛错。
  - zero delta 不推进 rotation。

## 设计约束（DesignConstraints）
- 不允许在本卡中新建正式 update system、animation component 或脚本钩子。
- 不允许把行为散落到 `SceneGraphService`、`BuildRenderFrame()` 或 Render 侧完成。
- 不允许为了更新 rotation 暴露 runtime object/component 的可变集合。
- 不允许跨到 `Engine.App`、`Engine.Render`、`Engine.SceneData` 修改业务逻辑。

## 失败与降级策略（FallbackBehavior）
- 若当前 runtime scene 没有同时具备 `Transform` 与 `MeshRenderer` 的 object，应无报错返回并保持统计更新。
- 若 rotation 表达方式存在等价实现差异，可以在不改变对外 `LocalTransform` 语义的前提下选择现有数学工具，但必须保持 deterministic。
- 若实现中发现只能通过 `BuildRenderFrame()` 顺手推进旋转才能让测试通过，必须停工回退，因为这违反 update/render 分离。
- 若需要改 SceneData schema、render contract 或引入新 component/system 才能完成，必须回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Scene/**`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-015.md`
  - `.ai-workflow/tasks/task-scene-013.md`
  - `.ai-workflow/tasks/task-scene-014.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M15-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M15-2026-05-01 > CodeDesignNotes > 最小旋转 smoke behavior`
  - `PLAN-M15-2026-05-01 > Milestones > M15.2`
  - `PLAN-M15-2026-05-01 > TestPlan`
  - `PLAN-M15-2026-05-01 > Risks`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - runtime update behavior
  - Scene tests
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 `Engine.App` 主循环
- 不扩展 snapshot 统计字段
- 不引入脚本、物理、动画、正式 component/system
- 不修改 JSON schema 或 Render contract
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 如果当前 transform 数学工具存在多种等价旋转写法，可选择当前仓库最稳定的写法，但结果必须仍能通过 snapshot/render frame 观察同一 rotation 语义。
- 处理规则：
  - 若问题影响 deterministic 语义、更新入口归属或“只转第一个可渲染对象”的规则，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 默认旋转对象选择规则、角速度、写回入口、零 delta 语义和测试口径都已明确。
  - 执行者无需回看计划也能知道这是 smoke behavior，不是正式动画系统。
  - 禁止路线和停工条件已写清，可避免行为漂移。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡很容易被误做成 animation/system 设计，或者把 update/render 边界搅混。
  - 需要同时守住 deterministic、对象选择规则、只改 rotation 和 snapshot/render 可观察性。
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
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 rotation 变化、zero delta、无可渲染对象和 render frame 可观察性
- Smoke: update 后默认样例 scene 的首个可渲染对象 rotation 变化，position/scale 保持不变；`BuildRenderFrame()` 读到 update 后状态
- Perf: 旋转行为不引入逐帧对象重建、文件读取或 render side effect

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-016.md`
- ClosedAt: `2026-05-01 20:20`
- Summary:
  - Verify: 已在 `RuntimeScene.Update(...)` 落地最小默认旋转 smoke behavior；等待 Build/Test/Smoke/Perf 门禁。
  - Review: Build/Test/Smoke/Boundary/Perf 通过，归档三件套已准备，等待 Human 复验。
- FilesChanged:
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-016.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-016.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 41 条通过）
  - Smoke: `pass`（update 后首个可渲染对象 rotation 变化；position/scale 保持；render frame 与 snapshot 观察一致）
  - Boundary: `pass`（仅改 AllowedPaths 与边界/任务/归档文档；未新增禁止依赖）
  - Perf: `pass`（无逐帧对象重建、文件读取或 render side effect）
- ModuleAttributionCheck: pass
