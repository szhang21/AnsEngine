# 任务: TASK-SCENE-013 M14 RuntimeScene 到 SceneRenderFrame 输出

## TaskId
`TASK-SCENE-013`

## 目标（Goal）
让 `SceneGraphService.BuildRenderFrame()` 从 `RuntimeScene` 的 runtime objects/components 输出 `SceneRenderFrame`，同时保持 Render contract、不改 App/Render 调用方，并确保 transform 与 camera 语义不退化。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M14-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M14.4`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P2

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
  - `TASK-SCENE-012`

## 里程碑上下文（MilestoneContext）
- M14.4 是对外可见行为不变、内部状态源完成迁移的关键点：`BuildRenderFrame` 仍要产出当前 Render 可消费的 contract，但数据来源必须改成 runtime objects/components。
- 本卡承担的是 runtime -> render frame 输出重写，不承担 runtime snapshot 查询面。
- 上游直接影响本卡的背景包括：Render 继续只消费 `Engine.Contracts.SceneRenderFrame`；`SceneRenderItem.NodeId` 来自 `SceneRuntimeObject.NodeId`；带 `Transform + MeshRenderer` 的对象才输出 render item；camera 继续沿用当前默认/description 语义。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Render contract 不变，App 和 Render 不需要知道 runtime object model。
  - `BuildRenderFrame()` 需从 runtime objects 中筛选带 `Transform + MeshRenderer` 的对象输出 `SceneRenderItem`。
  - mesh/material/transform 来自 components，camera 来自 runtime camera state，frame number 保持自增。
  - `Engine.Render` 不感知 runtime component 类型。
- 本卡执行时不得推翻的既定取舍：
  - 不允许让 Render/App 开始依赖 `SceneRuntimeObject`、`RuntimeScene` 或 component 类型。
  - 不允许借本卡改写 `SceneRenderFrame` 或 `SceneRenderItem` 契约形状。
  - 不允许把 transform/camera 语义退化回硬编码 demo 输出。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M14-2026-05-01 > SceneGraphServiceChanges > BuildRenderFrame()` 已定稿输出筛选规则、字段来源和 frame number 行为，本卡执行时不得改成 runtime object 全量裸暴露。
  - `PLAN-M14-2026-05-01 > RuntimeModel > SceneTransformComponent / SceneMeshRendererComponent / SceneCameraRuntimeState` 已定稿 output 来源组件形状。

## 实施说明（ImplementationNotes）
- 先把 `BuildRenderFrame()` 的输入源从旧 render item 缓存切到 `RuntimeScene`。
- 逐个 runtime object 筛选带 transform 和 mesh renderer 的对象，输出 `SceneRenderItem`，并确保 NodeId、mesh/material refs、transform 都来自 runtime state。
- 让 camera 输出改由 runtime camera state 生成，保持 description/default camera 兼容。
- 保持 `mFrameNumber` 自增逻辑和 `ISceneRenderContractProvider.BuildRenderFrame()` 对外接口不变。
- 补 Scene 测试和回归验证，覆盖：item count 与 mesh renderer object count 一致、transform 修改后重新 build frame 反映变化、camera 输出稳定、`Engine.Render.Tests` 不退化。

## 设计约束（DesignConstraints）
- 不允许修改 `Engine.Contracts` render contract 类型。
- 不允许在 `BuildRenderFrame()` 内重新解析 scene description 或文件路径。
- 不允许让 `Engine.Render`、`Engine.App` 或 `Engine.SceneData` 感知 runtime component 类型。
- 不允许顺手实现 update loop、world transform propagation 或 editor viewport。

## 失败与降级策略（FallbackBehavior）
- 若 legacy demo path 仍保留，允许继续存在，但 `LoadSceneDescription` 后的稳定输出必须明确走 runtime scene path，不能回退到旧 render item 主路径。
- 若某个 runtime object 缺失必须组件，本卡允许跳过 render item 输出，但必须保持行为可测试、可诊断。
- 若实现中发现必须扩 contract、改 Render 或改 App 才能输出 frame，必须停工回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `src/Engine.Contracts/SceneResourceContracts.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
  - `tests/Engine.Render.Tests/SceneRenderGpuMeshResourceCacheTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-011.md`
  - `.ai-workflow/tasks/task-scene-012.md`
  - `.ai-workflow/tasks/task-scene-009.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M14-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M14-2026-05-01 > SceneGraphServiceChanges > BuildRenderFrame()`
  - `PLAN-M14-2026-05-01 > Milestones > M14.4`
  - `PLAN-M14-2026-05-01 > TestPlan`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - `BuildRenderFrame()` runtime 输出重写
  - Scene 测试
  - 必要的 Render 回归验证入口
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.Render.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不新增 render contract 字段
- 不修改 App 装配或 Render 实现代码
- 不实现 runtime snapshot 查询面
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.Contracts/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若 legacy demo path 仍需保留，是在无 description load 时启用还是显式测试 helper；只要不污染 runtime scene 主输出即可。
- 处理规则：
  - 若问题影响 contract 稳定性、runtime path 唯一性或 Render 无感知约束，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 输出来源、筛选规则、contract 不变约束和测试重点都已明确。
  - 本卡与前序映射卡、后序 snapshot 卡切分清楚。
  - 无需回看计划全文也能知道本卡不能把 runtime component 暴露给 Render/App。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及内部状态源切换后的对外 contract 保持不变，以及 Render/Scene 回归稳定性。
  - 若路线选错，会直接让 runtime object 成为跨模块公共事实，或破坏现有渲染链路。
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
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj` 与 `Engine.Render.Tests` 相关回归通过
- Smoke: `SceneRenderFrame.Items` 与 runtime objects 中带 mesh renderer 的对象一致；transform 修改后重新 build frame 能反映变化；camera 输出保持当前默认/description 语义
- Perf: 无重复 description 解析、无额外逐帧文件读取，frame build 路径相对基线无明显退化

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
- FailureType: `Architecture/LegacyPath`
- DetectedAt: `2026-05-01 M14 Review`
- ReopenReason: `BuildRenderFrame still contains legacy render-item generation path; RuntimeScene is not the single render-frame state source.`
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-013.md`
- ClosedAt: `2026-05-01 14:18`
- Summary: 回流移除 `SceneGraphService` legacy render-item generation path，`BuildRenderFrame` 无条件从 `RuntimeScene` runtime objects/components 输出 `SceneRenderFrame`，Render contract 不变。
- FilesChanged:
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-013.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-013.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 30 条通过；`dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj --no-restore --nologo -v minimal`；Render.Tests 16 条通过；`dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`；App.Tests 6 条通过；`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过）
  - Smoke: `pass`（`BuildRenderFrame` 无条件读取 `RuntimeScene`；`AddRootNode` 写入 runtime components；transform/camera/material 在无运行时修改时保持稳定，运行时组件修改后 frame 反映新值）
  - Boundary: `pass`（legacy path 符号无残留；未改 Contracts/Render/SceneData 业务实现，未新增禁止依赖）
  - Perf: `pass`（frame build 只遍历 runtime object/components，无重复 description 解析、文件读取或内部逐帧 demo state 生成）
- ModuleAttributionCheck: pass
