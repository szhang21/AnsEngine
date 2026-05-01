# 任务: TASK-SCENE-017 M15 Runtime snapshot 诊断与边界测试

## TaskId
`TASK-SCENE-017`

## 目标（Goal）
扩展 `RuntimeSceneSnapshot` 诊断字段并补齐 Scene/App/Render 边界测试，证明 runtime update 状态可被只读观察，同时没有把 update context 或 runtime scene 类型泄露到 Render 或其他模块。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M15-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M15.4`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.App
- Engine.Render
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M15-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-APP-010`

## 里程碑上下文（MilestoneContext）
- M15.4 负责把 runtime update 的可观察性和边界证据补齐，避免 M15 只靠肉眼行为或单一测试旁证通过。
- 本卡承担的是 snapshot 诊断字段、只读观察语义和边界测试，不承担新的 update 行为、App 主循环接线或 QA 归档。
- 上游直接影响本卡的背景包括：`RuntimeSceneSnapshot` 需要新增 `UpdateFrameCount` 与 `AccumulatedUpdateSeconds`；`SceneRuntimeObjectSnapshot` 暂不新增字段，通过现有 `LocalTransform` 观察 rotation；`Engine.Render` 不得引用 `SceneUpdateContext` 或 runtime scene 类型。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `RuntimeSceneSnapshot` 增加 `UpdateFrameCount` 与 `AccumulatedUpdateSeconds`。
  - `SceneRuntimeObjectSnapshot` 暂不新增字段。
  - snapshot 不暴露 mutable runtime object/component。
  - `Engine.Render` 不引用 update context 或 runtime scene 类型。
- 本卡执行时不得推翻的既定取舍：
  - 不允许为了诊断方便公开 runtime scene 或 component 可变引用。
  - 不允许在本卡内修改 `SceneRenderFrame` contract 来承载 update 统计。
  - 不允许让 Render 或 App 直接消费 `SceneUpdateContext`。
  - 不允许把边界测试弱化成仅搜索编译通过。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M15-2026-05-01 > CodeDesignNotes > Snapshot 扩展` 已定稿 `RuntimeSceneSnapshot` 的新增字段仅为 `UpdateFrameCount` 和 `AccumulatedUpdateSeconds`，本卡不得擅自扩成新的可变诊断模型。
  - `PLAN-M15-2026-05-01 > CodeDesignNotes` 已定稿 `SceneRenderFrame.FrameNumber` 与 runtime update frame count 语义分离，本卡不得把两者混并。

## 实施说明（ImplementationNotes）
- 先在 Scene 侧扩展 snapshot 值对象与生成逻辑，只加入 update 诊断所需最小字段。
- 再补 Scene 测试，验证 snapshot 的 update 统计与 runtime state 一致，且 update 后 rotation 仍通过现有 `LocalTransform` 观察。
- 然后补边界测试或回归测试，覆盖：
  - `Engine.Scene` 不引用 `Engine.Platform`、`Engine.Render`、`Engine.App`。
  - `Engine.Render` 不引用 `SceneUpdateContext` 或 runtime scene 类型。
  - App 只通过既定 runtime abstraction 驱动 update，不直连内部 runtime model。
- 最后复核 `BuildRenderFrame()` 与 update frame count 语义分离，没有因为诊断字段而引入隐式 update。

## 设计约束（DesignConstraints）
- 不允许把 snapshot 扩展成新的跨模块可写模型。
- 不允许新增 render contract 字段或要求 Render 感知 Scene update 统计。
- 不允许通过修改 App/Render 业务逻辑来“证明”边界测试通过。
- 不允许顺手扩到脚本、物理、动画、play mode 或 SceneData schema。

## 失败与降级策略（FallbackBehavior）
- 若测试需要更多只读字段辅助诊断，可在 Scene 内 snapshot 上局部补充只读字段，但必须仍与计划约束兼容，不得泄露 mutable runtime state。
- 若边界验证方式在搜索测试和单元测试之间存在差异，可采用仓库现有最稳定的验证方式，但必须能产出明确证据。
- 若实现中发现只有修改 `SceneRenderFrame` contract 或让 Render 感知 update context 才能达成验收，必须停工回退。
- 若 update 统计与 render frame number 无法清晰分离，也必须回退修卡，不得模糊处理。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Scene/**`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `src/Engine.Render/**`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Render.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-014.md`
  - `.ai-workflow/tasks/task-scene-015.md`
  - `.ai-workflow/tasks/task-scene-016.md`
  - `.ai-workflow/tasks/task-app-010.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M15-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M15-2026-05-01 > CodeDesignNotes > Snapshot 扩展`
  - `PLAN-M15-2026-05-01 > Milestones > M15.4`
  - `PLAN-M15-2026-05-01 > TestPlan`
  - `PLAN-M15-2026-05-01 > Risks`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - runtime snapshot diagnostics
  - Scene tests
  - App/Render boundary regression tests
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Render.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不修改 `src/Engine.App/**` 业务实现
- 不修改 `src/Engine.Render/**` 业务实现
- 不新增正式 update phases、脚本、物理、动画
- 不修改 `Engine.Contracts.SceneRenderFrame`
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 边界测试可采用现有仓库最稳定的实现方式，但必须明确覆盖 Render 不引用 `SceneUpdateContext` 或 runtime scene 类型。
- 处理规则：
  - 若问题影响 snapshot 只读语义、update/render 计数分离或边界验证结论，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 诊断字段范围、边界测试目标、禁止路线和可接受验证方式都已明确。
  - 执行者不需要再回看计划才能知道本卡不能通过改 Render contract 过关。
  - 本卡和前后卡的职责边界清晰，不会把行为实现和 QA 收口混进来。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 同时涉及只读诊断模型、update/render 语义分离和跨模块边界证明，若卡写短很容易误放实现位置。
  - 验证需要 Scene/App/Render 多入口配合。
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
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Scene/App/Render 相关测试无退化
- Smoke: `RuntimeSceneSnapshot` 可只读观察 `UpdateFrameCount` 与 `AccumulatedUpdateSeconds`；update 后 rotation 仍可由 snapshot 与 render frame 观察；Render 不感知 update context/runtime scene 类型
- Perf: snapshot 诊断不引入逐帧 scene rebuild、文件 IO 或 render side effect

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-017.md`
- ClosedAt: `2026-05-01 20:36`
- Summary:
  - Verify: 已扩展 `RuntimeSceneSnapshot` update 诊断字段，并补齐 Scene/App/Render 边界测试；等待 Build/Test/Smoke/Perf 门禁。
  - Review: Build/Test/Smoke/Boundary/Perf 通过，归档三件套已准备，等待 Human 复验。
- FilesChanged:
  - `src/Engine.Scene/Runtime/RuntimeSceneSnapshot.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `tests/Engine.Render.Tests/RenderBoundaryTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-017.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-017.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过；`Engine.Render.Tests` 18 条通过）
  - Smoke: `pass`（snapshot 可只读观察 update 统计；rotation 可由 snapshot 与 render frame 观察；Render 不感知 update context/runtime scene 类型）
  - Boundary: `pass`（仅改 AllowedPaths 与边界/任务/归档文档；未改 App/Render 业务实现）
  - Perf: `pass`（snapshot 诊断不引入 scene rebuild、文件 IO 或 render side effect）
- ModuleAttributionCheck: pass
