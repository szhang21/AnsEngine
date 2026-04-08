# TASK-PLAT-001 归档快照

- TaskId: `TASK-PLAT-001`
- Title: 真实窗口生命周期落地
- Priority: `P0`
- PrimaryModule: `Engine.Platform`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-platform.md`
- BaselineRef: `references/project-baseline.md`
- ExecutionAgent: `Exec-Platform`
- ClosedAt: `2026-04-07 10:35`
- Status: `Done`
- Completion: `100`
- ModuleAttributionCheck: `pass`

## Scope

- AllowedModules:
  - `Engine.Platform`
- AllowedPaths:
  - `src/Engine.Platform/**`
  - `tests/**`

## Summary

- 扩展 `IWindowService` 接口，新增 `IsCloseRequested`、`Exists`、`ProcessEvents`、`RequestClose` 和 `IDisposable` 生命周期约束。
- 将 `NullWindowService` 从占位实现升级为真实窗口服务：可创建 OpenTK `NativeWindow`、处理事件、感知关闭意图并正确释放。
- 为无图形环境保留兼容模式：`useNativeWindow: false`。
- 同步更新平台边界合同中的公开接口描述与 `Boundary Change Log`。

## FilesChanged

- `src/Engine.Platform/PlatformContracts.cs`
- `src/Engine.Platform/PlatformPlaceholders.cs`
- `tests/Engine.Asset.Tests/AssetServiceTests.cs`
- `.ai-workflow/boundaries/engine-platform.md`
- `.ai-workflow/tasks/task-plat-001.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-PLAT-001.md`

## ValidationEvidence

- Build(Debug): pass（`dotnet build -c Debug`）
- Build(Release): pass（`dotnet build -c Release`）
- Test: pass（`dotnet test`）
- Smoke: pass（`dotnet run --project src/Engine.App/Engine.App.csproj`）
- Perf: pass（基础观察无卡死或阻塞）

## Risks

- RiskLevel: `medium`
- Notes:
  - 主循环尚未接入窗口事件驱动与关闭分支编排（属于后续 `Engine.App` 任务范围）。
