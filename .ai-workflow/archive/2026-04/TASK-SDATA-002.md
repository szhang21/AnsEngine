# TASK-SDATA-002 归档快照

- TaskId: `TASK-SDATA-002`
- Title: `M10 场景 JSON 描述模型、加载与规范化`
- Priority: `P0`
- PrimaryModule: `Engine.SceneData`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
- Owner: `Exec-SceneData`
- ClosedAt: `2026-04-26 15:12`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 新增 `JsonSceneDescriptionLoader`，在 `Engine.SceneData` 内完成 `JSON -> FileModel -> 校验/默认值/规范化 -> SceneDescriptionLoadResult` 主路径。
- 保持 `FileModel` 与 `Descriptions` 双层结构，并以显式失败结果表达 `NotFound / InvalidJson / MissingRequiredField / DuplicateObjectId / InvalidReference / InvalidValue`。
- 补齐样例场景 JSON、测试资源复制与回归测试，覆盖合法加载、非法 JSON、缺失 `mesh`、重复对象 `id`、默认材质、默认 transform 与默认相机回填。

## FilesChanged

- `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
- `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
- `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
- `tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj`
- `.ai-workflow/boundaries/engine-scenedata.md`
- `.ai-workflow/tasks/task-sdata-002.md`
- `.ai-workflow/archive/2026-04/TASK-SDATA-002.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build(Debug): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Debug --nologo -v minimal`）
- Build(Release): `pass`（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Release --nologo -v minimal`）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --nologo -v minimal`；12 条通过）
- Smoke: `pass`（样例场景 `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json` 已被 `JsonLoader_ValidScene_LoadsNormalizedDescription` 成功加载为 `SceneDescription`）
- Perf: `pass`（JSON 读取、反序列化与规范化只发生在加载阶段，无逐帧解析逻辑）

## Risks

- `low`：当前 loader 只覆盖 M10 计划约定的单场景、单相机、对象列表和默认值规范化范围；后续若扩展层级、Prefab、灯光或 schema 版本兼容，应继续在 `Engine.SceneData` 内增量扩展，不把规范化职责外溢到 `Scene/App`。
