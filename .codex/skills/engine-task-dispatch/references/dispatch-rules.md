# 任务派发规则（三 Agent 模式）

按以下顺序执行，不得跳步。

## 0) 角色边界（强约束）

- `Plan Agent`：只做计划协商，输出目标、里程碑、优先级、风险与阶段边界。
- `Dispatch Agent`：只做需求拆分、任务卡生成、并行标注与依赖建模。
- `Execution Agent`：只做任务卡执行与验收，不新增拆分、不重写任务边界。
- `Human`：与 Plan Agent 协商计划，审核任务卡，决定执行顺序与并行策略，指派给 Execution Agent。

协作顺序必须为：`Plan Agent -> Dispatch Agent -> Execution Agent`。

### 三条硬约束（不可跳过）

1. 模板先行：每次请求都必须使用对应角色固定提示词模板；未使用模板时拒绝执行。
2. 字段齐全：任务卡必须含 `计划引用`、`里程碑引用`、`ParallelPlan`（`ParallelGroup`、`CanRunParallel`、`DependsOn`），否则拒绝进入执行流（兼容旧字段名：`PlanRef`、`MilestoneRef`）。
3. 越权即回退：收到跨角色请求时，仅返回 `请按 <AgentName> 职责重试`，不得给出越权产出。
4. 执行充分性：正式任务卡必须满足 `ExecutionReady=true`，且关键设计约束、失败语义、非目标、参考点已落到任务卡；否则不得进入执行流。
5. 充分性清单：Dispatch Agent 在输出正式任务卡前，必须逐项执行 `references/task-card-sufficiency-checklist.md`。
6. 示例仅作参考：`references/task-card-examples.md` 仅用于展示写法，不构成充分性上限；真实任务若更复杂，必须写得比示例更细。
7. 复杂度联动：Dispatch Agent 在输出正式任务卡前，必须执行 `references/task-card-complexity-scaling.md`。
8. 结构示例下沉：若计划/里程碑已提供示例数据结构、字段草图或结构关系示意，Dispatch Agent 必须把它落到任务卡的 `DecisionCarryOver` 与 `ExamplesOrReferences`，不得只写“见计划”或完全省略。

### 边界文档同步硬约束（不可跳过）

- 仅当满足以下任一条件时，才强制边界文档更新：
  - `BoundarySyncPlan.NewFilesExpected=true`
  - `FilesChanged` 中存在新增源码/测试文件
- 触发后必须同步更新 `.ai-workflow/boundaries/` 下至少一个边界文档，且包含 `Boundary Change Log`。
- 未触发时，边界文档更新为可选，不得作为执行阻塞项。

## 1) 受理规则

### 1.0) 轻量卡前置分流

- 当 Human 明确请求“简单迭代 / 小修 / 局部优化 / 小 bug / 回归修复”时，`Dispatch Agent` 必须先判定是否走 `QuickCard`。
- 仅当请求超出 `QuickCard` 适用范围时，才进入正式任务卡受理。
- 不得为了流程统一而把所有简单单子强制提升为正式任务卡。

- 没有 `计划引用`（兼容：`PlanRef`）的正式任务卡派发请求，拒绝。
- 没有 `里程碑引用`（兼容：`MilestoneRef`）的正式任务卡派发请求，拒绝。
- 没有清晰结果的请求，拒绝。
- 没有验收标准的正式任务卡请求，拒绝。
- 没有明确优先级（`P0|P1|P2|P3`）的请求，拒绝。
- 没有主模块归属（`PrimaryModule`）的请求，拒绝。
- 没有边界合同路径（`BoundaryContractPath`）的正式任务卡请求，拒绝。
- 没有路径白名单（`AllowedPaths`）的正式任务卡请求，拒绝。
- 没有基线引用（`BaselineRef`）的正式任务卡请求，拒绝。
- 没有并行信息（`ParallelGroup`、`CanRunParallel`、`DependsOn`）的正式任务卡请求，拒绝。
- 正式任务卡若预计超过 1-3 小时才能验证完成，强制拆分。

### 1.1) QuickCard 受理条件

- 满足 `references/quick-card-rules.md` 的全部适用条件时，可受理为 `QuickCard`。
- `QuickTask`：简单迭代、局部改善、命名或测试补齐。
- `QuickBug`：已知模块、可复现、`Expected/Actual` 明确。
- `BugInvestigation`：现象明确但根因不清。
- 若命中以下任一条件，禁止受理为 `QuickCard`：
  - 跨模块
  - 需要修改公开契约
  - 需要边界文档更新
  - 预计超过半天
  - 风险达到 `medium/high`
  - 需要专项 QA 或归档三件套

