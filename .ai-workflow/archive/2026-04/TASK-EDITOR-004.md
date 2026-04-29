# TASK-EDITOR-004 归档快照

- TaskId: `TASK-EDITOR-004`
- Title: `M12 保存、另存为与 reload 验证`
- Priority: `P3`
- PrimaryModule: `Engine.Editor`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor.md`
- Owner: `Exec-Editor`
- ClosedAt: `2026-04-30 01:07`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneEditorSession` 新增 `Save` 和 `SaveAs`，复用 `ISceneDocumentStore` 写入当前 `SceneFileDocument`。
- 保存状态机固定为：保留内存状态、写盘、重新 load、normalize；全部成功后才更新当前路径并将 `IsDirty=false`。
- `SaveAs` 成功后切换 `SceneFilePath` 到新路径；保存失败或 reload/normalize 失败时保留当前内存文档、规范化场景、路径和 dirty。
- 测试覆盖保存成功、另存为成功、无文档失败、写盘失败回滚和写盘成功但 reload 失败回滚。

## FilesChanged

- `src/Engine.Editor/Session/SceneEditorSession.cs`
- `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
- `.ai-workflow/boundaries/engine-editor.md`
- `.ai-workflow/tasks/task-editor-004.md`
- `.ai-workflow/archive/2026-04/TASK-EDITOR-004.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test AnsEngine.sln --nologo -v minimal`；Editor.Tests 26 条通过，整解测试通过）
- Smoke: `pass`（`open -> edit -> save -> reload` 成功；保存成功 `IsDirty=false`；保存失败后 `IsDirty=true` 且内存修改保留）
- Boundary: `pass`（仅改 `src/Engine.Editor/**`、`tests/Engine.Editor.Tests/**` 与必需工作流/边界归档文档；未新增禁止依赖）

## Risks

- `low`：M12 仅提供 headless editor core；GUI 文件对话框、未保存关闭确认和 Undo/Redo 仍留给后续 M13/Milestone。
