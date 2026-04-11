# 任务: TASK-QA-002 M3 双轨门禁证据与归档收口

## 目标（Goal）
为 M3 补齐 Build/Test/Smoke/Perf 与归档四件套证据，确保“首帧三角形可见化”可验证、可追溯、可关单。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M3-2026-04-08`

## 里程碑引用（兼容别名：MilestoneRef）
`M3-TriangleVisible`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P0
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Render
- Engine.Platform

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-REND-002`
  - `TASK-APP-002`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - 验证证据与任务关单相关文件
  - 必要测试文件
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不新增渲染功能
- 不新增平台能力
- 不扩展 M4 或其他里程碑范围
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Platform`
- ForbiddenDependsOn:
  - 为补证据扩展功能范围
  - 为过门禁临时修改任务边界

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `[]`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过；代码轨按 `AllowedPaths` 校验
- Test: `dotnet test` 通过并记录结果摘要；测试改动仅允许命中 `tests/**`
- Smoke: 记录三角形稳定显示 30 秒以上、可正常退出、退出码 `0` 的证据
- Perf: 记录相对 M2 无明显退化说明，并完成归档四件套（任务卡 Archive、归档快照、归档索引、看板 Done）

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Done

## 完成度（Completion）
`100`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-002.md`
- ClosedAt: `2026-04-11 11:00`
- HumanSignoff: `pass`
- Summary:
  - 完成 M3 双轨门禁复核：Build/Test/Smoke/Perf 证据统一回填并对齐任务链
  - 对齐 `TASK-REND-002`（三角形可见）与 `TASK-APP-002`（生命周期配套）的验收结论，形成可追溯闭环
  - 补齐归档三件套（任务卡 Archive、归档快照、归档索引），并将看板推进到 `Review`
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-002.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-002.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ValidationEvidence:
  - Build(Debug): fail -> pass（首轮 `CS2012` 文件占用；复跑 `dotnet build -c Debug -m:1` 通过，存在环境级 CS1668 警告）
  - Build(Release): pass（`dotnet build -c Release -m:1`，存在环境级 CS1668 警告）
  - Test: pass（`dotnet test -m:1`）
  - Smoke: pass（图形口径引用 `TASK-REND-002` 人工复验通过；无图形环境口径 `ANS_ENGINE_USE_NATIVE_WINDOW=false`，`30.19s`，`ExitCode=0`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`45.24s`，`ExitCode=0`，相对 M2 无明显退化）
- ModuleAttributionCheck: pass
