# 任务: TASK-SCENE-008 M9 Scene 真实 mesh 引用收敛

## TaskId
`TASK-SCENE-008`

## 目标（Goal）
让 `Engine.Scene` 输出稳定的真实 `meshId` 引用并覆盖共享 mesh/缺失 mesh 路径验证，同时保持 Scene 不持有磁盘路径、导入器或 GPU 资源细节。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M9-2026-04-22`

## 里程碑引用（兼容别名：MilestoneRef）
`M9.3`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M9-G3`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-CONTRACT-005`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - Scene mesh 引用输出与共享对象用例
  - 缺失资源引用回退验证
  - Scene 测试
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现磁盘 catalog 解析
- 不实现 OBJ 导入
- 不实现 GPU buffer 创建
- OutOfScopePaths:
  - `src/Engine.Asset/**`
  - `src/Engine.Render/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Asset`
  - `Engine.Scene -> Engine.Render`
  - 在 Scene 中持有裸文件路径或导入器语义

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
- Test: `dotnet test` 通过；Scene 真实 mesh 引用、共享 mesh 与缺失 mesh 回退测试通过
- Smoke: Scene 输出可被下游消费，headless 与真实窗口路径均不因新引用语义崩溃
- Perf: 不引入逐帧路径解析或额外 IO

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SCENE-008.md`
- ClosedAt: `2026-04-25 18:32`
- Summary:
  - `SceneGraphService` 默认输出真实 `mesh://cube` 引用，并在多对象路径保留 `mesh://missing` 作为下游 fallback 验证入口。
  - 保持 Scene 只持 `meshId/materialId` 契约语义，不引入磁盘路径、catalog 或导入器细节。
  - 补齐共享 mesh 与缺失 mesh 引用测试，并同步更新 Scene 边界文档。
- FilesChanged:
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
- ValidationEvidence:
  - Build(Debug): `pass`（M9 Scene mesh 引用路径已进入当前代码基线；Human 于 `2026-04-25` 确认验收通过）
  - Build(Release): `pass`（同上）
  - Test: `pass`（`tests/Engine.Scene.Tests` 覆盖真实 mesh 引用、共享 mesh 与缺失 mesh fallback 语义）
  - Smoke: `pass`（Human 于 `2026-04-25` 确认新 mesh 引用语义未破坏 headless/真实窗口运行路径）
  - Perf: `pass`（Scene 仅输出稳定 `meshId`，未引入逐帧路径解析或 IO）
- ModuleAttributionCheck: pass
