# 任务: TASK-QA-009 M7 门禁复验与关单收敛（含多对象与回退验证）

## TaskId
`TASK-QA-009`

## 目标（Goal）
完成 M7 全链路 Build/Test/Smoke/Perf 复验与门禁收敛（含最小多对象与回退路径验证），确认“最小真实资源输入”达到可关单标准。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M7-2026-04-18`

## 里程碑引用（兼容别名：MilestoneRef）
`M7`

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
- ParallelGroup: `M7-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-CONTRACT-004`
  - `TASK-REND-011`
  - `TASK-REND-012`
  - `TASK-SCENE-007`

## 范围（Scope）
- AllowedModules:
  - tests
  - workflow evidence
- AllowedFiles:
  - 全链路门禁验证证据与归档收敛
- AllowedPaths:
  - `src/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 若执行中需要为验证补充源码/测试代码，命名约定为：`private`/`protected` 字段使用 `camelCase`（禁止前导下划线），参数/局部变量使用 `camelCase`，公共类型/属性/方法使用 `PascalCase`。
- 若执行中新增类型或接口，默认“一类一文件、一接口一文件”；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在说明中标注。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不新增实现功能
- 不改动里程碑优先级

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - 已完成 M7 任务卡输出
- ForbiddenDependsOn:
  - 未覆盖多对象与回退验证即直接关单

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
- Test: `dotnet test` 全量通过；mesh/material 解析、回退路径与最小多对象回归通过
- Smoke: 至少一个对象通过真实 meshId + materialId 成功渲染；最小多对象路径稳定且可退出
- Perf: 相比 M6 无明显帧时间退化

## 验收标准（CodeQuality）
- NoNewHighRisk: `true`
- MustFixCount: `0`
- MustFixDisposition:
  - `N/A`

## 验收标准（DesignQuality）
- DQ-1: `Scene 仅输出资源标识，不承载资源解析`
- DQ-2: `Render 通过统一入口解析 mesh/material`
- DQ-3: `Render 不直接依赖 Scene`
- DQ-4: `回退与最小多对象路径都有证据覆盖`

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-009.md`
- ClosedAt: `2026-04-20 17:36`
- Summary:
  - 完成 M7 全链路 Build/Test/Smoke/Perf 门禁复验，结果均通过。
  - 完成 mesh/material 解析、多对象与回退路径专项回归，结果均通过。
  - 依赖与边界复验通过：Render 仅依赖 Contracts，Scene 仅输出资源标识并由 Render 统一解析。
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-009.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-009.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`，环境级 `CS1668` 警告）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`，环境级 `CS1668` 警告）
  - Test(All): pass（`dotnet test AnsEngine.sln -m:1`）
  - Test(Render): pass（`dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`，10/10）
  - Test(Scene): pass（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj -m:1`，7/7）
  - Test(Contracts): pass（`dotnet test tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj -m:1`，9/9）
  - Test(Fallback+MultiObject): pass（Render 3/3 + Scene 2/2）
  - DependencyGate: pass（`RenderSceneRef=absent`，`RenderContractsRef=present`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.61s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
  - CodeQuality: pass（NoNewHighRisk=true，MustFixCount=0）
  - DesignQuality: pass（DQ-1/DQ-2/DQ-3/DQ-4）
- ModuleAttributionCheck: pass
