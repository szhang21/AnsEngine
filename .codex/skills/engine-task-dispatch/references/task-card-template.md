# 任务卡模板（Dispatch Agent 专用）

请严格使用以下结构。  
说明：任务卡由 `Dispatch Agent` 生成；`Execution Agent` 只消费任务卡执行，不负责拆分。

```md
# 任务: <ID> <短标题>

## 目标（Goal）
一句话、可验证的目标。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`<plan-id or path>`

## 里程碑引用（兼容别名：MilestoneRef）
`M1 | M2 | ...`

## 执行代理（ExecutionAgent）
<agent-name>

## 优先级（Priority）
P0 | P1 | P2 | P3
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.Render | Engine.Scene | Engine.Asset | Engine.Platform | Engine.App | 其他（需说明）

## 次级模块（SecondaryModules）
- （可选）

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/<module>.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G1 | G2 | ...`
- CanRunParallel: `true | false`
- DependsOn:
  - `<TASK-ID>`
  - `<TASK-ID>`

## 里程碑上下文（MilestoneContext）
- 仅摘录与本卡直接相关的里程碑背景
- 不得整段复制里程碑全文
- 需要回答：
  - 为什么现在做这张卡
  - 这张卡在当前里程碑中承担什么作用
  - 哪些上游背景会直接影响本卡实现

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `<decision>`
- 本卡执行时不得推翻的既定取舍：
  - `<constraint>`
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - 必须在此处明确写出“哪些结构约定已被上游定稿，执行时不得自行改写”

## 实施说明（ImplementationNotes）
- 本卡建议的实现入口
- 关键实现步骤或拆分顺序
- 必须覆盖的关键路径

## 设计约束（DesignConstraints）
- 明确不能怎么做
- 不允许跨越的边界
- 不允许擅自扩张的方向

## 失败与降级策略（FallbackBehavior）
- 若关键路径失败，应该如何降级
- 哪些失败可以回退继续运行
- 哪些失败必须显式报错/回退修卡

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `<path>`
- 相关测试入口：
  - `<path>`
- 相关已有任务/归档/文档：
  - `<path or id>`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - 必须在此处写入对应计划/里程碑引用位置
  - 必须明确该示例是“参考实现约束”还是“仅示意但字段关系已定”
  - 必须避免只写“见 M10 计划”这类模糊引用，需尽量定位到段落/小节/标题

## 范围（Scope）
- AllowedModules:
- AllowedFiles:
- AllowedPaths:
> 说明：`AllowedPaths` 仅用于源码/测试改动范围，不包含边界文档路径。

## 跨模块标记（CrossModule）
true | false

## 非范围（OutOfScope）
- 
- 
- OutOfScopePaths:

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `<question or none>`
- 处理规则：
  - 若影响范围/边界/验收，必须先回退，不得自行脑补

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true | false`
- WhyReady:
  - 仅凭任务卡即可实施
  - 无需回看里程碑全文即可理解关键约束
  - 失败语义、非目标、边界已落卡
- MissingInfo:
  - `<none | missing item>`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
- ForbiddenDependsOn:

## 边界变更请求（BoundaryChangeRequest）
- Required: `true | false`
- Status: `none | pending | approved | rejected`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:
> 说明：仅当 `DependencyContract` 超出 `BoundaryContractPath` 允许范围时，`Required=true` 且必须先经 Human 批准。

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true | false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/<module>.md`
- ChangeLogRequired: `true`
> 说明：`BoundaryDocsToUpdate` 为独立规则，不受 `AllowedPaths` 限制。
> 触发条件：仅当 `NewFilesExpected=true` 或执行中实际新增源码/测试文件时，才强制执行边界文档更新。

## 验收标准（Acceptance）
- Build:
- Test:
- Smoke:
- Perf:
- CodeQuality: （QA 验证卡必填；非 QA 卡可选）
  - NoNewHighRisk: `true | false`
  - MustFixCount: `<number>`
  - MustFixDisposition: `none | accepted | follow-up-created`
- DesignQuality: （QA 验证卡必填；非 QA 卡可选）
  - DQ-1 职责单一（SRP）: `pass | fail`
  - DQ-2 依赖反转（DIP）: `pass | fail`
  - DQ-3 扩展点保留（OCP-oriented）: `pass | fail`
  - DQ-4 开闭性评估（可选）: `pass | warn | fail`

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Todo | InProgress | Verify | Review | Done
> 说明：Execution 仅负责推进到 `Review` 并准备归档三件套；`Review -> Done` 只能由 Human 复验通过后显式触发，Workflow Steward 仅在 Human 明确 `关单 <TaskId>` 后代执行机械同步，不拥有独立签收权。
> 执行要求：Execution 开工后必须先把任务卡落盘到 `InProgress`；实现完成待验证时必须落盘到 `Verify`；验证通过待评审时必须落盘到 `Review`；门禁失败或发现 must-fix 时必须回退并落盘原因。

