# TASK-SCENE-015 归档快照

- TaskId: `TASK-SCENE-015`
- Title: `M15 Runtime update context 与统计地基`
- Priority: `P0`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-05-01 20:16`
- Status: `Review`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- 新增 Scene 自有 `SceneUpdateContext`，保持 `DeltaSeconds`、`TotalSeconds`、`AnyInputDetected` 参数形状，并对负 delta 抛 `ArgumentOutOfRangeException`。
- `SceneGraphService.UpdateRuntime(...)` 作为 facade 转发到 `RuntimeScene.Update(...)`，未泄露 runtime owner。
- `RuntimeScene` 新增 `UpdateFrameCount` 与 `AccumulatedUpdateSeconds` 统计，`Clear()` / `LoadFromDescription(...)` 会重置统计。
- 本卡未实现默认旋转、App 主循环接线或 snapshot 诊断字段扩展。

## FilesChanged

- `src/Engine.Scene/SceneUpdateContext.cs`
- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-015.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-015.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 36 条通过）
- Smoke: `pass`（`SceneGraphService.UpdateRuntime(...)` 可调用；空 scene update 不崩溃；`UpdateFrameCount` / `AccumulatedUpdateSeconds` 可从 Scene 侧测试观察）
- Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**` 与任务指定边界/归档文档；未新增 Platform/App/Render/Editor 依赖）
- Perf: `pass`（update 仅做常量时间统计记账，不触发文件 IO、scene rebuild 或 render frame number 隐式依赖）

## Risks

- `low`：当前只建立 update 入口和统计地基；后续可见行为与 snapshot 诊断由后续任务卡承担。
