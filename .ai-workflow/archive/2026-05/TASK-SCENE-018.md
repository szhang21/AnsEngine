# TASK-SCENE-018 归档快照

- TaskId: `TASK-SCENE-018`
- Title: `M16 Scene runtime component bridge`
- Priority: `P1`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-05-02 11:48`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `RuntimeScene.LoadFromDescription(...)` 改为从 normalized `SceneObjectDescription.Components` 投影出的 Transform/MeshRenderer component descriptions 构建 runtime components。
- Transform-only object 会进入 runtime snapshot，`HasTransform=true` 且 `HasMeshRenderer=false`。
- `BuildRenderItems()` 继续只输出同时具备 Transform 与 MeshRenderer 的 runtime object。
- M15 默认旋转 smoke behavior 仍只选择第一个可渲染 object，不旋转 Transform-only object。
- 首次 App 回归与整解 build 并行执行时因 shared apphost 输出竞争出现瞬时 `NETSDK1177` / `MSB3030`，已记录并串行重跑通过。

## FilesChanged

- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
- `src/Engine.Scene/Runtime/SceneMeshRendererComponent.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-018.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-018.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 48 条通过）
- Regression: `pass`（Render.Tests 18 条通过；App.Tests 9 条串行重跑通过）
- Smoke: `pass`（Transform-only object 在 snapshot 可见且 `HasMeshRenderer=false`；render items 仅包含可渲染 object；M15 update 只旋转首个可渲染 object）
- Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**` 与任务指定边界/归档文档；`rg` 检查未发现 Scene 引入 Render/App/Editor/Platform 或 file-model/JSON runtime 依赖）
- Perf: `pass`（load 时一次性 bridge normalized descriptions；render filtering 无逐帧 JSON 读取、文件 IO、对象重建或 render side effect）

## Risks

- `low`：SceneData 的旧对象级投影仍作为串行迁移期间的兼容入口存在；本卡 runtime bridge 已消费 normalized component projections。
