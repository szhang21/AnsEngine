# 任务: TASK-PHYS-004 M23 Physics mutating kinematic move API

## TaskId
`TASK-PHYS-004`

## 目标（Goal）
在 `Engine.Physics` 中新增显式 mutating kinematic move API，让 App 能把脚本驱动的 desired transform 应用回 `PhysicsWorld` 内部 body state，同时保留 `ResolveKinematicMove(...)` 的纯查询语义。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M23-2026-05-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M23.1`

## 执行代理（ExecutionAgent）
Exec-Physics

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Physics

## 次级模块（SecondaryModules）
- none

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-physics.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M23-G1`
- CanRunParallel: `false`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M23 修复 M20 遗留的 physics state 与 Scene Transform 脱节问题：上一帧 Scene 已写回 resolved transform，但下一帧 PhysicsWorld 仍以旧 body transform 做 kinematic resolve。
- M23.1 是本里程碑的底座卡；没有 mutating API，App 只能继续调用纯查询 `ResolveKinematicMove(...)`，无法让 PhysicsWorld 内部状态进入下一帧。
- 本卡只负责 Physics core 的 API、状态更新和测试，不负责 App orchestrator 接线、Scene writeback 或 Render 可见路径。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 保留 `PhysicsWorld.ResolveKinematicMove(string bodyId, PhysicsTransform desiredTransform)` 为 non-mutating query。
  - 新增 `PhysicsWorld.ApplyKinematicMove(string bodyId, PhysicsTransform desiredTransform)` 为 mutating API。
  - `ApplyKinematicMove(...)` 必须复用与 `ResolveKinematicMove(...)` 相同的校验、失败语义、X/Y/Z conservative static AABB resolve 和 result shape。
  - 成功 apply 后必须更新目标 Dynamic body 的 Transform 与 AABB；若被 static AABB 阻挡，则更新到 resolved transform，而不是 desired transform。
  - static body movement 必须失败，不能被 apply 移动。
  - rotation 继续不参与 AABB 计算。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 `ResolveKinematicMove(...)` 改成有副作用。
  - 不允许暴露可变 body 引用或让外部系统直接修改 PhysicsWorld 内部列表。
  - 不允许引入 gravity、velocity、force、impulse、dynamic solver、dynamic-dynamic collision、trigger、CCD、friction 或 bounce。
  - 不允许让 `Engine.Physics` 依赖任何 Engine 模块。
- 计划结构引用：
  - `PLAN-M23-2026-05-13 > API Shape`
  - `PLAN-M23-2026-05-13 > M23.1`
  - `PLAN-M23-2026-05-13 > Failure And Fallback`
  - `PLAN-M23-2026-05-13 > TestPlan`

## 实施说明（ImplementationNotes）
- 先抽取 resolve 共用 helper，统一承载 body id 校验、desired transform 校验、static body 拒绝、current body/AABB 读取和 X/Y/Z conservative static AABB resolve。
- 再新增 `ApplyKinematicMove(...)`，在 helper 返回 success result 后替换目标 Dynamic body snapshot/list entry，使内部 Transform 与 AABB 都与 `ResolvedTransform` 对齐。
- 保证 `ResolveKinematicMove(...)` 仍只返回 result，不修改 `PhysicsWorld` 内部状态。
- 补 Physics tests，至少覆盖：
  - `ResolveKinematicMove(...)` 多次调用保持 world state 不变。
  - `ApplyKinematicMove(...)` 在无碰撞时更新 Dynamic body transform。
  - `ApplyKinematicMove(...)` 在碰撞阻挡时更新到 resolved transform。
  - apply 后 query/AABB 观察到新位置。
  - static body apply 失败且不移动。
  - missing/blank/non-finite/malformed input 的诊断与既有 resolve 语义稳定。

## 设计约束（DesignConstraints）
- 不允许在本卡修改 App、Scene、Scripting、Render 或 SceneData 源码。
- 不允许把 `ApplyKinematicMove(...)` 实现成 App 层缓存或 Scene writeback 的别名。
- 不允许把 internal body collection 变成外部可变集合。
- 不允许把 M23 扩大成动态物理模拟。

