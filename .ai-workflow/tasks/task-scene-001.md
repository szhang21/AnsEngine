# 任务: TASK-SCENE-001 M4 Scene-Render 最小契约定义

## 目标（Goal）
定义并落地 `Engine.Scene -> Engine.Render` 的最小渲染提交契约，作为 M4 后续实现卡的唯一依赖基线。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M4-2026-04-11`

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
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G1`
- CanRunParallel: `false`
- DependsOn:
  - `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - 场景到渲染最小提交契约相关文件
  - 相关最小测试文件
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现完整材质系统与资源导入管线
- 不在 `Engine.App` 承载渲染细节
- 不顺手改造非 M4 必需接口
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Platform/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Render` 内部实现
  - `Engine.Scene -> Engine.Asset` 导入器实现

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过，契约相关最小测试通过
- Smoke: 契约接入后应用可启动并进入循环，无崩溃
- Perf: 契约层引入后无明显帧时间抖动

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

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SCENE-001.md`
- ClosedAt: `2026-04-11 11:25`
- Summary:
  - 在 `Engine.Scene` 定义最小 Scene-Render 提交契约：`SceneRenderItem`、`SceneRenderFrame`、`ISceneRenderContractProvider`
  - `SceneGraphService` 实现契约并输出只读渲染快照，维持模块边界不引入 OpenGL/Render 依赖
  - 增补 `Engine.Scene.Tests` 契约测试（空场景、单节点提交）并通过
  - 同步更新 `Engine.Scene` 边界合同变更记录
- FilesChanged:
  - `src/Engine.Scene/SceneRenderContracts.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-001.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-SCENE-001.md`
- ValidationEvidence:
  - Build(Debug): fail -> pass（首轮 `CS2012` 文件占用；复跑 `dotnet build -c Debug -m:1` 通过）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1`；`Engine.Scene.Tests` 3/3 通过）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，约 `30.22s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，`ExitCode=0`，约 `45.17s`，无明显退化）
- ModuleAttributionCheck: pass
