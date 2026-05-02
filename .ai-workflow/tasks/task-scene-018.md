# 任务: TASK-SCENE-018 M16 Scene runtime component bridge

## TaskId
`TASK-SCENE-018`

## 目标（Goal）
让 `RuntimeScene.LoadFromDescription(...)` 从 normalized component descriptions 构建 runtime components，并支持 Transform-only object 进入 runtime snapshot 但不进入 render frame。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M16-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M16.3`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Contracts
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M16-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SDATA-007`

## 里程碑上下文（MilestoneContext）
- M16.3 的职责是把新的 normalized component descriptions 真正桥接到 runtime object/component model，证明 M16 不只是文档 schema 改造，而是运行链路完成适配。
- 本卡承担 runtime bridge、Transform-only object 语义和 render frame 过滤，不承担 SceneData 文件/normalizer 规则，也不承担 editor component editing 或 QA 收口。
- 直接影响本卡实现的上游背景包括：`RuntimeScene` 已能持有 `SceneTransformComponent` 与 `SceneMeshRendererComponent`；`BuildRenderFrame()` 必须保持纯读取 runtime state；M15 update 仍只旋转第一个同时具备 Transform 和 MeshRenderer 的 object。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `RuntimeScene.LoadFromDescription(...)` 迭代 normalized component descriptions。
  - `Transform` 映射到 `SceneTransformComponent`。
  - `MeshRenderer` 映射到 `SceneMeshRendererComponent`。
  - Transform-only object：
    - runtime object 会创建
    - runtime snapshot 包含 object 与 transform
    - `HasMeshRenderer=false`
    - render frame 排除它
  - 带 Transform + MeshRenderer 的 object 仍输出 render item。
