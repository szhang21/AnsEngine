# TASK-APP-010 归档快照

- TaskId: `TASK-APP-010`
- Title: `M15 App 主循环 runtime update 接线`
- Priority: `P1`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-App`
- ClosedAt: `2026-05-01 20:33`
- Status: `Review`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- `ISceneRuntime` 新增显式 update 入口，App 主循环每帧在 render 前调用 scene runtime update。
- `ApplicationHost.Run()` 成功路径顺序为 ProcessEvents -> Input -> Time -> SceneRuntime.Update -> RenderFrame -> Present。
- `SceneRuntimeAdapter` 负责把 `TimeSnapshot` / `InputSnapshot` 翻译成 `SceneUpdateContext`，再转发给 `SceneGraphService.UpdateRuntime(...)`。
- loader failure 与 render failure 的既有关闭、shutdown、dispose 语义保持稳定。

## FilesChanged

- `src/Engine.App/SceneRuntimeContracts.cs`
- `src/Engine.App/ApplicationBootstrap.cs`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-app-010.md`
- `.ai-workflow/archive/2026-05/TASK-APP-010.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`；App.Tests 8 条通过）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=1 /Users/ans/.dotnet/dotnet run --project src/Engine.App/Engine.App.csproj --nologo`，退出码 0）
- Boundary: `pass`（仅改 `src/Engine.App/**`、`tests/Engine.App.Tests/**` 与任务指定边界/归档文档；未引用 Scene runtime internal 类型）
- Perf: `pass`（每帧新增一次 update 调用，不引入逐帧文件 IO、重复 scene initialize 或 render 前后双重 update）

## Risks

- `low`：App 只新增主循环阶段编排和 adapter 翻译；后续 editor/play mode 或正式 update phases 需单独任务卡承接。
