# 任务: TASK-SDATA-003 M11 SceneData 文档读写接口与 JSON store

## TaskId
`TASK-SDATA-003`

## 目标（Goal）
在 `Engine.SceneData` 新增文档级读写入口与 JSON store，使 `SceneFileDocument` 可以从 `.scene.json` 文件读取、保存并通过显式结果表达失败，同时保留现有 `ISceneDescriptionLoader` 作为运行时规范化加载入口。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M11-2026-04-27`

## 里程碑引用（兼容别名：MilestoneRef）
`M11.1`

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
- ParallelGroup: `M11-G1`
- CanRunParallel: `false`
- DependsOn: `[]`

## 里程碑上下文（MilestoneContext）
- M11 的第一优先级是把“编辑器数据底座”收敛在 `Engine.SceneData` 内，而不是让保存逻辑回流到 `Scene` 或 `App`。
- 本卡承担的是文档级读取/保存入口与 JSON store，不承担文档校验复用全收口、不承担对象级编辑操作，也不承担新工具宿主模块。
- 上游背景直接影响本卡的点包括：M11 明确不新增 `Engine.Editor`/`Engine.Tools`；`ISceneDescriptionLoader` 继续是运行时入口；保存对象优先是 `SceneFileDocument`，而不是规范化 `SceneDescription`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.SceneData` 是唯一允许承载 scene JSON 读写的模块。
  - 新增文档读写入口，例如 `ISceneDocumentStore` 或等价命名，但不把它混进现有 `ISceneDescriptionLoader`。
  - 编辑器状态、GUI、Gizmo、Undo/Redo、资源浏览器都不属于 M11。
  - `SceneData -> Contracts` 是唯一项目依赖方向，不能为了读写方便引入 `Scene/App/Asset/Render/Platform`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把保存逻辑写进 `Engine.App` 或 `Engine.Scene`。
  - 不允许把 `ISceneDescriptionLoader` 改造成编辑服务。
  - 不允许让 `Engine.SceneData` 去校验资源是否真实存在。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M11-2026-04-27 > SceneDataContents > 文件描述层` 中 `SceneFileDocument` / `SceneFileDefinition` / `SceneFileObjectDefinition` / `SceneFileCameraDefinition` / `SceneFileTransformDefinition` 已是上游定稿的文档模型主形状，本卡读写入口必须围绕这些结构工作。
  - `PLAN-M11-2026-04-27 > SceneDataContents > 加载与失败语义` 中 `SceneDescriptionLoadResult` 代表运行时加载结果，本卡若新增文档级结果类型，必须与其职责边界清晰分离，不得混淆。

## 实施说明（ImplementationNotes）
- 先在 `Engine.SceneData` 内建立文档级读写抽象，例如 `ISceneDocumentStore` 与对应结果/失败类型，保证职责只覆盖 `SceneFileDocument` 的 load/save。
- 再实现 JSON store，覆盖：
  - 读取 `.scene.json` 为 `SceneFileDocument`
  - 保存 `SceneFileDocument` 到磁盘
  - 缺失文件、非法 JSON、写入失败等显式失败路径
- 明确 `JsonSceneDescriptionLoader` 仍保持“文件描述层 -> 规范化场景层”的运行时职责，不直接承担文档保存。
- 最后补 SceneData 专项测试，至少覆盖：读取成功、保存成功、缺失文件、非法 JSON、只读/写入失败路径。

## 设计约束（DesignConstraints）
- 不允许新增 `Engine.Editor`、`Engine.Tools` 或类似宿主模块。
- 不允许在 `Engine.SceneData` 内创建运行时场景对象或 world transform 缓存。
- 不允许让 JSON store 依赖 `Engine.Scene`、`Engine.App`、`Engine.Asset`、`Engine.Render`、`Engine.Platform`。
- 不允许把文档保存目标改成 `SceneDescription`，从而绕过 `SceneFileDocument` 文件模型。

## 失败与降级策略（FallbackBehavior）
- 对缺失文件、非法 JSON、写入失败等可恢复问题，返回文档级显式失败结果，不依赖异常作为主流程分支。
- 对路径存在但 JSON 结构不合法的情况，允许返回失败并保留原文件不变，不做部分写入。
- 若实现中发现必须扩展 `Engine.Contracts` 或引入新模块才能继续，必须停工回退修卡，不得私自打洞。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/Abstractions/ISceneDescriptionLoader.cs`
  - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
  - `src/Engine.SceneData/FileModel/SceneFileDocument.cs`
  - `src/Engine.SceneData/FileModel/SceneFileDefinition.cs`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
