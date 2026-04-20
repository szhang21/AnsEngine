# 任务: TASK-SCENE-007 M7 Scene 资源引用输出对齐

## TaskId
`TASK-SCENE-007`

## 目标（Goal）
让 `Engine.Scene` 稳定输出对象资源引用（meshId + materialId），并与 M7 的 Render 资源入口保持一致。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M7-2026-04-18`

## 里程碑引用（兼容别名：MilestoneRef）
`M7`

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
- ParallelGroup: `M7-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-REND-011`
  - `TASK-REND-012`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - Scene 帧输出中的 meshId/materialId 对齐与回退规则
  - 对应测试文件
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认“一类一文件、一接口一文件”；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在 Scene 中解析资源数据
- 不在 Scene 中实现渲染提交逻辑
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Asset/**`
  - `src/Engine.App/**`

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
- Test: `dotnet test` 通过；Scene 资源引用输出测试通过
- Smoke: Scene 输出的 meshId/materialId 可被 Render 解析并成功渲染
- Perf: 相比 M6 无明显退化

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

## ExecutionStatus
- Status: `Done`
- Completion: `100`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-SCENE-007.md`
- ClosedAt: `2026-04-19 21:15`
- Summary:
  - `SceneGraphService` 输出路径改为“候选资源 -> Scene 侧回退解析 -> 结构化资源引用构造”，确保 `meshId/materialId` 始终与 Render M7 已支持入口对齐。
  - 新增材质循环与缺失资源回退规则：`default -> pulse -> highlight -> missing(fallback->default)`。
  - 多对象场景下缺失 mesh 不外泄，统一回退到 `mesh://triangle`，保证 Render 消费稳定。
- FilesChanged:
  - `src/Engine.Scene/SceneGraphService.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `.ai-workflow/tasks/task-scene-007.md`
  - `.ai-workflow/archive/2026-04/TASK-SCENE-007.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`）
  - Test: pass（`dotnet test AnsEngine.sln -m:1`；`Engine.Scene.Tests` 7/7 通过）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`16.07s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.84s`）
- ModuleAttributionCheck: `pass`
