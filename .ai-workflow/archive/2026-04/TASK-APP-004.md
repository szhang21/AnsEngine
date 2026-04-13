# TASK-APP-004 归档快照（Execution Prepared）

- TaskId: `TASK-APP-004`
- Title: `M4 App 契约 Provider 装配`
- Priority: `P0`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-App`
- ClosedAt: `2026-04-14 01:06`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `RuntimeBootstrap` 显式装配契约 provider（`Engine.Contracts.ISceneRenderContractProvider`）并注入 `NullRenderer`。
- 抽出可测试的渲染器创建路径，避免装配测试依赖 GLFW 主线程上下文。
- 新增 `Engine.App.Tests` 并覆盖装配路径测试，验证 Render 注入 provider 与 Scene 实例一致。

## FilesChanged

- `AnsEngine.sln`
- `src/Engine.App/Engine.App.csproj`
- `src/Engine.App/ApplicationBootstrap.cs`
- `tests/Engine.App.Tests/Engine.App.Tests.csproj`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-app-004.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-APP-004.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`，环境级 CS1668 警告）
- Build(Release): `pass`（`dotnet build -c Release -m:1`，环境级 CS1668 警告）
- Test: `pass`（`dotnet test -m:1`，`Engine.App.Tests` 1/1）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`18.88s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`33.85s`）

## Risks

- `low`：测试通过反射验证装配细节，后续若字段重命名需同步测试；运行时行为不受影响。
