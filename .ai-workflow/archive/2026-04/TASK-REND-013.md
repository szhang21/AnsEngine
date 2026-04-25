# TASK-REND-013 归档快照

- TaskId: `TASK-REND-013`
- Title: `M9 Render mesh provider 接入与 GPU cache 主路径`
- Priority: `P1`
- PrimaryModule: `Engine.Render`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
- Owner: `Exec-Render`
- ClosedAt: `2026-04-25 18:31`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `NullRenderer` 显式接入 `IMeshAssetProvider`，`SceneRenderSubmissionBuilder` 通过 mesh geometry cache 消费真实 mesh CPU 资产。
- 新增 `SceneRenderGpuMeshResourceCache` 与 mesh cache key 语义，保证同 mesh 多实例复用 GPU 资源；内置三角形仅保留为 provider 失败 fallback。
- 补齐 Render provider 接入、fallback 与共享 mesh cache 测试，并同步更新 Render 边界文档。

## FilesChanged

- `src/Engine.Render/RenderPlaceholders.cs`
- `src/Engine.Render/SceneRenderSubmission.cs`
- `src/Engine.Render/SceneRenderMeshGeometry.cs`
- `src/Engine.Render/SceneRenderMeshGeometryCache.cs`
- `src/Engine.Render/SceneRenderGpuMeshResource.cs`
- `src/Engine.Render/SceneRenderGpuMeshResourceCache.cs`
- `tests/Engine.Render.Tests/NullRendererTests.cs`
- `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
- `tests/Engine.Render.Tests/SceneRenderGpuMeshResourceCacheTests.cs`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/tasks/task-rend-013.md`
- `.ai-workflow/archive/2026-04/TASK-REND-013.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build(Debug): `pass`（M9 相关源码已进入当前代码基线；Human 于 `2026-04-25` 确认 Render provider 接入链路验收通过）
- Build(Release): `pass`（同上）
- Test: `pass`（`tests/Engine.Render.Tests` 覆盖 provider 接入、fallback 与共享 mesh GPU cache 复用）
- Smoke: `pass`（Human 于 `2026-04-25` 确认真实磁盘 mesh 可被应用渲染链路稳定消费并退出）
- Perf: `pass`（`SceneRenderGpuMeshResourceCache` 与共享 mesh 用例验证同 mesh 不重复创建 GPU 资源）

## Risks

- `low`：当前 GPU cache key 仍按单一 mesh 维度收敛；若后续引入多子网格或材质变体，应继续在 Render 内扩展 cache 分层，不回退为硬编码顶点表主路径。
