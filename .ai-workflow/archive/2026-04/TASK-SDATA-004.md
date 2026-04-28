# TASK-SDATA-004 归档快照

- TaskId: `TASK-SDATA-004`
- Title: `M11 校验复用与 load-save-load 往返稳定`
- Priority: `P1`
- PrimaryModule: `Engine.SceneData`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
- Owner: `Exec-SceneData`
- ClosedAt: `2026-04-28 01:00`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 抽出 `SceneFileDocumentNormalizer` 统一承载校验、默认值与 `SceneDescription` 规范化规则。
- `JsonSceneDescriptionLoader` 通过 document store 读取文件，再调用 normalizer，避免 loader/store 规则分叉。
- 补齐 load-save-load 语义等价、默认值、重复 id、非法引用格式与非有限 transform 测试。

## FilesChanged

- `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
- `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
- `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
- `.ai-workflow/boundaries/engine-scenedata.md`
- `.ai-workflow/tasks/task-sdata-004.md`
- `.ai-workflow/archive/2026-04/TASK-SDATA-004.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Debug --nologo -v minimal`）
- Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Release --nologo -v minimal`）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --nologo -v minimal`；28 条通过）
- Smoke: `pass`（保存后的 `.scene.json` 可被 `JsonSceneDescriptionLoader` 加载为等价 `SceneDescription`）
- Perf: `pass`（loader/store 复用 normalizer，未引入多套重复规范化主路径）

## Risks

- `low`：当前引用格式校验为最小 `scheme://id` 形态，不检查真实资源存在性；若后续 schema 细化，应继续留在 SceneData 校验层。
