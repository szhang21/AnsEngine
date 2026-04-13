# 任务: TASK-APP-004 M4 App 契约 Provider 装配

## 目标（Goal）
由 `Engine.App` 负责契约 provider 的 DI 装配与生命周期编排，确保 Render 仅消费契约，不感知 Scene 实现。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M4B-2026-04-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M4-SceneRenderPipeline`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P0
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Render

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-CONTRACT-001`
  - `TASK-REND-006`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - App 组合根与 DI 装配相关文件
  - 相关测试文件
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不在 App 层实现渲染细节
- 不改动 Scene/Render 业务语义
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.Asset/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Contracts`
- ForbiddenDependsOn:
  - `Engine.App` 内直接实现渲染后端
  - `Engine.App` 内直接实现领域数据生产

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `[]`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过，装配路径测试通过
- Smoke: 应用可启动并稳定渲染，退出码 `0`
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
- FailureType: `PostAcceptanceBug`
- DetectedAt: `2026-04-11`
- ReopenReason:
- OriginTaskId: `TASK-APP-003`
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-APP-004.md`
- ClosedAt: `2026-04-14 01:06`
- Summary:
  - `Engine.App` 组合根显式装配 `Engine.Contracts.ISceneRenderContractProvider` 并注入 `Engine.Render`。
  - 新增可测试渲染器创建路径，避免装配测试触发 GLFW 主线程限制。
  - 新增 `Engine.App.Tests` 验证契约 provider 注入路径并接入 solution。
- FilesChanged:
  - `AnsEngine.sln`
  - `src/Engine.App/Engine.App.csproj`
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `tests/Engine.App.Tests/Engine.App.Tests.csproj`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-APP-004.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1`；`Engine.App.Tests` 1/1）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`18.88s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`33.85s`）
- ModuleAttributionCheck: pass
