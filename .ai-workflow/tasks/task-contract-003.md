# 任务: TASK-CONTRACT-003 M6 相机与 MVP 最小契约扩展

## TaskId
`TASK-CONTRACT-003`

## 目标（Goal）
在 `Engine.Contracts` 中补齐最小相机/视图/投影契约，使渲染输入可表达对象变换与视图语义，并保持默认兼容路径可运行。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M6-2026-04-17`

## 里程碑引用（兼容别名：MilestoneRef）
`M6`

## 执行代理（ExecutionAgent）
Exec-Contracts

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Contracts

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Render

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-contracts.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M6-G1`
- CanRunParallel: `false`
- DependsOn: `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Contracts
- AllowedFiles:
  - Contracts 中相机/MVP 相关类型与字段
  - 对应契约兼容测试
- AllowedPaths:
  - `src/Engine.Contracts/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现 Scene 具体相机生产逻辑
- 不实现 Render shader 细节
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Contracts -> Engine.Core`（如已存在）
- ForbiddenDependsOn:
  - `Engine.Contracts -> Engine.Scene`
  - `Engine.Contracts -> Engine.Render`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；MVP/Camera 契约兼容测试通过
- Smoke: 默认兼容路径可启动、可见、可退出
- Perf: 契约扩展无明显退化

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
