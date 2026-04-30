# 任务: TASK-EAPP-004 M13 Inspector 对象编辑

## TaskId
`TASK-EAPP-004`

## 目标（Goal）
在 `Engine.Editor.App` 的 Inspector 中显示并编辑选中对象的 Id、Name、Mesh、Material、Position、Rotation、Scale，所有提交都通过 `SceneEditorSession` 更新并正确反映 dirty、last error 和 UI 回滚语义。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M13-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M13.4`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.Editor.App

## 次级模块（SecondaryModules）
- Engine.Editor
- Engine.SceneData
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M13-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EAPP-003`

## 里程碑上下文（MilestoneContext）
- M13.4 是“人能通过 GUI 编辑场景”的核心能力：用户选择对象后，必须能在 Inspector 修改对象字段。
- 本卡承担字段展示、输入编辑、提交到 session 和失败回滚，不承担文件保存、Open/Save As 或对象增删。
- 上游直接影响本卡的背景包括：`SceneEditorSession` 已提供 `UpdateObjectId`、`UpdateObjectName`、`UpdateObjectResources`、`UpdateObjectTransform`；失败时不应污染文档、selection 或 dirty。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Inspector 必须显示并编辑 `Id`、`Name`、`Mesh`、`Material`、`Position`、`Rotation`、`Scale`。
  - 所有修改都必须调用 `SceneEditorSession` 的对象更新 API。
  - 成功编辑后 `IsDirty=true`，失败时显示 last error，UI 回到 session 当前有效值。
- 本卡执行时不得推翻的既定取舍：
  - 不允许 GUI 直接修改 JSON DTO 并绕过 session。
  - 不允许在 `Engine.Editor.App` 复制 `SceneData` 校验规则；校验结果以 session 失败结果为准。
  - 不允许把资源真实存在校验塞进本卡。
- 计划结构约定：
  - `PLAN-M13-2026-04-30 > Milestones > M13.4` 已定稿字段范围和 session API 路线。

## 实施说明（ImplementationNotes）
- Inspector 从 `SceneEditorSession.SelectedObject` 读取当前有效对象快照，无选择时显示空状态。
- 为文本字段维护短生命周期输入缓冲，但提交成功后必须重新从 session 读取有效值；提交失败时丢弃无效输入并显示 last error。
- Id 修改成功后必须依赖 session selection 跟随新 id 的语义，Hierarchy 高亮同步新 id。
- Transform 输入必须转换为 `SceneFileTransformDefinition` 或 session API 需要的类型；非有限数、非法引用等失败由 session 返回。
- 补测试或 seam 覆盖：name/transform 成功编辑、重复 id/空 mesh/非法 transform 失败回滚、成功编辑 dirty=true、失败不改 dirty/selection。

## 设计约束（DesignConstraints）
- 不允许直接写 `SceneFileDocument` 或 JSON 文件。
- 不允许新增 `Engine.Editor` GUI 依赖或改变 M12 session 失败语义。
- 不允许实现 Save/Open/Add/Remove，保存闭环留给后续卡。
- 新增 C# 代码遵守 Engine 编码规范和一类一文件约定。

## 失败与降级策略（FallbackBehavior）
- 无选中对象时 Inspector 必须显示空状态，所有编辑入口禁用或无操作，不得抛异常。
- session 更新失败时，last error 显示具体失败消息，UI 回到 session 当前有效值，dirty 和 selection 保持 session 结果。
- 若字段编辑需要新的 session API 才能合理实现，必须回退给 Dispatch，不得绕过 session 直接改文档。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `src/Engine.SceneData/FileModel/SceneFileObjectDefinition.cs`
  - `src/Engine.SceneData/FileModel/SceneFileTransformDefinition.cs`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-003.md`
  - `.ai-workflow/tasks/task-editor-003.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
- 计划结构引用：
  - `PLAN-M13-2026-04-30 > Milestones > M13.4`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > Dirty 语义 / Selection 语义`

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - Inspector 绘制、字段输入、session 提交、错误回滚和对应测试
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Save/Open/Save As
- 不实现 Add/Remove Object
- 不实现资源浏览器、材质编辑器或 mesh picker
- 不修改 `Engine.SceneData` schema 或 normalizer
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 输入提交时机可采用 Enter、失焦或显式 Apply，只要失败回滚和 session 单一真相成立。
- 处理规则：
  - 若提交时机选择影响验收行为，必须在测试与自检说明中写明，不得留下隐式语义。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确字段范围、session API、dirty/selection 语义、失败回滚和非范围。
  - 参考源码与测试入口足以执行。
  - 执行者无需回看里程碑全文即可避免直接改 JSON 的错误路线。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及多字段输入缓冲、session 提交、dirty/selection 同步和失败回滚，状态路线容易做偏。
  - 若绕过 session，会直接破坏 M13 关键边界。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
  - `Engine.Editor.App -> Engine.Contracts`
  - `Engine.Editor.App -> ImGui.NET`
- ForbiddenDependsOn:
  - `Engine.Editor.App -> Engine.App`
  - `Engine.Editor -> ImGui.NET`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln` 通过
- Test: `dotnet test AnsEngine.sln` 通过，Inspector 提交与失败回滚测试通过
- Smoke: 选择对象后 Inspector 可编辑 name 或 transform；成功后 Hierarchy/Inspector 立即反映新值且 `IsDirty=true`；失败显示 last error 并回到有效值
- Perf: Inspector 不做逐帧文件写入或重新加载；输入缓冲无明显帧时间退化说明

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Review

## 完成度（Completion）
95

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EAPP-004.md`
- ClosedAt: `2026-04-30 15:48`
- Summary: Inspector 新增 Id、Name、Mesh、Material、Position、Rotation、Scale 输入缓冲和显式 Apply，提交全部经 `SceneEditorSession` API，失败回滚到 session 当前有效值。
- FilesChanged:
  - `src/Engine.Editor.App/EditorInspectorSnapshot.cs`
  - `src/Engine.Editor.App/EditorInspectorInputState.cs`
  - `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
  - `src/Engine.Editor.App/EditorGuiRenderer.cs`
  - `tests/Engine.Editor.App.Tests/EditorInspectorInputStateTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-eapp-004.md`
  - `.ai-workflow/archive/2026-04/TASK-EAPP-004.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；Inspector 提交与失败回滚测试通过，`Engine.Editor.App.Tests` 18 条通过）
  - Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；Inspector 字段渲染在真实 ImGui frame，退出码 0）
  - Perf: `pass`（Inspector 不做逐帧文件写入或重新加载；输入缓冲仅保留当前选中对象编辑态）
  - Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档）
- ModuleAttributionCheck: pass
- HumanSignoff: `pending`
