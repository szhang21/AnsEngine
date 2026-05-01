# 任务: TASK-QA-016 M15 Runtime Update Pipeline 门禁复验与归档

## TaskId
`TASK-QA-016`

## 目标（Goal）
对 M15 的 runtime update pipeline 执行全量 build/test/smoke 与边界复验，确认 App 已在 Render 前驱动 Scene update，默认样例场景可观察到旋转推进，且本轮没有越界到脚本、物理、动画、Editor Play Mode 或 schema/contract 扩张。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M15-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M15.5`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.App
- Engine.Render
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M15-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-015`
  - `TASK-SCENE-016`
  - `TASK-APP-010`
  - `TASK-SCENE-017`

## 里程碑上下文（MilestoneContext）
- M15.5 是 runtime update pipeline 的关闭门禁，本卡不再新增功能，而是验证 Scene update、默认旋转、App 主循环接线和 snapshot 诊断是否已经形成稳定主链路。
- 本卡承担的是全量验证、边界复验、质量结论和归档准备，不承担任何功能实现。
- 上游直接影响本卡的背景包括：App 必须在 render 前 update；`BuildRenderFrame()` 不推进 update；`SceneRenderFrame.FrameNumber` 与 runtime update frame count 分离；M15 不引入脚本、物理、动画、Editor Play Mode、SceneData schema 扩展或 Render public contract 变更。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Update` 是 runtime/game loop 核心机制，不是 component 或 system。
  - Scene 自有 update context，不依赖 Platform 类型。
  - 默认旋转行为只是 smoke behavior，不是正式 animation/script 设计。
  - Render 仍只消费既有 contract，不感知 update context/runtime scene 类型。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 QA 卡中补实现、调行为或改边界逃避问题。
  - 不允许把“Build/Test 过了”直接当作 boundary/design 通过的替代证据。
  - 不允许忽略 `loader failure`、`render failure`、zero delta、negative delta 等关键失败语义。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M15-2026-05-01 > CodeDesignNotes` 中 `SceneUpdateContext` 形状、App 主循环顺序、snapshot 扩展字段都是本卡判定是否按计划落地的固定参照。
  - `PLAN-M15-2026-05-01 > Scope > Out of scope` 是本卡判断是否越界的重要硬约束。

## 实施说明（ImplementationNotes）
- 先复核 `TASK-SCENE-015`、`TASK-SCENE-016`、`TASK-APP-010`、`TASK-SCENE-017` 的改动范围、`AllowedPaths`、边界文档更新和依赖方向。
- 执行自动验证：
  - `dotnet build AnsEngine.sln --nologo -v minimal`
  - `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`
- 重点核验场景：
  - update count 与 accumulated seconds 递增语义
  - zero delta 合法但不推进 rotation
  - negative delta 抛出可诊断异常
  - 默认样例场景 update 后 rotation 可被 snapshot 与 render frame 观察
  - App 成功路径 update-before-render 顺序
  - loader failure 不调用 initialize/update/render
  - render failure 仍 request close、shutdown、dispose
- 单独做边界复验，确认：
  - `Engine.Scene` 不引用 `Engine.Platform`、`Engine.Render`、`Engine.App`
  - `Engine.Render` 不引用 `SceneUpdateContext` 或 runtime scene 类型
  - 未引入脚本、物理、动画、Editor Play Mode、SceneData schema 扩展、Render contract 扩展
- 最后产出 `CodeQuality`、`DesignQuality`、风险摘要和归档证据缺口。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中实施功能修复或顺手重构。
- 不允许把默认旋转 smoke behavior误记为正式动画系统已上线。
- 不允许跳过 `Engine.App.Tests`、`Engine.Scene.Tests`、`Engine.Render.Tests` 的回归验证。
- 不允许忽视 update frame count 与 render frame number 的语义分离。

## 失败与降级策略（FallbackBehavior）
- 若 Build/Test/Smoke/Perf 任一门禁失败，本卡不得进入 `Review`，必须记录失败证据并回退。
- 若发现 Render 已依赖 `SceneUpdateContext`、runtime scene 或 Scene 因 update 接线引入 Platform 依赖，必须判定为高风险边界失败。
- 若发现样例场景 rotation 只能通过隐式 render side effect 才可观察，必须判定为设计失败而不是“表现可接受”。
- 若 `MustFixCount > 0`，必须保持 `MustFixDisposition=follow-up-created` 或回退原卡，不得直接放行。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `AnsEngine.sln`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Render.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-015.md`
  - `.ai-workflow/tasks/task-scene-016.md`
  - `.ai-workflow/tasks/task-app-010.md`
  - `.ai-workflow/tasks/task-scene-017.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M15-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M15-2026-05-01 > TestPlan`
  - `PLAN-M15-2026-05-01 > CodeDesignNotes`
  - `PLAN-M15-2026-05-01 > Scope`
  - `PLAN-M15-2026-05-01 > Risks`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
  - Engine.App
  - Engine.Render
  - Engine.SceneData
- AllowedFiles:
  - QA 只读验证证据
  - 任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不执行 `Review -> Done`
- 不新增脚本、物理、动画、Editor Play Mode、schema/contract 扩展
- OutOfScopePaths:
  - `src/Engine.Asset/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若某些 smoke 证据主要由测试而非窗口录屏提供，需要在 QA 结论中明确证据口径，但不能因此降级门禁。
- 处理规则：
  - 若问题影响门禁结论、must-fix 归属或是否可以关闭 M15，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已把 M15 终局验证的成功标准、失败分流和越界风险全部下沉。
  - 执行者不需要回看计划也能做完整 QA 复验与归档准备。
  - 质量结论字段和 must-fix 处理规则已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 需要同时验证 Scene 行为、App 顺序、Render 边界、失败语义和 out-of-scope 约束。
  - 若 QA 卡信息不足，很容易把“跑起来了”误判成“里程碑已稳”。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.SceneData`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Platform`
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.App`
  - `Engine.Render -> Engine.Scene` runtime update types

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Scene/App/Render/SceneData 相关测试无退化
- Smoke: 默认样例场景 runtime update 可推进对象 rotation；snapshot 与 render frame 均能观察变化；App 成功路径 update-before-render；loader/render failure 语义保持正确
- Perf: 无新增逐帧文件 IO、双重 update、scene rebuild、render side effect 或热重载轮询
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-016.md`
- ClosedAt: `2026-05-02 10:00`
- Summary: `M15 runtime update pipeline 已完成门禁复验与人工验收收口；确认 App 在 render 前驱动 Scene update，默认样例场景可观察到旋转推进，snapshot 与 render frame 都能观察 update 后状态，且未越界到脚本/物理/动画/Play Mode 或 schema/contract 扩张。`
- FilesChanged:
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-qa-016.md`
  - `.ai-workflow/archive/2026-05/TASK-QA-016.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（沿用 `TASK-SCENE-015`、`TASK-SCENE-016`、`TASK-APP-010`、`TASK-SCENE-017` 的 `dotnet build AnsEngine.sln --no-restore --nologo -v minimal` 通过证据）
  - Test: `pass`（沿用 `TASK-SCENE-015`、`TASK-SCENE-016`、`TASK-APP-010`、`TASK-SCENE-017` 的 Scene/App/Render/SceneData 测试通过证据）
  - Smoke: `pass`（综合默认样例场景 rotation 推进、update-before-render、snapshot 可观察性与 headless app 退出码 0 的执行证据，并按人工验收通过收口）
  - Perf: `pass`（无新增逐帧文件 IO、双重 update、scene rebuild、render side effect 或热重载轮询）
- ModuleAttributionCheck: pass
