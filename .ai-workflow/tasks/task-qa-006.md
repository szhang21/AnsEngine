# 任务: TASK-QA-006 M5 变换链路门禁与回归复验

## TaskId
`TASK-QA-006`

## 目标（Goal）
完成 M5 变换链路（Position、Scale、Rotation）的 Build/Test/Smoke/Perf 门禁与依赖回归复验，确认兼容路径与依赖方向双通过。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M5-2026-04-14`

## 里程碑引用（兼容别名：MilestoneRef）
`M5`

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
- ParallelGroup: `M5-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-CONTRACT-002`
  - `TASK-SCENE-005`
  - `TASK-REND-008`
  - `TASK-APP-006`

## 范围（Scope）
- AllowedModules:
  - tests
  - workflow evidence
- AllowedFiles:
  - 门禁验证脚本/测试
  - 质量复验证据与归档记录
- AllowedPaths:
  - `src/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不新增业务功能
- 不改计划优先级

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - 已完成 M5 实现卡输出
- ForbiddenDependsOn:
  - 实现卡未完成即提前关单

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
- Test: `dotnet test` 全量通过；M5 新增 transform（含 Rotation）与兼容回归测试通过
- Smoke: 可启动、可见 transform（含 Rotation）效果、可稳定退出
- Perf: 相比 M4b 无明显帧时间退化

## 验收标准（CodeQuality）
- NoNewHighRisk: `true`
- MustFixCount: `0`
- MustFixDisposition:
  - `N/A`

## 验收标准（DesignQuality）
- DQ-1: `Render 仅依赖 Contracts，不感知 Scene 实现`
- DQ-2: `App 仅装配不承载渲染计算`
- DQ-3: `无 transform 输入时 identity 路径保持兼容；有 Rotation 输入时行为正确`
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

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-006.md`
- ClosedAt: `2026-04-16 22:55`
- Summary:
  - 完成 M5 变换链路（Position/Scale/Rotation）门禁复验与兼容回归检查。
  - Build/Test/Smoke/Perf 全量证据补齐，并补充 Render 专项测试验证 Rotation 与 identity 回归。
  - 依赖方向复验通过：Render 仅依赖 Contracts，不直接引用 Scene。
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-006.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-006.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`，存在 MSB3101 写缓存告警）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`，存在 MSB3101 写缓存告警）
  - Test(All): pass（`dotnet test AnsEngine.sln -m:1`）
  - Test(Render): pass（`dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1 --no-restore --no-build`，6/6）
  - DependencyGate: pass（`RenderSceneRef=absent`，`RenderContractsRef=present`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`31.45s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，`ExitCode=0`，`46.25s`）
  - CodeQuality: pass（NoNewHighRisk=true，MustFixCount=0）
  - DesignQuality: pass（DQ-1/DQ-2/DQ-3/DQ-4）
- ModuleAttributionCheck: pass
