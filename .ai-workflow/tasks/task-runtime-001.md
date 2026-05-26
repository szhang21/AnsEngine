# 任务: TASK-RUNTIME-001 M24 Engine.Runtime module and tick pipeline

## 目标（Goal）
新增 `Engine.Runtime` 模块与测试项目，集中承载 Scene/script/physics runtime session 初始化和 tick pipeline，让 runtime update components 与 physics writeback 在 render 前完成。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE`

## 里程碑引用（兼容别名：MilestoneRef）
`M24.4`

## 执行代理（ExecutionAgent）
Exec-Runtime

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Runtime

## 次级模块（SecondaryModules）
- tests
- Engine.App

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-runtime.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M24-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCRIPT-005`

## 里程碑上下文（MilestoneContext）
- App 当前承担 scene/script/physics update 顺序，已经接近伪 runtime；M24 要把运行时 orchestration 收敛到新的 `Engine.Runtime`。
- 本卡建立 `EngineRuntimeSession`、runtime tick context/result、component update traversal 和 physics sync/writeback 迁移落点。
- 本卡是 `TASK-APP-023` 的前置，App contraction 只能在 runtime session 可用后进行。

## 决策继承（DecisionCarryOver）
- 继承决策：
  - `Engine.Runtime` 可依赖 `Engine.Scene`、`Engine.Scripting`、`Engine.Physics`、`Engine.SceneData`、`Engine.Runtime.Abstractions`、`Engine.Contracts`。
  - Runtime owns session initialization, update component traversal, script binding consumption, physics world creation, physics apply/writeback orchestration.
  - First pipeline: `Scene base update/statistics -> Runtime update components -> Physics apply/writeback -> Scene state ready for render`。
- 不得推翻的既定取舍：
  - Runtime 不拥有 rendering/present/window lifecycle。
  - Runtime 不解析 platform input，不直接依赖 `Engine.Platform`。
  - Runtime 不实现 dynamic physics solver redesign、external scripting、animation、audio、signal system。
- 上游已定结构约束：
  - `EngineRuntimeSession.Initialize(SceneDescription sceneDescription)` 形态可作为 public entry 参考。
  - `RuntimeTickContext(double DeltaSeconds, double TotalSeconds, RuntimeInputSnapshot Input)` 是推荐 tick input。
  - Tick failure 必须能在 render 前暴露 script/physics deterministic diagnostics。

## 实施说明（ImplementationNotes）
- 第一步创建 `src/Engine.Runtime/Engine.Runtime.csproj`、`tests/Engine.Runtime.Tests/Engine.Runtime.Tests.csproj`，并把 solution 加入新项目。
- 同步创建 `.ai-workflow/boundaries/engine-runtime.md`，先写明 Engine.Runtime 的职责、允许依赖、禁止依赖、public session/tick API、质量门禁；实现前必须完成该边界文档。
- 实现 `EngineRuntimeSession`、`RuntimeInitializationResult`、`RuntimeTickContext`、`RuntimeTickResult` 等最小 result/context shape。
- 把 App 现有 `RuntimePhysicsOrchestrator` 与 `ScenePhysicsWorldDefinitionBridge` 的等价职责迁移到 Runtime 模块，保留 App 兼容调用直到 `TASK-APP-023` 完成收缩。
- Runtime 初始化从 `SceneDescription` 建立 Scene runtime、PhysicsWorld、script update components；tick 时先推进 Scene statistics，再 update components，再 physics sync/writeback。
- 补 Runtime tests：initialization、tick order、update traversal、script failure before render、physics sync after script update、dependency direction。

## 设计约束（DesignConstraints）
- 不允许把 App host、window、renderer、present、asset warmup 放进 Engine.Runtime。
- 不允许 Runtime 直接依赖 `Engine.Platform` 或 OpenTK。
- 不允许只把 App spaghetti 移动到新项目；必须有 session/tick API、边界合同、独立测试。
- 不允许把 Scripting 的 registry/factory/bind diagnostics 复制到 Runtime；Runtime 只消费 binding outputs。
- 不允许改变 Physics solver MVP 行为范围。

## 失败与降级策略（FallbackBehavior）
- Script update failure 或 physics sync failure 必须使 `RuntimeTickResult` fail before render，并携带 object/component/stage diagnostics。
- Scene initialization failure 或 missing required component 必须显式返回 initialization/tick failure，不让 App 继续 render 陈旧状态。
- 若发现 Runtime 需要 Platform/Render dependency 才能完成，停工回退并提出边界变更请求。
- 若迁移 App physics helper 时必须跨越 `AllowedPaths`，只允许在本卡必要兼容面内触碰 `src/Engine.App/RuntimePhysicsOrchestrator.cs` / `ScenePhysicsWorldDefinitionBridge.cs`，更大 App 收缩留给 `TASK-APP-023`。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/RuntimePhysicsOrchestrator.cs`
  - `src/Engine.App/ScenePhysicsWorldDefinitionBridge.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Scripting/ScriptRuntime.cs`
  - `src/Engine.Physics/PhysicsWorld.cs`
- 相关测试入口：
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `tests/Engine.Physics.Tests/PhysicsFoundationTests.cs`
  - `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
