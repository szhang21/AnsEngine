# 任务: TASK-EAPP-005 M13 Open/Save/Save As 工作流

## TaskId
`TASK-EAPP-005`

## 目标（Goal）
在 `Engine.Editor.App` 中接入 Open、Save、Save As GUI 工作流，支持 `ANS_ENGINE_EDITOR_SCENE_PATH` 覆盖默认 scene，并确保保存真实写回源码 sample scene、成功清 dirty、失败保留 dirty 且不污染 session。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M13-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M13.5`

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
  - `TASK-EAPP-004`

## 里程碑上下文（MilestoneContext）
- M13.5 把 GUI 对象编辑收口成文件闭环：用户能打开、保存和另存为 scene 文件。
- 本卡承担文件路径解析、Open/Save/Save As 操作编排和状态栏路径/dirty 更新，不承担对象增删或复杂系统文件对话框。
- 上游直接影响本卡的背景包括：M12 `SceneEditorSession.Save`、`SaveAs`、`Open` 已定义失败保留语义；M13 计划要求默认编辑源码目录 sample scene，而不是 bin 输出目录。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - GUI 必须接入 `Open`、`Save`、`SaveAs`。
  - 必须支持环境变量 `ANS_ENGINE_EDITOR_SCENE_PATH` 覆盖默认 scene。
  - 默认编辑源码目录 `src/Engine.App/SampleScenes/default.scene.json`。
  - Save 成功 `IsDirty=false`，Save 失败 `IsDirty=true`，Open 失败不污染当前 session，Save As 成功后状态栏路径更新。
- 本卡执行时不得推翻的既定取舍：
  - 不允许保存到 bin 输出目录副本并误报成功。
  - 不允许绕过 `SceneEditorSession.Save/SaveAs/Open`。
  - 不要求原生系统文件对话框；路径输入或最小选择即可。
- 计划结构约定：
  - `PLAN-M13-2026-04-30 > Milestones > M13.5` 已定稿文件工作流语义和环境变量名。

## 实施说明（ImplementationNotes）
- 在启动路径解析中加入 `ANS_ENGINE_EDITOR_SCENE_PATH`，优先使用该变量；未设置时解析仓库源码 sample scene。
- Toolbar 的 `Open`、`Save`、`Save As` 接入 session 操作；若无原生文件对话框，允许使用可编辑路径输入框或最小弹窗输入路径。
- Save 成功后只依赖 session 的 `IsDirty` 与 `SceneFilePath` 更新 UI；Save 失败必须显示 last error 并保持 dirty。
- Open 失败不得清空或污染已有 session；Open 成功后 Hierarchy/Inspector/Status Bar 必须刷新。
- 补测试或 seam 覆盖默认路径解析、环境变量覆盖、save 成功/失败状态、open 失败不污染当前 session。

## 设计约束（DesignConstraints）
- 不允许新增完整资源浏览器或复杂文件管理系统。
- 不允许直接使用 `JsonSceneDocumentStore` 绕过 `SceneEditorSession` 作为 GUI 主路径。
- 不允许修改 `Engine.App` 默认 scene 选择策略。
- 新增 C# 代码遵守 Engine 编码规范和一类一文件约定。

## 失败与降级策略（FallbackBehavior）
- 默认路径解析失败时，GUI 保持可启动并显示 last error，不得崩溃。
- Save/Save As 失败时保留内存修改，dirty 保持 true，状态栏显示失败。
- Open 失败时保留当前已打开 session；若当前无文档则保持无文档空状态。
- 若发现需要改变 `SceneEditorSession` 保存语义才能继续，必须回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `src/Engine.SceneData/DocumentStore/JsonSceneDocumentStore.cs`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-004.md`
  - `.ai-workflow/tasks/task-editor-004.md`
  - `.ai-workflow/tasks/task-app-009.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
- 计划结构引用：
  - `PLAN-M13-2026-04-30 > Milestones > M13.5`
  - `PLAN-M13-2026-04-30 > PlanningDecisions > 默认路径覆盖`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > 保存语义`

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - Open/Save/Save As GUI 编排、路径解析、状态同步和对应测试
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Add/Remove Object
- 不实现原生系统文件对话框作为硬要求
- 不修改 `Engine.App` 默认运行路径
- 不修改 `Engine.SceneData` 保存/加载契约
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - Open/Save As 的第一版路径选择可以是路径输入而非系统对话框。
- 处理规则：
  - 若 Human 要求原生文件对话框，应另立 UI 增强卡，不得扩大本卡。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确环境变量、默认路径、session API、成功/失败状态语义和非范围。
  - 参考点覆盖 M12 保存语义与 App sample scene。
  - 执行者无需回看计划全文即可避免 bin 目录误写。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及真实文件写回、路径解析、session 原子性和 GUI 状态刷新，失败语义必须清晰。
  - 默认路径选错会直接破坏 M13 验收。
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
- Test: `dotnet test AnsEngine.sln` 通过，路径解析与 save/open 状态测试通过
- Smoke: 修改对象后 Save 真实写回；Save 成功 `IsDirty=false`；Save 失败保持 dirty；Open 失败不污染当前 session；Save As 成功后状态栏路径更新
- Perf: Open/Save 只在用户触发时执行，无逐帧文件 IO；无明显帧时间退化说明

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EAPP-005.md`
- ClosedAt: `2026-04-30 15:53`
- Summary: Toolbar 接入 Open、Save、Save As 路径输入工作流，全部通过 `SceneEditorSession.Open/Save/SaveAs`，支持 `ANS_ENGINE_EDITOR_SCENE_PATH` 启动覆盖并验证保存/打开失败状态。
- FilesChanged:
  - `src/Engine.Editor.App/EditorFileWorkflowState.cs`
  - `src/Engine.Editor.App/EditorGuiRenderer.cs`
  - `tests/Engine.Editor.App.Tests/EditorFileWorkflowStateTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-eapp-005.md`
  - `.ai-workflow/archive/2026-04/TASK-EAPP-005.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；路径解析与 save/open 状态测试通过，`Engine.Editor.App.Tests` 23 条通过）
  - Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；Open/Save/Save As 控件渲染在真实 ImGui frame，退出码 0）
  - Perf: `pass`（Open/Save 仅在用户触发时执行，无逐帧文件 IO）
  - Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档）
- ModuleAttributionCheck: pass
- HumanSignoff: `pending`
