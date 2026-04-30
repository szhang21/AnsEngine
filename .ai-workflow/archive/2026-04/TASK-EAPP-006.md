# TASK-EAPP-006 归档快照

- TaskId: `TASK-EAPP-006`
- Title: `M13 Add/Remove Object GUI 工作流`
- Priority: `P3`
- PrimaryModule: `Engine.Editor.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
- Owner: `Exec-EditorApp`
- ClosedAt: `2026-04-30 15:58`
- Status: `Review`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- Toolbar 的 Add Object 与 Remove Selected 接入真实 object workflow。
- Add Object 从当前对象集合推导最小可用 `object-XXX` id，创建默认 `mesh://cube`、`material://default`、identity transform 对象并自动选中。
- Remove Selected 调用 `SceneEditorSession.RemoveSelectedObject`，成功后对象消失且 selection 清空。
- 测试覆盖默认字段、id 缺口策略、Add 后选中、Remove 清 selection、保存后 reload 保留增删结果。

## FilesChanged

- `src/Engine.Editor.App/EditorDefaultObjectFactory.cs`
- `src/Engine.Editor.App/EditorObjectWorkflowState.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `tests/Engine.Editor.App.Tests/EditorObjectWorkflowStateTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-006.md`
- `.ai-workflow/archive/2026-04/TASK-EAPP-006.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；`Engine.Editor.App.Tests` 28 条通过）
- Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；Add/Remove 控件渲染在真实 ImGui frame，退出码 0）
- Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档；未修改 `Engine.Editor` 或 `SceneData`）
- Perf: `pass`（id 生成只扫描当前对象集合，无逐帧文件 IO）

## Risks

- `low`：id 生成采用最小可用编号填补缺口；如未来需要只追加最大编号，应另行调整 UX 规则。
