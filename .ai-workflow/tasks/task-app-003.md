# 任务: TASK-APP-003 M4 提交流程编排配套

## 目标（Goal�?保持 `Engine.App` 编排职责不变，完�?M4 场景更新/提交/渲染阶段衔接与生命周期收口配套�?
## 任务来源（TaskSource�?DispatchAgent

## 计划引用（兼容别名：PlanRef�?`PLAN-M4-2026-04-11`

## 里程碑引用（兼容别名：MilestoneRef�?`M4-SceneRenderPipeline`

## 执行代理（ExecutionAgent�?Exec-App

## 优先级（Priority�?P0
> 说明：优先级主定义来�?`计划引用`；Dispatch 仅允许同里程碑内微调�?
## 主模块归属（PrimaryModule�?Engine.App

## 次级模块（SecondaryModules�?- Engine.Scene
- Engine.Render

## 边界合同路径（BoundaryContractPath�?- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef�?- `references/project-baseline.md`

## 并行计划（ParallelPlan�?- ParallelGroup: `G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-002`
  - `TASK-REND-004`

## 范围（Scope�?- AllowedModules:
  - Engine.App
- AllowedFiles:
  - 应用编排与生命周期配套文�?  - 应用模块测试文件
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 跨模块标记（CrossModule�?false

## 非范围（OutOfScope�?- 不在 `Engine.App` 实现具体绘制逻辑
- 不扩展材�?资源系统
- 不并行推�?M5 范围
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`

## 依赖约束（DependencyContract�?- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Render`
- ForbiddenDependsOn:
  - `Engine.App` 内直接实现渲染后端细�?  - `Engine.App` 内直接实现场景内部逻辑

## 边界同步计划（BoundarySyncPlan�?- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `[]`
- ChangeLogRequired: `true`

## 验收标准（Acceptance�?- Build: `dotnet build -c Debug` �?`dotnet build -c Release` 通过
- Test: `dotnet test` 通过，编排链路相关测试通过
- Smoke: 应用可启动并持续渲染 30 秒以上，关闭后退出码 `0`
- Perf: 主循环调度无明显退�?
## 交付物（Deliverables�?- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status�?Review

## 完成度（Completion�?`95`

## 缺陷回流字段（Defect Triage�?- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive�?- ArchivePath: `.ai-workflow/archive/2026-04/TASK-APP-003.md`
- ClosedAt: `2026-04-11 11:55`
- Summary:
  - `Engine.App` 在组合根中将 `SceneGraphService` 作为 `ISceneRenderContractProvider` 注入 `NullRenderer`
  - 主循环维�?`ProcessEvents -> Input/Time -> Render -> Present` 编排职责，不引入渲染后端细节
  - M4 链路实现“场景输�?-> 渲染消费”的应用层衔�?  - 同步更新 `Engine.App` 边界合同变更记录
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-003.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-APP-003.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`，存在环境级 CS1668 警告�?  - Build(Release): pass（`dotnet build -c Release -m:1`，存在环境级 CS1668 警告�?  - Test: fail -> pass（首�?`dotnet test -m:1` �?`CS2012` 文件占用失败，清理进程后复跑通过�?  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，约 `30.15s`�?  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，`ExitCode=0`，约 `45.15s`�?- ModuleAttributionCheck: pass

