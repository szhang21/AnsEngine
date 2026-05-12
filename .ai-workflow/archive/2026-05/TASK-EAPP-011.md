# Archive: TASK-EAPP-011 M21 Scene View preview foundation

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- Repaired the acceptance-dispute defect where Scene View still drew a fixed placeholder triangle.
- `EditorScenePreviewHost.Refresh` now projects triangles from real `SceneRenderSubmission` mesh batches into the preview snapshot.
- `EditorGuiRenderer.DrawScenePreview` now draws those projected triangles, preserving material color, instead of synthesizing a single placeholder shape.
- Added regression coverage that the default sample scene produces 2 preview batches and 24 projected triangles from the real cube meshes.

## FilesChanged

- `src/Engine.Editor.App/EditorScenePreviewHost.cs`
- `src/Engine.Editor.App/EditorScenePreviewSnapshot.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `tests/Engine.Editor.App.Tests/EditorAppControllerTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-011.md`
- `.ai-workflow/archive/2026-05/TASK-EAPP-011.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL warnings only)
- Test: pass (`dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal`; 40 passed)
- Smoke: pass (default sample scene preview snapshot contains 2 real mesh batches and 24 projected triangles; Apply/Save preview refresh remains covered; no ApplicationHost/script/physics/play-mode references added)
- Boundary: pass (changes stayed in `Engine.Editor.App`, Editor.App tests and workflow metadata; `Engine.Editor.App` still does not reference `Engine.App`)
- Perf: pass (projection happens only during operation-triggered preview refresh, not via per-frame scene reload or runtime app duplication)

## Risk

- low: Scene View is still an edit-time preview only. It now draws projected mesh triangles from the render submission, but picking, gizmo, Play Mode and richer viewport controls remain future scope.
