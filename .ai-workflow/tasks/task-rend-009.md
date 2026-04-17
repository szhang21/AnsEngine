# 任务: TASK-REND-009 M6 Render MVP Uniform 渲染改造（合并 M6 Mesh 数据统一入口收敛）

## TaskId
`TASK-REND-009`

> 本卡吸收原 `TASK-REND-010` 的 mesh 数据统一入口收敛目标，Render 只保留这一张主执行卡。

## 目标（Goal）
将 `Engine.Render` 从 CPU 直接写最终裁剪空间顶点，升级为基于 shader uniform 执行 MVP 变换的最小真实渲染管线。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M6-2026-04-17`

## 里程碑引用（兼容别名：MilestoneRef）
`M6`

## 执行代理（ExecutionAgent）
Exec-Render

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Render

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M6-G2`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-CONTRACT-003`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - Render MVP 提交与 shader uniform 相关代码
  - mesh 数据统一入口收敛相关代码
  - 对应测试文件
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不修改 Scene 数据生产规则
- 不引入纹理系统/复杂材质系统
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Scene` 编译期依赖
  - CPU 直接写最终裁剪空间顶点路径继续作为主路径

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；MVP uniform 与 mesh 入口路径测试通过
- Smoke: 启动后可见矩阵驱动结果，且统一 mesh 输入入口下 demo mesh 仍可稳定退出
- Perf: 相比 M5 无明显退化

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Done

## 完成度（Completion）
`100`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## ExecutionStatus
- Status: `Done`
- Completion: `100`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-REND-009.md`
- ClosedAt: `2026-04-17 12:45`
- Summary:
  - `SceneRenderSubmissionBuilder` 收敛为统一 mesh 入口（`MeshId -> model-space vertices`），并产出逐批次 `ModelViewProjection` 提交。
  - `NullRenderer` 顶点 shader 引入 `uMvp` uniform，渲染路径切换为“模型空间顶点 + GPU 执行 MVP 变换”。
  - 新增/更新 Render 测试，覆盖 identity 回归、rotation 生效、多批次提交与 camera 影响路径。
- FilesChanged:
  - `src/Engine.Render/SceneRenderSubmission.cs`
  - `src/Engine.Render/RenderPlaceholders.cs`
  - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
  - `.ai-workflow/tasks/task-rend-009.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-REND-009.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`）
  - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.94s`）
- ModuleAttributionCheck: pass
