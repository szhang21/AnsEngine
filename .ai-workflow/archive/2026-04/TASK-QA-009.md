# TASK-QA-009 归档快照（Execution Prepared）
- TaskId: `TASK-QA-009`
- Title: `M7 门禁复验与关单收敛（含多对象与回退验证）`
- Priority: `P1`
- PrimaryModule: `QA`
- BoundaryContractPath:
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-QA`
- ClosedAt: `2026-04-20 17:36`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 完成 M7 全链路 Build/Test/Smoke/Perf 门禁复验，结果均通过。
- 完成 mesh/material 解析、多对象与回退路径专项回归，结果均通过。
- 依赖与边界复验通过：Render 仅依赖 Contracts，Scene 仅输出资源标识并由 Render 统一解析。

## FilesChanged

- `.ai-workflow/tasks/task-qa-009.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-QA-009.md`

## ValidationEvidence

- Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug -m:1`，环境级 `CS1668` 警告）
- Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release -m:1`，环境级 `CS1668` 警告）
- Test(All): `pass`（`dotnet test AnsEngine.sln -m:1`）
- Test(Render): `pass`（`dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`，10/10）
- Test(Scene): `pass`（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj -m:1`，7/7）
- Test(Contracts): `pass`（`dotnet test tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj -m:1`，9/9）
- Test(Fallback+MultiObject): `pass`（Render 3/3 + Scene 2/2）
- DependencyGate: `pass`（`RenderSceneRef=absent`，`RenderContractsRef=present`）
- Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.61s`）
- Perf: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
- CodeQuality: `pass`（NoNewHighRisk=true，MustFixCount=0）
- DesignQuality: `pass`（DQ-1/DQ-2/DQ-3/DQ-4）

## Risks

- `low`：当前 Smoke/Perf 为 headless 路径，图形可见口径依赖既有人工验收链；未观察到相较 M6 的明显退化。
