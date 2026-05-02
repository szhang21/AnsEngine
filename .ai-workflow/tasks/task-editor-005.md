# 任务: TASK-EDITOR-005 M16 Editor component API migration

## TaskId
`TASK-EDITOR-005`

## 目标（Goal）
让 `Engine.Editor` 的 `SceneEditorSession` 与文档编辑工作流迁移到 component API，使 headless editor core 能正确打开、编辑、保存 Transform-only object，以及通过 component operations 更新 Transform 与 MeshRenderer。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M16-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M16.4a`

## 执行代理（ExecutionAgent）
Exec-Editor

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Editor

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Contracts
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M16-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-018`

## 里程碑上下文（MilestoneContext）
- M16.4 的 headless core 部分必须先迁移 `Engine.Editor`，否则 GUI 侧只能继续围绕旧 `UpdateObjectTransform` / `UpdateObjectResources` 扁平 API 编排。
- 本卡承担的是 component editing API、session 语义与 Editor tests 收口，不承担 Inspector 组件分组 GUI 渲染。
- 直接影响本卡实现的上游背景包括：SceneData 编辑 API 要迁移为 component API；Object id/name 仍可保持 object-level；Transform-only object 必须可打开、选择、保存且不自动补 `MeshRenderer`；Add Object 默认仍创建 `Transform + MeshRenderer` 保持可见。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - SceneData 编辑 API 迁移为 component API。
  - 旧 `UpdateObjectTransform` / `UpdateObjectResources` 不应再作为主 API 路径。
  - Add Object 默认创建 `Transform + MeshRenderer`。
  - Transform-only object 可打开、选择、保存，不自动补 `MeshRenderer`。
  - dirty/save/reload 语义保持稳定。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 `Engine.Editor` 内复制 SceneData normalizer 或 JSON 规则。
  - 不允许为兼容 GUI 方便保留旧扁平编辑 API 作为主路径。
  - 不允许在保存时静默修复缺失 optional `MeshRenderer`。
  - 不允许把 ImGui/OpenTK 或 GUI 状态带入 `Engine.Editor`。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M16-2026-05-02 > EditorDesign` 已定稿 Inspector 与编辑工作流围绕组件分组和 component API 组织，而不是扁平 mesh/material/transform 字段。
  - `PLAN-M16-2026-05-02 > PlanningDecisions` 已定稿 `Transform` 必需、`MeshRenderer` 可选、Transform-only object 不自动补组件。

## 实施说明（ImplementationNotes）
- 先在 `Engine.Editor` 将 session 的内部编辑路径切到 component API：
  - object id/name 仍保留 object-level 更新
  - Transform / MeshRenderer 改走 component add/update/remove 或等价显式入口
- 再迁移 Add Object 默认工厂，确保新增对象仍创建 `Transform + MeshRenderer`，保持新对象可见。
- 然后补 session/query 语义，确保 Transform-only object：
  - 可打开/选择
  - 可保存并通过 reload
  - 不会在 save/reload 中被自动补 `MeshRenderer`
- 最后补 Editor tests，覆盖 Transform-only object、component editing、dirty/save/reload 稳定性和旧 API 不再是主路径。

## 设计约束（DesignConstraints）
- 不允许在本卡直接修改 `Engine.Editor.App` GUI 布局、Inspector 绘制或输入缓冲。
- 不允许保留旧扁平编辑方法作为正式推荐路径继续扩展。
- 不允许改变 `Engine.Editor` 无 GUI、无 App、无 Render 依赖边界。
- 不允许顺手引入 Undo/Redo、Prefab、Play Mode 或资源浏览器。

## 失败与降级策略（FallbackBehavior）
- 若过渡期需要保留旧方法作为内部桥接兼容层，允许短期保留，但必须明确它不再是主 API，并避免新逻辑继续依赖它。
- 若 Transform-only object 在现有 session 查询面上缺少显示字段，可补只读查询投影，但不得自动注入 optional component。
- 若实现中发现必须改 GUI 才能完成 headless core 语义，必须停工回退；GUI 迁移留给下一张 `Engine.Editor.App` 卡。
- 若 component API 迁移必须改 SceneData 规则本身，也必须回退，因为那属于前序卡范围。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Contracts/**`
- 相关测试入口：
  - `tests/Engine.Editor.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-editor-003.md`
  - `.ai-workflow/tasks/task-editor-004.md`
  - `.ai-workflow/tasks/task-sdata-005.md`
  - `.ai-workflow/tasks/task-sdata-007.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M16-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M16-2026-05-02 > EditorDesign`
  - `PLAN-M16-2026-05-02 > Milestones > M16.4`
  - `PLAN-M16-2026-05-02 > TestPlan > Editor tests`
  - 上述 editor design 引用属于“实现约束”，component API 方向和 Transform-only object 语义已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor
