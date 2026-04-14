# TASK-SCENE-004 归档快照（Execution Prepared）

- TaskId: `TASK-SCENE-004`
- Title: `M4b Scene 单契约出口收敛`
- Priority: `P0`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-04-14 10:43`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `Engine.Scene` 渲染输出收敛到单一 `Engine.Contracts` 契约出口，移除 Scene 内部契约镜像类型。
- 删除 `src/Engine.Scene/SceneRenderContracts.cs`，去除 `FromContracts` 每帧转换路径。
- `SceneGraphService` 保持渲染行为不变（材质脉冲与帧号递增），同时消除双轨语义。

## FilesChanged

- `src/Engine.Scene/SceneGraphService.cs`
- `src/Engine.Scene/SceneRenderContracts.cs`（删除）
- `.ai-workflow/tasks/task-scene-004.md`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-SCENE-004.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`，环境级 CS1668 警告）
- Build(Release): `pass`（`dotnet build -c Release -m:1`，环境级 CS1668 警告）
- Test: `pass`（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`26.59s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`41.48s`）
- HotPathEvidence: `pass`（`src/Engine.Scene` 无 `FromContracts` 调用；仅单契约 `SceneRenderFrame/SceneRenderItem` 构建）

## Risks

- `low`：移除兼容层后，若外部仍引用旧 Scene 契约类型将编译失败；当前仓库内扫描未发现残留引用。
