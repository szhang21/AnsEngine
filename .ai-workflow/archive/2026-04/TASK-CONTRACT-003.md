# TASK-CONTRACT-003 归档快照（Execution Prepared）
- TaskId: `TASK-CONTRACT-003`
- Title: `M6 相机与 MVP 最小契约扩展`
- Priority: `P0`
- PrimaryModule: `Engine.Contracts`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
- Owner: `Exec-Contracts`
- ClosedAt: `2026-04-17 10:58`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 新增 `SceneCamera` 契约，承载最小 `View/Projection` 语义并提供 identity 默认值。
- `SceneRenderFrame` 扩展 `Camera` 字段并保留旧双参构造，确保历史调用默认兼容。
- 新增契约测试覆盖默认相机兼容路径与显式相机保真路径。

## FilesChanged

- `src/Engine.Contracts/SceneRenderContracts.cs`
- `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
- `.ai-workflow/tasks/task-contract-003.md`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-CONTRACT-003.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build -c Release -m:1`）
- Test: `pass`（`dotnet test -m:1`，`Engine.Contracts.Tests` 6/6）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`19.04s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`34.21s`）

## Risks

- `low`：当前仅扩展最小相机语义，后续若引入多相机/裁剪面策略需保持字段可选与向后兼容。
