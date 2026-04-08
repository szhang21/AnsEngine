# 看板流转规则（三 Agent 模式）

## 看板列定义

- `Todo`：已定义、可开始、未执行
- `InProgress`：正在实施
- `Verify`：等待构建/测试/冒烟/性能检查
- `Review`：等待边界/代码质量评审
- `Done`：已通过全部门禁

职责约束：

- `Plan Agent` 负责计划制定与阶段更新（不入看板执行列）。
- `Dispatch Agent` 负责把任务放入 `Todo`（含并行与依赖字段）。
- `Execution Agent` 负责 `InProgress -> Verify -> Review -> Done`。
- `Human` 负责审批计划与任务卡，决定哪些 `Todo` 进入当前执行波次。

## 允许的状态流转

- `Todo -> InProgress`
- `InProgress -> Verify`
- `Verify -> Review`
- `Review -> Done`
- `Verify -> InProgress`（门禁失败）
- `Review -> InProgress`（发现 must-fix）

除上述路径外，禁止其他流转。

## WIP 限制

- Global `InProgress <= 3`
- Per module `InProgress <= 1`
- Cross-module tasks active at the same time `<= 1`
- Same `ParallelGroup` 同时激活数量由 Human 决定（建议不超过 2）

超过限制时，必须延后派发或拆分任务。

## 并行与依赖门禁

- 任务卡必须包含 `ParallelGroup`、`CanRunParallel`、`DependsOn`。
- 任务卡必须包含 `计划引用`、`里程碑引用`（兼容旧字段名：`PlanRef`、`MilestoneRef`）。
- 任务卡必须声明边界同步计划（`BoundarySyncPlan`）。
- `DependsOn` 未完成时，禁止进入 `InProgress`。
- `CanRunParallel=false` 的任务不得与同组任务并发执行。
- 若执行中发现依赖图错误，任务退回 `Todo`，由 Dispatch Agent 修卡。

## 门禁要求

### InProgress -> Verify

- 已完成定义范围内实现
- 已附自检记录
- 未越界 `AllowedPaths`

### Verify -> Review

- Build 证据
- Test 证据
- Smoke 证据
- Perf 说明

### Review -> Done

- 边界检查通过
- Must-fix 已修复或明确接受
- 风险摘要已更新
- 归档已写入（含模块归属校验结论）
- 若任务新增文件：必须已更新边界文档并写入变更日志
