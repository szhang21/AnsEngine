# TASK-CONTRACT-005 归档快照

- TaskId: `TASK-CONTRACT-005`
- Title: `M9 Mesh CPU 资产契约与失败语义定稿`
- Priority: `P0`
- PrimaryModule: `Engine.Contracts`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
- Owner: `Exec-Contracts`
- ClosedAt: `2026-04-23 01:14`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 新增 `MeshAssetVertex` 与 `MeshAssetData`，把 M9 规范化 mesh CPU 数据模型收敛到 `Engine.Contracts`。
- 新增 `IMeshAssetProvider`、`MeshAssetLoadResult`、`MeshAssetLoadFailure` 与 `MeshAssetLoadFailureKind`，明确 provider 查询与显式失败语义。
- 保持 `SceneMeshRef` 作为稳定查询输入，不暴露 OBJ/OpenGL 细节，并补齐契约测试与边界文档说明。

## FilesChanged

- `src/Engine.Contracts/IMeshAssetProvider.cs`
- `src/Engine.Contracts/MeshAssetData.cs`
- `src/Engine.Contracts/MeshAssetLoadFailure.cs`
- `src/Engine.Contracts/MeshAssetLoadFailureKind.cs`
- `src/Engine.Contracts/MeshAssetLoadResult.cs`
- `src/Engine.Contracts/MeshAssetVertex.cs`
- `tests/Engine.Contracts.Tests/MeshAssetContractsTests.cs`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/tasks/task-contract-005.md`
- `.ai-workflow/archive/2026-04/TASK-CONTRACT-005.md`
- `.ai-workflow/archive/archive-index.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug --nologo`）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release --nologo`）
- Test: `pass`（`dotnet test AnsEngine.sln --nologo -v minimal`；`Engine.Contracts.Tests` 共 18 条通过）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.App/Engine.App.csproj --no-build`，ExitCode=0）
- Perf: `pass`（契约层仅新增只读数据模型与结果类型，无逐帧 IO 或主动分配循环）

## Risks

- `low`：当前结果类型先覆盖 `NotFound / InvalidData / UnsupportedFormat` 三类主失败语义；后续若需要更细的诊断码，应继续以向后兼容方式扩展，而不是回退到异常驱动分支。
