# TASK-APP-007 归档快照（Execution Prepared）

- TaskId: `TASK-APP-007`
- Title: `M6 App 装配与生命周期校准`
- Priority: `P0`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-App`
- ClosedAt: `2026-04-17 13:20`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 保持 `Engine.App` “仅装配不计算”边界，M6 MVP 计算仍由 Scene/Render 消费链路承担。
- App 装配测试扩展为 M6 口径：验证 native 渲染路径注入 provider 后，相机语义可见且连续帧 `Camera.View` 会变化。
- 新增异常生命周期收口测试：渲染帧失败时，`ApplicationHost` 仍触发窗口关闭请求并执行 `Shutdown` 与 `Dispose`。

## FilesChanged

- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/tasks/task-app-007.md`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-APP-007.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release -m:1`）
- Test: `pass`（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj -m:1`）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.63s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）

## Risks

- `low`：当前覆盖焦点是装配正确性与生命周期收口，不涉及更复杂运行模式切换；后续若引入多渲染路径，需补充对应装配验证。
