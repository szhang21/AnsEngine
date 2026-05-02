# 任务: TASK-EAPP-008 M16 Inspector component groups integration

## TaskId
`TASK-EAPP-008`

## 目标（Goal）
让 `Engine.Editor.App` 的 Inspector 和 GUI 编排迁移到 component groups 视图，并能正确展示与编辑 `Object` / `Transform` / `MeshRenderer`，同时对 Transform-only object 显示明确的无 `MeshRenderer` 状态。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M16-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M16.4b`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P1

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
- ParallelGroup: `M16-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EDITOR-005`

## 里程碑上下文（MilestoneContext）
- M16.4 的 GUI 部分必须在 headless component API 稳定后再落地，否则 Inspector 只能继续渲染旧扁平字段。
- 本卡承担的是 Inspector 组件分组展示、GUI 输入映射与无 `MeshRenderer` 状态呈现，不承担 `Engine.Editor` component API 设计或 SceneData 规则收口。
- 直接影响本卡实现的上游背景包括：Inspector 应显示 `Object` / `Transform` / `MeshRenderer` groups；Transform-only object 可打开、选择、保存而不自动补组件；所有 GUI 操作仍必须经 `SceneEditorSession`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Inspector 组件分组为：
    - `Object`: object id/name
    - `Transform`: position/rotation/scale
    - `MeshRenderer`: mesh/material 或明确无 `MeshRenderer` 状态
  - Add Object 默认创建 `Transform + MeshRenderer`，保持新增对象可见。
  - Transform-only object 可打开、选择、保存，不自动补 `MeshRenderer`。
  - GUI 必须通过 component API/session API 提交更新。
- 本卡执行时不得推翻的既定取舍：
  - 不允许 GUI 直接写 `SceneFileDocument` 或绕过 `SceneEditorSession`。
  - 不允许将 Transform-only object 的无组件状态“自动修复”为新增 `MeshRenderer`。
  - 不允许在 `Engine.Editor.App` 复制 SceneData validation 规则。
  - 不允许回退到扁平 inspector 面板组织。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M16-2026-05-02 > EditorDesign` 已定稿 Inspector component groups 的分组语义与 no-`MeshRenderer` 状态表达。
  - `PLAN-M16-2026-05-02 > PlanningDecisions` 已定稿 `Transform` 必需、`MeshRenderer` 可选，这会直接影响 GUI 空状态呈现逻辑。

## 实施说明（ImplementationNotes）
- 先迁移 GUI snapshot / inspector view model，使其按 `Object`、`Transform`、`MeshRenderer` 三组组织，而不是旧扁平字段。
- 再接通 GUI 输入到 `SceneEditorSession` 的 component API：
  - `Object` 组走 id/name 更新
  - `Transform` 组走 Transform component 更新
  - `MeshRenderer` 组走 MeshRenderer component 更新或明确空状态显示
- 然后补 Transform-only object GUI 语义：
  - 可被选择并显示 `Object` + `Transform`
  - `MeshRenderer` 区域显示清晰“无 MeshRenderer”状态
  - 不自动新增 optional component
- 最后补 `Engine.Editor.App.Tests`，覆盖 component groups、Transform-only object 空状态、更新提交和错误回滚。

## 设计约束（DesignConstraints）
- 不允许在本卡中修改 `Engine.Editor` headless core 公开语义或重新设计 component API。
- 不允许把 OpenTK/ImGui 依赖回流到 `Engine.Editor`。
- 不允许顺手做 Play Mode、Prefab、资源浏览器或新的窗口布局重构。
- 不允许修改 `Engine.App` 运行时主路径。

