# 任务: TASK-REND-012 M7 Material 参数入口落地

## TaskId
`TASK-REND-012`

## 目标（Goal）
在 `Engine.Render` 建立最小 material 参数入口，使 `materialId` 真实影响最终渲染参数，替换仅 hash 派生颜色语义。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M7-2026-04-18`

## 里程碑引用（兼容别名：MilestoneRef）
`M7`

## 执行代理（ExecutionAgent）
Exec-Render

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Render

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M7-G2`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-CONTRACT-004`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - materialId 解析入口与参数绑定相关代码
  - 对应测试文件
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认“一类一文件、一接口一文件”；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不引入纹理系统与复杂 shader 变体
- 不扩展完整材质编辑体系
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Scene` 编译期依赖
  - materialId 继续保持空语义字段

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；materialId 解析与回退测试通过
- Smoke: 至少一个对象通过真实 materialId 产生可观察渲染差异
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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-REND-012.md`
- ClosedAt: `2026-04-18 00:58`
- Summary:
  - `SceneRenderSubmissionBuilder` 新增最小 material 参数入口：`materialId -> SceneRenderMaterialParameters`，主路径由解析结果直接驱动顶点颜色参数。
  - 移除 hash 派生颜色语义，明确 `material://default` / `material://pulse` / `material://highlight` 解析映射与未知材质回退策略。
  - Render 测试补充 material 命中与回退断言，验证 `materialId` 对最终渲染参数有可观察影响。
- FilesChanged:
  - `src/Engine.Render/SceneRenderSubmission.cs`
  - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
  - `.ai-workflow/tasks/task-rend-011.md`
  - `.ai-workflow/tasks/task-rend-012.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-REND-011.md`
  - `.ai-workflow/archive/2026-04/TASK-REND-012.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`）
  - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.70s`）
- ModuleAttributionCheck: `pass`
