# 任务: TASK-QA-017 M16 Component Serialization Bridge 门禁复验与归档

## TaskId
`TASK-QA-017`

## 目标（Goal）
对 M16 的 component serialization bridge 执行全量 build/test/smoke 与边界复验，确认 `2.0` component scene schema、Scene runtime bridge、Editor component workflow 和 sample scene 主链路都已稳定落地。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M16-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M16.5`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.SceneData

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Editor
- Engine.Editor.App
- Engine.App
- Engine.Render

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scenedata.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M16-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SDATA-006`
  - `TASK-SDATA-007`
  - `TASK-SCENE-018`
  - `TASK-EDITOR-005`
  - `TASK-EAPP-008`

## 里程碑上下文（MilestoneContext）
- M16.5 是 breaking schema migration 的关闭门禁，本卡不再新增功能，而是确认从 SceneData 文档层到 runtime/editor/app sample 的整条桥接链已经稳定。
- 本卡承担的是 build/test/smoke、边界复验、质量结论和归档准备，不承担业务实现。
- 直接影响本卡实现的上游背景包括：旧 `1.0` 不兼容、`2.0` component array 成为唯一主路径、Transform-only object 合法但不渲染、Editor 以 component groups 工作、M16 不引入脚本/物理/动画/Prefab/Play Mode。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `version: "2.0"` 是唯一主路径。
  - 旧 `1.0` 扁平对象格式加载失败。
  - `Transform` 必需、`MeshRenderer` 可选。
  - duplicate/unknown component load failure。
  - Camera 不组件化。
  - M15 默认旋转 smoke behavior 不序列化。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 QA 卡中补实现、偷偷恢复 `1.0` 兼容或直接改 sample scene 逃避问题。
  - 不允许把“渲染里看见东西了”当作 schema/runtime/editor 全链路通过的替代证据。
  - 不允许忽视 Transform-only object 的合法非渲染语义。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M16-2026-05-02 > SchemaDesign`、`NormalizedModelDesign`、`RuntimeBridgeDesign`、`EditorDesign` 是本卡判定是否按计划落地的固定参照。
  - `PLAN-M16-2026-05-02 > Scope > Out of scope` 是本卡判定是否越界的重要硬约束。

## 实施说明（ImplementationNotes）
- 先复核 `TASK-SDATA-006`、`TASK-SDATA-007`、`TASK-SCENE-018`、`TASK-EDITOR-005`、`TASK-EAPP-008` 的改动范围、`AllowedPaths` 和边界文档更新。
- 执行自动验证：
  - `dotnet build AnsEngine.sln --nologo -v minimal`
  - `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`
- 重点核验场景：
  - `2.0` component array sample scene 可加载/保存/重载
  - `1.0` 场景显式失败
  - Transform-only object 可进入 snapshot、可在 editor 打开保存，但不进入 render frame
  - Editor Inspector 显示 `Object` / `Transform` / `MeshRenderer` groups
  - `Engine.App -> SceneData -> Scene -> Render` 链路可运行
  - headless smoke 通过
- 单独做边界复验，确认：
  - `Engine.SceneData` 不依赖 `Engine.Scene` runtime types
  - `Engine.Scene` 不依赖 `Engine.Editor` / `Engine.Editor.App` / `Engine.Render`
  - `Engine.Render` 不感知 SceneData component schema 或 runtime component types
  - M16 未引入脚本、物理、动画、Prefab、Play Mode
- 最后产出 `CodeQuality`、`DesignQuality`、风险摘要和归档证据缺口。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中修功能、改 schema 或移动模块边界。
- 不允许跳过 `Engine.SceneData.Tests`、`Engine.Scene.Tests`、`Engine.Editor.Tests`、`Engine.Editor.App.Tests`、`Engine.App.Tests` 回归验证。
- 不允许忽视 sample scene 已迁移为 `2.0` 的事实。
- 不允许把 Transform-only object 不渲染误判为功能失败。

## 失败与降级策略（FallbackBehavior）
- 若 Build/Test/Smoke/Perf 任一门禁失败，本卡不得进入 `Review`，必须记录失败证据并回退。
- 若发现任何模块通过偷引依赖或恢复 `1.0` 兼容来通过测试，必须判定为高风险边界失败。
- 若发现 Transform-only object 只能通过自动补 `MeshRenderer` 才能通过 editor/save/render 链路，必须判定为设计失败。
- 若 `MustFixCount > 0`，必须保持 `MustFixDisposition=follow-up-created` 或回退原卡，不得直接放行。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`
  - `src/Engine.App/**`
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.Editor.Tests/**`
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Render.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-006.md`
  - `.ai-workflow/tasks/task-sdata-007.md`
  - `.ai-workflow/tasks/task-scene-018.md`
  - `.ai-workflow/tasks/task-editor-005.md`
  - `.ai-workflow/tasks/task-eapp-008.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M16-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M16-2026-05-02 > SchemaDesign`
  - `PLAN-M16-2026-05-02 > NormalizedModelDesign`
  - `PLAN-M16-2026-05-02 > RuntimeBridgeDesign`
  - `PLAN-M16-2026-05-02 > EditorDesign`
  - `PLAN-M16-2026-05-02 > TestPlan`
  - `PLAN-M16-2026-05-02 > Scope`

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
  - Engine.Scene
  - Engine.Editor
  - Engine.Editor.App
  - Engine.App
  - Engine.Render
- AllowedFiles:
  - QA 只读验证证据
  - 任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不执行 `Review -> Done`
- 不新增脚本、物理、动画、Prefab、Play Mode
- OutOfScopePaths:
  - `src/Engine.Asset/**`
  - `src/Engine.Platform/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若部分 smoke 证据主要来自测试或 headless run，而非人工窗口录屏，需要在 QA 结论中明确证据口径，但不能降格门禁。
- 处理规则：
  - 若问题影响门禁结论、must-fix 归属或是否可以关闭 M16，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已把 M16 终局验证的成功标准、越界风险、Transform-only object 语义和必须覆盖的测试面都下沉了。
  - 执行者无需回看计划也能做全链路 QA 复验。
  - must-fix 和失败回退规则已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - M16 是 breaking schema migration，QA 需要同时验证文件层、normalized 层、runtime、editor 和 app sample，多入口且高风险。
  - 若 QA 卡写短，很容易把“部分链路可跑”误当成“迁移已稳”。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.SceneData -> Engine.Contracts`
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
  - `Engine.Editor -> Engine.SceneData`
  - `Engine.Editor -> Engine.Contracts`
  - `Engine.Editor -> Engine.Core`
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
  - `Engine.Editor.App -> Engine.Contracts`
  - `Engine.Editor.App -> Engine.Platform`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Render`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.SceneData -> Engine.Scene` runtime types
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.Editor`
  - `Engine.Scene -> Engine.Editor.App`
  - `Engine.Render -> Engine.SceneData` component schema
  - `Engine.Render -> Engine.Scene` runtime component types

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，SceneData/Scene/Editor/Editor.App/App/Render 相关测试无退化
- Smoke: `2.0` sample scene 可运行；Transform-only object 可加载/编辑/保存但不渲染；Inspector 显示 component groups；headless app smoke 通过
- Perf: 无新增逐帧 JSON 解析、双重 normalize、自动补组件轮询或 render side effect
- CodeQuality:
  - NoNewHighRisk: `true`
  - MustFixCount: `0`
  - MustFixDisposition: `none`
- DesignQuality:
  - DQ-1 职责单一（SRP）: `pass`
  - DQ-2 依赖反转（DIP）: `pass`
  - DQ-3 扩展点保留（OCP-oriented）: `pass`
  - DQ-4 开闭性评估（可选）: `pass`

## 交付物（Deliverables）
- QAReport
- Build/Test/Smoke/Perf evidence
- CodeQuality and DesignQuality conclusions
- Risk list (high|medium|low)
- Archive readiness notes

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-017.md`
- ClosedAt: `2026-05-02 15:00`
- Summary: `M16 component serialization bridge 已完成门禁复验与人工验收收口；确认 2.0 component array schema、normalized component model、runtime bridge、Editor headless component API 与 Inspector component groups 均已打通，Transform-only object 可加载/编辑/保存但不渲染，且未越界到脚本/物理/动画/Prefab/Play Mode。`
- FilesChanged:
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-qa-017.md`
  - `.ai-workflow/archive/2026-05/TASK-QA-017.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（沿用 `TASK-SDATA-006`、`TASK-SDATA-007`、`TASK-SCENE-018`、`TASK-EDITOR-005`、`TASK-EAPP-008` 的 `dotnet build AnsEngine.sln --no-restore --nologo -v minimal` 通过证据）
  - Test: `pass`（沿用 SceneData/Scene/Editor/Editor.App/App/Render 相关测试通过证据）
  - Smoke: `pass`（综合 `2.0` sample scene 运行、Transform-only object load/edit/save but not render、Inspector component groups、headless app smoke 与 Editor.App auto-exit 证据，并按人工验收通过收口）
  - Perf: `pass`（无新增逐帧 JSON 解析、双重 normalize、自动补组件轮询或 render side effect）
- ModuleAttributionCheck: pass
