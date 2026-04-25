# 任务归档策略

## 归档位置

默认使用以下路径：

- 任务看板文件：`.ai-workflow/board.md`
- 任务卡目录：`.ai-workflow/tasks/<task-id>.md`
- 模块边界合同目录：`.ai-workflow/boundaries/`
- 关闭归档索引：`.ai-workflow/archive/archive-index.md`
- 已关闭任务快照：`.ai-workflow/archive/<yyyy-mm>/<task-id>.md`

若项目已有工作流目录规范，优先遵循现有约定并保持一致。

## 归档必填字段

当任务从 `Review -> Done` 时，归档条目必须包含：

- 任务 id 与标题
- 优先级（`P0|P1|P2|P3`）
- 主模块归属（`PrimaryModule`）
- 边界合同路径（`BoundaryContractPath`）
- 负责人
- 完成时间
- 最终状态
- 改动摘要（短要点）
- 变更文件列表
- 验证证据摘要（build/test/smoke/perf）
- 模块归属校验结果（`ModuleAttributionCheck`）
- 未解决风险或后续任务

## 优先级定义

- `P0`：发布阻塞或严重回归，立即处理
- `P1`：高业务/学习价值，当前周期完成
- `P2`：常规计划项，排在 P0/P1 之后
- `P3`：低紧急度优化/重构，可延后

## 归档写入规则

- 未写归档不得标记 `Done`。
- 归档必须追加写入，不得静默重写历史结论。
- 若最终结果偏离原目标，需增加 `ScopeDeviation` 说明。
- 若任务取消，必须归档取消原因与替代任务 id。
- 若存在跨模块改动，必须写明 `CrossModule=true` 的审批依据与影响范围。

## 轻量卡保留规则

- `QuickCard` 默认不写入正式 `archive-index.md`。
- 已完成 `QuickCard` 应在原卡中补齐：
  - `CompletedAt`
  - `ChangeSummary`
  - `ValidationSummary`
- 若 `QuickCard` 升级为正式任务卡：
  - 原轻量卡保留入口记录
  - 必须写明 `EscalatedToTaskId`
  - 必须写明 `EscalationReason`
- 仅当 Human 明确要求把某类轻量卡纳入长期统计时，才允许另建轻量归档索引；默认不与正式归档混写。
