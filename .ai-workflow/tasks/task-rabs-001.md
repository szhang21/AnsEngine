# 任务: TASK-RABS-001 M22 Runtime Abstractions module foundation

## TaskId
`TASK-RABS-001`

## 目标（Goal）
新增 `Engine.Runtime.Abstractions` 与测试项目，定义最小 runtime object/component 公共抽象，并以边界测试确认该模块只依赖 `Engine.Contracts` 且不包含 update、scene traversal 或 mutation API。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M22-2026-05-11`

## 里程碑引用（兼容别名：MilestoneRef）
`M22.1`

## 执行代理（ExecutionAgent）
Exec-RuntimeAbstractions

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Runtime.Abstractions

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-runtime-abstractions.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M22-G1`
- CanRunParallel: `false`
- DependsOn:

## 里程碑上下文（MilestoneContext）
- M22 的目标是建立未来 runtime/script 共享抽象，让自定义脚本程序集不需要引用 `Engine.Scene`。
- 本卡是 M22 的地基，先新增 `Engine.Runtime.Abstractions` 模块和最小 API 形状，后续 Scene/Scripting 都依赖它。
- 上游直接影响本卡的背景包括：该模块只能依赖 `Engine.Contracts`；M22 不引入脚本加载、组件 update 生命周期、scene traversal、add/remove component 或跨对象查询。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 使用 `Engine.Runtime.Abstractions` 作为共享 runtime/script API seed。
  - 命名固定为 `IRuntimeComponent`、`IRuntimeObject`、`IRuntimeTransformComponent`，避免和 SceneData/editor component DTO 混淆。
  - `IRuntimeTransformComponent` 使用 `Engine.Contracts.SceneTransform`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许引用 `Engine.Scene`、`Engine.Scripting`、`Engine.App`、`Engine.Render`、`Engine.Physics`、`Engine.SceneData`、`Engine.Editor`、`Engine.Editor.App`。
  - 不允许新增 `Update`、`FindObject`、`AddComponent`、`RemoveComponent`、`Parent`、`Children`、`Scene` 或跨对象查询 API。
- 计划中已定稿的接口形状：
  - `public interface IRuntimeComponent { }`
  - `public interface IRuntimeObject { string ObjectId { get; } string ObjectName { get; } T? GetComponent<T>() where T : class, IRuntimeComponent; bool HasComponent<T>() where T : class, IRuntimeComponent; }`
  - `public interface IRuntimeTransformComponent : IRuntimeComponent { SceneTransform LocalTransform { get; } void SetLocalTransform(SceneTransform transform); }`

## 实施说明（ImplementationNotes）
- 新增 `src/Engine.Runtime.Abstractions/Engine.Runtime.Abstractions.csproj`，只引用 `src/Engine.Contracts/Engine.Contracts.csproj`。
- 新增 `tests/Engine.Runtime.Abstractions.Tests/Engine.Runtime.Abstractions.Tests.csproj` 并接入 `AnsEngine.sln`。
- 按一接口一文件落地 `IRuntimeComponent`、`IRuntimeObject`、`IRuntimeTransformComponent`。
- 新增边界测试或项目引用扫描，确认 Runtime.Abstractions 仅引用 Contracts，且源代码不包含被 M22 禁止的 public API 名称。
- 新增 `.ai-workflow/boundaries/engine-runtime-abstractions.md` 并更新 `.ai-workflow/boundaries/README.md` 映射。

## 设计约束（DesignConstraints）
- 不允许把未来 M23+ 的 updateable component 生命周期提前放进本卡。
- 不允许引入 mutable component collection API；M22 只提供 typed lookup。
- 不允许让 Runtime.Abstractions 知道 Scene concrete runtime object 或 Scripting context。
- 新增 C# 代码遵守 Engine 编码规范：私有/保护字段用 `mCamelCase`，静态字段用 `sCamelCase`，常量用 `kCamelCase`；默认一个类/接口一个文件。

