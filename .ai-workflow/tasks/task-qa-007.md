# 任务: TASK-QA-007 M6 MVP 渲染链路门禁与回归复验

## TaskId
`TASK-QA-007`

## 目标（Goal）
完成 M6 MVP 渲染链路的 Build/Test/Smoke/Perf 门禁与依赖回归复验，确认矩阵驱动路径与边界约束双通过。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M6-2026-04-17`

## 里程碑引用（兼容别名：MilestoneRef）
`M6`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
QA

## 次级模块（SecondaryModules）
- Engine.Contracts
- Engine.Scene
- Engine.Render
- Engine.App

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-contracts.md`
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M6-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-CONTRACT-003`
  - `TASK-SCENE-006`
  - `TASK-REND-009`
  - `TASK-REND-010`
  - `TASK-APP-007`

## 范围（Scope）
- AllowedModules:
  - tests
  - workflow evidence
- AllowedFiles:
  - 门禁验证测试与证据记录
  - QA 复验归档相关文件
- AllowedPaths:
  - `src/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不新增业务功能
- 不重排计划优先级

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - 已完成 M6 实现卡结果
- ForbiddenDependsOn:
  - 实现卡未完成即关单

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 全量通过；MVP/Camera/mesh 入口回归测试通过
- Smoke: 可启动、可见矩阵驱动结果、可稳定退出
- Perf: 相比 M5 无明显帧时间退化

## 验收标准（CodeQuality）
- NoNewHighRisk: `true`
- MustFixCount: `0`
- MustFixDisposition:
  - `N/A`

## 验收标准（DesignQuality）
- DQ-1: `Render 仅依赖 Contracts，不直接依赖 Scene`
- DQ-2: `CPU 不再作为最终裁剪空间顶点主计算路径`
- DQ-3: `模型变换与相机变更可真实影响画面`
- DQ-4: `边界文档与实现一致`

## 交付物（Deliverables）
- Gate evidence summary
- Regression checklist
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

## Archive
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-007.md`
- ClosedAt: `2026-04-17 17:50`
- Summary:
  - 完成 M6 MVP 渲染链路（含 Camera 与 mesh 统一入口）的 Build/Test/Smoke/Perf 全量门禁复验。
  - 回归验证通过：Render 仅依赖 Contracts、不直接依赖 Scene；MVP uniform 路径在渲染主链路生效。
  - 边界文档与实现一致性复核通过，未发现新的高风险问题。
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-007.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-007.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`）
  - Test(All): pass（`dotnet test AnsEngine.sln -m:1`）
  - Test(Render): pass（`dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`，7/7）
  - Test(Contracts): pass（`dotnet test tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj -m:1`，6/6）
  - Test(Scene): pass（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj -m:1`，5/5）
  - DependencyGate: pass（`RenderSceneRef=absent`，`RenderContractsRef=present`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`16.22s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.80s`）
  - CodeQuality: pass（NoNewHighRisk=true，MustFixCount=0）
  - DesignQuality: pass（DQ-1/DQ-2/DQ-3/DQ-4）
- ModuleAttributionCheck: pass
