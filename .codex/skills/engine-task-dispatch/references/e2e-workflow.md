## 0) 路径分类硬规则（先读这个）

- `references/*`（包括 `review-checklist.md`）是 skill 规则源，读取位置为 `.codex/skills/**/references/`，只读。
- `.ai-workflow/**` 是项目流程产物目录，用于任务卡、看板、归档快照、归档索引的写入。
- 执行代理不得因为仓库根目录不存在 `references/review-checklist.md` 而阻塞；必须先按 skill 路径解析规则查找。

# 五角色端到端流程（One-Page）

本文档是 `Plan -> Dispatch -> Execution` 主链 + `QA` 质量复核层 + `Workflow Steward` 治理层的单页总览，定义每个节点该做什么、落什么盘、何时流转与归档。

## 1) 总体顺序

1. `Plan Agent` 产出计划并归档
2. `Dispatch Agent` 先判断是否命中 `QuickCard` 适用范围；若不命中，再校验 Plan 归档两件套并拆正式任务卡
3. `Human` 按 `QuickCardId` 或 `TaskId` 派发
4. `Execution Agent` 读取轻量卡或任务卡执行并回填状态
5. `QA Agent` 执行 QA 验证卡并处理 Human 质疑（复现与证据核对），不执行关单
6. `Workflow Steward Agent` 默认审计，按 Human 显式命令执行元数据修复/关单
7. 通过门禁后由 Human 显式进入 `Done`；Workflow Steward 仅可在 Human 明确输入 `关单 <TaskId>` 后代执行机械同步，且 Human 始终是唯一签收主体；Execution 不得代签，QA 不得代签

## 2) 节点职责

- `Plan Agent`
  - 负责：目标、里程碑、主优先级、风险、关键决策
  - 输出：`计划引用`、里程碑定义、`PlanArchive`
  - 落盘：`.ai-workflow/plan-archive/<yyyy-mm>/<计划引用>.md`

- `Dispatch Agent`
  - 负责：先做 `QuickCard` 分流；若进入正式流，再做 Plan 前置门禁校验、任务拆分、并行/依赖、任务级排序（仅同里程碑内）
  - 额外责任：把里程碑中的关键背景、关键决策、关键约束和失败语义下沉进任务卡，使任务卡可脱离里程碑独立执行
  - 额外责任：对每张正式任务卡执行 `task-card-sufficiency-checklist.md`
  - 额外责任：若参考 `task-card-examples.md`，只能把它作为表达下限，不能作为信息上限
  - 额外责任：对每张正式任务卡执行复杂度分级，确保信息量随复杂度同步增长
  - 落盘：
    - 轻量卡：`.ai-workflow/quickcards/<quick-card-id>.md`
    - 任务卡：`.ai-workflow/tasks/<task-id>.md`
    - 看板同步：`board.md` 的 `Todo`
  - 对 Human 默认输出：`Manager View`（简述+编号+波次+路径）
  - 禁止默认回显任务卡全文（除非 Human 明确要求“展开任务卡全文”）

- `Execution Agent`
  - 输入：`QuickCardId`、`TaskId` 或全文（推荐仅给编号）
  - 读取：`.ai-workflow/quickcards/<quick-card-id>.md` 或 `.ai-workflow/tasks/<task-id>.md`
  - 负责：实现、验证、状态推进、关单资料
  - 禁止：重拆需求、越界改动、跨角色决策
  - 正式任务卡补充：任务卡是唯一执行依据；若必须回看里程碑全文才能实施，应回退并标记 `TaskCardInsufficient`
  - 状态推进补充：每推进一个阶段，都必须先更新任务卡 `Status/Completion` 再对 Human 回报结果
  - 轻量卡补充：若执行中触发升卡条件，必须停止继续扩张并回退给 Dispatch Agent
  - 默认编码约定：遵循 `engine-coding-standards`，C# 私有/受保护字段使用 `mCamelCase`，静态字段使用 `sCamelCase`，`const` 使用 `kCamelCase`；构造器参数与局部变量使用 `camelCase`
  - 文件组织约定：默认一个类一个文件、一个接口一个文件；只有小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期才允许例外
  - 关单边界：Execution 只能把卡推进到 `Review` 并准备归档三件套，不得自行把任务置为 `Done` 或更新看板到 `Done`

