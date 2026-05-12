# Archive: TASK-SCENE-023 M22 MeshRenderer component container migration

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneMeshRendererComponent` now implements `IRuntimeComponent`.
- `SceneRuntimeObject` registers MeshRenderer in its runtime component container.
- `SceneRuntimeObject.MeshRenderer` remains as a compatibility accessor backed by `GetComponent<SceneMeshRendererComponent>()`.
- Render frame output stays driven by `Transform + MeshRenderer`; Render and SceneData do not depend on Runtime.Abstractions.

## FilesChanged

- `src/Engine.Scene/Runtime/SceneMeshRendererComponent.cs`
- `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-023.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-023.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
- Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed, including `Engine.Scene.Tests` 62/62)
- Focused Test: pass (`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`; 62/62)
- Smoke: pass (MeshRenderer typed lookup, Transform-only no render item, multi-object render order, and M21 render output compatibility covered by scene tests)
- Boundary: pass (`Engine.Render` and `Engine.SceneData` do not reference `Engine.Runtime.Abstractions`)
- Perf: pass (MeshRenderer is registered once at object construction and lookup is bounded to the internal component collection)

## Risk

- low: MeshRenderer lookup now flows through the component container, while render behavior and public compatibility access remain unchanged.
