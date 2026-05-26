# 任务: TASK-SCENE-024 M24 Transform component container alignment

## 目标（Goal）
让 `SceneRuntimeObject.GetComponent<IRuntimeTransformComponent>()` 返回真实 `SceneTransformComponent`，并保证 convenience property、snapshot、render frame 共用同一 Transform 实例。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE`

## 里程碑引用（兼容别名：MilestoneRef）
`M24.2`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- tests

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M24-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-RABS-002`

## 里程碑上下文（MilestoneContext）
- M22 已让 `SceneRuntimeObject` 实现 `IRuntimeObject`，但 Transform 仍是特殊 core field，未进入 typed component lookup。
- M24 要让 scripts 和 runtime update components 通过 `context.Owner.GetComponent<IRuntimeTransformComponent>()` 修改自身 Transform，因此 Transform 必须成为真实 component lookup result。
- 本卡只对齐 Scene component container，不引入 scripting、Engine.Runtime scheduler 或 physics 行为。

## 决策继承（DecisionCarryOver）
- 继承决策：
  - `SceneRuntimeObject.Transform` 可作为迁移期 convenience property 保留。
  - convenience property 和 `GetComponent<IRuntimeTransformComponent>()` 必须引用同一个 `SceneTransformComponent` 实例。
  - snapshot 和 render item construction 必须读取该同一实例。
- 不得推翻的既定取舍：
  - `Engine.Scene` 可以依赖 `Engine.Runtime.Abstractions`，但不得依赖 `Engine.Scripting`、`Engine.Runtime`、`Engine.Physics`、`Engine.App`。
  - 不开放 public add/remove component mutation API，不做 hierarchy/world transform redesign。
- 上游已定结构约束：
  - Transform 进入 `SceneRuntimeObject` component collection。
  - MeshRenderer 仍保持已迁入 component container 的现有语义。
  - Transform-only object 仍可 snapshot，但不进入 render frame，除非已有 mesh renderer。

## 实施说明（ImplementationNotes）
- 定位 `SceneRuntimeObject` 当前 Transform field 与 component collection 初始化点，把 `SceneTransformComponent` 注册为 typed component。
- 确保 `Transform` property 返回 collection 内同一个实例，而不是另一个字段副本。
- 检查 `RuntimeSceneSnapshot`、`SceneRuntimeObjectSnapshot`、`BuildRenderFrame` 路径，保证读取 component-updated transform。
- 更新或移除旧测试中“Transform 不在 typed lookup”之类断言，新增 instance identity 与 render/snapshot observability 测试。
- 更新 `.ai-workflow/boundaries/engine-scene.md` 变更日志，说明 Transform 从 core field 迁移为真实 runtime component lookup result。

## 设计约束（DesignConstraints）
- 不允许引入 `Engine.Scene -> Engine.Scripting` 或 `Engine.Scene -> Engine.Runtime` 依赖。
- 不允许维护两个 Transform 状态源。
- 不允许把 script binding、script update 或 runtime scheduler 放进 Scene。
- 不允许改变 SceneData schema、Render contract 或 MeshRenderer render semantics。

## 失败与降级策略（FallbackBehavior）
- 若发现 Transform 无法安全复用同一实例，必须回退修卡或先补小范围重构，不得临时双写两个状态源。
- 若 render/snapshot 观察到不同 Transform，门禁失败并回退到 `InProgress`。
- 若需要新增 public mutation/traversal API 才能完成目标，立即停工回退 Dispatch/Plan。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
- 相关文档/任务：
  - `.ai-workflow/plan-archive/2026-05/PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE.md` 的 `Scene Component Container` 与 `M24.2 Transform Component Container Alignment`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `TASK-SCENE-021`、`TASK-SCENE-022`、`TASK-SCENE-023`
- 示例结构定位：
  - M24 `Scene Component Container` 小节中的同实例要求是强约束，不是实现建议。

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
  - tests
- AllowedFiles:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- Script behavior migration
- Engine.Runtime module creation
- Physics writeback changes
- SceneData schema changes
- Render contract redesign
- Public component mutation API
- OutOfScopePaths:
  - `src/Engine.Scripting/**`
  - `src/Engine.Runtime/**`
  - `src/Engine.App/**`
  - `src/Engine.Physics/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Render/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - none
- 处理规则：
  - 若现有 object construction 顺序导致同实例注册不可行，先回退修卡，不得双写绕过。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 上游已明确 Transform component container 对齐目标。
  - 本卡已列明同实例、依赖方向、snapshot/render 观察和非目标。
  - 执行者可只按本卡定位源码和测试入口。
- MissingInfo:
  - none

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Core`
  - `Engine.Contracts`
  - `Engine.Runtime.Abstractions`
  - `Engine.SceneData`
- ForbiddenDependsOn:
  - `Engine.Scripting`
  - `Engine.Runtime`
  - `Engine.Physics`
  - `Engine.App`
  - `Engine.Render`
  - `Engine.Asset`

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
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过。
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal` 通过，且覆盖 transform typed lookup、property/lookup 同实例、snapshot/render 读取同状态。
- Smoke: 现有 sample scene 仍能生成 render frame；Transform-only object snapshot 行为不退化。
- Perf: component lookup 不引入明显逐帧额外分配；记录无明显退化说明。
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
- Minimal patch
- Transform typed lookup implementation
- Scene tests for identity and observability
- Boundary change log if implementation changes component boundary wording
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 复杂度评估（ComplexityAssessment）
- Level: `L3`
- Why:
  - Transform 双状态源会直接破坏 runtime 正确性。
  - Scene 必须守住不依赖 Scripting/Runtime 的边界。
  - 验收需要同时覆盖 lookup、snapshot、render 三条路径。
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-024.md`
- ClosedAt: `2026-05-27`
- Summary: Registered `SceneTransformComponent` in `SceneRuntimeObject` runtime component lookup so the `Transform` convenience property, typed lookup, snapshot, and render frame all observe the same Transform instance.
- FilesChanged:
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL warnings; 0 errors.
  - Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal` passed; 63 passed, 0 failed.
  - Smoke: existing sample/render frame paths still compile and Scene tests cover transform-only snapshot without render item plus renderable object output.
  - Perf: component collection construction remains one-time object initialization; lookup remains a small linear scan with no per-frame allocation added.
  - CodeQuality: NoNewHighRisk=`true`; MustFixCount=`0`; MustFixDisposition=`none`.
  - DesignQuality: DQ-1=`pass`; DQ-2=`pass`; DQ-3=`pass`; DQ-4=`pass`.
- ModuleAttributionCheck: pass
