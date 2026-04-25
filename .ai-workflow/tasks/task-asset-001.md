# 任务: TASK-ASSET-001 M9 OBJ 磁盘导入与 mesh CPU 资产缓存主路径

## TaskId
`TASK-ASSET-001`

## 目标（Goal）
在 `Engine.Asset` 落地 `meshId -> catalog -> OBJ -> 规范化 mesh CPU 资产 -> 缓存` 主路径，并提供缺失/损坏资源的显式失败结果，不引入逐帧磁盘读取。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M9-2026-04-22`

## 里程碑引用（兼容别名：MilestoneRef）
`M9.2`

## 执行代理（ExecutionAgent）
Exec-Asset

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Asset

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-asset.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M9-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-CONTRACT-005`

## 范围（Scope）
- AllowedModules:
  - Engine.Asset
- AllowedFiles:
  - OBJ 导入器
  - mesh catalog 读取与映射
  - mesh CPU 资产缓存与失败结果适配
  - Asset 测试与样例测试资源
- AllowedPaths:
  - `src/Engine.Asset/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不创建 OpenGL 资源
- 不修改 Scene 输出逻辑
- 不把 `meshId` 退化为裸文件路径
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Asset -> Engine.Core`
  - `Engine.Asset -> Engine.Platform`
  - `Engine.Asset -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Asset -> Engine.Render`
  - `Engine.Asset -> Engine.Scene`
  - 在 Asset 内创建或持有 OpenGL 资源

## 边界变更请求（BoundaryChangeRequest）
- Required: `true`
- Status: `approved`
- RequestReason: `M9 需要由 Engine.Asset 输出规范化 mesh CPU 资产并通过 Engine.Contracts 对 Render/App 暴露稳定查询接口；当前 engine-asset.md 未覆盖 Asset -> Contracts。`
- ImpactModules:
  - `Engine.Asset`
  - `Engine.Contracts`
- HumanApprovalRef: `Human command "拆卡m9" on 2026-04-23, accepting PLAN-M9-2026-04-22 dependency direction`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-asset.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；OBJ 导入、catalog 映射、缓存命中、缺失/损坏资源测试通过
- Smoke: headless 模式下能加载至少一个真实磁盘 mesh 并完成一次启动/退出
- Perf: 同一 mesh 重复请求命中缓存，稳定帧循环阶段无重复磁盘读取

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-ASSET-001.md`
- ClosedAt: `2026-04-23 23:35`
- Summary:
  - 新增 `DiskMeshAssetProvider`，在 `Engine.Asset` 内落地 `meshId -> catalog -> OBJ -> MeshAssetData -> cache` 主路径。
  - 新增 OBJ 导入与 catalog 解析实现，并通过显式 `MeshAssetLoadResult` 返回缺失、损坏和格式不支持等失败语义。
  - 保持 `NullAssetService` 兼容现有 `IAssetService` 调用，同时可选代理到 mesh provider，补齐 Asset 边界文档与专项测试。
- FilesChanged:
  - `src/Engine.Asset/AssetHandle.cs`
  - `src/Engine.Asset/DiskMeshAssetProvider.cs`
  - `src/Engine.Asset/Engine.Asset.csproj`
  - `src/Engine.Asset/MeshCatalog.cs`
  - `src/Engine.Asset/NullAssetService.cs`
  - `src/Engine.Asset/ObjMeshFileLoader.cs`
  - `src/Engine.Asset/AssetPlaceholders.cs`
  - `tests/Engine.Asset.Tests/AssetServiceTests.cs`
  - `.ai-workflow/boundaries/engine-asset.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
- ValidationEvidence:
  - Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug --nologo`）
  - Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release --nologo`）
  - Test: `pass`（`dotnet test AnsEngine.sln --nologo -v minimal`；`Engine.Asset.Tests` 共 8 条通过）
  - Smoke: `pass`（`dotnet test tests/Engine.Asset.Tests/Engine.Asset.Tests.csproj --nologo -v minimal --filter "FullyQualifiedName~HeadlessPath"` 通过，验证 headless 路径可加载真实磁盘 mesh）
  - Perf: `pass`（重复请求命中 `meshId -> MeshAssetLoadResult` 缓存；专项测试验证不重复读取已缓存成功结果）
- ModuleAttributionCheck: pass
