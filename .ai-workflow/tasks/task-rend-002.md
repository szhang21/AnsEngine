# 任务: TASK-REND-002 首帧三角形最小渲染链路

## 目标（Goal）
在 `Engine.Render` 内完成最小 shader 编译/链接、顶点提交与 draw 调用链路，实现窗口内稳定可见三角形。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M3-2026-04-08`

## 里程碑引用（兼容别名：MilestoneRef）
`M3-TriangleVisible`

## 执行代理（ExecutionAgent）
Exec-Render

## 优先级（Priority）
P0
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
- ParallelGroup: `G1`
- CanRunParallel: `false`
- DependsOn:
  - `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - 渲染器实现与最小三角形渲染链路相关文件
  - 渲染模块测试文件
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 `Engine.App` 主循环职责边界
- 不引入材质系统、资源管线改造、场景系统深化
- 不新增多图元/多通道渲染特性
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Platform`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Scene` 内部实现
  - `Engine.Render -> Engine.Asset` 导入器实现
  - 在渲染模块内承载应用主循环逻辑

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-render.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过；代码轨按 `AllowedPaths` 校验
- Test: `dotnet test` 通过；新增/修改测试仅允许命中 `tests/**`
- Smoke: 启动后窗口可稳定显示三角形 30 秒以上，关闭窗口后正常退出
- Perf: 相对 M2 无明显退化，并记录运行观察说明；触发边界同步时文档轨按 `BoundaryDocsToUpdate` 校验

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Todo

## 完成度（Completion）
`0`

## 归档（Archive）
- ArchivePath:
- ClosedAt:
- Summary:
- FilesChanged:
- ValidationEvidence:
- ModuleAttributionCheck: pass | fail
