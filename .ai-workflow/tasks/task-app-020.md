# 任务: TASK-APP-020 M20 App Physics production bridge

## TaskId
`TASK-APP-020`

## 目标（Goal）
在 `Engine.App` 中新增 production bridge，把真实 `SceneDescription` 映射为 `PhysicsWorldDefinition` 并在初始化阶段创建 `PhysicsWorld`，让 M19 的 test-only adapter 升级为生产主路径。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M20-2026-05-04`

## 里程碑引用（兼容别名：MilestoneRef）
`M20.2`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.SceneData
- Engine.Physics

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M20-G1`
- CanRunParallel: `true`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M20.2 是 Physics Runtime Collision MVP 的生产桥接卡；没有这张卡，M19 的 Physics core 仍停留在 `tests/Engine.Physics.Tests/**` 的 SceneData adapter 级别，无法进入真实 App 初始化路径。
- 本卡承担的是 App 内 production bridge、`SceneDescription -> PhysicsWorldDefinition` 映射与 `PhysicsWorld` 初始化，不承担 Scene writeback 和运行时 collision resolve。
- 直接影响本卡实现的上游背景包括：App 是 composition root；Physics 保持零 Engine 模块依赖；M20 不改变 SceneData schema，本卡只能消费已有 `RigidBody` / `BoxCollider` 描述。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - App bridge 读取：
    - `SceneObjectDescription.ObjectId`
    - `SceneObjectDescription.ObjectName`
    - `SceneObjectDescription.LocalTransform`
    - `SceneRigidBodyComponentDescription`
    - `SceneBoxColliderComponentDescription`
  - 映射规则：
    - 只有具备 `Transform + RigidBody + BoxCollider` 的对象进入 physics
    - SceneData `Static` -> Physics `Static`
    - SceneData `Dynamic` -> Physics `Dynamic`
  - bridge 不得修改输入 `SceneDescription`。
  - 初始化阶段 App 必须创建 `PhysicsWorld`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把生产 bridge 放进 `Engine.Physics` 或 `Engine.Scene`。
  - 不允许把 M19 test-only adapter 直接搬进 `src/Engine.Physics/**`。
  - 不允许在本卡引入 runtime writeback、main loop physics order 或碰撞解算逻辑。
  - 不允许顺手改 SceneData schema 或 Physics core public shape。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M20-2026-05-04 > App Production Bridge` 已定稿 bridge 输入字段、body type 映射和“不修改 `SceneDescription`”语义。
  - `PLAN-M20-2026-05-04 > Dependency Direction` 已定稿 App 拥有 production bridge，而 `Engine.Physics` 不知道 SceneData 或 Scene runtime objects。

## 实施说明（ImplementationNotes）
- 先在 `Engine.App` 中增加小型 production bridge/internal adapter，把 `SceneDescription` 映射到 `PhysicsWorldDefinition`。
- 再在 bootstrap / application initialization path 中创建 `PhysicsWorld`，并把 bridge failure 变成 deterministic run failure / 可测试诊断。
- 然后补 App tests，至少覆盖：
  - real `SceneDescription` to `PhysicsWorldDefinition`
  - only objects with Transform + RigidBody + BoxCollider enter physics
  - Static / Dynamic body type mapping
  - input `SceneDescription` not mutated
  - M19 test-only adapter 逻辑不再作为生产唯一入口

## 设计约束（DesignConstraints）
- 不允许把 `Engine.App` 生产 bridge 写成直接读取 JSON 文本或绕过 `SceneData` normalizer 的路径。
- 不允许把 `PhysicsWorldDefinition` bridge 逻辑散落在 `ApplicationBootstrap.cs` 的主流程中；应收敛为小型 internal App class 或等价局部抽象。
- 不允许让 bridge 依赖 Scene runtime object、`SceneGraphService` 或 `Engine.Scene` 写回能力。
- 不允许顺手加入 gravity、solver 或 runtime collision resolve。

## 失败与降级策略（FallbackBehavior）
- 若 bridge 命名需按仓库风格微调，允许等价命名，但职责必须仍清晰落在 App 层。
- 若某些 malformed scene 只能在 Physics core `Load(...)` 才能最终失败，允许 bridge 保持最小映射并把 deterministic failure 留给 Physics load，但测试必须能观察稳定诊断。
- 若实现中发现只有让 `Engine.Physics` 直接依赖 `SceneData` 才能继续，必须停工回退。
- 若 production bridge 只能靠复制 test-only adapter 到多个位置才能工作，也必须回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/PhysicsFoundationTests.cs`
- 相关测试入口：
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Physics.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-phys-002.md`
  - `.ai-workflow/tasks/task-sdata-009.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M20-2026-05-04.md`
- 计划结构引用：
  - `PLAN-M20-2026-05-04 > Milestones > M20.2`
  - `PLAN-M20-2026-05-04 > App Production Bridge`
  - `PLAN-M20-2026-05-04 > Dependency Direction`
  - `PLAN-M20-2026-05-04 > Runtime Data Flow`
  - `PLAN-M20-2026-05-04 > TestPlan`
  - 上述 bridge/dependency 引用属于“参考实现约束”，桥接归属、输入字段和生产化方向已定。

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - production bridge / internal adapter
  - bootstrap initialization wiring
  - App tests
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Scene writeback
- 不实现 runtime physics order integration
- 不实现 collision resolve
- 不修改 Physics core 或 SceneData schema
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - bridge/internal adapter 的具体类名可按 App 现有组织调整，但必须避免继续把所有逻辑塞进 `ApplicationBootstrap.cs`。
- 处理规则：
  - 若问题影响 App/Physics 边界、输入 scene 来源或 deterministic failure 语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - bridge 归属、映射规则、初始化责任和禁止路线都已下沉。
  - 执行者无需回看计划全文也能知道这张卡不能把生产 bridge 放回 Physics core。
  - 边界审批和停工条件已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 M20 真正把 Physics core 接入生产路径的第一张 App 卡，稍做不慎就会把 bridge 污染到 Physics core 或 bootstrap 巨石里。
  - 同时涉及新依赖放宽与 deterministic failure 语义，认知复杂度高。
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
- Required: `true`
- Status: `approved`
- RequestReason: `M20.2 需要由 App 作为 composition root 持有 production Physics bridge 并初始化 PhysicsWorld，当前 engine-app 边界合同尚未声明对 Engine.Physics 的允许依赖。`
- ImpactModules:
  - `Engine.App`
  - `Engine.Physics`
- HumanApprovalRef: `Human approved via “拆卡m20” on 2026-05-04`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 production bridge mapping、world init 和 deterministic failure
- Smoke: 真实 scene JSON 可走通 `SceneData load -> App bridge -> PhysicsWorld.Load(...)` 初始化主路径
- Perf: 不引入逐帧 SceneData 重新桥接、Physics core 反向依赖或 bootstrap 巨石化 side path

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Review

## 完成度（Completion）
`95`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-APP-020.md`
- ClosedAt: `2026-05-05 00:08`
- Summary:
  - Added App-owned production bridge from `SceneDescription` to `PhysicsWorldDefinition`.
  - Initialized `PhysicsWorld` during `ApplicationHost.Run()` startup after SceneData load and before Scene runtime initialization.
  - Updated bundled default scene JSON with existing RigidBody / BoxCollider schema so smoke uses real physics bodies.
  - Covered mapping, filtering, no-mutation, deterministic init failure, and real JSON smoke in App tests.
- FilesChanged:
  - `src/Engine.App/Engine.App.csproj`
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/ScenePhysicsWorldDefinitionBridge.cs`
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-020.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/2026-05/TASK-APP-020.md`
  - `.ai-workflow/archive/archive-index.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`，24/24 passed）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.App/Engine.App.csproj --nologo`，exit 0；真实默认 scene JSON 经 SceneData load -> App bridge -> PhysicsWorld.Load 初始化）
  - Perf: pass（Physics bridge 仅在初始化阶段执行；未引入逐帧 SceneData 重新桥接、Physics core 反向依赖或 bootstrap 巨石化 side path）
- ModuleAttributionCheck: pass
