# 任务: TASK-RABS-002 M24 Runtime update component lifecycle shape

## 目标（Goal）
为 `Engine.Runtime.Abstractions` 增加 runtime update component lifecycle 的最小公共接口与输入快照契约，保持抽象层 scheduler-free。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE`

## 里程碑引用（兼容别名：MilestoneRef）
`M24.1`

## 执行代理（ExecutionAgent）
Exec-Runtime-Abstractions

## 优先级（Priority）
P0
> 说明：优先级来自 M24 计划，M24.1 是后续 Scene/Scripting/Runtime 卡的前置接口地基。

## 主模块归属（PrimaryModule）
Engine.Runtime.Abstractions

## 次级模块（SecondaryModules）
- tests

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-runtime-abstractions.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M24-G1`
- CanRunParallel: `false`
- DependsOn:
  - （无）

## 里程碑上下文（MilestoneContext）
- M24 要把脚本和后续 runtime 行为挂到统一 runtime component lifecycle 上，而不是继续由 App 或 Scripting 各自调度。
- 本卡只负责公共抽象 shape，不实现 traversal、scheduler、Scene storage 或 script binding。
- 下游 `TASK-SCENE-024`、`TASK-SCRIPT-005`、`TASK-RUNTIME-001` 都依赖本卡新增的 update component、update context、input snapshot 和 fail-fast result。

## 决策继承（DecisionCarryOver）
- 继承决策：
  - `IRuntimeComponent` 继续保持 marker，不是所有 component 都自动 updateable。
  - 新增 `IRuntimeUpdateComponent : IRuntimeComponent`，只有实现该接口的行为组件参与 update lifecycle。
  - `RuntimeUpdateContext.Owner` 是 per-object update 的唯一对象入口，不引入跨对象查询。
- 不得推翻的既定取舍：
  - `Engine.Runtime.Abstractions` 只能依赖 `Engine.Contracts` 和 .NET 标准库。
  - 不允许引用 `Engine.Scene`、`Engine.Scripting`、`Engine.App`、`Engine.Physics`、`Engine.Runtime` 等 concrete 模块。
  - 本卡不加入 fixed update API，但命名需为未来 `IRuntimeFixedUpdateComponent` 留空间。
- 上游已定结构约束：
  - `IRuntimeUpdateComponent.Update(RuntimeUpdateContext context)` 返回 `RuntimeUpdateResult`。
  - `RuntimeUpdateContext` 至少承载 `IRuntimeObject Owner`、`double DeltaSeconds`、`double TotalSeconds`、`RuntimeInputSnapshot Input`。
  - `RuntimeInputSnapshot` 至少支持 `Empty`、`AnyInputDetected`、`FromKeys(params RuntimeKey[] keys)`、`IsKeyDown(RuntimeKey key)`。
  - `RuntimeKey` 至少包含 `W/A/S/D`。
  - `RuntimeUpdateResult` 必须能表达 success/failure，失败携带 `RuntimeUpdateFailure(Message, ObjectId?, ComponentType?)`。

## 实施说明（ImplementationNotes）
- 在 `src/Engine.Runtime.Abstractions/` 中新增或扩展 public API 文件；遵守一个接口/类型一个文件，字段命名按 `engine-coding-standards` 使用 `mCamelCase/sCamelCase/kCamelCase`。
- 先新增 update lifecycle 类型，再补 `RuntimeInputSnapshot` 的不可变按键集合行为，最后补 fail-fast result/failure shape。
- 更新 `tests/Engine.Runtime.Abstractions.Tests/RuntimeAbstractionsApiShapeTests.cs` 或新增同模块测试，覆盖 API shape、input snapshot 行为和依赖方向。
- 更新 `.ai-workflow/boundaries/engine-runtime-abstractions.md`，把 “非职责：不负责 update lifecycle” 调整为 “只定义 update lifecycle 抽象，不负责 scheduler/traversal/mutation”。

## 设计约束（DesignConstraints）
- 不允许在抽象层实现遍历、排序、scheduler、component collection mutation。
- 不允许把 `ScriptInputSnapshot`、`Engine.Platform.InputSnapshot` 或 Scene-specific 类型暴露到 runtime abstractions。
- 不允许让 Transform 或 MeshRenderer 为了适配本卡而实现 update 接口。
- 不允许修改下游模块行为；本卡只提供可被下游消费的 public shape 和测试。

