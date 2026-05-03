# 任务: TASK-PHYS-001 M19 Engine.Physics module and boundary foundation

## TaskId
`TASK-PHYS-001`

## 目标（Goal）
新建独立 `Engine.Physics` 模块与测试项目，落地 physics world / step context / snapshot / query 的最小公开形状，并用边界测试固定 `Engine.Physics` 的允许依赖方向。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M19-2026-05-03`

## 里程碑引用（兼容别名：MilestoneRef）
`M19.1`

## 执行代理（ExecutionAgent）
Exec-Physics

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Physics

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Contracts
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-physics.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M19-G1`
- CanRunParallel: `true`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M19.1 是 Physics foundation 的真正起点；没有独立 `Engine.Physics` 模块、公开 world 形状和边界合同，后续 SceneData 物理 schema 与 PhysicsWorld 主路径都没有稳定落点。
- 本卡承担的是新模块骨架、公开类型地基、测试工程和边界合同，不承担 SceneData `RigidBody/BoxCollider` schema，也不承担 fixed-step 真实加载与查询行为实现。
- 直接影响本卡实现的上游背景包括：M19 只做 physics foundation，不做 App 主循环、Transform 回写、可见物理反馈或 solver；`Engine.Physics` 允许直接依赖 `Engine.SceneData`，但绝不能依赖 `Engine.Scene/App/Render/Scripting/Editor`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 新建 `Engine.Physics` 与 `Engine.Physics.Tests`。
  - 建议公开类型至少包括：
    - `PhysicsWorld`
    - `PhysicsStepContext`
    - `PhysicsWorldSnapshot`
    - `PhysicsBodySnapshot`
    - `PhysicsBodyType`
    - `PhysicsAabb`
    - `PhysicsQueryResult` 或等价显式结果类型
  - `Engine.Physics` 允许依赖：
    - `Engine.SceneData`
    - `Engine.Contracts`
    - `Engine.Core`
  - `Engine.Physics` 禁止依赖：
    - `Engine.Scene`
    - `Engine.App`
    - `Engine.Render`
    - `Engine.Scripting`
    - `Engine.Editor`
    - `Engine.Editor.App`
    - `OpenTK / ImGui / OpenGL`
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 foundation 卡中加入 Transform 回写、App 调度、重力或碰撞求解。
  - 不允许把 Physics world 建成依赖 `Scene` runtime object 的系统。
  - 不允许通过 stub-only DTO 让模块骨架成立但没有清晰 world/snapshot/query 公开面。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M19-2026-05-03 > Engine.Physics Public Shape` 已定稿 world / step / snapshot / body / AABB / query 的最小公开形状方向，执行时不得换成与之无关的“物理管理器 + 任意集合暴露”模式。
  - `PLAN-M19-2026-05-03 > Boundary Rules` 已定稿允许/禁止依赖集合，执行时不得自行放宽。

## 实施说明（ImplementationNotes）
- 先建立 `Engine.Physics` 与 `Engine.Physics.Tests` 工程并接入解决方案。
- 再定义最小公开值对象和类型壳：
  - body type
  - step context
  - world snapshot / body snapshot / AABB / query result
- 然后补边界测试，验证项目引用与源码都不出现 `Scene/App/Render/Scripting/Editor` 禁止依赖。
- 公开类型可先以最小可编译壳落地，但命名与职责必须为后续 `TASK-PHYS-002` 留稳定入口。

## 设计约束（DesignConstraints）
- 不允许在本卡实现 `LoadFromDescription(...)` 的完整物理构建逻辑。
- 不允许在本卡引入 JSON 解析、文件 IO、OpenTK、OpenGL 或 ImGui。
- 不允许把公开 snapshot/query 面设计成可变内部集合直出。
- 不允许顺手扩张到 App/Scene 接线、可视 smoke 行为或 physics solver。

## 失败与降级策略（FallbackBehavior）
- 若 query/result 的具体类型命名需按仓库风格微调，允许等价命名，但 world / step / snapshot / AABB / query 这几类公开入口必须仍然存在。
- 若实现中发现只有依赖 `Engine.Scene` runtime types 才能定义 Physics world，必须停工回退。
- 若边界测试无法证明禁止依赖缺失，不得以“后续再补”放行。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `AnsEngine.sln`
  - `src/Engine.SceneData/**`
  - `src/Engine.Core/**`
  - `src/Engine.Contracts/**`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/**`
  - `tests/Engine.Scene.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M19-2026-05-03.md`
  - `.ai-workflow/tasks/task-plat-003.md`
- 计划结构引用：
  - `PLAN-M19-2026-05-03 > Milestones > M19.1`
  - `PLAN-M19-2026-05-03 > Engine.Physics Public Shape`
  - `PLAN-M19-2026-05-03 > Boundary Rules`
  - `PLAN-M19-2026-05-03 > HandoffToDispatch`
  - 上述 public shape / boundary rules 引用属于“参考实现约束”，world 公开形状与依赖方向已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Physics
- AllowedFiles:
  - new physics project
  - new physics test project
  - public shape types
  - boundary tests
  - solution wiring
- AllowedPaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`
  - `AnsEngine.sln`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 SceneData physics schema
- 不实现 PhysicsWorld 从真实 scene 加载
- 不实现 Step 统计、snapshot 内容填充或 query 算法
- 不实现 App/Scene/Render/Scripting integration
- OutOfScopePaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scripting/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - query/result 的具体类型命名可按仓库风格微调，但必须仍对应 world snapshot 与 query 入口，而不是延后为实现时再决定。
- 处理规则：
  - 若问题影响允许依赖集合、world 公开面或 foundation/MVP 分界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 新模块、公开形状、禁止路线和依赖方向都已下沉到卡面。
  - 执行者无需回看里程碑全文也能知道这是一张骨架与边界卡，不是物理行为实现卡。
  - 停工条件和非范围已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是新模块和新子系统的边界地基，若公开形状或依赖方向选错，后续 PhysicsWorld 与 SceneData 连接都会返工。
  - 同时要防止 foundation 卡滑入 Runtime MVP，认知复杂度高。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Physics -> Engine.SceneData`
  - `Engine.Physics -> Engine.Contracts`
  - `Engine.Physics -> Engine.Core`
- ForbiddenDependsOn:
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
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-physics.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Physics.Tests/Engine.Physics.Tests.csproj --no-restore --nologo -v minimal` 通过，至少覆盖工程可引用与边界依赖检查
- Smoke: `Engine.Physics` 可编译、测试项目可引用，且存在稳定 world / step / snapshot / query 公开入口
- Perf: 不引入 JSON 解析、App loop 绑定、Transform 回写或渲染依赖

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-PHYS-001.md`
- ClosedAt:
- Summary:
- FilesChanged:
- ValidationEvidence:
- ModuleAttributionCheck: pass
