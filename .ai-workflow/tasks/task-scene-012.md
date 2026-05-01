# 任务: TASK-SCENE-012 M14 SceneDescription 到 RuntimeScene 映射

## TaskId
`TASK-SCENE-012`

## 目标（Goal）
将 `SceneGraphService.LoadSceneDescription(SceneDescription)` 改为创建和替换 `RuntimeScene`，使每个 `SceneObjectDescription` 稳定映射成一个 `SceneRuntimeObject`，并在重新加载 scene 时清空旧 runtime state。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M14-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M14.3`

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
  - `TASK-SCENE-011`

## 里程碑上下文（MilestoneContext）
- M14.3 是 M14 的关键迁移点：`SceneGraphService` 不再把 `SceneDescription` 直接转成 render item list，而是先落到 `RuntimeScene`。
- 本卡承担的是 description 到 runtime object 的映射与旧 runtime state 清空，不承担 `BuildRenderFrame` 从 runtime object 输出。
- 上游直接影响本卡的背景包括：NodeId 从 1 开始稳定递增；每个 `SceneObjectDescription` 都映射为一个 runtime object；旧 `mRenderItems` 主路径要移除或降级为 legacy demo path；`NodeCount` 必须与 runtime object count 一致。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 数据流保持 `SceneData.SceneDescription -> Engine.Scene RuntimeScene -> Engine.Contracts.SceneRenderFrame -> Engine.Render`。
  - `LoadSceneDescription(SceneDescription)` 要清空旧 runtime state、创建新的 runtime objects、重置 frame number。
  - `NodeId` 继续用于 `SceneRenderItem.NodeId`，但 object identity 未来优先使用 `ObjectId`。
  - 旧 `AddRootNode()` demo path 可以保留，但必须和 runtime scene 稳定输出路径分开。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在本卡中把 `SceneDescription` 重新解释为 render-only DTO 壳。
  - 不允许让 `SceneData` 反向感知 runtime object/component。
  - 不允许在重载新 scene 时保留旧 runtime objects 形成脏状态混合。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M14-2026-05-01 > RuntimeModel > SceneRuntimeObject` 已定稿 `NodeId`、`ObjectId`、`ObjectName`、`Transform`、`MeshRenderer` 为 runtime object 形状。
  - `PLAN-M14-2026-05-01 > SceneGraphServiceChanges > LoadSceneDescription(SceneDescription)` 已定稿清空旧 state、创建 runtime objects、重置 frame number 的实施顺序，本卡执行时不得改成增量叠加语义。

## 实施说明（ImplementationNotes）
- 先梳理 `SceneGraphService` 当前 `LoadSceneDescription` 与旧 `mRenderItems` 主路径，识别迁移切入点。
- 让 `LoadSceneDescription` 改为：
  - 清空旧 runtime scene
  - 逐个 `SceneObjectDescription` 生成 `SceneRuntimeObject`
  - 稳定分配 `NodeId`
  - 映射 camera runtime state
  - 重置 `mFrameNumber`
- 明确 legacy/demo path 和 scene description runtime path 的边界，避免两条路径写到同一主状态源。
- 补 Scene 测试，覆盖：空对象 scene、单对象 scene、多对象 scene、重复 load 清空旧状态、`NodeCount` 一致。

## 设计约束（DesignConstraints）
- 不允许在本卡内重写 `BuildRenderFrame` 为最终 runtime 输出主路径。
- 不允许修改 `SceneDescription`、`SceneObjectDescription`、`SceneCameraDescription` 字段形状。
- 不允许让 `LoadSceneDescription` 依赖 JSON、文件路径、asset catalog 或 editor 状态。
- 不允许让 NodeId 分配变成与对象顺序无关的不可预测逻辑。

## 失败与降级策略（FallbackBehavior）
- 若旧 demo path 仍被局部测试或启动链路依赖，允许保留 legacy path，但必须明确与 runtime scene description path 隔离。
- 若某个 `SceneDescription` 已经通过上游校验却在本层映射失败，应显式报错并回退修卡，不得在 Scene 内静默猜测缺失字段。
- 若迁移后 `NodeCount`、frame number 或旧 state 清空语义无法保持稳定，必须停工回退，不得把脏迁移推进到后续 render frame 卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.SceneData/Descriptions/SceneDescription.cs`
  - `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
  - `src/Engine.SceneData/Descriptions/SceneCameraDescription.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-010.md`
  - `.ai-workflow/tasks/task-scene-011.md`
  - `.ai-workflow/tasks/task-scene-009.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M14-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M14-2026-05-01 > SceneGraphServiceChanges`
  - `PLAN-M14-2026-05-01 > RuntimeModel`
  - `PLAN-M14-2026-05-01 > Milestones > M14.3`
  - `PLAN-M14-2026-05-01 > TestPlan`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - `LoadSceneDescription` runtime 映射
  - runtime scene 替换逻辑
  - Scene 测试
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 runtime object 到 render frame 的最终输出
- 不实现 snapshot 查询面
- 不实现 App/Render 改造
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - legacy `AddRootNode()` path 是留在 `SceneGraphService` 内还是抽成更明确的 demo helper；两者都可，但必须不污染新的 runtime scene description 主路径。
- 处理规则：
  - 若问题影响主状态源切换、NodeId 稳定性或旧 state 清空语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 映射顺序、状态替换语义、legacy path 边界和测试重点都已明确。
  - 本卡和后续 render frame/snapshot 路线切分清楚。
  - 无需回看计划全文也能知道本卡不能继续沿用 `mRenderItems` 作为主运行时真相。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及 `SceneGraphService` 主状态源迁移、旧路径隔离和重复 load 清空语义。
  - 若路线选错，会直接把 runtime scene owner 退化回 render item list，或留下脏状态混合风险。
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
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj` 通过，sample/multi-object/reload 清空行为测试通过
- Smoke: sample scene load 后 runtime object count 正确，重复 load 新 scene 会清空旧 runtime objects，`NodeCount` 一致
- Perf: runtime scene 映射只发生在显式 load 路径；稳定帧循环阶段无重复 JSON 解析或文件读取

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-012.md`
- ClosedAt: `2026-05-01 05:51`
- Summary: `RuntimeScene.LoadFromDescription` 创建 runtime objects/components/camera state，`SceneGraphService.LoadSceneDescription` 改为以 runtime scene 为主状态源并隔离 legacy AddRootNode path。
- FilesChanged:
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-012.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-012.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 22 条通过；`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过）
  - Smoke: `pass`（空/单/多对象 scene 可映射到 runtime scene；重复 load 清空旧 runtime state；`NodeCount` 与 runtime object count 一致）
  - Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**` 与任务指定边界/归档文档；未新增 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL 依赖）
  - Perf: `pass`（runtime scene 映射仅在显式 load 路径发生，无重复 JSON 解析、文件读取或逐帧映射）
- ModuleAttributionCheck: pass
