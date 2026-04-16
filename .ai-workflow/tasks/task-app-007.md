# 任务: TASK-APP-007 M6 App 装配与生命周期校准

## TaskId
`TASK-APP-007`

## 目标（Goal）
在 `Engine.App` 保持“只装配不计算”职责前提下，完成 M6 MVP 渲染链路装配校准并验证生命周期稳定性。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M6-2026-04-17`

## 里程碑引用（兼容别名：MilestoneRef）
`M6`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Render
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M6-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-006`
  - `TASK-REND-010`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - RuntimeBootstrap/ApplicationHost 组合根与装配相关文件
  - 对应测试文件
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在 App 中实现矩阵计算
- 不在 App 中实现渲染提交细节
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Platform/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.App` 内承载 MVP 渲染计算实现

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；App 装配与生命周期测试通过
- Smoke: 启动、可见、可退出路径保持稳定
- Perf: 主循环无明显退化

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Todo

## 完成度（Completion）
`0`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`
