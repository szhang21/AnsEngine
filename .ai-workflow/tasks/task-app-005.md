# 任务: TASK-APP-005 M4b App 场景运行时抽象依赖修复

## TaskId
`TASK-APP-005`

## 目标（Goal）
将 `ApplicationHost` 对 `SceneGraphService` 的具体类型依赖改为最小场景运行时接口依赖（例如 `ISceneRuntime`），由组合根完成实现绑定。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M4B-2026-04-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M4-SceneRenderPipeline`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G7`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-004`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - ApplicationHost 与 RuntimeBootstrap 装配抽象化相关文件
  - App 模块对应测试文件
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在本卡改动渲染细节
- 不在本卡扩展 Scene 数据语义
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Asset/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Contracts`
- ForbiddenDependsOn:
  - `ApplicationHost` 对具体 `SceneGraphService` 的硬编码依赖

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；ApplicationHost 可由场景接口替身驱动
- Smoke: 启动后窗口/渲染行为与当前基线一致，关闭退出码 `0`
- Perf: 主循环无明显退化

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
- OriginTaskId: `TASK-APP-004`
- HumanSignoff: `pending`
