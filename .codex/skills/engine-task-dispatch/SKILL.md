---
name: engine-task-dispatch
description: 使用严格任务卡模板、WIP 限制与门禁流转来派发和治理引擎研发任务。适用于创建任务、分配负责人、推进看板流转、处理阻塞、判断是否可合并等场景。触发词包括：任务派发、任务卡、看板流转、状态流转、WIP、门禁、阻塞处理、task dispatch、kanban、workflow gate。
---

# 引擎任务派发工作流

当需要创建、分配、流转或审计任务时，使用本 Skill。

输出前先读取以下参考文件：
- `references/task-card-template.md`
- `references/board-workflow.md`
- `references/dispatch-rules.md`
- `references/archive-policy.md`
- `references/boundary-contract-template.md`
- `references/project-baseline.md`
- `references/e2e-workflow.md`

## 必选输出类型

根据用户意图输出以下之一：

1. `TaskCard`（新任务卡）
2. `DispatchDecision`（派发/改派/延后决策）
3. `BoardTransition`（状态流转及门禁证据）
4. `DailyPlan`（日计划与 WIP 平衡）
5. `BlockerReport`（阻塞原因与解阻动作）
6. `AuditReport`（Steward 只读审计报告）
7. `FixPlan`（Steward 可执行修复清单）
8. `ApplyResult`（Steward 已执行修复结果）

如信息不足，必须显式写出假设。

## 强约束

- 角色模板强制：每次响应开头必须先声明当前角色（`Plan Agent` / `Dispatch Agent` / `Execution Agent` / `Workflow Steward Agent`），并匹配对应固定提示词模板；不匹配则停止输出并要求重试。
- 字段完整性强制：任务卡若缺少 `计划引用`、`里程碑引用`、`ParallelPlan`（含 `ParallelGroup`、`CanRunParallel`、`DependsOn`）任一字段，必须拒绝流转或执行（兼容旧字段名：`PlanRef`、`MilestoneRef`）。
- 越权响应强制回退：若收到不属于当前角色职责的请求，只允许返回 `请按 <AgentName> 职责重试`，不得部分执行。
- 新增文件强制边界同步：仅当 `NewFilesExpected=true` 或执行中实际新增源码/测试文件时，才强制更新 `.ai-workflow/boundaries/` 下至少一个边界文档并写入变更记录；否则不作为阻塞条件。
- Plan 输出强制前推：Plan Agent 不得仅回复“目标不明确/请补充需求”；必须先基于当前仓库与看板状态给出候选计划和默认推荐。
- Plan 归档强制：Plan Agent 必须输出可落盘的计划归档块（含里程碑快照），归档路径默认 `.ai-workflow/plan-archive/<yyyy-mm>/<计划引用>.md`。
- Plan 归档触发：新计划生效、里程碑或优先级发生重大调整、计划关闭时，必须更新计划归档。
- Dispatch 前置门禁：Dispatch Agent 在拆卡前必须先校验 Plan 归档两件套已落盘（计划快照 + `plan-archive-index.md`）；未通过不得创建任务卡。
- Steward 审计默认只读：Workflow Steward Agent 默认仅允许审计与建议，不得直接修改任何文件。
- Steward 修改需显式命令：仅当 Human 明确输入 `执行修复 <IssueId...>` 或 `关单 <TaskId>` 时，Workflow Steward Agent 才允许执行元数据修改。
- Steward 修改边界：仅允许任务卡/里程碑卡/索引/看板元数据修复；禁止修改业务源码、验收标准、任务目标语义。
- Steward 关单门禁：收到 `关单 <TaskId>` 时，仅当 `Status=Review`、`HumanSignoff=pass`、归档三件套完整，才允许执行 `Review -> Done` 与看板 Done 更新。
- Dispatch 可见性强制：任务卡一旦成功写入 `.ai-workflow/tasks/`，默认仅输出 `Manager View`（任务编号与简述）；不得回显任务卡全文，除非 Human 明确输入“展开任务卡全文”。
- Execution 关单强制：Execution 仅可完成“归档三件套准备”（任务卡 Archive、归档快照、归档索引），不得自行将任务置为 `Done`。
- Human 最终关单强制：`Review -> Done` 仅允许 Human 在复验通过后执行（含看板 Done 更新与 `HumanSignoff=pass`）。
- 路径解析强制：执行阶段遇到相对路径时，必须按“三步解析”自动查找后才能报错：
  1) 先按仓库根目录解析；
  2) 再按相关 skill 目录解析（`.codex/skills/<skill>/...`）；
  3) 再在 `.codex/skills/**/references/` 中按文件名兜底搜索。
