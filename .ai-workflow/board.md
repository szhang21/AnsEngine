# AI 工作流看板

> 用于管理任务状态流转：`Todo -> InProgress -> Verify -> Review -> Done`

## 使用规则（摘要）

- 每张任务卡必须包含：`PrimaryModule`、`BoundaryContractPath`、`AllowedPaths`。
- 每张任务卡必须包含并行信息：`ParallelGroup`、`CanRunParallel`、`DependsOn`。
- 每张任务卡必须包含计划引用：`计划引用`、`里程碑引用`（兼容别名：`PlanRef`、`MilestoneRef`）。
- 全局 `InProgress <= 3`，同模块 `InProgress <= 1`，跨模块任务同时 `<= 1`。
- 未通过门禁不得流转；`Review -> Done` 前必须完成归档写入。
- `Plan Agent` 负责计划，`Dispatch Agent` 负责创建 `Todo` 任务卡，`Execution Agent` 仅执行任务卡。
- 模板先行：每次交互先贴对应角色模板，未贴模板不执行。
- 字段齐全：缺少 `计划引用/里程碑引用/ParallelGroup/CanRunParallel/DependsOn` 任一字段的任务卡不得进入执行流。
- 越权回退：角色不匹配时只返回 `请按 <AgentName> 职责重试`。
- 新增文件边界同步：只要任务新增文件，必须同步更新至少一个边界文档并写入变更记录，否则不得 `Review -> Done`。

## Todo

- （空）

## InProgress

- （空）

## Verify

- （空）

## Review

- （空）

## Done

- `TASK-BOOT-001` 初始化 `AnsEngine` 多项目骨架（Build/Test 门禁通过，已归档）
- `TASK-PLAT-001` 真实窗口生命周期落地（Build/Test/Smoke 通过，已归档）
- `TASK-APP-001` 最小主循环与退出编排（Build/Test/Smoke/Perf 通过，已归档）
- `TASK-REND-001` 最小清屏可视反馈（Build/Test/Smoke/Perf 通过，已归档）
- `TASK-QA-001` 可见反馈门禁证据补齐（Build/Test/Smoke/Perf 证据已归档）
- `TASK-REND-002` 首帧三角形最小渲染链路（人工复验通过，已归档）
- `TASK-APP-002` M3 运行装配与生命周期配套（人工验收通过，已归档）
- `TASK-QA-002` M3 双轨门禁证据与归档收口（人工验收通过，已归档）

## Blocked

- （空）
