# TASK-ASSET-001 归档快照

- TaskId: `TASK-ASSET-001`
- Title: `M9 OBJ 磁盘导入与 mesh CPU 资产缓存主路径`
- Priority: `P0`
- PrimaryModule: `Engine.Asset`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-asset.md`
- Owner: `Exec-Asset`
- ClosedAt: `2026-04-23 23:35`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 在 `Engine.Asset` 新增 `DiskMeshAssetProvider`，完成 `meshId -> catalog -> OBJ -> MeshAssetData -> cache` 的最小生产主路径。
- 新增 `MeshCatalog` 与 `ObjMeshFileLoader`，支持基于 catalog 的相对路径解析、OBJ 面索引解析与规范化 CPU mesh 构建。
- `NullAssetService` 保持现有 `IAssetService` 兼容行为，并可选代理到 `IMeshAssetProvider`；同时补齐缺失、损坏、格式不支持、缓存命中与 headless 专项测试。

## FilesChanged

- `src/Engine.Asset/AssetHandle.cs`
- `src/Engine.Asset/DiskMeshAssetProvider.cs`
- `src/Engine.Asset/Engine.Asset.csproj`
- `src/Engine.Asset/MeshCatalog.cs`
- `src/Engine.Asset/NullAssetService.cs`
- `src/Engine.Asset/ObjMeshFileLoader.cs`
- `src/Engine.Asset/AssetPlaceholders.cs`
- `tests/Engine.Asset.Tests/AssetServiceTests.cs`
- `.ai-workflow/boundaries/engine-asset.md`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/tasks/task-asset-001.md`
- `.ai-workflow/archive/2026-04/TASK-ASSET-001.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug --nologo`）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release --nologo`）
- Test: `pass`（`dotnet test AnsEngine.sln --nologo -v minimal`；`Engine.Asset.Tests` 共 8 条通过）
- Smoke: `pass`（`dotnet test tests/Engine.Asset.Tests/Engine.Asset.Tests.csproj --nologo -v minimal --filter "FullyQualifiedName~HeadlessPath"` 通过，验证 headless 路径可加载真实磁盘 mesh）
- Perf: `pass`（重复请求命中 `meshId -> MeshAssetLoadResult` 缓存；专项测试验证缓存命中后不会因磁盘文件变化而重新解析）

## Risks

- `low`：当前磁盘格式仅支持带 `v/vt/vn` 面索引的同步 OBJ 最小子集；后续若接入更复杂 OBJ 变体、glTF 或异步加载，需要在 Asset 内增量扩展解析器与 provider，而不是改变公开契约。
