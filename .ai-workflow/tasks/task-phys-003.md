# 任务: TASK-PHYS-003 M20 Physics kinematic collision resolve

## TaskId
`TASK-PHYS-003`

## 目标（Goal）
在 `Engine.Physics` 中新增 kinematic movement resolve，使 static AABB 能对脚本/逻辑层给出的 desired transform 做静态碰撞约束，并输出 resolved transform 供 App 写回 Scene。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M20-2026-05-04`

## 里程碑引用（兼容别名：MilestoneRef）
`M20.3`

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
- ParallelGroup: `M20-G1`
- CanRunParallel: `true`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M20.3 是 Physics Runtime Collision MVP 的核心行为卡；没有 kinematic resolve，M20 只能把 physics world 初始化进 App，却不能真正约束脚本移动结果。
- 本卡承担的是 Physics core 内的 desired -> resolved transform 计算和显式 result，不承担 Scene writeback、App 顺序接线或 dynamic rigidbody 模拟。
- 直接影响本卡实现的上游背景包括：M20 只做 static collision + kinematic movement；Dynamic body 在 M20 只是可被脚本驱动的 mover 候选，不做自动动力学；Physics 继续零 Engine 模块依赖。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 推荐 API：
    - `PhysicsKinematicMoveResult ResolveKinematicMove(string bodyId, PhysicsTransform desiredTransform)` 或等价显式 result
  - result 至少包含：
    - `BodyId`
    - `DesiredTransform`
    - `ResolvedTransform`
    - `HasHit`
    - blocking/hit body id 或等价 contact state
  - 未碰撞时 `ResolvedTransform == DesiredTransform`
  - 碰撞时 resolved transform 不得进入 static collider
  - rotation 不参与碰撞计算，沿用 M19 AABB 规则
- 本卡执行时不得推翻的既定取舍：
  - 不允许引入 dynamic gravity、velocity、force、impulse、solver 或 visible physics side effect。
  - 不允许让 static bodies 在 resolve 中移动。
  - 不允许让 Physics 读取 Scene runtime object 或直接写回 Scene。
  - 不允许把碰撞策略留给执行者现场决定。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M20-2026-05-04 > Kinematic Collision Model` 已定稿 Scene/Script 提供 desired transform，Physics 提供 resolved transform，App 写回 Scene 的所有权分工。
  - `PLAN-M20-2026-05-04 > Milestones > M20.3` 已要求任务卡钉死具体策略。本卡固定采用“按轴顺序单独尝试位移、被阻挡轴回退”的 conservative axis resolve：
    - 以 current transform 为起点
    - 按 `X -> Y -> Z` 顺序分别尝试应用该轴 delta
    - 每次只在单轴候选位置重新计算 mover AABB
    - 若与任一 static AABB overlap，则该轴位移取消并记录 first blocking body id
    - 若不 overlap，则保留该轴位移并继续下一轴
    - 最终 `ResolvedTransform` 保留所有未被阻挡的轴位移
  - 该策略是“参考实现约束”，执行时不得改成最小平移向量、扫掠碰撞或任意其他未约定算法。

## 实施说明（ImplementationNotes）
- 先在 Physics core 中增加 kinematic move result / contact state 类型和公开 resolve API。
- 再实现按 `X -> Y -> Z` 的单轴保守位移解析，只对 static AABB 做阻挡判断。
- 然后保证 malformed input fail-fast：
  - invalid body id
  - invalid desired transform
  - body missing collider / transform / type state 不可用于 resolve
- 最后补 Physics tests，至少覆盖：
  - no collision returns desired transform
  - move into static collider is blocked
  - partial axis block preserves unblocked axes
  - static bodies do not move
  - invalid input failures
  - zero Engine dependencies preserved

## 设计约束（DesignConstraints）
- 不允许在本卡引入 SceneData、Scene、App、Render、Scripting 依赖。
- 不允许把 resolve 结果直接写进 world snapshot 之外的外部系统；Physics 只返回结果。
- 不允许把 overlap query 或 ground query 的现有只读语义改成有副作用。
- 不允许顺手加入 slope、step climbing、friction、bounce、continuous collision detection。

