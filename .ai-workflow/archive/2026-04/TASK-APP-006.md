# TASK-APP-006 归档快照（Execution Prepared）
- TaskId: `TASK-APP-006`
- Title: `M5 App 装配兼容校准`
- Priority: `P0`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-App`
- ClosedAt: `2026-04-15 15:23`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 保持 `Engine.App` “仅装配不计算”边界，继续使用 `Scene -> Contracts -> Render` 链路。
- App 装配测试增加 M5 校准断言：provider 在初始化后可输出含 rotation 的连续帧 transform。
- 生命周期回归确认：初始化、帧驱动、收口在 M5 链路下稳定。

## FilesChanged

- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/tasks/task-app-006.md`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-APP-006.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build -c Release -m:1`）
- Test: `pass`（`dotnet test -m:1`）
- Smoke: `pass`（`dotnet run --no-build` + `ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.80s`）
- Perf: `pass`（`dotnet run --no-build` + `ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.80s`）

## Risks

- `low`：当前 App 仅验证装配链路，不负责 transform 数学行为；后续链路退化需由 Scene/Render 侧测试共同拦截。

