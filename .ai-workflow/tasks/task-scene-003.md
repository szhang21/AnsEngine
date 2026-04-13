# 任务: TASK-SCENE-003 M4 渲染输入契约下沉到独立层

## 目标（Goal）
消费已建立的独立契约层，在 `Engine.Scene` 侧完成最小场景渲染数据输出适配，建立 `Scene -> Contracts` 的稳定依赖。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M4B-2026-04-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M4-SceneRenderPipeline`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P0
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Render
- Engine.App

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-CONTRACT-001`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - 契约层定义与 Scene 侧输出适配文件
  - 相关项目引用与最小测试文件
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在本卡内移除 Render 对 Scene 的依赖
- 不扩展材质系统与资源导入体系
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Platform/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Contracts`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Render` 内部实现
  - `Engine.Scene -> Engine.Asset` 导入器实现

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过，契约层最小链路测试通过
- Smoke: 接入契约层后应用可启动且不崩溃
- Perf: 契约层引入后无明显性能退化

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
- FailureType: `PostAcceptanceBug`
- DetectedAt: `2026-04-11`
- ReopenReason:
- OriginTaskId: `TASK-REND-004`
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SCENE-003.md`
- ClosedAt: `2026-04-14 00:40`
- Summary:
  - `Engine.Scene` 新增对 `Engine.Contracts` 依赖，建立 `Scene -> Contracts` 稳定方向。
  - `SceneGraphService` 完成双接口适配：对外保留 `Engine.Scene` 兼容返回，同时实现 `Engine.Contracts` 契约接口。
  - 新增契约接口链路测试并同步更新 `engine-scene` 边界变更日志。
- FilesChanged:
  - `src/Engine.Scene/Engine.Scene.csproj`
  - `src/Engine.Scene/SceneRenderContracts.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-SCENE-003.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`24.74s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`62.53s`）
- ModuleAttributionCheck: pass
