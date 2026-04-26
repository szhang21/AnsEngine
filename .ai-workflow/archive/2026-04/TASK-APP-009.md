# TASK-APP-009 归档快照

- TaskId: `TASK-APP-009`
- Title: `M10 场景文件选择与 SceneData loader 装配`
- Priority: `P1`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-App`
- ClosedAt: `2026-04-26 15:45`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `Engine.App` 新增对 `Engine.SceneData` 的装配，组合根现在会显式创建 `JsonSceneDescriptionLoader` 并在启动路径中消费 `SceneDescriptionLoadResult`。
- 新增默认样例场景文件与 `ANS_ENGINE_SCENE_PATH` 覆盖入口，确保修改场景文件即可影响运行结果，而无需改 C# 代码。
- `ApplicationHost` 启动时先加载场景描述、再初始化 Scene 运行时；加载失败时显式输出错误并受控退出，不再保留隐式硬编码场景后门。

## FilesChanged

- `src/Engine.App/ApplicationBootstrap.cs`
- `src/Engine.App/Engine.App.csproj`
- `src/Engine.App/SceneRuntimeContracts.cs`
- `src/Engine.App/SampleScenes/default.scene.json`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/boundaries/engine-scenedata.md`
- `.ai-workflow/tasks/task-app-009.md`
- `.ai-workflow/archive/2026-04/TASK-APP-009.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.App/Engine.App.csproj -c Debug --nologo -v minimal`）
- Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.App/Engine.App.csproj -c Release --nologo -v minimal --no-restore`）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --nologo -v minimal --no-restore`；6 条通过）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=1 /Users/ans/.dotnet/dotnet run --project src/Engine.App/Engine.App.csproj --no-build` 退出码 0）
- Perf: `pass`（场景文件加载只发生在启动阶段，稳定运行阶段无重复 loader 创建或场景重载）

## Risks

- `low`：当前对外覆盖入口先收敛为 `ANS_ENGINE_SCENE_PATH` 环境变量；后续如果扩到配置文件或命令行参数，应继续由 App 负责来源解析和注入，不改变 `ISceneDescriptionLoader.Load(string sceneFilePath)` 主语义。