## 失败与降级策略（FallbackBehavior）
- 若内部 snapshot 是 immutable record/class，可通过替换 `mBodies` 中目标 entry 完成状态更新；不得为省事暴露 mutable body ref。
- 若公共类型命名需按仓库风格微调，允许等价命名，但 public API 必须清晰区分 resolve/query 与 apply/mutate。
- 若发现更新 AABB 需要共享更多 helper，应优先抽取 Physics 内部私有 helper；不得引入 Engine 外部依赖。
- 若实现中必须修改 App 才能测试 apply，停止并回退；App 接线由 `TASK-APP-022` 承担。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Physics/PhysicsWorld.cs`
- 相关测试入口：
  - `tests/Engine.Physics.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-phys-003.md`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M23-2026-05-13.md`

## 范围（Scope）
- AllowedModules:
  - Engine.Physics
- AllowedFiles:
  - PhysicsWorld kinematic resolve/apply API
  - Physics tests
  - Engine.Physics boundary documentation
- AllowedPaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 App orchestrator
- 不修改 Scene writeback API
- 不修改 SceneData schema
- 不实现 gravity / velocity / acceleration / force / impulse / solver / trigger / layer / material / CCD
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Scene/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Render/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 内部 helper 命名与 test fixture 组织可按现有 Physics 风格微调。
- 处理规则：
  - 若问题影响 `Resolve` 纯查询语义、`Apply` 状态更新语义或 Physics 零 Engine 依赖，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - API 形状、状态更新点、失败语义、非目标和测试矩阵均已落卡。
  - 执行者无需回看计划全文即可知道本卡不是 solver 卡，也不是 App 接线卡。
  - M23 的关键风险“Resolve 被误改成 mutating”已明确禁止。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 本卡虽局限在 Physics，但会改变 public API 与 world state 语义，是 M23 后续 App 状态同步的根依赖。
  - 写短会让执行者容易误把既有 `Resolve` 改成 mutating，或漏掉 blocked resolved transform/AABB 更新。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - .NET 标准库
  - `System.Numerics`
- ForbiddenDependsOn:
  - `Engine.Physics -> Engine.Scene`
  - `Engine.Physics -> Engine.App`
  - `Engine.Physics -> Engine.Render`
  - `Engine.Physics -> Engine.Scripting`
  - `Engine.Physics -> Engine.SceneData`
  - `Engine.Physics -> Engine.Core`
  - `Engine.Physics -> Engine.Contracts`

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
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Physics.Tests/Engine.Physics.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 Resolve non-mutating、Apply mutate、blocked resolved transform、AABB 更新、static body failure、malformed diagnostics
- Smoke: 连续两次 kinematic apply/query 证明第二次从更新后的 PhysicsWorld body transform 出发
- Perf: 不引入 solver、CCD、逐帧全量重建 world 或任何 Engine 模块依赖

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
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-PHYS-004.md`
- ClosedAt: `2026-05-13`
- Summary:
  - Kept `ResolveKinematicMove(...)` non-mutating.
  - Added `ApplyKinematicMove(...)` as the explicit mutating kinematic move API.
  - Successful apply replaces the dynamic body snapshot so Transform and AABB match the resolved transform.
  - Static body, missing id, blank id, and malformed transform diagnostics reuse existing resolve semantics.
- FilesChanged:
  - `src/Engine.Physics/PhysicsWorld.cs`
  - `tests/Engine.Physics.Tests/PhysicsFoundationTests.cs`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/tasks/task-phys-004.md`
  - `.ai-workflow/archive/2026-05/TASK-PHYS-004.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
  - Test: pass (`dotnet test tests/Engine.Physics.Tests/Engine.Physics.Tests.csproj --no-restore --nologo -v minimal`; 20/20)
  - Smoke: pass (two-step apply/query path proves the second resolve starts from updated PhysicsWorld body state)
  - Perf: pass (no solver/CCD/gravity/full world rebuild introduced; apply replaces one body entry and recalculates one AABB)
  - Boundary: pass (`Engine.Physics` still has no Engine module dependencies)
- ModuleAttributionCheck: `pass`
