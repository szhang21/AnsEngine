# 任务: TASK-QA-018 M17 Scripting Foundation gate review and archive

## TaskId
`TASK-QA-018`

## 目标（Goal）
对 M17 的 scripting foundation 执行全量 build/test/smoke 与边界复验，确认 Script component、script runtime、Scene bridge、App `RotateSelf` 主链路和 Editor preserve 行为都已稳定落地。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M17-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M17.5`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.Scripting

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Scene
- Engine.App
- Engine.Editor
- Engine.Render

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scripting.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M17-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCRIPT-001`
  - `TASK-SCRIPT-002`
  - `TASK-SDATA-008`
  - `TASK-SCENE-019`
  - `TASK-APP-011`

## 里程碑上下文（MilestoneContext）
- M17.5 是 scripting foundation 的关闭门禁，本卡不再新增功能，而是确认从 SceneData `Script` component 到 App 中 `RotateSelf` 执行的整条脚本主链路已经稳定，并且 Editor 在不编辑脚本的前提下能完整 preserve 组件。
- 本卡承担的是 build/test/smoke、边界复验、质量结论和归档准备，不承担业务实现。
- 直接影响本卡实现的上游背景包括：M17 只做内置注册表脚本 foundation；unknown script、bad property、script exception 都 fail fast；Editor M17 不提供 Script UI，只要求 open/save/save-as preserve；M17 不引入外部 DLL、源码编译、热重载、Play Mode、物理或动画。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 新建 `Engine.Scripting`，依赖方向固定为 `Engine.Scripting -> Engine.Scene`。
  - `Script` component 使用 `scriptId + properties`。
  - `properties` 只支持 number / bool / string。
  - 多 Script component 允许并按 scene 文件顺序执行。
  - missing script / bad property / script exception fail fast。
  - M15 默认旋转 smoke behavior 由 `RotateSelf` 替代。
  - Editor M17 只保留 Script component，不编辑 Script component。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 QA 卡中补实现、偷偷恢复 M15 默认旋转或引入外部 DLL/源码编译。
  - 不允许把“窗口里转了”当作 SceneData/Scene/Scripting/App/Editor 全链路通过的唯一证据。
  - 不允许忽视 Editor save/reload preserve Script component 顺序与 properties 的要求。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M17-2026-05-02 > ScriptingDesign`、`SceneDataDesign`、`SceneBridgeDesign`、`AppIntegrationDesign`、`EditorPreservationDesign` 是本卡判定是否按计划落地的固定参照。
  - `PLAN-M17-2026-05-02 > Scope > Out of scope` 是本卡判断是否越界的重要硬约束。

## 实施说明（ImplementationNotes）
- 先复核 `TASK-SCRIPT-001`、`TASK-SDATA-008`、`TASK-SCENE-019`、`TASK-APP-011` 的改动范围、`AllowedPaths` 和边界文档更新。
- 执行自动验证：
  - `dotnet build AnsEngine.sln --nologo -v minimal`
  - `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`
- 重点核验场景：
  - valid `RotateSelf` scene runs and updates before render
  - unknown script id fails run cleanly
  - script exception fails run cleanly
  - script-updated Transform 可被 snapshot 与 render frame 观察
  - Editor open/save/save-as preserves Script component、properties 和顺序
  - Inspector 不要求 Script editing controls
- 单独做边界复验，确认：
  - `Engine.Scripting` 不引用 `Render/Editor/Editor.App`
  - `Engine.Scene` 不引用 `Engine.Scripting`
  - `Engine.Render` 不提及 Script component 或 `ScriptRuntime`
  - M17 未引入外部 DLL 加载、源码编译、热重载、Play Mode、物理或动画
- 最后产出 `CodeQuality`、`DesignQuality`、风险摘要和归档证据缺口。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中修功能、改脚本 API 或移动模块边界。
- 不允许跳过 `Engine.Scripting.Tests`、`Engine.SceneData.Tests`、`Engine.Scene.Tests`、`Engine.App.Tests` 和 Editor preserve 回归验证。
- 不允许忽视 `RotateSelf` 已接管可见 runtime 行为这一事实。
- 不允许把 Editor 无 Script UI 误判为缺陷，因为这在 M17 明确不在范围内。

## 失败与降级策略（FallbackBehavior）
- 若 Build/Test/Smoke/Perf 任一门禁失败，本卡不得进入 `Review`，必须记录失败证据并回退。
- 若发现任何模块通过偷引反向依赖、恢复外部 DLL/源码编译或保留 M15 默认旋转来通过测试，必须判定为高风险边界失败。
- 若发现 Editor save 丢失 Script component、properties 或顺序，必须判定为必须修复，不得口头接受。
- 若 `MustFixCount > 0`，必须保持 `MustFixDisposition=follow-up-created` 或回退原卡，不得直接放行。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scripting/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Editor/**`
  - `src/Engine.App/SampleScenes/default.scene.json`
