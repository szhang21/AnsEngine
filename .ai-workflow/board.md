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

- `TASK-QA-009` M7 门禁复验与关单收敛（含多对象与回退验证）（人工验收通过，已归档）
- `TASK-REND-011` M7 Mesh 数据入口落地（人工验收通过，已归档）
- `TASK-REND-012` M7 Material 参数入口落地（人工验收通过，已归档）
- `TASK-CONTRACT-003` M6 相机与 MVP 最小契约扩展（人工验收通过，已归档）
- `TASK-CONTRACT-004` M7 资源输入契约收敛（人工验收通过，已归档）
- `TASK-SCENE-006` M6 Scene 对象与相机语义输出（人工验收通过，已归档）
- `TASK-REND-009` M6 Render MVP Uniform 渲染改造（合并 M6 Mesh 数据统一入口收敛）（人工验收通过，已归档）
- `TASK-APP-007` M6 App 装配与生命周期校准（人工验收通过，已归档）
- `TASK-QA-007` M6 MVP 渲染链路门禁与回归复验（人工验收通过，已归档）
- `TASK-SCENE-007` M7 Scene 资源引用输出对齐（人工验收通过，已归档）
- `TASK-BOOT-001` 初始化 `AnsEngine` 多项目骨架（Build/Test 门禁通过，已归档）
- `TASK-PLAT-001` 真实窗口生命周期落地（Build/Test/Smoke 通过，已归档）
- `TASK-APP-001` 最小主循环与退出编排（Build/Test/Smoke/Perf 通过，已归档）
- `TASK-REND-001` 最小清屏可视反馈（Build/Test/Smoke/Perf 通过，已归档）
- `TASK-QA-001` 可见反馈门禁证据补齐（Build/Test/Smoke/Perf 证据已归档）
- `TASK-REND-002` 首帧三角形最小渲染链路（人工复验通过，已归档）
- `TASK-APP-002` M3 运行装配与生命周期配套（人工验收通过，已归档）
- `TASK-QA-002` M3 双轨门禁证据与归档收口（人工验收通过，已归档）
- `TASK-SCENE-001` M4 Scene-Render 最小契约定义（人工验收通过，已归档）
- `TASK-SCENE-002` M4 最小场景渲染数据输出（人工验收通过，已归档）
- `TASK-REND-004` M4 场景驱动渲染消费（人工验收通过，已归档）
- `TASK-APP-003` M4 提交流程编排配套（人工验收通过，已归档）
- `TASK-QA-003` M4 验证与关单收敛（人工验收通过，已归档）
- `TASK-REND-005` M4 渲染边界文档对齐当前依赖（人工验收通过，已归档）
- `TASK-CONTRACT-001` M4 独立契约层建立与边界落盘（人工验收通过，已归档）
- `TASK-SCENE-003` M4 渲染输入契约下沉到独立层（人工验收通过，已归档）
- `TASK-REND-006` M4 Render 依赖反转与解耦（人工验收通过，已归档）
- `TASK-APP-004` M4 App 契约 Provider 装配（人工验收通过，已归档）
- `TASK-QA-004` M4 解耦门禁与质量复验（人工验收通过，已归档）
- `TASK-SCENE-004` M4b Scene 单契约出口收敛（人工验收通过，已归档）
- `TASK-REND-007` M4b Render 默认回退 Provider 清理（人工验收通过，已归档）
- `TASK-APP-005` M4b App 场景运行时抽象依赖修复（人工验收通过，已归档）
- `TASK-QA-005` M4b MustFix 关口复验与双轨门禁收口（人工验收通过，已归档）
- `TASK-CONTRACT-002` M5 渲染变换契约兼容扩展（人工验收通过，已归档）
- `TASK-APP-006` M5 App 装配兼容校准（人工验收通过，已归档）
- `TASK-SCENE-005` M5 Scene 变换渲染帧输出（人工验收通过，已归档）
- `TASK-REND-008` M5 Render 变换消费与提交应用（人工验收通过，已归档）
- `TASK-QA-006` M5 变换链路门禁与回归复验（含 Rotation）（人工验收通过，已归档）

## Blocked

- （空）



