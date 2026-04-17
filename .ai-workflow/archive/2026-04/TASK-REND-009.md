# TASK-REND-009 归档快照（Execution Prepared）

- TaskId: `TASK-REND-009`
- Title: `M6 Render MVP Uniform 渲染改造（合并 M6 Mesh 数据统一入口收敛）`
- Priority: `P0`
- PrimaryModule: `Engine.Render`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
- Owner: `Exec-Render`
- ClosedAt: `2026-04-17 12:45`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneRenderSubmissionBuilder` 收敛为统一 mesh 入口（`MeshId -> model-space vertices`），按批次输出 `ModelViewProjection`。
- `NullRenderer` 切换为 shader `uMvp` uniform 路径，主链路不再依赖 CPU 预写最终裁剪空间顶点。
- Render 测试升级为 MVP 路径校验：identity 回归、rotation 生效、多批次布局与 camera 影响均可验证。

## FilesChanged

- `src/Engine.Render/SceneRenderSubmission.cs`
- `src/Engine.Render/RenderPlaceholders.cs`
- `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
- `.ai-workflow/tasks/task-rend-009.md`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-REND-009.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release -m:1`）
- Test: `pass`（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.94s`）

## Risks

- `low`：当前统一 mesh 入口仅内置最小三角形，后续扩展新 mesh 类型时需补充映射策略与回归用例。