## 2) 拆分与并行规则（Dispatch Agent）

- 一张任务卡只对应一个结果。
- 一张任务卡必须且只能有一个主模块归属。
- 公开 API 变更必须单独建卡。
- 跨模块任务必须显式 `CrossModule=true` 并写明原因。
- 涉及源码/测试新增或重构的卡，必须默认遵守“一类一文件、一接口一文件”的文件组织约定；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外。
- 每张卡都必须标注：
  - `ParallelGroup`：可并行任务分组（例如 `G1`、`G2`）
  - `CanRunParallel`：是否允许并行执行
  - `DependsOn`：前置任务列表（无则空）
- Dispatch 输出必须给出“建议执行波次”（Wave），供人工选择并行批次。
- 每张任务卡必须携带 `计划引用` 与 `里程碑引用`，确保可追溯到计划阶段。
- Dispatch 必须把里程碑中与本卡直接相关的关键背景、关键决策、关键约束下沉到任务卡，禁止把“细节都在里程碑里”当作默认前提。
- 若计划/里程碑中已经定义了示例数据结构或字段关系，这些内容默认属于“关键决策”，不是可选背景。
- 若执行者仍需要回看里程碑全文才能开工，说明拆卡不合格，必须补卡而不是继续派发。
- Dispatch 在落卡前必须完成一次“任务卡充分性问答”；若关键项存在 `No`，必须先修卡。
- Dispatch 可参考 `task-card-examples.md` 组织写法，但不得把示例条目数、示例长度或示例结构当成完成标准。
- Dispatch 必须显式判断本卡复杂度是 `L1/L2/L3`，并确保信息量与该等级匹配。
- 当计划/里程碑中存在结构示例时，Dispatch 必须至少回答：
  - 该结构示例是否影响本卡实现
  - 哪些字段/关系/命名已经被定稿
  - 任务卡中对应落在 `DecisionCarryOver` 的哪几条
  - 任务卡中对应落在 `ExamplesOrReferences` 的哪几个引用

## 2.1) 优先级分层规则（Plan 主导）

- 主优先级由 `Plan Agent` 在计划中定义（里程碑级）。
- `Dispatch Agent` 只能在同一 `里程碑引用` 内做任务级微调排序。
- `Dispatch Agent` 不得跨里程碑上调/下调优先级。
- 若任务卡优先级与 `计划引用` 不一致，必须回退给 `Plan Agent` 更新计划，禁止静默改写。

## 3) 执行规则（Execution Agent）

- 只能执行已存在任务卡，不得擅自拆新任务。
- 允许执行已存在 `QuickCard`，但若执行中触发升卡条件，必须停止继续扩张并回退给 Dispatch Agent。
- 开工前必须先落盘任务卡状态变更：`Todo -> InProgress`。
- 完成实现待验证时必须落盘：`InProgress -> Verify`。
- 门禁通过待评审时必须落盘：`Verify -> Review`。
- 门禁失败或发现 must-fix 时必须落盘回退：`Verify/Review -> InProgress`。
- 不得扩大 `Scope/AllowedPaths`。
- 正式任务卡是 `Execution Agent` 的唯一执行依据；计划/里程碑只可作为追溯背景，不得覆盖或扩展任务卡。
- 发现任务卡缺字段或冲突时，回退给 Dispatch Agent 修卡。
- 发现“必须回看里程碑全文才能理解关键实现要求”时，必须判定为 `TaskCardInsufficient` 并回退给 Dispatch Agent 补卡。
- 发现轻量卡命中升卡条件时，必须回填 `EscalationReason` 并请求 Dispatch Agent 转正式任务卡。
- 若执行中出现新增需求，必须新建“派发请求”，由 Dispatch Agent 出新卡。
- 若任务目标与当前计划冲突，先回退给 Plan Agent 更新计划，再进入派发。
- 若发现任务优先级与计划不一致，只记录并回退，不得自行重排。
- 若本轮没有更新任务卡 `Status/Completion`，Execution 不得宣称“本卡已推进”或“本卡已完成当前阶段”。
- 相对路径解析必须按固定三步执行：
  - 先按仓库根目录解析
  - 再按相关 skill 目录解析（`.codex/skills/<skill>/...`）
  - 再在 `.codex/skills/**/references/` 进行文件名兜底搜索
- 仅当三步均失败时，才允许返回“路径未找到”。

## 4) 范围与基线规则

