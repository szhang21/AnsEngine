# 任务: TASK-SCENE-011 M14 Transform/MeshRenderer/Camera 组件

## TaskId
`TASK-SCENE-011`

## 目标（Goal）
在 `Engine.Scene` 内新增 `SceneTransformComponent`、`SceneMeshRendererComponent` 和 `SceneCameraRuntimeState`，并提供从 `SceneObjectDescription` / `SceneCameraDescription` 映射到 runtime 组件状态的最小能力。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M14-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M14.2`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Core
- Engine.Contracts
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M14-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-010`

## 里程碑上下文（MilestoneContext）
- M14.2 的作用是把 runtime object 从“只有 identity 的壳”推进到“有 transform、mesh/material、camera 语义的最小运行时对象模型”。
- 本卡承担的是组件和 camera runtime state 本身，不承担完整 scene description load，也不承担 render frame 输出重写。
- 上游直接影响本卡的背景包括：所有 object 都有 `Transform`；M14 映射时默认每个 object 都有 `MeshRenderer`；camera 先不挂到 object 上；不做 world transform、parent/children 或资源存在性校验。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `SceneTransformComponent` 保存 local position/rotation/scale，并提供转换到 `Engine.Contracts.SceneTransform` 的方法。
  - `SceneMeshRendererComponent` 只持有 `SceneMeshRef` / `SceneMaterialRef`，不加载 mesh 文件，不检查资源存在性。
  - `SceneCameraRuntimeState` 从 `SceneCameraDescription` 映射，并提供 `BuildCamera(aspectRatio, nearPlane, farPlane)` 或等价能力。
  - M14 不把 camera 做成挂在 object 上的 component。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在本卡内实现 world transform 传播、层级 parent/child 或 camera-on-object 设计。
  - 不允许让 mesh renderer 依赖 `Engine.Asset` 或文件路径。
  - 不允许把缺省 camera 语义改成与当前行为不兼容的另一套默认值。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M14-2026-05-01 > ModuleDesign` 中 `SceneTransformComponent`、`SceneMeshRendererComponent`、`SceneCameraRuntimeState` 已是上游定稿命名方向。
  - `PLAN-M14-2026-05-01 > RuntimeModel > SceneTransformComponent / SceneMeshRendererComponent / SceneCameraRuntimeState` 已定稿字段语义、非职责和最小行为边界，本卡执行时不得改成另一套 component 结构。

## 实施说明（ImplementationNotes）
- 先为 runtime object 补 transform、mesh renderer 和 camera runtime state 类型落点。
- 为 transform 组件实现 local position/rotation/scale 持有与到 `Engine.Contracts.SceneTransform` 的转换能力。
- 为 mesh renderer 组件实现 `SceneMeshRef` / `SceneMaterialRef` 持有。
- 为 camera runtime state 实现从 description 映射与缺省 camera 回退能力。
- 补 Scene 测试，覆盖：transform 映射正确、mesh/material 引用映射正确、camera description 映射正确、缺省 camera 语义与当前行为一致。

## 设计约束（DesignConstraints）
- 不允许在本卡内读取文件、加载资源、连接 Render 或修改 `Engine.SceneData` schema。
- 不允许把 transform 组件做成 world/local 混合语义。
- 不允许让 camera runtime state 直接依赖 OpenTK、ImGui、OpenGL 或 editor 概念。
- 不允许顺手加入脚本、物理、动画或 update loop 钩子。

## 失败与降级策略（FallbackBehavior）
- 若 description 缺省 camera 为空，本卡允许沿用当前默认相机语义继续运行，但必须保持该路径可测试、可诊断。
- 若发现某个组件定义必须依赖 `Asset`、`Render` 或 editor 才能表达，必须停工回退修卡。
- 若 transform 到 contract 的转换无法在本卡内定稳，可保留最小值对象转换函数，但不得延迟为“以后再想”。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `src/Engine.SceneData/Descriptions/SceneDescription.cs`
  - `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
  - `src/Engine.SceneData/Descriptions/SceneCameraDescription.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-010.md`
  - `.ai-workflow/tasks/task-scene-009.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M14-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M14-2026-05-01 > ModuleDesign`
  - `PLAN-M14-2026-05-01 > RuntimeModel > SceneTransformComponent`
  - `PLAN-M14-2026-05-01 > RuntimeModel > SceneMeshRendererComponent`
  - `PLAN-M14-2026-05-01 > RuntimeModel > SceneCameraRuntimeState`
  - `PLAN-M14-2026-05-01 > Milestones > M14.2`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - runtime 组件类型
  - camera runtime state
  - Scene 测试
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 scene description 到 runtime object 全量 load
- 不实现 runtime object 到 render frame 输出
- 不实现 world transform、hierarchy、scripting、physics、animation
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `SceneCameraRuntimeState.BuildCamera(...)` 的参数入口是否直接复用当前 `SceneGraphService` 默认 near/far/aspect 策略；只要默认行为兼容即可。
- 处理规则：
  - 若问题影响默认 camera 语义、模块边界或 contract 转换形状，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 组件种类、字段职责、非职责和测试重点都已下沉。
  - 本卡与后续 load/render/snapshot 路线切分清楚。
  - 无需回看计划全文也能知道本卡不能做资源加载、world transform 或 camera-on-object。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及多个组件类型、contract 转换、缺省 camera 兼容和未来扩展点。
  - 若组件边界或语义写错，会直接影响后续 load、render frame 和 snapshot 查询。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.Asset`
  - `Engine.Scene -> Engine.App`
  - `Engine.Scene -> Engine.Editor`
  - `Engine.Scene -> Engine.Editor.App`

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
- Build: `dotnet build AnsEngine.sln` 通过
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj` 通过，transform/mesh/material/camera 映射测试通过
- Smoke: 缺省 camera 与 description camera 都可生成有效 `SceneCamera`；transform 到 contract 输出稳定
- Perf: 不引入逐帧资源加载或额外 runtime owner 热路径退化

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-011.md`
- ClosedAt: `2026-05-01 05:49`
- Summary: 新增 transform、mesh renderer、camera runtime state 组件，支持 description 映射与 contract/camera 输出，默认 camera 语义保持兼容。
- FilesChanged:
  - `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
  - `src/Engine.Scene/Runtime/SceneMeshRendererComponent.cs`
  - `src/Engine.Scene/Runtime/SceneCameraRuntimeState.cs`
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-011.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-011.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 19 条通过；`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过）
  - Smoke: `pass`（transform 到 `SceneTransform` 输出稳定；mesh/material refs 保持；description/default camera 均可生成有效 `SceneCamera`）
  - Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**` 与任务指定边界/归档文档；未新增 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL 依赖）
  - Perf: `pass`（组件仅为值状态与显式转换，不引入逐帧资源加载或 update loop）
- ModuleAttributionCheck: pass
