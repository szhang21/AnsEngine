# 任务: TASK-QA-022 M21 Editor authoring MVP gate review and archive

## TaskId
`TASK-QA-022`

## 目标（Goal）
验证 M21 的 Editor authoring 主链路已经形成 `edit Script/RigidBody/BoxCollider -> save -> App runtime collision smoke` 闭环，并确认 Scene View 非空预览、边界更新与非目标约束同时成立，为归档做好证据准备。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M21-2026-05-05`

## 里程碑引用（兼容别名：MilestoneRef）
`M21.5`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.Editor.App

## 次级模块（SecondaryModules）
- Engine.Editor
- Engine.SceneData
- Engine.App
- Engine.Physics

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M21-G5`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EAPP-011`

## 里程碑上下文（MilestoneContext）
- M21.5 是这轮里程碑的收口卡，负责证明 editor authoring 不只是 UI 看起来更像工具，而是真能产出进入 runtime physics 主路径的 scene。
- 本卡承担的是 Build/Test/Smoke/边界复验、runtime collision smoke、archive readiness 与 must-fix 分流，不承担功能实现。
- 直接影响本卡的上游背景包括：M21.2 提供 headless component APIs，M21.3 提供 Inspector authoring，M21.4 提供 SceneView preview 与 `Engine.Editor.App` 边界放宽。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 成功标准固定为：
    - 在 Editor 中创建或编辑一个 `MoveOnInput + Dynamic RigidBody + BoxCollider` mover
    - 在 Editor 中创建或编辑一个 `Static RigidBody + BoxCollider` wall
    - 保存后运行 App，mover 被 wall 阻挡
    - Editor 中央区域能看到当前 scene 的非空预览
  - M21 不做：
    - Gizmo
    - 鼠标 picking
    - Undo/Redo
    - Project Browser
    - Play Mode
    - trigger gameplay
    - dynamic gravity / solver
  - `Engine.Editor.App` 边界需要显式更新为允许依赖 `Engine.Scene` / `Engine.Render` / `Engine.Asset`
  - `Engine.Editor` 必须继续保持 headless，不依赖 GUI、Render、Asset、Scene 或 App
- 本卡执行时不得推翻的既定取舍：
  - 不允许把“非空预览”误判为仍显示占位文本或空白区域。
  - 不允许用仅 Editor 单测通过替代 runtime collision smoke。
  - 不允许接受顺手滑入 Play Mode、script execution preview、dynamic physics 的实现。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M21-2026-05-05 > GoalSummary` 与 `Milestones > M21.5` 已定稿最终闭环验收目标。
  - `PLAN-M21-2026-05-05 > TestPlan` 已定稿本轮必须覆盖的 Editor、Editor.App、Render/smoke 验证面。

## 实施说明（ImplementationNotes）
- 先收集执行卡自检证据，确认 `TASK-EAPP-009`、`TASK-EDITOR-006`、`TASK-EAPP-010`、`TASK-EAPP-011` 均已到 `Review` 并具备 build/test/smoke/perf 证据。
- 再执行 M21 全链路复验：
  - Editor authoring -> save
  - App runtime smoke
  - SceneView nonblank preview
  - boundary direction and no-goal checks
- 若发现 must-fix，必须创建 follow-up 或回退，不得让 QA 卡带病进入 `Review`。
- 最后准备归档三件套所需摘要、文件清单、验证证据与剩余风险。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡内实现功能补丁；只允许验证、复现、质疑、分流和归档准备。
- 不允许忽略 `Engine.Editor` headless 边界检查。
- 不允许把 M20 runtime physics 旧通过结果直接当成 M21 runtime collision smoke 替代；必须验证“editor 产出的 scene”进入真实 runtime。
- 不允许接受超出 M21 范围的功能漂移作为“顺手增强”。

