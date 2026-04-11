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

- TaskId: `TASK-REND-002`
  Title: 首帧三角形最小渲染链路
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-11 10:30`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 任务经修复后再次提交人工验收，确认“稳定可见三角形”达成
    - 关闭流程完成：任务卡、归档快照、归档索引与看板状态已对齐
    - 保留回流与复验记录，满足审计追溯
  FilesChanged:
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/tasks/task-rend-002.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-002.md`
  ValidationEvidence:
    - Build: pass（Debug/Release 通过，存在环境级 CS1668 警告）
    - Test: pass（`dotnet test`，首次因锁文件失败后重试通过）
    - Smoke: pass（人工复验通过：稳定可见三角形并可正常退出）
    - Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，运行约 48.68s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-002.md`

- TaskId: `TASK-APP-002`
  Title: M3 运行装配与生命周期配套
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-11 10:45`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 将渲染初始化纳入统一 `try/finally`，初始化失败也可执行收口
    - 异常路径补充 `RequestClose`，避免窗口生命周期悬挂
    - 增加 `ANS_ENGINE_USE_NATIVE_WINDOW` 装配开关，默认仍为真实窗口路径
    - 新增 `HeadlessRenderer` 适配无图形环境 smoke/perf 验证
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-002.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-002.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，约 `15.64s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，约 `45.14s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-002.md`

- TaskId: `TASK-SCENE-002`
  Title: M4 最小场景渲染数据输出
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-11 11:40`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 场景提交数据增加最小动态变化（首节点材质按帧切换）
    - 保持 Scene-Render 契约稳定，不扩展跨模块依赖面
    - 补充场景输出测试并通过
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-002.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-002.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 4/4）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.15s，退出码 `0`）
    - Perf: pass（45.12s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-002.md`

- TaskId: `TASK-REND-004`
  Title: M4 场景驱动渲染消费
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-11 11:40`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 渲染模块改为消费 `SceneRenderFrame`，替代固定 demo 提交路径
    - 新增提交构建器并按场景提交动态更新 VBO 绘制
    - 新增渲染消费链路最小测试项目并通过
  FilesChanged:
    - `src/Engine.Render/Engine.Render.csproj`
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/Engine.Render.Tests.csproj`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/tasks/task-rend-004.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.15s，退出码 `0`）
    - Perf: pass（45.12s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-004.md`

- TaskId: `TASK-APP-003`
  Title: M4 提交流程编排配套
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-11 11:55`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 组合根注入 Scene 提交提供器到 Render，完成 M4 场景提交链路编排
    - 主循环保持编排职责边界，不引入渲染后端或场景内部实现
    - 同步更新 App 边界合同与任务归档资料
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-003.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-003.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: fail -> pass（`dotnet test -m:1` 首次 `CS2012`，清理进程后复跑通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.15s，退出码 `0`）
    - Perf: pass（45.15s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-003.md`

- TaskId: `TASK-QA-003`
  Title: M4 验证与关单收敛
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-11 12:05`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M4 Build/Test/Smoke/Perf 全量门禁复核与证据回填
    - 纳入 `Engine.Render.Tests` 独立测试链路验证并通过
    - 质量结论收敛：NoNewHighRisk=true，MustFixCount=0
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-003.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-003.md`
    - `.ai-workflow/boundaries/engine-app.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.14s，退出码 `0`）
    - Perf: pass（45.13s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-003.md`

- TaskId: `TASK-QA-002`
  Title: M3 双轨门禁证据与归档收口
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-11 11:00`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M3 Build/Test/Smoke/Perf 证据复核与汇总
    - 对齐 `TASK-REND-002`（可视验收）与 `TASK-APP-002`（运行收口）的联合验收链
    - 完成归档三件套与看板 Review 推进，待 Human 最终关单
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-002.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-002.md`
    - `.ai-workflow/boundaries/engine-app.md`
  ValidationEvidence:
    - Build: fail -> pass（Debug 首次 `CS2012` 文件占用；复跑 Debug 通过；Release 通过）
    - Test: pass（`dotnet test -m:1`）
    - Smoke: pass（图形口径引用 `TASK-REND-002` 人工复验；当前环境口径 30.19s，退出码 `0`）
    - Perf: pass（45.24s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-002.md`

- TaskId: `TASK-SCENE-001`
  Title: M4 Scene-Render 最小契约定义
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-11 11:25`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 在 `Engine.Scene` 定义最小 Scene-Render 提交契约基线
    - `SceneGraphService` 输出只读提交快照，维持模块边界不侵入渲染实现
    - 新增契约测试并通过，形成 M4 后续任务统一依赖面
  FilesChanged:
    - `src/Engine.Scene/SceneRenderContracts.cs`
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-001.md`
  ValidationEvidence:
    - Build: fail -> pass（Debug 首次 `CS2012` 文件占用；复跑 Debug 通过；Release 通过）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 3/3 通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.22s，退出码 `0`）
    - Perf: pass（45.17s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-001.md`


