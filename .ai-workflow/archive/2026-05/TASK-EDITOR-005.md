# TASK-EDITOR-005 归档快照

- TaskId: `TASK-EDITOR-005`
- Title: `M16 Editor component API migration`
- Priority: `P1`
- PrimaryModule: `Engine.Editor`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor.md`
- Owner: `Exec-Editor`
- ClosedAt: `2026-05-02 12:37`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneEditorSession` headless edit path migrated to component operations for Transform and MeshRenderer.
- Default `AddObject(string, string)` creates Transform + MeshRenderer so new objects remain visible.
- Transform-only objects can be opened, selected, edited, saved, and reloaded without auto-adding MeshRenderer.
- Legacy flat update methods remain only as short-term compatibility bridges into component operations.
- Downstream `Engine.Editor.App.Tests` legacy fixture failures are deferred to `TASK-EAPP-008`, which owns GUI/test paths.

## FilesChanged

- `src/Engine.Editor/Session/SceneEditorSession.cs`
- `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
- `.ai-workflow/boundaries/engine-editor.md`
- `.ai-workflow/tasks/task-editor-005.md`
- `.ai-workflow/archive/2026-05/TASK-EDITOR-005.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Editor.Tests/Engine.Editor.Tests.csproj --no-restore --nologo -v minimal`；Engine.Editor.Tests 30 条通过）
- Smoke: `pass`（默认新增对象包含 Transform + MeshRenderer；Transform-only object 可打开、选择、保存，且不自动补 MeshRenderer）
- Boundary: `pass`（仅改 `src/Engine.Editor/**`、`tests/Engine.Editor.Tests/**` 与任务指定边界/归档文档；未新增 App/Render/Platform/Asset/Editor.App 依赖）
- Perf: `pass`（component edits 仅在显式 session 命令执行；无逐帧 IO、无 GUI 依赖渗入）

## Risks

- `low`：GUI Inspector 仍需 `TASK-EAPP-008` 迁移到 component groups，旧 GUI 测试夹具仍会在该卡修复。
