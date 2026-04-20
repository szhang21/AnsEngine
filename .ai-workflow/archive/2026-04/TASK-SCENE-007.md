# TASK-SCENE-007 归档快照（Execution Prepared）
- TaskId: `TASK-SCENE-007`
- Title: `M7 Scene 资源引用输出对齐`
- Priority: `P0`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-04-19 21:15`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneGraphService` 资源输出收敛为“候选值 -> Scene 回退解析 -> 结构化引用构造”，确保 `meshId/materialId` 可被 Render M7 入口稳定消费。
- 材质输出周期升级为 `default -> pulse -> highlight -> missing`，其中 `missing` 在 Scene 侧回退为 `default`，避免不受控资源标识外泄。
- 多对象场景下引入缺失 mesh 候选验证路径，但输出阶段统一回退为 `mesh://triangle`，保持渲染链路一致性。

## FilesChanged

- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/tasks/task-scene-007.md`
- `.ai-workflow/archive/2026-04/TASK-SCENE-007.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/boundaries/engine-contracts.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release -m:1`）
- Test: `pass`（`dotnet test AnsEngine.sln -m:1`；`Engine.Scene.Tests` 7/7）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`16.07s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.84s`）

## Risks

- `low`：当前 Scene 侧回退策略基于内置支持集合；后续若 Render 扩展材质入口，需要同步更新 Scene 支持集合以保持一致。