- 本卡执行时不得推翻的既定取舍：
  - runtime bridge 不读取 JSON，不感知 file schema 细节。
  - 不允许把 `BuildRenderFrame()` 改成推进 update 或生成补救性 component。
  - 不允许因为 Transform-only object 不渲染就跳过创建 runtime object。
  - 不允许修改 M15 默认旋转规则，让 Transform-only object 也被旋转。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M16-2026-05-02 > RuntimeBridgeDesign` 已定稿 Transform-only object 与 Transform+MeshRenderer object 的 runtime/render 语义。
  - `PLAN-M16-2026-05-02 > NormalizedModelDesign` 已定稿 Scene 侧消费的是 normalized component descriptions，而不是旧对象级 `Mesh/Material/LocalTransform` 字段。

## 实施说明（ImplementationNotes）
- 先调整 `RuntimeScene.LoadFromDescription(...)` 或等价入口，按 normalized component descriptions 构建 runtime components。
- 再确保 runtime snapshot 仍能暴露 Transform-only object 的 identity/transform/`HasMeshRenderer=false` 语义。
- 然后校准 `BuildRenderFrame()`：
  - 只输出同时具备 Transform 和 MeshRenderer 的 object
  - mesh/material/transform 输出保持与既有 render contract 对齐
- 最后补 Scene 测试，覆盖：
  - Transform-only object 在 snapshot 中存在
  - Transform-only object 不进入 render frame
  - 带 MeshRenderer 的 object 仍输出 render item
  - M15 update 仍只旋转第一个可渲染 object

## 设计约束（DesignConstraints）
- 不允许在本卡中引入 `Engine.Scene -> Engine.Editor`、`Engine.Scene -> Engine.Editor.App`、`Engine.Scene -> Engine.Render` 依赖。
- 不允许让 runtime bridge 直接依赖 file-model DTO 或 JSON serializer 类型。
- 不允许顺手扩到 Camera 组件化、脚本、物理、动画、Prefab 或 Play Mode。
- 不允许修改 `Engine.Contracts.SceneRenderFrame` 公开 contract。

## 失败与降级策略（FallbackBehavior）
- 若 normalized component collection 的遍历方式需要根据当前模型做小幅调整，允许局部实现差异，但最终必须仍由 Scene 内部 bridge 完成。
- 若某 object 缺少可渲染组件，应按 Transform-only 语义继续存在于 runtime snapshot，而不是报错或被删除；真正缺少 Transform 的失败应已在 SceneData 卡中被拦住。
- 若实现中发现只能通过让 Render 感知 component schema 才能完成过滤，必须停工回退。
- 若 update 行为因 bridge 迁移被迫修改对象选择规则，也必须回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/**`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `src/Engine.SceneData/**`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-012.md`
  - `.ai-workflow/tasks/task-scene-014.md`
  - `.ai-workflow/tasks/task-scene-016.md`
  - `.ai-workflow/tasks/task-scene-017.md`
  - `.ai-workflow/tasks/task-sdata-007.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M16-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M16-2026-05-02 > RuntimeBridgeDesign`
  - `PLAN-M16-2026-05-02 > NormalizedModelDesign`
  - `PLAN-M16-2026-05-02 > Milestones > M16.3`
  - `PLAN-M16-2026-05-02 > TestPlan`
  - 上述 runtime bridge 引用属于“参考实现约束”，不是可自由替换的方向建议。

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - runtime bridge
  - runtime snapshot / render filtering
  - Scene tests
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 SceneData file-model/serializer
- 不修改 editor session 或 inspector GUI
- 不修改 render public contract
- 不组件化 Camera
- OutOfScopePaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`
  - `src/Engine.Render/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - runtime bridge 的内部遍历/分派写法可随当前代码组织调整，但 Scene 仍只能消费 normalized descriptions，不得退回 file DTO。
- 处理规则：
  - 若问题影响 Transform-only object 语义、update 选择规则或 render filtering 口径，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - runtime bridge 目标、Transform-only object 语义和 update/render 边界都已写清。
  - 执行者无需回看里程碑也能知道本卡不能让 Render 感知 component schema。
  - 失败分流和非范围明确，便于稳定实施。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 M16 运行链路的关键桥接卡，若 bridge 选错位置或语义，runtime/update/render 全都会偏。
  - 需要同时守住 SceneData 边界、render filtering 和 M15 行为不回归。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.App`
  - `Engine.Scene -> Engine.Editor`
  - `Engine.Scene -> Engine.Editor.App`
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
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 Transform-only snapshot/render filtering 与 M15 update 不回归
- Smoke: Transform-only object 可进入 runtime snapshot 且 `HasMeshRenderer=false`；render frame 只输出可渲染 object；M15 默认旋转仍只作用于首个可渲染 object
- Perf: bridge 与 render filtering 不引入逐帧 JSON 读取、对象重建或 render side effect

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-018.md`
- ClosedAt: `2026-05-02 11:48`
- Summary:
  - 2026-05-02: Implemented runtime component bridge from normalized `SceneObjectDescription.Components` into Scene runtime components.
  - 2026-05-02: Added Transform-only runtime smoke coverage: object appears in snapshot with `HasMeshRenderer=false`, is excluded from render frame, and is not selected by M15 default rotation behavior.
  - 2026-05-02 GateFailure: App regression and full solution build were first launched in parallel and raced on `src/Engine.App/obj/Debug/net7.0/apphost`, producing transient `NETSDK1177` / `MSB3030`; validation remains in `InProgress` and will be rerun serially before Review.
- FilesChanged:
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
  - `src/Engine.Scene/Runtime/SceneMeshRendererComponent.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-018.md`
- ValidationEvidence:
  - Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal` passed, 48 tests.
  - Regression: `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj --no-restore --nologo -v minimal` passed, 18 tests.
  - Regression: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` passed, 9 tests after serial rerun.
  - Build: `dotnet build AnsEngine.sln --no-restore --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Smoke: Transform-only object remains in runtime snapshot with `HasMeshRenderer=false`, is excluded from render items, and M15 update rotates only the first object with Transform + MeshRenderer.
  - Perf: bridge maps normalized descriptions once during load; render filtering remains runtime-state iteration with no JSON/file reads or object rebuilds per frame.
  - Boundary: `rg` path check found no forbidden Scene dependencies or file-model/JSON usage in `src/Engine.Scene`; matches were only boundary assertion test text.
- ModuleAttributionCheck: pass