## 失败与降级策略（FallbackBehavior）
- 若 `SceneTransform` 所在契约不足以支撑 transform abstraction，必须回退给 Dispatch/Plan，不得把新 transform 类型放进 Runtime.Abstractions。
- 若边界测试发现除 Contracts 外的项目引用，必须视为本卡失败并回退。
- 若执行者认为需要 traversal、add/remove 或 update API，必须停工修卡，不得私自扩展公开面。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
  - `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
  - `src/Engine.Scripting/IScriptSelfObject.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
  - `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/plan-archive/2026-05/PLAN-M22-2026-05-11.md`
  - `.ai-workflow/boundaries/README.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
- 计划结构引用：
  - `PLAN-M22-2026-05-11 > TechnicalDesign > Public Abstractions`
  - `PLAN-M22-2026-05-11 > Milestones > M22.1`
  - 上述接口形状属于参考实现约束，不是示意草图。

## 范围（Scope）
- AllowedModules:
  - Engine.Runtime.Abstractions
- AllowedFiles:
  - Runtime.Abstractions 项目、接口文件、测试项目、solution 接入
- AllowedPaths:
  - `AnsEngine.sln`
  - `src/Engine.Runtime.Abstractions/**`
  - `tests/Engine.Runtime.Abstractions.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 concrete runtime object/component storage
- 不修改 `Engine.Scene` 或 `Engine.Scripting`
- 不新增 update/traversal/mutation API
- 不引入自定义脚本程序集加载
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Scripting/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `SceneTransform` 的 using/namespace 以现有 `Engine.Contracts` 实际定义为准。
- 处理规则：
  - 若 Contracts 需要新增类型或变更 public shape，必须回退，不得在本卡扩大为契约变更任务。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 计划已给出完整接口形状、依赖方向和禁止 API 清单。
  - 本卡实现入口、测试入口、边界文档和非范围已明确。
  - 执行者无需回看计划全文即可知道 public API 的上限。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 新增公共抽象模块，API 一旦过宽会污染后续 Script/Scene 边界。
  - 需要同时满足接口形状、项目依赖、solution 接入和边界文档同步。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Runtime.Abstractions -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Runtime.Abstractions -> Engine.Scene`
  - `Engine.Runtime.Abstractions -> Engine.Scripting`
  - `Engine.Runtime.Abstractions -> Engine.App`
  - `Engine.Runtime.Abstractions -> Engine.Render`
  - `Engine.Runtime.Abstractions -> Engine.Physics`
  - `Engine.Runtime.Abstractions -> Engine.SceneData`
  - `Engine.Runtime.Abstractions -> Engine.Editor`
  - `Engine.Runtime.Abstractions -> Engine.Editor.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-runtime-abstractions.md`
  - `.ai-workflow/boundaries/README.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，新增 Runtime.Abstractions 边界/API shape 测试通过
- Smoke: 模块可被 solution 加载；接口 public shape 与计划一致；禁止 API 名称未出现
- Perf: 仅新增接口模块，无 runtime 主路径成本；记录无性能影响说明

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-RABS-001.md`
- ClosedAt: `2026-05-11`
- Summary:
  - Added `Engine.Runtime.Abstractions` project with `IRuntimeComponent`, `IRuntimeObject`, and `IRuntimeTransformComponent`.
  - Added `Engine.Runtime.Abstractions.Tests` API shape and dependency boundary tests.
  - Added runtime abstractions boundary contract and boundaries README mapping.
- FilesChanged:
  - `AnsEngine.sln`
  - `src/Engine.Runtime.Abstractions/Engine.Runtime.Abstractions.csproj`
  - `src/Engine.Runtime.Abstractions/IRuntimeComponent.cs`
  - `src/Engine.Runtime.Abstractions/IRuntimeObject.cs`
  - `src/Engine.Runtime.Abstractions/IRuntimeTransformComponent.cs`
  - `tests/Engine.Runtime.Abstractions.Tests/Engine.Runtime.Abstractions.Tests.csproj`
  - `tests/Engine.Runtime.Abstractions.Tests/RuntimeAbstractionsApiShapeTests.cs`
  - `.ai-workflow/boundaries/engine-runtime-abstractions.md`
  - `.ai-workflow/boundaries/README.md`
  - `.ai-workflow/tasks/task-rabs-001.md`
  - `.ai-workflow/archive/2026-05/TASK-RABS-001.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
  - Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed, including `Engine.Runtime.Abstractions.Tests` 4/4)
  - Focused Test: pass (`dotnet test tests/Engine.Runtime.Abstractions.Tests/Engine.Runtime.Abstractions.Tests.csproj --no-restore --nologo -v minimal`; 4/4)
  - Smoke: pass (solution loads new module; public API shape matches M22.1; forbidden API names are absent from Runtime.Abstractions source)
  - Perf: pass (interface-only module; no runtime main-path cost)
- ModuleAttributionCheck: pass
