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

- TaskId: `TASK-REND-005`
  Title: M4 渲染边界文档对齐当前依赖
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-13 20:38`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 对齐 Render 边界文档到当前真实依赖状态（当前为 `Engine.Render -> Engine.Scene`）
    - 修正 `Engine.Contracts` 依赖表述为后续目标态，消除文档与实现漂移
    - 记录后续解耦回切条件与路径
  FilesChanged:
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/tasks/task-rend-005.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-005.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，15.17s，退出码 `0`）
    - Perf: pass（30.17s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-005.md`

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

- TaskId: `TASK-CONTRACT-001`
  Title: M4 独立契约层建立与边界落盘
  Priority: `P0`
  PrimaryModule: `Engine.Contracts`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-13 21:00`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新建独立契约层 `src/Engine.Contracts` 并定义最小渲染输入契约类型
    - 新建 `Engine.Contracts.Tests` 并补充最小契约用例，验证契约可直接消费
    - 同步边界文档 `engine-contracts` 变更日志，完成归档三件套准备
  FilesChanged:
    - `AnsEngine.sln`
    - `src/Engine.Contracts/Engine.Contracts.csproj`
    - `src/Engine.Contracts/SceneRenderContracts.cs`
    - `tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj`
    - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/tasks/task-contract-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-CONTRACT-001.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Contracts.Tests` 2/2）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，22.82s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，37.83s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-001.md`

- TaskId: `TASK-SCENE-003`
  Title: M4 渲染输入契约下沉到独立层
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-14 00:40`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.Scene` 接入 `Engine.Contracts` 引用，建立 `Scene -> Contracts` 稳定依赖方向
    - `SceneGraphService` 完成双接口适配，内部以契约层类型输出并保持旧接口兼容
    - 新增契约接口链路测试并通过，同步更新 Scene 边界变更日志
  FilesChanged:
    - `src/Engine.Scene/Engine.Scene.csproj`
    - `src/Engine.Scene/SceneRenderContracts.cs`
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-003.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-003.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，24.74s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，62.53s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-003.md`

- TaskId: `TASK-REND-006`
  Title: M4 Render 依赖反转与解耦
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-14 00:49`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.Render` 编译期依赖由 `Engine.Scene` 切换到 `Engine.Contracts`
    - `Render` 提交构建链路统一消费契约层 `SceneRenderFrame/SceneRenderItem`
    - `Engine.Render.Tests` 切换契约层引用并验证消费路径通过
  FilesChanged:
    - `src/Engine.Render/Engine.Render.csproj`
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/Engine.Render.Tests.csproj`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-rend-006.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-006.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，Render 契约消费测试通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，20.68s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，35.66s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-006.md`

- TaskId: `TASK-APP-004`
  Title: M4 App 契约 Provider 装配
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-14 01:06`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.App` 显式装配 `Engine.Contracts.ISceneRenderContractProvider` 并注入 Render
    - 抽出可测试渲染器创建路径，规避 GLFW 主线程限制
    - 新增 `Engine.App.Tests` 覆盖装配路径并通过
  FilesChanged:
    - `AnsEngine.sln`
    - `src/Engine.App/Engine.App.csproj`
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `tests/Engine.App.Tests/Engine.App.Tests.csproj`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-004.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.App.Tests` 1/1）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，18.88s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，33.85s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-004.md`

- TaskId: `TASK-QA-004`
  Title: M4 解耦门禁与质量复验
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-14 01:28`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 “Render 不得直接引用 Scene” 的门禁复验并通过
    - 完成 Build/Test/Smoke/Perf 全量复核并回填证据
    - 质量结论收敛：NoNewHighRisk=true，MustFixCount=0
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-004.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.87s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，45.83s，退出码 `0`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-004.md`

- TaskId: `TASK-QA-005`
  Title: M4b MustFix 关口复验与双轨门禁收口
  Priority: `P0`
  PrimaryModule: `QA`
  BoundaryContractPath:
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-14 17:19`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成三张 MustFix 修复卡统一复验并形成关口收口证据
    - Build/Test/Smoke/Perf 全量门禁通过
    - 依赖门禁通过：Render 不直接引用 Scene，仅消费 Contracts
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-005.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-005.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，31.15s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，46.09s，退出码 `0`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-005.md`

