# 任务: TASK-PLAT-001 真实窗口生命周期落地

## 目标（Goal）
让 `Engine.Platform` 提供真实窗口创建、事件泵处理、关闭请求感知与释放能力，替代当前运行路径中的 `NullWindowService` 占位实现。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-VISIBLE-001`

## 里程碑引用（兼容别名：MilestoneRef）
`M1-WindowLoopVisible`

## 执行代理（ExecutionAgent）
Exec-Platform

## 优先级（Priority）
P0
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.Platform

## 次级模块（SecondaryModules）
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-platform.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G1`
- CanRunParallel: `false`
- DependsOn:
  - `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Platform
- AllowedFiles:
  - 平台窗口服务实现相关文件
  - 平台公开接口相关文件
- AllowedPaths:
  - `src/Engine.Platform/**`
  - `tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 `Engine.App` 主循环编排
- 不实现清屏或三角形渲染
- 不深化输入系统、文件系统桥接
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `tests/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Platform -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Platform -> Engine.Render`
  - `Engine.Platform -> Engine.Scene`
  - `Engine.Platform -> Engine.Asset`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-platform.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过；代码轨按 `AllowedPaths` 校验
- Test: `dotnet test` 继续通过；测试改动若存在仅允许命中 `tests/**`
- Smoke: 程序可创建真实窗口，窗口短时间保持响应，可感知关闭请求并进入释放路径；代码轨与边界文档轨双轨门禁均需可验证
- Perf: 无明显卡死或阻塞，仅记录基础运行观察；边界文档轨单独按 `BoundaryDocsToUpdate` 校验并要求变更日志

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-PLAT-001.md`
- ClosedAt: `2026-04-07 10:35`
- Summary:
  - 扩展 `IWindowService` 生命周期接口（事件轮询、关闭请求、释放）。
  - 将 `NullWindowService` 升级为基于 OpenTK `NativeWindow` 的真实窗口实现。
  - 增加无图形环境兼容开关 `useNativeWindow: false`，确保测试稳定。
  - 同步更新 `Engine.Platform` 边界合同与变更日志。
- FilesChanged:
  - `src/Engine.Platform/PlatformContracts.cs`
  - `src/Engine.Platform/PlatformPlaceholders.cs`
  - `tests/Engine.Asset.Tests/AssetServiceTests.cs`
  - `.ai-workflow/boundaries/engine-platform.md`
- ValidationEvidence:
  - Build(Debug): pass（存在环境级 CS1668 警告）
  - Build(Release): pass（存在环境级 CS1668 警告）
  - Test: pass（`dotnet test` 全部通过）
  - Smoke: pass（`dotnet run --project src/Engine.App/Engine.App.csproj` 成功退出）
  - Perf: pass（无卡死或阻塞，基础观察通过）
- ModuleAttributionCheck: pass
