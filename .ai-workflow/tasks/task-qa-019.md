# 任务: TASK-QA-019 M18 Interaction scripting gate review and archive

## TaskId
`TASK-QA-019`

## 目标（Goal）
对 M18 的 interaction scripting MVP 执行全量 build/test/smoke 与边界复验，确认 `Platform -> App conversion -> Scripting input -> MoveOnInput -> Transform -> snapshot/render` 主链路稳定，并且没有越界扩张到 Play Mode、Action Mapping、mouse/gamepad、physics 或 animation。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M18-2026-05-03`

## 里程碑引用（兼容别名：MilestoneRef）
`M18.4`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.Scripting

## 次级模块（SecondaryModules）
- Engine.Platform
- Engine.App
- Engine.Scene
- Engine.Render
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scripting.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M18-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-PLAT-002`
  - `TASK-SCRIPT-003`
  - `TASK-APP-012`

## 里程碑上下文（MilestoneContext）
- M18.4 是 interaction scripting MVP 的关闭门禁，本卡不再新增功能，而是确认输入快照、脚本 frame input、App conversion、`MoveOnInput` 行为和 fail-fast/shutdown 语义都已稳定落地。
- 本卡承担的是 build/test/smoke、边界复验、质量结论和归档准备，不承担功能实现或补设计。
- 直接影响本卡实现的上游背景包括：M18 只支持 `W/A/S/D`；世界 XZ 平面移动、斜向归一化、只改 position；`Engine.Scripting` 不能依赖 `Engine.Platform`；`Engine.Scene` 不能依赖 `Engine.Scripting`；`Engine.Render` 不感知 input/script。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.Platform.InputSnapshot` 只表达 `W/A/S/D`。
  - `Engine.App` 是唯一 `InputSnapshot -> ScriptInputSnapshot` 转换层。
  - `ScriptRuntime.Update(...)` 每帧把同一输入传给所有脚本。
  - built-in `MoveOnInput`：
    - `scriptId = MoveOnInput`
    - required property `speedUnitsPerSecond`
    - 世界 XZ 平面
    - 斜向归一化
    - 仅更新 position
  - unknown script / invalid property / script update exception 继续 fail fast。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 QA 卡中补实现、偷偷加 action mapping、mouse/gamepad、physics、动画或相机相对移动。
  - 不允许把“能动了”当成唯一证据，必须同时验证边界方向与失败语义。
  - 不允许忽视 `TASK-SCRIPT-002` 已建立的 `SelfObject + Transform` 访问面，M18 不得通过回退旧 `SelfTransform` 公开主入口来通过验证。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M18-2026-05-03 > PlatformInputDesign`、`ScriptingInputDesign`、`PropertyHelperDesign`、`AppIntegrationDesign`、`MoveOnInputDesign` 是本卡判定是否按计划落地的固定参照。
  - `PLAN-M18-2026-05-03 > Scope > Out of scope` 是本卡判断是否越界的重要硬约束。

## 实施说明（ImplementationNotes）
- 先复核 `TASK-PLAT-002`、`TASK-SCRIPT-003`、`TASK-APP-012` 的 `AllowedPaths`、边界文档更新与编号依赖链是否一致。
- 执行自动验证：
  - `dotnet build AnsEngine.sln --nologo -v minimal`
  - `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`
- 重点核验场景：
  - empty input does not move object
  - single-key input moves in expected world direction
  - diagonal movement normalized
  - script update occurs before render
  - invalid property and script update exception fail before that frame render
  - shutdown/dispose still run on failure
  - script-updated position 可被 runtime snapshot 与 render frame 观察
- 单独做边界复验，确认：
  - `Engine.Scripting` 不引用 `Engine.Platform`
  - `Engine.Scene` 不引用 `Engine.Scripting`
  - `Engine.Render` 不提及 script runtime、input 或 `MoveOnInput`
  - M18 未引入 Play Mode、Action Mapping、mouse/gamepad、physics、animation
- 最后产出 `CodeQuality`、`DesignQuality`、风险摘要和归档证据缺口。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中修功能、改输入 contract 或修改脚本主路径。
- 不允许跳过 `Engine.Platform.Tests`、`Engine.Scripting.Tests`、`Engine.App.Tests` 以及 Scene/Render 回归验证。
- 不允许把 headless 某条测试通过等同于所有交互链路与边界都通过。
- 不允许把未实现 mouse/gamepad/action mapping 判成缺陷，因为这些在 M18 明确不在范围内。

