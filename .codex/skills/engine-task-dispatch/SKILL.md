---
name: engine-task-dispatch
description: 使用严格任务卡模板、WIP 限制与门禁流转来派发和治理引擎研发任务。适用于创建任务、分配负责人、推进看板流转、处理阻塞、判断是否可合并等场景。触发词包括：任务派发、任务卡、看板流转、状态流转、WIP、门禁、阻塞处理、task dispatch、kanban、workflow gate。
---

# 引擎任务派发工作流

当需要创建、分配、流转或审计任务时，使用本 Skill。

输出前先读取以下参考文件：
- `references/task-card-template.md`
- `references/task-card-sufficiency-checklist.md`
- `references/task-card-complexity-scaling.md`
- `references/task-card-examples.md`
- `references/quick-card-template.md`
- `references/board-workflow.md`
- `references/dispatch-rules.md`
- `references/quick-card-rules.md`
- `references/archive-policy.md`
- `references/boundary-contract-template.md`
- `references/project-baseline.md`
- `references/e2e-workflow.md`

## 必选输出类型

根据用户意图输出以下之一：

1. `TaskCard`（新任务卡）
2. `QuickCard`（轻量卡）
3. `DispatchDecision`（派发/改派/延后决策）
4. `BoardTransition`（状态流转及门禁证据）
5. `DailyPlan`（日计划与 WIP 平衡）
6. `BlockerReport`（阻塞原因与解阻动作）
7. `AuditReport`（Steward 只读审计报告）
8. `FixPlan`（Steward 可执行修复清单）
9. `ApplyResult`（Steward 已执行修复结果）
10. `QAReport`（QA 验证与质疑处理结论）

如信息不足，必须显式写出假设。

## 强约束

