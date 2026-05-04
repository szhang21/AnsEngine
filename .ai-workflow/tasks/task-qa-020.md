# 任务: TASK-QA-020 M19 Physics Foundation gate review and archive

## TaskId
`TASK-QA-020`

## 目标（Goal）
对 M19 Physics foundation 执行全量 build/test/smoke 与边界复验，确认 `PhysicsWorldDefinition -> PhysicsWorld -> Step -> Snapshot/Query` 核心链路稳定，同时 SceneData fixture 只能经测试侧 adapter 验证映射，并且没有滑入 Transform 回写、App 接线或可见 runtime physics。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M19-2026-05-03`

## 里程碑引用（兼容别名：MilestoneRef）
`M19.4`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.Physics

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Scene
- Engine.App
- Engine.Render
- Engine.Scripting

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-physics.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M19-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-PHYS-001`
  - `TASK-SDATA-009`
  - `TASK-PHYS-002`

## 里程碑上下文（MilestoneContext）
- M19.4 是 Physics foundation 的关闭门禁，本卡不再新增能力，而是确认物理 schema、PhysicsWorld、fixed-step 统计、snapshot/query 和边界方向都已稳定落地。
- 本卡承担的是 build/test/smoke、边界复验、质量结论和归档准备，不承担功能实现。
- 直接影响本卡实现的上游背景包括：M19 是 foundation milestone，不承诺窗口内可见物理；M20 才做 App fixed-step、gravity、collision response 和 Scene Transform writeback。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Physics 真实路径必须是：
    - `PhysicsWorldDefinition`
    - `Engine.Physics.PhysicsWorld`
    - fixed step
    - snapshot/query
  - realistic SceneData fixture 可作为测试证据，但必须先由测试侧 adapter 映射为 `PhysicsWorldDefinition`。
  - `Engine.Physics` 不得依赖 `SceneData/Contracts/Core/Scene/App/Render/Scripting/Editor/Editor.App`。
  - `Engine.SceneData` 不得依赖 `Engine.Physics`。
  - `Engine.Scene` / `Engine.Render` / `Engine.Scripting` 不感知 Physics。
  - M19 不写回 Transform、不接入 App 主循环、不产生 visible gravity/collision response。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 QA 卡中补实现或接受“Physics world 存在就算完成”的空壳通过。
  - 不允许把 stub-only DTO tests 当作真实 foundation 证据；核心 definition 测试必须覆盖真实 world/snapshot/query，SceneData fixture 证据必须明确是测试侧 adapter。
  - 不允许把“没有 visible physics”误判为缺陷，因为这在 M19 明确不在范围内。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M19-2026-05-03 > RuntimeRealityCheck`、`AABB Rules`、`Boundary Rules`、`TestPlan` 是本卡判定是否按计划落地的固定参照。
  - `PLAN-M19-2026-05-03 > Scope > Out of scope` 是本卡判断是否越界的重要硬约束。

## 实施说明（ImplementationNotes）
- 先复核 `TASK-PHYS-001`、`TASK-SDATA-009`、`TASK-PHYS-002` 的 `AllowedPaths`、边界文档更新和依赖链。
- 执行自动验证：
  - `dotnet build AnsEngine.sln --nologo -v minimal`
  - `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`
- 重点核验场景：
  - valid `RigidBody` / `BoxCollider` JSON loads and round-trips
  - `PhysicsWorldDefinition` enters `PhysicsWorld` path
  - optional realistic SceneData fixture enters test-only adapter, then `PhysicsWorldDefinition -> PhysicsWorld`
  - body count / ids / names / body types stable
  - AABB rules follow pinned center/size/absolute-scale behavior
  - ground query detects above/intersecting/below ground without mutating body state
  - `Step(...)` updates only fixed-step statistics, not Scene Transform
- 单独做边界复验，确认：
  - `Engine.Physics` 不引用 `SceneData/Contracts/Core/Scene/App/Render/Scripting/Editor/Editor.App`
  - `Engine.SceneData` 不引用 `Engine.Physics`
  - `Engine.Scene` / `Engine.Render` / `Engine.Scripting` 不提及 Physics
  - M19 未引入 Transform writeback、App main loop integration 或 visible runtime physics
- 最后产出 `CodeQuality`、`DesignQuality`、风险摘要和归档证据缺口。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中修功能、改 physics schema 或改 world query 语义。
- 不允许跳过 `Engine.SceneData.Tests`、`Engine.Physics.Tests` 和边界检查。
- 不允许用 headless build/test 成功替代 `PhysicsWorldDefinition` 进入 PhysicsWorld 的证据。
- 不允许把 M20 能力缺失误报为 M19 缺陷。

## 失败与降级策略（FallbackBehavior）
- 若 Build/Test/Smoke/Perf 任一门禁失败，本卡不得进入 `Review`，必须记录失败证据并回退。
- 若发现任何模块通过偷引 Physics 依赖、写回 Transform 或接入 App loop 来通过测试，必须判定为高风险边界失败。
- 若发现 `PhysicsWorldDefinition` 没有真正进入 `PhysicsWorld` 主路径，必须判定为 must-fix，不得口头接受。
- 若存在 SceneData fixture 证据，该证据必须通过测试侧 adapter；若 production Physics 直接依赖 SceneData，必须判定为高风险边界失败。
- 若 `MustFixCount > 0`，必须保持 `MustFixDisposition=follow-up-created` 或回退原卡，不得直接放行。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Physics/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scripting/**`
- 相关测试入口：
  - `tests/Engine.Physics.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Render.Tests/**`
  - `tests/Engine.Scripting.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-phys-001.md`
  - `.ai-workflow/tasks/task-sdata-009.md`
  - `.ai-workflow/tasks/task-phys-002.md`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M19-2026-05-03.md`
- 计划结构引用：
  - `PLAN-M19-2026-05-03 > RuntimeRealityCheck`
  - `PLAN-M19-2026-05-03 > Boundary Rules`
  - `PLAN-M19-2026-05-03 > AABB Rules`
  - `PLAN-M19-2026-05-03 > TestPlan`
  - `PLAN-M19-2026-05-03 > Scope`

## 范围（Scope）
- AllowedModules:
  - Engine.Physics
  - Engine.SceneData
  - Engine.Scene
  - Engine.App
  - Engine.Render
  - Engine.Scripting
- AllowedFiles:
  - QA 只读验证证据
  - 任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不执行 `Review -> Done`
- 不新增 Transform writeback、App fixed-step、gravity、solver、visible runtime physics
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.Tests/**`
  - `tests/Engine.Editor.App.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若部分 smoke 证据主要来自 realistic fixture 而非独立 sample scene，需要在 QA 结论中明确证据口径，但不能降低“真实数据流”要求。
- 处理规则：
  - 若问题影响门禁结论、must-fix 归属或是否可以关闭 M19，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - Physics foundation 的真实路径、边界风险、非范围和 must-fix 判定标准都已下沉到卡面。
  - 执行者无需回看里程碑全文也能完成 M19 的全链路 QA 复验。
  - 失败回退与质量结论要求已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - M19 同时涉及新模块、新 schema、definition 主路径、fixture adapter 证据与多个逆向边界，QA 需要同时验证行为、边界和“未滑入 M20”。
  - 若只看 build/test 绿灯，非常容易误判 foundation 已稳。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.SceneData -> Engine.Contracts`
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Scripting`
  - `Engine.Render -> Engine.Contracts`
  - `Engine.Scripting -> Engine.Core`
  - `Engine.Scripting -> Engine.Contracts`
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
  - `Engine.SceneData -> Engine.Physics`
  - `Engine.Scene -> Engine.Physics`
  - `Engine.Render -> Engine.Physics`
  - `Engine.Scripting -> Engine.Physics`

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
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Physics / SceneData 相关测试无退化
- Smoke: `PhysicsWorldDefinition` 能进入 `PhysicsWorld -> Step -> Snapshot/Query` 主路径；如使用 realistic SceneData fixture，必须经测试侧 adapter；无 visible runtime physics claim
- Perf: 无逐帧 JSON 解析、App loop 接线、Transform writeback 或渲染 side path
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-020.md`
- ClosedAt: `2026-05-04 21:26`
- Summary:
  - Rechecked M19 implementation cards, allowed paths, boundary docs, and archive evidence.
  - Ran full build and full test suite.
  - Verified `PhysicsWorldDefinition -> PhysicsWorld -> Step -> Snapshot/Query` evidence and test-only SceneData adapter constraint.
  - Confirmed no Transform writeback, App loop integration, gravity, solver, or visible runtime physics claim.
- FilesChanged:
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-qa-020.md`
  - `.ai-workflow/archive/2026-05/TASK-QA-020.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`，235/235 passed）
  - Smoke: pass（PhysicsWorldDefinition main path and realistic SceneData fixture through test-only adapter verified）
  - Perf: pass（no per-frame JSON parsing, App loop integration, Transform writeback, gravity/solver, or render side path）
  - CodeQuality: NoNewHighRisk=true, MustFixCount=0, MustFixDisposition=none
  - DesignQuality: DQ-1=pass, DQ-2=pass, DQ-3=pass, DQ-4=pass
- ModuleAttributionCheck: pass
