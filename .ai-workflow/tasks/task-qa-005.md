# 任务: TASK-QA-005 M4b MustFix 关口复验与双轨门禁收口

## TaskId
`TASK-QA-005`

## 目标（Goal）
对 `TASK-SCENE-004`、`TASK-REND-007`、`TASK-APP-005` 三张 MustFix 修复卡进行统一复验，确保契约出口、装配依赖与渲染注入门禁全部闭环。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M4B-2026-04-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M4-SceneRenderPipeline`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
QA

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Render
- Engine.App

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G8`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-004`
  - `TASK-REND-007`
  - `TASK-APP-005`

## 范围（Scope）
- AllowedModules:
  - tests
  - workflow evidence
- AllowedFiles:
  - 测试与门禁证据文件
  - QA 记录与归档证据相关文件
- AllowedPaths:
  - `src/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现业务代码功能新增
- 不调整计划优先级

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - 已完成 MustFix 修复卡结果
- ForbiddenDependsOn:
  - 跳过 MustFix 直接验收关单

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 全量通过；新增/调整测试覆盖三类缺陷位点
- Smoke: 可启动、可见、可退出；连续运行 30-60 秒稳定
- Perf: 与当前基线相比无明显退化

## 验收标准（CodeQuality）
- NoNewHighRisk: `true`
- MustFixCount: `3`
- MustFixDisposition:
  - `Scene 双轨契约出口已收敛到 Engine.Contracts`
  - `ApplicationHost 已改为场景最小接口依赖`
  - `Render 生产路径已移除默认 provider 回退`

## 验收标准（DesignQuality）
- DQ-1: `Render 不感知 Scene 语义，仅消费契约输入`
- DQ-2: `组合根负责装配，运行时依赖方向清晰`
- DQ-3: `门禁可显式暴露漏注入与依赖回退`
- DQ-4: `边界文档与实现保持一致并有变更记录`

## 交付物（Deliverables）
- Gate evidence summary
- Must-fix verification checklist
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Done

## 完成度（Completion）
`100`

## 缺陷回流字段（Defect Triage）
- FailureType: `PostAcceptanceBug`
- DetectedAt: `2026-04-14`
- ReopenReason:
- OriginTaskId: `TASK-QA-004`
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-005.md`
- ClosedAt: `2026-04-14 17:19`
- Summary:
  - 完成 `TASK-SCENE-004`、`TASK-REND-007`、`TASK-APP-005` 三张 MustFix 修复卡统一复验。
  - 完成 Build/Test/Smoke/Perf 与依赖门禁复核，确认解耦与注入关口均闭环。
  - QA 质量结论：NoNewHighRisk=true，三项 MustFix 均已按处置项落地。
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-005.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-005.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`，0 警告 0 错误）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`，0 警告 0 错误）
  - Test: pass（`dotnet test AnsEngine.sln -m:1`，全部测试通过）
  - DependencyGate: pass（`RenderSceneRef=absent`，`RenderContractsRef=present`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`31.15s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，`ExitCode=0`，`46.09s`）
  - CodeQuality: pass（NoNewHighRisk=true，MustFixCount=3，三项处置均已落地）
  - DesignQuality: pass（DQ-1/DQ-2/DQ-3/DQ-4）
- ModuleAttributionCheck: pass