## 失败与降级策略（FallbackBehavior）
- 无选中对象时 Inspector 仍应显示空状态，不得抛异常。
- 若当前选中对象没有 `MeshRenderer`，应显示明确 no-`MeshRenderer` 状态，而不是报错或自动补组件。
- 若 GUI 适配暴露出缺失的 headless component API，必须停工回退到 `TASK-EDITOR-005`，不得绕过 session 直接改文档。
- 若输入缓冲策略有实现差异，可沿用现有 Inspector 输入模式，但提交后必须重新对齐 session 当前有效值。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Editor.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-004.md`
  - `.ai-workflow/tasks/task-eapp-006.md`
  - `.ai-workflow/tasks/task-editor-005.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M16-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M16-2026-05-02 > EditorDesign`
  - `PLAN-M16-2026-05-02 > Milestones > M16.4`
  - `PLAN-M16-2026-05-02 > TestPlan > Editor tests`
  - 上述 GUI 设计引用属于“实现约束”，分组语义和 Transform-only object 展示方式已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - Inspector component groups
  - GUI snapshot / input mapping
  - Editor.App tests
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 `Engine.Editor` component API 设计
- 不修改 `Engine.SceneData` schema/normalizer
- 不修改 `Engine.App` 运行时主循环
- 不新增 Play Mode、Prefab、资源浏览器
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.App/**`
  - `src/Engine.Scene/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - no-`MeshRenderer` 状态的具体文案或 UI 呈现可局部调整，但必须清晰区分“合法缺省”与“错误状态”。
- 处理规则：
  - 若问题影响是否自动补组件、session 单一真相或 GUI/Editor 边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 组件分组、Transform-only object 展示和经由 session 提交的要求都已明确。
  - 执行者无需回看计划也能知道这张卡不能直接写 JSON 或自动补 MeshRenderer。
  - 与前序 Editor core 卡的边界已拆清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及 GUI 视图模型、输入缓冲、component groups 和 Transform-only 空状态，若写短很容易回退到旧扁平 inspector。
  - 同时要守住 `Engine.Editor.App -> Engine.Editor` 的单向编排边界。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
  - `Engine.Editor.App -> Engine.Contracts`
  - `Engine.Editor.App -> Engine.Platform`
  - `Engine.Editor.App -> ImGui.NET`
- ForbiddenDependsOn:
  - `Engine.Editor.App -> Engine.App`
  - `Engine.Editor.App -> Engine.Render`
  - `Engine.Editor.App -> Engine.Asset`
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
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 component groups、Transform-only object、错误回滚
- Smoke: Inspector 显示 `Object` / `Transform` / `MeshRenderer` groups；Transform-only object 显示明确无 `MeshRenderer` 状态且不会自动补组件；新增对象仍默认可见
- Perf: GUI 不做逐帧文件写入、无额外 sample scene 重载轮询；输入缓冲无明显退化说明

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-EAPP-008.md`
- ClosedAt: `2026-05-02 12:43`
- Summary:
  - 2026-05-02: Started after `TASK-EDITOR-005` reached Review; card fields, dependencies, AllowedPaths and component group constraints checked.
  - 2026-05-02: Migrated Inspector snapshot/rendering/input state to `Object` / `Transform` / `MeshRenderer` component groups.
  - 2026-05-02: Routed GUI edit submission through component session APIs and preserved Transform-only objects without auto-adding MeshRenderer.
  - 2026-05-02: Migrated Editor.App tests/fixtures to M16 `version: "2.0"` component array and added Transform-only Inspector coverage.
- FilesChanged:
  - `src/Engine.Editor.App/EditorInspectorSnapshot.cs`
  - `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
  - `src/Engine.Editor.App/EditorInspectorInputState.cs`
  - `src/Engine.Editor.App/EditorGuiRenderer.cs`
  - `src/Engine.Editor.App/EditorAppController.cs`
  - `src/Engine.Editor.App/EditorDefaultObjectFactory.cs`
  - `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
  - `tests/Engine.Editor.App.Tests/EditorInspectorInputStateTests.cs`
  - `tests/Engine.Editor.App.Tests/EditorObjectWorkflowStateTests.cs`
  - `tests/Engine.Editor.App.Tests/EditorFileWorkflowStateTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-eapp-008.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --no-restore --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Test: `dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal` passed, 33 tests.
  - Smoke: `ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=0.1 ANS_ENGINE_EDITOR_ENABLE_NATIVE_IMGUI_FRAMES=0 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build --no-restore` exited 0.
  - Smoke: Inspector snapshot exposes `Object` / `Transform` / `MeshRenderer` groups; Transform-only object displays `No MeshRenderer component.` and apply does not add MeshRenderer.
  - Perf: GUI changes remain explicit user/session operations; no per-frame file writes or sample scene reload polling added.
  - Boundary: changes stayed under `src/Engine.Editor.App/**`, `tests/Engine.Editor.App.Tests/**` and task workflow docs; dependency rg hits are existing path resolver / boundary tests, not new forbidden project references.
- ModuleAttributionCheck: pass
