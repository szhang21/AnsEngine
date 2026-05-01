# TASK-SCENE-010 归档快照

- TaskId: `TASK-SCENE-010`
- Title: `M14 Runtime Object 基础模型`
- Priority: `P0`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-05-01 05:46`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 新增 internal `SceneRuntimeObject`，承载 `NodeId`、`ObjectId`、`ObjectName`。
- 新增 internal `RuntimeScene`，负责 runtime object 集合持有、创建和清空。
- `SceneGraphService` 持有 runtime scene owner，`NodeCount` 从 runtime object count 返回。
- `LoadSceneDescription` 重新加载时清空旧 runtime objects 并重建 object identity。

## FilesChanged

- `src/Engine.Scene/Properties/AssemblyInfo.cs`
- `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-010.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-010.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 14 条通过；`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过）
- Smoke: `pass`（重新 load scene 后 runtime object count 清空并重建，`NodeCount` 与 runtime scene object count 一致）
- Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**` 与任务指定边界/归档文档；未新增 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL 依赖）
- Perf: `pass`（runtime owner 仅在显式 add/load 路径维护 object list，不新增逐帧文件 IO 或 update loop）

## Risks

- `low`：当前 runtime object 仅包含 identity；transform、mesh renderer、camera、runtime snapshot 和 render frame 重写由后续 M14 任务承接。
