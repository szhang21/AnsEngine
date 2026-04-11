# 任务: TASK-APP-002 M3 运行装配与生命周期配套

## 目标（Goal）
在不改变编排职责边界的前提下，完成 `Engine.App` 对 M3 三角形渲染路径的运行装配、初始化顺序与退出收口配套。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M3-2026-04-08`

## 里程碑引用（兼容别名：MilestoneRef）
`M3-TriangleVisible`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P0
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Render
- Engine.Platform

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-REND-002`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - 启动装配与运行编排相关文件
  - 应用模块测试文件
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不在 `Engine.App` 写入 OpenGL 细节
- 不改造 `Engine.Render` 内部渲染实现
- 不引入非 M3 目标的系统性重构
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Render`
- ForbiddenDependsOn:
  - `Engine.App` 内直接实现渲染后端细节
  - `Engine.App` 内直接实现平台底层适配

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `[]`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过；代码轨按 `AllowedPaths` 校验
- Test: `dotnet test` 通过；测试改动仅允许命中 `tests/**`
- Smoke: M3 路径下初始化顺序稳定、退出收口稳定，关闭窗口退出码 `0`
- Perf: 不引入明显主循环调度退化；若执行中实际新增源码/测试文件则触发文档轨校验

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-APP-002.md`
- ClosedAt: `2026-04-11 10:45`
- HumanSignoff: `pass`
- Summary:
  - 将 `ApplicationHost.Run` 的渲染初始化纳入统一 `try/finally`，保证初始化异常时也执行窗口收口
  - 异常路径增加关闭意图发出，避免失败后窗口状态悬挂
  - `RuntimeBootstrap` 增加 `ANS_ENGINE_USE_NATIVE_WINDOW` 装配开关；默认仍启用真实窗口
  - 在无窗口模式下装配 `HeadlessRenderer`，用于无图形环境的稳定运行验证
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-002.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-APP-002.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`，存在环境级 MSB3101/CS1668 警告）
  - Build(Release): pass（`dotnet build -c Release -m:1`，存在环境级 MSB3101/CS1668 警告）
  - Test: pass（`dotnet test -m:1`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `dotnet .../Engine.App.dll`，`ExitCode=0`，约 `15.64s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `dotnet .../Engine.App.dll`，`ExitCode=0`，约 `45.14s`）
- ModuleAttributionCheck: pass
