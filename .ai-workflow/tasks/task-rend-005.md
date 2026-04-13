# 任务: TASK-REND-005 M4 渲染边界文档对齐当前依赖

## 目标（Goal）
先将 `.ai-workflow/boundaries/engine-render.md` 对齐到当前真实依赖状态，消除文档与实现漂移，为后续解耦改造提供准确基线。

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
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.Render

## 次级模块（SecondaryModules）
- Engine.Scene

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G1`
- CanRunParallel: `false`
- DependsOn:
  - `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - 渲染边界合同文件与必要任务元数据
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`
> 说明：`AllowedPaths` 仅用于源码/测试改动范围，不包含边界文档路径。

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不改变渲染功能行为（本卡仅对齐边界文档，不调整代码实现）。
- 不进行契约下沉实现改造。
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Platform/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Asset` 导入器实现
  - `Engine.Render` 内承载应用编排逻辑

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
> 说明：`BoundaryDocsToUpdate` 为独立规则，不受 `AllowedPaths` 限制。
> 触发条件：仅当 `NewFilesExpected=true` 或执行中实际新增源码/测试文件时，才强制执行边界文档更新。

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过
- Smoke: 本卡仅文档对齐，不改变运行行为；现有 smoke 结果不回退
- Perf: 无新增性能风险；记录“文档对齐任务”说明即可

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-REND-005.md`
- ClosedAt: `2026-04-13 20:38`
- Summary:
  - 对齐 `Engine.Render` 边界文档到当前真实依赖：当前实现仍为 `Engine.Render -> Engine.Scene`（契约消费）
  - 修正文档漂移：`Engine.Contracts` 仍属后续目标，不应标记为当前已落地依赖
  - 更新边界变更日志并记录后续回切条件（完成契约下沉后再回切到 `Engine.Contracts`）
- FilesChanged:
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/tasks/task-rend-005.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-REND-005.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`，存在环境级 CS1668 警告）
  - Build(Release): pass（`dotnet build -c Release -m:1`，存在环境级 CS1668 警告）
  - Test: pass（`dotnet test -m:1`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，约 `15.17s`；无行为回退）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，约 `30.17s`，无明显退化）
- ModuleAttributionCheck: pass
