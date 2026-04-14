# 任务: TASK-REND-007 M4b Render 默认回退 Provider 清理

## TaskId
`TASK-REND-007`

## 目标（Goal）
移除 `Engine.Render` 在生产路径中的默认回退 provider（`DefaultSceneRenderContractProvider`）以暴露装配缺失问题，避免掩盖组合根漏注入。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M4B-2026-04-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M4-SceneRenderPipeline`

## 执行代理（ExecutionAgent）
Exec-Render

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Render

## 次级模块（SecondaryModules）
- Engine.App
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G6`
- CanRunParallel: `false`
- DependsOn: `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - 渲染器构造与 provider 注入相关文件
  - 渲染模块对应测试文件
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在本卡扩展 Scene 语义
- 不在本卡调整 App 主循环
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Platform/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - 生产路径允许“静默默认 provider”兜底
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
- Test: `dotnet test` 通过；漏注入场景必须显式失败（或明确测试工厂隔离）
- Smoke: 正常装配路径下渲染可启动、可见、可退出
- Perf: 与当前基线无明显退化

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
- FailureType: `PostAcceptanceBug`
- DetectedAt: `2026-04-14`
- ReopenReason:
- OriginTaskId: `TASK-REND-006`
- HumanSignoff: `pending`
