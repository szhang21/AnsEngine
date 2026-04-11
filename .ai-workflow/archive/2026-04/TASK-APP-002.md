# TASK-APP-002 归档快照（Execution Prepared）

- TaskId: `TASK-APP-002`
- Title: `M3 运行装配与生命周期配套`
- Priority: `P0`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-App`
- ClosedAt: `2026-04-11 10:45`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 将 `ApplicationHost.Run` 的渲染初始化纳入统一 `try/finally`，保障初始化失败时仍可收口。
- 异常路径增加关闭意图发出，减少失败后窗口状态悬挂风险。
- 装配新增 `ANS_ENGINE_USE_NATIVE_WINDOW` 开关（默认 `true`），保持 M3 真实窗口路径不变。
- 无图形环境下装配 `HeadlessRenderer`，用于稳定执行 smoke/perf 验证。

## FilesChanged

- `src/Engine.App/ApplicationBootstrap.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-app-002.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-APP-002.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build -c Release -m:1`）
- Test: `pass`（`dotnet test -m:1`）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，约 `15.64s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，约 `45.14s`）

## Risks

- `medium`：当前执行环境无法稳定创建真实窗口（`NativeWindow` 触发 `AccessViolation`），本次 smoke/perf 采用 headless 路径验证；建议在图形桌面环境补做一次真实窗口口径复验。
