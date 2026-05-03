# Archive: TASK-PLAT-003 M18.F1 Native WASD input polling for runtime app

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ClosedAt: `2026-05-03 18:09`

## Summary

- Added Platform-owned `IKeyboardStateProvider` and `NativeWindowInputService`.
- `NullWindowService` now maps native OpenTK `KeyboardState` W/A/S/D into `EngineKey` without exposing OpenTK types outside Platform.
- `RuntimeBootstrap.Build()` now uses native input service for native window mode and keeps `NullInputService` for headless mode.
- Added direct bool-state `InputSnapshot.FromKeyStates(...)` construction to avoid per-frame collection allocation on native polling.
- Added Platform/App tests for native input mapping and native/headless input service composition.

## FilesChanged

- `src/Engine.Platform/PlatformContracts.cs`
- `src/Engine.Platform/PlatformPlaceholders.cs`
- `src/Engine.Platform/NativeWindowInputService.cs`
- `tests/Engine.Platform.Tests/InputSnapshotTests.cs`
- `src/Engine.App/ApplicationBootstrap.cs`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/boundaries/engine-platform.md`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-plat-003.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/2026-05/TASK-PLAT-003.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build: pass
  - Command: `dotnet build AnsEngine.sln --nologo -v minimal`
  - Notes: completed successfully with existing `net7.0` EOL warnings.
- Test: pass
  - Command: `dotnet test tests/Engine.Platform.Tests/Engine.Platform.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 10/10 tests passed.
  - Command: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 18/18 tests passed.
- Smoke: pass
  - Command: `ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=0.05 dotnet run --project src/Engine.App/Engine.App.csproj --no-restore --nologo`
  - Result: exited 0 with default sample scene.
  - Command: `ANS_ENGINE_AUTO_EXIT_SECONDS=0.05 dotnet run --project src/Engine.App/Engine.App.csproj --no-restore --nologo`
  - Result: native window path exited 0 with default sample scene.
  - Manual: real W/A/S/D keyboard response remains pending Human review/signoff.
- Boundary: pass
  - OpenTK `Keys` / `KeyboardState` remain sealed inside `Engine.Platform`.
  - `Engine.App` composes native/headless input services but does not directly read native keyboard state.
  - `Engine.Scripting`, `Engine.Scene`, and `Engine.Render` boundaries are unchanged.
- Perf: pass
  - Native input polling performs one direct W/A/S/D state read per key and constructs a small readonly `InputSnapshot`.
  - No action mapping, mouse/gamepad path, physics, or gameplay side path introduced.

## Boundary

- BoundaryContractPath: `.ai-workflow/boundaries/engine-platform.md`
- BoundarySync: pass
- ModuleAttributionCheck: pass
- Notes:
  - Platform and App boundary change logs record native input service and bootstrap composition changes.
  - New source file is covered by boundary sync.

## Risk

- Risk: `low`
- Notes: residual risk is low after human manual verification/signoff; automated coverage and native path composition are both archived.
