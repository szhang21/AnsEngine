# 任务: TASK-REND-004 M4 场景驱动渲染消费

## 目标（Goal）
让 `Engine.Render` 消费 `Engine.Scene` 输出的最小渲染数据完成绘制，替代内部写死 demo 数据路径。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M4-2026-04-11`

## 里程碑引用（兼容别名：MilestoneRef）
`M4-SceneRenderPipeline`

## 执行代理（ExecutionAgent）
Exec-Render

## 优先级（Priority）
P0
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.Render

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G2`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-SCENE-001`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - 渲染消费链路与最小绘制相关文件
  - 渲染模块测试文件
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不引入材质系统与复杂渲染特性
- 不在 `Engine.App` 写入渲染后端细节
- 不改造资源导入体系
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Platform/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Scene`（仅契约）
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Scene` 内部实现
  - `Engine.Render -> Engine.Asset` 导入器实现

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-render.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过，渲染消费链路相关测试通过
- Smoke: 场景驱动渲染链路可持续运行 30 秒以上并正常退出
- Perf: 相比 M3 无明显退化（无异常帧时间抖动）

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-REND-004.md`
- ClosedAt: `2026-04-11 11:40`
- Summary:
  - `Engine.Render` 接入 `Engine.Scene` 契约层并消费 `SceneRenderFrame`，替代原固定单三角硬编码提交路径
  - 新增 `SceneRenderSubmissionBuilder`，将场景提交数据转换为渲染顶点流
  - `NullRenderer` 按场景提交动态更新 VBO（`DynamicDraw`）并绘制
  - 新增渲染消费链路最小测试项目并通过
  - 同步更新 `Engine.Render` 边界合同变更记录
- FilesChanged:
  - `src/Engine.Render/Engine.Render.csproj`
  - `src/Engine.Render/RenderPlaceholders.cs`
  - `src/Engine.Render/SceneRenderSubmission.cs`
  - `tests/Engine.Render.Tests/Engine.Render.Tests.csproj`
  - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/tasks/task-rend-004.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-REND-004.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，约 `30.15s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，`ExitCode=0`，约 `45.12s`）
- ModuleAttributionCheck: pass
