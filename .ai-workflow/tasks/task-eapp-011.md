# 任务: TASK-EAPP-011 M21 Scene View preview foundation

## TaskId
`TASK-EAPP-011`

## 目标（Goal）
在 `Engine.Editor.App` 中落地最小 `Scene View` 预览基础能力，让编辑器能以非空预览消费当前 scene，并通过 `Engine.Render` 现有绘制路径显示 edit-time scene，而不执行 script、physics 或 `ApplicationHost`。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M21-2026-05-05`

## 里程碑引用（兼容别名：MilestoneRef）
`M21.4`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Editor.App

## 次级模块（SecondaryModules）
- Engine.Editor
- Engine.Scene
- Engine.Render
- Engine.Asset

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M21-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EAPP-009`
  - `TASK-EAPP-010`

## 里程碑上下文（MilestoneContext）
- M21.4 是本轮 editor 从“调试式 GUI”跃迁到“具备真实工作区预览”的关键卡；如果没有这张卡，M21 不能以“Unity-like Editor Authoring MVP”收口。
- 本卡承担的是 `Engine.Editor.App` 内的 preview composition、SceneView refresh、与 Render/Asset/Scene 的窄接线，不承担 runtime app 主循环、Play Mode、script 执行或 physics simulate。
- 直接影响本卡实现的上游背景包括：M21.1 已把中央区域收敛为 SceneView 容器；M10/M14/M20 已建立 scene load -> render 主链路；计划明确允许 `Engine.Editor.App` 在边界更新后作为 GUI + preview composition root 依赖 Render/Asset/Scene。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 需要在 `Engine.Editor.App` 内新增 `EditorScenePreviewHost` 或等价 internal helper。
  - preview host 职责固定为：
    - consume `SceneEditorSession.Scene`
    - own a preview `SceneGraphService` or narrow adapter
    - load normalized scene into preview scene state
    - use `DiskMeshAssetProvider` or equivalent sample asset provider for preview meshes
    - render a nonblank edit-time scene preview
  - preview 刷新时机固定为：
    - scene opened
    - Apply succeeds
    - Save/SaveAs succeeds
    - selection changes, if selection visualization is UI-level or preview-level
  - M21 Scene View 不执行 scripts、不运行 physics、不调用 `ApplicationHost`、不进入 Play Mode。
  - preferred render strategy：
    - reuse `Engine.Render` draw path through existing `SceneRenderFrame` contracts where feasible
    - 若 ImGui child region 嵌入需要 viewport/scissor plumbing，代码必须留在 `Engine.Editor.App`
    - avoid changing `Engine.Render` public contracts unless absolutely required
  - selection visualization：
    - required: Hierarchy/Inspector selected state clear
    - optional: SceneView visual highlight only if it does not widen render contracts in M21
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 preview 变成第二套 `Engine.App` runtime 或调用 `ApplicationHost`。
  - 不允许把 preview 逻辑下沉到 `Engine.Render`、`Engine.Scene` 或 `Engine.Editor` 里。
  - 不允许为了预览顺手扩展 Render 公共 contract，除非别无选择且范围受控在 `Engine.Editor.App` 嵌入层。
  - 不允许把 script execution、physics step、Play Mode 或 selection-gizmo 混进本卡。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Scene View Preview` 已定稿 preview host 责任、刷新事件和“非 runtime/非 play-mode”约束，执行时不得改成别的 ownership flow。
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Dependency Direction` 已定稿 `Engine.Editor.App` 经边界更新后允许依赖 `Engine.Scene` / `Engine.Render` / `Engine.Asset`，但 `Engine.Editor` 仍必须 headless。

## 实施说明（ImplementationNotes）
- 先在 `Engine.Editor.App` 内新增 preview host/orchestrator，收敛 scene snapshot 到 renderable preview frame 的装配逻辑。
- 再把 preview 刷新触发接到 open / apply / save / save-as / selection-change 等现有 controller/session 事件点上。
- 然后把 SceneView 容器与 preview host 渲染输出接起来，确保中央区域出现非空预览，而不是文本占位。
- 最后补 Editor.App tests 和 smoke，至少覆盖：
  - open scene -> preview nonblank
  - apply edits -> preview refresh
  - save/save-as -> preview refresh
  - `Engine.Editor` 仍无 GUI/Render/Asset/Scene 依赖
  - `Engine.Editor.App` 边界合同与实际依赖一致

## 设计约束（DesignConstraints）
- 不允许在 `Engine.Editor.App` 外新增 preview composition root。
- 不允许直接复制 `Engine.App` runtime bootstrap 形成平行运行时；preview 必须是 edit-time narrow path。
- 不允许修改 `Engine.Render` / `Engine.Scene` 公共 API 作为默认路线；只有嵌入所需最小适配可被考虑。
- 不允许顺手引入 picking、gizmo、camera freelook、play button、physics preview 或 script hot reload。

