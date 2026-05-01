# TASK-SCENE-013 归档快照

- TaskId: `TASK-SCENE-013`
- Title: `M14 RuntimeScene 到 SceneRenderFrame 输出`
- Priority: `P2`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-05-01 14:18`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 回流修复 `Architecture/LegacyPath`，移除 `SceneGraphService` 中的 legacy render item list 与逐帧 demo frame generator。
- `BuildRenderFrame` 无条件从 `RuntimeScene` 的 runtime objects/components 输出 `SceneRenderFrame`。
- render item 的 node id、mesh/material 和 transform 均来自 runtime object/component。
- camera 输出来自 runtime camera state，并保持 description/default 语义兼容。
- `AddRootNode` 改为直接创建 runtime object + transform + mesh renderer，`RuntimeScene` 成为 render frame 单一状态源。

## FilesChanged

- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-013.md`
- `.ai-workflow/archive/2026-05/TASK-SCENE-013.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`，Scene.Tests 30 条通过；`dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj --no-restore --nologo -v minimal`，Render.Tests 16 条通过；`dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`，App.Tests 6 条通过；`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过）
- Smoke: `pass`（`BuildRenderFrame` 无条件读取 `RuntimeScene`；`AddRootNode` 写入 runtime components；transform/camera/material 在无运行时修改时保持稳定，运行时组件修改后 frame 反映新值）
- Boundary: `pass`（legacy path 符号无残留；未改 Contracts/Render/SceneData 业务实现，未新增禁止依赖）
- Perf: `pass`（frame build 只遍历 runtime object/components，无重复 description 解析、文件读取或内部逐帧 demo state 生成）

## Risks

- `low`：旧 demo 动画语义已移除；后续若需要对象动画或 camera motion，应由 runtime update pipeline 显式驱动 runtime components。
