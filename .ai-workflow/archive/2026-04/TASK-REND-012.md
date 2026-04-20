# TASK-REND-012 归档快照（Execution Prepared）
- TaskId: `TASK-REND-012`
- Title: `M7 Material 参数入口落地`
- Priority: `P0`
- PrimaryModule: `Engine.Render`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
- Owner: `Exec-Render`
- ClosedAt: `2026-04-18 00:58`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneRenderSubmissionBuilder` 增加最小 material 参数解析入口：`ResolveMaterial(materialId)`。
- 引入显式材质映射（`material://default` / `material://pulse` / `material://highlight`），替换 hash 派生颜色语义。
- 对未知材质标识使用默认材质参数回退，确保渲染可持续并可预测。

## FilesChanged

- `src/Engine.Render/SceneRenderSubmission.cs`
- `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
- `.ai-workflow/tasks/task-rend-011.md`
- `.ai-workflow/tasks/task-rend-012.md`
- `.ai-workflow/board.md`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-REND-011.md`
- `.ai-workflow/archive/2026-04/TASK-REND-012.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release -m:1`）
- Test: `pass`（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.70s`）

## Risks

- `low`：当前材质参数仅覆盖 RGB，后续引入纹理、金属度等参数时需扩展解析结果并补充兼容测试。
