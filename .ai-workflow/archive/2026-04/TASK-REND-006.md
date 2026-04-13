# TASK-REND-006 归档快照（Execution Prepared）

- TaskId: `TASK-REND-006`
- Title: `M4 Render 依赖反转与解耦`
- Priority: `P0`
- PrimaryModule: `Engine.Render`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
- Owner: `Exec-Render`
- ClosedAt: `2026-04-14 00:49`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `Engine.Render` 移除 `Engine.Scene` 编译期依赖并接入 `Engine.Contracts`。
- 渲染提交构建链路（`SceneRenderSubmissionBuilder` / `DefaultSceneRenderContractProvider`）统一消费契约层类型。
- `Engine.Render.Tests` 同步切换到契约层引用，验证 Render 契约消费路径稳定。

## FilesChanged

- `src/Engine.Render/Engine.Render.csproj`
- `src/Engine.Render/RenderPlaceholders.cs`
- `src/Engine.Render/SceneRenderSubmission.cs`
- `tests/Engine.Render.Tests/Engine.Render.Tests.csproj`
- `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-rend-006.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-REND-006.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`，环境级 CS1668 警告）
- Build(Release): `pass`（`dotnet build -c Release -m:1`，环境级 CS1668 警告）
- Test: `pass`（`dotnet test -m:1`，Render 契约消费测试通过）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`20.68s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`35.66s`）

## Risks

- `low`：当前解耦为编译期依赖层面，后续需由 `TASK-APP-004` 统一推进装配口径，避免运行时仍保留旧路径假设。
