# 任务: TASK-PHYS-002 M19 PhysicsWorld load, fixed step, snapshot and AABB queries

## TaskId
`TASK-PHYS-002`

## 目标（Goal）
在 `Engine.Physics` 中实现 `PhysicsWorldDefinition -> PhysicsWorld` 主路径、固定步进统计、只读 snapshot，以及 AABB overlap / ground query，让 M19 Physics foundation 具备独立核心数据流和可验证查询能力。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M19-2026-05-03`

## 里程碑引用（兼容别名：MilestoneRef）
`M19.3`

## 执行代理（ExecutionAgent）
Exec-Physics

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Physics

## 次级模块（SecondaryModules）
- none

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-physics.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M19-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-PHYS-001`

## 里程碑上下文（MilestoneContext）
- M19.3 是 Physics foundation 的主实现卡；没有这张卡，`Engine.Physics` 只是模块骨架，physics-owned definition 也无法进入真实 world / step / snapshot / query 主路径。
- 本卡承担的是 PhysicsWorld 的真实加载、fixed-step 统计、snapshot 与 query，不承担 App 主循环接入、Scene Transform 回写或可见物理效果。
- 直接影响本卡实现的上游背景包括：Physics 生产代码只消费 `PhysicsWorldDefinition`；只有同时具备 physics transform、body、box collider 定义的对象进入 world；M19 忽略 rotation；M20 才做 App/Scene runtime behavior、SceneData bridge 与 Transform writeback。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `PhysicsWorld.Load(PhysicsWorldDefinition)` 或等价入口必须存在。
  - 只有同时具备 `PhysicsTransform`、`PhysicsBodyDefinition`、`PhysicsBoxColliderDefinition` 的对象进入 physics world。
  - `PhysicsWorld.Step(PhysicsStepContext)` 负责更新 step count 与 accumulated fixed seconds。
  - `PhysicsWorld.CreateSnapshot()` 输出只读 body/collider/AABB 状态。
  - query 至少包括：
    - AABB overlap
    - ground query
  - malformed physics input 应显式失败；若采用异常，测试必须钉死足够稳定的 failure kind/message。
  - AABB 规则：
    - center = Transform position + collider center
    - half extents = positive box size * absolute transform scale * 0.5
    - rotation ignored
    - ground plane fixed at `Y = 0`
  - `Step` 不修改输入 `PhysicsWorldDefinition`，也不回写 `Engine.Scene` Transform。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 PhysicsWorld 改成依赖 `Engine.Scene` runtime object 的系统。
  - 不允许让 `src/Engine.Physics/**` 引用 `Engine.SceneData`、`Engine.Contracts`、`Engine.Core` 或 `SceneDescription`。
  - 不允许在 M19 引入力、速度积分、碰撞求解、重力、反弹、摩擦或 solver。
  - 不允许把 query API 设计成直接暴露内部可变 body 集合。
  - 不允许把 App 主循环或 Scene update 接进本卡。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M19-2026-05-03 > Data Flow` 已定稿 `PhysicsWorldDefinition -> PhysicsWorld -> Step -> Snapshot/Query` 主路径；真实 SceneData fixture 只能经测试侧 adapter 映射后进入该入口，生产 Physics 不得直接消费 SceneData。
  - `PLAN-M19-2026-05-03 > AABB Rules` 已定稿 AABB 中心、半尺寸、忽略 rotation 和固定地面查询语义。
  - 当前 SceneData 对 Transform scale 仅保证 finite、不保证正值，因此本卡必须按计划的 `absolute transform scale` 路线处理负 scale，并在测试中钉死；不得新增 SceneData 级正 scale 假设。

## 实施说明（ImplementationNotes）
- 先实现 `Load(PhysicsWorldDefinition)` 或等价 world 构建入口，消费 physics-owned definition 中的 transform / body / box collider 描述。
- 再实现 body 进入 world 的筛选和 malformed input fail-fast：
  - 缺 physics transform fail
  - malformed physics components fail
  - 仅完整三件套对象进入 world
- 然后实现 `Step(...)` 的最小固定步进统计，不推进任何可见物理行为，只更新 world 统计与可观察状态。
- 接着实现 `CreateSnapshot()` 与 query APIs，保证输出只读 body/collider/AABB 结果。
- 最后补 `Engine.Physics.Tests`，至少覆盖：
  - `PhysicsWorldDefinition -> PhysicsWorld` 主路径
  - 可选测试侧 adapter：realistic SceneData fixture -> PhysicsWorldDefinition -> PhysicsWorld；adapter 必须只存在于 `tests/Engine.Physics.Tests/**`
  - body count / ids / names / body types 稳定
  - AABB 计算
  - overlap / ground query
  - malformed input fail fast
  - no Transform writeback / no input PhysicsWorldDefinition mutation

## 设计约束（DesignConstraints）
- 不允许修改 `Engine.Scene`、`Engine.App`、`Engine.Render`、`Engine.Scripting` 以完成 PhysicsWorld 主路径。
- 不允许修改 `Engine.SceneData` 或添加 `Engine.SceneData` 项目引用来完成 PhysicsWorld 主路径。
- 不允许把 Physics world state 与 Scene runtime state 共用同一套可变对象。
- 不允许在本卡内引入额外 collider 形状、trigger、layer 或 material。
- 不允许把 fixed-step 统计做成依赖外部主循环时间系统才能使用。

## 失败与降级策略（FallbackBehavior）
- 若 world 构建入口名称需按仓库风格微调，允许等价命名，但必须仍然是从 `PhysicsWorldDefinition` 入手的主路径。
- 若 malformed input 语义暂时只能用异常表达，允许短期保留，但测试必须钉死足够稳定的 failure kind/message，后续不得模糊化。
- 若实现中发现必须回写 Scene Transform、引入 App time loop 或依赖 Scene runtime object 才能继续，必须停工回退。
- 若 AABB/ground query 只能靠直接暴露内部 body 集合来让测试通过，也必须回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Physics/**`
- 相关测试入口：
  - `tests/Engine.Physics.Tests/**`
  - `tests/Engine.SceneData.Tests/**`（只作 fixture/schema 参考，不允许生产 Physics 依赖）
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-phys-001.md`
  - `.ai-workflow/tasks/task-sdata-009.md`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M19-2026-05-03.md`
- 计划结构引用：
  - `PLAN-M19-2026-05-03 > Milestones > M19.3`
  - `PLAN-M19-2026-05-03 > Engine.Physics Public Shape`
  - `PLAN-M19-2026-05-03 > Data Flow`
  - `PLAN-M19-2026-05-03 > AABB Rules`
  - `PLAN-M19-2026-05-03 > RuntimeRealityCheck`
  - `PLAN-M19-2026-05-03 > TestPlan`
  - 上述 data flow / AABB rules 引用属于“实现约束”，world 输入主路径、AABB 算法和 foundation/MVP 分界已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Physics
- AllowedFiles:
  - physics world definition types
  - physics world load path
  - step context / statistics
  - snapshot / query implementation
  - physics tests
- AllowedPaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Scene Transform writeback
- 不实现 SceneData -> PhysicsWorldDefinition 生产桥接
- 不实现 App main loop integration
- 不实现 gravity / solver / visible collision response
- 不实现额外 collider / trigger / layer / material
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scripting/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - malformed physics input 若先以异常表达，必须在实现中统一稳定诊断口径；否则优先选择显式结果类型。
- 处理规则：
  - 若问题影响 world 输入主路径、AABB 规则、Transform writeback 边界或 foundation/MVP 分界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - world 主路径、AABB 规则、fixed-step 统计、scale 处理和禁止路线都已落卡。
  - 执行者无需回看里程碑全文也能知道 M19 只做 foundation，不做 runtime 可见物理。
  - 关键风险与回退条件已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡是 M19 的真实核心实现，涉及独立 definition 数据流、world state、步进统计和查询语义，且边界选错就会直接滑入 M20。
  - 还需要把当前 SceneData finite-only scale 语义与 AABB 规则明确钉死，避免后续歧义。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - .NET 标准库
  - `System.Numerics`
- ForbiddenDependsOn:
  - `Engine.Physics -> Engine.SceneData`
  - `Engine.Physics -> Engine.Contracts`
  - `Engine.Physics -> Engine.Core`
  - `Engine.Physics -> Engine.Scene`
  - `Engine.Physics -> Engine.App`
  - `Engine.Physics -> Engine.Render`
  - `Engine.Physics -> Engine.Scripting`
  - `Engine.Physics -> Engine.Editor`
  - `Engine.Physics -> Engine.Editor.App`

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
- Test: `dotnet test tests/Engine.Physics.Tests/Engine.Physics.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 `PhysicsWorldDefinition` 加载、AABB、query、step 统计和 failure 语义
- Smoke: 至少一条 `PhysicsWorldDefinition` 能走通 `PhysicsWorldDefinition -> PhysicsWorld -> Step -> Snapshot/Query` 主路径；如使用 SceneData fixture，只能经测试侧 adapter 映射
- Perf: 不引入 App loop 绑定、逐帧 JSON 解析、Transform 回写或渲染 side path

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-PHYS-002.md`
- ClosedAt: `2026-05-04 21:26`
- Summary:
  - Implemented `PhysicsWorld.Load(PhysicsWorldDefinition)` with malformed input fail-fast diagnostics.
  - Implemented fixed-step statistics, read-only snapshots, AABB overlap query, and ground query.
  - Added AABB tests for position + collider center + absolute scale while ignoring rotation.
  - Added test-only SceneData fixture adapter inside `tests/Engine.Physics.Tests/**`.
- FilesChanged:
  - `src/Engine.Physics/PhysicsWorld.cs`
  - `src/Engine.Physics/PhysicsQueryResult.cs`
  - `tests/Engine.Physics.Tests/Engine.Physics.Tests.csproj`
  - `tests/Engine.Physics.Tests/PhysicsFoundationTests.cs`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/tasks/task-phys-002.md`
  - `.ai-workflow/archive/2026-05/TASK-PHYS-002.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test tests/Engine.Physics.Tests/Engine.Physics.Tests.csproj --no-restore --nologo -v minimal`，10/10 passed）
  - Smoke: pass（`PhysicsWorldDefinition -> PhysicsWorld -> Step -> Snapshot/Query` 主路径通过；SceneData fixture 仅经测试侧 adapter 映射）
  - Perf: pass（no App loop binding, per-frame JSON parsing, Transform writeback, solver, gravity, or render side path）
- ModuleAttributionCheck: pass