- 相关测试入口：
  - `tests/Engine.Scripting.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Editor.Tests/**`
  - `tests/Engine.Render.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-script-001.md`
  - `.ai-workflow/tasks/task-sdata-008.md`
  - `.ai-workflow/tasks/task-scene-019.md`
  - `.ai-workflow/tasks/task-app-011.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M17-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M17-2026-05-02 > ScriptingDesign`
  - `PLAN-M17-2026-05-02 > SceneDataDesign`
  - `PLAN-M17-2026-05-02 > SceneBridgeDesign`
  - `PLAN-M17-2026-05-02 > AppIntegrationDesign`
  - `PLAN-M17-2026-05-02 > EditorPreservationDesign`
  - `PLAN-M17-2026-05-02 > TestPlan`
  - `PLAN-M17-2026-05-02 > Scope`

## 范围（Scope）
- AllowedModules:
  - Engine.Scripting
  - Engine.SceneData
  - Engine.Scene
  - Engine.App
  - Engine.Editor
  - Engine.Render
- AllowedFiles:
  - QA 只读验证证据
  - 任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不执行 `Review -> Done`
- 不新增外部 DLL、源码编译、热重载、Play Mode、物理、动画
- OutOfScopePaths:
  - `src/Engine.Asset/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若部分 smoke 证据主要来自 headless run 或测试而非人工窗口录屏，需要在 QA 结论中明确证据口径，但不能降格门禁。
- 处理规则：
  - 若问题影响门禁结论、must-fix 归属或是否可以关闭 M17，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已把 M17 终局验证的成功标准、越界风险、Editor preserve 义务和 fail-fast 语义都下沉了。
  - 执行者无需回看计划也能完成全链路 QA 复验。
  - must-fix 和失败回退规则已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - M17 涉及新模块、新 schema、新 runtime bridge、新 app flow，QA 需要同时验证 runtime、data、boundary 和 preserve 行为，多入口且高风险。
  - 若 QA 卡写短，很容易把“RotateSelf 旋转了”误判成“脚本基础设施已稳”。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scripting -> Engine.Scene`
  - `Engine.Scripting -> Engine.Core`
  - `Engine.Scripting -> Engine.Contracts`
  - `Engine.SceneData -> Engine.Contracts`
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Scripting`
  - `Engine.Editor -> Engine.SceneData`
  - `Engine.Editor -> Engine.Contracts`
  - `Engine.Editor -> Engine.Core`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Scripting`
  - `Engine.Render -> Engine.Scripting`
  - `Engine.Render -> Script component/runtime terms`
  - `Engine.Scripting -> Engine.Editor`
  - `Engine.Scripting -> Engine.Editor.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Scripting/SceneData/Scene/App/Editor/Render 相关测试无退化
- Smoke: valid `RotateSelf` scene 可运行并在 render 前更新；unknown script scene clean fail；Editor open/save/save-as preserves Script component
- Perf: 无新增逐帧程序集加载、源码编译、热重载轮询、任意对象查询或 render side effect
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-018.md`
- ClosedAt: `2026-05-03 13:45`
- Summary:
  - 2026-05-03: Completed M17 final gate review after upstream cards reached Review and human acceptance was provided.
  - 2026-05-03: Rechecked scripting runtime lifecycle, SceneData Script schema, Scene self-transform bridge, App `RotateSelf` integration, and Editor preserve scope against M17 plan constraints.
  - 2026-05-03: Consolidated cross-module build/test/smoke evidence and confirmed no out-of-scope expansion into external DLL loading, source compilation, hot reload, Play Mode, physics, or animation.
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-018.md`
  - `.ai-workflow/archive/2026-05/TASK-QA-018.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（沿用 `TASK-SCRIPT-001`、`TASK-SDATA-008`、`TASK-SCENE-019`、`TASK-APP-011`、`TASK-SCRIPT-002` 的构建通过证据）
  - Test: `pass`（沿用 Scripting/SceneData/Scene/App 相关测试通过证据；`TASK-SCRIPT-002` 额外覆盖 Scripting/App 回归）
  - Smoke: `pass`（`RotateSelf` 在 render 前更新；unknown script clean fail；Editor 对 Script component 维持 preserve 范围）
  - Perf: `pass`（无新增逐帧程序集加载、源码编译、热重载轮询、任意对象查询或 render side effect）
- ModuleAttributionCheck: pass