## 失败与降级策略（FallbackBehavior）
- 若 SceneView 预览可见但 runtime smoke 失败，必须判定 M21 未完成并回退相关执行卡，不得以“预览已完成”单独关里程碑。
- 若 build/test 全绿但 `Engine.Editor` 引入了 GUI/Render/Scene 依赖，必须判定 DesignQuality 失败。
- 若发现 must-fix 数量大于 `0`，必须在 QA 结论中写明 follow-up / reopen 路径，不能直接推进 `Review`。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`
  - `src/Engine.App/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.Asset/**`
- 相关测试入口：
  - `tests/Engine.Editor.Tests/**`
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Render.Tests/**`
  - `tests/Engine.Platform.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-009.md`
  - `.ai-workflow/tasks/task-editor-006.md`
  - `.ai-workflow/tasks/task-eapp-010.md`
  - `.ai-workflow/tasks/task-eapp-011.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M21-2026-05-05.md`
- 计划结构引用：
  - `PLAN-M21-2026-05-05 > GoalSummary`
  - `PLAN-M21-2026-05-05 > Scope`
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Dependency Direction`
  - `PLAN-M21-2026-05-05 > TestPlan`
  - `PLAN-M21-2026-05-05 > Milestones > M21.5`
  - 上述目标/测试/边界引用属于“验收约束”，不能只凭局部单测替代闭环验证。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
  - Engine.Editor
  - Engine.App
  - Engine.Scene
  - Engine.Render
  - Engine.Asset
- AllowedFiles:
  - QA notes / task archive prep / boundary verification evidence
  - 仅当发现 MustFix 需转卡时，允许更新 workflow 元数据
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`
  - `src/Engine.Asset/**`
  - `tests/Engine.Asset.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不执行关单
- 不替 Human 做最终签收
- OutOfScopePaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若 preview smoke 难以纯自动化证明，可接受“自动化 + 明确手工 smoke 证据”组合，但不能缺失可复述的验证口径。
- 处理规则：
  - 若问题影响是否满足成功标准、must-fix 分流或边界方向判断，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 成功标准、非目标、复验面和 must-fix 分流规则都已下沉到卡面。
  - 执行者无需回看里程碑全文即可知道 QA 要验证的是 editor authoring 闭环，而不是单一 UI 改动。
  - QA 边界和停工条件明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 Editor/App/Scene/Render 多链路收口卡，既要查边界，又要看可见预览和 runtime physics 闭环。
  - 如果写得过短，极易把“UI 看起来完成”误判成“里程碑闭环完成”。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
  - `Engine.Editor.App -> Engine.Contracts`
  - `Engine.Editor.App -> Engine.Platform`
  - `Engine.Editor.App -> Engine.Scene`
  - `Engine.Editor.App -> Engine.Render`
  - `Engine.Editor.App -> Engine.Asset`
- ForbiddenDependsOn:
  - `Engine.Editor -> Engine.Render`
  - `Engine.Editor -> Engine.Asset`
  - `Engine.Editor -> Engine.Scene`
  - `Engine.App -> Engine.Editor`
  - `Engine.App -> Engine.Editor.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `true`
- Status: `approved`
- RequestReason: `M21 QA 需要验证 Engine.Editor.App 的 preview composition root 依赖放宽是否已经正确落地到边界合同。`
- ImpactModules:
  - `Engine.Editor.App`
  - `Engine.Editor`
  - `Engine.Scene`
  - `Engine.Render`
  - `Engine.Asset`
- HumanApprovalRef: `Human approved via “拆卡m21” on 2026-05-05`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/boundaries/engine-editor.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过；必要时补跑 `Engine.Render.Tests` 与 `Engine.Platform.Tests`
- Smoke: Editor open sample scene -> edit Script/RigidBody/BoxCollider -> save；随后 headless App runtime smoke 证明 `MoveOnInput` mover 被 static wall 阻挡；SceneView 为非空预览
- Perf:
  - 未引入逐帧 scene reload / asset reload 风暴
  - 未把 Editor preview 变成完整 runtime app duplication
- CodeQuality:
  - NoNewHighRisk: `true`
  - MustFixCount: `0`
  - MustFixDisposition: `none`
- DesignQuality:
  - DQ-1 职责单一（SRP）: `pass`
  - DQ-2 依赖反转（DIP）: `pass`
  - DQ-3 扩展点保留（OCP-oriented）: `pass`
  - DQ-4 开闭性评估（可选）: `pass`

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
InProgress

## 完成度（Completion）
`70`

## 缺陷回流字段（Defect Triage）
- FailureType: `AcceptanceDispute`
- DetectedAt: `2026-05-06`
- ReopenReason: `DependsOn TASK-EAPP-011 was reopened during human acceptance review. The prior QA conclusion that M21 preview was sufficient and archive-ready is no longer valid until Scene View renders projected triangles from real submission mesh batches.`
- OriginTaskId:
- HumanSignoff: `fail`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-022.md`
- ClosedAt:
- Summary:
  - QA conclusion reopened because `TASK-EAPP-011` no longer satisfies the expected M21 Scene View preview behavior during human acceptance review.
  - M21 cannot be archived until preview rendering is corrected and the runtime/editor smoke is revalidated.
- FilesChanged:
- ValidationEvidence:
- CodeQuality:
  - NoNewHighRisk: `false`
  - MustFixCount: `1`
  - MustFixDisposition: `follow-up-created`
- DesignQuality:
  - DQ-1 SRP: `pass`
  - DQ-2 DIP: `pass`
  - DQ-3 OCP-oriented extension: `pass`
  - DQ-4 Open/closed evaluation: `warn`
- ModuleAttributionCheck: fail
