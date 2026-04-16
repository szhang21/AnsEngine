# 任务: TASK-REND-008 M5 Render 变换消费与提交应用

## TaskId
`TASK-REND-008`

## 目标（Goal）
在 `Engine.Render` 提交路径中消费并应用 transform（Position、Scale、Rotation），保持 identity 默认行为，并持续维持 `Render -> Contracts` 依赖方向。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M5-2026-04-14`

## 里程碑引用（兼容别名：MilestoneRef）
`M5`

## 执行代理（ExecutionAgent）
Exec-Render

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Render

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M5-G2`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-CONTRACT-002`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - Render 提交构建与 transform（含 Rotation）应用逻辑
  - Render 对应测试
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不修改 Scene 数据生产逻辑
- 不新增材质系统或资源管线能力
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Scene` 编译期依赖

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
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过，Render transform（含 Rotation）消费与 identity 回归测试通过
- Smoke: 运行时可见 transform（含 Rotation）效果且可稳定退出
- Perf: 相比 M4b 无明显帧时间退化

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-REND-008.md`
- ClosedAt: `2026-04-15 19:32`
- Summary:
  - `SceneRenderSubmissionBuilder` 对每个顶点按 `Scale -> Rotation -> Translation` 应用 transform。
  - 增加 identity 快路径，确保无 transform 退化场景保持旧布局与行为。
  - Render 测试补充 identity 回归与 Rotation 生效断言，验证契约消费链路稳定。
- FilesChanged:
  - `src/Engine.Render/SceneRenderSubmission.cs`
  - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
  - `.ai-workflow/tasks/task-rend-008.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-REND-008.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1`，`Engine.Render.Tests` 6/6）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`25.50s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`40.36s`）
- ModuleAttributionCheck: pass
