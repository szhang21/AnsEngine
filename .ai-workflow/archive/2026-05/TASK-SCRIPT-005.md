# TASK-SCRIPT-005 Archive Snapshot

## Summary

- Status: `Done`
- Completion: `100`
- HumanSignoff: `pass`
- PlanRef: `PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE`
- MilestoneRef: `M24.3`
- PrimaryModule: `Engine.Scripting`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scripting.md`
- DependsOn: `TASK-RABS-002`, `TASK-SCENE-024` completed to Review/archive-prepared state.

## Work Completed

- Added runtime update component binding output from `ScriptRuntime`.
- Added `ScriptUpdateComponentBindingDescription` and `ScriptUpdateComponentBindingResult`.
- Added Scripting-owned `RotateSelfScript` and `MoveOnInputScript` implementations that implement `IRuntimeUpdateComponent`.
- Built-in scripts update Transform via `RuntimeUpdateContext.Owner.GetComponent<IRuntimeTransformComponent>()`.
- Missing Transform returns deterministic `RuntimeUpdateFailure`.
- Existing `ScriptRuntime.Update(...)` remains as compatibility path while exposing `BoundUpdateComponents` for future `Engine.Runtime` traversal.
- Updated Scripting tests for runtime update context, built-in script behavior, missing Transform failure, binding order, and no Scene/App/Platform dependency.
- Updated Scripting boundary change log.

## Files Changed

- `src/Engine.Scripting/ScriptRuntime.cs`
- `src/Engine.Scripting/ScriptUpdateComponentBindingDescription.cs`
- `src/Engine.Scripting/ScriptUpdateComponentBindingResult.cs`
- `src/Engine.Scripting/RotateSelfScript.cs`
- `src/Engine.Scripting/MoveOnInputScript.cs`
- `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
- `.ai-workflow/boundaries/engine-scripting.md`
- `.ai-workflow/tasks/task-script-005.md`
- `.ai-workflow/archive/2026-05/TASK-SCRIPT-005.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## Validation Evidence

- Build: pass, `dotnet build AnsEngine.sln --nologo -v minimal`; 0 errors, existing `net7.0` EOL and Windows Kits `LIB` path warnings observed.
- Test: pass, `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal`; 22 passed, 0 failed.
- Smoke: pass, fake `IRuntimeObject` + `IRuntimeTransformComponent` tests prove built-in scripts update through owner lookup.
- Perf: pass, no per-frame JSON parsing, assembly scan, global scene query, or Scripting-owned scheduler was added.
- Dependency: pass, `Engine.Scripting` does not reference `Engine.Scene`, `Engine.App`, or `Engine.Platform`.

## Quality

- CodeQuality: NoNewHighRisk=`true`; MustFixCount=`0`; MustFixDisposition=`none`.
- DesignQuality: DQ-1=`pass`; DQ-2=`pass`; DQ-3=`pass`; DQ-4=`pass`.
- ModuleAttributionCheck: `pass`

## Risk

- Medium. The compatibility path remains until `Engine.Runtime` and `Engine.App` are migrated, but built-in script update behavior now has the runtime component path needed by downstream M24 cards.
