# TASK-SDATA-007 归档快照

- TaskId: `TASK-SDATA-007`
- Title: `M16 normalized component descriptions and validation`
- Priority: `P0`
- PrimaryModule: `Engine.SceneData`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
- Owner: `Exec-SceneData`
- ClosedAt: `2026-05-02 11:44`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- `SceneObjectDescription` 迁移为 component-based normalized model。
- 新增 `SceneComponentDescription`、`SceneTransformComponentDescription`、`SceneMeshRendererComponentDescription`。
- Normalizer 收口：Transform 必需、MeshRenderer 可选、duplicate/unknown component fail、Transform 数值 finite、material 空白时默认 `material://default`。
- Transform-only object 可 normalize 成功，且不引入 runtime component 类型依赖。

## FilesChanged

- `src/Engine.SceneData/Descriptions/SceneComponentDescription.cs`
- `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
- `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
- `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
- `.ai-workflow/boundaries/engine-scenedata.md`
- `.ai-workflow/tasks/task-sdata-007.md`
- `.ai-workflow/archive/2026-05/TASK-SDATA-007.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`；SceneData.Tests 33 条通过）
- Smoke: `pass`（Transform-only object normalize 成功；缺 Transform 失败；MeshRenderer 缺 material 得到 `material://default`）
- Boundary: `pass`（仅改 `src/Engine.SceneData/**`、`tests/Engine.SceneData.Tests/**` 与任务指定边界/归档文档；未新增禁止依赖）
- Perf: `pass`（normalize 仅显式 load/save/reload 执行，无逐帧解析或跨模块回调）

## Risks

- `low`：旧扁平属性仍作为短期只读投影存在以支撑后续串行迁移；主 normalized model 已切到 components。
