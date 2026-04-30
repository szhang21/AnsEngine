# 任务: TASK-EAPP-003 M13 Hierarchy 面板与选择联动

## TaskId
`TASK-EAPP-003`

## 目标（Goal）
在 `Engine.Editor.App` 的 Hierarchy 面板中显示 `SceneEditorSession.Objects`，点击对象调用 `SelectObject(objectId)`，并把 selection 同步到高亮、状态栏和 Inspector 空/选中态。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M13-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M13.3`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.Editor.App

## 次级模块（SecondaryModules）
- Engine.Editor

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M13-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EAPP-002`

## 里程碑上下文（MilestoneContext）
- M13.3 是 Inspector 编辑前置：GUI 必须先能列出对象并把用户点击转换为 session selection。
- 本卡只负责 Hierarchy 展示与选择联动，不负责对象字段编辑、增删对象或保存。
- 上游直接影响本卡的背景包括：`SceneEditorSession.Objects`、`SelectedObjectId`、`SelectObject` 已在 M12 定稿，selection 不应改变 dirty。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Hierarchy 必须读取 `SceneEditorSession.Objects`。
  - 每个 object 显示 object name 和 object id。
  - 点击对象必须调用 `SceneEditorSession.SelectObject(objectId)`。
  - 当前选中对象必须高亮，selection 变化同步到状态栏和 Inspector。
- 本卡执行时不得推翻的既定取舍：
  - 不允许 GUI 自己维护与 session 不一致的 selected object。
  - 不允许 selection 改变 `IsDirty`。
- 计划结构约定：
  - `PLAN-M13-2026-04-30 > Milestones > M13.3` 已定稿 Hierarchy 展示字段和 selection 交互。

## 实施说明（ImplementationNotes）
- 在现有 Hierarchy 区域渲染 session objects 列表，展示 `Name` 与 `ObjectId`，名称缺省时仍显示 id。
- 点击列表项时调用 `SceneEditorSession.SelectObject(objectId)`，成功后依赖 session 的 `SelectedObjectId` 作为唯一选中来源。
- 选中项高亮必须来自 session 当前值；失败时写入 last error 并保持原 selection。
- Inspector 区域在本卡只需要感知“有选中对象/无选中对象”的状态，为 `TASK-EAPP-004` 留出字段编辑入口。
- 补测试或 seam 验证：选择成功不改变 dirty、选择不存在对象不会污染 selection、状态栏反映最新 selected id。

## 设计约束（DesignConstraints）
- 不允许直接改写 `SelectedObjectId` 的 GUI 镜像作为主状态。
- 不允许在本卡实现 Inspector 字段提交、Add/Remove 或保存。
- 不允许将 scene object DTO 直接写回 JSON。
- 新增 C# 代码遵守 Engine 编码规范和一类一文件约定。

## 失败与降级策略（FallbackBehavior）
- 如果当前无文档或对象列表为空，Hierarchy 显示空状态且不抛异常。
- 如果 `SelectObject` 返回失败，last error 必须显示失败消息，dirty 和 selection 保持 session 当前有效值。
- 如果发现 session selection API 不足以表达 GUI 需求，必须回退修卡，不得绕过 session。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-002.md`
  - `.ai-workflow/tasks/task-editor-003.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
- 计划结构引用：
  - `PLAN-M13-2026-04-30 > Milestones > M13.3`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > Selection 语义`

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - Hierarchy 绘制、selection 调用、状态同步和对应测试
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Inspector 对象字段编辑
- 不实现 Add/Remove Object
- 不实现文件保存
- 不修改 `SceneEditorSession` selection 语义
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 列表排序默认沿用 session objects 顺序；若未来需要排序或过滤，另行建卡。
- 处理规则：
  - 若出现需要搜索、分组、拖拽层级等完整 Hierarchy 功能，必须回退，不得顺手扩张。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确对象来源、显示字段、点击行为、dirty 不变语义和失败处理。
  - 参考入口和上游 selection 语义已定位。
  - 不需要回看计划全文即可实施。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 涉及 GUI selection 与 session 状态单一真相，存在状态漂移风险。
  - 验证需要覆盖对象列表、点击成功、失败保持状态和 dirty 不变。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
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
- Test: `dotnet test AnsEngine.sln` 通过，selection/dirty 相关测试通过
- Smoke: 默认场景对象显示在 Hierarchy；点击对象后高亮、状态栏和 Inspector 选中态同步；dirty 不变
- Perf: Hierarchy 每帧只消费 session 快照，不做逐帧文件加载或昂贵重建说明

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EAPP-003.md`
- ClosedAt: `2026-04-30 15:40`
- Summary: Hierarchy 列表项点击接入 `EditorAppController.SelectObject`，选中状态、高亮、Inspector 选中态与 Status Bar selected id 均从 `SceneEditorSession` 生成。
- FilesChanged:
  - `src/Engine.Editor.App/EditorGuiRenderer.cs`
  - `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-eapp-003.md`
  - `.ai-workflow/archive/2026-04/TASK-EAPP-003.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning 与本机 Windows Kits `LIB` warning）
  - Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；selection/dirty 测试通过，`Engine.Editor.App.Tests` 15 条通过）
  - Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；默认场景对象渲染在 Hierarchy，真实 ImGui frame 退出码 0）
  - Perf: `pass`（Hierarchy 每帧消费 session 快照，不做逐帧文件加载或昂贵重建）
  - Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档）
- ModuleAttributionCheck: pass
- HumanSignoff: `pending`