- `QA Agent`
  - 负责：执行 `TASK-QA-*`、输出 `QAReport`、处理 Human 质疑（复现步骤、Expected/Actual、证据补齐）
  - 禁止：实现功能卡、修改业务源码、代替 Dispatch 拆卡、代替 Steward 做元数据治理、执行关单或看板 Done 更新
  - 输入：`TASK-QA-*` 或 Human 质疑报告（建议含 `TaskId/Repro/Expected/Actual/Evidence`）

- `Workflow Steward Agent`
  - 默认：`Audit` 只读检查（任务卡/里程碑卡/索引/看板一致性）
  - 显式命令后：`Apply` 仅做元数据修复（字段、状态、索引、看板）
  - 禁止：修改业务源码、改写验收标准、改写任务目标语义
  - 禁止：执行任何 `TaskId` 实现类请求（如 `执行TASK-*`）；此类请求必须回退到 `Execution Agent`
  - 关单边界：仅在 Human 明确输入 `关单 <TaskId>` 且门禁满足时执行机械同步；Human 始终是唯一签收主体，QA 不得代为关单

- `Human`
  - 负责：确认计划、审核拆卡、按波次派工、最终接受结果
  - 可选：命令 QA 执行 `TASK-QA-*` 或质疑复核；命令 Steward 执行 `审计任务状态` / `执行修复 <IssueId...>` / `关单 <TaskId>`
  - 若发现代码风格偏离默认编码约定，可在派发前先让 Dispatch 修卡，不要在执行中临时改规则

## 3) 落盘与可见性规则

- 先落盘，再回显。
- `QuickCard` 与正式 `TaskCard` 均适用。
- 任务卡已落盘后，默认只看 `Manager View`。
- 轻量卡已落盘后，默认只看 `Quick View`。
- 全文查看走文件路径：`.ai-workflow/tasks/<task-id>.md`。

轻量卡全文查看路径：`.ai-workflow/quickcards/<quick-card-id>.md`。

## 4) 任务卡最小关键字段

- `计划引用`
- `里程碑引用`
- `MilestoneContext`
- `DecisionCarryOver`
- `ImplementationNotes`
- `DesignConstraints`
- `FallbackBehavior`
- `ExamplesOrReferences`
- `Priority`
- `PrimaryModule`
- `ParallelPlan`（`ParallelGroup`、`CanRunParallel`、`DependsOn`）
- `BoundarySyncPlan`
- `OpenQuestions`
- `ExecutionReadiness`
- `Status`
- `Completion`
- `Acceptance`（Build/Test/Smoke/Perf）

## 5) 双轨校验（关键）

- 代码轨：
  - `src/**`、`tests/**` 改动必须命中 `AllowedPaths`
- 边界文档轨：
  - 仅按 `BoundaryDocsToUpdate` 校验
  - 不受 `AllowedPaths` 限制
  - 仅当 `NewFilesExpected=true` 或执行中实际新增源码/测试文件时，边界文档更新才是硬门禁

## 6) 状态与完成度

- `Todo` -> `Completion=0`
- `InProgress|Verify|Review` -> `Completion=10-99`
- `Done` -> `Completion=100`
- `Execution Agent` 每次推进阶段时，必须同时落盘 `Status` 与 `Completion`
- 若只口头说明“开始了 / 验证完了 / 进评审了”但未更新任务卡，视为未推进

## 7) 关单与归档事务

Execution 阶段（技术关单准备）：

1. 更新任务卡 `Archive` 字段（保持 `Status=Review`、`HumanSignoff=pending`）
2. 写归档快照：`.ai-workflow/archive/<yyyy-mm>/<task-id>.md`
3. 追加归档索引：`.ai-workflow/archive/archive-index.md`

Human/Steward 阶段（最终关单）：

4. Human 复验通过并设置 `HumanSignoff=pass`
5. Human 显式执行 `Review -> Done`（Steward 仅在 Human 明确 `关单 <TaskId>` 时代执行机械同步）
6. 同步更新看板到 `Done`

