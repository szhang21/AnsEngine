# 任务: TASK-SCENE-010 M14 Runtime Object 基础模型

## TaskId
`TASK-SCENE-010`

## 目标（Goal）
在 `Engine.Scene` 内建立轻量 runtime object 基础模型和 `RuntimeScene` 持有能力，使 `SceneGraphService` 可以从 runtime scene 获取 object count，而不是继续把运行时状态隐含在 render item list 中。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M14-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M14.1`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P0

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
- DependsOn: `[]`

## 里程碑上下文（MilestoneContext）
- M14 的第一步不是先改 `BuildRenderFrame`，而是先让 `Engine.Scene` 真正拥有 runtime scene owner 和 object identity，否则后续组件、映射和快照都没有稳定落点。
- 本卡承担的是基础 runtime object/scene 容器和 object count 接线，不承担 transform、mesh renderer、camera 组件和 render frame 输出改造。
- 上游直接影响本卡实现的背景包括：M14 不新增模块；runtime model 先留在 `Engine.Scene` 内；`SceneGraphService` 作为过渡期 facade/owner；`Engine.App -> SceneData -> Scene -> Render` 链路不能退化。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - M14 不新增 `Engine.Runtime` 或其他新模块，runtime object model 先放在 `Engine.Scene`。
  - `SceneRuntimeObject` / `RuntimeScene` 可优先设为 `internal`。
  - `SceneGraphService.NodeCount` 最终应从 runtime object count 获取。
  - `Engine.Scene` 依赖保持为 `Engine.Core`、`Engine.Contracts`、`Engine.SceneData`，禁止新增 `Render/Asset/App/Editor` 依赖。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 runtime object/component 暴露成跨模块公共可变事实。
  - 不允许借本卡顺手引入 ECS、hierarchy、world transform 或 update loop。
  - 不允许通过修改 `Engine.App` 或 `Engine.Render` 适配本卡内部迁移。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M14-2026-05-01 > ModuleDesign` 中 `SceneRuntimeObject`、`RuntimeScene`、`SceneRuntimeObjectSnapshot`、`RuntimeSceneSnapshot` 已是上游定稿的命名方向，本卡执行时不得自行改成另一套 runtime 主类型命名。
  - `PLAN-M14-2026-05-01 > RuntimeModel > SceneRuntimeObject` 已定稿 `NodeId`、`ObjectId`、`ObjectName` 作为基础 object identity 字段，本卡必须围绕这些字段立 runtime object 基础形状。

## 实施说明（ImplementationNotes）
- 先在 `src/Engine.Scene/Runtime/` 建立 runtime 目录和基础类型落点。
- 再实现 `SceneRuntimeObject` 的最小 identity 部分和 `RuntimeScene` 的对象集合持有/清空能力。
- 把 `SceneGraphService` 的最小内部状态切到“持有一个 runtime scene + frame number”，并让 `NodeCount` 从 runtime scene object count 返回。
- 补 Scene 测试，覆盖：可创建 runtime object、重新 load scene 时旧 object 清空、object id/name 保持输入值、无禁止依赖。

## 设计约束（DesignConstraints）
- 不允许在本卡内实现 transform/world transform、mesh renderer、camera 或 render frame 输出重写。
- 不允许让测试直接依赖内部可变集合；如确需触达内部类型，优先通过 `SceneGraphService` 间接验证。
- 不允许新增 `Engine.Render`、`Engine.Asset`、`Engine.App`、`Engine.Editor`、`Engine.Editor.App`、OpenTK、ImGui、OpenGL 依赖。
- 不允许将旧 render item list 继续伪装成 runtime object 集合。

## 失败与降级策略（FallbackBehavior）
- 若内部类型可见性导致测试无法稳定验证，允许评估 `InternalsVisibleTo("Engine.Scene.Tests")`，但必须保持内部类型不对业务模块公开。
- 若迁移到 runtime scene owner 过程中发现旧 demo path 被破坏，可临时保留 legacy/demo path，但必须和新的 runtime scene path 分开，不得混成同一状态源。
- 若实现中发现必须扩散到 App/Render 才能完成 object count 切换，必须停工回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Scene/Engine.Scene.csproj`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-009.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M14-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M14-2026-05-01 > ModuleDesign`
  - `PLAN-M14-2026-05-01 > RuntimeModel > SceneRuntimeObject`
  - `PLAN-M14-2026-05-01 > SceneGraphServiceChanges`
  - `PLAN-M14-2026-05-01 > Milestones > M14.1`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - runtime 基础模型
  - runtime scene owner
  - Scene 测试
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 transform/mesh renderer/camera 组件
- 不实现 `LoadSceneDescription` 到 runtime scene 映射
- 不实现 `BuildRenderFrame` 从 runtime objects 输出
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `RuntimeScene` 是否需要在本卡内先提供最小 snapshot 钩子，还是留到 `TASK-SCENE-014`；只要不提前暴露可变集合即可。
- 处理规则：
  - 若问题影响内部可见性、模块边界或后续 snapshot 路线，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡只交付基础 runtime object/scene owner，结果单一明确。
  - 上游命名、字段形状、禁止路线和测试重点都已落卡。
  - 无需回看计划全文也能知道本卡不能扩成 ECS、hierarchy 或跨模块公共模型。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 单模块基础模型迁移，但涉及内部状态源切换和后续全部 runtime 卡的落点稳定性。
  - 若基础 object/runtime scene 形状立歪，后续组件、映射和快照都会返工。
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
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj` 通过，runtime object 基础模型与清空行为测试通过
- Smoke: 重新 load scene 时旧 runtime objects 清空，`NodeCount` 与 runtime scene object count 一致
- Perf: 不引入额外逐帧分配主路径；runtime owner 建立仅影响显式 scene load/build 路径

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-010.md`
- ClosedAt: `2026-05-01 05:46`
- Summary: 新增 internal `SceneRuntimeObject` 与 `RuntimeScene`，`SceneGraphService` 持有 runtime scene owner，`NodeCount` 改从 runtime object count 返回。
- FilesChanged:
  - `src/Engine.Scene/Properties/AssemblyInfo.cs`
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-010.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-010.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 14 条通过；`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过）
  - Smoke: `pass`（重新 load scene 后 runtime object count 清空并重建，`NodeCount` 与 runtime scene object count 一致）
  - Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**` 与任务指定边界/归档文档；未新增 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL 依赖）
  - Perf: `pass`（runtime owner 仅在显式 add/load 路径维护 object list，不新增逐帧文件 IO 或 update loop）
- ModuleAttributionCheck: pass
