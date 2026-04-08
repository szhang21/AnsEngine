# 任务归档索引

> 本文件为已关闭任务的索引入口。详细快照请放在 `.ai-workflow/archive/<yyyy-mm>/<task-id>.md`。

## 字段约定

- TaskId
- Title
- Priority（P0/P1/P2/P3）
- PrimaryModule
- BoundaryContractPath
- Owner
- ClosedAt
- Status（Done/Cancelled）
- ModuleAttributionCheck（pass/fail）
- Summary
- FilesChanged
- ValidationEvidence
- SnapshotPath

## 归档记录

### 模板

```md
- TaskId: <TASK-ID>
  Title: <任务标题>
  Priority: <P0|P1|P2|P3>
  PrimaryModule: <Engine.Render|Engine.Scene|...>
  BoundaryContractPath: <.ai-workflow/boundaries/xxx.md>
  Owner: <name>
  ClosedAt: <YYYY-MM-DD HH:mm>
  Status: <Done|Cancelled>
  ModuleAttributionCheck: <pass|fail>
  Summary:
    - <改动点1>
    - <改动点2>
  FilesChanged:
    - <path1>
    - <path2>
  ValidationEvidence:
    - Build: <pass/fail + note>
    - Test: <pass/fail + note>
    - Smoke: <pass/fail + note>
    - Perf: <pass/fail + note>
  SnapshotPath: <.ai-workflow/archive/yyyy-mm/task-id.md>
```

### 当前记录

- TaskId: `TASK-BOOT-001`
  Title: 初始化 `AnsEngine` 多项目骨架
  Priority: `P1`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Codex`
  ClosedAt: `2026-04-06 20:33`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 建立 .NET 8 多项目骨架与 solution
    - 建立跨模块最小接口占位与组合根启动路径
    - 完成 Debug/Release build 与 test 门禁
  FilesChanged:
    - `AnsEngine.sln`
    - `src/**`
    - `tests/**`
    - `.ai-workflow/board.md`
    - `.ai-workflow/tasks/task-boot-001.md`
    - `.ai-workflow/archive/**`
  ValidationEvidence:
    - Build: pass（Debug/Release 均通过，存在环境级 CS1668 警告）
    - Test: pass（3 个测试项目各 1 条用例通过）
    - Smoke: N/A（未实现窗口循环）
    - Perf: N/A（未引入帧循环）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-BOOT-001.md`

- TaskId: `TASK-PLAT-001`
  Title: 真实窗口生命周期落地
  Priority: `P0`
  PrimaryModule: `Engine.Platform`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-platform.md`
  Owner: `Exec-Platform`
  ClosedAt: `2026-04-07 10:35`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 扩展 `IWindowService` 生命周期接口，补齐事件轮询、关闭请求和释放能力
    - 基于 OpenTK `NativeWindow` 实现真实窗口服务，替代纯占位行为
    - 提供 `useNativeWindow` 兼容开关，保障无图形环境测试稳定
    - 同步更新平台边界合同与 `Boundary Change Log`
  FilesChanged:
    - `src/Engine.Platform/PlatformContracts.cs`
    - `src/Engine.Platform/PlatformPlaceholders.cs`
    - `tests/Engine.Asset.Tests/AssetServiceTests.cs`
    - `.ai-workflow/boundaries/engine-platform.md`
  ValidationEvidence:
    - Build: pass（Debug/Release 均通过，存在环境级 CS1668 警告）
    - Test: pass（`dotnet test` 全部通过）
    - Smoke: pass（`dotnet run --project src/Engine.App/Engine.App.csproj` 成功退出）
    - Perf: pass（基础观察无卡死或阻塞）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-PLAT-001.md`

- TaskId: `TASK-APP-001`
  Title: 最小主循环与退出编排
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-07 11:10`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.App` 由单次 `Run` 升级为持续主循环并消费窗口关闭信号
    - 统一退出收口，保障渲染器与窗口服务按顺序释放
    - 增加自动退出开关用于 smoke/perf 自动化验证
    - 同步更新 `Engine.App` 边界合同与变更日志
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-001.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug` / `dotnet build -c Release`）
    - Test: pass（`dotnet test`）
    - Smoke: pass（自动退出验证，退出码 `0`）
    - Perf: pass（连续运行 30s+ 无明显阻塞）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-001.md`

- TaskId: `TASK-REND-001`
  Title: 最小清屏可视反馈
  Priority: `P1`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-07 12:20`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `NullRenderer` 初始化设置非默认清屏色，每帧执行 `GL.Clear`，提供持续可见背景反馈
    - 保持最小补丁，不引入三角形/Shader/Mesh 复杂渲染链路
    - 同步更新 `Engine.Render` 边界合同与变更日志
  FilesChanged:
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/tasks/task-rend-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-001.md`
  ValidationEvidence:
    - Build: pass（Debug/Release 通过，存在环境级 CS1668 警告）
    - Test: pass（`dotnet test`）
    - Smoke: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=30` 下稳定退出，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=45` 下运行约 55s，无明显阻塞）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-001.md`

- TaskId: `TASK-QA-001`
  Title: 可见反馈门禁证据补齐
  Priority: `P1`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-07 13:10`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 复核并补齐 M2 阶段 Build/Test/Smoke/Perf 门禁证据
    - 明确记录“可启动、可见、可退出”执行步骤与退出码结果
    - 同步更新 `Engine.App` 边界合同变更日志（验证口径）
  FilesChanged:
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-qa-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-001.md`
  ValidationEvidence:
    - Build: pass（Debug/Release 均通过）
    - Test: pass（`dotnet test`）
    - Smoke: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=10` 下稳定退出，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=45` 下运行约 55s，无明显异常）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-001.md`
