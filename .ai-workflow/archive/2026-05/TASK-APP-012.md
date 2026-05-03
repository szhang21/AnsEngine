# Archive: TASK-APP-012 M18 App input-to-scripting integration and MoveOnInput

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ClosedAt: `2026-05-03 14:53`

## Summary

- App now converts Platform `InputSnapshot` W/A/S/D key state into Scripting `ScriptInputSnapshot`.
- `ApplicationHost.Run()` passes script input to `ScriptRuntime.Update(...)` after Scene update and before render.
- Built-in `MoveOnInput` is registered and moves only self Transform.Position on the world XZ plane, with normalized diagonal movement.
- Default sample scene declares `MoveOnInput`.
- App project sets `UseAppHost=false` to avoid macOS apphost code signing failures in the current validation environment.

## FilesChanged

- `src/Engine.App/ApplicationBootstrap.cs`
- `src/Engine.App/Engine.App.csproj`
- `src/Engine.App/SampleScenes/default.scene.json`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-app-012.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/2026-05/TASK-APP-012.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build: pass
  - Command: `dotnet build AnsEngine.sln --nologo -v minimal`
  - Notes: completed successfully with existing `net7.0` EOL warnings.
- Test: pass
  - Command: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 16/16 tests passed.
- Smoke: pass
  - Command: `ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=0.05 dotnet run --project src/Engine.App/Engine.App.csproj --no-restore --nologo`
  - Result: exited 0 with default sample scene.
- Boundary: pass
  - `Engine.App` remains the Platform input to Scripting input conversion layer.
  - `Engine.Scripting` does not reference `Engine.Platform`.
  - `Engine.Scene` does not reference `Engine.Scripting`.
  - `Engine.Render` does not mention input, script runtime, or `MoveOnInput`.
- Perf: pass
  - One Platform-to-Scripting input conversion per frame.
  - No physics/collision simulation, Platform type leakage, or render side effect introduced.

## Boundary

- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- BoundarySync: pass
- ModuleAttributionCheck: pass
- Notes:
  - Boundary change log records M18 App input conversion and `MoveOnInput`.
  - `UseAppHost=false` is documented as validation-environment build stabilization.

## Risk

- Risk: `low`
- Notes: native key polling remains dependent on future Platform work; M18 MVP path is validated through Platform snapshots and App tests.
