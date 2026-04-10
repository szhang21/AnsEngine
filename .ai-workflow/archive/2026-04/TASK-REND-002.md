# TASK-REND-002 归档快照

- TaskId: `TASK-REND-002`
- Title: 首帧三角形最小渲染链路
- Priority: `P0`
- PrimaryModule: `Engine.Render`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
- BaselineRef: `references/project-baseline.md`
- ExecutionAgent: `Exec-Render`
- ClosedAt: `2026-04-08 10:20`
- Status: `Done`
- Completion: `100`
- ModuleAttributionCheck: `pass`

## Scope

- AllowedModules:
  - `Engine.Render`
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## Summary

- 在 `NullRenderer` 内新增最小三角形渲染链路：
  - 顶点/片元 shader 编译与程序链接；
  - VBO/VAO 顶点提交；
  - 每帧 `DrawArrays` 绘制。
- 保留非默认清屏作为背景反馈，并在每帧 `GL.Clear` 后执行 draw。
- 在 `Shutdown` 中释放 program/shader/VAO/VBO，保证最小资源生命周期完整。
- 同步更新 `Engine.Render` 边界合同变更日志。

## FilesChanged

- `src/Engine.Render/RenderPlaceholders.cs`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/tasks/task-rend-002.md`
- `.ai-workflow/board.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/archive/2026-04/TASK-REND-002.md`

## ValidationEvidence

- Build(Debug): pass（`dotnet build -c Debug`）
- Build(Release): pass（`dotnet build -c Release`）
- Test: pass（`dotnet test`；首次锁文件失败，清理进程后重试通过）
- Smoke: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，运行约 34.95 秒，退出码 `0`）
- Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，运行约 48.68 秒，退出码 `0`）

## Risks

- RiskLevel: `medium`
- Notes:
  - 当前未通过自动化截图直接证明“三角形像素可见”，主要凭链路执行与稳定运行证据；后续可补充帧缓冲读回或截图校验测试。
