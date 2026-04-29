# 任务: TASK-EDITOR-003 M12 编辑命令编排与 selection/dirty 语义

## TaskId
`TASK-EDITOR-003`

## 目标（Goal）
让 `SceneEditorSession` 在 `Engine.Editor` 内包装 M11 的 `SceneFileDocumentEditor`，稳定支持选择对象、清空选择、新增对象、删除对象、删除当前选中对象、修改对象 id/name/resources/transform，并保证 `selection/dirty/document/scene` 更新语义与失败回滚语义一致。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M12-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M12.3`

## 执行代理（ExecutionAgent）
Exec-Editor

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.Editor

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Contracts
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M12-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EDITOR-002`

## 里程碑上下文（MilestoneContext）
- M12.3 是把 M11 的文档编辑原语提升为“GUI 可直接驱动的编辑会话语义”，重点不在多做编辑功能，而在把 selection、dirty 和失败回滚规则钉死。
- 本卡承担的是 session 内编辑命令编排、selection 语义和 dirty 语义，不承担文件保存写回。
- 上游直接影响本卡的背景包括：`SceneFileDocumentEditor` 已在 `SceneData` 完成对象级增删改；selection 只保存逻辑 object id；成功编辑要同步更新当前文档与规范化场景快照并设置 `IsDirty=true`；编辑失败不得改变 document、selection 或 dirty。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Editor session 只组织工作流和状态，不重复实现 JSON 编辑规则。
  - dirty 语义：成功编辑后置 `true`；选择对象不改变 dirty；保存成功前不自动清 dirty。
  - selection 语义：只记录 object id；删除当前选中对象后 selection 清空；修改当前选中对象 id 后 selection 跟随新 id。
  - 公开编辑操作继续走显式结果类型，而不是静默失败。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把对象编辑逻辑搬回 `Engine.Editor` 重新实现一遍。
  - 不允许为了“省一次 normalize”而让编辑成功后只改文档不改规范化场景快照。
  - 不允许在无文档状态下把失败简化为 no-op。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSession` 已定稿以下编辑操作：`SelectObject`、`ClearSelection`、`AddObject`、`RemoveObject`、`RemoveSelectedObject`、`UpdateObjectId`、`UpdateObjectName`、`UpdateObjectResources`、`UpdateObjectTransform`；本卡执行时不得改成另一套编辑面。
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorFailureKind` 已定稿失败种类方向，至少要覆盖 `NoDocumentOpen`、`ObjectNotFound`、`DuplicateObjectId`、`InvalidReference`、`InvalidTransform`、`SelectionInvalid` 或等价语义。

## 实施说明（ImplementationNotes）
- 先为 `SceneEditorSession` 加入“有文档才能编辑”的统一前置检查，把 `NoDocumentOpen` 类失败收口在 session 层。
- 再围绕 `SceneFileDocumentEditor` 编排编辑主路径：
  - 输入当前文档与编辑参数
  - 调用 `SceneData` 编辑原语
  - 成功后重建当前文档快照与规范化场景快照
  - 依规则更新 `selection` 与 `IsDirty`
- 分别补选择语义与编辑语义测试，重点覆盖：
  - 选择存在对象成功但 dirty 不变
  - 选择不存在对象失败且 selection 不变
  - 删除当前选中对象后 selection 清空
  - 修改当前选中对象 id 后 selection 跟随
  - 编辑失败不改变 document、selection、dirty

## 设计约束（DesignConstraints）
- 不允许在本卡内实现 Save/SaveAs 或路径切换。
- 不允许让外部直接修改 `Document`/`Objects` 集合来绕过 session 命令。
- 不允许把 `SceneFileDocumentEditor` 的失败语义吞掉并降格为泛化错误。
- 不允许引入 Undo/Redo、命令历史、GUI 视图模型或鼠标拾取状态。

## 失败与降级策略（FallbackBehavior）
- 无文档时所有 select/edit 操作都必须返回 `NoDocumentOpen` 或等价显式失败，不做隐式 no-op。
- 编辑失败必须保留旧文档、旧规范化场景、旧 selection 和旧 dirty。
- 选择不存在对象失败时必须保留旧 selection。
- 若发现 `SceneData` 现有编辑原语不足以表达本卡需求，必须停工回退修卡，不得在 Editor 内复制底层规则绕过。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/Editing/SceneFileDocumentEditor.cs`
  - `src/Engine.SceneData/Editing/SceneDocumentEditResult.cs`
  - `src/Engine.SceneData/Editing/SceneDocumentEditFailure.cs`
  - `src/Engine.SceneData/Descriptions/SceneDescription.cs`
  - `src/Engine.Editor/**`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `tests/Engine.Editor.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-005.md`
  - `.ai-workflow/tasks/task-editor-002.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M12-2026-04-30.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - `PLAN-M12-2026-04-30 > Milestones > M12.3`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > Dirty 语义`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > Selection 语义`
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSession`
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorFailureKind`
  - 上述引用在本卡属于“字段关系已定的参考实现约束”，尤其是编辑操作集合和 selection/dirty 更新规则。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor
- AllowedFiles:
  - SceneEditorSession 编辑命令编排
  - selection/dirty 状态语义
  - 对应 Editor session 测试
- AllowedPaths:
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现保存、另存为和磁盘写回
- 不修改 `SceneData` 的编辑规则和 failure kind 定义
- 不实现 Undo/Redo、GUI、Hierarchy、Inspector 或拾取
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Scene/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `UpdateObjectResources` 是否拆成 mesh/material 两个方法还是保留组合入口；只要不偏离计划公开接口集合即可。
- 处理规则：
  - 若问题影响公开编辑面、selection 跟随规则或 dirty 语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已写清 session 层与 SceneData 层的职责分工。
  - selection/dirty 成败语义与禁止路线都已下沉。
  - 执行者无需回看计划全文也能知道编辑失败不能污染任何当前状态。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及多条编辑路径、共享 session 状态、selection 跟随和失败原子性。
  - 若语义写不准，会直接导致 GUI 后续行为漂移且难以补救。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor -> Engine.SceneData`
  - `Engine.Editor -> Engine.Contracts`
  - `Engine.Editor -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Editor -> Engine.App`
  - `Engine.Editor -> Engine.Render`
  - `Engine.Editor -> Engine.Platform`
  - `Engine.Editor -> Engine.Asset`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln` 通过
- Test: Editor session 的 selection/dirty/edit 测试通过；`dotnet test AnsEngine.sln` 通过
- Smoke: 无文档时 select/edit/save 类操作返回显式失败；选择存在对象不改 dirty；编辑成功后 `IsDirty=true`
- Perf: 编辑成功后的规范化更新允许在显式命令路径触发；不得引入逐帧 re-normalize

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EDITOR-003.md`
- ClosedAt: `2026-04-30 01:04`
- Summary: `SceneEditorSession` 新增 selection 与对象编辑命令编排，成功编辑后同步文档/规范化场景并置 dirty，失败保持旧状态。
- FilesChanged:
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/tasks/task-editor-003.md`
  - `.ai-workflow/archive/2026-04/TASK-EDITOR-003.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test AnsEngine.sln --nologo -v minimal`；整解测试通过）
  - Smoke: `pass`（无文档 select/edit 显式失败；选择存在对象不置 dirty；编辑成功置 dirty；编辑失败不污染 document/scene/selection/dirty）
  - Boundary: `pass`（仅改 `src/Engine.Editor/**`、`tests/Engine.Editor.Tests/**` 与必需工作流/边界归档文档；未新增禁止依赖）
- ModuleAttributionCheck: pass
