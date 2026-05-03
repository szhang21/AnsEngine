# Archive: TASK-PLAT-002 M18 Platform key-state input snapshot foundation

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ClosedAt: `2026-05-03 14:27`

## Summary

- Added `EngineKey` with the M18 W/A/S/D key set.
- Extended `InputSnapshot` into a readonly key-state snapshot with `Empty`, `FromKeys(...)`, `AnyInputDetected`, and `IsKeyDown(...)`.
- Updated `NullInputService` to return empty input.
- Added `Engine.Platform.Tests` coverage for empty, single-key, multi-key, null input, constructor compatibility, and boundary checks.

## FilesChanged

- `src/Engine.Platform/PlatformContracts.cs`
- `src/Engine.Platform/PlatformPlaceholders.cs`
- `tests/Engine.Platform.Tests/Engine.Platform.Tests.csproj`
- `tests/Engine.Platform.Tests/InputSnapshotTests.cs`
- `tests/Engine.Platform.Tests/PlatformBoundaryTests.cs`
- `.ai-workflow/boundaries/engine-platform.md`
- `.ai-workflow/tasks/task-plat-002.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/2026-05/TASK-PLAT-002.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build: pass
  - Command: `dotnet build AnsEngine.sln --nologo -v minimal`
  - Notes: completed successfully with existing `net7.0` EOL warnings.
- Test: pass
  - Command: `dotnet restore tests/Engine.Platform.Tests/Engine.Platform.Tests.csproj --nologo -v minimal`
  - Command: `dotnet test tests/Engine.Platform.Tests/Engine.Platform.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 7/7 tests passed.
- Smoke: pass
  - `NullInputService` returns `InputSnapshot.Empty`.
  - `InputSnapshot.FromKeys(...)` independently expresses W/A/S/D and multi-key combinations.
- Boundary: pass
  - `src/Engine.Platform` has no `Engine.Scene`, `Engine.Scripting`, `Engine.App`, or `Engine.Render` references.
- Perf: pass
  - Key states are stored as a small value snapshot without per-frame mutable collection copying.

## Boundary

- BoundaryContractPath: `.ai-workflow/boundaries/engine-platform.md`
- BoundarySync: pass
- ModuleAttributionCheck: pass
- Notes:
  - Boundary change log records the M18 key-state input snapshot foundation.
  - No OpenTK key enum is exposed outside `Engine.Platform`.

## Risk

- Risk: `low`
- Notes: native OpenTK key polling remains intentionally out of scope; the stable snapshot contract is ready for App conversion.
