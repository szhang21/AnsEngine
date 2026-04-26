# 任务: TASK-APP-009 M10 场景文件选择与 SceneData loader 装配

## TaskId
`TASK-APP-009`

## 目标（Goal）
由 `Engine.App` 负责选择默认样例场景文件、装配 `ISceneDescriptionLoader` 与 Scene 初始化入口，并支持环境变量或配置覆盖场景路径，不把场景业务逻辑塞进组合根。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M10-2026-04-25`

## 里程碑引用（兼容别名：MilestoneRef）
`M10.3`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Scene
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M10-G3`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-SDATA-002`
  - `TASK-SCENE-009`

## 里程碑上下文（MilestoneContext）
- M10 的组合根目标是让样例场景文件进入真实启动路径，而不是继续由 `SceneGraphService` 在代码里内建场景内容。
- 本卡承担的是“场景文件选择 + loader 装配 + Scene 初始化接线”，不承担 JSON 解析细节、不承担场景业务建模，也不承担 Render 主路径变更。
- 上游背景直接影响本卡的点包括：默认样例场景要能驱动 M9 真实 mesh 资产链路；App 只负责装配，不负责场景业务逻辑；需要支持环境变量或配置覆盖路径。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.App` 只负责选择场景文件并装配 `ISceneDescriptionLoader`。
  - `SceneData` 负责 JSON 解析与规范化；`Scene` 负责运行时对象与渲染输出。
  - 样例场景文件修改后无需改 C# 代码即可影响运行结果。
  - 结构约定已被上游定稿：
    - `PLAN-M10-2026-04-25 > SceneDataContents > 加载与失败语义` 中 `ISceneDescriptionLoader.Load(string sceneFilePath)` 与 `SceneDescriptionLoadResult`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层` 中 `SceneDescription`
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 JSON 校验、默认值补齐或场景对象生成逻辑塞进 App。
  - 不允许通过环境变量分支把核心 loader 注入变成隐式行为。
  - 不允许让 Render 或 Scene 自己定位场景文件路径。
  - 不允许自行改写 loader 调用主语义，例如把 `Load(string sceneFilePath)` 包装成 App 私有无参神秘入口，从而让配置来源不可测试。

## 实施说明（ImplementationNotes）
- 先梳理 `ApplicationBootstrap`/`RuntimeBootstrap` 当前装配路径，定位 Scene 初始化发生的位置。
- 加入默认样例场景文件解析入口，并支持环境变量或配置覆盖为外部场景路径。
- 把 `ISceneDescriptionLoader`、Scene 初始化入口和现有 renderer/asset provider 装配串起来，确保 headless/native 两条路径都走同一套场景加载语义。
- 最后补 App 测试，覆盖默认场景选择、路径覆盖、加载失败收口和主循环启动/退出稳定性。

## 设计约束（DesignConstraints）
- 不允许在 App 中内联 JSON 反序列化代码。
- 不允许在 App 中持有场景对象集合并代替 `Scene` 管理运行时状态。
- 不允许通过静态全局变量或服务定位器隐藏 loader 注入。
- 不允许为了 smoke 通过而偷偷保留“无文件时回到硬编码场景”的隐式主路径。

## 失败与降级策略（FallbackBehavior）
- 若样例场景文件缺失或 loader 返回失败结果，允许 App 走显式错误输出并受控退出；不得静默切回硬编码场景主路径。
- 若 headless 路径无法创建真实窗口，不应阻塞基本 smoke；但场景加载路径仍必须被执行并可验证。
- 若接线中发现 `Scene` 初始化入口或 `SceneData` 契约不足，必须回退修卡，不得在 App 中临时补业务逻辑绕过。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/Program.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
- 相关测试入口：
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-002.md`
  - `.ai-workflow/tasks/task-scene-009.md`
  - `.ai-workflow/archive/2026-04/TASK-APP-008.md`
  - 结构示例定位：
    - `PLAN-M10-2026-04-25 > SceneDataContents > 加载与失败语义`
    - `PLAN-M10-2026-04-25 > HandoffToDispatch > 主模块边界`
    - `PLAN-M10-2026-04-25 > M10.3`
  - 约束说明：
    - 上述计划中的 loader/result 结构在本卡属于“参考实现约束”，App 必须按该契约装配和消费，不能另起一套场景加载返回协议。

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - 场景文件路径选择与覆盖配置
  - SceneData loader 装配
  - App 测试
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在 App 中实现 JSON 解析细节
- 不在 App 中承载 Scene 运行时对象逻辑
- 不修改 Render 现有消费模型
- OutOfScopePaths:
  - `src/Engine.SceneData/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.Asset/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 环境变量命名与样例场景默认物理路径具体落点；若影响公开运行方式或文档约定，先回退确认。
- 处理规则：
  - 若问题影响 App 对外配置入口、错误收口或默认样例路径策略，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 已明确本卡只处理场景文件选择与 loader/Scene 接线，不承载 JSON 或场景业务逻辑。
  - 关键实施入口、失败收口和测试面已下沉。
  - 执行者无需回看 M10 全文，也能知道不能用硬编码场景作为隐式后门。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 装配路径单模块主导，但同时涉及配置入口、启动收口和 headless/native 一致性。
  - 若写太短，容易把场景业务逻辑误塞进 App。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Contracts`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Render`
- ForbiddenDependsOn:
  - 在 App 中承载场景对象生成、JSON 业务校验或运行时映射逻辑
  - 通过全局定位器绕过显式 loader 注入

## 边界变更请求（BoundaryChangeRequest）
- Required: `true`
- Status: `approved`
- RequestReason: `M10 计划明确要求 Engine.App 装配 SceneData loader 并选择场景文件，因此需要在现有 App 边界上新增 Engine.App -> Engine.SceneData 依赖。`
- ImpactModules:
  - `Engine.App`
  - `Engine.SceneData`
- HumanApprovalRef: `Human command "m10拆卡吧" on 2026-04-25, accepting PLAN-M10-2026-04-25 dependency direction`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；默认场景选择与路径覆盖测试通过
- Smoke: 样例场景文件可驱动真实 mesh 渲染并稳定启动/退出
- Perf: 场景文件仅在初始化阶段加载，稳定运行阶段无重复 loader 创建或场景重载

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-APP-009.md`
- ClosedAt: `2026-04-26 15:45`
- Summary: `Engine.App` 已装配 `ISceneDescriptionLoader`、默认样例场景路径与环境变量覆盖入口；启动路径先加载 `SceneDescription` 再初始化 Scene，局部门禁通过，等待 Human 复验后关单。
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/Engine.App.csproj`
  - `src/Engine.App/SceneRuntimeContracts.cs`
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-app-009.md`
  - `.ai-workflow/archive/2026-04/TASK-APP-009.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.App/Engine.App.csproj -c Debug --nologo -v minimal`）
  - Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.App/Engine.App.csproj -c Release --nologo -v minimal --no-restore`）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --nologo -v minimal --no-restore`；6 条通过）
  - Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=1 /Users/ans/.dotnet/dotnet run --project src/Engine.App/Engine.App.csproj --no-build` 退出码 0）
  - Perf: `pass`（场景文件加载只发生在启动阶段，稳定运行阶段无重复 loader 创建或场景重载）
- ModuleAttributionCheck: pass
