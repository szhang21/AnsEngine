# 任务: TASK-EAPP-002 M13 编辑器基础布局与状态栏

## TaskId
`TASK-EAPP-002`

## 目标（Goal）
在 `Engine.Editor.App` 中落地最小编辑器布局：Toolbar、Hierarchy 占位、Inspector 占位与 Status Bar，并让状态栏稳定显示当前 scene path、dirty、selected object id 和 last error。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M13-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M13.2`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P1

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
  - `TASK-EAPP-001`

## 里程碑上下文（MilestoneContext）
- M13.2 在最小 GUI 宿主可运行后，为后续 Hierarchy、Inspector 和文件工作流提供稳定 UI 落点。
- 本卡承担基础布局和状态可见性，不承担真实对象选择、字段编辑或保存写回。
- 上游直接影响本卡的背景包括：`SceneEditorSession` 已提供 `SceneFilePath`、`IsDirty`、`SelectedObjectId`、`SelectedObject` 与显式失败结果；M13 计划要求工具型 ImGui 风格即可。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Toolbar 必须预留 `Open`、`Save`、`Save As`、`Add Object`、`Remove Selected`。
  - Hierarchy 与 Inspector 先作为区域占位，后续卡逐步接入行为。
  - Status Bar 必须来自 `SceneEditorSession` 当前状态，而不是 GUI 自己维护一份独立真相。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 dirty/selection 状态复制成脱离 session 的 GUI 主状态。
  - 不允许在本卡提前实现对象字段编辑或保存写盘。
- 计划结构约定：
  - `PLAN-M13-2026-04-30 > Milestones > M13.2` 已定稿基础布局元素和状态栏字段。

## 实施说明（ImplementationNotes）
- 在 `Engine.Editor.App` 内新增或扩展 GUI frame 绘制层，组织 Toolbar、左侧 Hierarchy 区域、中间/右侧 Inspector 区域和底部 Status Bar。
- Toolbar 按计划显示按钮，但未实现的按钮只允许调用明确占位处理并写入 last error 或 disabled 状态。
- Status Bar 从当前 `SceneEditorSession` 读取 path、dirty、selected object id；last error 由 GUI 宿主记录最近一次 session 操作失败消息。
- 补最小测试或可验证 seams，确认状态格式化不依赖窗口运行，且无文档/无选择时显示稳定空状态。

## 设计约束（DesignConstraints）
- 不允许在本卡实现 Hierarchy 真实选择列表、Inspector 字段提交或 Save/Open 文件操作。
- 不允许让 `Engine.Editor` 感知 ImGui 或 UI 状态。
- 不允许新增 `Engine.App` 引用或改变默认运行时入口。
- 新增 C# 代码遵守 Engine 编码规范和一类一文件约定。

## 失败与降级策略（FallbackBehavior）
- 如果 session 未打开文档，状态栏必须显示可诊断空状态，不得抛异常终止 GUI。
- 如果按钮功能尚未接入，必须明确禁用或给出 last error，不得静默假装成功。
- 如果布局实现需要修改 `SceneEditorSession` 状态语义，必须回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `src/Engine.Editor/Session/SceneEditorFailure.cs`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-001.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M13-2026-04-30.md`
- 计划结构引用：
  - `PLAN-M13-2026-04-30 > Milestones > M13.2`
  - `PLAN-M13-2026-04-30 > PlanningDecisions`

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - Editor GUI 布局、状态栏、last error 展示和对应测试
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现真实 Hierarchy 点击选择
- 不实现 Inspector 字段编辑
- 不实现 Open/Save/Save As/Add/Remove 行为
- 不修改 `Engine.Editor` 公开 session 语义
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 具体布局比例可由 Execution 在 `Engine.Editor.App` 内确定，但必须保持 Toolbar/Hierarchy/Inspector/Status Bar 四块可见。
- 处理规则：
  - 若布局需求扩张到 viewport、dock system 或复杂主题系统，必须回退，不得顺手实现。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确四个 UI 区域、状态来源和不实现的按钮行为。
  - 依赖 `TASK-EAPP-001` 后即可在现有 GUI 宿主中落地。
  - 失败/空状态语义和非范围已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 涉及 GUI 状态展示和 session 状态映射，容易误做成独立 GUI 状态源。
  - 验证面包括启动布局、空状态、dirty/selection 显示和未实现按钮行为。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> ImGui.NET`
  - `Engine.Editor.App -> OpenTK`
- ForbiddenDependsOn:
  - `Engine.Editor -> ImGui.NET`
  - `Engine.App -> Engine.Editor.App`

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
- Test: `dotnet test AnsEngine.sln` 通过，状态格式化/布局状态相关测试通过
- Smoke: 启动 Editor GUI 后 Toolbar、Hierarchy 占位、Inspector 占位和 Status Bar 可见；默认 scene path 与 dirty 状态可见
- Perf: 基础布局无逐帧文件 IO 或重复 session open；无明显帧时间退化说明

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Done

## 完成度（Completion）
100

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EAPP-002.md`
- ClosedAt: `2026-04-30 15:35`
- Summary: 新增真实 ImGui/OpenGL 渲染后端与基础 GUI 布局快照/绘制层，Toolbar、Hierarchy、Inspector 与 Status Bar 从 `SceneEditorSession` 状态生成并在窗口中渲染。
- FilesChanged:
  - `src/Engine.Editor.App/Engine.Editor.App.csproj`
  - `src/Engine.Editor.App/EditorAppOptions.cs`
  - `src/Engine.Editor.App/EditorAppWindow.cs`
  - `src/Engine.Editor.App/EditorGuiRenderer.cs`
  - `src/Engine.Editor.App/EditorGuiSnapshot.cs`
  - `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
  - `src/Engine.Editor.App/EditorHierarchyItemSnapshot.cs`
  - `src/Engine.Editor.App/EditorInspectorSnapshot.cs`
  - `src/Engine.Editor.App/EditorStatusBarSnapshot.cs`
  - `src/Engine.Editor.App/EditorToolbarAction.cs`
  - `src/Engine.Editor.App/ImGuiOpenGlRenderer.cs`
  - `tests/Engine.Editor.App.Tests/EditorAppOptionsTests.cs`
  - `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-eapp-002.md`
  - `.ai-workflow/archive/2026-04/TASK-EAPP-002.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；新增/更新布局状态测试通过，`Engine.Editor.App.Tests` 13 条通过）
  - Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；真实 ImGui frame 启动并关闭，退出码 0）
  - Perf: `pass`（基础布局每帧消费 session 快照，无逐帧文件 IO 或重复 open；未改变 `Engine.App` 主路径）
  - Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档）
- ModuleAttributionCheck: pass
