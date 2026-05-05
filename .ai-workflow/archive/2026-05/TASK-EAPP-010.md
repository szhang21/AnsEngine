# Archive: TASK-EAPP-010 M21 Inspector Script and Physics component stack integration

## Status

- Status: `Review`
- Completion: `95`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- Added Inspector snapshots for Scripts, RigidBody, BoxCollider and PhysicsParticipation groups.
- Added `EditorAppController` wrappers for Script/RigidBody/BoxCollider session component APIs.
- Extended Inspector input state and renderer for Script JSON, RigidBody body type/mass and BoxCollider size/center Apply plus add/remove.
- Added tests for group display, apply/update, JSON parse failure rollback and physics-ready state.

## FilesChanged

- `src/Engine.Editor.App/EditorAppController.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
- `src/Engine.Editor.App/EditorInspectorInputState.cs`
- `src/Engine.Editor.App/EditorInspectorSnapshot.cs`
- `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
- `tests/Engine.Editor.App.Tests/EditorInspectorInputStateTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-010.md`
- `.ai-workflow/archive/2026-05/TASK-EAPP-010.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL warnings only)
- Test: pass (`dotnet test tests/Engine.Editor.App.Tests/Engine.Editor.App.Tests.csproj --no-restore --nologo -v minimal`; 37 passed)
- Smoke: pass (Inspector can edit Script/RigidBody/BoxCollider through controller/session; invalid Script JSON updates LastError and leaves document unchanged)
- Boundary: pass (`Engine.Editor.App.csproj` still references only Editor, SceneData, Contracts, Platform; no Render/Asset/Scene dependency added)
- Perf: pass (Script JSON parse only on Apply; no per-frame scene/asset/render work)

## Risk

- low: v1 edits the first script component and does not provide reorder/typed property grid; this matches M21 scope.
