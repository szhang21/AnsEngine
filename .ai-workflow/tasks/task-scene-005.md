# 任务: TASK-SCENE-005 M5 Scene 变换渲染帧输出

## TaskId
`TASK-SCENE-005`

## 目标（Goal）
在 `Engine.Scene` 侧产出包含 transform（Position、Scale、Rotation）的渲染输入帧，保持与 M5 契约扩展一致。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M5-2026-04-14`

## 里程碑引用（兼容别名：MilestoneRef）
`M5`

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
- ParallelGroup: `M5-G2`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-CONTRACT-002`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - Scene 渲染帧生成与 transform（含 Rotation）赋值
  - Scene 对应测试
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不改 Render 提交消费逻辑
- 不改 App 主循环编排
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
- Test: `dotnet test` 通过，Scene transform（含 Rotation）输出测试通过
- Smoke: 启动后可观察到 transform（含 Rotation）输入被稳定提交（无崩溃）
- Perf: 与 M4b 相比无明显退化

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

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SCENE-005.md`
- ClosedAt: `2026-04-15 19:32`
- Summary:
  - `SceneGraphService` 输出项显式携带 `SceneTransform`，首帧保持 identity 兼容。
  - 连续帧对首节点输出轻量 transform 动态（Position.X 与 Rotation.Yaw），并保留材质切换链路。
  - 场景测试补充 transform/rotation 验证，覆盖接口路径输出与有效性校验。
- FilesChanged:
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/tasks/task-scene-005.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-SCENE-005.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build -c Debug -m:1`）
  - Build(Release): pass（`dotnet build -c Release -m:1`）
  - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`25.50s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`40.36s`）
- ModuleAttributionCheck: pass
