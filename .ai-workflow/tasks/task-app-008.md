# 任务: TASK-APP-008 M9 Mesh provider 装配与样例运行路径接线

## TaskId
`TASK-APP-008`

## 目标（Goal）
由 `Engine.App` 作为组合根装配 mesh provider、样例资源入口和运行时依赖，使真实磁盘 mesh 主链路在 headless/真实窗口路径都可启动、运行和退出。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M9-2026-04-22`

## 里程碑引用（兼容别名：MilestoneRef）
`M9.3`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Asset
- Engine.Render
- Engine.Scene
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M9-G3`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-ASSET-001`
  - `TASK-REND-013`
  - `TASK-SCENE-008`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - provider/service 装配
  - 样例资源运行路径与配置接线
  - App 测试
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不在 App 内实现导入器
- 不在 App 内实现 GPU cache
- 不修改 Scene/Render 业务职责边界
- OutOfScopePaths:
  - `src/Engine.Asset/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Contracts`
- ForbiddenDependsOn:
  - 在 App 中承载 OBJ 解析或 GPU 资源逻辑
  - 通过全局定位器绕过显式依赖注入

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；App 装配与异常收口测试通过
- Smoke: 真实 mesh 主链路可在 headless/真实窗口路径完成启动与稳定退出
- Perf: 装配与样例运行路径不引入逐帧服务定位或重复初始化

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-APP-008.md`
- ClosedAt: `2026-04-25 18:33`
- Summary:
  - 组合根新增 `DiskMeshAssetProvider` 与 sample mesh 资源目录装配，native 渲染路径显式注入 mesh provider。
  - `ApplicationHost` 在进入主循环前预热一次 bootstrap mesh 解析，确保 headless/真实窗口路径都走到真实磁盘 mesh 主链路。
  - 补齐 App 装配测试、样例资源文件与边界文档，保持 App 只做装配不承载 OBJ/GPU 逻辑。
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/Engine.App.csproj`
  - `src/Engine.App/SampleAssets/cube.obj`
  - `src/Engine.App/SampleAssets/mesh-catalog.txt`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-app.md`
- ValidationEvidence:
  - Build(Debug): `pass`（M9 App 装配路径已进入当前代码基线；Human 于 `2026-04-25` 确认验收通过）
  - Build(Release): `pass`（同上）
  - Test: `pass`（`tests/Engine.App.Tests` 覆盖 mesh provider 注入、bootstrap 运行与异常收口路径）
  - Smoke: `pass`（Human 于 `2026-04-25` 确认真实 mesh 主链路可在 headless/真实窗口路径启动并稳定退出）
  - Perf: `pass`（provider/sample 资源只在启动阶段装配与预热，不引入逐帧重复初始化）
- ModuleAttributionCheck: pass
