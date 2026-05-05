# 任务: TASK-EAPP-009 M21 Unity-like Editor shell and theme baseline

## TaskId
`TASK-EAPP-009`

## 目标（Goal）
将 `Engine.Editor.App` 的编辑器外壳升级为稳定的 Unity-like 停靠布局与专业深色主题，并把中央工作区收敛为可测试的 `SceneView` 区域模型。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M21-2026-05-05`

## 里程碑引用（兼容别名：MilestoneRef）
`M21.1`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Editor.App

## 次级模块（SecondaryModules）
- Engine.Editor

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M21-G1`
- CanRunParallel: `false`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M21.1 是本轮 editor 体验升级的入口卡；如果外壳和区域语义不先稳定，后续 Inspector 组件堆栈与 Scene View 预览都会继续附着在“调试面板式”布局上。
- 本卡承担的是 GUI shell、布局快照、命名收敛和主题风格，不承担新的编辑业务语义、组件 authoring 规则或 Render 预览接线。
- 直接影响本卡的上游背景包括：M13 已有 Toolbar/Hierarchy/Inspector/Status 骨架，M13.F1 已有 docked layout，但计划已明确本轮要把 `MainWorkspace*` 语义收敛成 `SceneView*` 并移除中央占位文案。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 布局必须固定为 Unity-like shell：
    - top Toolbar: roughly `48-56px`
    - left Hierarchy: about `22%` width
    - center Scene View: primary workspace
    - right Inspector: about `32%` width
    - bottom Status / Console-lite: roughly `34-48px`
  - 需要把 `EditorGuiLayoutSnapshot.MainWorkspace*` 概念扩展或重命名为 `SceneView*`，且该变化必须可测试。
  - 需要应用专业深色工具主题：深色 panel、清晰选中高亮、低或适中的圆角、统一 `FramePadding` / `ItemSpacing` / title spacing / panel border。
  - 必须移除占位文案 `Viewport is intentionally reserved for a later milestone.`