- TaskId: `TASK-SCENE-004`
  Title: M4b Scene 单契约出口收敛
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-14 10:43`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Scene 渲染出口收敛为单一 `Engine.Contracts` 契约，移除双轨兼容层
    - 删除 `SceneRenderContracts.cs` 并清除 `FromContracts` 每帧转换路径
    - 保持场景输出行为一致，同时降低热路径分配与语义分叉风险
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `src/Engine.Scene/SceneRenderContracts.cs`（删除）
    - `.ai-workflow/tasks/task-scene-004.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，26.59s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，41.48s，退出码 `0`）
    - HotPathEvidence: pass（`src/Engine.Scene` 无 `FromContracts` 调用，仅单契约构建）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-004.md`

- TaskId: `TASK-REND-007`
  Title: M4b Render 默认回退 Provider 清理
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-14 15:24`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 移除 `NullRenderer` 默认回退 provider 构造入口，渲染器必须显式注入 `ISceneRenderContractProvider`
    - 删除 `DefaultSceneRenderContractProvider`，防止生产路径静默兜底掩盖装配遗漏
    - 新增漏注入失败测试，验证 provider 为 `null` 时快速失败并抛出可诊断异常
  FilesChanged:
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/NullRendererTests.cs`
    - `.ai-workflow/tasks/task-rend-007.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-007.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`，存在环境级 CS1668 警告）
    - Test: pass（`dotnet test -m:1`，`NullRendererTests` 通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`51.83s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`67.36s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-007.md`

- TaskId: `TASK-APP-005`
  Title: M4b App 场景运行时抽象依赖修复
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-14 17:14`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 在 `Engine.App` 新增 `ISceneRuntime` 并将 `ApplicationHost` 改为依赖该接口
    - 组合根新增 `SceneRuntimeAdapter` 绑定 `SceneGraphService`，保持 Render 契约 provider 注入不变
    - 新增抽象驱动测试，验证主循环可在场景替身下完成初始化与退出
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `src/Engine.App/SceneRuntimeContracts.cs`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/tasks/task-app-005.md`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-005.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.App.Tests` 2/2）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`18.63s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`32.37s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-005.md`

- TaskId: `TASK-CONTRACT-002`
  Title: M5 渲染变换契约兼容扩展
  Priority: `P0`
  PrimaryModule: `Engine.Contracts`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
  Owner: `Exec-Contracts`
  ClosedAt: `2026-04-15 13:46`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `SceneTransform`（Position/Scale/Rotation）并提供 `Identity`
    - `SceneRenderItem` 扩展 `Transform` 字段并保留旧三参构造默认 identity 兼容行为
    - 新增契约测试覆盖默认兼容路径与 Rotation 保真路径
  FilesChanged:
    - `src/Engine.Contracts/SceneRenderContracts.cs`
    - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
    - `.ai-workflow/tasks/task-contract-002.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-CONTRACT-002.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Contracts.Tests` 4/4）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`19.37s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`34.38s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-002.md`

- TaskId: `TASK-SCENE-005`
  Title: M5 Scene 变换渲染帧输出
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-15 19:32`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneGraphService` 输出项显式携带 `SceneTransform`，首帧保持 identity 兼容
    - 连续帧对首节点输出轻量 transform 动态（Position.X 与 Rotation.Yaw）
    - 场景测试补充 transform/rotation 验证并通过
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/tasks/task-scene-005.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-005.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`25.50s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`40.36s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-005.md`

