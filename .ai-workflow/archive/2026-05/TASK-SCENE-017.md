# TASK-SCENE-017 归档快照

- TaskId: `TASK-SCENE-017`
- Title: `M15 Runtime snapshot 诊断与边界测试`
- Priority: `P2`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-05-01 20:36`
- Status: `Review`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- `RuntimeSceneSnapshot` 新增只读诊断字段 `UpdateFrameCount` 与 `AccumulatedUpdateSeconds`。
- snapshot 继续通过现有 `SceneRuntimeObjectSnapshot.LocalTransform` 观察 update 后 rotation，未扩展 object snapshot 字段。
- 补齐 Scene/App/Render 边界回归测试，确认 Render 不引用 `SceneUpdateContext` 或 runtime scene/object/component 类型，App 仍通过 `ISceneRuntime` abstraction 驱动 update。
- `BuildRenderFrame()` 与 runtime update 统计保持分离，render frame number 不推进 update count。

## FilesChanged

- `src/Engine.Scene/Runtime/RuntimeSceneSnapshot.cs`
- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `tests/Engine.Render.Tests/RenderBoundaryTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-017.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-017.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过；`/Users/ans/.dotnet/dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj --no-restore --nologo -v minimal`，Render.Tests 18 条通过）
- Smoke: `pass`（`RuntimeSceneSnapshot` 可只读观察 update frame count/seconds；update 后 rotation 可由 snapshot 与 render frame 观察；Render 不感知 update context/runtime scene 类型）
- Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**`、`tests/Engine.App.Tests/**`、`tests/Engine.Render.Tests/**` 与任务指定边界/归档文档；未改 App/Render 业务实现）
- Perf: `pass`（snapshot 只携带已有统计值，不触发 scene rebuild、文件 IO 或 render side effect）

## Risks

- `low`：snapshot 诊断字段保持最小只读范围；后续若扩展诊断，应继续避免 Render/App 感知 runtime internals。