- 仅当三步都失败时，才允许返回“路径未找到”。
- 路径分类硬规则：`references/*`（含 `review-checklist.md`）属于 skill 规则源，默认从 `.codex/skills/**/references/` 读取，且按只读处理；任务执行产物（任务卡状态、看板、归档快照、归档索引）仅允许写入 `.ai-workflow/**`。
- 任务卡字段必须与 `task-card-template.md` 一致。
- 状态流转仅允许：`Todo -> InProgress -> Verify -> Review -> Done`。
- 任一门禁失败，任务必须回退到 `InProgress` 并记录原因。
- 超过 WIP 限制时拒绝派发。
- 缺少验收标准的任务一律拒绝。
- 缺少 `Priority (P0|P1|P2|P3)` 的任务一律拒绝。
- 缺少 `PrimaryModule` 的任务一律拒绝。
- 缺少 `BoundaryContractPath` 的任务一律拒绝。
- 缺少 `AllowedPaths` 的任务一律拒绝。
- 缺少 `BaselineRef` 的任务一律拒绝。
- 超过 1-3 小时不可验证完成的任务必须拆分。
- 未按 `archive-policy.md` 完成归档，不允许 `Review -> Done`。
- 每张任务卡必须且只能归属一个主模块。
- 新建文件必须归属到主模块的允许路径内。
- 与 `project-baseline.md` 冲突的实施任务，必须先走“基线变更任务卡”。

## 派发算法

严格按以下顺序执行：

1. 校验任务定义完整性。
2. 校验主模块归属、边界合同、路径白名单与基线引用。
3. 检查当前看板 WIP 与瓶颈。
4. 按模块归属与负载选择负责人。
5. 输出带依据的派发决策。
6. 输出下一检查点与期望证据。

不得跳步。

## 门禁证据规则

`Verify -> Review` 前必须具备：

- Build 结果
- Test 结果
- Smoke 结果
- Perf 说明（基线对比或无明显退化说明）
- 变更文件路径检查结果（全部命中 `AllowedPaths`）

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

- `Decision`：assign/defer/split/reject
- `Owner`：执行人
- `WhyNow`：当前优先处理理由
- `GatePlan`：门禁证据检查点
- `Priority`：P0/P1/P2/P3
- `PrimaryModule`：唯一主模块归属
- `BoundaryContractPath`：对应边界合同路径
- `BaselineRef`：工程基线引用路径
- `Risk`：high/medium/low + 一句话理由
- `ArchiveAction`：归档路径 + 写入/更新动作
- `NextAction`：下一步精确动作

尽量使用精炼要点，避免纯叙述长文。

## Plan 归档索引硬规则（新增）

- Plan Agent 在写入计划归档文件后，必须同步更新 `.ai-workflow/plan-archive/plan-archive-index.md`。
- `plan-archive-index.md` 最少字段：`PlanId`、`Status`、`LastUpdated`、`MilestoneSummary`、`SnapshotPath`。
- 若归档快照与索引任一未落盘，Plan Agent 不得宣称“归档完成”。
- Plan Agent 对 Human 的归档完成回执必须包含：`PlanArchivePath`、`IndexPath`、`LastWriteTime`、`文件前5行`（快照与索引各一份）。

## 失败回退协议（统一）

- 任何拒绝、阻塞、修卡请求、未关单、越权回退，都必须使用统一失败回执格式。
- 统一失败回执字段（全部必填）：
  - `FailureType`：`MissingField|PathConflict|DependencyBlocked|ScopeViolation|GateFailed|OwnershipMismatch|ArchiveIncomplete|PathNotFound|AcceptanceDispute|PostAcceptanceBug|Other`
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
- 规则 B（延迟发现缺陷）：
  - 条件：任务已 `Done` 且在后续使用中发现问题（`FailureType=PostAcceptanceBug`）。
  - 处理：默认 `CreateBugCard`（或 Follow-up），原任务保持 `Done`，不得回退原任务。
  - 要求：新卡必须记录 `OriginTaskId`、`DetectedAt`，并按常规门禁流转。
- 证据不足规则：
  - 条件：缺少可复现实证据或期望/实际不明确。
  - 处理：`CreateVerifyCard`，先补复现与证据，不直接回退或派修复。
- 缺省判定规则（防误分流）：
  - 若 `FailureType` 未显式提供，默认按 `AcceptanceDispute` 处理（`ReopenOriginal`），禁止默认归类为 `PostAcceptanceBug`。
  - 仅当 Human 明确声明“已通过验收后/数日后/线上回归”或显式给出 `FailureType=PostAcceptanceBug` 时，才允许走 `CreateBugCard`。

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
  - Workflow Steward Agent 可代执行 Human 关单动作，但前提是 Human 显式发出 `关单 <TaskId>` 命令。
