# 任务: TASK-SCENE-022 M22 Transform contract alignment

## TaskId
`TASK-SCENE-022`

## 目标（Goal）
让 `SceneTransformComponent` 实现 `IRuntimeTransformComponent`，同时保持 `SceneRuntimeObject.Transform` 作为独立 core spatial field，并确保 script self-transform、physics writeback、snapshot 和 render frame 行为不变。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M22-2026-05-11`

## 里程碑引用（兼容别名：MilestoneRef）
`M22.3`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Runtime.Abstractions
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M22-G3`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-SCENE-021`

## 里程碑上下文（MilestoneContext）
- M22.3 对齐 Transform runtime abstraction，但 M22 明确要求 Transform 继续是对象空间核心字段，不进入 generic component collection。
- 本卡只让 concrete `SceneTransformComponent` 实现 `IRuntimeTransformComponent`，并验证已有 script/physics/render 主路径不变。
- 上游直接影响本卡的背景包括：`IRuntimeTransformComponent.LocalTransform` 与 `SetLocalTransform(SceneTransform)` 形状已由 `TASK-RABS-001` 定稿。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `SceneTransformComponent` implements `IRuntimeTransformComponent`。
  - `SceneRuntimeObject.Transform` remains an independent core spatial field。
  - `Transform` is not stored in generic component collection in M22。
  - `TrySetObjectTransform`、`BindScriptObject`、snapshot、render frame behavior 必须走同一个 Transform instance。
- 本卡执行时不得推翻的既定取舍：
  - 不允许 Transform double ownership。
  - 不允许在 M22 引入 world transform、parent/children 或 scene traversal。
  - 不允许修改 physics writeback 的显式失败语义。

## 实施说明（ImplementationNotes）
- 修改 `SceneTransformComponent` 实现 `IRuntimeTransformComponent`，保持现有 `LocalTransform` get/set 语义。
- 检查 `SceneRuntimeObject.Transform`、`SceneScriptObjectHandle`、`RuntimeScene.TrySetObjectTransform`、snapshot/render frame 读取路径，确保它们仍指向同一 Transform component。
- 补测试覆盖：missing Transform failure semantics unchanged；script self-transform modification unchanged；physics writeback via `TrySetObjectTransform` unchanged；Transform-only object remains valid。
- 更新 Scene 边界合同变更记录，明确 Transform 对齐 runtime abstraction 但不进入 generic container。

## 设计约束（DesignConstraints）
- 不允许把 Transform 注册进 runtime component collection。
- 不允许新增 `GetComponent<IRuntimeTransformComponent>()` 必然返回 Transform 的语义，除非计划另行批准；M22 的 Transform 仍经 core property 访问。
- 不允许改动 `Engine.Scripting` 接口；Scripting 对齐由 `TASK-SCRIPT-004` 完成。
- 新增 C# 代码遵守 Engine 编码规范和一类一文件约定。

## 失败与降级策略（FallbackBehavior）
- 如果 Transform 对齐导致 script 或 physics tests 失败，必须回退本卡实现，不得修改那些主路径验收标准。
- 如果发现需要 Transform 进入 generic lookup 才能满足 Scripting，必须回退修卡，因为计划明确禁止 double ownership。
- 如果 missing Transform 语义变化，必须保持原显式失败结果，不得静默创建 Transform。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `src/Engine.Scene/Runtime/SceneScriptObjectHandle.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Runtime.Abstractions/**`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
  - `tests/Engine.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-020.md`
  - `.ai-workflow/tasks/task-script-002.md`
  - `.ai-workflow/tasks/task-scene-021.md`
  - `.ai-workflow/boundaries/engine-scene.md`
- 计划结构引用：
  - `PLAN-M22-2026-05-11 > Engine.Scene Runtime Object Model`
  - `PLAN-M22-2026-05-11 > Milestones > M22.3`
  - `PLAN-M22-2026-05-11 > PlanningDecisions > Keep Transform as core spatial field`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - Scene Transform component abstraction implementation、Scene tests、Scene boundary update
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不迁移 MeshRenderer
- 不修改 Scripting public API
- 不把 Transform 放入 generic component collection
- 不新增 hierarchy/world transform/traversal API
- OutOfScopePaths:
  - `src/Engine.Scripting/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Physics/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 是否让 `SceneTransformComponent` explicit interface implement 可由 Execution 决定，但 public behavior 必须保持兼容。
- 处理规则：
  - 若实现方式影响现有 `SceneScriptObjectHandle` 或 `TrySetObjectTransform` 调用者，必须在本卡内修正并测试，不得留给后续卡。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确 Transform abstraction 形状、独立字段决策、主路径不变要求和测试口径。
  - 与 MeshRenderer/Scripting 的后续工作已拆开。
  - 执行者无需回看计划全文即可避免 double ownership。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - Transform 被 scripting、physics writeback、render snapshot 共同使用，任何 ownership 变化都可能破坏主路径。
  - 需要严格验证行为不变而不是只让接口编译通过。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Runtime.Abstractions`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
  - `Engine.Scene -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Scripting`
  - `Engine.Scene -> Engine.Physics`
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Transform/script/physics writeback/render snapshot 相关回归通过
- Smoke: missing Transform failure semantics、script self-transform 修改、physics writeback、Transform-only object 行为均不变
- Perf: Transform abstraction 不引入额外 runtime allocation 或 render path 明显退化说明

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-022.md`
- ClosedAt: `2026-05-11`
- Summary:
  - Made `SceneTransformComponent` implement `IRuntimeTransformComponent`.
  - Added `LocalTransform` and `SetLocalTransform(SceneTransform)` while preserving existing vector/quaternion/scale setter.
  - Updated script self-transform and scene writeback paths to use the same Transform instance through the new contract-shaped setter.
  - Confirmed Transform remains a core `SceneRuntimeObject.Transform` field and does not enter generic component lookup.
- FilesChanged:
  - `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
  - `src/Engine.Scene/Runtime/SceneScriptObjectHandle.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-022.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-022.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
  - Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed, including `Engine.Scene.Tests` 61/61)
  - Focused Test: pass (`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`; 61/61)
  - Smoke: pass (missing Transform failure semantics, script self-transform, `TrySetObjectTransform`, Transform-only object, snapshot and render behavior unchanged)
  - Boundary: pass (Transform aligns to Runtime.Abstractions while remaining core field; no new forbidden Scene dependencies)
  - Perf: pass (interface implementation adds no extra allocation or render path work)
- ModuleAttributionCheck: pass
