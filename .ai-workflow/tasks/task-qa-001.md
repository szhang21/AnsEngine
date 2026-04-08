# 任务: TASK-QA-001 可见反馈门禁证据补齐

## 目标（Goal）
在 M2 功能完成后补齐“可启动、可见、可退出”的验证闭环，沉淀 Build/Test/Smoke/Perf 证据，不新增产品能力。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-VISIBLE-001`

## 里程碑引用（兼容别名：MilestoneRef）
`M2-FirstVisibleFrame`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P1
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Platform
- Engine.Render

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-APP-001`
  - `TASK-REND-001`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - 应用级 smoke/验证说明相关文件
  - 必要的测试或校验文件
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不新增平台窗口能力
- 不新增渲染特性
- 不把日志面板、FPS 展示纳入本卡
- OutOfScopePaths:
  - `src/Engine.Platform/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Render`
- ForbiddenDependsOn:
  - 为过门禁临时扩大功能范围
  - 在验证卡中追加新的平台或渲染特性

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过；代码轨按 `AllowedPaths` 校验
- Test: `dotnet test` 通过，必要时补最小运行链路校验；测试改动仅允许命中 `tests/**`
- Smoke: 明确记录“可启动、可见、可退出”的执行步骤与结果；代码轨与边界文档轨双轨门禁均需可验证
- Perf: 明确记录“连续运行 30-60 秒无明显异常”的观察结果；边界文档轨单独按 `BoundaryDocsToUpdate` 校验并要求变更日志

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-001.md`
- ClosedAt: `2026-04-07 13:10`
- Summary:
  - 复核并落盘 M2 验证闭环：Build/Test/Smoke/Perf 证据补齐。
  - 明确“可启动、可见、可退出”执行步骤与结果，确认退出码为 `0`。
  - 同步更新 `Engine.App` 边界合同 `Boundary Change Log`（验证口径补充）。
- FilesChanged:
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-qa-001.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-001.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug`）
  - Build(Release): pass（`dotnet build -c Release`）
  - Test: pass（`dotnet test`）
  - Smoke: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=10` 下运行稳定，退出码 `0`）
  - Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=45` 下运行约 55s，无明显异常，退出码 `0`）
- ModuleAttributionCheck: pass
