# TASK-SCENE-006 归档快照（Execution Prepared）

- TaskId: `TASK-SCENE-006`
- Title: `M6 Scene 对象与相机语义输出`
- Priority: `P0`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-04-17 12:45`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneGraphService` 输出帧新增 `SceneCamera(View/Projection)`，让 Scene 侧具备最小真实相机语义。
- 保留 M5 的 transform/material 动态输出，并新增轻量相机轨道变化（连续帧 `View` 变化、`Projection` 稳定）。
- `Engine.Scene.Tests` 补充相机语义有效性断言，覆盖空场景、普通路径与 contracts 接口路径。

## FilesChanged

- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/tasks/task-scene-006.md`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-SCENE-006.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release -m:1`）
- Test: `pass`（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj -m:1`）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.94s`）

## Risks

- `low`：当前相机轨道为最小动态口径，后续若引入多相机策略需补充切换规则与兼容测试。