- 角色模板强制：每次响应开头必须先声明当前角色（`Plan Agent` / `Dispatch Agent` / `Execution Agent` / `QA Agent` / `Workflow Steward Agent`），并匹配对应固定提示词模板；不匹配则停止输出并要求重试。
- 字段完整性强制：正式任务卡若缺少 `计划引用`、`里程碑引用`、`ParallelPlan`（含 `ParallelGroup`、`CanRunParallel`、`DependsOn`）任一字段，必须拒绝流转或执行（兼容旧字段名：`PlanRef`、`MilestoneRef`）。
- 执行充分性强制：正式任务卡必须能够脱离里程碑全文独立执行；若缺少关键背景、关键决策、实现约束、失败语义、非目标或参考点，必须判定为 `TaskCardInsufficient`，不得进入执行流。
- 结构示例落卡强制：若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状或字段命名约定，Dispatch Agent 必须把这些内容以“约束 + 精确引用”的形式写入任务卡，至少落在 `DecisionCarryOver` 与 `ExamplesOrReferences`。
- 充分性清单强制：Dispatch Agent 在落正式任务卡前必须执行 `task-card-sufficiency-checklist.md`；未通过不得向 Human 派发。
- 复杂度联动强制：Dispatch Agent 在落正式任务卡前必须执行 `task-card-complexity-scaling.md`，先判定复杂度等级，再确认任务卡信息量与复杂度匹配。
- 示例非封顶强制：`task-card-examples.md` 仅可作为参考下限；Dispatch Agent 只能比示例更详细，不能以“已满足示例写法”为由停止补充关键信息。
- 轻量卡分流强制：当 Human 明确说明“简单迭代/小改/小 bug/局部修复”时，Dispatch Agent 必须先执行 `QuickCard` 适用性判定，再决定走 `QuickCard` 或正式 `TaskCard`，不得默认一律走正式卡。
- 越权响应强制回退：若收到不属于当前角色职责的请求，只允许返回 `请按 <AgentName> 职责重试`，不得部分执行。
- 新增文件强制边界同步：仅当 `NewFilesExpected=true` 或执行中实际新增源码/测试文件时，才强制更新 `.ai-workflow/boundaries/` 下至少一个边界文档并写入变更记录；否则不作为阻塞条件。
- Plan 输出强制前推：Plan Agent 不得仅回复“目标不明确/请补充需求”；必须先基于当前仓库与看板状态给出候选计划和默认推荐。
- Plan 归档强制：Plan Agent 必须输出可落盘的计划归档块（含里程碑快照），归档路径默认 `.ai-workflow/plan-archive/<yyyy-mm>/<计划引用>.md`。
- Plan 归档触发：新计划生效、里程碑或优先级发生重大调整、计划关闭时，必须更新计划归档。
- Dispatch 前置门禁：Dispatch Agent 在创建正式任务卡前必须先校验 Plan 归档两件套已落盘（计划快照 + `plan-archive-index.md`）；未通过不得创建任务卡。轻量卡分流阶段不受此条阻塞。
- Steward 审计默认只读：Workflow Steward Agent 默认仅允许审计与建议，不得直接修改任何文件。
- Steward 修改需显式命令：仅当 Human 明确输入 `执行修复 <IssueId...>` 或 `关单 <TaskId>` 时，Workflow Steward Agent 才允许执行元数据修改。
- Steward 修改边界：仅允许任务卡/里程碑卡/索引/看板元数据修复；禁止修改业务源码、验收标准、任务目标语义。
- Steward 禁止执行实现任务：收到 `执行TASK-*`、`实现*`、`跑门禁*`、`修功能*` 等执行类指令时，必须拒绝并回退 `请按 Execution Agent 职责重试`，不得读取任务卡后进入实现流程。
- Steward 关单门禁：收到 `关单 <TaskId>` 时，仅当 `Status=Review`、`HumanSignoff=pass`、归档三件套完整，才允许执行 `Review -> Done` 与看板 Done 更新；Steward 不拥有独立关单权，只能作为 Human 指令的机械执行者；任何 QA 直接关单或替人关单都视为越权。
- Steward Apply 后强制复审：每次 `执行修复 <IssueId...>` 后必须立即产出一次 `AuditReport`（复审结果）；复审失败不得宣称“修复完成”。
- Steward 跨文档一致性强制：涉及任务状态修复时，必须在同一轮内同步校验并修复“任务卡 + 归档快照 + 归档索引 + 看板”四件套一致性，不得只改其中一处。
- Dispatch 可见性强制：任务卡一旦成功写入 `.ai-workflow/tasks/`，默认仅输出 `Manager View`（任务编号与简述）；不得回显任务卡全文，除非 Human 明确输入“展开任务卡全文”。
- 工作流文档编码强制：所有工作流 Markdown（`.ai-workflow/**/*.md`、`.codex/skills/**/references/*.md`）必须使用 UTF-8（建议带 BOM），禁止以“ANSI/GBK”写入。
- 编码写入硬规则（PowerShell）：读文件必须显式指定 `-Encoding UTF8`（如 `Get-Content -Raw -Encoding UTF8`），写文件必须显式指定 `-Encoding UTF8`（如 `Set-Content -Encoding UTF8`），禁止使用不带编码的 `Out-File`/`Set-Content` 生成任务卡与归档。
- 编码自检强制：Dispatch/Execution/Steward 在落盘任务卡、归档快照、索引后必须自检是否出现典型 mojibake 片段（例如 `浠诲姟`、`锛`、`鏂囨`、`瀵归綈`）；若命中，必须立即修复并用权威模板重写对应文件，禁止将“乱码文件”标记为落盘成功。
- Execution 关单强制：Execution 仅可完成“归档三件套准备”（任务卡 Archive、归档快照、归档索引），不得自行将任务置为 `Done`。
- Execution 状态落盘强制：Execution 每次推进阶段时，必须先更新任务卡 `Status/Completion` 再汇报结果；未落盘不得宣称任务已进入 `InProgress`、`Verify` 或 `Review`。
- 终态关单强制：`Review -> Done` 仅允许 Human 在复验通过后显式触发；Workflow Steward Agent 仅可在 Human 明确输入 `关单 <TaskId>` 后代执行机械同步，不拥有独立签收权；Execution 不得自签 `Done`，QA 也不得代签。
- QA Agent 职责边界：仅可执行 `TASK-QA-*` 与 Human 质疑复核（复现/证据比对/QA 报告），不得实现功能任务（如 `TASK-REND-*`、`TASK-APP-*`、`TASK-PLAT-*`），不得执行关单、状态流转、归档写入或看板更新。
- QA Agent 越权回退：收到实现类任务、关单请求、状态流转请求或代码重构请求时，必须拒绝并回退 `请按 Execution Agent 职责重试`；收到关单/元数据治理请求时，必须回退 `请按 Workflow Steward Agent 职责重试`。
- Human 最终关单强制：`Review -> Done` 仅允许 Human 在复验通过后显式触发；Workflow Steward Agent 仅可在 Human 明确 `关单 <TaskId>` 后代执行机械同步（含看板 Done 更新与 `HumanSignoff=pass`），但 Human 始终是唯一签收主体；Execution 不能代执行关单，QA 不能代执行关单。
- 路径解析强制：执行阶段遇到相对路径时，必须按“三步解析”自动查找后才能报错：
  1) 先按仓库根目录解析；
  2) 再按相关 skill 目录解析（`.codex/skills/<skill>/...`）；
  3) 再在 `.codex/skills/**/references/` 中按文件名兜底搜索。
