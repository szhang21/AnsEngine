# 任务卡目录说明

本目录用于存放任务卡文件，建议每张任务卡一个独立文件：

- 文件命名建议：`<task-id>.md`
- 示例：`task-boot-001.md`

## 任务卡要求

- 必填字段：`Priority`、`PrimaryModule`、`BoundaryContractPath`、`AllowedPaths`、`Acceptance`
- 并行字段必填：`ParallelGroup`、`CanRunParallel`、`DependsOn`
- 计划字段必填：`计划引用`、`里程碑引用`（兼容别名：`PlanRef`、`MilestoneRef`）
- 边界同步字段必填：`BoundarySyncPlan`
- 状态流转：`Todo -> InProgress -> Verify -> Review -> Done`
- 关闭任务前必须在归档中记录：
  - 主模块归属
  - 边界合同路径
  - 模块归属校验结果

## 三 Agent 分工

- `Plan Agent`：只负责制定/调整计划与里程碑，不执行代码。
- `Dispatch Agent`：只生成任务卡（可多张）与并行/依赖标注，不执行代码。
- `Execution Agent`：只执行任务卡，不负责拆分需求。
- `Human`：与 Plan Agent 协商方向，审核任务卡并决定执行顺序、并发批次与派工。

## 默认展示模式（Manager View）

- `Dispatch Agent` 默认只对 Human 展示：简述、任务编号、依赖图、波次计划、任务卡路径。
- 完整任务卡正文默认不回显，统一落盘到：`.ai-workflow/tasks/<task-id>.md`。
- `Human` 给 `Execution Agent` 时可只发送 `TaskId`。

## 路径解析硬规则

- 执行阶段遇到相对路径时，Execution Agent 必须先自动执行三步解析：
  - 仓库根目录解析
  - skill 目录解析（`.codex/skills/<skill>/...`）
  - `.codex/skills/**/references/` 文件名兜底搜索
- 仅当三步都失败时，才允许返回“路径未找到”。

## 三条硬约束（不可跳过）

- 模板先行：每次交互必须先贴对应角色固定提示词模板；未贴模板视为无效请求。
- 字段齐全：任务卡缺少 `计划引用`、`里程碑引用`、`ParallelGroup`、`CanRunParallel`、`DependsOn` 任一字段，禁止进入执行流。
- 越权回退：Agent 收到非本角色职责请求时，只允许返回 `请按 <AgentName> 职责重试`，不得部分执行。

## 新增文件边界同步规则（硬约束）

- 只要任务新增文件，必须同步更新至少一个边界文档（`.ai-workflow/boundaries/*.md`）。
- 边界文档更新必须包含 `Boundary Change Log` 记录。
- 未满足该规则的任务不得从 `Review` 进入 `Done`。
- `AllowedPaths` 仅用于源码/测试改动范围；边界文档是否允许改动由 `BoundaryDocsToUpdate` 独立控制。
- 当 `NewFilesExpected=false` 且执行中无新增源码/测试文件时，边界文档更新不作为执行阻塞项。
