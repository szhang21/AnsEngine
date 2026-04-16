# 任务: TASK-SCENE-006 M6 Scene 对象与相机语义输出

## TaskId
`TASK-SCENE-006`

## 目标（Goal）
在 `Engine.Scene` 中输出“对象 transform + 最小 camera/view/projection”渲染输入，确保视图参数变化可真实影响画面。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M6-2026-04-17`

## 里程碑引用（兼容别名：MilestoneRef）
`M6`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M6-G2`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-CONTRACT-003`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - Scene 渲染帧输出与相机语义组装
  - 对应测试文件
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不修改 Render shader 实现
- 不修改 App 主循环逻辑
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.App/**`
  - `src/Engine.Platform/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Render` 内部实现

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
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；Scene 输出对象+相机语义测试通过
- Smoke: 修改相机或对象变换时画面按预期变化
- Perf: 与 M5 相比无明显退化

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Todo

## 完成度（Completion）
`0`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`
