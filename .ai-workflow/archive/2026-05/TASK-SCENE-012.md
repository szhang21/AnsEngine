# TASK-SCENE-012 归档快照

- TaskId: `TASK-SCENE-012`
- Title: `M14 SceneDescription 到 RuntimeScene 映射`
- Priority: `P1`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-05-01 05:51`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `RuntimeScene.LoadFromDescription` 将 `SceneDescription` 映射为 runtime objects、transform/mesh renderer components 与 camera runtime state。
- `SceneGraphService.LoadSceneDescription` 改为以 runtime scene 为主状态源，并重置 frame number。
- `NodeId` 按 scene object 顺序从 1 稳定分配。
- `AddRootNode` legacy demo path 与 scene description runtime path 隔离，避免状态混合。

## FilesChanged

- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-012.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-012.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 22 条通过；`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过）
- Smoke: `pass`（空/单/多对象 scene 可映射到 runtime scene；重复 load 清空旧 runtime state；`NodeCount` 与 runtime object count 一致）
- Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**` 与任务指定边界/归档文档；未新增 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL 依赖）
- Perf: `pass`（runtime scene 映射仅在显式 load 路径发生，无重复 JSON 解析、文件读取或逐帧映射）

## Risks

- `low`：`mRenderItems` 仍作为 013 前的兼容输出缓存保留；最终 render frame 输出迁移由 `TASK-SCENE-013` 承接。