- TaskId: `TASK-REND-008`
  Title: M5 Render 变换消费与提交应用
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-15 19:32`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 提交顶点应用 `Scale -> Rotation -> Translation` 变换
    - identity 路径保持旧布局兼容
    - 渲染测试新增 identity 回归与 rotation 生效断言并通过
  FilesChanged:
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/tasks/task-rend-008.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-008.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Render.Tests` 6/6）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`25.50s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`40.36s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-008.md`

- TaskId: `TASK-APP-006`
  Title: M5 App 装配兼容校准
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-15 15:23`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 保持 App 仅装配不计算边界，维持 `Scene -> Contracts -> Render` 链路
    - 补充装配测试校准，验证 provider 初始化后可输出含 rotation 的连续帧 transform
    - 复核 M5 链路下主循环生命周期稳定性
  FilesChanged:
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/tasks/task-app-006.md`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-006.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`）
    - Smoke: pass（`dotnet run --no-build` + `ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.80s`）
    - Perf: pass（`dotnet run --no-build` + `ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.80s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-006.md`

- TaskId: `TASK-CONTRACT-003`
  Title: M6 相机与 MVP 最小契约扩展
  Priority: `P0`
  PrimaryModule: `Engine.Contracts`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
  Owner: `Exec-Contracts`
  ClosedAt: `2026-04-17 10:58`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `SceneCamera(View, Projection)` 契约并提供 identity 默认值
    - `SceneRenderFrame` 扩展 `Camera` 字段并保留旧双参构造兼容路径
    - 新增契约测试覆盖默认相机兼容与显式相机保真
  FilesChanged:
    - `src/Engine.Contracts/SceneRenderContracts.cs`
    - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
    - `.ai-workflow/tasks/task-contract-003.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-CONTRACT-003.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Contracts.Tests` 6/6）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`19.04s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`34.21s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-003.md`

- TaskId: `TASK-QA-006`
  Title: M5 变换链路门禁与回归复验（含 Rotation）
  Priority: `P1`
  PrimaryModule: `QA`
  BoundaryContractPath:
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-16 22:55`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M5 变换链路（Position/Scale/Rotation）门禁复验与兼容回归检查
    - Build/Test/Smoke/Perf 全量证据补齐，并补充 Render 专项测试
    - 依赖方向复验通过：Render 仅依赖 Contracts，不直接引用 Scene
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-006.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-006.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`，存在 MSB3101 写缓存告警）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1 --no-restore --no-build`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，31.45s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，46.25s，退出码 `0`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-006.md`

- TaskId: `TASK-REND-010`
  Title: M6 Mesh 数据统一入口收敛（已并入 TASK-REND-009）
  Priority: `P1`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-17 00:47`
  Status: `Cancelled`
  ModuleAttributionCheck: `pass`
  Summary:
    - 已并入 `TASK-REND-009`，不再作为独立执行卡
    - M6 Render 只保留一个主执行单元，mesh 数据统一入口收敛收进主卡范围
  FilesChanged:
    - `.ai-workflow/tasks/task-rend-010.md`
    - `.ai-workflow/tasks/task-rend-009.md`
    - `.ai-workflow/plan-archive/2026-04/PLAN-M6-2026-04-17.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: N/A（卡片合并，无独立实现）
    - Test: N/A（卡片合并，无独立实现）
    - Smoke: N/A（卡片合并，无独立实现）
    - Perf: N/A（卡片合并，无独立实现）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-010.md`

- TaskId: `TASK-QA-007`
  Title: M6 MVP 渲染链路门禁与回归复验
  Priority: `P1`
  PrimaryModule: `QA`
  BoundaryContractPath:
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-17 17:50`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M6 MVP 渲染链路（含 Camera 与 mesh 统一入口）的 Build/Test/Smoke/Perf 全量门禁复验
    - 回归验证通过：Render 仅依赖 Contracts、不直接依赖 Scene；MVP uniform 路径在渲染主链路生效
    - 边界文档与实现一致性复核通过，未发现新的高风险问题
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-007.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-007.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + M6 关键专项测试通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`16.22s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.80s`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-007.md`