任一失败则不得 `Done`，回退处理。

## 8) 建议的日常用法（最短路径）

1. 简单需求或小 bug：先让 `Dispatch Agent` 判定是否走 `QuickCard`
2. 若是轻量卡，你只看 `QuickCardId + EscalationGuard`
3. 若不是轻量卡，再回到 `Plan Agent -> Dispatch Agent -> TaskId` 主流程
4. 你按编号发给 `Execution Agent`
5. 轻量卡完成后核对最小验证证据；正式任务卡完成后核对三件套 + Human 最终 Done（含看板）

## 9) Plan 归档两件套（新增）

Plan 节点完成归档时，必须原子完成以下两件：
1. 计划快照：`.ai-workflow/plan-archive/<yyyy-mm>/<plan-id>.md`
2. 计划索引：`.ai-workflow/plan-archive/plan-archive-index.md`

任一缺失则视为“未完成归档”，不得口头报完成。

## 10) 失败回退回执（统一）

所有 Agent 在失败/阻塞时统一返回以下字段：
- `FailureType`
- `BlockedBy`
- `RequiredFix`
- `Owner`
- `RetryCommand`
- `Evidence`

执行顺序：
1. 先判断失败归因与 Owner
2. 再给最小修复动作（RequiredFix）
3. 最后给 Human 一条可直接执行的重试指令（RetryCommand）

## 11) 三次检测（必检）

1. Dispatch 落卡后立即检测（脏卡阻断）：
   - 字段完整性、依赖合法性、`Status/Completion` 一致性、路径规则、`ExecutionReady`、充分性清单结果、复杂度匹配结果。
   - 依赖合法性必须以 `BoundaryContractPath` 为准，发现越界依赖时先走“边界变更请求”，未批准不得落卡。
   - 前置条件：Plan 归档两件套已存在（计划快照 + `plan-archive-index.md` 索引命中）。
2. Execution 关单前检测（关单完整性）：
   - 归档三件套、`AllowedPaths` 命中、`BoundarySyncPlan` 条件、证据字段齐全、`HumanSignoff=pending`。
   - QA 验证卡额外检测：`CodeQuality` 与 `DesignQuality` 结论齐全，且 `MustFixCount=0` 或已转卡。
   - 若执行中曾出现“必须回看里程碑全文才能继续”，必须先回退修卡，不得硬做。
   - 若执行中阶段已变化但任务卡状态未同步落盘，必须先补状态再继续。
3. 每个里程碑完成时 Plan 检测（计划同步）：
   - 里程碑状态与任务状态一致、`PlanArchive` 快照已更新、`plan-archive-index.md` 已同步。

## 12) Human 发现缺陷后的顺序（新增）

1. Human 提交 `HumanRejectionReport` 给 Dispatch（含 `TaskId/FailureType/Repro/Expected/Actual/Evidence/DetectedAt`）。
2. Dispatch 先分流，再出卡/回退：
   - `AcceptanceDispute`（验收窗口内）=> `ReopenOriginal`
   - `PostAcceptanceBug`（Done 后发现）=> `CreateBugCard`
   - 证据不足 => `CreateVerifyCard`
   - 若 `FailureType` 未给出：默认走 `AcceptanceDispute`（防误分流）。
3. Human 只按 Dispatch 返回的 `TaskId` 派发给 Execution。
4. Execution 按分流结果执行；不接受口头直派修复。

## 13) Steward 介入时机（新增）

1. Dispatch 落卡后，先执行 `审计任务状态`（只读）。
2. 若发现元数据问题，先看 `FixPlan`，Human 选定 IssueId 后再下 `执行修复 <IssueId...>`。
3. 每次 Apply 修复后，Steward 必须立即再跑一次 `AuditReport`（复审），未通过不得报“修复完成”。
4. Execution 完成后，可再次审计归档一致性（只读）。
 5. 仅当 Human 明确输入 `关单 <TaskId>` 且门禁满足，Steward 才能执行机械同步；Human 始终是唯一签收主体。
