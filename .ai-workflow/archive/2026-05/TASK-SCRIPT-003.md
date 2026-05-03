# Archive: TASK-SCRIPT-003 M18 Scripting input context and property helper

## Status

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- ClosedAt: `2026-05-03 14:30`

## Summary

- Added scripting-owned `ScriptKey` and `ScriptInputSnapshot` for W/A/S/D frame input.
- Added `ScriptContext.Input` and `WithFrame(...)`; `ScriptRuntime.Update(...)` can now pass one frame input snapshot to every bound script.
- Added `ScriptPropertyReader` required number/boolean/string helpers with stable missing, type mismatch, and non-finite numeric failure behavior.
- Added Scripting tests covering input propagation, shared frame input, helper behavior, invalid property failure, and RotateSelf helper usage in the scripting test fixture.

## FilesChanged

- `src/Engine.Scripting/ScriptKey.cs`
- `src/Engine.Scripting/ScriptInputSnapshot.cs`
- `src/Engine.Scripting/ScriptPropertyReader.cs`
- `src/Engine.Scripting/ScriptContext.cs`
- `src/Engine.Scripting/ScriptRuntime.cs`
- `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
- `.ai-workflow/boundaries/engine-scripting.md`
- `.ai-workflow/tasks/task-script-003.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/2026-05/TASK-SCRIPT-003.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build: pass
  - Command: `dotnet build AnsEngine.sln --nologo -v minimal`
  - Notes: completed successfully with existing `net7.0` EOL warnings.
- Test: pass
  - Command: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal`
  - Result: 17/17 tests passed.
- Smoke: pass
  - `ScriptRuntime.Update(..., ScriptInputSnapshot)` passes the same frame input to every bound script.
  - Empty input compatibility path still succeeds.
  - Invalid property values fail deterministically before binding or through stable helper diagnostics.
- Boundary: pass
  - `Engine.Scripting` has no `Engine.Platform` project/source dependency.
  - `src/Engine.Scripting` has no actual `Engine.Scene` references.
- Perf: pass
  - No platform conversion, reflection property scanning, JSON parsing, or global mutable input state introduced.

## Boundary

- BoundaryContractPath: `.ai-workflow/boundaries/engine-scripting.md`
- BoundarySync: pass
- ModuleAttributionCheck: pass
- Notes:
  - Boundary change log records scripting-owned input and property helper additions.
  - Platform input conversion remains out of Scripting and is reserved for App.

## Risk

- Risk: `low`
- Notes: App built-in `RotateSelf` still uses its existing direct property read until `TASK-APP-012`, because `TASK-SCRIPT-003` was restricted to Scripting paths.
