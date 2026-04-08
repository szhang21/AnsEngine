# TASK-APP-001 归档快照

- TaskId: `TASK-APP-001`
- Title: 最小主循环与退出编排
- Priority: `P0`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- BaselineRef: `references/project-baseline.md`
- ExecutionAgent: `Exec-App`
- ClosedAt: `2026-04-07 11:10`
- Status: `Done`
- Completion: `100`
- ModuleAttributionCheck: `pass`

## Scope

- AllowedModules:
  - `Engine.App`
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## Summary

- 将应用执行路径从单次调用升级为持续主循环，按帧处理事件、输入/时间快照与渲染调用。
- 在主循环中消费窗口生命周期信号（`Exists` / `IsCloseRequested`），关闭时退出并返回 `0`。
- 通过 `finally` 统一收口 `renderer.Shutdown()` 与 `windowService.Dispose()`，保证资源释放顺序。
- 增加 `ANS_ENGINE_AUTO_EXIT_SECONDS`，用于自动化 smoke/perf 证据采集。
- 同步更新 `Engine.App` 边界合同与 `Boundary Change Log`。

## FilesChanged

- `src/Engine.App/ApplicationBootstrap.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-app-001.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-APP-001.md`

## ValidationEvidence

- Build(Debug): pass（`dotnet build -c Debug`）
- Build(Release): pass（`dotnet build -c Release`）
- Test: pass（`dotnet test`）
- Smoke: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=2` 下运行并退出码 `0`）
- Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=30` 下运行约 36 秒，退出码 `0`）

## Risks

- RiskLevel: `low`
- Notes:
  - 当前 smoke 的“用户关闭窗口”采用自动退出开关近似验证；真实人工关闭验证可在本地交互环境补一条截图/录屏证据。
