# 任务: TASK-SDATA-009 M19 SceneData RigidBody and BoxCollider component schema

## TaskId
`TASK-SDATA-009`

## 目标（Goal）
在 `Engine.SceneData` 中新增 `RigidBody` 与 `BoxCollider` component file schema、normalized descriptions、validation 与 round-trip 支持，为后续 Physics bridge/adapter 提供稳定的场景数据来源。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M19-2026-05-03`

## 里程碑引用（兼容别名：MilestoneRef）
`M19.2`

## 执行代理（ExecutionAgent）
Exec-SceneData

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.SceneData

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scenedata.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M19-G1`
- CanRunParallel: `true`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M19.2 负责把物理组件正式带进 SceneData component array 主路径；没有这层，PhysicsWorld 只能靠手写 definition fixture，无法验证真实 SceneData fixture 到 `PhysicsWorldDefinition` 的映射数据流。
- 本卡承担的是 SceneData 文档层与 normalized 层的物理 schema、validation、read/write/round-trip，不承担 PhysicsWorld 构建、fixed step 或 query 算法。
- 直接影响本卡实现的上游背景包括：M19 继续沿用 M16+ component array 模型；SceneData 只做 schema 与数值语义校验，不运行 physics；`Engine.SceneData` 不得依赖 `Engine.Physics`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `RigidBody` component file schema：
    - `type: "RigidBody"`
    - `bodyType: "Static" | "Dynamic"`
    - `mass: optional number`
  - `BoxCollider` component file schema：
    - `type: "BoxCollider"`
    - `size: { x, y, z }`
    - `center: optional { x, y, z }`, default zero
  - validation 规则：
    - unknown `bodyType` fails
    - missing / non-positive `size` fails
    - non-finite `size` / `center` / `mass` fails
    - Dynamic `mass` 若出现必须 `> 0`
    - Static `mass` 可省略，且可规范化为 `0`
  - round-trip 必须保持 physics component 字段稳定。
