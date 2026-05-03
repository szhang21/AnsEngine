# TASK-SCRIPT-001 归档快照

- TaskId: `TASK-SCRIPT-001`
- Title: `M17 Engine.Scripting module and script lifecycle foundation`
- Priority: `P0`
- PrimaryModule: `Engine.Scripting`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scripting.md`
- Owner: `Exec-Scripting`
- ClosedAt: `2026-05-02 14:04`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- Added `Engine.Scripting` and `Engine.Scripting.Tests` projects to the solution.
- Implemented built-in script registry, explicit diagnostic failures, script context/property model, and bind/initialize/update lifecycle.
- Added narrow `IScriptSelfTransform` so scripts can mutate only the supplied self Transform handle.
- Kept external DLL loading, source compilation, hot reload, sandboxing, JSON/file IO, and all-scene object access out of scope.

## FilesChanged

- `src/Engine.Scripting/**`
- `tests/Engine.Scripting.Tests/**`
- `AnsEngine.sln`
- `.ai-workflow/boundaries/engine-scripting.md`
- `.ai-workflow/tasks/task-script-001.md`
- `.ai-workflow/archive/2026-05/TASK-SCRIPT-001.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal`；Engine.Scripting.Tests 10 条通过）
- Smoke: `pass`（registry create、Initialize once、Update per frame、script exception diagnostic failure、自身 Transform handle mutation均有覆盖）
- Boundary: `pass`（未新增 forbidden dependency；`Engine.Scene.csproj` 不引用 `Engine.Scripting`；未引入外部程序集扫描/源码编译/JSON/file IO）
- Perf: `pass`（runtime update 仅遍历已绑定实例；无逐帧程序集扫描、源码编译、全场景查询或 JSON 解析）

## Risks

- `low`：Scene runtime bridge 尚未接入真实 runtime object Transform，留给 `TASK-SCENE-019`。
