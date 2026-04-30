# 任务: TASK-EAPP-006 M13 Add/Remove Object GUI 工作流

## TaskId
`TASK-EAPP-006`

## 目标（Goal）
在 `Engine.Editor.App` 中实现 Add Object 与 Remove Selected GUI 工作流：Add 创建默认 cube 对象并自动选中，Remove 删除当前选中对象，保存后 reload 仍能看到增删结果。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M13-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M13.6`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.Editor.App

## 次级模块（SecondaryModules）
- Engine.Editor
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M13-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EAPP-005`

## 里程碑上下文（MilestoneContext）
- M13.6 补齐最小场景编辑器自然预期能力：用户不仅能修改已有对象，也能新增和删除对象。
- 本卡依赖已有布局、Hierarchy selection、Inspector 和 Save 工作流；只实现 Add/Remove 的 GUI 编排和默认对象生成。
- 上游直接影响本卡的背景包括：`SceneEditorSession.AddObject`、`RemoveSelectedObject` 和保存 reload 语义已在 M12/M13 前序卡可用。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Add 创建默认 cube 对象并自动选中。
  - Remove Selected 删除当前选中对象。
  - 默认 id 使用 `object-001`、`object-002` 递增避重。
  - 默认 mesh 为 `mesh://cube`，默认 material 为 `material://default`，默认 transform 为 position `0,0,0`、identity rotation、scale `1,1,1`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许直接修改 JSON DTO 绕过 `SceneEditorSession.AddObject/RemoveSelectedObject`。
  - 不允许引入资源浏览器、Prefab 或材质编辑器。
- 计划结构约定：
  - `PLAN-M13-2026-04-30 > Milestones > M13.6` 已定稿默认对象字段和增删验收口径。

## 实施说明（ImplementationNotes）
- 在 Toolbar 的 `Add Object` 调用默认对象工厂，生成不重复 id，构造 `SceneFileObjectDefinition` 后调用 `SceneEditorSession.AddObject`。
- Add 成功后必须选择新对象；若 session 不会自动选择，GUI 可在成功后调用 `SelectObject(newId)`，但 selection 主状态仍以 session 为准。
- `Remove Selected` 调用 `SceneEditorSession.RemoveSelectedObject`；无选择时禁用或返回 last error。
- Add/Remove 成功后刷新 Hierarchy、Inspector 和 dirty 状态；失败时显示 last error 并保持 session 当前有效值。
- 补测试或 seam 覆盖 id 递增避重、默认字段、Add 后选中、Remove 后 selection 清空、save/reload 后增删结果仍存在。

## 设计约束（DesignConstraints）
- 不允许实现对象模板库、Prefab、资源选择器或批量操作。
- 不允许改变 `SceneData` 对默认字段和非法引用的校验规则。
- 不允许把 id 生成状态持久化到 scene JSON 外部；应从当前对象集合推导下一个可用 id。
- 新增 C# 代码遵守 Engine 编码规范和一类一文件约定。

## 失败与降级策略（FallbackBehavior）
- 无文档时 Add/Remove 必须显示 last error，不得创建 GUI 私有对象。
- Add 因重复 id 或非法默认对象失败时，不改变 selection 和 dirty，并显示 session 失败。
- Remove 无选择或对象不存在时，不改变文档，显示 last error。
- 若默认对象字段与 `SceneData` 校验冲突，必须回退修卡，不得放宽校验。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `src/Engine.SceneData/FileModel/SceneFileObjectDefinition.cs`
  - `src/Engine.SceneData/FileModel/SceneFileTransformDefinition.cs`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-005.md`
  - `.ai-workflow/tasks/task-editor-003.md`
  - `.ai-workflow/tasks/task-editor-004.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
- 计划结构引用：
  - `PLAN-M13-2026-04-30 > Milestones > M13.6`
  - `PLAN-M13-2026-04-30 > PlanningDecisions`

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - Add/Remove GUI 编排、默认对象工厂、id 生成和对应测试
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现对象模板库、Prefab、资源浏览器
- 不实现 Undo/Redo 或删除确认弹窗
- 不修改 `Engine.SceneData` schema
- 不修改 `Engine.Editor` API，除非回退修卡后另行批准
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 默认 id 从现有对象集合推导，若中间编号缺失，可选择填补最小空缺或递增最大编号后一位，但必须测试说明。
- 处理规则：
  - 若 id 策略影响用户可见行为，需在自检中记录，不得隐式改变验收口径。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确默认对象字段、id 生成、Add/Remove session API、成功/失败状态和保存 reload 验收。
  - 非范围防止扩张成资源系统或完整编辑器功能。
  - 执行者无需回看计划全文即可实施。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 涉及默认对象生成、id 避重、selection 更新和保存后 reload 验证。
  - 但范围仍集中在单模块 GUI 编排。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
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
- Test: `dotnet test AnsEngine.sln` 通过，默认对象/id/selection 测试通过
- Smoke: Add 后 Hierarchy 出现新对象且 Inspector 显示新对象；Remove Selected 后对象消失且 selection 清空；保存后 reload 仍能看到增删结果
- Perf: id 生成只扫描当前对象集合，无逐帧文件 IO 或明显帧时间退化说明

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EAPP-006.md`
- ClosedAt: `2026-04-30 15:58`
- Summary: Toolbar 接入 Add Object 与 Remove Selected，默认对象工厂生成 `object-XXX` cube 对象并自动选中，删除选中对象后清 selection，保存/reload 验证增删结果。
- FilesChanged:
  - `src/Engine.Editor.App/EditorDefaultObjectFactory.cs`
  - `src/Engine.Editor.App/EditorObjectWorkflowState.cs`
  - `src/Engine.Editor.App/EditorGuiRenderer.cs`
  - `tests/Engine.Editor.App.Tests/EditorObjectWorkflowStateTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-eapp-006.md`
  - `.ai-workflow/archive/2026-04/TASK-EAPP-006.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；默认对象/id/selection 测试通过，`Engine.Editor.App.Tests` 28 条通过）
  - Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；Add/Remove 控件渲染在真实 ImGui frame，退出码 0）
  - Perf: `pass`（id 生成只扫描当前对象集合，无逐帧文件 IO）
  - Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档）
- ModuleAttributionCheck: pass
- HumanSignoff: `pending`
