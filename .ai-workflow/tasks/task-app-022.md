# 任务: TASK-APP-022 M23 App physics orchestrator state sync

## TaskId
`TASK-APP-022`

## 目标（Goal）
将 `Engine.App` 的 runtime physics orchestrator 从纯查询 `ResolveKinematicMove(...)` 切到 mutating `ApplyKinematicMove(...)`，确保 Scene 写回与 PhysicsWorld 内部 body state 在连续帧之间保持同步。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M23-2026-05-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M23.2`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Physics
- Engine.Scene
- Engine.Scripting

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M23-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-PHYS-004`

## 里程碑上下文（MilestoneContext）
- M23.2 是 M23 的 runtime 接线卡；M20 已有 `Script -> Physics resolve -> Scene writeback -> Render`，但 PhysicsWorld 没被更新，导致后续帧以旧 body transform 继续计算。
- 本卡在 `TASK-PHYS-004` 提供 `ApplyKinematicMove(...)` 后，调整 App orchestrator 的调用点和测试证据。
- M23.3 runtime smoke and boundary verification 中与 App 主路径相关的连续帧验证并入本卡执行，QA 总门禁由 `TASK-QA-024` 收口。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 主循环仍保持 `SceneRuntime.Update -> ScriptRuntime.Update -> Physics state-sync/writeback -> Render`。
  - App 是唯一 runtime bridge：从 Scene snapshot 读取 desired transform，调用 Physics API，再把 resolved transform 写回 Scene。
  - Physics 不直接写 Scene，Scene 不依赖 Physics，Render 仍只观察 Scene 输出。
  - App 必须调用 `PhysicsWorld.ApplyKinematicMove(...)`，不再在 M23 主路径调用纯查询 `ResolveKinematicMove(...)`。
  - App 写回 Scene 的 transform 必须与 Physics apply 返回的 `ResolvedTransform` 一致。
  - 若 apply 成功但 Scene writeback 失败，仍按现有策略在 render 前返回 deterministic failure；M23 不做 rollback，但必须在风险说明中记录。
- 本卡执行时不得推翻的既定取舍：
  - 不允许改 SceneData schema。
  - 不允许让 Physics 直接持有 Scene runtime object。
  - 不允许新增 gravity、velocity、solver、Play Mode 或 Editor UI。
  - 不允许为了同步状态绕过 Scene writeback contract。
- 计划结构引用：
  - `PLAN-M23-2026-05-13 > Runtime Flow`
  - `PLAN-M23-2026-05-13 > M23.2`
  - `PLAN-M23-2026-05-13 > M23.3`
  - `PLAN-M23-2026-05-13 > Failure And Fallback`
  - `PLAN-M23-2026-05-13 > TestPlan`

## 实施说明（ImplementationNotes）
- 先更新 `RuntimePhysicsOrchestrator` 或等价 App helper，使 kinematic path 调用 `ApplyKinematicMove(...)` 并继续使用返回的 `ResolvedTransform` 写回 Scene。
- 保持无 physics body / 无 collider path 的既有行为，不让 physics-free scene 被错误阻挡或丢失脚本移动。
- 补 App tests，至少覆盖：
  - orchestrator 调用 mutating physics path 并把同一个 resolved transform 写回 Scene。
  - 连续两帧中第二帧的 PhysicsWorld body transform 已是第一帧 apply 后的位置，不再使用 stale transform。
  - 现有 collision smoke 仍通过，script-driven mover 不穿入 static collider。
  - writeback failure 仍在 render 前暴露为 deterministic failure。
- 更新 `.ai-workflow/boundaries/engine-app.md`，声明 App runtime physics orchestrator 在 M23 使用 mutating state-sync API 和 no-rollback residual risk。

## 设计约束（DesignConstraints）
- 不允许把状态同步逻辑散落回主循环巨石；应继续收敛在 runtime physics orchestrator/helper 内。
- 不允许修改 `Engine.Physics` 源码；若发现 Physics API 不足，应回退 `TASK-PHYS-004`，不得在 App 侧做 shadow state。
- 不允许修改 Scene 内部集合或绕过公开 writeback contract。
- 不允许让 Render 或 SceneData 感知 physics state sync。

## 失败与降级策略（FallbackBehavior）
- 若 `ApplyKinematicMove(...)` 返回 failure，沿用现有 App failure/diagnostic path，不渲染错误状态。
- 若 apply success 但 Scene writeback failure，保留计划规定的 no rollback 语义：App 返回 failure before render，并在执行交付风险中标记 residual risk。
- 若连续帧测试需要 fixture 扩展，优先在 App tests 内构造最小 fake Scene/Physics bridge，不扩大生产 API。
- 若必须改 SceneData schema 或 Scene->Physics 依赖才能完成，必须停工回退。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/RuntimePhysicsOrchestrator.cs`
  - `src/Engine.App/**`
- 相关测试入口：
  - `tests/Engine.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-app-021.md`
  - `.ai-workflow/tasks/task-phys-004.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M23-2026-05-13.md`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - runtime physics orchestrator/helper
  - App tests
  - Engine.App boundary documentation
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Physics mutating API
- 不修改 SceneData schema
- 不实现 gravity / solver / velocity / Editor UI / Play Mode
- 不让 Scene、Render、SceneData 感知 PhysicsWorld 内部状态
- OutOfScopePaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Render/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 连续帧测试可选择直接测试 orchestrator，也可经由更高层 App runtime smoke 证明；必须能观察 stale transform 被消除。
- 处理规则：
  - 若问题影响 App/Physics/Scene ownership 或 render 前 failure 语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 调用 API、帧顺序、所有权、失败语义和连续帧验证目标均已明确。
  - 依赖 `TASK-PHYS-004` 已显式声明，避免 App 执行者提前造 shadow sync。
  - M23.3 中 App runtime smoke 已合并进本卡，QA 卡只做总门禁复验。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 本卡改动点小，但位于 Script、Physics、Scene writeback、Render 的主链路汇合处，顺序和失败语义不能漂移。
  - 连续帧 stale-state bug 难靠单帧 smoke 发现，必须在卡面钉死测试。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Scripting`
  - `Engine.App -> Engine.Physics`
- ForbiddenDependsOn:
  - `Engine.App` 直接依赖 Scene runtime object/component 内部集合
  - `Engine.App -> Engine.Editor`
  - `Engine.App -> Engine.Editor.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 mutating path、Scene writeback、连续帧 state sync、collision smoke、writeback failure
- Smoke: headless/runtime smoke 能证明 script desired transform -> Physics apply -> Scene resolved transform -> next frame Physics uses updated body transform -> render observes Scene
- Perf: 不引入逐帧重建 PhysicsWorld、额外 SceneData IO、Render side effect 或跨模块内部集合访问

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)，必须包含 apply success + Scene writeback failure 的 no-rollback residual risk 结论
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Todo

## 完成度（Completion）
`0`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`

## 归档（Archive）
- ArchivePath:
- ClosedAt:
- Summary:
- FilesChanged:
- ValidationEvidence:
- ModuleAttributionCheck:
