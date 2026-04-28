# 任务: TASK-SDATA-004 M11 校验复用与 load-save-load 往返稳定

## TaskId
`TASK-SDATA-004`

## 目标（Goal）
在 `Engine.SceneData` 抽出或复用统一的校验/规范化规则，使 `SceneFileDocument` 在保存后可被 `JsonSceneDescriptionLoader` 再次加载为等价 `SceneDescription`，完成 load-save-load 往返稳定闭环。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M11-2026-04-27`

## 里程碑引用（兼容别名：MilestoneRef）
`M11.2`

## 执行代理（ExecutionAgent）
Exec-SceneData

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.SceneData

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scenedata.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M11-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SDATA-003`

## 里程碑上下文（MilestoneContext）
- M11 第二步的核心不是“能保存文件”本身，而是保存后的文件仍然满足运行时主链路的可加载与语义等价要求。
- 本卡承担的是校验/规范化复用与往返稳定，不承担对象级编辑 API，也不承担 Scene/App 新能力扩张。
- 上游背景直接影响本卡的点包括：默认值策略必须可预测；`version/scene id/name/camera/objects/mesh/material/transform` 语义要稳定；校验范围只到 schema、必填、重复 id、引用格式和 transform 数值，不检查真实资源存在性。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 运行时 `JsonSceneDescriptionLoader` 与文档 store 不能各自维护一套分叉的校验/规范化规则。
  - 保存后重新加载得到的 `SceneDescription` 必须与保存前语义等价。
  - 默认值逻辑仍归属 `Engine.SceneData`，不得外溢到 `Scene` 或 `App`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把资源存在性检查引入 `SceneData` 校验。
  - 不允许为通过往返测试而修改 `Scene`/`App` 去兜底默认值。
  - 不允许把规范化目标改成直接序列化 `SceneDescription`。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M11-2026-04-27 > SceneDataContents > 文件描述层` 与 `> 规范化场景层` 中 `Version/Scene/Id/Name/Camera/Objects/Mesh/Material/Transform` 关系已定，本卡往返测试必须围绕这些字段语义构建。
  - `PLAN-M11-2026-04-27 > PlanningDecisions > 默认值策略` 中 `material://default`、`LocalTransform=Identity`、缺省相机等策略延续为本卡的等价判断基线。

## 实施说明（ImplementationNotes）
- 先梳理当前 `JsonSceneDescriptionLoader` 的校验与默认值逻辑，抽出可被文档读写复用的公共规则或公共转换入口。
- 让文档保存后的文件经过再次加载时，能够得到等价的 `SceneDescription`，重点覆盖：
  - `version`
  - `scene id/name`
  - `camera`
  - `objects`
  - `mesh/material`
  - `LocalTransform`
- 补 load-save-load 测试，并把“等价”的判定标准写实到测试中，而不是仅检查文件可重新解析。
- 同时验证默认值回填后的输出不把规范化责任转嫁给 `Scene` 或 `App`。

## 设计约束（DesignConstraints）
- 不允许在 loader 和 document store 中复制粘贴两套校验代码长期并存。
- 不允许把等价判断降级成“只要不崩就算通过”。
- 不允许在 `SceneData` 中加入真实 mesh/material 资源存在性探测。
- 不允许修改 `SceneDescription` 公开语义来配合保存器偷懒。

## 失败与降级策略（FallbackBehavior）
- 对于重复对象 id、缺少必填字段、非法引用格式、非有限 transform 数值等输入，必须显式失败，不做静默修正。
- 对于缺省 `materialId`、缺省 `LocalTransform`、缺省相机这类计划已定义的默认值场景，允许规范化并继续成功。
- 若实现中发现等价判定标准无法仅靠 `SceneData` 自身建立，必须回退修卡，不得通过改 `Scene`/`App` 行为绕过。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
  - `src/Engine.SceneData/Descriptions/SceneDescription.cs`
  - `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
  - `src/Engine.SceneData/Descriptions/SceneTransformDescription.cs`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-003.md`
  - `.ai-workflow/archive/2026-04/TASK-SDATA-002.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M11-2026-04-27.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - 必须在此处写入对应计划/里程碑引用位置
  - 必须明确该示例是“参考实现约束”还是“仅示意但字段关系已定”
  - 必须避免只写“见 M10 计划”这类模糊引用，需尽量定位到段落/小节/标题
  - `PLAN-M11-2026-04-27 > Milestones > M11.2`
  - `PLAN-M11-2026-04-27 > SceneDataContents > 文件描述层`
  - `PLAN-M11-2026-04-27 > SceneDataContents > 规范化场景层`
  - `PLAN-M11-2026-04-27 > PlanningDecisions > 校验范围`
  - `PLAN-M11-2026-04-27 > PlanningDecisions > 保存对象`
  - 上述引用在本卡属于“字段关系与默认值语义已定的参考实现约束”。

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
- AllowedFiles:
  - 校验/规范化复用逻辑
  - load-save-load 往返测试
  - 默认值策略等价性验证
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现对象级增删改 API
- 不新增 App 工具入口
- 不修改 `Engine.Scene` 或 `Engine.App` 的默认值责任
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Render/**`
  - `src/Engine.Contracts/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - “等价”是否按字段完全一致还是按规范化后语义一致判断；若影响测试结论，需要先在本卡内明确统一标准。
- 处理规则：
  - 若问题影响验收口径或默认值策略边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 已明确往返稳定的目标是 `SceneDescription` 语义等价，而不是文件字节级相等。
  - 默认值、失败类型和禁止路线已落卡。
  - 无需回看里程碑全文也能知道本卡不能把资源存在性校验塞进 `SceneData`。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 同时涉及校验复用、默认值语义、运行时等价性和回归测试口径。
  - 若判断标准写不清，很容易出现“保存可用但主链路语义漂移”的假通过。
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
- Test: load-save-load、默认值策略、重复 id、非法引用、非有限 transform 数值测试通过
- Smoke: 保存后的 `.scene.json` 仍可被 `JsonSceneDescriptionLoader` 成功加载为等价 `SceneDescription`
- Perf: 不引入重复规范化或多套校验造成的明显回归

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SDATA-004.md`
- ClosedAt: `2026-04-28 01:00`
- Summary: `Engine.SceneData` 已抽出 `SceneFileDocumentNormalizer` 统一复用校验、默认值和规范化规则；保存后的 `.scene.json` 可重新加载为语义等价的 `SceneDescription`。
- FilesChanged:
  - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
  - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-sdata-004.md`
  - `.ai-workflow/archive/2026-04/TASK-SDATA-004.md`
  - `.ai-workflow/archive/archive-index.md`
- ValidationEvidence:
  - Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Debug --nologo -v minimal`）
  - Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Release --nologo -v minimal`）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --nologo -v minimal`；28 条通过）
  - Smoke: `pass`（load-save-load 测试验证保存后文件可被 `JsonSceneDescriptionLoader` 加载为等价 `SceneDescription`）
  - Perf: `pass`（loader/store 复用 normalizer，未引入多套重复规范化主路径）
- ModuleAttributionCheck: pass
