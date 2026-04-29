# TASK-EDITOR-003 归档快照

- TaskId: `TASK-EDITOR-003`
- Title: `M12 编辑命令编排与 selection/dirty 语义`
- Priority: `P2`
- PrimaryModule: `Engine.Editor`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor.md`
- Owner: `Exec-Editor`
- ClosedAt: `2026-04-30 01:04`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneEditorSession` 新增 `SelectObject`、`ClearSelection`、`AddObject`、`RemoveObject`、`RemoveSelectedObject`、`UpdateObjectId`、`UpdateObjectName`、`UpdateObjectResources`、`UpdateObjectTransform`。
- 编辑命令统一委托 `SceneFileDocumentEditor`，成功后重建当前文档与规范化场景快照，并设置 `IsDirty=true`。
- selection 只保存 object id：选择对象不改变 dirty；删除当前选中对象后清空 selection；修改当前选中对象 id 后 selection 跟随新 id。
- 无文档和编辑失败均返回显式失败；编辑失败不改变旧 document、scene、selection 或 dirty。

## FilesChanged

- `src/Engine.Editor/Session/SceneEditorSession.cs`
- `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
- `.ai-workflow/boundaries/engine-editor.md`
- `.ai-workflow/tasks/task-editor-003.md`
- `.ai-workflow/archive/2026-04/TASK-EDITOR-003.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test AnsEngine.sln --nologo -v minimal`；整解测试通过）
- Smoke: `pass`（无文档 select/edit 显式失败；选择存在对象不置 dirty；编辑成功置 dirty；编辑失败不污染 document/scene/selection/dirty）
- Boundary: `pass`（仅改 `src/Engine.Editor/**`、`tests/Engine.Editor.Tests/**` 与必需工作流/边界归档文档；未新增禁止依赖）

## Risks

- `low`：当前不包含保存/另存为与磁盘 reload 闭环，后续由 `TASK-EDITOR-004` 承接。