## 失败与降级策略（FallbackBehavior）
- 若发现 `Engine.Contracts` 现有 transform/input contract 不足，停工回退给 Dispatch/Plan，不在本卡引入新 concrete module dependency。
- 若 dependency scan 发现新增 forbidden dependency，必须回退实现，不能通过测试白名单绕过。
- 若 API shape 与 M24 示例结构冲突，优先保持计划中的字段关系，必要时回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Runtime.Abstractions/IRuntimeComponent.cs`
  - `src/Engine.Runtime.Abstractions/IRuntimeObject.cs`
  - `src/Engine.Runtime.Abstractions/IRuntimeTransformComponent.cs`
- 相关测试入口：
  - `tests/Engine.Runtime.Abstractions.Tests/RuntimeAbstractionsApiShapeTests.cs`
- 相关文档/计划：
  - `.ai-workflow/plan-archive/2026-05/PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE.md` 的 `Runtime Abstractions API Shape` 与 `M24.1 Runtime Abstractions Lifecycle Shape`
  - `.ai-workflow/boundaries/engine-runtime-abstractions.md`
  - `TASK-RABS-001` / `TASK-QA-023`
- 示例结构定位：
  - M24 `Runtime Abstractions API Shape` 中的 interface/record/enum 示例是参考实现约束，字段关系已定，执行时不得自行改成 scripting/platform 专用输入模型。

## 范围（Scope）
- AllowedModules:
  - Engine.Runtime.Abstractions
  - tests
- AllowedFiles:
  - `src/Engine.Runtime.Abstractions/**`
  - `tests/Engine.Runtime.Abstractions.Tests/**`
- AllowedPaths:
  - `src/Engine.Runtime.Abstractions/**`
  - `tests/Engine.Runtime.Abstractions.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- Engine.Runtime concrete scheduler
- Scene component registration
- Script behavior migration
- App input conversion
- Physics writeback
- Fixed update API
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Scripting/**`
  - `src/Engine.App/**`
  - `src/Engine.Physics/**`
  - `src/Engine.Runtime/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - none
- 处理规则：
  - 若实现需要新增跨对象查询、fixed update 或 mutation API，立即回退，不得自行扩张。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 计划已给出 API shape、依赖方向和非目标。
  - 本卡已下沉字段关系、禁止依赖和本卡级验收口径。
  - 执行者无需回看里程碑全文即可实施。
- MissingInfo:
  - none

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Scene`
  - `Engine.Scripting`
  - `Engine.App`
  - `Engine.Physics`
  - `Engine.Runtime`
  - `Engine.Render`
  - `Engine.SceneData`
  - `Engine.Editor`
  - `Engine.Editor.App`

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
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过。
- Test: `dotnet test tests/Engine.Runtime.Abstractions.Tests/Engine.Runtime.Abstractions.Tests.csproj --no-restore --nologo -v minimal` 通过，且覆盖 update component lifecycle、RuntimeInputSnapshot W/A/S/D、result/failure shape。
- Smoke: solution 可加载，现有 Scene/Scripting/App 测试不因新增抽象 API 编译失败。
- Perf: 仅新增接口和值对象，不引入 runtime 主路径 traversal 或 per-frame allocation requirement；记录无明显主路径成本说明。
- CodeQuality:
  - NoNewHighRisk: `true`
  - MustFixCount: `0`
  - MustFixDisposition: `none`
- DesignQuality:
  - DQ-1 职责单一（SRP）: `pass`
  - DQ-2 依赖反转（DIP）: `pass`
  - DQ-3 扩展点保留（OCP-oriented）: `pass`
  - DQ-4 开闭性评估（可选）: `pass`

## 交付物（Deliverables）
- Minimal patch
- Runtime lifecycle public API
- API shape and dependency tests
- Boundary change log
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 复杂度评估（ComplexityAssessment）
- Level: `L2`
- Why:
  - 公共 API 会影响 Scene/Scripting/Runtime 后续卡。
  - 需要严格守住抽象层依赖方向。
- SufficiencyMatch: `pass`

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-RABS-002.md`
- ClosedAt: `2026-05-27`
- Summary: Added scheduler-free runtime update lifecycle public API shape to `Engine.Runtime.Abstractions`, including update component, context, runtime input snapshot/key, and update result/failure contracts.
- FilesChanged:
  - `src/Engine.Runtime.Abstractions/IRuntimeUpdateComponent.cs`
  - `src/Engine.Runtime.Abstractions/RuntimeUpdateContext.cs`
  - `src/Engine.Runtime.Abstractions/RuntimeInputSnapshot.cs`
  - `src/Engine.Runtime.Abstractions/RuntimeKey.cs`
  - `src/Engine.Runtime.Abstractions/RuntimeUpdateResult.cs`
  - `src/Engine.Runtime.Abstractions/RuntimeUpdateFailure.cs`
  - `tests/Engine.Runtime.Abstractions.Tests/RuntimeAbstractionsApiShapeTests.cs`
  - `.ai-workflow/boundaries/engine-runtime-abstractions.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL and Windows Kits `LIB` path warnings; 0 errors.
  - Test: `dotnet test tests/Engine.Runtime.Abstractions.Tests/Engine.Runtime.Abstractions.Tests.csproj --no-restore --nologo -v minimal` passed; 8 passed, 0 failed.
  - Smoke: solution build loaded and compiled downstream Scene/Scripting/App projects with the new abstractions.
  - Perf: public API/value shape only; no traversal, scheduler, per-frame runtime path, or component mutation implementation added.
  - CodeQuality: NoNewHighRisk=`true`; MustFixCount=`0`; MustFixDisposition=`none`.
  - DesignQuality: DQ-1=`pass`; DQ-2=`pass`; DQ-3=`pass`; DQ-4=`pass`.
- ModuleAttributionCheck: pass
