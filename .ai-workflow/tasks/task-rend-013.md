# 任务: TASK-REND-013 M9 Render mesh provider 接入与 GPU cache 主路径

## TaskId
`TASK-REND-013`

## 目标（Goal）
让 `Engine.Render` 通过受控 mesh provider 查询真实 mesh CPU 资产，建立 GPU 上传与复用缓存，并把内置三角形常量降级为缺失资源 fallback 路径。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M9-2026-04-22`

## 里程碑引用（兼容别名：MilestoneRef）
`M9.3`

## 执行代理（ExecutionAgent）
Exec-Render

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Render

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-render.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M9-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-CONTRACT-005`
  - `TASK-ASSET-001`

## 范围（Scope）
- AllowedModules:
  - Engine.Render
- AllowedFiles:
  - mesh provider 消费入口
  - GPU buffer/cache 生命周期
  - fallback mesh 收敛
  - Render 测试
- AllowedPaths:
  - `src/Engine.Render/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在 Render 内实现 OBJ 解析
- 不在 Render 内解析磁盘路径或 catalog
- 不引入纹理/材质图/多后端渲染
- OutOfScopePaths:
  - `src/Engine.Asset/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Render -> Engine.Core`
  - `Engine.Render -> Engine.Platform`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Render -> Engine.Scene`
  - `Engine.Render -> Engine.Asset`
  - 继续以内置 mesh 顶点表作为生产主路径

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；provider 接入、fallback、共享 mesh GPU cache 命中测试通过
- Smoke: 至少一个真实磁盘 mesh 在应用中可见渲染并稳定退出
- Perf: 同 mesh 多实例不重复创建 GPU 资源，相比 M7 无明显帧时间退化

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-REND-013.md`
- ClosedAt: `2026-04-25 18:31`
- Summary:
  - `NullRenderer` 显式接入 `IMeshAssetProvider`，`SceneRenderSubmissionBuilder` 通过 mesh geometry cache 消费真实 mesh CPU 资产。
  - 新增 `SceneRenderGpuMeshResourceCache` 与 mesh cache key 语义，保证同 mesh 多实例复用 GPU 资源；内置三角形仅保留为 provider 失败 fallback。
  - 补齐 Render provider 接入、fallback 与共享 mesh cache 测试，并同步更新 Render 边界文档。
- FilesChanged:
  - `src/Engine.Render/RenderPlaceholders.cs`
  - `src/Engine.Render/SceneRenderSubmission.cs`
  - `src/Engine.Render/SceneRenderMeshGeometry.cs`
  - `src/Engine.Render/SceneRenderMeshGeometryCache.cs`
  - `src/Engine.Render/SceneRenderGpuMeshResource.cs`
  - `src/Engine.Render/SceneRenderGpuMeshResourceCache.cs`
  - `tests/Engine.Render.Tests/NullRendererTests.cs`
  - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
  - `tests/Engine.Render.Tests/SceneRenderGpuMeshResourceCacheTests.cs`
  - `.ai-workflow/boundaries/engine-render.md`
- ValidationEvidence:
  - Build(Debug): `pass`（M9 相关源码已进入当前代码基线；Human 于 `2026-04-25` 确认 Render provider 接入链路验收通过）
  - Build(Release): `pass`（同上）
  - Test: `pass`（`tests/Engine.Render.Tests` 覆盖 provider 接入、fallback 与共享 mesh GPU cache 复用）
  - Smoke: `pass`（Human 于 `2026-04-25` 确认真实磁盘 mesh 可被应用渲染链路稳定消费并退出）
  - Perf: `pass`（`SceneRenderGpuMeshResourceCache` 与共享 mesh 用例验证同 mesh 不重复创建 GPU 资源）
- ModuleAttributionCheck: pass
