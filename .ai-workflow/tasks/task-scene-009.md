# 任务: TASK-SCENE-009 M10 SceneDescription 到运行时场景初始化入口

## TaskId
`TASK-SCENE-009`

## 目标（Goal）
在 `Engine.Scene` 增加从 `SceneDescription` 初始化运行时场景的入口，让 Scene 基于数据驱动对象和相机输出现有 `SceneRenderFrame`，同时保持 Scene 不直接做 JSON 解析或文件 IO。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M10-2026-04-25`

## 里程碑引用（兼容别名：MilestoneRef）
`M10.3`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Contracts
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M10-G3`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-SDATA-002`

## 里程碑上下文（MilestoneContext）
- M10 的主链路要求从“硬编码 `SceneGraphService` 内容”切到“`SceneDescription -> Scene 运行时对象 -> SceneRenderFrame`”，因此 `Scene` 必须新增数据驱动初始化入口。
- 本卡承担的是 `SceneDescription` 到运行时场景的映射与渲染输出接线，不承担 JSON 解析、不承担 App 文件选择，也不承担 Render JSON 感知。
- 上游背景直接影响本卡的点包括：对象实例使用 `LocalTransform`；`Render` 继续只消费现有 `SceneRenderFrame`；默认相机可由上游或本卡映射时补齐，但语义源头在 `SceneData`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.Scene` 只消费规范化后的 `SceneDescription`，不直接解析 JSON。
  - `Render` 不理解 JSON；它继续消费 `mesh + transform + camera` 的现有契约输出。
  - `LocalTransform` 在 M10 无层级时等价 world transform，但后续 M11 会扩展为真正的 local-to-world。
  - 结构约定已被上游定稿：
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层` 中 `SceneDescription.SceneId/SceneName/Camera/Objects`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层` 中 `SceneObjectDescription.ObjectId/ObjectName/Mesh/Material/LocalTransform`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层` 中 `SceneCameraDescription.Position/Target/FieldOfViewRadians`
- 本卡执行时不得推翻的既定取舍：
  - 不允许把默认值规范化责任重新推回 `Scene`。
  - 不允许让 `Scene` 直接依赖文件路径、asset catalog 或 OBJ 语义。
  - 不允许为“方便接线”而修改 Render 去消费 `SceneData` 或 JSON DTO。
  - 不允许自行改写 `SceneDescription` 与 `SceneObjectDescription` 的字段形状，例如把 `LocalTransform` 拆成另一套 Scene 私有输入结构后再隐藏上游语义。

## 实施说明（ImplementationNotes）
- 先定位 `SceneGraphService` 当前硬编码对象和相机初始化入口，识别可以被 `SceneDescription` 驱动替换的切入点。
- 增加从 `SceneDescription` 初始化 Scene 运行时状态的入口，至少覆盖对象列表、`meshId`、`materialId`、`LocalTransform`、相机。
- 保持现有 `BuildRenderFrame()` 出口不变，只让内部数据来源从硬编码常量切换为描述驱动。
- 补 Scene 测试，覆盖默认相机、对象映射、`LocalTransform` 语义和多对象稳定输出。

## 设计约束（DesignConstraints）
- 不允许在 `Engine.Scene` 中直接读取场景文件或做 JSON 反序列化。
- 不允许在 `Engine.Scene` 中解析 asset catalog 或 mesh 顶点数据。
- 不允许为了复用旧逻辑而让 `SceneDescription` 退化成对硬编码 demo 数据的包装壳。
- 不允许把 `LocalTransform` 写死成“以后也等同 world transform”的语义。

## 失败与降级策略（FallbackBehavior）
- 若 `SceneDescription` 加载失败，本卡不负责吞错兜底；应由上层决定是否退出或回退到显式错误路径。
- 若单个对象描述缺失关键字段并已经通过上游错误结果漏入本层，应显式报错并回退修卡，而不是在 Scene 内静默猜测。
- 若默认相机未提供且上游规范化未生成，本卡允许使用现有引擎默认相机策略继续运行，但必须保持该行为可测试、可诊断。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `src/Engine.Contracts/SceneResourceContracts.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-002.md`
  - `.ai-workflow/tasks/task-scene-008.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M10-2026-04-25.md`
  - 结构示例定位：
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层`
    - `PLAN-M10-2026-04-25 > PlanningDecisions > Transform 语义`
    - `PLAN-M10-2026-04-25 > HandoffToDispatch > 关键门禁`
  - 约束说明：
    - 上述计划中的结构示例在本卡属于“字段关系已定、Scene 必须消费”的实现约束；本卡只决定如何映射到运行时，不决定是否改写这些字段结构。

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - SceneDescription 初始化入口
  - 运行时对象/相机映射
  - Scene 测试
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不解析 JSON 或磁盘路径
- 不处理 asset catalog 或 mesh 导入
- 不改 Render 现有提交流程
- OutOfScopePaths:
  - `src/Engine.SceneData/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Render/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 是否需要保留旧的无参初始化路径用于过渡测试；若影响公开入口或行为兼容，先回退确认。
- 处理规则：
  - 若问题影响 `Scene` 对外入口、默认相机策略或 `LocalTransform` 语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 已明确本卡只接 `SceneDescription -> Scene`，不碰 JSON 解析和 Render JSON 感知。
  - `LocalTransform`、默认相机、对象映射和测试覆盖点已下沉。
  - 执行者无需回看里程碑全文，也能知道哪些默认值该信任上游，哪些行为必须可测试。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 同时存在边界风险、`LocalTransform` 语义风险和现有 `SceneGraphService` 数据源替换风险。
  - 若路线选错，会直接把 JSON/IO 重新塞回 Scene 或破坏 Render 解耦。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Asset`
  - `Engine.Scene -> Engine.Render`
  - 在 Scene 中直接执行 JSON 解析或文件 IO 流程编排

