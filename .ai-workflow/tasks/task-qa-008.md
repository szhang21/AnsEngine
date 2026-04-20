# 任务: TASK-QA-008 M7 多对象与回退路径验证

## TaskId
`TASK-QA-008`

## 目标（Goal）
完成 M7 最小多对象渲染与资源缺失回退路径验证，确保资源入口不只对单对象生效且异常引用可控。

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
- Engine.Scene
- Engine.Render
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`
- `.ai-workflow/boundaries/engine-render.md`
- `.ai-workflow/boundaries/engine-contracts.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M7-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-007`

## 范围（Scope）
- AllowedModules:
  - tests
  - workflow evidence
- AllowedFiles:
  - 多对象与回退路径测试与证据
- AllowedPaths:
  - `src/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 若执行中需要为验证补充源码/测试代码，命名约定为：`private`/`protected` 字段使用 `camelCase`（禁止前导下划线），参数/局部变量使用 `camelCase`，公共类型/属性/方法使用 `PascalCase`。
- 若执行中新增类型或接口，默认“一类一文件、一接口一文件”；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在说明中标注。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不新增业务功能
- 不调整计划优先级

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - 已完成 M7 实现卡输出
- ForbiddenDependsOn:
  - 实现卡未完成即推进关单

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
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；多对象与资源回退路径测试通过
- Smoke: 最小多对象渲染稳定，缺失资源触发预期回退且可退出
- Perf: 相比 M6 无明显退化

## 验收标准（CodeQuality）
- NoNewHighRisk: `true`
- MustFixCount: `0`
- MustFixDisposition:
  - `N/A`

## 验收标准（DesignQuality）
- DQ-1: `meshId/materialId 为真实生效字段`
- DQ-2: `回退策略明确且可测试`
- DQ-3: `Render 不直接依赖 Scene`
- DQ-4: `多对象路径与单对象路径一致可用`

## 交付物（Deliverables）
- Gate evidence summary
- Regression checklist
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Cancelled

## 完成度（Completion）
`0`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`

## Archive
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-008.md`
- ClosedAt: `2026-04-18 00:52`
- Summary:
  - 已并入 `TASK-QA-009`，改为单 QA 主卡收口
  - 多对象与回退路径验证并入 `TASK-QA-009` 的目标与验收项
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-008.md`
  - `.ai-workflow/tasks/task-qa-009.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: N/A（任务卡并入，无独立实现）
  - Test: N/A（任务卡并入，无独立实现）
  - Smoke: N/A（任务卡并入，无独立实现）
  - Perf: N/A（任务卡并入，无独立实现）
