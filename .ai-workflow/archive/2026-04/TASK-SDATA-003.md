# TASK-SDATA-003 归档快照

- TaskId: `TASK-SDATA-003`
- Title: `M11 SceneData 文档读写接口与 JSON store`
- Priority: `P0`
- PrimaryModule: `Engine.SceneData`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
- Owner: `Exec-SceneData`
- ClosedAt: `2026-04-28 01:00`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 新增 `ISceneDocumentStore`、`JsonSceneDocumentStore`、文档级 load/save 结果与失败类型。
- `SceneFileDocument` 支持 JSON 读取、保存、缺失文件、非法 JSON 与写入失败的显式失败语义。
- `JsonSceneDescriptionLoader` 保持运行时入口职责，并开始复用同一 Scene file JSON serializer。

## FilesChanged

- `src/Engine.SceneData/Abstractions/ISceneDocumentStore.cs`
- `src/Engine.SceneData/DocumentStore/**`
- `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
- `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
- `.ai-workflow/boundaries/engine-scenedata.md`
- `.ai-workflow/tasks/task-sdata-003.md`
- `.ai-workflow/archive/2026-04/TASK-SDATA-003.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Debug --nologo -v minimal`）
- Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Release --nologo -v minimal`）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --nologo -v minimal`；28 条通过）
- Smoke: `pass`（测试覆盖 `SceneFileDocument` 保存后再读取）
- Perf: `pass`（读写仅由显式 document store 操作触发，无运行时逐帧 IO）

## Risks

- `low`：当前 store 直接写目标文件；若后续需要原子保存、备份或并发保护，应在 document store 内扩展，不改变运行时 loader 职责。
