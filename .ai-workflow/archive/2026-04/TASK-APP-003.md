# TASK-APP-003 归档快照（Execution Prepared）

- TaskId: `TASK-APP-003`
- Title: `M4 提交流程编排配套`
- Priority: `P0`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-App`
- ClosedAt: `2026-04-11 11:55`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 组合根将 `SceneGraphService` 作为 `ISceneRenderContractProvider` 注入 `NullRenderer`，完成 M4 提交流程衔接。
- `Engine.App` 保持编排边界：不实现渲染后端、不实现场景内部逻辑。
- 主循环阶段顺序保持稳定并兼容现有退出收口。
- 更新 `Engine.App` 边界合同记录 M4 编排口径。

## FilesChanged

- `src/Engine.App/ApplicationBootstrap.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-app-003.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-APP-003.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build -c Debug -m:1`，环境级 CS1668 警告）
- Build(Release): `pass`（`dotnet build -c Release -m:1`，环境级 CS1668 警告）
- Test: `fail -> pass`（首次 `CS2012` 文件占用，清理进程后 `dotnet test -m:1` 通过）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`30.15s`，`ExitCode=0`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`45.15s`，`ExitCode=0`）

## Risks

- `medium`: 当前运行环境仍使用 headless 口径验证，建议在图形桌面环境补一次可视链路抽样复验。