## 失败与降级策略（FallbackBehavior）
- 若 result 类型命名需按仓库风格微调，允许等价命名，但必须保留 desired/resolved/bodyId/hasHit 这些核心事实。
- 若 contact state 细节一时难以丰富，至少要稳定返回 first blocking body id 和 `HasHit`，不得退化为只有布尔值。
- 若实现中发现必须依赖 Engine 模块或 Scene writeback 才能继续，必须停工回退。
- 若单轴保守解法出现角落滑动等非 MVP 现象，只要满足“绝不穿入 static collider、结果 deterministic”，可保留到 M21+；不得临时扩 solver。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Physics/**`
- 相关测试入口：
  - `tests/Engine.Physics.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-phys-002.md`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M20-2026-05-04.md`
- 计划结构引用：
  - `PLAN-M20-2026-05-04 > Milestones > M20.3`
  - `PLAN-M20-2026-05-04 > Kinematic Collision Model`
  - `PLAN-M20-2026-05-04 > Failure And Fallback`
  - `PLAN-M20-2026-05-04 > TestPlan`
  - 上述 collision model 引用属于“实现约束”，body ownership、desired/resolved 分工和按轴保守解法已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Physics
- AllowedFiles:
  - kinematic resolve API/result types
  - collision resolve logic
  - physics tests
- AllowedPaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Scene writeback
- 不实现 App 调用顺序
- 不实现 gravity / solver / friction / bounce
- 不实现额外 collider / trigger / layer / material
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - hit body/contact state 的具体字段名可按 Physics 现有命名风格调整，但必须能稳定表达 first blocking body。
- 处理规则：
  - 若问题影响按轴保守解法、结果确定性或 Physics 零依赖边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - desired/resolved 分工、按轴策略、失败语义和非目标都已落卡。
  - 执行者无需回看计划全文也能知道这不是 solver 卡，而是 deterministic kinematic constraint MVP。
  - 核心算法不再留空给执行者现场决定。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 M20 最容易“想当然”扩成真正物理系统的一张卡，必须同时钉死算法、所有权和非目标。
  - 若写短，执行者很可能自行发明另一套 resolve 策略或把 static body 移动起来。
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
- Test: `dotnet test tests/Engine.Physics.Tests/Engine.Physics.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 no-hit、hit、partial-axis-block、invalid-input、boundary
- Smoke: kinematic body 贴近 static box 时不会穿入；无碰撞时 resolved transform 等于 desired transform
- Perf: 不引入 solver、连续碰撞检测、渲染 side path 或 Engine 模块依赖

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-PHYS-003.md`
- ClosedAt: `2026-05-05 00:13`
- Summary:
  - Added `PhysicsWorld.ResolveKinematicMove(...)` and explicit `PhysicsKinematicMoveResult`.
  - Implemented fixed X -> Y -> Z single-axis conservative resolve against static AABB blockers.
  - Preserved unblocked axes, recorded first blocking body id, and left world snapshot unchanged.
  - Covered no-hit, hit, partial-axis block, static body immobility, invalid input, and boundary tests.
- FilesChanged:
  - `src/Engine.Physics/PhysicsWorld.cs`
  - `tests/Engine.Physics.Tests/PhysicsFoundationTests.cs`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/tasks/task-phys-003.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/2026-05/TASK-PHYS-003.md`
  - `.ai-workflow/archive/archive-index.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test tests/Engine.Physics.Tests/Engine.Physics.Tests.csproj --no-restore --nologo -v minimal`，15/15 passed）
  - Smoke: pass（无碰撞时 resolved == desired；kinematic body 移向 static box 时不会穿入，阻挡轴被取消）
  - Perf: pass（未引入 solver、连续碰撞检测、渲染 side path 或 Engine 模块依赖）
- ModuleAttributionCheck: pass
