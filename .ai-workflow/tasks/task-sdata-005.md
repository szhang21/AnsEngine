# 任务: TASK-SDATA-005 M11 对象级文档编辑操作与失败语义

## TaskId
`TASK-SDATA-005`

## 目标（Goal）
在 `Engine.SceneData` 提供最小对象级文档编辑操作，使 `SceneFileDocument` 支持对象的增删改（`id/name/mesh/material/transform`），并通过显式结果表达编辑失败，不触碰 Scene 运行时对象。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M11-2026-04-27`

## 里程碑引用（兼容别名：MilestoneRef）
`M11.3`

## 执行代理（ExecutionAgent）
Exec-SceneData

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.SceneData

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scenedata.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M11-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SDATA-003`
  - `TASK-SDATA-004`

## 里程碑上下文（MilestoneContext）
- M11 的对象编辑不是 GUI 编辑器，而是文档数据能力底座，供未来 M12/M13 编辑器宿主消费。
- 本卡承担的是 `SceneFileDocument` 层面的对象增删改与失败语义，不承担 GUI、Undo/Redo、层级、Prefab、运行时 Scene 同步。
- 上游背景直接影响本卡的点包括：对象字段仅限 `id/name/mesh/material/transform`；重复 id、空 mesh、非法引用格式、非有限 transform 值都必须返回明确失败；修改后的文档必须仍可保存并重新加载。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 编辑操作只处理文档数据，不触碰 `Scene` 运行时对象。
  - 失败继续通过显式结果表达，而不是异常驱动正常分支。
  - 校验范围仍只到 schema/字段/引用格式/transform 数值，不检查真实资源存在性。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把编辑操作放到 `Engine.App` 或 `Engine.Scene`。
  - 不允许新增 selection、Gizmo、Undo/Redo 等编辑器状态。
  - 不允许在本卡中引入层级、Prefab 或灯光编辑。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M11-2026-04-27 > SceneDataContents > 文件描述层` 中 `SceneFileObjectDefinition` 的 `Id/Name/Mesh/Material/Transform` 已是上游定稿字段，本卡的编辑操作必须围绕这些字段工作。
  - `PLAN-M11-2026-04-27 > SceneDataContents > 文件描述层` 中 `SceneFileTransformDefinition.Position/Rotation/Scale` 是 transform 编辑的文档形状，本卡不得另起一套私有编辑 DTO 替代主模型。

## 实施说明（ImplementationNotes）
- 先定义文档级编辑抽象或服务，明确输入为 `SceneFileDocument` 与对象编辑命令/参数，输出为显式结果。
- 覆盖最小操作集合：
  - 新增对象
  - 删除对象
  - 修改 `id`
  - 修改 `name`
  - 修改 `mesh/material`
  - 修改 `transform`
- 复用 M11.2 的校验/规范化逻辑，确保编辑后结果仍可保存并重新加载。
- 补测试覆盖成功编辑与失败路径，尤其是重复对象 id、空 mesh、非法 mesh/material 引用格式、非有限 transform 值。

## 设计约束（DesignConstraints）
- 不允许编辑操作直接操作 `SceneDescription` 或 `SceneGraphService` 运行时对象。
- 不允许在本卡内为“编辑方便”引入新的跨模块依赖。
- 不允许把失败分支写成隐式半成功状态，例如部分修改已落地但结果仍标记成功。
- 不允许把引用格式校验升级成真实资源探测。

## 失败与降级策略（FallbackBehavior）
- 对重复 object id、空 mesh、非法 mesh/material 引用、非有限 transform 值，必须返回明确失败并保持原文档可预测。
- 对单次编辑失败，允许保留原始文档不变，不做部分提交。
- 若实现中发现必须引入 GUI 状态、Undo/Redo 或运行时 Scene 同步才能继续，必须停工回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/FileModel/SceneFileObjectDefinition.cs`
  - `src/Engine.SceneData/FileModel/SceneFileTransformDefinition.cs`
  - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-003.md`
  - `.ai-workflow/tasks/task-sdata-004.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M11-2026-04-27.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - 必须在此处写入对应计划/里程碑引用位置
  - 必须明确该示例是“参考实现约束”还是“仅示意但字段关系已定”
  - 必须避免只写“见 M10 计划”这类模糊引用，需尽量定位到段落/小节/标题
  - `PLAN-M11-2026-04-27 > Milestones > M11.3`
  - `PLAN-M11-2026-04-27 > SceneDataContents > 文件描述层`
  - `PLAN-M11-2026-04-27 > PlanningDecisions > 校验范围`
  - 上述引用在本卡属于“字段关系已定的参考实现约束”，特别是 `SceneFileObjectDefinition` 与 `SceneFileTransformDefinition` 的字段边界。

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
- AllowedFiles:
  - 文档级对象编辑抽象/实现
  - 编辑失败结果类型或扩展
  - 对应 SceneData 测试
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 GUI、Undo/Redo、层级、Prefab
- 不同步 Scene 运行时对象
- 不修改 `Engine.App` 或 `Engine.Scene`
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Render/**`
  - `src/Engine.Contracts/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 编辑 API 是按命令对象还是按方法集暴露；只要失败语义清晰且不污染文件模型即可。
- 处理规则：
  - 若问题影响失败语义、文档原子性或模块边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 已明确本卡只做文档层编辑，不做 GUI 或运行时同步。
  - 字段边界、失败场景和复用约束已落卡。
  - 无需回看里程碑全文也能知道本卡不能顺手做层级/Undo/资源存在性校验。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 单模块主导，但包含多个编辑路径和多类失败语义。
  - 容易误扩张成编辑器状态或运行时同步逻辑。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.SceneData -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.SceneData -> Engine.Scene`
  - `Engine.SceneData -> Engine.App`
  - `Engine.SceneData -> Engine.Asset`
  - `Engine.SceneData -> Engine.Render`
  - `Engine.SceneData -> Engine.Platform`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scenedata.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: 对象增删改、重复 id、空 mesh、非法引用、非有限 transform 值测试通过
- Smoke: 修改后的文档可保存，并能重新加载为 `SceneDescription`
- Perf: 编辑操作保持文档级小步修改，不引入不必要的全量重建开销

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SDATA-005.md`
- ClosedAt: `2026-04-28 01:00`
- Summary: `Engine.SceneData` 已新增文档级对象编辑服务与显式编辑失败语义，支持对象增删、id/name、mesh/material、transform 修改，并验证编辑后文档可保存和重新加载。
- FilesChanged:
  - `src/Engine.SceneData/Editing/**`
  - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-sdata-005.md`
  - `.ai-workflow/archive/2026-04/TASK-SDATA-005.md`
  - `.ai-workflow/archive/archive-index.md`
- ValidationEvidence:
  - Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Debug --nologo -v minimal`）
  - Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Release --nologo -v minimal`）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --nologo -v minimal`；28 条通过）
  - Smoke: `pass`（编辑后文档经 `JsonSceneDocumentStore.Save` 保存，并由 `JsonSceneDescriptionLoader` 成功加载为 `SceneDescription`）
  - Perf: `pass`（编辑服务对文档对象做小步不可变更新，未引入运行时 Scene 同步或逐帧 IO）
- ModuleAttributionCheck: pass
