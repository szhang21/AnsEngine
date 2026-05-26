# Archive: TASK-PHYS-004 M23 Physics mutating kinematic move API

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- Kept `ResolveKinematicMove(...)` non-mutating.
- Added `ApplyKinematicMove(...)` as the explicit mutating kinematic move API.
- Successful apply replaces the dynamic body snapshot so Transform and AABB match the resolved transform.
- Static body, missing id, blank id, and malformed transform diagnostics reuse existing resolve semantics.

## FilesChanged

- `src/Engine.Physics/PhysicsWorld.cs`
- `tests/Engine.Physics.Tests/PhysicsFoundationTests.cs`
- `.ai-workflow/boundaries/engine-physics.md`
- `.ai-workflow/tasks/task-phys-004.md`
- `.ai-workflow/archive/2026-05/TASK-PHYS-004.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
- Test: pass (`dotnet test tests/Engine.Physics.Tests/Engine.Physics.Tests.csproj --no-restore --nologo -v minimal`; 20/20)
- Smoke: pass (two-step apply/query path proves the second resolve starts from updated PhysicsWorld body state)
- Perf: pass (no solver/CCD/gravity/full world rebuild introduced; apply replaces one body entry and recalculates one AABB)
- Boundary: pass (`Engine.Physics` still has no Engine module dependencies)

## Risk

- low: `ApplyKinematicMove(...)` mutates only the target dynamic body entry; existing `ResolveKinematicMove(...)` remains the pure query path.