- 本卡执行时不得推翻的既定取舍：
  - 不允许让 SceneData 依赖 `Engine.Physics` 的 runtime types、enum 或 query/result 类型。
  - 不允许在本卡实现 physics 运行逻辑、AABB 计算或 world state。
  - 不允许把物理组件混回旧扁平对象字段模型。
  - 不允许偷偷放宽 invalid physics component 为警告后继续成功加载。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M19-2026-05-03 > TechnicalDesign > SceneData Schema` 已定稿 `RigidBody` / `BoxCollider` 的字段形状与 validation 语义，执行时不得改字段名或改成别的嵌套结构。
  - `PLAN-M19-2026-05-03 > Milestones > M19.2` 已定稿 `bodyType` 仅支持 `Static` / `Dynamic`，`center` 默认 zero，`SceneData` 不依赖 `Engine.Physics`。

## 实施说明（ImplementationNotes）
- 先在 SceneData file model 中新增 `RigidBody` / `BoxCollider` 多态 component DTO 与 JSON 读写支持。
- 再在 normalized model 中增加 physics component descriptions，并把 normalizer 接到 component array 主路径。
- 然后显式实现 validation 与默认值：
  - `bodyType` 限定
  - Dynamic / Static `mass` 语义
  - `size` 正有限数
  - `center` 默认 zero
- 最后补 SceneData tests，覆盖 valid load、invalid body type、invalid size、invalid mass、round-trip 稳定和“SceneData 不引用 Physics”边界证据。

## 设计约束（DesignConstraints）
- 不允许在本卡让 `Engine.SceneData` 引用 `Engine.Physics` 项目或类型。
- 不允许在本卡引入 physics world、step、AABB、ground query 语义。
- 不允许顺手扩展到 Sphere/Capsule/Mesh collider、trigger、layer、physics material 或 Editor physics UI。
- 不允许为未来 solver 预留过重 schema 字段而超出 `RigidBody` / `BoxCollider` 最小集合。

## 失败与降级策略（FallbackBehavior）
- 若 normalized physics component 的具体类名需按仓库风格微调，允许等价命名，但 `RigidBody` / `BoxCollider` 字段形状和 validation 语义不得改变。
- 若 Static `mass` 的规范化具体表现需要在“省略”与“归零”之间统一，可在卡内选择一种稳定行为，但必须在测试中钉死并保持 round-trip 语义一致。
- 若实现中发现需要依赖 `Engine.Physics` enum/type 才能表示 `bodyType`，必须停工回退。
- 若 round-trip 不能稳定保留 physics component 字段，不得先以“loader 能过就行”放行。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/**`
  - `src/Engine.Contracts/**`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-007.md`
  - `.ai-workflow/tasks/task-sdata-008.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M19-2026-05-03.md`
- 计划结构引用：
  - `PLAN-M19-2026-05-03 > TechnicalDesign > SceneData Schema`
  - `PLAN-M19-2026-05-03 > Milestones > M19.2`
  - `PLAN-M19-2026-05-03 > TestPlan`
  - `PLAN-M19-2026-05-03 > Assumptions`
  - 上述 schema 引用属于“字段关系与 validation 规则已定的参考实现约束”。

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
- AllowedFiles:
  - physics component file DTO
  - normalized descriptions
  - normalizer / validation
  - SceneData tests
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 PhysicsWorld
- 不实现 fixed step / snapshot / query
- 不引入 Physics 项目依赖
- 不扩展到额外 collider / material / layer 概念
- OutOfScopePaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - Static `mass` 在 normalized / round-trip 路径中选择“缺省保留”还是“规范化为 0”需要执行时统一，但必须保持单一稳定语义并由测试钉死。
- 处理规则：
  - 若问题影响字段形状、Physics 依赖方向或 round-trip 稳定性，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 物理 schema 形状、validation 语义、SceneData 边界和 round-trip 目标都已落卡。
  - 执行者无需回看里程碑也能知道本卡不能让 SceneData 感知 Physics runtime。
  - 关键分歧点和测试钉死要求已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 M19 真实场景数据入口卡，字段和 validation 一旦做偏，会同时影响后续 Physics bridge/adapter、测试 fixture 和未来扩展空间。
  - 同时要守住“不依赖 Physics runtime”的边界，认知复杂度高。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.SceneData -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.SceneData -> Engine.Physics`
  - `Engine.SceneData -> Engine.Scene`
  - `Engine.SceneData -> Engine.App`
  - `Engine.SceneData -> Engine.Render`
  - `Engine.SceneData -> Engine.Scripting`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scenedata.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 valid/invalid/round-trip/boundary
- Smoke: 真实 SceneData fixture 或 sample scene 可加载出包含 `RigidBody` / `BoxCollider` 的 `SceneDescription`
- Perf: 物理 schema 校验仅发生在显式 load/save/reload 阶段，不引入逐帧 physics 运行或跨模块回调

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SDATA-009.md`
- ClosedAt: `2026-05-04 21:26`
- Summary:
  - Added SceneData file schema for `RigidBody` and `BoxCollider` components.
  - Added normalized `SceneRigidBodyComponentDescription` and `SceneBoxColliderComponentDescription`.
  - Added validation for body type, mass, box size, center, default center, and normalized static/dynamic mass semantics.
  - Added SceneData valid/invalid/round-trip/boundary tests without depending on `Engine.Physics`.
- FilesChanged:
  - `src/Engine.SceneData/FileModel/SceneFileComponentDefinition.cs`
  - `src/Engine.SceneData/Descriptions/SceneComponentDescription.cs`
  - `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
  - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-sdata-009.md`
  - `.ai-workflow/archive/2026-05/TASK-SDATA-009.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`，44/44 passed）
  - Smoke: pass（JSON fixture loads into `SceneDescription` with `RigidBody` / `BoxCollider` components）
  - Perf: pass（physics schema validation only runs during explicit load/save/reload, no per-frame physics or cross-module callback）
- ModuleAttributionCheck: pass