## 完成度（Completion）
`0-100`（整数百分比）

## 缺陷回流字段（Defect Triage）
- FailureType: `AcceptanceDispute | PostAcceptanceBug | TaskCardInsufficient | Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending | pass | fail`

## 归档（Archive）
- ArchivePath:
- ClosedAt:
- Summary:
- FilesChanged:
- ValidationEvidence:
- ModuleAttributionCheck: pass | fail
```

## 校验规则

- 缺少 `TaskSource=DispatchAgent` 视为任务卡无效。
- 缺少 `计划引用`（或 `PlanRef`）视为任务卡无效。
- 缺少 `里程碑引用`（或 `MilestoneRef`）视为任务卡无效。
- 缺少 `ExecutionAgent` 视为任务卡无效。
- 缺少 `ParallelPlan` 任一关键字段（`ParallelGroup`、`CanRunParallel`、`DependsOn`）视为任务卡无效。
- 缺少 `BoundarySyncPlan` 视为任务卡无效。
- 缺少 `MilestoneContext` 视为任务卡无效。
- 缺少 `DecisionCarryOver` 视为任务卡无效。
- 缺少 `ImplementationNotes` 视为任务卡无效。
- 缺少 `DesignConstraints` 视为任务卡无效。
- 缺少 `FallbackBehavior` 视为任务卡无效。
- 缺少 `ExamplesOrReferences` 视为任务卡无效。
- 缺少 `OutOfScope` 视为任务卡无效。
- 缺少 `OpenQuestions` 视为任务卡无效。
- 缺少 `ExecutionReadiness` 视为任务卡无效。
- 缺少 `Acceptance` 视为任务卡无效。
- 当任务为 QA 验证卡（`TaskId` 含 `TASK-QA-` 或 `ExecutionAgent=Exec-QA`）时，`Acceptance` 缺少 `CodeQuality` 或 `DesignQuality` 视为无效。
- 缺少 `Priority` 视为任务卡无效。
- 缺少 `PrimaryModule` 视为任务卡无效。
- 缺少 `BoundaryContractPath` 视为任务卡无效。
- 缺少 `BaselineRef` 视为任务卡无效。
- 缺少 `AllowedPaths` 视为任务卡无效。
- `DependencyContract.AllowedDependsOn` 超出边界合同允许范围且 `BoundaryChangeRequest.Status` 非 `approved`，视为任务卡无效。
- 缺少 `Completion` 视为任务卡无效。
- `ExecutionReady` 不是 `true` 的任务卡不得进入执行流。
- 若计划/里程碑已明确给出示例数据结构，而任务卡未在 `DecisionCarryOver` 或 `ExamplesOrReferences` 中落下对应引用与约束，视为任务卡无效。
- 缺陷回流场景缺少 `DetectedAt` 视为任务卡无效。
- 当 `FailureType=AcceptanceDispute` 且走 `ReopenOriginal` 时，缺少 `ReopenReason` 视为无效。
- 当走 `CreateBugCard|CreateVerifyCard` 时，缺少 `OriginTaskId` 视为无效。
- 缺少 `HumanSignoff` 视为任务卡无效。
- 任务关闭时缺少 `Archive` 字段视为无效。
- 任务涉及跨模块改动但 `CrossModule` 不是 `true` 视为无效。
- 若一张卡包含多个结果，必须拆卡。
- 当 `NewFilesExpected=true` 时，归档若无边界文档更新证据视为无效。
- 代码文件（`src/**`、`tests/**`）必须命中 `AllowedPaths`。
- 边界文档更新必须命中 `BoundaryDocsToUpdate`，不以 `AllowedPaths` 为判定依据。
- 当 `NewFilesExpected=false` 且无新增源码/测试文件时，边界文档更新不作为阻塞条件。
- `Status=Todo` 时 `Completion` 必须为 `0`。
- `Status=Done` 时 `Completion` 必须为 `100`。
- `Status=InProgress|Verify|Review` 时 `Completion` 必须为 `10-99`。
- Execution 若未同步更新 `Status/Completion`，不得宣称“已开始 / 已完成实现 / 已完成验证 / 已进入评审”。
- QA 验证卡若 `CodeQuality.MustFixCount>0` 且 `MustFixDisposition=none`，不得流转到 `Review`。
