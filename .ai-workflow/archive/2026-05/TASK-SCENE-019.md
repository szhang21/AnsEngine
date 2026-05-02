# TASK-SCENE-019 归档快照

- TaskId: `TASK-SCENE-019`
- Title: `M17 Scene script access bridge`
- Priority: `P1`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-05-02 15:00`
- Status: `Review`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- Added narrow `SceneGraphService.BindScriptObject(...)` bridge for one bound runtime object.
- Added `SceneScriptObjectHandle` exposing only object id/name and self local transform get/set.
- Transform-only objects can host script bridge updates and remain excluded from render items.
- Removed M15 default rotation smoke behavior from `RuntimeScene.Update`.
- Kept `Engine.Scene` free of any `Engine.Scripting` dependency.

## FilesChanged

- `src/Engine.Scene/SceneGraphService.cs`
- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `src/Engine.Scene/Runtime/SceneScriptObjectHandle.cs`
- `src/Engine.Scene/Runtime/SceneScriptObjectBindFailure.cs`
- `src/Engine.Scene/Runtime/SceneScriptObjectBindFailureKind.cs`
- `src/Engine.Scene/Runtime/SceneScriptObjectBindResult.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-019.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-019.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Engine.Scene.Tests 52 条通过）
- Regression: `pass`（Engine.Scripting.Tests 10 条通过）
- Smoke: `pass`（self-transform bridge 修改可被 snapshot/render frame 观察；Transform-only bridge 修改合法但不渲染；默认 update 不再旋转）
- Boundary: `pass`（`Engine.Scene` 未引用 `Engine.Scripting`、Render、App、Editor、Platform；bridge 不暴露 runtime collection）
- Perf: `pass`（无逐帧 JSON、任意对象查询或 render side effect）

## Risks

- `low`：App 尚未把 Script components 绑定到 scripting runtime，留给 `TASK-APP-011`。
