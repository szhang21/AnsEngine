# 任务: TASK-SCENE-023 M22 MeshRenderer component container migration

## TaskId
`TASK-SCENE-023`

## 目标（Goal）
让 `SceneMeshRendererComponent` 实现 `IRuntimeComponent` 并迁入 `SceneRuntimeObject` 的 runtime component container，同时保留 `MeshRenderer` 兼容访问器由 typed lookup 驱动，确保 render frame 输出与 M21 行为一致。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M22-2026-05-11`

## 里程碑引用（兼容别名：MilestoneRef）
`M22.4`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P1

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
- M22.4 将 MeshRenderer 作为第一个普通 runtime component 迁入 generic component container。
- 本卡验证 component container 的真实使用路径，同时要求 render output 与 M21 保持一致。
- 上游直接影响本卡的背景包括：Transform 仍是独立 core spatial field，render frame 仍从 `Transform + MeshRenderer` 输出，Render 不引用 runtime abstractions。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `SceneMeshRendererComponent` implements `IRuntimeComponent`。
  - Runtime object construction places MeshRenderer in the component container。
  - Keep `MeshRenderer` compatibility accessor backed by `GetComponent<SceneMeshRendererComponent>()`。
  - `BuildRenderItems` continues to emit render items from `Transform + MeshRenderer`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许让 `Engine.Render` reference `Engine.Runtime.Abstractions`。
  - 不允许把 Transform 迁入 component container。
  - 不允许改变 transform-only object 不输出 render item 的语义。
- 计划结构约定：
  - Compatibility property shape: `public SceneMeshRendererComponent? MeshRenderer => GetComponent<SceneMeshRendererComponent>();`

## 实施说明（ImplementationNotes）
- 修改 `SceneMeshRendererComponent` 实现 `IRuntimeComponent`。
- 调整 `SceneRuntimeObject` construction / factory / load path，使 MeshRenderer 存入 runtime component container。
- 保留 `SceneRuntimeObject.MeshRenderer` 兼容属性，但让它从 typed lookup 返回结果。
- 检查 `RuntimeScene.BuildRenderItems` 或等价 render frame 生产路径，确保仍使用 Transform + MeshRenderer 输出 `SceneRenderItem`。
- 补测试覆盖：`GetComponent<SceneMeshRendererComponent>()` returns registered MeshRenderer；transform-only object emits no render item；multi-object render order stable；renderable object output matches M21。

## 设计约束（DesignConstraints）
- 不允许修改 Render 公开契约或 Render 项目依赖。
- 不允许把 MeshRenderer 存双份导致兼容属性和 typed lookup 不一致。
- 不允许把 RigidBody、BoxCollider、Script 或 Camera 同步迁入 runtime component container。
- 新增 C# 代码遵守 Engine 编码规范和一类一文件约定。

## 失败与降级策略（FallbackBehavior）
- 如果 render output 发生差异，必须回退迁移或补兼容，不得把行为变化推给 QA。
- 如果 component lookup 与 compatibility property 不一致，必须以 single source of truth 修复。
- 如果迁移诱发 Render 或 SceneData 对 Runtime.Abstractions 的依赖，必须回退。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/Runtime/SceneMeshRendererComponent.cs`
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
  - `tests/Engine.Render.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-018.md`
  - `.ai-workflow/tasks/task-scene-021.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-render.md`
- 计划结构引用：
  - `PLAN-M22-2026-05-11 > Milestones > M22.4`
  - `PLAN-M22-2026-05-11 > Runtime Data Flow`
  - `PLAN-M22-2026-05-11 > Engine.Scene Runtime Object Model`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - MeshRenderer runtime component implementation、Scene render item production tests、Scene boundary update
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 Render module
- 不修改 SceneData schema
- 不迁移 Physics/Script/Camera components
- 不改变 Transform ownership
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Physics/**`
  - `src/Engine.Scripting/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 内部 registration 入口由 Execution 根据 `SceneRuntimeObject` 构造方式选择，但不得暴露 public mutation API。
- 处理规则：
  - 若迁移需要 public add/remove component API，必须回退修卡。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确 MeshRenderer 的 container 迁移、compatibility accessor、render output 不变和禁止扩张对象。
  - 测试入口覆盖 typed lookup 与 render behavior。
  - 执行者无需回看计划全文即可避免 Render 依赖 Runtime.Abstractions。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - MeshRenderer 是 render 主路径的一部分，迁移容器但行为必须完全兼容。
  - 容易误改 Render 依赖或引入双重所有权。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Runtime.Abstractions`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
  - `Engine.Scene -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Runtime.Abstractions`
  - `Engine.SceneData -> Engine.Runtime.Abstractions`
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.Scripting`
  - `Engine.Scene -> Engine.Physics`

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
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Scene render and component lookup tests 通过
- Smoke: Renderable object output matches M21；Transform-only object emits no render item；multi-object render order stable；Render 不引用 runtime abstractions
- Perf: MeshRenderer lookup 不引入明显 render frame 构建退化说明

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Done

## 完成度（Completion）
100

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-023.md`
- ClosedAt: `2026-05-11`
- Summary:
  - `SceneMeshRendererComponent` now implements `IRuntimeComponent`.
  - `SceneRuntimeObject` registers MeshRenderer in the runtime component container.
  - `MeshRenderer` compatibility accessor is backed by typed component lookup.
  - Render output semantics and Render/SceneData dependencies remain unchanged.
- FilesChanged:
  - `src/Engine.Scene/Runtime/SceneMeshRendererComponent.cs`
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-023.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-023.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
  - Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed, including `Engine.Scene.Tests` 62/62)
  - Focused Test: pass (`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`; 62/62)
  - Smoke: pass (MeshRenderer typed lookup, Transform-only no render item, multi-object render order, and M21 render output compatibility covered by scene tests)
  - Boundary: pass (`Engine.Render` and `Engine.SceneData` do not reference `Engine.Runtime.Abstractions`)
  - Perf: pass (MeshRenderer is registered once at object construction and lookup is bounded to the internal component collection)
- ModuleAttributionCheck: `pass`
