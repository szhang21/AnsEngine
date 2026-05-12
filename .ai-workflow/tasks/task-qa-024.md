# 任务: TASK-QA-024 M23 Physics state sync gate review and archive

## TaskId
`TASK-QA-024`

## 目标（Goal）
对 M23 Kinematic Physics State Synchronization 执行门禁复验，确认 Physics mutating API、App 连续帧状态同步、边界依赖与 no-goals 均符合计划，并准备归档证据。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M23-2026-05-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M23.4`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Physics
- Engine.Scene
- Engine.Scripting
- Engine.Render
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M23-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-PHYS-004`
  - `TASK-APP-022`

## 里程碑上下文（MilestoneContext）
- M23.4 是 M23 关闭门禁，不新增功能，只验证 PhysicsWorld state 与 Scene Transform 在 script-driven kinematic path 上不再跨帧脱节。
- 本卡覆盖 M23.3 的 runtime smoke and boundary verification 总收口：Physics API shape、App orchestrator 调用路径、连续帧行为、边界文档和 no-goals。
- 上游直接影响本卡的背景包括：M23 不改变 SceneData schema，不引入动态物理模拟，不改变 Physics/Scene ownership，只让 App 调用 mutating Physics API 后继续写回 Scene。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `ResolveKinematicMove(...)` 必须保持 non-mutating。
  - `ApplyKinematicMove(...)` 必须更新 Dynamic body transform/AABB，并返回与 resolve 一致的 result shape。
  - App orchestrator 必须调用 `ApplyKinematicMove(...)`，并把 `ResolvedTransform` 写回 Scene。
  - 连续帧不应再以 stale PhysicsWorld body transform 做 kinematic resolve。
  - Render、SceneData 不应感知 physics state sync。
  - M23 对 apply success + Scene writeback failure 不做 rollback，QA 必须记录 residual risk 和现有 fail-before-render 证据。
- 本卡执行时不得推翻的既定取舍：
  - QA 不实现功能修复。
  - 不允许以“build/test 通过”放行边界依赖漂移。
  - 不允许新增 gravity、velocity、solver、trigger、CCD、Editor UI 或 SceneData schema。
  - 不允许 QA 自行关单到 Done。
- 计划结构引用：
  - `PLAN-M23-2026-05-13 > M23.3`
  - `PLAN-M23-2026-05-13 > M23.4`
  - `PLAN-M23-2026-05-13 > TestPlan`
  - `PLAN-M23-2026-05-13 > DispatchConstraints`

## 实施说明（ImplementationNotes）
- 执行 build/test 门禁并记录 warning：
  - `dotnet build AnsEngine.sln --nologo -v minimal`
  - `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`
- 执行 Physics API/behavior 检查：
  - `ResolveKinematicMove(...)` 测试证明 non-mutating。
  - `ApplyKinematicMove(...)` 测试证明 no-hit 更新、blocked resolved 更新、AABB 更新、static body failure 和 malformed diagnostics。
- 执行 App runtime 检查：
  - `RuntimePhysicsOrchestrator` 或等价 helper 使用 mutating apply path。
  - Scene writeback 使用 apply result 的 `ResolvedTransform`。
  - 连续帧测试或 smoke 证明第二帧 PhysicsWorld 使用更新后的 body transform。
  - writeback failure 仍 before render，且 no rollback residual risk 已记录。
- 执行边界检查：
  - `Engine.Physics` 不依赖任何 Engine 模块。
  - `Engine.App` 仍是唯一 runtime bridge。
  - `Engine.Scene` 不依赖 `Engine.Physics`。
  - `Engine.Render` 与 `Engine.SceneData` 不感知 physics state sync。
  - `.ai-workflow/boundaries/engine-physics.md` 与 `.ai-workflow/boundaries/engine-app.md` 已同步变更记录。
- 输出 `CodeQuality`、`DesignQuality`、风险摘要和归档准备结论。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中修改业务源码或补实现。
- 不允许接受 `ResolveKinematicMove(...)` 变成 mutating 作为实现捷径。
- 不允许忽略连续帧 stale-state 验证。
- 不允许把 no-rollback residual risk 漏记为已解决。

## 失败与降级策略（FallbackBehavior）
- Build/Test 任一失败，本卡必须回退，不能进入 Review。
- 若发现 MustFix（Resolve 变 mutating、Apply 不更新 AABB、App 未调用 Apply、连续帧 stale state、依赖漂移、SceneData schema 改动），必须要求回退原卡或转 follow-up，不得口头放行。
- 若本地 runtime smoke 环境受限，必须记录替代证据和人工补验缺口，由 Human 决定是否接受。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Physics/**`
  - `src/Engine.App/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`
- 相关测试入口：
  - `tests/Engine.Physics.Tests/**`
  - `tests/Engine.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-phys-004.md`
  - `.ai-workflow/tasks/task-app-022.md`
  - `.ai-workflow/tasks/task-phys-003.md`
  - `.ai-workflow/tasks/task-app-021.md`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M23-2026-05-13.md`

## 范围（Scope）
- AllowedModules:
  - Engine.App
  - Engine.Physics
  - Engine.Scene
  - Engine.Scripting
  - Engine.Render
  - Engine.SceneData
- AllowedFiles:
  - QA 只读验证证据、任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Scripting/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不修改 public API
- 不执行关单或 Done 流转
- 不引入 gravity / solver / CCD / Editor UI / SceneData schema changes
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若 runtime smoke 不能在本地跑通，QA 必须列明缺口和替代证据。
- 处理规则：
  - 任何影响 gate 结论的问题必须回退或转卡，不得自行脑补为 pass。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确 build/test、Physics API、App runtime、连续帧、依赖边界和边界文档检查口径。
  - QA 不需要回看计划全文即可判定 M23 是否可进入 Review。
  - MustFix 与 residual risk 处理已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - QA 横跨 Physics 与 App 主路径，还要确认 Scene/Render/SceneData 没有被状态同步扩散污染。
  - M23 的主要风险是单帧测试看似通过但连续帧仍使用 stale PhysicsWorld state。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Physics`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Scripting`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.SceneData`
- ForbiddenDependsOn:
  - `Engine.Physics -> Engine.Scene`
  - `Engine.Physics -> Engine.App`
  - `Engine.Scene -> Engine.Physics`
  - `Engine.Render -> Engine.Physics`
  - `Engine.SceneData -> Engine.Physics`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过
- Smoke: Resolve non-mutating、Apply mutating、App apply/writeback、连续帧无 stale PhysicsWorld state、Render/SceneData 无感知均通过
- Perf: 无明显 runtime path 退化；不引入逐帧全量 world rebuild、额外 SceneData IO 或 solver path
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
- Risk list (high|medium|low)，必须包含 no-rollback residual risk 复核
- Archive readiness notes

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
