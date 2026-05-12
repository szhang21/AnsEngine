# 任务: TASK-SCENE-021 M22 Scene runtime object implements abstractions

## TaskId
`TASK-SCENE-021`

## 目标（Goal）
让 `Engine.Scene` 引用 `Engine.Runtime.Abstractions`，使 `SceneRuntimeObject` 实现 `IRuntimeObject`，并新增 runtime component container 与 typed lookup，保持现有 object identity、NodeId、snapshot 和边界语义不变。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M22-2026-05-11`

## 里程碑引用（兼容别名：MilestoneRef）
`M22.2`

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
- ParallelGroup: `M22-G2`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-RABS-001`

## 里程碑上下文（MilestoneContext）
- M22.2 将 M14-M21 已有的 Scene concrete runtime object 对齐到新的共享 runtime abstraction。
- 本卡只建立 `SceneRuntimeObject` 的 `IRuntimeObject` 实现和 component container 基础，不迁移 Transform 或 MeshRenderer 语义。
- 上游直接影响本卡的背景包括：`Runtime.Abstractions` 只提供 `GetComponent<T>()` / `HasComponent<T>()` typed lookup；M22 禁止 add/remove/traversal/update API。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.Scene` may reference `Engine.Runtime.Abstractions`。
  - `SceneRuntimeObject` implements `IRuntimeObject`。
  - `SceneRuntimeObject` owns runtime component collection for non-core components and typed lookup。
  - Empty object component collection is stable，missing lookup returns `null`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许让 Scene 引用 `Engine.Scripting` 或 `Engine.Physics`。
  - 不允许把 Transform 放入 generic component collection；Transform alignment 留给 `TASK-SCENE-022`。
  - 不允许把 MeshRenderer 迁移进 container；迁移留给 `TASK-SCENE-023`。
- 计划结构约定：
  - `IRuntimeObject.GetComponent<T>()` 返回 `T?`，`HasComponent<T>()` 与 lookup 一致。

## 实施说明（ImplementationNotes）
- 在 `src/Engine.Scene/Engine.Scene.csproj` 新增对 `Engine.Runtime.Abstractions` 的项目引用。
- 修改 `SceneRuntimeObject` 实现 `IRuntimeObject`，公开 `ObjectId`、`ObjectName` 与 typed component lookup。
- 新增内部 component collection 存储非 core runtime components；本卡可先支持空集合和显式注册路径，但不得暴露 add/remove public API。
- 补 `Engine.Scene.Tests` 覆盖空集合、missing component returns null、`HasComponent<T>()` 与 `GetComponent<T>()` 一致、object identity/snapshot/NodeId 语义不变。
- 更新 Scene 边界合同，记录允许依赖 `Engine.Runtime.Abstractions`，同时确认仍禁止 Scripting/Physics 直接依赖。

## 设计约束（DesignConstraints）
- 不允许新增 public `AddComponent` / `RemoveComponent` / `FindObject` / traversal API。
- 不允许改变 `RuntimeSceneSnapshot` 与 render frame 的既有可观察输出。
- 不允许把 component collection 泄露为可变集合。
- 新增 C# 代码遵守 Engine 编码规范和一类一文件约定。

## 失败与降级策略（FallbackBehavior）
- 如果 generic lookup 需要公开 mutation API 才能测试，必须改用内部构造路径或测试 fixture，不得扩展 public API。
- 如果引入 Runtime.Abstractions 后边界测试发现 Scene 依赖 Scripting/Physics，必须回退。
- 如果 object identity、NodeId 或 snapshot 行为变化，视为主路径回归，不得进入 Review。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Scene/Runtime/RuntimeSceneSnapshot.cs`
  - `src/Engine.Scene/Engine.Scene.csproj`
  - `src/Engine.Runtime.Abstractions/**`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
  - `tests/Engine.Runtime.Abstractions.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-rabs-001.md`
  - `.ai-workflow/tasks/task-scene-010.md`
  - `.ai-workflow/tasks/task-scene-014.md`
  - `.ai-workflow/boundaries/engine-scene.md`
- 计划结构引用：
  - `PLAN-M22-2026-05-11 > Milestones > M22.2`
  - `PLAN-M22-2026-05-11 > Engine.Scene Runtime Object Model`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - Scene runtime object abstraction implementation、component lookup tests、Scene boundary update
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Transform contract alignment
- 不迁移 MeshRenderer 到 component container
- 不修改 Scripting 或 App bridge
- 不新增 traversal/update/mutation API
- OutOfScopePaths:
  - `src/Engine.Scripting/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - component collection 的内部数据结构可由 Execution 选择，但必须保持 typed lookup 和只读外部语义。
- 处理规则：
  - 若数据结构选择影响 public API 或边界，必须回退修卡。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确依赖、接口实现、component lookup 范围、禁止 API 和测试口径。
  - 与 Transform/MeshRenderer/Scripting 的后续职责已拆开。
  - 执行者无需回看计划全文即可实施。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - Scene runtime object 是后续 Transform、MeshRenderer、Scripting 对齐的核心状态点。
  - 若 public mutation/traversal API 或边界依赖做错，会破坏 M22 抽象层目标。
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
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Scene component lookup / boundary tests 通过
- Smoke: empty component collection stable；missing component lookup returns null；ObjectId/ObjectName/NodeId/snapshot 语义不变
- Perf: component lookup 不引入逐帧分配热点或 render path 明显退化说明

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-021.md`
- ClosedAt: `2026-05-11`
- Summary:
  - Added `Engine.Scene -> Engine.Runtime.Abstractions` project reference.
  - Made `SceneRuntimeObject` implement `IRuntimeObject` while preserving NodeId, ObjectId, ObjectName, snapshot and render behavior.
  - Added internal non-core runtime component collection and typed lookup tests.
  - Updated Scene boundary contract for the new allowed dependency.
- FilesChanged:
  - `src/Engine.Scene/Engine.Scene.csproj`
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj`
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-021.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-021.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
  - Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed, including `Engine.Scene.Tests` 59/59)
  - Focused Test: pass (`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`; 59/59)
  - Smoke: pass (empty component collection stable; missing typed lookup returns null; `HasComponent<T>()` matches lookup; identity/snapshot/render semantics unchanged)
  - Boundary: pass (`Engine.Scene` now references Runtime.Abstractions and still has no Scripting/Physics/App/Render dependency)
  - Perf: pass (typed lookup scans an internal immutable small component list and is not on render path)
- ModuleAttributionCheck: pass
