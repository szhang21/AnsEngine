# TASK-EAPP-002 归档快照

- TaskId: `TASK-EAPP-002`
- Title: `M13 编辑器基础布局与状态栏`
- Priority: `P1`
- PrimaryModule: `Engine.Editor.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
- Owner: `Exec-EditorApp`
- ClosedAt: `2026-04-30 15:35`
- Status: `Review`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- 新增 Toolbar、Hierarchy、Inspector、Status Bar 的 GUI snapshot 模型，状态来源保持为 `SceneEditorSession`。
- 新增真实 ImGui/OpenGL 渲染后端，负责 ImGui context、字体纹理、shader、VAO/VBO/EBO、鼠标 IO 和 draw data 提交。
- 窗口默认启用真实 ImGui frame，保留环境变量关闭路径作为诊断后门。
- 未实现按钮只写入明确 last error，不伪造 Open/Save/Add/Remove 成功。

## FilesChanged

- `src/Engine.Editor.App/Engine.Editor.App.csproj`
- `src/Engine.Editor.App/EditorAppOptions.cs`
- `src/Engine.Editor.App/EditorAppWindow.cs`
- `src/Engine.Editor.App/EditorGuiRenderer.cs`
- `src/Engine.Editor.App/EditorGuiSnapshot.cs`
- `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
- `src/Engine.Editor.App/EditorHierarchyItemSnapshot.cs`
- `src/Engine.Editor.App/EditorInspectorSnapshot.cs`
- `src/Engine.Editor.App/EditorStatusBarSnapshot.cs`
- `src/Engine.Editor.App/EditorToolbarAction.cs`
- `src/Engine.Editor.App/ImGuiOpenGlRenderer.cs`
- `tests/Engine.Editor.App.Tests/EditorAppOptionsTests.cs`
- `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
- `.ai-workflow/boundaries/engine-editor-app.md`
- `.ai-workflow/tasks/task-eapp-002.md`
- `.ai-workflow/archive/2026-04/TASK-EAPP-002.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；`Engine.Editor.App.Tests` 13 条通过，整解测试通过）
- Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；真实 ImGui frame 启动并关闭，退出码 0）
- Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档；未修改 `Engine.Editor`、`Engine.App`）
- Perf: `pass`（基础布局每帧消费 session 快照，无逐帧文件 IO 或重复 session open）

## Risks

- `low`：当前 Toolbar 按钮仍为占位行为，后续 `TASK-EAPP-005/006` 会接入真实 Open/Save/Add/Remove。
