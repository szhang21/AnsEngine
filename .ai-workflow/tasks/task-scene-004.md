# 任务: TASK-SCENE-004 M4b Scene 单契约出口收敛

## TaskId
`TASK-SCENE-004`

## 目标（Goal）
将 `Engine.Scene` 渲染输出收敛为单一 `Engine.Contracts` 契约出口，移除热路径双轨转换（`Engine.Scene` 与 `Engine.Contracts` 并存）带来的分配与语义分叉风险。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M4B-2026-04-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M4-SceneRenderPipeline`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Contracts
- Engine.Render

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G5`
- CanRunParallel: `false`
- DependsOn: `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - `SceneGraphService` 渲染帧构建与契约出口相关文件
  - 场景模块对应测试文件
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在本卡实现新渲染特性
- 不改动 `Engine.Render` 绘制算法
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
- Test: `dotnet test` 通过；Scene 契约出口相关测试通过
- Smoke: 启动后首帧渲染行为与当前基线一致（无回退）
- Perf: 热路径不再存在每帧双轨转换分配（以基准日志或剖析证据说明）

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
- FailureType: `PostAcceptanceBug`
- DetectedAt: `2026-04-14`
- ReopenReason:
- OriginTaskId: `TASK-SCENE-003`
- HumanSignoff: `pending`
