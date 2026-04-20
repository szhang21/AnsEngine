# 任务: TASK-CONTRACT-004 M7 资源输入契约收敛

## TaskId
`TASK-CONTRACT-004`

## 目标（Goal）
在 `Engine.Contracts` 中定义最小真实资源输入契约，使 `meshId` 与 `materialId` 从占位字段升级为可被渲染链路真实消费的输入。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M7-2026-04-18`

## 里程碑引用（兼容别名：MilestoneRef）
`M7`

## 执行代理（ExecutionAgent）
Exec-Contracts

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Contracts

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Render

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-contracts.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M7-G1`
- CanRunParallel: `false`
- DependsOn: `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Contracts
- AllowedFiles:
  - mesh/material 最小资源引用契约与兼容字段
  - 对应契约测试
- AllowedPaths:
  - `src/Engine.Contracts/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认“一类一文件、一接口一文件”；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现 Render 内部解析逻辑
- 不实现 Scene 资源输出逻辑
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Contracts -> Engine.Core`（如已存在）
- ForbiddenDependsOn:
  - `Engine.Contracts -> Engine.Scene`
  - `Engine.Contracts -> Engine.Render`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；资源契约兼容测试通过
- Smoke: 默认路径可启动、可见、可退出
- Perf: 契约扩展无明显退化

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-004.md`
- ClosedAt: `2026-04-18 11:05`
- Summary:
  - 新增 `SceneMeshRef` 与 `SceneMaterialRef` 最小资源引用契约，提供非空校验，收敛 M7 资源输入语义。
  - `SceneRenderItem` 支持结构化资源构造（`SceneMeshRef`/`SceneMaterialRef`）并保留 `meshId/materialId` 兼容字段与旧构造路径。
  - 补充 Contracts 测试覆盖结构化资源兼容路径与非法资源标识拦截，保证后续 Render/Scene 任务可直接消费。
- FilesChanged:
  - `src/Engine.Contracts/SceneResourceContracts.cs`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
  - `.ai-workflow/tasks/task-contract-004.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-CONTRACT-004.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`）
  - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj -m:1`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.79s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
- ModuleAttributionCheck: pass