- 禁止隐式跨模块改动。
- 新建文件必须落在 `AllowedPaths` 内，否则视为越界。
- `AllowedPaths` 仅用于源码/测试文件范围校验，不用于边界文档校验。
- 边界文档改动必须依据 `BoundaryDocsToUpdate` 单独校验。
- 若任务与 `references/project-baseline.md` 冲突，必须先创建基线变更任务卡。

## 5) 门禁规则

- 无门禁证据，不允许流转。
- 任一门禁失败，任务回退到 `InProgress`。
- Gate 角色只报告 pass/fail，不得静默改写任务范围。
- 无归档证据，不允许 `Review -> Done`。
- `Todo -> InProgress` 前必须检查：`PrimaryModule`、`BoundaryContractPath`、`AllowedPaths` 已填写。
- `Todo -> InProgress` 前必须检查：`BaselineRef` 已填写且有效。
- `Todo -> InProgress` 前必须检查：并行字段已填写且依赖关系可解析。
- `Todo -> InProgress` 前必须检查：`ExecutionReady=true`。
- `Todo -> InProgress` 前必须检查：`MilestoneContext`、`DecisionCarryOver`、`ImplementationNotes`、`DesignConstraints`、`FallbackBehavior`、`ExamplesOrReferences`、`OpenQuestions` 已填写。
- `Todo -> InProgress` 前必须检查：`Completion=0`。
- `Todo -> InProgress` 后必须检查：任务卡已实际落盘为 `Status=InProgress`。
- `Verify -> Review` 前必须检查：`src/**`、`tests/**` 文件全部命中 `AllowedPaths`。
- `Verify -> Review` 前必须检查：边界文档文件不参与 `AllowedPaths` 校验。
- `Verify -> Review` 前必须检查：仅当触发边界同步条件时，边界文档改动才需要全部命中 `BoundaryDocsToUpdate`。
- `InProgress -> Verify` 前必须检查：任务卡已实际落盘为 `Status=Verify`。
- `Verify -> Review` 前必须检查：任务卡已实际落盘为 `Status=Review`。
- `InProgress/Verify/Review` 阶段必须检查：`Completion` 处于 `10-99`。
- `Review -> Done` 前必须检查：`ModuleAttributionCheck=pass`。
- `Review -> Done` 前必须检查：仅当触发边界同步条件时，`BoundaryDocsUpdated=true` 且变更日志已写入。
- `Review -> Done` 前必须检查：`Completion=100`。
- `Review -> Done` 仅允许 Human 复验通过后触发，Execution 不得自动推进到 `Done`。

### 5.1) QuickCard 最小门禁

- `Todo -> InProgress` 前必须检查：
  - `PrimaryModule` 已填写
  - `ScopeLimit` 已填写
  - `Completion=0`
- `InProgress -> Review` 前必须检查：
  - 变更仍处于单模块、单结果范围
  - 未命中升卡条件
- `Review -> Done` 前必须检查：
  - 至少一条 Build/最小编译证据
  - 至少一条测试证据或“无自动化测试，仅人工复现”说明
  - 一条简短自检说明
  - `Completion=100`

## 6) 阻塞处理

每个阻塞任务必须输出：

- 阻塞原因
- 影响范围
- 解阻动作
- 负责人和截止时间

若阻塞原因属于“任务卡信息不足”，必须额外输出：

- `FailureType=TaskCardInsufficient`
- 缺失的是哪类信息：
  - 上下文缺失
  - 决策缺失
  - 失败语义缺失
  - 非范围缺失
  - 参考点缺失
- 是否允许继续：`false`

若阻塞原因属于“状态未推进”，必须额外输出：

- `FailureType=GateFailed`
- `BlockedBy=Execution status transition not persisted`
- `RequiredFix`：
  - 先更新任务卡 `Status/Completion`
  - 再重新提交当前阶段证据
- `Owner=ExecutionAgent`

阻塞导致优先级变化时，必须输出更新后的 `P-level` 及原因。

## 7) 归档规则

- 任务关闭时，按 `references/archive-policy.md` 写入归档。
- 归档条目必须包含：任务 id、标题、优先级、负责人、改动内容、变更文件、验证摘要。
- 归档条目必须包含：主模块归属、边界合同路径、模块归属校验结果。
- 归档条目建议附带：实际并行执行批次与依赖偏差说明（如有）。
- 若任务取消，归档中必须写取消原因与替代任务 id。
- Execution 关单时必须原子完成：
    - 任务卡 `Status/Completion/Archive` 更新
    - 归档快照写入 `.ai-workflow/archive/<yyyy-mm>/<TASK-ID>.md`
    - 归档索引追加写入 `archive-index.md`
    - 看板任务移动到 `Done`
