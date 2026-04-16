# TASK-QA-006 归档快照（Execution Prepared）

- TaskId: `TASK-QA-006`
- Title: `M5 变换链路门禁与回归复验（含 Rotation）`
- Priority: `P1`
- PrimaryModule: `QA`
- BoundaryContractPath:
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-QA`
- ClosedAt: `2026-04-16 22:55`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 完成 M5 变换链路（Position/Scale/Rotation）Build/Test/Smoke/Perf 门禁复验。
- 补充 Render 专项测试链路复验，确认 transform（含 Rotation）与 identity 兼容行为通过。
- 依赖方向复验通过：`Engine.Render` 不直接引用 `Engine.Scene`，仅依赖 `Engine.Contracts`。

## FilesChanged

- `.ai-workflow/tasks/task-qa-006.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-QA-006.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug -m:1`，存在 MSB3101 写缓存告警）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release -m:1`，存在 MSB3101 写缓存告警）
- Test(All): `pass`（`dotnet test AnsEngine.sln -m:1`）
- Test(Render): `pass`（`dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1 --no-restore --no-build`，6/6）
- DependencyGate: `pass`（`RenderSceneRef=absent`，`RenderContractsRef=present`）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`31.45s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，`ExitCode=0`，`46.25s`）
- CodeQuality: `pass`（NoNewHighRisk=true，MustFixCount=0）
- DesignQuality: `pass`（DQ-1/DQ-2/DQ-3/DQ-4）

## Risks

- `low`: `dotnet` 在当前环境对部分 `obj/*AssemblyReference.cache` 写入受限（MSB3101），不影响当前验证结果，但建议后续清理权限或统一构建环境。
