# 任务: TASK-SCRIPT-005 M24 Scripting runtime update component conversion

## 目标（Goal）
将内置脚本行为转换为可由 runtime owner/component lookup 驱动的 update component，使 `RotateSelf` 与 `MoveOnInput` 不再依赖 Scene 专用 self-object adapter 路径。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE`

## 里程碑引用（兼容别名：MilestoneRef）
`M24.3`

## 执行代理（ExecutionAgent）
Exec-Scripting

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Scripting

## 次级模块（SecondaryModules）
- tests

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scripting.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M24-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-RABS-002`
  - `TASK-SCENE-024`

## 里程碑上下文（MilestoneContext）
- M17/M18 的 `ScriptRuntime` 通过 `ScriptContext.Self` 与 App/Scene adapter 更新 Transform，脚本还不是 runtime component lifecycle 的一部分。
- M24 要让 script instance 成为 runtime update component，由后续 `Engine.Runtime` 统一 traversal 和 tick。
- 本卡只改 `Engine.Scripting` 的 binding/update component 输出与内置脚本实现，不创建 concrete runtime scheduler，不依赖 `Engine.Scene`。

## 决策继承（DecisionCarryOver）
- 继承决策：
  - Built-in scripts 通过 `IRuntimeUpdateComponent.Update(RuntimeUpdateContext context)` 被驱动。
  - `RotateSelf` 和 `MoveOnInput` 必须通过 `context.Owner.GetComponent<IRuntimeTransformComponent>()` 获取自身 Transform。
  - Missing Transform 必须返回 deterministic runtime update failure。
  - `ScriptRuntime` 保留 registry/factory/bind diagnostics 职责，但不再作为长期 per-frame update scheduler。
- 不得推翻的既定取舍：
  - `Engine.Scripting` 仍不得依赖 `Engine.Scene` 或 `Engine.App`。
  - 不实现 external DLL loading、source compilation、hot reload、cross-object query 或 Editor Script UI。
  - 迁移 adapter 只可作为过渡，不能保留 `SceneScriptSelfObject` 作为长期 public path。
- 上游已定结构约束：
  - Runtime input 使用 `RuntimeInputSnapshot` / `RuntimeKey`，不是 platform input 或旧 `ScriptInputSnapshot` 作为主 runtime frame model。
  - Script order 仍按 scene file / binding order 保持稳定。

## 实施说明（ImplementationNotes）
- 定位 `IScriptBehavior`、`ScriptRuntime`、`ScriptContext`、内置 `RotateSelf`/`MoveOnInput` 的当前 update 路径。
- 让脚本实例或包装组件实现/产出 `IRuntimeUpdateComponent`，其 update 使用 `RuntimeUpdateContext.Owner` 查询 `IRuntimeTransformComponent`。
- 保持 registry/factory/property binding diagnostics 在 `Engine.Scripting` 内，输出可由 `Engine.Runtime` 消费的绑定结果。
- 将旧 `ScriptInputSnapshot` 使用迁移到 runtime input 模型，必要时保留兼容 adapter 但不得让它成为新主路径。
- 补 tests：成功更新 Transform、Missing Transform failure、多脚本更新顺序、no Scene dependency。
- 更新 `.ai-workflow/boundaries/engine-scripting.md`，说明 Scripting 从 self-object adapter update 迁移到 runtime update component 消费模式。

## 设计约束（DesignConstraints）
- 不允许 `Engine.Scripting` 引用 `Engine.Scene`、`Engine.App`、`Engine.Platform`。
- 不允许脚本访问任意非自身对象或全局 scene query。
- 不允许在 Scripting 内解析 scene JSON 或读取文件。
- 不允许把 scheduler/tick order 写入 Scripting；Scripting 只提供可绑定、可更新的 component/behavior 与诊断。

## 失败与降级策略（FallbackBehavior）
- Missing Transform 返回 `RuntimeUpdateResult` failure，必须带 object id/component/script 相关诊断，且由后续 runtime 在 render 前 fail fast。
- Script factory/bind/property failure 继续走显式失败结果，不抛给 App 主循环作为未分类异常。
- 若旧 `ScriptContext` public shape 与新 lifecycle 无法兼容，先保留窄 adapter 并标注 migration risk；不得重新引入 Scene dependency。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scripting/IScriptBehavior.cs`
  - `src/Engine.Scripting/ScriptRuntime.cs`
  - `src/Engine.Scripting/ScriptContext.cs`
  - `src/Engine.Scripting/ScriptRegistry.cs`
  - `src/Engine.Scripting/ScriptInputSnapshot.cs`
- 相关测试入口：
  - `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
  - `tests/Engine.Scripting.Tests/ScriptRegistryTests.cs`
