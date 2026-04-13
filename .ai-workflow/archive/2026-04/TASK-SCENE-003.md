# TASK-SCENE-003 归档快照（Execution Prepared）

- TaskId: `TASK-SCENE-003`
- Title: `M4 渲染输入契约下沉到独立层`
- Priority: `P0`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-04-14 00:40`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `Engine.Scene` 正式接入 `Engine.Contracts` 项目依赖，建立 `Scene -> Contracts` 编译期依赖方向。
- `SceneGraphService` 输出链路改为以契约层类型为内部基准，并通过双接口维持旧路径兼容。
- 新增契约接口链路测试，验证 `SceneGraphService` 可被 `Engine.Contracts.ISceneRenderContractProvider` 直接消费。

## FilesChanged

- `src/Engine.Scene/Engine.Scene.csproj`
- `src/Engine.Scene/SceneRenderContracts.cs`
- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-003.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-SCENE-003.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`，环境级 CS1668 警告）
- Build(Release): `pass`（`dotnet build -c Release -m:1`，环境级 CS1668 警告）
- Test: `pass`（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`24.74s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`62.53s`）

## Risks

- `low`：当前保留 `Engine.Scene` 旧契约类型用于过渡，需在后续 `TASK-REND-006`/`TASK-APP-004` 完成后清理兼容层，避免双语义长期并存。