- 相关文档/计划：
  - `.ai-workflow/plan-archive/2026-05/PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE.md` 的 `Engine.Runtime Module`、`RuntimeRealityCheck`、`M24.4 Engine.Runtime Module And Tick Pipeline`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.codex/skills/engine-task-dispatch/references/boundary-contract-template.md`
- 示例结构定位：
  - M24 `Engine.Runtime Module` 中的 `EngineRuntimeSession` / `RuntimeTickContext` 示例是 public API 参考约束；可命名微调，但职责与数据流不得改写。

## 范围（Scope）
- AllowedModules:
  - Engine.Runtime
  - tests
  - Engine.App compatibility extraction only
- AllowedFiles:
  - `src/Engine.Runtime/**`
  - `tests/Engine.Runtime.Tests/**`
  - `AnsEngine.sln`
  - `src/Engine.App/RuntimePhysicsOrchestrator.cs`
  - `src/Engine.App/ScenePhysicsWorldDefinitionBridge.cs`
  - `src/Engine.App/Engine.App.csproj`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- AllowedPaths:
  - `src/Engine.Runtime/**`
  - `tests/Engine.Runtime.Tests/**`
  - `AnsEngine.sln`
  - `src/Engine.App/RuntimePhysicsOrchestrator.cs`
  - `src/Engine.App/ScenePhysicsWorldDefinitionBridge.cs`
  - `src/Engine.App/Engine.App.csproj`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- Full App host contraction
- Renderer initialize/shutdown/present changes
- Platform input polling changes
- Asset/bootstrap mesh warmup changes
- Editor integration
- External script loading / hot reload
- Animation/audio/signal systems
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Asset/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `Engine.Runtime` 边界合同尚不存在，执行本卡必须先按 boundary template 创建该文档并写入变更记录。
- 处理规则：
  - 若 Human/Steward 判定缺少 boundary seed 不可开工，则先执行本卡中的 boundary creation 子步骤，不得进入 App contraction。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 计划已明确 Runtime 依赖方向、session/tick职责、pipeline 顺序和失败语义。
  - 本卡明确了新模块、测试项目、边界创建、App helper 迁移允许范围。
  - 执行者无需回看 M24 全文即可按本卡实施。
- MissingInfo:
  - none

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene`
  - `Engine.Scripting`
  - `Engine.Physics`
  - `Engine.SceneData`
  - `Engine.Runtime.Abstractions`
  - `Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.App`
  - `Engine.Platform`
  - `Engine.Render`
  - `Engine.Editor`
  - `Engine.Editor.App`
  - `Engine.Asset`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-runtime.md`
  - `.ai-workflow/boundaries/README.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过。
- Test: `dotnet test tests/Engine.Runtime.Tests/Engine.Runtime.Tests.csproj --no-restore --nologo -v minimal` 通过；必要时相关 App/Scene/Scripting/Physics tests 也通过。
- Smoke: Runtime tick 可在 headless test 中完成 script update -> physics writeback -> scene state ready for render，并在 failure 时 render 前失败。
- Perf: 新 runtime tick 不引入明显额外全量分配或重复 physics world rebuild；记录无明显退化说明。
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
- New `Engine.Runtime` source project
- New `Engine.Runtime.Tests` test project
- Runtime boundary contract seed and change log
- Runtime session/tick API and tests
- Migrated physics orchestration equivalent
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 复杂度评估（ComplexityAssessment）
- Level: `L3`
- Why:
  - 新模块和 orchestration 边界选错会破坏 App/Runtime 职责。
  - 需要协调 Scene/Scripting/Physics 的真实主路径。
  - 失败语义必须 render 前收口。
- SufficiencyMatch: `pass`

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-RUNTIME-001.md`
- ClosedAt: `2026-05-27`
- Summary: Added `Engine.Runtime` source/test projects with `EngineRuntimeSession`, runtime tick context/result, script update component traversal, SceneData-to-PhysicsWorld bridge, and runtime physics writeback before render.
- FilesChanged:
  - `src/Engine.Runtime/**`
  - `tests/Engine.Runtime.Tests/**`
  - `AnsEngine.sln`
  - `.ai-workflow/boundaries/engine-runtime.md`
  - `.ai-workflow/boundaries/README.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL warnings; 0 errors.
  - Test: `dotnet test tests/Engine.Runtime.Tests/Engine.Runtime.Tests.csproj --no-restore --nologo -v minimal` passed; 7 passed, 0 failed.
  - Smoke: Runtime tests cover script update -> physics writeback -> scene state ready before render and fail-before-render script diagnostics.
  - Perf: `PhysicsWorld` is created during initialization and reused across ticks; no per-frame world rebuild added.
  - CodeQuality: NoNewHighRisk=`true`; MustFixCount=`0`; MustFixDisposition=`none`.
  - DesignQuality: DQ-1=`pass`; DQ-2=`pass`; DQ-3=`pass`; DQ-4=`pass`.
- ModuleAttributionCheck: pass
