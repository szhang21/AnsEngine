# Archive: TASK-SCENE-022 M22 Transform contract alignment

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneTransformComponent` now implements `IRuntimeTransformComponent`.
- Added contract-shaped `LocalTransform` and `SetLocalTransform(SceneTransform)` while preserving existing local position/rotation/scale API.
- Updated script self-transform and `RuntimeScene.TrySetObjectTransform` paths to use the same Transform instance through the new setter.
- Confirmed Transform remains the core `SceneRuntimeObject.Transform` field and is not registered in the generic component collection.

## FilesChanged

- `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
- `src/Engine.Scene/Runtime/SceneScriptObjectHandle.cs`
- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-022.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-022.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
- Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed, including `Engine.Scene.Tests` 61/61)
- Focused Test: pass (`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`; 61/61)
- Smoke: pass (missing Transform failure semantics, script self-transform, `TrySetObjectTransform`, Transform-only object, snapshot and render behavior unchanged)
- Boundary: pass (Transform aligns to Runtime.Abstractions while remaining core field; no new forbidden Scene dependencies)
- Perf: pass (interface implementation adds no extra allocation or render path work)

## Risk

- low: Transform ownership intentionally stays unchanged. Future world transform, hierarchy, or traversal features remain out of scope.
