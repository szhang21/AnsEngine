# Archive: TASK-SCRIPT-002 M17.F1 Script SelfObject/Transform decoupling convergence

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ClosedAt: `2026-05-03 13:41`

## Summary

- `Engine.Scripting` script access now exposes `ScriptContext.Self.Transform` through `IScriptSelfObject` and `IScriptTransformComponent`.
- Removed the `Engine.Scripting -> Engine.Scene` project reference; Scripting now consumes only `Engine.Contracts` transform contracts for this access surface.
- App now wraps `SceneScriptObjectHandle` with `SceneScriptSelfObject` / `SceneScriptTransformComponent`, keeping Scene handle knowledge in the App bridge layer.
- `RotateSelf` and Scripting tests now use `context.Self.Transform` while preserving bind, initialize, update, unknown script, and script exception behavior.

## FilesChanged

- `src/Engine.Scripting/Engine.Scripting.csproj`
- `src/Engine.Scripting/IScriptSelfObject.cs`
- `src/Engine.Scripting/IScriptTransformComponent.cs`
- `src/Engine.Scripting/IScriptSelfTransform.cs`
- `src/Engine.Scripting/ScriptBindingDescription.cs`
- `src/Engine.Scripting/ScriptContext.cs`
- `src/Engine.Scripting/ScriptRuntime.cs`
- `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
- `src/Engine.App/ApplicationBootstrap.cs`
- `.ai-workflow/boundaries/engine-scripting.md`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-script-002.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build: pass
  - Command: `dotnet build AnsEngine.sln --nologo -v minimal`
  - Notes: completed successfully with existing `net7.0` EOL warnings.
- Test: pass
  - Command: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 11/11 tests passed.
  - Command: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 12/12 tests passed.
- Smoke: pass
  - Command: `ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=0.05 dotnet run --project src/Engine.App/Engine.App.csproj --no-restore --nologo`
  - Result: exited 0; App tests verify `RotateSelf` updates before render through `Self.Transform`.
- Boundary: pass
  - `Engine.Scripting.csproj` no longer references `Engine.Scene`.
  - `rg -n -F "Engine.Scene" src/Engine.Scripting tests/Engine.Scripting.Tests -g '*.cs' -g '*.csproj'` only finds Scripting test boundary assertions.
  - `IScriptSelfTransform` / `SelfTransform` no longer remain in Scripting/App source.
- Perf: pass
  - No per-frame object query, reflection component dispatch, extra script binding polling, or render side effect introduced.

## Boundary

- BoundaryContractPath: `.ai-workflow/boundaries/engine-scripting.md`
- BoundarySync: pass
- ModuleAttributionCheck: pass
- Notes:
  - `Engine.Scripting` boundary now forbids direct `Engine.Scene` dependency.
  - `Engine.App` boundary records the bridge adapter shape that connects Scene handles to Scripting abstractions.

## Risk

- Risk: `low`
- Notes: residual risk is limited to future script component expansion; broader component query APIs remain intentionally out of scope.
