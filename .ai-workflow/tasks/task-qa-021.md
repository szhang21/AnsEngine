# 任务: TASK-QA-021 M20 Physics Runtime Collision MVP gate review and archive

## TaskId
`TASK-QA-021`

## 目标（Goal）
对 M20 Physics Runtime Collision MVP 执行全量 build/test/smoke 与边界复验，确认 `script movement -> physics resolve -> scene writeback -> render` 主链路稳定，并且没有滑入 Dynamic Physics 或 Editor UI。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M20-2026-05-04`

## 里程碑引用（兼容别名：MilestoneRef）
`M20.5`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Physics
- Engine.SceneData
- Engine.Render
- Engine.Scripting

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M20-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-020`
  - `TASK-APP-020`
  - `TASK-PHYS-003`
  - `TASK-APP-021`

## 里程碑上下文（MilestoneContext）
- M20.5 是 Physics Runtime Collision MVP 的关闭门禁，本卡不再新增能力，而是确认 runtime collision 主链路、writeback、render 可观察性和边界方向都已稳定落地。
- 本卡承担的是 build/test/smoke、边界复验、质量结论和归档准备，不承担功能实现。
- 直接影响本卡实现的上游背景包括：M20 只做 static collision + kinematic movement；Dynamic gravity/solver 留给 M21+；Scene 仍不得依赖 Physics，Physics 仍不得依赖任何 Engine 模块。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - runtime 主链路必须是：
    - SceneData load
    - Scene init
    - App production Physics bridge
    - Script update
    - Physics resolve
    - Scene writeback
    - Render observes resolved transform
  - Physics 是本帧最终位置约束者。
  - scene without physics preserves existing movement behavior.
  - M20 不做 Dynamic gravity / velocity / force / impulse / solver / Editor UI。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 QA 卡中补实现、偷偷扩大到动态物理或编辑器功能。
  - 不允许把 headless 某条测试通过当成全部主链路与边界都通过。
  - 不允许忽视 `Engine.Scene -> Engine.Physics` 禁令或 `Engine.Physics` 零 Engine 依赖约束。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M20-2026-05-04 > Runtime Data Flow`、`Kinematic Collision Model`、`Failure And Fallback`、`TestPlan` 是本卡判定是否按计划落地的固定参照。
  - `PLAN-M20-2026-05-04 > Scope > Out of scope` 是本卡判断是否越界的重要硬约束。

## 实施说明（ImplementationNotes）
- 先复核 `TASK-SCENE-020`、`TASK-APP-020`、`TASK-PHYS-003`、`TASK-APP-021` 的 `AllowedPaths`、边界文档更新和审批记录。
- 执行自动验证：
  - `dotnet build AnsEngine.sln --nologo -v minimal`
  - `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`
- 重点核验场景：
  - Scene writeback succeeds by object id and affects render/snapshot
  - production bridge maps real `SceneDescription` to `PhysicsWorldDefinition`
  - `MoveOnInput` cannot move through static collider
  - scene without physics preserves previous behavior
  - Script -> Physics -> Render order is stable
  - Physics-free and failure paths both remain deterministic
- 单独做边界复验，确认：
  - `Engine.Scene` 不引用 `Engine.Physics`
  - `Engine.Physics` 不引用任何 Engine 模块
  - `Engine.Render` / `Engine.Scripting` 不直接感知 Physics
  - M20 未引入 Dynamic gravity、solver、Editor UI
- 最后产出 `CodeQuality`、`DesignQuality`、风险摘要和归档证据缺口。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中修功能、改碰撞算法或改主循环顺序。
- 不允许跳过 `Engine.Scene.Tests`、`Engine.Physics.Tests`、`Engine.App.Tests` 与边界检查。
- 不允许把“render 位置被阻挡了”当成唯一证据，必须同时验证 ownership 与依赖方向。
- 不允许把未实现 Dynamic gravity/solver 判成 M20 缺陷。

## 失败与降级策略（FallbackBehavior）
- 若 Build/Test/Smoke/Perf 任一门禁失败，本卡不得进入 `Review`，必须记录失败证据并回退。
- 若发现任何模块通过 Scene-Physics 直接依赖、Physics 反向依赖 Engine 模块或动态物理 side path 来通过测试，必须判定为高风险边界失败。
- 若发现 physics-free scene 行为回归或 writeback failure 被吞掉，必须判定为 must-fix，不得口头接受。
- 若 `MustFixCount > 0`，必须保持 `MustFixDisposition=follow-up-created` 或回退原卡，不得直接放行。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Physics/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scripting/**`
- 相关测试入口：
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.Physics.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
  - `tests/Engine.Render.Tests/**`
  - `tests/Engine.Scripting.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-020.md`
  - `.ai-workflow/tasks/task-app-020.md`
  - `.ai-workflow/tasks/task-phys-003.md`
  - `.ai-workflow/tasks/task-app-021.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M20-2026-05-04.md`