## 失败与降级策略（FallbackBehavior）
- 若 ImGui child region 嵌入需要额外 viewport/scissor plumbing，可在 `Engine.Editor.App` 内新增小型 embedding helper；不得把这部分扩散进 `Engine.Render` 公共面。
- 若 selection highlight 需要扩大 render contracts，M21 允许完全不做 SceneView 选中高亮，只保留 Hierarchy/Inspector 的清晰选中态。
- 若实现中发现必须让 `Engine.Render -> Engine.Editor.App`、`Engine.Scene -> Engine.Editor.App` 或 `Engine.Editor -> Render/Asset/Scene` 才能继续，必须停工回退。
- 若最终仍只能显示空白或占位文本，不得宣称完成；M21.4 的最低线就是“visible nonblank preview”。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Contracts/**`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Render.Tests/**`
  - `tests/Engine.Scene.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-009.md`
  - `.ai-workflow/tasks/task-eapp-010.md`
  - `.ai-workflow/tasks/task-app-008.md`
  - `.ai-workflow/tasks/task-scene-013.md`
  - `.ai-workflow/tasks/task-app-021.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M21-2026-05-05.md`
- 计划结构引用：
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Dependency Direction`
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Scene View Preview`
  - `PLAN-M21-2026-05-05 > Milestones > M21.4`
  - `PLAN-M21-2026-05-05 > Risks`
  - 上述 preview/dependency 引用属于“实现约束”，preview host ownership、刷新事件和非 Play Mode 语义已定稿。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
  - Engine.Render
  - Engine.Scene
  - Engine.Asset
- AllowedFiles:
  - preview host / embedding helper / controller wiring
  - minimal render or scene adapter only if required for preview embedding
  - Editor.App tests
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Asset/**`
  - `tests/Engine.Asset.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现 Play Mode、script execution、physics simulate、picking、gizmo
- 不把 preview 逻辑下沉到 `Engine.Editor` headless core
- 不改 `Engine.App` runtime 入口
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - preview camera 是否完全复用 scene 当前相机，还是在无合适 camera 时使用固定 fallback camera，可按最小可见非空预览原则选择。
- 处理规则：
  - 若问题影响边界方向、Render public contract widening 或 preview/runtime ownership，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - preview host 职责、刷新时机、依赖方向和“不运行 script/physics”的硬约束都已落卡。
  - 执行者无需回看计划也能明确本卡是 SceneView edit-time preview 卡，而不是 runtime/play-mode 卡。
  - 边界放宽审批与停工条件明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 GUI、Scene、Render、Asset 多模块汇合点，稍微选错 ownership 就会破坏边界。
  - 还涉及可见性最低线“nonblank preview”和嵌入层 fallback 分流。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
  - `Engine.Editor.App -> Engine.Contracts`
  - `Engine.Editor.App -> Engine.Platform`
  - `Engine.Editor.App -> Engine.Scene`
  - `Engine.Editor.App -> Engine.Render`
  - `Engine.Editor.App -> Engine.Asset`
- ForbiddenDependsOn:
  - `Engine.Editor.App -> Engine.App`
  - `Engine.Editor` 依赖 `Engine.Scene`
  - `Engine.Editor` 依赖 `Engine.Render`
  - `Engine.Editor` 依赖 `Engine.Asset`

## 边界变更请求（BoundaryChangeRequest）
- Required: `true`
- Status: `approved`
- RequestReason: `M21.4 需要 Engine.Editor.App 成为 GUI + preview composition root，显式依赖 Engine.Scene / Engine.Render / Engine.Asset；当前 engine-editor-app 边界合同尚未声明这些允许依赖。`
- ImpactModules:
  - `Engine.Editor.App`
  - `Engine.Scene`
  - `Engine.Render`
  - `Engine.Asset`
- HumanApprovalRef: `Human approved via “拆卡m21” on 2026-05-05`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal` 通过，并补充 preview refresh / boundary tests；如有必要再跑 `Engine.Render.Tests` 与 `Engine.Scene.Tests`
- Smoke: Editor 可打开样例场景并在 SceneView 中显示非空预览；Apply/Save/SaveAs 后预览会刷新；不执行 script 或 physics
- Perf: 不引入逐帧 scene reload、重复 asset cold-load 风暴或 runtime app bootstrap duplication

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
InProgress

## 完成度（Completion）
`60`

## 缺陷回流字段（Defect Triage）
- FailureType: `AcceptanceDispute`
- DetectedAt: `2026-05-06`
- ReopenReason: `Human review found that Scene View preview still draws a fixed placeholder triangle after preview submission generation. M21.4 must render projected triangles from real SceneRenderSubmission mesh batches so the default scene preview shows two cubes rather than a single placeholder shape.`
- OriginTaskId:
- HumanSignoff: `fail`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-EAPP-011.md`
- ClosedAt:
- Summary:
  - Reopened during human acceptance review because Scene View preview still uses a fixed placeholder triangle instead of projected triangles from real preview mesh batches.
  - Required fix: upgrade `EditorScenePreviewHost.Refresh` and `EditorGuiRenderer.DrawScenePreview` to render projected geometry from `SceneRenderSubmission`.
- FilesChanged:
- ValidationEvidence:
- ModuleAttributionCheck: fail