- 仅当三步都失败时，才允许返回“路径未找到”。
- 路径分类硬规则：`references/*`（含 `review-checklist.md`）属于 skill 规则源，默认从 `.codex/skills/**/references/` 读取，且按只读处理；任务执行产物（任务卡状态、看板、归档快照、归档索引）仅允许写入 `.ai-workflow/**`。
- 任务卡字段必须与 `task-card-template.md` 一致。
- 轻量卡字段必须与 `quick-card-template.md` 一致。
- 正式任务卡若 `ExecutionReady!=true`，视为脏卡，不得派发。
- 状态流转仅允许：`Todo -> InProgress -> Verify -> Review -> Done`。
- 轻量卡状态流转仅允许：`Todo -> InProgress -> Review -> Done | Escalated | Rejected`，且 `Escalated` 后不得再直接关闭，必须转正式任务卡。
- 任一门禁失败，任务必须回退到 `InProgress` 并记录原因。
- 超过 WIP 限制时拒绝派发。
- 缺少验收标准的正式任务卡一律拒绝；轻量卡至少需要最小验证口径。
- 缺少 `Priority (P0|P1|P2|P3)` 的请求一律拒绝。
- 缺少 `PrimaryModule` 的请求一律拒绝。
- 缺少 `BoundaryContractPath` 的正式任务卡一律拒绝。
- 缺少 `AllowedPaths` 的正式任务卡一律拒绝。
- 缺少 `BaselineRef` 的正式任务卡一律拒绝。
- 正式任务卡若超过 1-3 小时不可验证完成必须拆分；轻量卡若预计超过半天必须升卡。
- 未按 `archive-policy.md` 完成归档，不允许 `Review -> Done`。
- 每张任务卡必须且只能归属一个主模块。
- 每张轻量卡必须且只能归属一个主模块。
- 每张正式任务卡必须包含对本卡足够的局部上下文，不得把关键实现细节留在里程碑文件中由执行者自行补完。
- 每张正式任务卡的信息量必须与复杂度等级匹配；复杂任务写成简短卡片，视为脏卡。
- 新建文件必须归属到主模块的允许路径内。
- 与 `project-baseline.md` 冲突的实施任务，必须先走“基线变更任务卡”。
- `QuickCard` 不得承载边界变更、基线变更、公开 API 设计、跨模块重构、正式 QA 验证卡。
- 依赖边界硬约束：任务卡 `DependencyContract.AllowedDependsOn` 必须是对应 `BoundaryContractPath` 允许依赖的子集；若超出边界合同，必须先提出“边界变更请求”并等待 Human 明确批准，未批准不得创建/执行该任务卡。

## 派发算法

严格按以下顺序执行：

1. 校验任务定义完整性。
2. 先判定是否满足 `QuickCard` 条件；不满足再进入正式任务卡拆分。
3. 对正式任务卡执行“里程碑信息下沉”，补齐执行上下文、决策继承、设计约束、失败语义与参考点。
4. 对正式任务卡执行复杂度分级，并确认信息量与复杂度匹配。
5. 校验主模块归属、边界合同、路径白名单与基线引用。
6. 检查当前看板 WIP 与瓶颈。
7. 按模块归属与负载选择负责人。
8. 输出带依据的派发决策。
9. 输出下一检查点与期望证据。

不得跳步。

## 门禁证据规则

`Verify -> Review` 前必须具备：