- 计划结构引用：
  - `PLAN-M20-2026-05-04 > Milestones > M20.5`
  - `PLAN-M20-2026-05-04 > Runtime Data Flow`
  - `PLAN-M20-2026-05-04 > Kinematic Collision Model`
  - `PLAN-M20-2026-05-04 > Failure And Fallback`
  - `PLAN-M20-2026-05-04 > TestPlan`
  - `PLAN-M20-2026-05-04 > Scope`

## 范围（Scope）
- AllowedModules:
  - Engine.App
  - Engine.Scene
  - Engine.Physics
  - Engine.SceneData
  - Engine.Render
  - Engine.Scripting
- AllowedFiles:
  - QA 只读验证证据
  - 任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不执行 `Review -> Done`
- 不新增 Dynamic gravity、solver、Editor UI、Play Mode
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.Tests/**`
  - `tests/Engine.Editor.App.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若 headless smoke 主要用 sample scene 而非人工可视录屏，需要在 QA 结论中明确证据口径，但不能降低“resolved transform 被 render 观察到”的要求。
- 处理规则：
  - 若问题影响门禁结论、must-fix 归属或是否可以关闭 M20，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - M20 真实 runtime 主链路、边界风险、非目标和 must-fix 标准都已下沉。
  - 执行者无需回看计划全文也能完成全链路 QA 复验。
  - 失败回退和质量结论要求明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - M20 同时涉及 Scene、Physics、App、Render、Scripting 的帧内顺序和边界，QA 必须验证行为、所有权和不扩张条件同时成立。
  - 若只看某个测试点，极易误判 Dynamic Physics 或 ownership 问题已被正确处理。
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
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
  - .NET 标准库
  - `System.Numerics`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Physics`
  - `Engine.Physics -> Engine.Scene`
  - `Engine.Physics -> Engine.App`
  - `Engine.Physics -> Engine.Render`
  - `Engine.Physics -> Engine.Scripting`
  - `Engine.Physics -> Engine.SceneData`
  - `Engine.Render -> Engine.Physics`
  - `Engine.Scripting -> Engine.Physics`

## 边界变更请求（BoundaryChangeRequest）
- Required: `true`
- Status: `approved`
- RequestReason: `M20 的两张 App integration 卡需要 App 持有 Engine.Physics 依赖并在初始化/主循环中桥接 Physics runtime，当前 engine-app 边界合同尚未声明对 Engine.Physics 的允许依赖。`
- ImpactModules:
  - `Engine.App`
  - `Engine.Physics`
  - `Engine.Scene`
- HumanApprovalRef: `Human approved via “拆卡m20” on 2026-05-04`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-physics.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Scene/Physics/App 相关测试无退化
- Smoke: headless App run 证明 script movement -> physics resolve -> scene writeback -> render transform；无 Dynamic gravity/solver claim
- Perf: 无逐帧 world 重建、跨模块内部集合泄露或渲染 side path
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-021.md`
- ClosedAt: `2026-05-05 00:37`
- Summary:
  - Rechecked M20 implementation cards, allowed paths, boundaries, and archive evidence.
  - Ran full build/test and headless App smoke.
  - Confirmed `script movement -> physics resolve -> scene writeback -> render` main path is covered.
  - Confirmed physics-free scene movement is preserved and writeback failure is visible.
  - Confirmed M20 did not add Dynamic gravity, velocity, force, impulse, solver, Editor UI, Play Mode, or forbidden Scene/Physics dependencies.
- FilesChanged:
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-physics.md`
  - `.ai-workflow/tasks/task-qa-021.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/2026-05/TASK-QA-021.md`
  - `.ai-workflow/archive/archive-index.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`，254/254 passed）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.App/Engine.App.csproj --nologo`，exit 0；默认 scene 走 script movement -> physics resolve -> scene writeback -> render）
  - Boundary: pass（`Engine.Scene` / `Engine.Render` / `Engine.Scripting` / `Engine.SceneData` 未引用 `Engine.Physics`；`Engine.Physics` 未引用其他 Engine 模块；未发现 gravity/velocity/force/impulse/solver/Play Mode/Editor UI 实现）
  - Perf: pass（无逐帧 world 重建、跨模块内部集合泄露、逐帧 SceneData remap 或 render side path）
  - CodeQuality: NoNewHighRisk=true, MustFixCount=0, MustFixDisposition=none
  - DesignQuality: DQ-1=pass, DQ-2=pass, DQ-3=pass, DQ-4=pass
- ModuleAttributionCheck: pass
