# Archive: TASK-EAPP-009 M21 Unity-like Editor shell and theme baseline

## Status

- Status: `Review`
- Completion: `95`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- Renamed the central layout contract from `MainWorkspace*` to `SceneView*`.
- Adjusted the fixed docked shell to a compact 56px toolbar with Hierarchy, Scene View, Inspector and Status regions.
- Added a testable dark tool theme snapshot and applied it in `EditorGuiRenderer`.
- Removed the old viewport placeholder text without adding component authoring or preview/render dependencies.

## FilesChanged

- `src/Engine.Editor.App/EditorGuiSnapshot.cs`
- `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-009.md`
- `.ai-workflow/archive/2026-05/TASK-EAPP-009.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL warnings only)
- Test: pass (`dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal`; 33 passed)
- Smoke: pass (old `Viewport is intentionally reserved for a later milestone.` text absent; central dock now renders `Scene View` container)
- Boundary: pass (`Engine.Editor.App.csproj` still references only Editor, SceneData, Contracts, Platform; no Render/Asset/Scene dependency added)
- Perf: pass (no file IO, preview/render hookup, asset loading, or repeated scene work introduced)

## Risk

- low: native visual polish still depends on later interactive smoke, but layout/theme semantics and old placeholder removal are covered by snapshot tests and source inspection.