- Build 结果
- Test 结果
- Smoke 结果
- Perf 说明（基线对比或无明显退化说明）
- 变更文件路径检查结果（全部命中 `AllowedPaths`）
- 当任务为 QA 验证卡（`TaskId` 含 `TASK-QA-` 或 `ExecutionAgent=Exec-QA`）时，必须额外具备：
  - `CodeQuality` 结论（至少包含：`NoNewHighRisk=true`、`MustFixCount=0` 或转卡说明）
  - `DesignQuality` 结论（至少包含：`DQ-1 职责单一`、`DQ-2 依赖反转`、`DQ-3 扩展点保留`）
  - 若 `MustFixCount>0`，不得进入 `Review`，必须转新卡并记录 `Owner/计划引用/里程碑引用`

`Review -> Done` 前必须具备：

- 边界检查状态
- Must-fix 已修复或被明确接受
- 剩余风险已记录
- 归档条目已写入（含优先级、改动摘要、验证证据）
- 模块归属校验结果（`ModuleAttributionCheck=pass`）
- `HumanSignoff=pass`（由 Human 复验确认）

## 风险等级

仅使用以下标签：

- `high`：高概率行为错误，阻塞合并
- `medium`：质量/性能风险，建议合并前修复
- `low`：轻微问题，可延期处理

## 范围控制规则

- 每张任务卡只允许单一结果、单一负责人。
- 同一卡禁止顺手重构。
- 公开 API 变更必须单独建卡。
- 跨模块任务必须显式标注且已设定优先级。

## 输出格式要求

每次派发响应必须包含：

- `Decision`
- `Owner`：执行人
- `Priority`：P0/P1/P2/P3
- `PrimaryModule`：唯一主模块归属
- `WhyNow`：当前优先处理理由
- `Risk`：high/medium/low + 一句话理由
- `NextAction`：下一步精确动作

若输出正式任务卡，必须额外包含：

- `GatePlan`：门禁证据检查点
- `BoundaryContractPath`：对应边界合同路径
- `BaselineRef`：工程基线引用路径
- `ArchiveAction`：归档路径 + 写入/更新动作

若输出 `QuickCard`，必须额外包含：

- `Decision`：`quickcard`
- `Type`：`QuickTask | QuickBug | BugInvestigation`
- `QuickCardId`
- `WhyQuick`
- `EscalationGuard`
- `QuickCardPath`

尽量使用精炼要点，避免纯叙述长文。

## Plan 归档索引硬规则（新增）

- Plan Agent 在写入计划归档文件后，必须同步更新 `.ai-workflow/plan-archive/plan-archive-index.md`。
- `plan-archive-index.md` 最少字段：`PlanId`、`Status`、`LastUpdated`、`MilestoneSummary`、`SnapshotPath`。
- 若归档快照与索引任一未落盘，Plan Agent 不得宣称“归档完成”。
- Plan Agent 对 Human 的归档完成回执必须包含：`PlanArchivePath`、`IndexPath`、`LastWriteTime`、`文件前5行`（快照与索引各一份）。

## 失败回退协议（统一）

- 任何拒绝、阻塞、修卡请求、未关单、越权回退，都必须使用统一失败回执格式。
- 统一失败回执字段（全部必填）：
  - `FailureType`：`MissingField|PathConflict|DependencyBlocked|ScopeViolation|GateFailed|OwnershipMismatch|ArchiveIncomplete|PathNotFound|AcceptanceDispute|PostAcceptanceBug|TaskCardInsufficient|Other`
  - `BlockedBy`：触发失败的规则条目（引用规则名或文件+行号）
  - `RequiredFix`：最小修复动作（1-3 条）
  - `Owner`：`PlanAgent|DispatchAgent|ExecutionAgent|Human`
  - `RetryCommand`：给 Human 的下一句可执行指令（单行）
  - `Evidence`：最小证据（缺失字段名、冲突路径、失败门禁项等）
- 禁止仅返回“不能执行/请修卡/路径有问题”这类无结构文案。

## 缺陷分流规则（即时失败 vs 延迟缺陷）

