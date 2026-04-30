# 任务: TASK-EAPP-001 M13 Editor GUI 宿主入口

## TaskId
`TASK-EAPP-001`

## 目标（Goal）
新增独立可执行编辑器宿主 `Engine.Editor.App`，建立 GUI 窗口主循环、`SceneEditorSession` 装配、默认 sample scene 加载和模块边界合同，使 `dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj` 能启动并关闭最小编辑器窗口。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M13-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M13.1`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Editor.App

## 次级模块（SecondaryModules）
- Engine.Editor
- Engine.SceneData
- Engine.Contracts
- Engine.Platform

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M13-G1`
- CanRunParallel: `false`
- DependsOn:

## 里程碑上下文（MilestoneContext）
- M13 的目标是把 M12 已完成的 `Engine.Editor` headless core 变成可运行、可点击、可保存的最小 GUI 场景编辑器。
- 本卡承担 M13 的第一步：建立独立 GUI 宿主入口和边界，避免把编辑器模式塞进 `Engine.App`，也避免把 OpenTK/ImGui 依赖引入 `Engine.Editor`。
- 上游直接影响本卡的背景包括：M12 已有 `SceneEditorSession`；默认 GUI 技术路线是 `OpenTK + ImGui.NET`；默认编辑文件是源码目录的 `src/Engine.App/SampleScenes/default.scene.json`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 默认入口必须是新增 `Engine.Editor.App`，不复用 `Engine.App`。
  - 默认运行命令必须是 `dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj`。
  - GUI 依赖只允许存在于 `Engine.Editor.App`，`Engine.Editor` 必须保持 headless core。
  - 默认编辑文件是源码目录下的 sample scene，而不是 bin 输出目录。
- 本卡执行时不得推翻的既定取舍：
  - 不允许修改 `Engine.App` 默认运行路径。
  - 不允许在 `Engine.Editor` 中新增 OpenTK、ImGui、窗口或输入依赖。
  - 不允许绕过 `SceneEditorSession` 直接读写 scene JSON 业务规则。
- 计划中的结构约定已定稿：
  - `PLAN-M13-2026-04-30 > ModuleBoundaries > Engine.Editor.App` 已定稿该模块职责、非职责、允许依赖和禁止路线。
  - `PLAN-M13-2026-04-30 > PlanningDecisions` 已定稿入口名、运行命令、GUI 技术、默认路径和路径覆盖变量。

## 实施说明（ImplementationNotes）
- 新增 `src/Engine.Editor.App/Engine.Editor.App.csproj`，引用 `Engine.Editor`、`Engine.SceneData`、`Engine.Contracts`，按实现需要引用 `Engine.Platform` 与 OpenTK/ImGui 包。
- 新增最小 `Program` / 宿主类型，创建窗口、初始化 ImGui 上下文、进入可关闭的主循环，并在启动时创建 `SceneEditorSession`。
- 默认 scene 路径先解析仓库源码路径 `src/Engine.App/SampleScenes/default.scene.json`；若执行环境难以定位仓库根，必须返回可诊断错误而不是静默加载 bin 目录副本。
- 新增 `.ai-workflow/boundaries/engine-editor-app.md` 与 `.ai-workflow/boundaries/README.md` 映射，写清 `Engine.Editor.App` 的 GUI 宿主边界和禁止依赖方向。
- 接入 `AnsEngine.sln`，补最小边界测试，确认 `Engine.Editor` 无 GUI/OpenTK/ImGui 依赖且 `Engine.App` 未引用 Editor App。

## 设计约束（DesignConstraints）
- 只建立最小窗口、主循环、session 装配和默认加载，不实现完整 Toolbar、Hierarchy、Inspector、Open/Save 工作流。
- 不允许把 GUI 宿主代码放进 `Engine.Editor` 或 `Engine.App`。
- 不允许在 GUI 宿主内复制 `SceneData` normalizer、document store 或 JSON 编辑业务规则。
- 新增 C# 代码遵守 Engine 编码规范：私有/保护字段用 `mCamelCase`，静态字段用 `sCamelCase`，常量用 `kCamelCase`；默认一个类一个文件、一个接口一个文件。

