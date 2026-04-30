# TASK-EAPP-005 归档快照

- TaskId: `TASK-EAPP-005`
- Title: `M13 Open/Save/Save As 工作流`
- Priority: `P3`
- PrimaryModule: `Engine.Editor.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
- Owner: `Exec-EditorApp`
- ClosedAt: `2026-04-30 15:53`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- Toolbar 的 Open、Save、Save As 接入路径输入工作流。
- 文件操作全部通过 `EditorAppController` 转发到 `SceneEditorSession.Open/Save/SaveAs`。
- 启动路径继续支持 `ANS_ENGINE_EDITOR_SCENE_PATH` 覆盖，默认路径仍解析源码 sample scene。
- 测试覆盖环境变量覆盖、保存成功写盘并清 dirty、Save As 更新路径、Open 失败不污染当前 session、Save As 失败保留 dirty。

## FilesChanged

- `src/Engine.Editor.App/EditorFileWorkflowState.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `tests/Engine.Editor.App.Tests/EditorFileWorkflowStateTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-005.md`
- `.ai-workflow/archive/2026-04/TASK-EAPP-005.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；`Engine.Editor.App.Tests` 23 条通过）
- Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；Open/Save/Save As 控件渲染在真实 ImGui frame，退出码 0）
- Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档；未修改 `Engine.App` 默认运行路径或 `SceneData` 契约）
- Perf: `pass`（Open/Save 只在用户触发时执行，无逐帧文件 IO）

## Risks

- `low`：第一版路径选择是文本输入，不包含原生系统文件对话框。
