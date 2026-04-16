# 任务: TASK-APP-006 M5 App 装配兼容校准

## TaskId
`TASK-APP-006`

## 目标（Goal）
在 `Engine.App` 保持“仅装配不计算”原则下，完成 M5 变换链路（Position、Scale、Rotation）的组合根兼容校准与最小生命周期验证。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M5-2026-04-14`

## 里程碑引用（兼容别名：MilestoneRef）
`M5`

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
- ParallelGroup: `M5-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-005`
  - `TASK-REND-008`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - RuntimeBootstrap/ApplicationHost 装配与兼容测试
  - App 对应测试
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现 transform 数学计算
- 不新增 Render 绘制特性
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
  - `Engine.App` 承载渲染变换计算逻辑

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
- Test: `dotnet test` 通过；App 装配链路测试通过
- Smoke: 启动、可见、可退出，行为与 M5 目标一致（含 Rotation 链路）
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

## Archive
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-APP-006.md`
- ClosedAt: `2026-04-15 15:23`
- Summary:
  - 保持 `Engine.App` “仅装配不计算”边界，继续通过 `Scene -> Contracts -> Render` 链路传递 M5 transform 数据。
  - 补充 App 装配测试校准：验证注入 provider 在初始化后可输出含 rotation 的连续帧 transform。
  - 复核主循环最小生命周期（初始化、帧驱动、收口）在 M5 链路下仍稳定。
- FilesChanged:
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/tasks/task-app-006.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-APP-006.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1`）
  - Smoke: pass（`dotnet run --no-build` + `ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.80s`）
  - Perf: pass（`dotnet run --no-build` + `ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.80s`）
- ModuleAttributionCheck: pass
