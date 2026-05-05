# 任务: TASK-EAPP-010 M21 Inspector Script and Physics component stack integration

## TaskId
`TASK-EAPP-010`

## 目标（Goal）
让 `Engine.Editor.App` 的 Inspector 支持 `Scripts`、`RigidBody`、`BoxCollider` 组件组的展示、输入、Apply、添加/移除和 `PhysicsParticipation` 状态提示，并全部通过 `EditorAppController -> SceneEditorSession` 路径生效。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M21-2026-05-05`

## 里程碑引用（兼容别名：MilestoneRef）
`M21.3`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P0

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
- ParallelGroup: `M21-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EAPP-009`
  - `TASK-EDITOR-006`

## 里程碑上下文（MilestoneContext）
- M21.3 是这轮 editor 从“能编辑 Transform/MeshRenderer”迈向“能 author runtime Script/Physics 组件”的用户面主卡。
- 本卡承担的是 Inspector snapshot、controller wrapper、输入缓冲、Apply 和 add/remove 交互，不承担 headless session API 本身或 Scene View preview 渲染。
- 直接影响本卡实现的上游背景包括：M21.2 会提供新的 session component APIs；M17/M19 已定义 Script/RigidBody/BoxCollider 的 SceneData file-model；M20 已确定 runtime physics 参与条件是同时具备 `Transform + RigidBody + BoxCollider`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `EditorAppController` 需要为新的 session APIs 提供 wrapper，并继续通过 `CaptureResult(...)` 把失败更新到 `LastError`。
  - `EditorInspectorSnapshot` 需要扩展为以下 component groups：
    - `Object`
    - `Transform`
    - `MeshRenderer`
    - `Scripts`
    - `RigidBody`
    - `BoxCollider`
    - `PhysicsParticipation`
  - `PhysicsParticipation` 只报告选中对象是否同时具备：
    - `Transform`
    - `RigidBody`
    - `BoxCollider`
  - Script authoring v1：
    - 显示 script list
    - 至少可编辑第一个 script component 的 `scriptId`
    - script properties 通过 JSON object text area 编辑
    - Apply 时解析为 `IReadOnlyDictionary<string, SceneFileScriptPropertyValue>`
    - parse failure 更新 `LastError` 且文档不变
  - RigidBody authoring v1：
    - body type combo: `Static` / `Dynamic`
    - mass numeric input
    - default `Dynamic` mass: `1`
    - default `Static` mass: `0` or omitted according to existing normalization behavior
  - BoxCollider authoring v1：
    - size `InputFloat3`
    - center `InputFloat3`
    - default size `(1, 1, 1)`
    - default center `(0, 0, 0)`
  - component add/remove buttons 需要放在各 component group title 附近
  - v1 Inspector 不允许移除 `Transform`
- 本卡执行时不得推翻的既定取舍：
  - 不允许绕过 `SceneEditorSession` 直接改 scene JSON/document。
  - 不允许把 script property UI 扩成 typed schema editor；M21 接受 JSON text area。
  - 不允许把 `PhysicsParticipation` 做成 runtime simulation、preview physics 或 Play Mode 开关。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Editor.App Controller And Inspector` 已定稿 Inspector groups、输入形状与 JSON 解析规则，执行时不得自行替换成其他 group 名称或不同 property 编辑语义。
  - `PLAN-M21-2026-05-05 > Milestones > M21.3` 已定稿交付结果是“支持输入、Apply、添加/移除和 Physics participation 状态提示”。

## 实施说明（ImplementationNotes）
- 先扩展 `EditorAppController` wrapper，让 GUI 仍只消费 controller，不直接碰 session 结果。
- 再扩展 `EditorInspectorSnapshot` 和相关输入缓冲模型，补 `Scripts`、`RigidBody`、`BoxCollider`、`PhysicsParticipation` 四组视图状态。
- 然后实现 Apply / add / remove 工作流：
  - Script JSON parse -> session update
  - RigidBody bodyType/mass -> session update
  - BoxCollider size/center -> session update
  - remove paths -> controller/session
- 最后补 Editor.App tests，至少覆盖 group 展示、input state 回流、controller 调用、parse failure last-error 和 `PhysicsParticipation` 判定。

