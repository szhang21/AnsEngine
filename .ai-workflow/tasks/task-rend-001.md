# 任务: TASK-REND-001 最小清屏可视反馈

## 目标（Goal）
让 `Engine.Render` 在真实窗口和主循环稳定后输出持续可见的非默认背景清屏结果，证明 `Platform -> App -> Render` 主链路打通。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M1M2-2026-04-08`

## 里程碑引用（兼容别名：MilestoneRef）
`M2-FirstVisibleFrame`

## 执行代理（ExecutionAgent）
Exec-Render

## 优先级（Priority）
P1
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.Render

## 次级模块（SecondaryModules）
- Engine.Platform
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-APP-001`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - 渲染器实现文件
  - 渲染公开合同相关文件
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现三角形、Shader/Mesh/Buffer 完整链路
- 不接管窗口生命周期
- 不引入材质系统、资源导入管线、复杂场景集成
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `tests/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Platform`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Scene` 内部实现
  - `Engine.Render -> Engine.Asset` 导入器实现
  - `Engine.Render` 内承担应用主循环编排

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-render.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过；代码轨按 `AllowedPaths` 校验
- Test: `dotnet test` 继续通过；测试改动若存在仅允许命中 `tests/**`
- Smoke: 启动后窗口持续显示非默认背景清屏结果，运行 30-60 秒不崩溃，关闭后释放正常；代码轨与边界文档轨双轨门禁均需可验证
- Perf: 仅记录基础帧时间或“无明显退化”说明；边界文档轨单独按 `BoundaryDocsToUpdate` 校验并要求变更日志

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-REND-001.md`
- ClosedAt: `2026-04-07 12:20`
- Summary:
  - 在 `Engine.Render` 增加最小可见反馈：初始化设置非默认清屏色，每帧执行 `GL.Clear`。
  - 保持渲染职责边界不变，不引入三角形/Shader/Mesh 完整链路。
  - 同步更新 `Engine.Render` 边界合同与 `Boundary Change Log`。
- FilesChanged:
  - `src/Engine.Render/RenderPlaceholders.cs`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/tasks/task-rend-001.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-REND-001.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug`，存在环境级 CS1668 警告）
  - Build(Release): pass（`dotnet build -c Release`，存在环境级 CS1668 警告）
  - Test: pass（`dotnet test`）
  - Smoke: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=30` 下运行稳定并退出码 `0`）
  - Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=45` 下运行约 55s，退出码 `0`）
- ModuleAttributionCheck: pass
