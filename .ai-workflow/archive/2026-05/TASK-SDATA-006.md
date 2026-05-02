# TASK-SDATA-006 归档快照

- TaskId: `TASK-SDATA-006`
- Title: `M16 SceneData component file schema`
- Priority: `P0`
- PrimaryModule: `Engine.SceneData`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
- Owner: `Exec-SceneData`
- ClosedAt: `2026-05-02 11:40`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 场景文件模型迁移到 `version: "2.0"` component array schema。
- 新增 `SceneFileComponentDefinition`、`SceneFileTransformComponentDefinition`、`SceneFileMeshRendererComponentDefinition` 和固定 Pascal `type` 读写。
- 旧 `version: "1.0"` 扁平对象格式加载失败，不做双轨生产兼容。
- 默认 app sample 与 SceneData test sample 均迁移到 `components`，保存不会回写 object-level `mesh/material/transform` 字段。

## FilesChanged

- `src/Engine.SceneData/FileModel/SceneFileComponentDefinition.cs`
- `src/Engine.SceneData/FileModel/SceneFileObjectDefinition.cs`
- `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
- `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
- `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
- `src/Engine.App/SampleScenes/default.scene.json`
- `.ai-workflow/boundaries/engine-scenedata.md`
- `.ai-workflow/tasks/task-sdata-006.md`
- `.ai-workflow/archive/2026-05/TASK-SDATA-006.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`；SceneData.Tests 31 条通过）
- Smoke: `pass`（默认 app sample 与 SceneData sample 均为 `version: "2.0"` + `components`；保存后 object 层不写旧扁平字段）
- Boundary: `pass`（仅改 `src/Engine.SceneData/**`、`tests/Engine.SceneData.Tests/**`、`src/Engine.App/SampleScenes/default.scene.json` 与任务指定边界/归档文档；未新增禁止依赖）
- Perf: `pass`（仅显式 load/save 序列化，无逐帧 JSON 解析或兼容分支轮询）

## Risks

- `low`：旧扁平属性作为短期 `JsonIgnore` 投影保留以避免提前改 Editor；后续 M16 卡会继续迁移 normalized model 与 editor 主 API。
