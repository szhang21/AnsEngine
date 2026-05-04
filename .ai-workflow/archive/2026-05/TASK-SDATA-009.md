# Archive: TASK-SDATA-009 M19 SceneData RigidBody and BoxCollider component schema

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ClosedAt: `2026-05-04 21:26`

## Summary

- Added `RigidBody` and `BoxCollider` component file schema.
- Added normalized SceneData descriptions for rigid body and box collider components.
- Added validation for `bodyType`, `mass`, positive finite box `size`, finite `center`, and default center.
- Pinned normalized mass semantics: Static mass becomes `0`; Dynamic missing mass becomes `1`.
- Added valid load, invalid schema, round-trip, smoke, and boundary coverage without referencing `Engine.Physics`.

## FilesChanged

- `src/Engine.SceneData/FileModel/SceneFileComponentDefinition.cs`
- `src/Engine.SceneData/Descriptions/SceneComponentDescription.cs`
- `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
- `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
- `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
- `.ai-workflow/boundaries/engine-scenedata.md`
- `.ai-workflow/tasks/task-sdata-009.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/2026-05/TASK-SDATA-009.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build: pass
  - Command: `dotnet build AnsEngine.sln --nologo -v minimal`
  - Notes: completed successfully with existing `net7.0` EOL warnings.
- Test: pass
  - Command: `dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 44/44 tests passed.
- Smoke: pass
  - JSON fixture loads into `SceneDescription` with normalized `RigidBody` and `BoxCollider` components.
- Boundary: pass
  - `Engine.SceneData` continues to avoid `Engine.Physics`.
- Perf: pass
  - Validation runs only on explicit load/save/reload paths.
  - No runtime physics, AABB/query, per-frame callback, or cross-module bridge introduced.

## Boundary

- BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
- BoundarySync: pass
- ModuleAttributionCheck: pass

## Risk

- Risk: `low`
- Notes: this is schema-only; production SceneData-to-Physics bridge remains out of scope for M19.
