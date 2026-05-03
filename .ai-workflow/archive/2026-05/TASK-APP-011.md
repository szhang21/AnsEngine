# Archive: TASK-APP-011 M17 App scripting runtime integration and RotateSelf sample

## Status

- Status: `Review`
- Completion: `95`
- HumanSignoff: `pending`
- ClosedAt: `2026-05-02 17:41`

## Summary

- `Engine.App` now composes `ScriptRegistry` / `ScriptRuntime` and registers built-in `RotateSelf`.
- `ApplicationHost.Run()` now binds scene Script components after scene runtime initialization and runs script update after scene base update but before render.
- `RotateSelf` reads numeric `speedRadiansPerSecond` and applies Y-axis self rotation through the narrow Scene script handle.
- App tests now cover valid `RotateSelf`, unknown script id clean failure, script update exception clean failure, shutdown/dispose stability, and removal of M15 default Scene update rotation.

## FilesChanged

- `src/Engine.App/ApplicationBootstrap.cs`
- `src/Engine.App/Engine.App.csproj`
- `src/Engine.App/SceneRuntimeContracts.cs`
- `tests/Engine.App.Tests/Engine.App.Tests.csproj`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-app-011.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass
  - Command: `dotnet build AnsEngine.sln --nologo -v minimal`
  - Notes: completed successfully with existing `net7.0` EOL and local `LIB` path warnings.
- Test: pass
  - Command: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 12/12 tests passed.
- Smoke: pass
  - Command: `ANS_ENGINE_USE_NATIVE_WINDOW=false; ANS_ENGINE_AUTO_EXIT_SECONDS=0.05; dotnet run --project src/Engine.App/Engine.App.csproj --no-restore --nologo`
  - Result: default sample scene exited 0; `RotateSelf` render-before-update behavior is covered by App test.
- Perf: pass
  - No per-frame assembly loading, source compilation, hot reload polling, repeated script binding, or render side effect introduced.

## Boundary

- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- BoundarySync: pass
- ModuleAttributionCheck: pass
- Notes:
  - `Engine.App` boundary now explicitly allows `Engine.Scripting`.
  - Boundary change log records the `TASK-APP-011` scripting runtime integration.

## Risk

- Risk: `low`
- Notes: main residual risk is that M17 final QA still needs to validate full cross-module editor preserve and scripting closeout in `TASK-QA-018`.
