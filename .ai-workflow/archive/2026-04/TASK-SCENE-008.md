# TASK-SCENE-008 归档快照

- TaskId: `TASK-SCENE-008`
- Title: `M9 Scene 真实 mesh 引用收敛`
- Priority: `P1`
- PrimaryModule: `Engine.Scene`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
- Owner: `Exec-Scene`
- ClosedAt: `2026-04-25 18:32`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneGraphService` 默认输出真实 `mesh://cube` 引用，并在多对象路径保留 `mesh://missing` 作为下游 fallback 验证入口。
- 保持 Scene 只持 `meshId/materialId` 契约语义，不引入磁盘路径、catalog 或导入器细节。
- 补齐共享 mesh 与缺失 mesh 引用测试，并同步更新 Scene 边界文档。

## FilesChanged

- `src/Engine.Scene/SceneGraphService.cs`
- `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/tasks/task-scene-008.md`
- `.ai-workflow/archive/2026-04/TASK-SCENE-008.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build(Debug): `pass`（M9 Scene mesh 引用路径已进入当前代码基线；Human 于 `2026-04-25` 确认验收通过）
- Build(Release): `pass`（同上）
- Test: `pass`（`tests/Engine.Scene.Tests` 覆盖真实 mesh 引用、共享 mesh 与缺失 mesh fallback 语义）
- Smoke: `pass`（Human 于 `2026-04-25` 确认新 mesh 引用语义未破坏 headless/真实窗口运行路径）
- Perf: `pass`（Scene 仅输出稳定 `meshId`，未引入逐帧路径解析或 IO）

## Risks

- `low`：当前 Scene 样例仍围绕少量固定 `meshId` 运行；后续若扩展样例集，应继续保持 `meshId` 语义化输出，不向 Scene 泄漏文件系统细节。
