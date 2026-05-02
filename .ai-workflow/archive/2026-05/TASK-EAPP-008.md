# TASK-EAPP-008 归档快照

- TaskId: `TASK-EAPP-008`
- Title: `M16 Inspector component groups integration`
- Priority: `P1`
- PrimaryModule: `Engine.Editor.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
- Owner: `Exec-EditorApp`
- ClosedAt: `2026-05-02 12:43`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- Inspector snapshot/rendering/input state migrated from flat fields to `Object` / `Transform` / `MeshRenderer` component groups.
- GUI edit submission routes through `SceneEditorSession` component APIs for Transform and MeshRenderer.
- Transform-only objects show a clear no-`MeshRenderer` state and are not auto-fixed by apply/save flows.
- Default Add Object still creates Transform + MeshRenderer so newly added objects remain visible.
- Editor.App tests and fixtures now use M16 `version: "2.0"` component arrays.

## FilesChanged

- `src/Engine.Editor.App/EditorInspectorSnapshot.cs`
- `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
- `src/Engine.Editor.App/EditorInspectorInputState.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `src/Engine.Editor.App/EditorAppController.cs`
- `src/Engine.Editor.App/EditorDefaultObjectFactory.cs`
- `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
- `tests/Engine.Editor.App.Tests/EditorInspectorInputStateTests.cs`
- `tests/Engine.Editor.App.Tests/EditorObjectWorkflowStateTests.cs`
- `tests/Engine.Editor.App.Tests/EditorFileWorkflowStateTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-008.md`
- `.ai-workflow/archive/2026-05/TASK-EAPP-008.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal`；Engine.Editor.App.Tests 33 条通过）
- Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=0.1 ANS_ENGINE_EDITOR_ENABLE_NATIVE_IMGUI_FRAMES=0 /Users/ans/.dotnet/dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build --no-restore`，ExitCode=0）
- Smoke: `pass`（Inspector exposes `Object` / `Transform` / `MeshRenderer`; Transform-only object shows `No MeshRenderer component.` and apply does not add MeshRenderer）
- Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档；未新增 forbidden project references）
- Perf: `pass`（GUI component editing remains explicit user/session commands；无逐帧文件写入、无 sample scene reload polling）

## Risks

- `low`：当前 GUI 仅展示 no-`MeshRenderer` 合法空状态，不提供显式添加 MeshRenderer 按钮；后续若需要组件增删 UI 应另立任务。
