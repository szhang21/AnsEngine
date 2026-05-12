# Archive: TASK-SCRIPT-004 M22 Scripting runtime abstraction alignment

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `Engine.Scripting` now references `Engine.Runtime.Abstractions`.
- `IScriptSelfObject` aligns to `IRuntimeObject`, and `Self.Transform` returns `IRuntimeTransformComponent`.
- `IScriptTransformComponent` remains as a compatibility interface over `IRuntimeTransformComponent`.
- App's scripting bridge wraps Scene handles as runtime self objects without changing script lifecycle or update order.

## FilesChanged

- `src/Engine.Scripting/Engine.Scripting.csproj`
- `src/Engine.Scripting/IScriptSelfObject.cs`
- `src/Engine.Scripting/IScriptTransformComponent.cs`
- `tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj`
- `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
- `src/Engine.App/Engine.App.csproj`
- `src/Engine.App/ApplicationBootstrap.cs`
- `.ai-workflow/boundaries/engine-scripting.md`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-script-004.md`
- `.ai-workflow/archive/2026-05/TASK-SCRIPT-004.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
- Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed, including `Engine.Scripting.Tests` 18/18 and `Engine.App.Tests` 27/27)
- Focused Test: pass (`dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal`; 18/18)
- App Bridge Test: pass (`dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`; 27/27)
- Smoke: pass (`context.Self.Transform` scripts continue to work; ScriptRuntime binding/update lifecycle and App update order unchanged)
- Boundary: pass (`Engine.Scripting` still does not reference `Engine.Scene`; App owns the Scene/Scripting bridge)
- Perf: pass (adapter adds no per-frame search or script loading work)

## Risk

- low: Scripting now exposes shared runtime abstraction types, but cross-object access remains absent and Scene concrete types remain outside Scripting.
