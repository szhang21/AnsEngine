# TASK-SCENE-009 归档快照

- TaskId: `TASK-SCENE-009`
- Title: `M10 SceneDescription 到运行时场景初始化入口`
- Priority: `P1`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-04-26 15:28`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `Engine.Scene` 新增对 `Engine.SceneData` 的编译期依赖，并在 `SceneGraphService` 中新增 `LoadSceneDescription(SceneDescription)` 初始化入口。
- `SceneDescription` 中的对象、材质、`LocalTransform` 与相机现在可直接映射为稳定的 `SceneRenderFrame` 输出，且不在 Scene 内执行 JSON 解析或文件 IO。
- 保留旧的 `AddRootNode()` 路径做兼容，同时补齐描述驱动初始化、多对象稳定输出和默认相机回退测试。

## FilesChanged

- `src/Engine.Scene/Engine.Scene.csproj`
- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/boundaries/engine-scenedata.md`
- `.ai-workflow/tasks/task-scene-009.md`
- `.ai-workflow/archive/2026-04/TASK-SCENE-009.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.Scene/Engine.Scene.csproj -c Debug --nologo -v minimal`）
- Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.Scene/Engine.Scene.csproj -c Release --nologo -v minimal`）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --nologo -v minimal`；11 条通过）
- Smoke: `pass`（`LoadSceneDescription_BuildRenderFrame_MapsObjectsCameraAndLocalTransforms` 已验证样例式 `SceneDescription` 可驱动现有 `SceneRenderFrame` 输出）
- Perf: `pass`（`SceneDescription` 映射仅发生在初始化入口；稳定帧循环阶段无 JSON 解析或文件读取）

## Risks

- `low`：当前 `Scene` 仅按 M10 语义把 `LocalTransform` 直接视为当前帧可用变换；后续如果 M11 引入真正层级关系，应在 Scene 内部扩展 local-to-world 计算，而不是修改 `SceneData` 公开模型。
