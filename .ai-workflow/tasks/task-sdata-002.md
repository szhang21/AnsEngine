# 任务: TASK-SDATA-002 M10 场景 JSON 描述模型、加载与规范化

## TaskId
`TASK-SDATA-002`

## 目标（Goal）
在 `Engine.SceneData` 定义文件描述层与规范化场景层类型，实现 `ISceneDescriptionLoader`、显式失败结果和默认值规范化，让样例场景 JSON 能稳定加载为 `SceneDescription`。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M10-2026-04-25`

## 里程碑引用（兼容别名：MilestoneRef）
`M10.2`

## 执行代理（ExecutionAgent）
Exec-SceneData

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.SceneData

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scenedata.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M10-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SDATA-001`

## 里程碑上下文（MilestoneContext）
- M10 的第二步是把“文件描述层”和“规范化场景层”真实落地，否则后续 `Scene` 接线会直接绑定原始 JSON 结构。
- 本卡承担的是 JSON 描述模型、加载结果、失败语义、默认值规范化和样例场景文件；不承担 Scene 运行时初始化和 App 装配。
- 上游背景直接影响本卡的点包括：对象使用 `LocalTransform`；`meshId` 必填；缺省 `materialId` 与相机、transform 需要自动回填；`SceneData` 不能泄漏 asset catalog、OBJ 或运行时对象细节。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `SceneData` 至少分“文件描述层”和“规范化场景层”两套类型，不能只保留原始 DTO。
  - `ISceneDescriptionLoader` 必须返回显式成功/失败结果，而不是用异常驱动主流程。
  - `LocalTransform` 在 M10 无层级时等价 world transform，但语义必须保留为 local。
  - 结构约定已被上游定稿：
    - `PLAN-M10-2026-04-25 > SceneDataContents > 文件描述层`：`SceneFileDocument` / `SceneFileDefinition` / `SceneFileObjectDefinition` / `SceneFileCameraDefinition` / `SceneFileTransformDefinition`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层`：`SceneDescription` / `SceneObjectDescription` / `SceneCameraDescription` / `SceneTransformDescription`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 加载与失败语义`：`ISceneDescriptionLoader`、`SceneDescriptionLoadResult`、`SceneDescriptionLoadFailure`、`SceneDescriptionLoadFailureKind`
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 `meshId` 退化为文件路径。
  - 不允许在公开模型中暴露 OBJ、asset catalog、GPU 或编辑器状态字段。
  - 不允许把默认值逻辑下放给 `Scene` 或 `App` 兜底。
  - 对象和字段命名约定已在计划中给出，本卡不得自行把 `Mesh/Material/Transform` 或 `ObjectId/ObjectName/LocalTransform` 改成另一套语义不兼容命名。

## 实施说明（ImplementationNotes）
- 先定义文件描述层类型：`SceneFileDocument`、`SceneFileDefinition`、对象/相机/Transform DTO。
- 再定义规范化层类型：`SceneDescription`、`SceneObjectDescription`、`SceneCameraDescription`、`SceneTransformDescription` 以及 `SceneDescriptionLoadResult/Failure`。
- 实现 `ISceneDescriptionLoader` 时先覆盖 JSON 读取、反序列化、基础校验，再做默认值规范化和失败映射。
- 最后补样例场景文件与测试，至少覆盖：合法 JSON、非法 JSON、缺失 `meshId`、重复对象 `Id`、默认材质/transform/相机回填。

## 设计约束（DesignConstraints）
- 不允许 `SceneDescription` 直接引用 `SceneGraphService`、运行时节点或缓存对象。
- 不允许把 `LocalTransform` 命名或实现成 world-only 语义。
- 不允许在 loader 中解析 asset catalog、OBJ 或 mesh 数据。
- 不允许把失败语义留给调用方猜测，必须显式区分 `NotFound/InvalidJson/MissingRequiredField/DuplicateObjectId/InvalidReference/InvalidValue` 等类别。

## 失败与降级策略（FallbackBehavior）
- 对于缺失文件、非法 JSON、重复对象 `Id`、缺少必填字段等可恢复问题，返回显式失败结果，不抛出异常作为主路径控制流。
- 对于缺省 `materialId`、缺省 `LocalTransform`、缺省相机等可规范化问题，填充默认值并继续返回成功结果。
- 若实现中发现现有公开契约无法表达场景描述层所需最小语义，必须回退修卡，而不是把业务桥接临时塞进 `Engine.Core`。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
- 相关测试入口：
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/plan-archive/2026-04/PLAN-M10-2026-04-25.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-010.md`
  - `.ai-workflow/tasks/task-sdata-001.md`
  - 结构示例定位：
    - `PLAN-M10-2026-04-25 > SceneDataContents > 文件描述层`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 加载与失败语义`
    - `PLAN-M10-2026-04-25 > PlanningDecisions > 默认值`
  - 约束说明：
    - 上述计划中的结构示例在本卡属于“字段关系已定的参考实现约束”，允许局部实现细节调整，但不允许改写分层、类型命名主语义或失败枚举分类。

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
- AllowedFiles:
  - 场景 JSON DTO 与规范化描述类型
  - loader、失败语义、默认值与校验逻辑
  - SceneData 测试与样例场景文件
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在此卡内接入 `Engine.Scene` 运行时初始化
- 不在此卡内改 Render 提交
- 不实现层级、Prefab、灯光、glTF 场景导入
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.Asset/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 是否把场景 schema 版本号仅保留在文件描述层，还是同步出现在规范化层；若实现中必须影响公开结构，先回退确认。
- 处理规则：
  - 若问题影响公开模型、失败语义或默认值策略，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 已明确区分文件描述层、规范化层、loader、失败语义和默认值范围。
  - 本卡的关键禁止路线、失败分流和测试覆盖点已下沉。
  - 执行者无需回看 M10 全文，也能知道哪些默认值该在 SceneData 内完成。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 同时涉及文档模型分层、失败语义、默认值规范化和后续 M11 兼容口。
  - 如果做错，Scene/App 会被迫消费原始 JSON 或 world-only transform 语义。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.SceneData -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.SceneData -> Engine.Scene`
  - `Engine.SceneData -> Engine.Asset`
  - `Engine.SceneData -> Engine.Render`
  - 在公开模型中泄漏运行时对象、OBJ、asset catalog 或 GPU 资源细节

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scenedata.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；合法 JSON、非法 JSON、缺失必填字段、重复对象 `Id`、默认值回填测试通过
- Smoke: 样例场景文件可加载为 `SceneDescription`
- Perf: 加载阶段可接受，稳定运行阶段无重复解析或逐帧 JSON 反序列化

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SDATA-002.md`
- ClosedAt: `2026-04-26 15:12`
- Summary: `JsonSceneDescriptionLoader`、默认值规范化、显式失败语义、样例场景 JSON 与回归测试已落地，局部门禁通过，等待 Human 复验后关单。 
- FilesChanged:
  - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
  - `tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-sdata-002.md`
  - `.ai-workflow/archive/2026-04/TASK-SDATA-002.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Debug --nologo -v minimal`）
  - Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Release --nologo -v minimal`）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --nologo -v minimal`；12 条通过）
  - Smoke: `pass`（样例场景 `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json` 已被 `JsonLoader_ValidScene_LoadsNormalizedDescription` 成功加载为 `SceneDescription`）
  - Perf: `pass`（JSON 读取、反序列化与规范化只发生在加载阶段，无逐帧解析逻辑）
- ModuleAttributionCheck: pass
