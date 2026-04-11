# TASK-SCENE-001 归档快照（Execution Prepared）

- TaskId: `TASK-SCENE-001`
- Title: `M4 Scene-Render 最小契约定义`
- Priority: `P0`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-04-11 11:25`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 在 `Engine.Scene` 新增最小提交契约：`SceneRenderItem`、`SceneRenderFrame`、`ISceneRenderContractProvider`。
- `SceneGraphService` 实现契约并输出渲染快照，保持 `Scene` 仅负责数据组织，不依赖 `Render` 内部实现。
- 增加最小契约测试：空场景提交为空、单节点提交包含默认 mesh/material。
- 更新 `Engine.Scene` 边界合同变更日志，记录 M4 契约基线。

## FilesChanged

- `src/Engine.Scene/SceneRenderContracts.cs`
- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-001.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-SCENE-001.md`

## ValidationEvidence

- Build(Debug): `fail -> pass`（首次 `CS2012` 文件占用，复跑 `dotnet build -c Debug -m:1` 通过）
- Build(Release): `pass`（`dotnet build -c Release -m:1`）
- Test: `pass`（`dotnet test -m:1`，`Engine.Scene.Tests` 3/3 通过）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`30.22s`，`ExitCode=0`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`45.17s`，`ExitCode=0`，无明显退化）

## Risks

- `low`：当前仅定义了最小提交字段，后续引入更多渲染属性时需通过版本化或向后兼容扩展避免接口抖动。
