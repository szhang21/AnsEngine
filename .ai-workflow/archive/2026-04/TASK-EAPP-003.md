# TASK-EAPP-003 归档快照

- TaskId: `TASK-EAPP-003`
- Title: `M13 Hierarchy 面板与选择联动`
- Priority: `P2`
- PrimaryModule: `Engine.Editor.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
- Owner: `Exec-EditorApp`
- ClosedAt: `2026-04-30 15:40`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- Hierarchy 列表继续读取 `SceneEditorSession.Objects`，显示 object name 和 object id。
- 点击 Hierarchy 对象调用 `EditorAppController.SelectObject`，最终走 `SceneEditorSession.SelectObject`。
- 选中高亮、Status Bar selected id 和 Inspector 选中态均从 session 当前 selection 生成。
- 测试覆盖选择成功不改变 dirty、选择失败保留原 selection 并显示 last error。

## FilesChanged

- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-003.md`
- `.ai-workflow/archive/2026-04/TASK-EAPP-003.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning 与本机 Windows Kits `LIB` warning）
- Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；`Engine.Editor.App.Tests` 15 条通过）
- Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；默认场景对象渲染在 Hierarchy，真实 ImGui frame 退出码 0）
- Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档；未修改 `Engine.Editor` selection 语义）
- Perf: `pass`（Hierarchy 每帧消费 session 快照，不做逐帧文件加载或昂贵重建）

## Risks

- `low`：真实鼠标点击 smoke 仍需人工复验；自动测试覆盖 session selection 结果和 GUI snapshot 同步。