## 边界变更请求（BoundaryChangeRequest）
- Required: `true`
- Status: `approved`
- RequestReason: `M10 计划明确要求 Scene 从 SceneDescription 初始化运行时场景，因此需要在现有 Scene 边界上新增 Engine.Scene -> Engine.SceneData 依赖。`
- ImpactModules:
  - `Engine.Scene`
  - `Engine.SceneData`
- HumanApprovalRef: `Human command "m10拆卡吧" on 2026-04-25, accepting PLAN-M10-2026-04-25 dependency direction`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；Scene 初始化、默认相机和 `LocalTransform` 语义测试通过
- Smoke: 样例场景文件可驱动现有渲染链路稳定启动/退出
- Perf: 初始化成本可接受，稳定帧循环阶段无重复 JSON 解析或文件读取

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SCENE-009.md`
- ClosedAt: `2026-04-26 15:28`
- Summary: `SceneGraphService` 已新增 `SceneDescription` 初始化入口，`Scene` 可消费规范化场景描述并输出稳定 `SceneRenderFrame`，局部门禁通过，等待 Human 复验后关单。
- FilesChanged:
  - `src/Engine.Scene/Engine.Scene.csproj`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-scene-009.md`
  - `.ai-workflow/archive/2026-04/TASK-SCENE-009.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.Scene/Engine.Scene.csproj -c Debug --nologo -v minimal`）
  - Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.Scene/Engine.Scene.csproj -c Release --nologo -v minimal`）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --nologo -v minimal`；11 条通过）
  - Smoke: `pass`（`LoadSceneDescription_BuildRenderFrame_MapsObjectsCameraAndLocalTransforms` 已验证样例式 `SceneDescription` 可驱动现有 `SceneRenderFrame` 输出）
  - Perf: `pass`（`SceneDescription` 映射仅发生在初始化入口；稳定帧循环阶段无 JSON 解析或文件读取）
- ModuleAttributionCheck: pass
