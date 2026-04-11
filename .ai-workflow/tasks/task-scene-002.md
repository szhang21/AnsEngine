# 任务: TASK-SCENE-002 M4 最小场景渲染数据输出

## 目标（Goal）
在既定 M4 契约基础上，由 `Engine.Scene` 输出最小可渲染数据，支撑至少一个场景节点驱动画面变化。

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
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G2`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-SCENE-001`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - 场景最小渲染数据生成相关文件
  - 场景模块测试文件
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不增加场景编辑器能力
- 不引入资源系统深度接入
- 不修改渲染模块内部实现
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.App/**`
  - `src/Engine.Asset/**`

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
- Test: `dotnet test` 通过，场景输出相关最小测试通过
- Smoke: 至少一个场景节点可驱动画面变化（显示/隐藏或基础参数变化）
- Perf: 相比 M3 无明显退化

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SCENE-002.md`
- ClosedAt: `2026-04-11 11:40`
- Summary:
  - 在既有 M4 契约基线上，场景输出引入最小动态参数：首节点 `MaterialId` 随帧在 `material://default` 与 `material://pulse` 间切换
  - 保持 Scene-Render 契约稳定，不新增跨模块依赖
  - 补充场景输出测试，覆盖空场景、默认提交与连续帧参数变化
  - 同步更新 `Engine.Scene` 边界合同变更记录
- FilesChanged:
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-002.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-SCENE-002.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 4/4 通过）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，约 `30.15s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，`ExitCode=0`，约 `45.12s`）
- ModuleAttributionCheck: pass
