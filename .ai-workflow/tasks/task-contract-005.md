# 任务: TASK-CONTRACT-005 M9 Mesh CPU 资产契约与失败语义定稿

## TaskId
`TASK-CONTRACT-005`

## 目标（Goal）
在 `Engine.Contracts` 中定义 M9 所需的稳定 mesh CPU 资产契约、provider 查询接口与失败结果语义，为 Asset/Render/App 提供唯一跨模块桥接面。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M9-2026-04-22`

## 里程碑引用（兼容别名：MilestoneRef）
`M9.1`

## 执行代理（ExecutionAgent）
Exec-Contracts

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Contracts

## 次级模块（SecondaryModules）
- Engine.Asset
- Engine.Render
- Engine.App

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-contracts.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M9-G1`
- CanRunParallel: `false`
- DependsOn: `[]`

## 范围（Scope）
- AllowedModules:
  - Engine.Contracts
- AllowedFiles:
  - mesh CPU 资产只读数据模型
  - provider 查询接口与失败结果类型
  - 契约兼容与错误语义测试
- AllowedPaths:
  - `src/Engine.Contracts/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 命名约定：`private`/`protected` 字段使用 `camelCase`，禁止前导下划线；构造器参数、方法参数、局部变量使用 `camelCase`；公共类型/属性/方法使用 `PascalCase`。
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在变更说明中注明原因。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现 OBJ 导入器
- 不实现磁盘 catalog 读取
- 不实现 GPU buffer 创建与缓存
- OutOfScopePaths:
  - `src/Engine.Asset/**`
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Contracts -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Contracts -> Engine.Asset`
  - `Engine.Contracts -> Engine.Render`
  - `Engine.Contracts -> Engine.Scene`
  - 在契约层暴露 OBJ 专有解析结构或 OpenGL 类型

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
- Test: `dotnet test` 通过；新增 mesh 契约、结果语义与兼容测试通过
- Smoke: 契约可被装配进 headless 运行路径且不破坏现有启动/退出
- Perf: 契约层不引入逐帧分配型行为或运行期 IO

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-005.md`
- ClosedAt: `2026-04-23 01:14`
- Summary:
  - 新增 M9 mesh CPU 资产桥接契约：`MeshAssetVertex`、`MeshAssetData`、`IMeshAssetProvider`、`MeshAssetLoadResult` 与失败语义类型。
  - 保持 `SceneMeshRef` 作为查询输入，明确“显式结果优先、异常只用于不可恢复错误”的契约语义。
  - 补齐 `Engine.Contracts` 契约测试与边界文档，确保后续 Asset/Render/App 共享同一桥接面。
- FilesChanged:
  - `src/Engine.Contracts/IMeshAssetProvider.cs`
  - `src/Engine.Contracts/MeshAssetData.cs`
  - `src/Engine.Contracts/MeshAssetLoadFailure.cs`
  - `src/Engine.Contracts/MeshAssetLoadFailureKind.cs`
  - `src/Engine.Contracts/MeshAssetLoadResult.cs`
  - `src/Engine.Contracts/MeshAssetVertex.cs`
  - `tests/Engine.Contracts.Tests/MeshAssetContractsTests.cs`
  - `.ai-workflow/boundaries/engine-contracts.md`
- ValidationEvidence:
  - Build(Debug): `pass`（`dotnet build AnsEngine.sln -c Debug --nologo`）
  - Build(Release): `pass`（`dotnet build AnsEngine.sln -c Release --nologo`）
  - Test: `pass`（`dotnet test AnsEngine.sln --nologo -v minimal`；`Engine.Contracts.Tests` 共 18 条通过）
  - Smoke: `pass`（`ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.App/Engine.App.csproj --no-build`，ExitCode=0）
  - Perf: `pass`（契约层仅新增只读数据模型与结果类型，无逐帧 IO 或主动分配循环）
- ModuleAttributionCheck: pass
