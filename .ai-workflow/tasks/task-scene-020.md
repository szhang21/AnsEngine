# 任务: TASK-SCENE-020 M20 Scene Transform writeback contract

## TaskId
`TASK-SCENE-020`

## 目标（Goal）
让 `Engine.Scene` 暴露通用的按 `objectId` 写回 Transform 的 runtime API，并保证写回后的 runtime snapshot 与 render frame 都能观察到最终 Transform，同时保持 `Engine.Scene` 不依赖 `Engine.Physics`。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M20-2026-05-04`

## 里程碑引用（兼容别名：MilestoneRef）
`M20.1`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Contracts
- Engine.Core
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M20-G1`
- CanRunParallel: `true`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M20.1 是 Physics Runtime Collision MVP 的 Scene 侧前置地基；没有一个通用、显式失败的 Transform 写回 API，App 无法把 physics resolve 后的最终位置安全写回 runtime scene。
- 本卡承担的是 Scene 通用 writeback contract、本地 runtime object Transform 更新和可观察性验证，不承担 Physics 依赖、App bridge 或碰撞求解。
- 直接影响本卡实现的上游背景包括：Scene writeback 必须是 generic runtime transform capability，不允许命名或职责上绑死 Physics；M20 保持 `Engine.Scene` 不引用 `Engine.Physics`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 推荐 API：
    - `SceneTransformWriteResult TrySetObjectTransform(string objectId, SceneTransform transform)` 或等价显式结果
  - 失败 kind 至少覆盖：
    - object not found
    - missing Transform
    - invalid Transform
  - 写回成功后 runtime snapshot 与 render frame 都应观察到新 Transform。
  - 写回只影响目标对象，不影响其他对象。
- 本卡执行时不得推翻的既定取舍：
  - 不允许 `Engine.Scene` 引入 `Engine.Physics` 依赖。
  - 不允许把 writeback API 命名成 physics-specific 接口或类型。
  - 不允许把 missing object / missing Transform 做成 silent no-op。
  - 不允许通过 `BuildRenderFrame()` 的临时覆盖而不是 runtime state 更新来伪造“写回成功”。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M20-2026-05-04 > Scene Writeback Contract` 已定稿 API 形状方向、使用 `Engine.Contracts.SceneTransform` 作为外部值类型，以及 invalid transform failure 语义。
  - `PLAN-M20-2026-05-04 > PlanningDecisions` 已定稿 Scene writeback 是通用能力，未来 Animation/Play Mode 可复用，执行时不得做成仅 Physics 可调用的特例桥。

## 实施说明（ImplementationNotes）
- 先在 `RuntimeScene` 或等价 runtime owner 中增加按 `objectId` 更新现有 Transform 的能力。
- 再在 `SceneGraphService` 暴露通用 writeback API，并保持其外部使用 `SceneTransform`、内部更新 runtime transform component。
- 然后补显式失败结果类型与 failure kinds，覆盖 missing object、missing Transform、invalid transform。
- 最后补 Scene tests，至少覆盖：
  - writeback success only affects target object
  - writeback reflected in runtime snapshot
  - writeback reflected in render frame
  - missing object / missing Transform / invalid transform fail explicitly
  - `Engine.Scene` 未新增 Physics 依赖

## 设计约束（DesignConstraints）
- 不允许在本卡新增 Physics-specific type、bridge 或 `Engine.Scene -> Engine.Physics` 依赖。
- 不允许顺手加入动态对象创建/销毁支持、批量 writeback 或 world transform solver。
- 不允许改变既有 script self-transform bridge 语义。
- 不允许扩大 render contract，只允许通过 Scene 现有 runtime state 让 render 观察到写回结果。

## 失败与降级策略（FallbackBehavior）
- 若 write result 类型命名需按仓库风格微调，允许等价命名，但必须保持显式 success/failure 而不是布尔返回加日志。
- 若 invalid transform 现有项目约定只校验 finite 值，则至少保持 non-finite failure；若已有 quaternion 更严格约定，应沿用并在测试中钉死。
- 若实现中发现只有引入 `Engine.Physics` 类型才能继续，必须停工回退。
- 若 render frame 无法观察写回结果，不得先以“snapshot 正确即可”降格通过。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/**`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `src/Engine.App/SceneRuntimeContracts.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-019.md`
  - `.ai-workflow/tasks/task-scene-018.md`
  - `.ai-workflow/tasks/task-phys-002.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M20-2026-05-04.md`
- 计划结构引用：
  - `PLAN-M20-2026-05-04 > Milestones > M20.1`
  - `PLAN-M20-2026-05-04 > Scene Writeback Contract`
  - `PLAN-M20-2026-05-04 > Runtime Data Flow`
  - `PLAN-M20-2026-05-04 > TestPlan`
  - 上述 writeback contract 引用属于“参考实现约束”，API 方向、失败语义和 render/snapshot 可观察性已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - runtime transform writeback API
  - write result/failure types
  - Scene tests
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Physics bridge
- 不实现 App 调用顺序或 writeback 调度
- 不实现碰撞求解
- 不引入 `Engine.Scene -> Engine.Physics`
- OutOfScopePaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - write result / failure type 的具体命名可按现有 Scene 风格调整，但必须保持显式 failure kinds。
- 处理规则：
  - 若问题影响 Scene 泛化能力、失败语义或 Physics 零依赖边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - API 方向、显式失败要求、可观察性目标和边界禁令都已落卡。
  - 执行者无需回看计划全文也能知道这是通用 Scene writeback 能力，而不是 Physics 特供接口。
  - 停工条件和非范围明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡直接决定 M20 是否能在不破坏 Scene/Physics 边界的前提下完成最终位置写回。
  - 若写短，很容易做成 silent no-op 或 physics-specific side door。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Physics`
  - `Engine.Scene -> Engine.App`
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.Scripting`
  - `Engine.Scene -> Engine.Platform`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 writeback success/failure 与 render/snapshot observability
- Smoke: 写回后 render frame 和 runtime snapshot 都看到最终 Transform；缺失对象/Transform 不 silent no-op
- Perf: 不引入逐帧集合扫描、跨模块回调或 render side effect

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
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-020.md`
- ClosedAt: `2026-05-04 23:55`
- Summary:
  - Added generic Scene transform writeback API using `Engine.Contracts.SceneTransform`.
  - Added explicit write failure kinds for object not found, missing Transform, and invalid Transform.
  - Verified successful writeback is reflected in runtime snapshot and render frame while affecting only the target object.
  - Confirmed `Engine.Scene` still has no `Engine.Physics` dependency.
- FilesChanged:
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Scene/Runtime/SceneTransformWriteFailureKind.cs`
  - `src/Engine.Scene/Runtime/SceneTransformWriteFailure.cs`
  - `src/Engine.Scene/Runtime/SceneTransformWriteResult.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-020.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-020.md`
  - `.ai-workflow/archive/archive-index.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`，57/57 passed）
  - Smoke: pass（writeback 后 runtime snapshot 与 render frame 均观察到最终 Transform；缺失对象/Transform 显式失败且非 silent no-op）
  - Perf: pass（只在显式 writeback 调用中按 objectId 查找目标对象；未引入逐帧集合扫描、跨模块回调或 render side effect）
- ModuleAttributionCheck: pass
