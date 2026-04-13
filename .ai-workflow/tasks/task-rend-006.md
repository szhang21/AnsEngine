# 任务: TASK-REND-006 M4 Render 依赖反转与解耦

## 目标（Goal）
将 `Engine.Render` 的编译期依赖改为仅面向渲染输入契约层（例如 `Engine.Contracts` / `Engine.Render.Contracts`），移除对 `Engine.Scene` 的直接项目引用。

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
- Engine.App

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-CONTRACT-001`
  - `TASK-SCENE-003`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - Render 消费契约与项目引用调整相关文件
  - 相关测试文件
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`
> 说明：`AllowedPaths` 仅用于源码/测试改动范围，不包含边界文档路径。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在本卡引入新渲染特性（只做依赖解耦与契约消费改造）。
- 不修复 Scene 内部语义实现。
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Platform/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Scene` 编译期依赖
  - `Engine.Render -> Engine.Asset` 导入器实现

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`
> 说明：`BoundaryDocsToUpdate` 为独立规则，不受 `AllowedPaths` 限制。
> 触发条件：仅当 `NewFilesExpected=true` 或执行中实际新增源码/测试文件时，才强制执行边界文档更新。

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；Render 消费契约路径测试通过
- Smoke: 场景可驱动渲染输出，最小图元稳定可见（本卡不新增渲染特性）
- Perf: 相比 M4 初版无明显退化

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-REND-006.md`
- ClosedAt: `2026-04-14 00:49`
- Summary:
  - `Engine.Render` 移除对 `Engine.Scene` 的编译期项目依赖，改为仅消费 `Engine.Contracts`。
  - `Render` 提交构建与默认 Provider 全量切换为 `Engine.Contracts` 契约类型。
  - `Engine.Render.Tests` 去除对 `Engine.Scene` 引用并完成契约消费路径验证。
- FilesChanged:
  - `src/Engine.Render/Engine.Render.csproj`
  - `src/Engine.Render/RenderPlaceholders.cs`
  - `src/Engine.Render/SceneRenderSubmission.cs`
  - `tests/Engine.Render.Tests/Engine.Render.Tests.csproj`
  - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-REND-006.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1`；Render 契约消费测试通过）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`20.68s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`35.66s`）
- ModuleAttributionCheck: pass
