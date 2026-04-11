# TASK-REND-004 归档快照（Execution Prepared）

- TaskId: `TASK-REND-004`
- Title: `M4 场景驱动渲染消费`
- Priority: `P0`
- PrimaryModule: `Engine.Render`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
- Owner: `Exec-Render`
- ClosedAt: `2026-04-11 11:40`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `Engine.Render` 通过契约层消费 `Engine.Scene` 的 `SceneRenderFrame`。
- 新增 `SceneRenderSubmissionBuilder`，将场景提交转为顶点数据并动态写入 VBO。
- `NullRenderer` 移除固定 demo 顶点提交流程，改为按场景提交绘制。
- 新增渲染消费链路最小测试项目并通过。
- 同步更新 `Engine.Render` 边界合同变更记录。

## FilesChanged

- `src/Engine.Render/Engine.Render.csproj`
- `src/Engine.Render/RenderPlaceholders.cs`
- `src/Engine.Render/SceneRenderSubmission.cs`
- `tests/Engine.Render.Tests/Engine.Render.Tests.csproj`
- `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/tasks/task-rend-004.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-REND-004.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build -c Release -m:1`）
- Test: `pass`（`dotnet test -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`30.15s`，`ExitCode=0`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`45.12s`，`ExitCode=0`）

## Risks

- `medium`：当前环境不支持真实窗口口径，Smoke/Perf 采用 headless 路径；建议在图形桌面环境补一次可视链路抽样复验。