- 但 `Review -> Done` 的最终触发权不属于 Execution；Execution 仅负责准备关单材料，不得自签 `Done`。
- 任一步未完成时，不得宣称任务已完成。

## 8) 派发决策输出格式（Dispatch Agent）

必须使用以下结构：

```md
Decision: assign | defer | split | reject
计划引用: <plan-id or plan-doc-path>
里程碑引用: <M1|M2|...>
Priority: P0 | P1 | P2 | P3
PrimaryModule: <module>
BoundaryContractPath: <path>
BaselineRef: <references/project-baseline.md>
Parallel:
- ParallelGroup: <G1|G2|...>
- CanRunParallel: <true|false>
- DependsOn: [<TASK-ID>, ...]
WavePlan:
- Wave1: [<TASK-ID>, ...]
- Wave2: [<TASK-ID>, ...]
WhyNow: <one line>
GatePlan:
- Verify:
- Review:
Risk:
- <high|medium|low>: <one line>
ArchiveAction: <archive path and action>
NextAction: <exact next step>
```

## 8.1) 人类可见输出模式（默认）

- 默认使用 `Manager View`，只向 Human 展示：
  - `Summary`
  - `TaskIds`
  - `TaskGraph`（仅编号）
  - `WavePlan`（仅编号）
  - `DispatchDecision`（精简）
  - `TaskCardPaths`
- 完整任务卡正文必须写入 `.ai-workflow/tasks/<task-id>.md`，不在默认回复中展开。
- 仅当 Human 明确要求“展开任务卡全文”时，才允许回显全文。
- Human 提到“重新给出任务卡/再发一次任务卡”时，默认仍按 `Manager View` 处理，不视为“展开全文”指令。
- 只要任务卡已落盘，Dispatch 不得重复贴全文；Human 需自行到任务目录查看或显式下达“展开任务卡全文”。

### 8.1a) QuickCard 输出模式

- 当分流结果为 `QuickCard` 时，默认向 Human 输出精简视图：
  - `Decision`
  - `Type`
  - `QuickCardId`
  - `PrimaryModule`
  - `Priority`
  - `WhyQuick`
  - `EscalationGuard`
  - `QuickCardPath`
- 完整轻量卡正文写入 `.ai-workflow/quickcards/<quick-card-id>.md`，默认不回显全文。

## 8.2) 编号派发模式（Human -> Execution）

- Human 可只派发 `TaskId` 给 Execution Agent。
- Human 可只派发 `QuickCardId` 给 Execution Agent。
- Execution Agent 必须先读取对应的 `.ai-workflow/tasks/<task-id>.md` 或 `.ai-workflow/quickcards/<quick-card-id>.md`，并回显关键字段后再执行。

## 9) 输出前字段自检门禁（Dispatch Agent）

- Dispatch 在输出最终卡片前，必须逐卡执行字段自检。
- Dispatch 在字段自检后，必须逐卡执行一次充分性清单检查。
- Dispatch 在充分性清单后，必须逐卡执行一次复杂度与信息量匹配检查。
- 若输出正式任务卡，缺少以下字段时整批输出视为无效，必须先补齐再输出：
  - `计划引用`（或 `PlanRef`）
  - `里程碑引用`（或 `MilestoneRef`）
  - `MilestoneContext`
  - `DecisionCarryOver`
  - `ImplementationNotes`
  - `DesignConstraints`
  - `FallbackBehavior`
  - `ExamplesOrReferences`
  - `Acceptance`（Build/Test/Smoke/Perf 四项）
  - `AllowedPaths`
  - `DependsOn`（无依赖也需显式空列表）
  - `BoundarySyncPlan`
  - `OpenQuestions`
  - `ExecutionReadiness`
  - `Status`
  - `Completion`
- 若正式任务卡 `ExecutionReady=false`，视为脏卡，不得输出给 Human。
- 若充分性清单关键项任一失败，视为 `TaskCardInsufficient`，不得输出给 Human。
- 若任务复杂度明显高于示例而任务卡信息量未同步增加，也视为 `TaskCardInsufficient`。
- 若复杂度等级为 `L2/L3` 而任务卡信息密度仍停留在低复杂度表达，也视为 `TaskCardInsufficient`。
- 若计划/里程碑已有示例数据结构而任务卡未显式落引用与约束，也视为 `TaskCardInsufficient`。
- 若输出 `QuickCard`，至少必须具备：
  - `QuickCardId`
  - `Type`
  - `Priority`
  - `PrimaryModule`
  - `ScopeLimit`
  - `Status`
  - `Completion`
