# Archive: TASK-APP-020 M20 App Physics production bridge

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ClosedAt: `2026-05-05 00:08`

## Summary

- Added App-owned `SceneDescription -> PhysicsWorldDefinition` production bridge.
- Mapped only objects with Transform + RigidBody + BoxCollider into Physics.
- Mapped SceneData Static/Dynamic body types to Physics Static/Dynamic body types.
- Initialized `PhysicsWorld` during `ApplicationHost.Run()` startup after SceneData load.
- Updated bundled default scene JSON with existing RigidBody / BoxCollider schema for real smoke coverage.

## FilesChanged

- `src/Engine.App/Engine.App.csproj`
- `src/Engine.App/ApplicationBootstrap.cs`
- `src/Engine.App/ScenePhysicsWorldDefinitionBridge.cs`
- `src/Engine.App/SampleScenes/default.scene.json`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-app-020.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/2026-05/TASK-APP-020.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build: pass
  - Command: `dotnet build AnsEngine.sln --nologo -v minimal`
  - Notes: completed successfully with existing `net7.0` EOL warnings.
- Test: pass
  - Command: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 24/24 tests passed.
- Smoke: pass
  - Command: `ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.App/Engine.App.csproj --nologo`
  - Result: exit 0.
  - Real default scene JSON loads through SceneData and initializes `PhysicsWorld` via App bridge.
- Boundary: pass
  - App is the only production bridge location changed.
  - `Engine.Physics` remains unchanged and does not depend on Engine modules.
- Perf: pass
  - Bridge runs once during initialization and does not add per-frame SceneData mapping.

## CodeQuality

- NoNewHighRisk: `true`
- MustFixCount: `0`
- MustFixDisposition: `none`

## DesignQuality

- DQ-1 SRP: `pass`
- DQ-2 DIP: `pass`
- DQ-3 OCP-oriented extension points: `pass`
- DQ-4 Open/closed assessment: `pass`

## Boundary

- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- BoundarySync: pass
- ModuleAttributionCheck: pass

## ArchiveReadiness

- `TASK-APP-020` has passed Human review/signoff and is ready for final archive closeout.
- `TASK-PHYS-003` may now add Physics kinematic resolve without moving App bridge logic into Physics.

## Risk

- Risk: `low`
- Notes: No runtime physics order, writeback, gravity, or solver was added in this card.
