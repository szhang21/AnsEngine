# Archive: TASK-SCENE-021 M22 Scene runtime object implements abstractions

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- Added `Engine.Scene -> Engine.Runtime.Abstractions` dependency for M22 runtime object abstraction alignment.
- `SceneRuntimeObject` now implements `IRuntimeObject`.
- Added an internal non-core runtime component collection with `GetComponent<T>()` and `HasComponent<T>()` typed lookup.
- Preserved existing core Transform/MeshRenderer fields, NodeId identity, snapshots and render frame behavior.
- Updated Scene boundary contract to allow Runtime.Abstractions while continuing to forbid Scripting/Physics/App/Render dependencies.

## FilesChanged

- `src/Engine.Scene/Engine.Scene.csproj`
- `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
- `tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj`
- `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-021.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-021.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
- Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed, including `Engine.Scene.Tests` 59/59)
- Focused Test: pass (`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`; 59/59)
- Smoke: pass (empty component collection stable; missing typed lookup returns null; `HasComponent<T>()` matches lookup; object identity/snapshot/render semantics unchanged)
- Boundary: pass (`Engine.Scene` references Runtime.Abstractions and still has no Scripting/Physics/App/Render dependency)
- Perf: pass (typed lookup scans an internal immutable small component list and is not on render path)

## Risk

- low: Transform and MeshRenderer remain on their existing core fields by design; later M22 cards own their alignment/migration.
