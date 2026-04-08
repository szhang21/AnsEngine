# 任务: TASK-APP-001 最小主循环与退出编排

## 目标（Goal）
让 `Engine.App` 从单次 `Run()` 升级为最小持续主循环，完成启动、每帧 tick、关闭退出与资源收口，并消费真实窗口生命周期信号。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-VISIBLE-001`

## 里程碑引用（兼容别名：MilestoneRef）
`M1-WindowLoopVisible`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P0
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Platform
- Engine.Render
- Engine.Scene
- Engine.Asset

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-PLAT-001`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - 应用启动装配文件
  - 应用运行循环编排文件
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不在 `Engine.App` 中写 OpenGL 细节
- 不新增真实渲染实现
- 不深化 Scene/Asset 内部功能
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `tests/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Asset`
- ForbiddenDependsOn:
  - `Engine.App` 内直接实现平台底层适配
  - `Engine.App` 内直接实现渲染后端细节

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过；代码轨按 `AllowedPaths` 校验
- Test: `dotnet test` 继续通过；测试改动若存在仅允许命中 `tests/**`
- Smoke: 程序启动后窗口保持可见且不闪退，用户关闭窗口后正常退出并返回退出码 `0`；代码轨与边界文档轨双轨门禁均需可验证
- Perf: 连续运行 30 秒无明显主循环阻塞；边界文档轨单独按 `BoundaryDocsToUpdate` 校验并要求变更日志

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Done

## 完成度（Completion）
`100`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-APP-001.md`
- ClosedAt: `2026-04-07 11:10`
- Summary:
  - `Engine.App` 从单次执行升级为最小持续主循环，消费窗口关闭信号并退出。
  - 统一退出收口：`renderer.Shutdown()` 与 `windowService.Dispose()` 在 `finally` 中保障执行。
  - 增加 `ANS_ENGINE_AUTO_EXIT_SECONDS` 作为自动化 smoke/perf 验证辅助开关。
  - 同步更新 `Engine.App` 边界合同及 `Boundary Change Log`。
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-001.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-APP-001.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug`）
  - Build(Release): pass（`dotnet build -c Release`）
  - Test: pass（`dotnet test`）
  - Smoke: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=2` 下 `dotnet run` 退出码 `0`）
  - Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=30` 下连续运行约 36s，退出码 `0`）
- ModuleAttributionCheck: pass
