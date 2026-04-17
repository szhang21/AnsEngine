# 任务: TASK-REND-010 M6 Mesh 数据统一入口收敛（已并入 TASK-REND-009）

## TaskId
`TASK-REND-010`

> 本卡已并入 `TASK-REND-009`，仅保留历史收敛记录，不再单独执行。

## 目标（Goal）
保留最小 triangle demo mesh，但收敛为统一 mesh 数据入口参与提交，避免散落在渲染流程中的硬编码顶点路径。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M6-2026-04-17`

## 里程碑引用（兼容别名：MilestoneRef）
`M6`

## 执行代理（ExecutionAgent）
Exec-Render

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Render

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M6-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-REND-009`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - mesh 数据入口与提交管线收敛代码
  - 对应测试文件
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不引入完整资源导入管线
- 不扩展多材质/纹理功能
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Scene` 编译期依赖

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-render.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；mesh 入口收敛测试通过
- Smoke: 最小 demo mesh 通过统一入口可见且稳定
- Perf: 相比 M6 前置实现无明显退化

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Cancelled

## 完成度（Completion）
`0`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`

## Archive
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-REND-010.md`
- ClosedAt: `2026-04-17 00:47`
- Summary:
  - 已并入 `TASK-REND-009`
  - Render 仅保留单一主执行卡，mesh 数据统一入口收敛收进主卡范围
- FilesChanged:
  - `.ai-workflow/tasks/task-rend-010.md`
  - `.ai-workflow/tasks/task-rend-009.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M6-2026-04-17.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: N/A（卡片合并，无独立实现）
  - Test: N/A（卡片合并，无独立实现）
  - Smoke: N/A（卡片合并，无独立实现）
  - Perf: N/A（卡片合并，无独立实现）
- SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-010.md`
