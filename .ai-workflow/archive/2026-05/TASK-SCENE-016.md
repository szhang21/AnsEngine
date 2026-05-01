# TASK-SCENE-016 归档快照

- TaskId: `TASK-SCENE-016`
- Title: `M15 Runtime update 默认旋转 smoke behavior`
- Priority: `P1`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-05-01 20:20`
- Status: `Review`
- HumanSignoff: `pending`
- ModuleAttributionCheck: `pass`

## Summary

- `RuntimeScene.Update(...)` 在统计更新后执行最小默认旋转 smoke behavior。
- 每次 update 只选择第一个同时具备 `Transform` 与 `MeshRenderer` 的 runtime object。
- 旋转只改 local rotation，按 `DeltaSeconds * MathF.PI * 0.5f` 绕 Y 轴推进；position、scale、mesh/material 和 camera 保持不变。
- `BuildRenderFrame()` 与 runtime snapshot 都只读取 update 后状态，没有隐式推进 update。

## FilesChanged

- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-016.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-016.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 41 条通过）
- Smoke: `pass`（update 后首个可渲染对象 rotation 变化；position/scale 与资源引用保持；`BuildRenderFrame()` 与 snapshot 观察到同一 rotation）
- Boundary: `pass`（仅改 `src/Engine.Scene/**`、`tests/Engine.Scene.Tests/**` 与任务指定边界/归档文档；未新增 Platform/App/Render/Editor 依赖）
- Perf: `pass`（旋转行为只遍历 runtime objects 找到首个可渲染对象，不触发文件 IO、对象重建或 Render side effect）

## Risks

- `low`：当前旋转仅是 M15 smoke behavior；后续正式动画/update system 需要单独任务卡承接。
