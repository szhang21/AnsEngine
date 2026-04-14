# TASK-APP-005 归档快照（Execution Prepared）
- TaskId: `TASK-APP-005`
- Title: `M4b App 场景运行时抽象依赖修复`
- Priority: `P0`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-App`
- ClosedAt: `2026-04-14 17:14`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 在 `Engine.App` 新增最小场景运行时接口 `ISceneRuntime`，`ApplicationHost` 由依赖 `SceneGraphService` 具体类型改为依赖接口。
- 组合根新增 `SceneRuntimeAdapter` 负责将 `SceneGraphService` 绑定为 `ISceneRuntime`，并保持 `ISceneRenderContractProvider` 注入路径不变。
- 测试补充 `ApplicationHost` 抽象驱动路径，验证场景初始化通过接口触发且主循环可正常退出。

## FilesChanged

- `src/Engine.App/ApplicationBootstrap.cs`
- `src/Engine.App/SceneRuntimeContracts.cs`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/tasks/task-app-005.md`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-APP-005.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build -c Release -m:1`）
- Test: `pass`（`dotnet test -m:1`，`Engine.App.Tests` 2/2）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`18.63s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`32.37s`）

## Risks

- `low`：`ISceneRuntime` 目前仅暴露初始化动作；若后续需要更多运行时编排能力，需在接口上进行增量扩展。