## 设计约束（DesignConstraints）
- 不允许把 JSON parse、normalization 或 schema 校验逻辑复制到 GUI；GUI 只做最小文本解析和错误展示。
- 不允许在本卡引入 Render、Asset、Scene 依赖；本卡仍只属于 Inspector authoring UI。
- 不允许移除 `Transform` 或修改现有 Object/Transform/MeshRenderer 基础编辑语义。
- 不允许顺手实现 script list reorder、多脚本复杂管理、Undo/Redo 或 typed property grid。

## 失败与降级策略（FallbackBehavior）
- 若多 script component 的完整 UI 管理在本轮超出最小范围，可接受 v1 仅稳定编辑第一个 script component，但列表展示和失败语义必须清晰。
- 若 Script properties JSON parse 失败，必须更新 `LastError` 且保持文档与 session 不变；不得部分写入。
- 若实现中发现必须修改 `Engine.Editor` 公开 API 之外的 SceneData schema 才能继续，必须停工回退；这超出本卡设计前提。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Editor.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-008.md`
  - `.ai-workflow/tasks/task-editor-006.md`
  - `.ai-workflow/tasks/task-sdata-008.md`
  - `.ai-workflow/tasks/task-sdata-009.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M21-2026-05-05.md`
- 计划结构引用：
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Editor.App Controller And Inspector`
  - `PLAN-M21-2026-05-05 > Milestones > M21.3`
  - `PLAN-M21-2026-05-05 > TestPlan`
  - 上述 inspector/input 引用属于“实现约束”，group 名称、输入控件形态和 parse-failure 语义已定稿。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - Inspector snapshot / controller / renderer / input buffer
  - Editor.App tests
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Scene View preview 渲染
- 不实现 headless session API
- 不实现 runtime physics bridge 或 Play Mode
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 多 script component 的 add/remove 按钮是否只面向首个 script 还是最小列表项级别，可按最小稳定交互选择，但必须保持列表与失败反馈清晰。
- 处理规则：
  - 若问题影响 session ownership、JSON parse failure 语义或组件组命名，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - Component groups、输入形状、controller 责任和 parse-failure 语义都已下沉到卡面。
  - 执行者无需回看计划也能知道本卡是 Inspector integration 卡，不是 preview/render 卡。
  - 允许的 v1 简化和必须停工条件都已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 GUI 状态、session API、JSON text parse 与 physics-ready 提示交汇点，误做空间很大。
  - 如果信息不够，容易把 GUI 直接做成业务真相或把 parse/validation 下沉错层。
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
- Test: `dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖新 groups、Apply、add/remove、last-error 和 physics-ready 判定
- Smoke: Editor 中可编辑 Script/RigidBody/BoxCollider，Apply 成功时 session 更新，parse failure 时 `LastError` 可见且文档不变
- Perf: 不引入逐帧 JSON parse、重复 controller/session roundtrip 或新 Render/Scene 依赖

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-EAPP-010.md`
- ClosedAt: `2026-05-05`
- Summary:
  - Added Inspector snapshots for Scripts, RigidBody, BoxCollider and PhysicsParticipation.
  - Added EditorAppController wrappers for new SceneEditorSession component APIs.
  - Extended Inspector input state and renderer for Script JSON, RigidBody body type/mass and BoxCollider size/center Apply plus add/remove.
  - Added tests for group display, apply/update, JSON parse failure rollback and physics-ready state.
- FilesChanged:
  - `src/Engine.Editor.App/EditorAppController.cs`
  - `src/Engine.Editor.App/EditorGuiRenderer.cs`
  - `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
  - `src/Engine.Editor.App/EditorInspectorInputState.cs`
  - `src/Engine.Editor.App/EditorInspectorSnapshot.cs`
  - `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
  - `tests/Engine.Editor.App.Tests/EditorInspectorInputStateTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-eapp-010.md`
  - `.ai-workflow/archive/2026-05/TASK-EAPP-010.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL warnings only)
  - Test: pass (`dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal`; 37 passed)
  - Smoke: pass (Inspector can edit Script/RigidBody/BoxCollider through controller/session; invalid Script JSON updates LastError and leaves document unchanged)
  - Boundary: pass (`Engine.Editor.App.csproj` still references only Editor, SceneData, Contracts, Platform; no Render/Asset/Scene dependency added)
  - Perf: pass (Script JSON parse only on Apply; no per-frame scene/asset/render work)
- ModuleAttributionCheck: pass