- 相关文档/任务：
  - `.ai-workflow/plan-archive/2026-05/PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE.md` 的 `Scripting As Runtime Update Component` 与 `M24.3 Scripting Runtime Component Conversion`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `TASK-SCRIPT-001`、`TASK-SCRIPT-003`、`TASK-SCRIPT-004`
- 示例结构定位：
  - M24 `Scripting As Runtime Update Component` 小节中关于 `RotateSelf`、`MoveOnInput`、Missing Transform failure、script order 的要求是执行约束。

## 范围（Scope）
- AllowedModules:
  - Engine.Scripting
  - tests
- AllowedFiles:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
- AllowedPaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- Engine.Runtime session/tick implementation
- Scene runtime object storage changes
- App host contraction
- Platform input polling changes
- External script loading / compilation / hot reload
- Editor Script UI
- Cross-object query or signal/event system
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Runtime/**`
  - `src/Engine.App/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - none
- 处理规则：
  - 若 binding output requires concrete Scene type to work, stop and return `TaskCardInsufficient`; do not add Scene dependency.

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确 public lifecycle、脚本迁移目标、failure semantics、禁止依赖和测试口径。
  - 关键设计来自 M24 已定结构，不需要执行者回看计划补决策。
  - 上游依赖卡确保 update API 与 Transform lookup 已存在。
- MissingInfo:
  - none

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Core`
  - `Engine.Contracts`
  - `Engine.Runtime.Abstractions`
- ForbiddenDependsOn:
  - `Engine.Scene`
  - `Engine.App`
  - `Engine.Runtime`
  - `Engine.Platform`
  - `Engine.Render`
  - `Engine.Editor`
  - `Engine.Editor.App`
  - `Engine.Asset`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scripting.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过。
- Test: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 runtime update context、RotateSelf、MoveOnInput、Missing Transform failure、多脚本顺序和 no Scene dependency。
- Smoke: 使用 fake `IRuntimeObject` + `IRuntimeTransformComponent` 可证明脚本通过 owner lookup 更新 transform。
- Perf: 脚本 update 不引入逐帧 JSON parsing、assembly scan 或全 scene query；记录无明显退化说明。
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
- Runtime-updateable script behavior output
- Scripting tests and dependency check
- Boundary change log
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 复杂度评估（ComplexityAssessment）
- Level: `L3`
- Why:
  - 容易误做成 Scripting-owned scheduler 或重新依赖 Scene。
  - 失败语义会影响 runtime render 前 fail-fast 行为。
  - 需要同时迁移输入、Transform lookup、binding output 和脚本顺序。
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCRIPT-005.md`
- ClosedAt: `2026-05-27`
- Summary: Added runtime-updateable script binding output and Scripting-owned `RotateSelf` / `MoveOnInput` implementations that update Transform through `RuntimeUpdateContext.Owner.GetComponent<IRuntimeTransformComponent>()`.
- FilesChanged:
  - `src/Engine.Scripting/ScriptRuntime.cs`
  - `src/Engine.Scripting/ScriptUpdateComponentBindingDescription.cs`
  - `src/Engine.Scripting/ScriptUpdateComponentBindingResult.cs`
  - `src/Engine.Scripting/RotateSelfScript.cs`
  - `src/Engine.Scripting/MoveOnInputScript.cs`
  - `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
  - `.ai-workflow/boundaries/engine-scripting.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL and Windows Kits `LIB` path warnings; 0 errors.
  - Test: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal` passed; 22 passed, 0 failed.
  - Smoke: fake `IRuntimeObject` + `IRuntimeTransformComponent` tests prove built-in scripts update through owner lookup.
  - Perf: script update path adds no per-frame JSON parsing, assembly scan, or scene query; update traversal remains outside Scripting.
  - CodeQuality: NoNewHighRisk=`true`; MustFixCount=`0`; MustFixDisposition=`none`.
  - DesignQuality: DQ-1=`pass`; DQ-2=`pass`; DQ-3=`pass`; DQ-4=`pass`.
- ModuleAttributionCheck: pass
