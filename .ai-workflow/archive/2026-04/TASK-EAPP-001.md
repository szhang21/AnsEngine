# TASK-EAPP-001 归档快照

- TaskId: `TASK-EAPP-001`
- Title: `M13 Editor GUI 宿主入口`
- Priority: `P0`
- PrimaryModule: `Engine.Editor.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
- Owner: `Exec-EditorApp`
- ClosedAt: `2026-04-30 14:28`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 新增独立可执行 `Engine.Editor.App` 项目，引用 `Engine.Editor`、`Engine.SceneData`、`Engine.Contracts`、`Engine.Platform`、OpenTK 与 ImGui.NET。
- 新增 `EditorAppController` 与 `EditorScenePathResolver`，启动时优先解析 `ANS_ENGINE_EDITOR_SCENE_PATH`，否则定位源码目录 `src/Engine.App/SampleScenes/default.scene.json`。
- 新增最小 OpenTK 窗口主循环并创建 ImGui context，支持自动退出 smoke 环境变量，不改变 `Engine.App` 默认运行路径。
- 新增 `Engine.Editor.App.Tests` 边界与启动 scene 测试，确认 GUI 依赖只存在于 Editor App。

## FilesChanged

- `AnsEngine.sln`
- `src/Engine.Editor.App/Engine.Editor.App.csproj`
- `src/Engine.Editor.App/Program.cs`
- `src/Engine.Editor.App/EditorAppProgram.cs`
- `src/Engine.Editor.App/EditorAppOptions.cs`
- `src/Engine.Editor.App/EditorScenePathResolver.cs`
- `src/Engine.Editor.App/EditorAppController.cs`
- `src/Engine.Editor.App/EditorAppWindow.cs`
- `tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj`
- `tests/Engine.Editor.App.Tests/EditorAppBoundaryTests.cs`
- `tests/Engine.Editor.App.Tests/EditorAppControllerTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/boundaries/README.md`
- `.ai-workflow/tasks/task-eapp-001.md`
- `.ai-workflow/archive/2026-04/TASK-EAPP-001.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning 与本机 Windows Kits `LIB` warning）
- Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；新增 `Engine.Editor.App.Tests` 5 条通过，整解测试通过）
- Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；退出码 0，默认尝试打开源码 sample scene）
- Boundary: `pass`（`Engine.Editor.App` 未引用 `Engine.App/Render/Asset`；`Engine.Editor` 未新增 OpenTK/ImGui/窗口依赖；`Engine.App` 未引用 Editor App）
- Perf: `pass`（仅新增 GUI 宿主启动与空帧清屏；未改变 `Engine.App` 主路径，无逐帧文件 IO）

## Risks

- `low`：当前窗口已创建 ImGui context，但真实面板绘制在后续 `TASK-EAPP-002` 起逐步接入。