- 相关已有任务/归档/文档：
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M11-2026-04-27.md`
  - `.ai-workflow/archive/2026-04/TASK-SDATA-002.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - 必须在此处写入对应计划/里程碑引用位置
  - 必须明确该示例是“参考实现约束”还是“仅示意但字段关系已定”
  - 必须避免只写“见 M10 计划”这类模糊引用，需尽量定位到段落/小节/标题
  - `PLAN-M11-2026-04-27 > Milestones > M11.1`
  - `PLAN-M11-2026-04-27 > PlanningDecisions > 保存对象`
  - `PLAN-M11-2026-04-27 > SceneDataContents > 文件描述层`
  - `PLAN-M11-2026-04-27 > SceneDataContents > 加载与失败语义`
  - 上述引用在本卡属于“字段关系已定的参考实现约束”，特别是 `SceneFileDocument` 文件模型与运行时 loader 角色分离。

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
- AllowedFiles:
  - 文档读写抽象与 JSON store
  - 文档级失败结果类型
  - 对应 SceneData 测试
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 load-save-load 语义等价全收口
- 不实现对象级增删改编辑 API
- 不实现 GUI 或新工具宿主模块
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Render/**`
  - `src/Engine.Contracts/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 文档级结果类型命名是否采用 `SceneDocument*` 还是 `SceneFile*Store*` 前缀；只要不和运行时 `SceneDescriptionLoadResult` 混淆即可。
- 处理规则：
  - 若问题影响职责边界、失败语义归属或公开接口清晰度，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确文档级读写与运行时 loader 的职责切分。
  - 文件模型、失败语义和禁止路线都已落卡。
  - 无需回看里程碑全文也能知道本卡不能把保存逻辑塞到 App/Scene。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及新接口、失败语义、文件 IO 和与既有 loader 的职责切分。
  - 若路线选错，会直接破坏 `SceneData` 边界并污染运行时加载入口。
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
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scenedata.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: 文档读取、保存、缺失文件、非法 JSON、写入失败路径测试通过
- Smoke: 不要求 GUI；至少能在测试或最小工具路径中完成一次 `SceneFileDocument` 读取与保存
- Perf: 读写仅发生在显式文档操作阶段，不引入运行时逐帧 IO

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SDATA-003.md`
- ClosedAt: `2026-04-28 01:00`
- Summary: `Engine.SceneData` 已新增文档级 `ISceneDocumentStore`、JSON store、读写结果与失败语义；`SceneFileDocument` 可读取、保存并保留 `ISceneDescriptionLoader` 的运行时规范化职责。
- FilesChanged:
  - `src/Engine.SceneData/Abstractions/ISceneDocumentStore.cs`
  - `src/Engine.SceneData/DocumentStore/**`
  - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-sdata-003.md`
  - `.ai-workflow/archive/2026-04/TASK-SDATA-003.md`
  - `.ai-workflow/archive/archive-index.md`
- ValidationEvidence:
  - Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Debug --nologo -v minimal`）
  - Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Release --nologo -v minimal`）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --nologo -v minimal`；28 条通过）
  - Smoke: `pass`（测试覆盖 `SceneFileDocument` 保存后再读取）
  - Perf: `pass`（读写仅由显式 document store 操作触发，无运行时逐帧 IO）
- ModuleAttributionCheck: pass
