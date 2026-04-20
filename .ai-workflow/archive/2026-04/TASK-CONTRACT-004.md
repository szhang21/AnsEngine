# TASK-CONTRACT-004 归档快照（Execution Prepared）

- TaskId: `TASK-CONTRACT-004`
- Title: `M7 资源输入契约收敛`
- Priority: `P0`
- PrimaryModule: `Engine.Contracts`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
- Owner: `Exec-Contracts`
- ClosedAt: `2026-04-18 11:05`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 新增 `SceneMeshRef` 与 `SceneMaterialRef` 资源引用契约，提供最小非空校验，建立 M7 资源输入的结构化语义。
- `SceneRenderItem` 新增结构化资源构造路径（`SceneMeshRef`/`SceneMaterialRef`），并保持 `meshId/materialId` 字符串字段与旧构造路径兼容。
- 兼容语义保证 Scene/Render 现有调用无需改动即可编译通过，为后续 `TASK-REND-011/012` 的资源解析入口落地提供稳定输入面。

## FilesChanged

- `src/Engine.Contracts/SceneResourceContracts.cs`
- `src/Engine.Contracts/SceneRenderContracts.cs`
- `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
- `.ai-workflow/tasks/task-contract-004.md`
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-CONTRACT-004.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug -m:1`）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release -m:1`）
- Test: `pass`（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj -m:1`）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.79s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）

## Risks

- `low`：当前仅定义最小结构化资源引用与校验，具体解析策略与回退语义将在 Render/Scene 后续任务中继续完善。