- 本卡执行时不得推翻的既定取舍：
  - 不允许引入自由 docking 框架、tab system、可拖拽多窗口或复杂 layout persistence。
  - 不允许在本卡顺手实现 Scene View 真正渲染预览；中央区域只需变成语义正确、风格稳定的 SceneView 容器。
  - 不允许把 GUI 主题或布局状态下沉到 `Engine.Editor` headless core。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Unity-like Shell And Theme` 已定稿区域形状与尺寸约束，执行时不得自行改成其他工作区比例或继续保留 `MainWorkspace` 术语。
  - `PLAN-M21-2026-05-05 > Milestones > M21.1` 已定稿“中央区域成为 Scene View、视觉收敛为专业深色工具风格”的交付结果。

## 实施说明（ImplementationNotes）
- 先收敛 `EditorGuiLayoutSnapshot`、相关 view model 和 renderer 命名，把中央区域从 `MainWorkspace` 改成 `SceneView` 语义，并补对应测试。
- 再调整固定停靠布局尺寸和分区计算，让 Toolbar、Hierarchy、SceneView、Inspector、Status 的大小与计划约束一致。
- 然后在 `EditorGuiRenderer` 或小型 theme helper 中集中应用深色主题参数，避免样式常量散落。
- 最后移除中央占位文案，改为 Scene View 容器标题/空态框架，并补 Editor.App GUI snapshot 与 smoke 验证。

## 设计约束（DesignConstraints）
- 不允许绕过现有 GUI snapshot/test 模型，直接把布局数字写死在 renderer 各处而不提供可测试输出。
- 不允许在本卡引入 Render、Asset、Scene 依赖；本卡仍应停留在当前 `Engine.Editor.App` 允许边界内。
- 不允许顺手改 Toolbar/Open/Save/Object workflow 业务语义。
- 不允许把 theme 常量散落到多个不相关窗口渲染函数中。

## 失败与降级策略（FallbackBehavior）
- 若主题细节需要根据 ImGui 实际观感微调，允许在不破坏“深色专业工具风格”的前提下做等价参数调整。
- 若 `SceneView` 重命名导致测试夹具牵连较大，可分两步提交：先补兼容字段，再完成全面重命名；但最终测试与公开 snapshot 语义必须以 `SceneView` 为准。
- 若实现中发现必须引入 preview 渲染依赖才能完成布局卡，必须停工回退；这说明卡被错误扩张到 M21.4。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/**`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-007.md`
  - `.ai-workflow/tasks/task-eapp-008.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M21-2026-05-05.md`
- 计划结构引用：
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Unity-like Shell And Theme`
  - `PLAN-M21-2026-05-05 > Milestones > M21.1`
  - 上述 layout/theme 引用属于“实现约束”，区域比例、命名收敛和占位文案移除已定稿。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - GUI layout snapshot / renderer / theme helper
  - Editor.App tests
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Script / RigidBody / BoxCollider authoring
- 不实现 Scene View preview 渲染
- 不实现自由 docking、Undo/Redo、Gizmo、Project Browser、Play Mode
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `Console-lite` 是否继续复用 Status Bar 区域命名，还是以 Status/Console 双标题呈现，可按最小改动原则选择。
- 处理规则：
  - 若问题影响模块边界、SceneView 语义收敛或未来 preview 嵌入点，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 布局比例、命名收敛、主题方向和中央占位移除都已落卡。
  - 执行者无需回看里程碑全文也能理解这是一张 shell/theme 卡，而不是 preview/render 卡。
  - 禁止扩张方向和停工条件明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 以单模块 GUI 调整为主，但存在命名收敛、测试同步和视觉基线误做风险。
  - 如果写得过短，容易把 M21.1 和 M21.4 混在一起。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
  - `Engine.Editor.App -> Engine.Contracts`
  - `Engine.Editor.App -> Engine.Platform`
- ForbiddenDependsOn:
  - `Engine.Editor.App -> Engine.App`
  - `Engine.Editor.App -> Engine.Render`
  - `Engine.Editor.App -> Engine.Asset`
  - `Engine.Editor.App -> Engine.Scene`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 Toolbar/Hierarchy/SceneView/Inspector/Status 布局快照和主题相关状态
- Smoke: `Engine.Editor.App` 启动后中央区域不再显示旧占位文案，GUI shell 为稳定的 Unity-like 深色布局
- Perf: 不引入逐帧文件 IO、重复 theme rebuild 或跨模块 preview/render 接线

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Review

## 完成度（Completion）
`95`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-EAPP-009.md`
- ClosedAt: `2026-05-05`
- Summary:
  - Editor GUI layout snapshot now exposes `SceneViewPosition` / `SceneViewSize` instead of `MainWorkspace*`.
  - Toolbar height is reduced to the M21 Unity-like shell range and Scene View occupies the central docked region.
  - Added a testable dark tool theme snapshot and applied it through `EditorGuiRenderer`.
  - Removed the old viewport placeholder text without adding preview/render dependencies.
- FilesChanged:
  - `src/Engine.Editor.App/EditorGuiSnapshot.cs`
  - `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
  - `src/Engine.Editor.App/EditorGuiRenderer.cs`
  - `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-eapp-009.md`
  - `.ai-workflow/archive/2026-05/TASK-EAPP-009.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL warnings only)
  - Test: pass (`dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal`; 33 passed)
  - Smoke: pass (old `Viewport is intentionally reserved for a later milestone.` text absent; central dock now renders `Scene View` container)
  - Boundary: pass (`Engine.Editor.App.csproj` still references only Editor, SceneData, Contracts, Platform; no Render/Asset/Scene dependency added)
  - Perf: pass (theme applied in-frame only; no file IO, preview/render hookup, or asset loading introduced)
- ModuleAttributionCheck: pass
