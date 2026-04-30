# TASK-EAPP-007 归档快照

- TaskId: `TASK-EAPP-007`
- Title: `M13 Docked Editor Layout`
- Priority: `P3`
- PrimaryModule: `Engine.Editor.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
- Owner: `Exec-EditorApp`
- ClosedAt: `2026-05-01 03:24`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- Editor GUI 收敛为固定停靠布局：Toolbar 顶部、Hierarchy 左侧、Workspace 中央、Inspector 右侧、Status Bar 底部。
- `EditorGuiSnapshot` 增加布局快照，测试可验证各区域位置和尺寸，不依赖原生窗口。
- `EditorGuiRenderer` 按布局快照使用固定 ImGui window bounds，避免自由漂浮窗口错位。
- Editor.App 相关工作流测试改用临时 scene，避免共享 sample scene 被人工/GUI smoke 写入后污染测试预期。

## FilesChanged

- `src/Engine.Editor.App/EditorGuiSnapshot.cs`
- `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
- `tests/Engine.Editor.App.Tests/EditorObjectWorkflowStateTests.cs`
- `tests/Engine.Editor.App.Tests/EditorInspectorInputStateTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-007.md`
- `.ai-workflow/archive/2026-04/TASK-EAPP-007.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；`Engine.Editor.App.Tests` 31 条通过）
- Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；真实 ImGui frame 启动并关闭，退出码 0）
- Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档；未改 `Engine.Editor`、`SceneData`、`Engine.App` 或 `Render`）
- Perf: `pass`（布局只消费 GUI snapshot 与 ImGui window bounds，无逐帧文件 IO、重复 session open 或新业务轮询）

## Risks

- `low`：当前是固定分区布局，不是可拖拽 docking 框架；后续若需要 dock tabs、viewport 或主题系统，应另立卡扩展。
