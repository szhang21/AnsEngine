## 0) 路径分类硬规则（先读这个）

- `references/*`（包括 `review-checklist.md`）是 skill 规则源，读取位置为 `.codex/skills/**/references/`，只读。
- `.ai-workflow/**` 是项目流程产物目录，用于任务卡、看板、归档快照、归档索引的写入。
- 执行代理不得因为仓库根目录不存在 `references/review-checklist.md` 而阻塞；必须先按 skill 路径解析规则查找。

# 四角色端到端流程（One-Page）

本文档是 `Plan -> Dispatch -> Execution` 主链 + `Workflow Steward` 治理层的单页总览，定义每个节点该做什么、落什么盘、何时流转与归档。

## 1) 总体顺序

1. `Plan Agent` 产出计划并归档
2. `Dispatch Agent` 先校验 Plan 归档两件套，再基于计划拆卡并落盘
3. `Human` 仅按任务编号派发
4. `Execution Agent` 读取任务卡执行并回填状态
5. `Workflow Steward Agent` 默认审计，按 Human 显式命令执行元数据修复/关单
6. 通过门禁后由 Human（或 Human 显式授权 Steward）进入 `Done`

## 2) 节点职责

- `Plan Agent`
  - 负责：目标、里程碑、主优先级、风险、关键决策
  - 输出：`计划引用`、里程碑定义、`PlanArchive`
  - 落盘：`.ai-workflow/plan-archive/<yyyy-mm>/<计划引用>.md`

- `Dispatch Agent`
  - 负责：先做 Plan 前置门禁校验，再拆分任务、并行/依赖、任务级排序（仅同里程碑内）
  - 落盘：
    - 任务卡：`.ai-workflow/tasks/<task-id>.md`
    - 看板同步：`board.md` 的 `Todo`
  - 对 Human 默认输出：`Manager View`（简述+编号+波次+路径）
  - 禁止默认回显任务卡全文（除非 Human 明确要求“展开任务卡全文”）

- `Execution Agent`
  - 输入：`TaskId` 或任务卡全文（推荐仅 `TaskId`）
  - 读取：`.ai-workflow/tasks/<task-id>.md`
  - 负责：实现、验证、状态推进、关单资料
  - 禁止：重拆需求、越界改动、跨角色决策

- `Workflow Steward Agent`
  - 默认：`Audit` 只读检查（任务卡/里程碑卡/索引/看板一致性）
  - 显式命令后：`Apply` 仅做元数据修复（字段、状态、索引、看板）
  - 禁止：修改业务源码、改写验收标准、改写任务目标语义
  - 关单权限：仅当 Human 明确输入 `关单 <TaskId>` 且门禁满足时可代执行

- `Human`
  - 负责：确认计划、审核拆卡、按波次派工、最终接受结果
  - 可选：命令 Steward 执行 `审计任务状态` / `执行修复 <IssueId...>` / `关单 <TaskId>`

## 3) 落盘与可见性规则

- 先落盘，再回显。
- 任务卡已落盘后，默认只看 `Manager View`。
- 全文查看走文件路径：`.ai-workflow/tasks/<task-id>.md`。

## 4) 任务卡最小关键字段

- `计划引用`
- `里程碑引用`
- `Priority`
- `PrimaryModule`
- `ParallelPlan`（`ParallelGroup`、`CanRunParallel`、`DependsOn`）
- `BoundarySyncPlan`
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

## 7) 关单与归档事务

Execution 阶段（技术关单准备）：

1. 更新任务卡 `Archive` 字段（保持 `Status=Review`、`HumanSignoff=pending`）
2. 写归档快照：`.ai-workflow/archive/<yyyy-mm>/<task-id>.md`
3. 追加归档索引：`.ai-workflow/archive/archive-index.md`

Human/Steward 阶段（最终关单）：

4. Human 复验通过并设置 `HumanSignoff=pass`
5. Human 执行 `Review -> Done`（或 Human 显式命令 Steward 代执行）
6. 同步更新看板到 `Done`

任一失败则不得 `Done`，回退处理。

## 8) 建议的日常用法（最短路径）

1. 你与 `Plan Agent` 确认计划
2. 你把计划交给 `Dispatch Agent`
3. 你只看 `TaskIds + WavePlan`
4. 你按波次发 `TaskId` 给 `Execution Agent`
5. 完成后核对：Execution 三件套 + Human 最终 Done（含看板）

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
   - 字段完整性、依赖合法性、`Status/Completion` 一致性、路径规则。
   - 前置条件：Plan 归档两件套已存在（计划快照 + `plan-archive-index.md` 索引命中）。
2. Execution 关单前检测（关单完整性）：
   - 归档三件套、`AllowedPaths` 命中、`BoundarySyncPlan` 条件、证据字段齐全、`HumanSignoff=pending`。
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
3. Execution 完成后，可再次审计归档一致性（只读）。
4. 仅当 Human 明确输入 `关单 <TaskId>` 且门禁满足，Steward 才能代执行关单。
