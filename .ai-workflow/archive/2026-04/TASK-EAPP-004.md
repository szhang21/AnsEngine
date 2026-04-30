# TASK-EAPP-004 归档快照

- TaskId: `TASK-EAPP-004`
- Title: `M13 Inspector 对象编辑`
- Priority: `P3`
- PrimaryModule: `Engine.Editor.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
- Owner: `Exec-EditorApp`
- ClosedAt: `2026-04-30 15:48`
- Status: `Review`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- Inspector 显示并编辑选中对象的 Id、Name、Mesh、Material、Position、Rotation、Scale。
- 输入缓冲以显式 `Apply` 作为提交时机，提交全部通过 `SceneEditorSession` 的 update API。
- 成功编辑后 session dirty 变为 true；失败时 last error 显示原因，并回滚输入缓冲到 session 当前有效值。
- 测试覆盖 name/transform 成功编辑、重复 id 失败、非法 transform 失败和失败不污染 selection/dirty。

## FilesChanged

- `src/Engine.Editor.App/EditorInspectorSnapshot.cs`
- `src/Engine.Editor.App/EditorInspectorInputState.cs`
- `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `tests/Engine.Editor.App.Tests/EditorInspectorInputStateTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-004.md`
- `.ai-workflow/archive/2026-04/TASK-EAPP-004.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；`Engine.Editor.App.Tests` 18 条通过）
- Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；Inspector 字段渲染在真实 ImGui frame，退出码 0）
- Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档；未修改 `Engine.Editor` 或 `SceneData`）
- Perf: `pass`（Inspector 不做逐帧文件写入或重新加载；输入缓冲仅保留当前选中对象编辑态）

## Risks

- `low`：输入提交时机采用显式 Apply；后续若需要 Enter/失焦提交，应另行增强 GUI 交互，不改变 session 语义。
