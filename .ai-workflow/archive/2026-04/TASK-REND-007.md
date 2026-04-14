# TASK-REND-007 归档快照（Execution Prepared）

- TaskId: `TASK-REND-007`
- Title: `M4b Render 默认回退 Provider 清理`
- Priority: `P0`
- PrimaryModule: `Engine.Render`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
- Owner: `Exec-Render`
- ClosedAt: `2026-04-14 15:24`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 移除 `NullRenderer` 默认回退 provider 构造入口，渲染器必须显式注入 `ISceneRenderContractProvider`。
- 删除 `DefaultSceneRenderContractProvider`，防止生产路径静默兜底掩盖装配遗漏。
- 新增漏注入失败测试，验证 provider 为 `null` 时会快速失败并给出可诊断异常。

## FilesChanged

- `src/Engine.Render/RenderPlaceholders.cs`
- `src/Engine.Render/SceneRenderSubmission.cs`
- `tests/Engine.Render.Tests/NullRendererTests.cs`
- `.ai-workflow/tasks/task-rend-007.md`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-REND-007.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`，环境级 CS1668 警告）
- Build(Release): `pass`（`dotnet build -c Release -m:1`，环境级 CS1668 警告）
- Test: `pass`（`dotnet test -m:1`，新增 `NullRendererTests` 通过）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`51.83s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`67.36s`）

## Risks

- `low`：漏注入将由静默回退变为显式失败，短期会暴露历史装配问题；符合本卡预期。
