# 任务: TASK-CONTRACT-002 M5 渲染变换契约兼容扩展

## TaskId
`TASK-CONTRACT-002`

## 目标（Goal）
在 `Engine.Contracts` 中为渲染输入增加 transform 能力（Position、Scale、Rotation），并保持无显式 transform 时的 identity 兼容行为。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M5-2026-04-14`

## 里程碑引用（兼容别名：MilestoneRef）
`M5`

## 执行代理（ExecutionAgent）
Exec-Contracts

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Contracts

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Render

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-contracts.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M5-G1`
- CanRunParallel: `false`
- DependsOn: `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Contracts
- AllowedFiles:
  - Contracts 渲染输入模型（含 Rotation）与兼容辅助类型
  - Contracts 对应测试
- AllowedPaths:
  - `src/Engine.Contracts/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现 Scene 数据生产逻辑
- 不实现 Render 绘制算法
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Contracts -> Engine.Core`（如已存在）
- ForbiddenDependsOn:
  - `Engine.Contracts -> Engine.Scene`
  - `Engine.Contracts -> Engine.Render`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；契约兼容路径测试通过
- Smoke: 现有运行路径无回退（无 transform 输入时行为保持一致；有 Rotation 输入时链路可用）
- Perf: 无显著新增分配或初始化退化

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-002.md`
- ClosedAt: `2026-04-15 13:46`
- Summary:
  - 新增 `SceneTransform`（Position/Scale/Rotation）并提供 `Identity` 兼容默认值。
  - `SceneRenderItem` 扩展 transform 字段，保留旧三参构造函数以保证无改造调用方默认得到 identity transform。
  - 补充 Contracts 测试覆盖默认兼容路径与 Rotation 保真路径。
- FilesChanged:
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
  - `.ai-workflow/tasks/task-contract-002.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-CONTRACT-002.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1`，`Engine.Contracts.Tests` 4/4）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`19.37s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`34.38s`）
- ModuleAttributionCheck: pass

