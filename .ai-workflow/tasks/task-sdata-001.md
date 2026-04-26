# 任务: TASK-SDATA-001 M10 SceneData 模块与边界落地

## TaskId
`TASK-SDATA-001`

## 目标（Goal）
新建独立 `Engine.SceneData` 模块、解决方案引用与边界合同入口，固定 `SceneData -> Contracts` 依赖方向，并明确 `SceneData` 只负责场景文档模型而不是运行时场景状态。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M10-2026-04-25`

## 里程碑引用（兼容别名：MilestoneRef）
`M10.1`

## 执行代理（ExecutionAgent）
Exec-SceneData

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.SceneData

## 次级模块（SecondaryModules）
- Engine.Contracts
- Engine.App

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scenedata.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M10-G1`
- CanRunParallel: `false`
- DependsOn: `[]`

## 里程碑上下文（MilestoneContext）
- M10 的第一优先级是先把“场景文档层”从 `Engine.Scene`/`Engine.App` 中独立出来，否则后续 JSON loader 很容易被塞回现有模块。
- 本卡承担的是模块与边界落地，不承担 JSON 解析实现、不承担 Scene 运行时映射，也不承担 App 样例场景接线。
- 上游背景直接影响本卡的点只有三个：`SceneData -> Contracts` 是计划默认依赖方向；`SceneData` 不是运行时层；未来编辑器优先对接 `SceneData`，但它不是编辑器状态模型。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 新增独立 `Engine.SceneData` 模块，不把 JSON 解析塞进 `Engine.Scene` 或 `Engine.App`。
  - `Engine.SceneData` 只允许依赖 `Engine.Contracts`，不新增 `Engine.Core` 业务桥接职责。
  - `SceneData` 代表场景文档层，不代表运行时场景对象或编辑器状态。
  - 结构约定已被上游定稿：`PLAN-M10-2026-04-25 > SceneDataContents` 已明确后续会存在“文件描述层”和“规范化场景层”两层结构，本卡建立模块时必须给这两层预留清晰落点，不得把未来类型散落到 `Scene/App`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许让 `Engine.SceneData` 依赖 `Scene/Asset/Render/App` 中任何一个实现模块。
  - 不允许在本卡内把 `SceneData` 设计成运行时对象容器或场景缓存中心。
  - 不允许以“先跑通”为由把新模块退化为 `App` 或 `Scene` 内部子目录。
  - 计划里的结构示例虽然还未在本卡内全部实现，但其字段命名和分层方向已是后续实施约束，本卡不得建立与 `SceneFile*` / `Scene*Description` 双层模型相冲突的命名或目录结构。

## 实施说明（ImplementationNotes）
- 先在 `src/` 下建立 `Engine.SceneData` 工程，并把它接入解决方案与需要的测试工程引用。
- 再建立最小公开入口占位，至少让后续 `ISceneDescriptionLoader` 与描述模型有稳定落点。
- 同步让 `.ai-workflow/boundaries/engine-scenedata.md` 和边界目录说明落盘，避免后续任务卡继续引用不存在的边界文件。
- 最后补最小模块/依赖测试，验证 `SceneData` 当前只依赖 `Contracts`，没有反向耦合到运行时层。

## 设计约束（DesignConstraints）
- 不允许在本卡内引入 JSON 解析主逻辑，避免“边界搭建”和“实现主路径”混成一张卡。
- 不允许在 `Engine.SceneData` 中直接定义运行时 `SceneGraphService` 相关对象。
- 不允许把 `Engine.SceneData` 做成 `Engine.Scene` 的内部命名空间或嵌套目录。
- 不允许以 `Engine.Core` 承载本应属于 `SceneData` 的业务文档模型。

