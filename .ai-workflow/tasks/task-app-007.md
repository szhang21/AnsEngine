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
Done

## 完成度（Completion）
`100`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## ExecutionStatus
- Status: `Done`
- Completion: `100`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-APP-007.md`
- ClosedAt: `2026-04-17 13:20`
- Summary:
  - 保持 `Engine.App` “仅装配不计算”边界，M6 链路继续通过 `Scene -> Contracts -> Render` 传递，不在 App 承载 MVP 计算。
  - 扩展 App 装配测试：验证 native 渲染路径注入的 provider 可输出非 identity 相机语义，并在连续帧体现 `View` 变化。
  - 补充异常生命周期回归：渲染帧异常时仍可触发 `RequestClose`、执行 `Shutdown` 与窗口 `Dispose`。
- FilesChanged:
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/tasks/task-app-007.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-APP-007.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`）
  - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj -m:1`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.63s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
- ModuleAttributionCheck: pass