- TaskId: `TASK-QA-009`
  Title: M7 门禁复验与关单收敛（含多对象与回退验证）
  Priority: `P1`
  PrimaryModule: `QA`
  BoundaryContractPath:
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-20 17:36`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M7 全链路 Build/Test/Smoke/Perf 门禁复验，结果均通过
    - 完成 mesh/material 解析、多对象与回退路径专项回归，结果均通过
    - 依赖与边界复验通过：Render 仅依赖 Contracts，Scene 仅输出资源标识并由 Render 统一解析
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-009.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-009.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`，环境级 `CS1668` 警告）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + fallback/multi-object 专项测试通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.61s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-009.md`

- TaskId: `TASK-SCENE-006`
  Title: M6 Scene 对象与相机语义输出
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-17 12:45`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Scene 输出帧新增 `SceneCamera(View/Projection)`，补齐最小真实相机语义
    - 保持 transform/material 动态输出，并新增轻量相机视图变化
    - 场景测试补充相机语义与有效性断言
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/tasks/task-scene-006.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-006.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.94s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-006.md`

- TaskId: `TASK-REND-009`
  Title: M6 Render MVP Uniform 渲染改造（合并 M6 Mesh 数据统一入口收敛）
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-17 12:45`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Render 提交链路改为“模型空间顶点 + MVP uniform”并由 shader 执行变换
    - 收敛 `MeshId` 统一 mesh 入口，吸收 `TASK-REND-010` 目标
    - Render 测试补齐 identity/rotation/camera/multi-batch 回归
  FilesChanged:
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/tasks/task-rend-009.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-009.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.94s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-009.md`

- TaskId: `TASK-APP-007`
  Title: M6 App 装配与生命周期校准
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-17 13:20`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 保持 App “仅装配不计算”边界，M6 MVP 计算仍由 Scene/Render 消费
    - 装配测试新增相机语义校准断言（连续帧 `Camera.View` 变化）
    - 生命周期测试覆盖渲染异常收口（`RequestClose -> Shutdown -> Dispose`）
  FilesChanged:
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/tasks/task-app-007.md`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-007.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.63s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-007.md`

- TaskId: `TASK-CONTRACT-004`
  Title: M7 资源输入契约收敛
  Priority: `P0`
  PrimaryModule: `Engine.Contracts`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
  Owner: `Exec-Contracts`
  ClosedAt: `2026-04-18 11:05`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增结构化资源引用契约 `SceneMeshRef` 与 `SceneMaterialRef`
    - `SceneRenderItem` 支持结构化资源构造并保留 `meshId/materialId` 兼容路径
    - 合同测试覆盖结构化资源兼容与非法标识拦截
  FilesChanged:
    - `src/Engine.Contracts/SceneResourceContracts.cs`
    - `src/Engine.Contracts/SceneRenderContracts.cs`
    - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
    - `.ai-workflow/tasks/task-contract-004.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-CONTRACT-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.79s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-004.md`

- TaskId: `TASK-REND-011`
  Title: M7 Mesh 数据入口落地
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-18 00:58`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneRenderSubmissionBuilder` 主路径统一走 `ResolveMesh(meshId)`，收敛 mesh 数据解析入口
    - 未知 mesh 标识回退到默认三角形，避免渲染链路中断
    - Render 测试补充 mesh 命中与回退断言
  FilesChanged:
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/tasks/task-rend-011.md`
    - `.ai-workflow/tasks/task-rend-012.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-011.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-012.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.70s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-011.md`

- TaskId: `TASK-REND-012`
  Title: M7 Material 参数入口落地
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-18 00:58`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneRenderSubmissionBuilder` 新增 `ResolveMaterial(materialId)` 最小材质参数入口
    - 以显式映射替换 hash 派生颜色语义，支持可观察的材质差异
    - 对未知材质启用默认参数回退，保证渲染稳定性
  FilesChanged:
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/tasks/task-rend-011.md`
    - `.ai-workflow/tasks/task-rend-012.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-011.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-012.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.70s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-012.md`

- TaskId: `TASK-SCENE-007`
  Title: M7 Scene 资源引用输出对齐
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-19 21:15`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Scene 资源输出新增候选值解析与回退，`meshId/materialId` 对齐 Render M7 已支持入口
    - 材质周期覆盖 `default/pulse/highlight`，缺失材质在 Scene 侧回退为 `default`
    - 多对象路径下缺失 mesh 不外泄，输出统一回退到 `mesh://triangle`
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/tasks/task-scene-007.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-007.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1`，`Engine.Scene.Tests` 7/7）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`16.07s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.84s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-007.md`


