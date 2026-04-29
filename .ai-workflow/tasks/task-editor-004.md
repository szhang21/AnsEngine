# 任务: TASK-EDITOR-004 M12 保存、另存为与 reload 验证

## TaskId
`TASK-EDITOR-004`

## 目标（Goal）
在 `SceneEditorSession` 落地保存当前文档、另存为新路径和保存后 reload/normalize 验证，使 `open -> edit -> save -> reload` 成为稳定闭环，并保证保存失败不丢失内存修改、保存成功后正确更新路径与 `IsDirty`。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M12-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M12.4`

## 执行代理（ExecutionAgent）
Exec-Editor

## 优先级（Priority）
P3

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
  - `TASK-EDITOR-003`

## 里程碑上下文（MilestoneContext）
- M12.4 是把 Editor session 从“可打开、可编辑”收口成“可保存、可重新验证”的完整工作流闭环。
- 本卡承担的是 save/save as/reload 验证，不再扩展编辑面，也不接 GUI。
- 上游直接影响本卡的背景包括：保存对象仍是 `SceneFileDocument`；保存成功后必须 reload/normalize 验证；保存失败不得丢失内存修改；另存为成功后当前路径切换到新路径并清 dirty。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 保存成功必须通过 `JsonSceneDescriptionLoader` 或 normalizer 重新验证保存后的文件。
  - 保存失败后内存文档保留，`IsDirty` 保持 true。
  - 另存为成功后 session 当前路径切换为新路径，`IsDirty=false`。
  - M12 不做未保存关闭确认；GUI 确认流程留给 M13。
- 本卡执行时不得推翻的既定取舍：
  - 不允许保存成功后跳过 reload/normalize 验证。
  - 不允许用“先清 dirty 后再尝试写盘”的顺序。
  - 不允许把 save 语义做成 `Engine.App` 工具逻辑或 CLI 入口逻辑。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSession` 已定稿 `Save`、`SaveAs`、`ReloadValidate` 入口名称与职责方向，本卡执行时不得改成隐式自动保存或其他宿主形状。
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorFailureKind` 已定稿至少覆盖 `NoDocumentOpen`、`SaveFailed`、`ReloadValidationFailed` 或等价显式失败种类。

## 实施说明（ImplementationNotes）
- 先在 `SceneEditorSession` 内实现“当前路径保存”主路径：检查有文档、调用 `ISceneDocumentStore.Save`、再执行 reload/normalize 验证。
- 再实现 `SaveAs`：向新路径写盘、验证成功后切换当前路径并保留当前内存文档/场景为已保存状态。
- 明确 save 状态机顺序：
  - 写盘前保留当前内存状态
  - 写盘成功后执行 reload/normalize 验证
  - 仅当全部成功时才把 `IsDirty` 置为 false，并更新当前路径
- 补测试覆盖：保存成功、保存失败不丢内存修改、另存为后路径更新、保存后的文件可被现有 loader 加载为 `SceneDescription`。

## 设计约束（DesignConstraints）
- 不允许在本卡内修改 M11 的文档 DTO、JSON schema 或 normalizer 规则。
- 不允许把保存成功条件放宽成“只写到磁盘即可”。
- 不允许用异常吞没方式隐藏写盘失败或 reload 失败。
- 不允许顺手接入 `Engine.App` 启动路径、CLI 工具或 GUI 文件对话框。

## 失败与降级策略（FallbackBehavior）
- 无文档时 `Save/SaveAs/ReloadValidate` 必须返回 `NoDocumentOpen` 或等价显式失败。
- 写盘失败时必须保留当前内存文档、当前场景快照、当前路径和 `IsDirty=true`。
- 写盘成功但 reload/normalize 失败时，必须返回显式失败并保留内存修改；不得把 session 错误地标成已保存。
- 若实现中发现需要改动 `SceneData` 公开保存/加载规则才能继续，必须停工回退修卡，不得在 Editor 内复制或分叉读写逻辑。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/Abstractions/ISceneDocumentStore.cs`
  - `src/Engine.SceneData/DocumentStore/SceneDocumentSaveResult.cs`
  - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
  - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
  - `src/Engine.Editor/**`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
  - `tests/Engine.Editor.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-003.md`
  - `.ai-workflow/tasks/task-sdata-004.md`
  - `.ai-workflow/tasks/task-editor-003.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M12-2026-04-30.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - `PLAN-M12-2026-04-30 > Milestones > M12.4`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > 保存语义`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > 关闭语义`
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSession`
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorFailureKind`
  - 上述引用在本卡属于“字段关系已定的参考实现约束”，尤其是 `Save/SaveAs/ReloadValidate` 入口与保存失败保留内存修改的规则。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor
- AllowedFiles:
  - SceneEditorSession save/save as/reload 路径
  - 对应 Editor session 测试
- AllowedPaths:
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不新增 GUI、文件对话框、关闭确认或 Undo/Redo
- 不修改 `Engine.App` 启动入口
- 不修改 `Engine.SceneData` schema、loader 或 document store 契约
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Scene/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `ReloadValidate` 是完全复用 loader 还是“document store + normalizer”组合，只要保持语义一致和无逻辑分叉即可。
- 处理规则：
  - 若问题影响保存成功判定、dirty 清理时机或路径切换时机，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已把保存状态机、失败保留语义和 reload 验证要求写清。
  - 依赖入口、禁止路线和测试重点都已明确。
  - 执行者无需回看计划全文也能知道“写盘成功但 reload 失败”不能算保存成功。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及 save/save as/reload 三条关键路径、dirty/path 切换和失败原子性。
  - 若状态机顺序写错，会直接导致未保存修改丢失或 session 与磁盘失真。
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
- Test: 保存、另存为、reload 验证与失败回滚测试通过；`dotnet test AnsEngine.sln` 通过
- Smoke: `open -> edit -> save -> reload` 成功；保存成功后 `IsDirty=false`；保存失败后 `IsDirty=true`
- Perf: 保存/reload 仅发生在显式 session 命令阶段；无逐帧 IO、无热重载轮询

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EDITOR-004.md`
- ClosedAt: `2026-04-30 01:07`
- Summary: `SceneEditorSession` 新增保存与另存为闭环，写盘后 reload/normalize 验证通过才清 dirty 或切换路径，失败保留内存修改。
- FilesChanged:
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/tasks/task-editor-004.md`
  - `.ai-workflow/archive/2026-04/TASK-EDITOR-004.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test AnsEngine.sln --nologo -v minimal`；Editor.Tests 26 条通过，整解测试通过）
  - Smoke: `pass`（`open -> edit -> save -> reload` 成功；保存成功 `IsDirty=false`；保存/重载失败保留 dirty 与内存修改）
  - Boundary: `pass`（仅改 `src/Engine.Editor/**`、`tests/Engine.Editor.Tests/**` 与必需工作流/边界归档文档；未新增禁止依赖）
- ModuleAttributionCheck: pass