## 失败与降级策略（FallbackBehavior）
- 如果 OpenTK/ImGui 包接线失败，允许先保留最小可编译宿主骨架，但不得宣称 GUI smoke 通过。
- 如果默认 sample scene 路径无法解析，必须显示 last error 或控制台错误，且保持窗口可关闭。
- 如果实现需要修改 `Engine.Editor` 公开接口或 `Engine.App` 启动路径，必须停工回退修卡，不得在本卡内扩张。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `AnsEngine.sln`
- 相关测试入口：
  - `tests/Engine.Editor.Tests/EditorModuleBoundaryTests.cs`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-editor-001.md`
  - `.ai-workflow/tasks/task-editor-002.md`
  - `.ai-workflow/tasks/task-qa-013.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M13-2026-04-30.md`
- 计划结构引用：
  - `PLAN-M13-2026-04-30 > Milestones > M13.1`
  - `PLAN-M13-2026-04-30 > ModuleBoundaries > Engine.Editor.App`
  - `PLAN-M13-2026-04-30 > PlanningDecisions`
  - 上述引用属于参考实现约束，尤其是独立入口、默认路径和 GUI 依赖边界。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - Editor GUI app 项目、入口、最小宿主、边界测试、solution 接入
- AllowedPaths:
  - `AnsEngine.sln`
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Toolbar、Hierarchy、Inspector、Open/Save、Add/Remove
- 不新增 Gizmo、鼠标 3D 拾取、Undo/Redo、资源浏览器、Prefab、热重载或 Play Mode
- 不修改 `Engine.App` 默认运行入口
- 不修改 `Engine.Editor` 的 headless 职责边界
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Render/**`
  - `src/Engine.Asset/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - ImGui.NET 的具体封装类型与文件拆分可由 Execution 在 `Engine.Editor.App` 内选择，但不得改变模块边界。
- 处理规则：
  - 若选择的 GUI 包或窗口封装要求改动 `Engine.Editor` / `Engine.App`，必须回退，不得自行脑补扩张。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确独立宿主、默认运行命令、默认加载路径和禁止依赖方向。
  - 实现入口、边界文档、测试入口和失败处理口径已下沉。
  - 执行者无需回看计划全文即可知道本卡只做最小宿主入口。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 新增可执行模块、第三方 GUI 依赖、solution 接入和边界合同，边界路线选错会污染 `Engine.Editor` 或 `Engine.App`。
  - 需要同时满足窗口 smoke、默认路径解析、session 装配和禁止依赖检查。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
  - `Engine.Editor.App -> Engine.Contracts`
  - `Engine.Editor.App -> Engine.Platform`
  - `Engine.Editor.App -> OpenTK`
  - `Engine.Editor.App -> ImGui.NET`
- ForbiddenDependsOn:
  - `Engine.Editor -> Engine.Editor.App`
  - `Engine.App -> Engine.Editor.App`
  - `Engine.Render -> Engine.Editor.App`
  - `Engine.Editor.App -> Engine.App`

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
  - `.ai-workflow/boundaries/README.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln` 通过，`Engine.Editor.App` 参与构建
- Test: `dotnet test AnsEngine.sln` 通过，新增边界测试覆盖 Editor/App 禁止依赖
- Smoke: `dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj` 能启动并关闭最小编辑器窗口，默认尝试打开源码 sample scene
- Perf: 仅新增 GUI 宿主启动成本；不改变运行时 `Engine.App` 主路径，无明显逐帧异常说明

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EAPP-001.md`
- ClosedAt: `2026-04-30 14:28`
- Summary: 新增独立 `Engine.Editor.App` 可执行宿主、OpenTK/ImGui 依赖边界、启动 scene 路径解析、最小窗口主循环、solution 接入和边界测试。
- FilesChanged:
  - `AnsEngine.sln`
  - `src/Engine.Editor.App/Engine.Editor.App.csproj`
  - `src/Engine.Editor.App/Program.cs`
  - `src/Engine.Editor.App/EditorAppProgram.cs`
  - `src/Engine.Editor.App/EditorAppOptions.cs`
  - `src/Engine.Editor.App/EditorScenePathResolver.cs`
  - `src/Engine.Editor.App/EditorAppController.cs`
  - `src/Engine.Editor.App/EditorAppWindow.cs`
  - `tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj`
  - `tests/Engine.Editor.App.Tests/EditorAppBoundaryTests.cs`
  - `tests/Engine.Editor.App.Tests/EditorAppControllerTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/boundaries/README.md`
  - `.ai-workflow/tasks/task-eapp-001.md`
  - `.ai-workflow/archive/2026-04/TASK-EAPP-001.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning 与本机 Windows Kits `LIB` warning）
  - Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；新增 `Engine.Editor.App.Tests` 5 条通过，整解测试通过）
  - Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；退出码 0，默认尝试打开源码 sample scene）
  - Perf: `pass`（仅新增 GUI 宿主启动与空帧清屏；未改变 `Engine.App` 主路径，无逐帧文件 IO）
  - Boundary: `pass`（`Engine.Editor.App` 未引用 `Engine.App/Render/Asset`；`Engine.Editor` 未新增 OpenTK/ImGui/窗口依赖；`Engine.App` 未引用 Editor App）
- ModuleAttributionCheck: pass
