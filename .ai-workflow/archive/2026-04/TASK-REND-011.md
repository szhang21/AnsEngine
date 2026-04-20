# TASK-REND-011 归档快照（Execution Prepared）
- TaskId: `TASK-REND-011`
- Title: `M7 Mesh 数据入口落地`
- Priority: `P0`
- PrimaryModule: `Engine.Render`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
- Owner: `Exec-Render`
- ClosedAt: `2026-04-18 00:58`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneRenderSubmissionBuilder` 统一 mesh 解析入口：主路径固定走 `ResolveMesh(meshId)`，输出模型空间顶点数组。
- 保留未知 mesh 回退策略（默认三角形），避免非法资源标识造成渲染中断。
- 同步收敛材质解析入口，消除对 hash 派生颜色语义的隐式耦合，形成 M7 最小可扩展资源消费基线。

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

- `low`：当前 mesh 资源仍以内置映射为主，后续引入外部资源系统时需保持解析入口与回退语义一致。