- 规则 A（即时验收失败）：
  - 条件：`FailureType=AcceptanceDispute` 且发生在验收窗口内（从 `Verify/Review` 到 Human 首次签收结论前）。
  - 处理：必须 `ReopenOriginal`（回退原任务），不得新建 Bug 卡。
  - 要求：任务卡补齐 `ReopenReason`、`DetectedAt`、`HumanSignoff=fail`。
  - 状态联动：原任务卡应为 `InProgress`（或按回退规则状态），归档索引应标记 `Cancelled`，归档快照不得保留 `Done/100` 语义。
  - 证据联动：当 `FailureType=AcceptanceDispute` 且触发 `ReopenOriginal` 时，`ValidationEvidence.Smoke` 不得为 `pass`。
- 规则 B（延迟发现缺陷）：
  - 条件：任务已 `Done` 且在后续使用中发现问题（`FailureType=PostAcceptanceBug`）。
  - 处理：默认 `CreateBugCard`（或 Follow-up），原任务保持 `Done`，不得回退原任务。
  - 要求：新卡必须记录 `OriginTaskId`、`DetectedAt`，并按常规门禁流转。
- `OriginTaskId` 字段约束：
  - 仅 `CreateBugCard/Follow-up` 场景强制必填，且必须指向原始任务卡。
  - `ReopenOriginal` 场景不得自指（例如 `OriginTaskId=当前TaskId`）；默认留空。
- 证据不足规则：
  - 条件：缺少可复现实证据或期望/实际不明确。
  - 处理：`CreateVerifyCard`，先补复现与证据，不直接回退或派修复。
- 缺省判定规则（防误分流）：
  - 若 `FailureType` 未显式提供，默认按 `AcceptanceDispute` 处理（`ReopenOriginal`），禁止默认归类为 `PostAcceptanceBug`。
  - 仅当 Human 明确声明“已通过验收后/数日后/线上回归”或显式给出 `FailureType=PostAcceptanceBug` 时，才允许走 `CreateBugCard`。

## 边界变更请求（新增）

- 触发条件：任务卡需要新增/放宽模块依赖，且与 `BoundaryContractPath` 当前合同不一致。
- 强制流程：
  1) Dispatch/Execution 必须中止当前任务流转；
  2) 先向 Human 提交“边界变更请求”（包含：变更项、理由、影响模块、回滚策略）；
  3) 仅在 Human 明确批准后，才允许更新边界合同并重建/修订任务卡。
- 未经批准直接把越界依赖写入任务卡，视为 `FailureType=ScopeViolation`。

## 三级检测点（强制）

- 检测点 A（Dispatch 落卡后，必检）：
  - 目标：阻断“脏卡”进入执行流。
  - 检查项：字段完整性、依赖合法性、`Status/Completion` 一致性、路径规则（`AllowedPaths` 与 `BoundarySyncPlan`）。
- 检测点 B（Execution 关单前，必检）：
  - 目标：阻断“口头完成、归档不全”。
  - 检查项：归档三件套完整、`AllowedPaths` 命中、`BoundarySyncPlan` 条件满足、验证证据字段齐全、`HumanSignoff=pending`。
- 检测点 C（每个里程碑完成时，Plan 必检）：
  - 目标：保证计划层与任务层同步。
  - 检查项：里程碑状态与任务完成状态一致、`PlanArchive` 快照已更新、`plan-archive-index.md` 已同步。

## Workflow Steward Agent（新增）

- 模式定义：
  - `Audit`（默认）：只读检查，不改文件。
  - `Apply`（显式命令）：只执行 Human 批准的修复项。
- 审计范围：
  - 任务卡字段合法性（含 `Status/Completion/HumanSignoff/FailureType/DetectedAt`）。
  - 状态流转合法性（仅允许 `Todo -> InProgress -> Verify -> Review -> Done`）。
  - 归档一致性（任务卡/快照/索引/看板）。
  - 计划一致性（`plan-archive` 快照与 `plan-archive-index` 命中）。
  - 里程碑一致性（里程碑状态与所属任务状态对齐）。
- 命令约定：
  - `审计任务状态`
  - `修复建议 <IssueId...>`
  - `执行修复 <IssueId...>`
  - `关单 <TaskId>`
- 关单责任：
  - Workflow Steward Agent 仅可在 Human 显式发出 `关单 <TaskId>` 命令后执行机械同步；Human 始终是唯一签收主体。