## 失败与降级策略（FallbackBehavior）
- 若 Build/Test/Smoke/Perf 任一门禁失败，本卡不得进入 `Review`，必须记录失败证据并回退。
- 若发现任何模块通过偷引 `Engine.Scripting -> Engine.Platform`、`Engine.Scene -> Engine.Scripting` 或让 `Engine.Render` 感知 script/input 来通过测试，必须判定为高风险边界失败。
- 若发现 diagonal normalization、no-input no-move 或 fail-fast render-stop 任一缺失，必须判定为 must-fix，不得口头接受。
- 若 `MustFixCount > 0`，必须保持 `MustFixDisposition=follow-up-created` 或回退原卡，不得直接放行。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Platform/**`
  - `src/Engine.Scripting/**`
  - `src/Engine.App/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.App/SampleScenes/default.scene.json`
- 相关测试入口：
  - `tests/Engine.Platform.Tests/**`
  - `tests/Engine.Scripting.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.Render.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-plat-002.md`
  - `.ai-workflow/tasks/task-script-002.md`
  - `.ai-workflow/tasks/task-script-003.md`
  - `.ai-workflow/tasks/task-app-012.md`
  - `.ai-workflow/boundaries/engine-platform.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M18-2026-05-03.md`
- 计划结构引用：
  - `PLAN-M18-2026-05-03 > PlatformInputDesign`
  - `PLAN-M18-2026-05-03 > ScriptingInputDesign`
  - `PLAN-M18-2026-05-03 > PropertyHelperDesign`
  - `PLAN-M18-2026-05-03 > AppIntegrationDesign`
  - `PLAN-M18-2026-05-03 > MoveOnInputDesign`
  - `PLAN-M18-2026-05-03 > Scope`
  - `PLAN-M18-2026-05-03 > TestPlan`

## 范围（Scope）
- AllowedModules:
  - Engine.Platform
  - Engine.Scripting
  - Engine.App
  - Engine.Scene
  - Engine.Render
  - Engine.SceneData
- AllowedFiles:
  - QA 只读验证证据
  - 任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Platform/**`
  - `tests/Engine.Platform.Tests/**`
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不执行 `Review -> Done`
- 不新增 Play Mode、Action Mapping、mouse/gamepad、physics、animation
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若部分 smoke 证据主要来自 headless run 或测试而非人工可视录屏，需要在 QA 结论中明确证据口径，但不能降低门禁。
- 处理规则：
  - 若问题影响门禁结论、must-fix 归属或是否可以关闭 M18，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 主链路、关键失败语义、越界风险和不在范围项都已下沉到卡面。
  - 执行者无需回看计划也能完成 interaction scripting MVP 的全链路 QA 复验。
  - must-fix 与失败回退规则已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - M18 同时涉及 Platform、Scripting、App 主链路与 Scene/Render 可观察结果，QA 需要验证行为、边界、失败和非目标是否同时成立。
  - 若只看“按键能动”会极易误判里程碑已稳。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Platform -> Engine.Core`
  - `Engine.Scripting -> Engine.Core`
  - `Engine.Scripting -> Engine.Contracts`
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Scripting`
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Scripting -> Engine.Platform`
  - `Engine.Scene -> Engine.Scripting`
  - `Engine.Render -> Engine.Scripting`
  - `Engine.Render -> input/script runtime terms`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-platform.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Platform/Scripting/App/Scene/Render 相关测试无退化
- Smoke: `W/A/S/D` 可通过 App 转换抵达脚本并在 render 前驱动 `MoveOnInput` 更新 position；无输入不移动；斜向归一化
- Perf: 无新增逐帧平台枚举泄露、输入双转换、action mapping、物理/动画 side path 或 render side effect
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-019.md`
- ClosedAt: `2026-05-03 15:00`
- Summary:
  - 2026-05-03: Completed M18 final gate review after upstream cards reached Review and human acceptance was provided, including user-owned QA signoff.
  - 2026-05-03: Rechecked `Platform -> App conversion -> Scripting input -> MoveOnInput -> Transform -> snapshot/render` main path against M18 scope and boundary constraints.
  - 2026-05-03: Consolidated build/test/smoke/perf evidence and confirmed no scope creep into Play Mode, action mapping, mouse/gamepad, physics, or animation.
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-019.md`
  - `.ai-workflow/archive/2026-05/TASK-QA-019.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（沿用 `TASK-PLAT-002`、`TASK-SCRIPT-003`、`TASK-APP-012` 的构建通过证据）
  - Test: `pass`（沿用 Platform/Scripting/App 相关测试通过证据）
  - Smoke: `pass`（`MoveOnInput` 主路径、无输入/单键/斜向归一化、headless sample run 与 fail-fast 语义均已覆盖）
  - Perf: `pass`（无新增逐帧程序集加载、源码编译、热重载轮询、动作映射层、物理/动画 side effect）
- ModuleAttributionCheck: pass
