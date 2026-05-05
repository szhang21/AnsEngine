# Archive: TASK-EAPP-011 M21 Scene View preview foundation

## Status

- Status: `Review`
- Completion: `95`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- Added `EditorScenePreviewHost` and `EditorScenePreviewSnapshot` inside `Engine.Editor.App`.
- Editor.App now refreshes preview after open, apply/edit, save/save-as and selection changes.
- Scene View renderer draws a nonblank edit-time preview surface from preview snapshot state.
- Boundary updated to allow Editor.App preview composition dependencies on Scene/Render/Asset while keeping Editor headless and App independent.

## FilesChanged

- `src/Engine.Editor.App/Engine.Editor.App.csproj`
- `src/Engine.Editor.App/EditorAppController.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `src/Engine.Editor.App/EditorGuiSnapshot.cs`
- `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
- `src/Engine.Editor.App/EditorScenePreviewHost.cs`
- `src/Engine.Editor.App/EditorScenePreviewSnapshot.cs`
- `tests/Engine.Editor.App.Tests/EditorAppBoundaryTests.cs`
- `tests/Engine.Editor.App.Tests/EditorAppControllerTests.cs`
- `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-011.md`
- `.ai-workflow/archive/2026-05/TASK-EAPP-011.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL warnings only)
- Test: pass (`dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal`; 39 passed)
- Render/Scene Tests: pass (`Engine.Render.Tests` 18 passed; `Engine.Scene.Tests` 57 passed)
- Smoke: pass (open sample scene produces nonblank SceneView preview; apply/save refresh preview; no ApplicationHost/script/physics/play-mode references)
- Boundary: pass (Editor.App dependencies match approved Scene/Render/Asset preview boundary; Engine.Editor remains headless; Engine.App still not referenced)
- Perf: pass (preview refresh is operation-triggered, not per-frame scene reload or runtime app duplication)

## Risk

- low: Scene View currently renders a minimal nonblank preview surface from render submission state, not picking/gizmo/play-mode; richer viewport interaction remains future scope.
