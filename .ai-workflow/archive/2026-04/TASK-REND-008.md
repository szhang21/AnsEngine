# TASK-REND-008 归档快照（Execution Prepared）
- TaskId: `TASK-REND-008`
- Title: `M5 Render 变换消费与提交应用`
- Priority: `P0`
- PrimaryModule: `Engine.Render`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
- Owner: `Exec-Render`
- ClosedAt: `2026-04-15 19:32`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneRenderSubmissionBuilder` 对提交顶点应用 `Scale -> Rotation -> Translation` 变换。
- 增加 identity 快路径，保障无显式 transform 时输出布局与历史行为兼容。
- 新增渲染测试覆盖 identity 回归与 rotation 生效路径。

## FilesChanged

- `src/Engine.Render/SceneRenderSubmission.cs`
- `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
- `.ai-workflow/tasks/task-rend-008.md`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-REND-008.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build -c Release -m:1`）
- Test: `pass`（`dotnet test -m:1`，`Engine.Render.Tests` 6/6）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`25.50s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`40.36s`）

## Risks

- `low`：当前顶点变换在 CPU 侧逐顶点执行；后续复杂场景需评估批量化或 GPU 侧矩阵路径。