## 失败与降级策略（FallbackBehavior）
- 若发现现有边界文档结构不足以容纳 `Engine.SceneData`，允许先补边界文档和目录映射，再继续工程接线；不得绕过边界合同直接建模块。
- 若实现中发现必须新增 `SceneData -> 其他模块` 依赖才能继续，必须停工并回退修卡，不得私自打洞。
- 若测试工程接线暂时未能一次到位，可先完成模块建立和最小编译验证，但必须记录缺失测试点并在本卡内补齐，而不是转交下游卡兜底。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `AnsEngine.sln`
  - `src/Engine.Contracts/Engine.Contracts.csproj`
  - `src/Engine.Scene/Engine.Scene.csproj`
- 相关测试入口：
  - `tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj`
  - `tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj`
- 相关已有任务/归档/文档：
  - `.ai-workflow/plan-archive/2026-04/PLAN-M10-2026-04-25.md`
  - `.ai-workflow/boundaries/README.md`
  - `.ai-workflow/tasks/task-contract-001.md`
  - 结构示例定位：
    - `PLAN-M10-2026-04-25 > SceneDataContents > 文件描述层`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层`
    - `PLAN-M10-2026-04-25 > HandoffToDispatch > 主模块边界`
  - 约束说明：
    - 上述计划段落中的结构示例在本卡属于“参考实现约束”，至少其分层关系和命名前缀（`SceneFile*` / `Scene*Description`）已定，执行时不得随意改写。

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
- AllowedFiles:
  - `Engine.SceneData` 工程骨架
  - solution/project 引用接线
  - 最小边界测试与模块依赖校验
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现 JSON 解析主逻辑
- 不实现 Scene 运行时对象
- 不实现 Render 提交路径
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.Asset/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `none`
- 处理规则：
  - 若实现中发现 `SceneData` 需要额外依赖 `Asset/Scene/App/Render` 才能站住，必须回退，不得自行脑补调整边界。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 已明确本卡只做模块与边界落地，不与 JSON 主路径实现混卡。
  - 依赖方向、禁止做法、最小实施顺序和参考入口已落卡。
  - 执行者无需回看 M10 全文，也能知道本卡不能把 `SceneData` 做成运行时层。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 新增模块与边界方向，存在明显的职责误放风险。
  - 需要同时处理解决方案接线、边界合同和最小测试验证。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.SceneData -> Engine.Contracts`
  - `Engine.Contracts -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.SceneData -> Engine.Scene`
  - `Engine.SceneData -> Engine.Asset`
  - `Engine.SceneData -> Engine.Render`
  - `Engine.SceneData -> Engine.App`
  - 将编辑器状态、运行时对象或文件 IO 流程编排塞进 `SceneData`

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
  - `.ai-workflow/boundaries/README.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；新增 `Engine.SceneData` 最小测试、模块依赖与边界测试通过
- Smoke: 组合根可引用 `SceneData` loader 抽象但不要求画面变化
- Perf: 引入模块后无逐帧初始化或运行期开销漂移

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SDATA-001.md`
- ClosedAt: `2026-04-26 00:00`
- Summary:
  - 新建独立 `Engine.SceneData` 模块并接入 solution，固定 `SceneData -> Contracts` 依赖方向。
  - 落地最小公开入口与描述模型占位，为后续 `TASK-SDATA-002` 的 JSON 主路径提供稳定落点。
  - 补齐最小模块依赖测试与边界文档，确认 `SceneData` 不反向耦合 `Scene/Asset/Render/App` 运行时层。
- FilesChanged:
  - `AnsEngine.sln`
  - `src/Engine.SceneData/**`
  - `tests/**`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/README.md`
- ValidationEvidence:
  - Build(Debug): `pass`（Human 于 `2026-04-26` 确认 M10.1 模块落地验收通过）
  - Build(Release): `pass`（同上）
  - Test: `pass`（Human 于 `2026-04-26` 确认最小模块依赖与边界测试通过）
  - Smoke: `pass`（组合根可引用 `SceneData` loader 抽象且未破坏现有运行路径）
  - Perf: `pass`（模块引入后未增加逐帧初始化或运行期开销）
- ModuleAttributionCheck: pass
