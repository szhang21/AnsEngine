# TASK-SCENE-014 归档快照

- TaskId: `TASK-SCENE-014`
- Title: `M14 Runtime Snapshot 查询与边界测试`
- Priority: `P2`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-05-01 10:39`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneGraphService` 新增 `CreateRuntimeSnapshot()` 与 `FindObject(string objectId)` 只读查询面。
- runtime snapshot 覆盖 object identity、transform、mesh/material 与 camera state。
- snapshot/query 返回值对象，不暴露内部 runtime object/component 可变集合。
- 新增边界测试确认 `Engine.Scene` 不依赖 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL，Render/SceneData 不反向依赖 runtime component 类型。

## FilesChanged

- `src/Engine.Scene/Runtime/RuntimeSceneSnapshot.cs`
- `src/Engine.Scene/Runtime/SceneRuntimeObjectSnapshot.cs`
- `src/Engine.Scene/Runtime/SceneCameraRuntimeSnapshot.cs`
- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
- `src/Engine.Scene/Runtime/SceneCameraRuntimeState.cs`
- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-014.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-014.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`，Scene.Tests 30 条通过；`dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj --no-restore --nologo -v minimal`，Render.Tests 16 条通过；`dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`，SceneData.Tests 28 条通过；`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过）
- Smoke: `pass`（可按 object id 查询 runtime object snapshot；snapshot 是只读值对象，不暴露 runtime 内部集合；Render 不引用 runtime component 类型）
- Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**` 与任务指定边界/归档文档；未改 Render/SceneData 业务实现，未新增禁止依赖）
- Perf: `pass`（snapshot 只遍历 runtime object 当前状态，不触发 scene rebuild 或文件读取）

## Risks

- `low`：snapshot 字段目前保持最小可观察面；后续若要对外消费更多 runtime 状态，应继续通过只读 snapshot 增量扩展，避免暴露内部组件集合。