- AllowedFiles:
  - SceneEditorSession component API migration
  - headless editor tests
- AllowedPaths:
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 Inspector GUI 绘制或布局
- 不修改 `Engine.Scene` runtime bridge
- 不修改 `Engine.SceneData` schema/normalizer 规则
- 不引入 GUI、Undo/Redo、Prefab、Play Mode
- OutOfScopePaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`
  - `src/Engine.Scene/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - component API 的具体入口命名可随现有 `SceneEditorSession` 风格调整，但主编辑路径必须已组件化。
- 处理规则：
  - 若问题影响 dirty/save/reload 语义、Transform-only object 行为或 `Engine.Editor` 边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 组件化编辑方向、Transform-only object 语义和 Add Object 默认策略都已明确。
  - 执行者无需回看计划也能知道这张卡只做 headless core，不做 GUI。
  - 旧 API 的处置和停工条件已经写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 Editor headless core 的语义迁移卡，若继续围绕扁平 API，会直接阻塞 M16 GUI 和保存链路。
  - 同时涉及 component API、dirty/save/reload 和 Transform-only object 语义，容易做偏。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor -> Engine.SceneData`
  - `Engine.Editor -> Engine.Contracts`
  - `Engine.Editor -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Editor -> Engine.App`
  - `Engine.Editor -> Engine.Render`
  - `Engine.Editor -> Engine.Platform`
  - `Engine.Editor -> Engine.Asset`
  - `Engine.Editor -> Engine.Editor.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Editor.Tests/Engine.Editor.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 Add Object、Transform-only object、component editing、dirty/save/reload
- Smoke: `SceneEditorSession` 可打开并保存 Transform-only object，不自动补 `MeshRenderer`；新建对象默认仍有 `Transform + MeshRenderer`
- Perf: component editing 与 save/reload 仅在显式 session 命令时运行，无逐帧 IO、无 GUI 依赖渗入

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
- DetectedAt: `2026-05-02 11:52`
- ReopenReason: `Resolved as downstream GUI scope; TASK-EDITOR-005 acceptance is Engine.Editor headless core and Engine.Editor.Tests. TASK-EAPP-008 will migrate Editor.App fixtures and Inspector UI.`
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-EDITOR-005.md`
- ClosedAt: `2026-05-02 12:37`
- Summary:
  - 2026-05-02: Started after `TASK-SCENE-018` reached Review; card fields, dependencies, AllowedPaths and M16 schema constraints checked.
  - 2026-05-02: Migrated `SceneEditorSession` headless edit path to component operations for Transform and MeshRenderer; default add-object factory creates Transform + MeshRenderer, and MeshRenderer removal preserves Transform-only objects.
  - 2026-05-02: Updated `Engine.Editor.Tests` fixtures to M16 `version: "2.0"` component array and added Transform-only save/select coverage.
  - 2026-05-02 GateFailure resolved as downstream GUI scope: `Engine.Editor.App.Tests` failures are owned by `TASK-EAPP-008`, while this card's headless core acceptance passes.
- FilesChanged:
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/tasks/task-editor-005.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --no-restore --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Test: `dotnet test tests/Engine.Editor.Tests/Engine.Editor.Tests.csproj --no-restore --nologo -v minimal` passed, 30 tests.
  - DeferredRegression: `Engine.Editor.App.Tests` legacy `version: "1.0"` fixture failures are assigned to `TASK-EAPP-008`, which owns Editor.App GUI/test paths.
  - Smoke: component edit coverage passed in `Engine.Editor.Tests`: default AddObject creates Transform + MeshRenderer, Transform-only object can open/select/save without auto-adding MeshRenderer.
  - Perf: component edits run only on explicit session commands; no GUI dependency or per-frame IO added.
  - Boundary: `rg` path/dependency check found only boundary assertion test text for forbidden dependency names; implementation stays under allowed Editor paths.
- ModuleAttributionCheck: pass
