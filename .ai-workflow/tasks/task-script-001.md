# 任务: TASK-SCRIPT-001 M17 Engine.Scripting module and script lifecycle foundation

## TaskId
`TASK-SCRIPT-001`

## 目标（Goal）
新建独立 `Engine.Scripting` 模块与测试项目，落地内置 script registry、script runtime 和 `Initialize/Update` 生命周期地基，为后续 Scene/App 脚本链路提供可诊断、可测试的最小脚本运行时。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M17-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M17.1`

## 执行代理（ExecutionAgent）
Exec-Scripting

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Scripting

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Core
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scripting.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M17-G1`
- CanRunParallel: `false`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M17.1 是 scripting foundation 的地基；没有独立 `Engine.Scripting` 模块、registry 和 lifecycle，后续 Script component schema、Scene bridge 和 App integration 都没有稳定落点。
- 本卡承担的是脚本模块、公开接口、registry/runtime 和 failure 语义，不承担 SceneData `Script` schema、Scene self-transform bridge 或 App 中的 `RotateSelf` sample 组合。
- 直接影响本卡实现的上游背景包括：M17 目标是 scripting foundation，不是完整脚本系统；只允许内置注册表；不加载外部 DLL、不编译源码、不做热重载；依赖方向必须固定为 `Engine.Scripting -> Engine.Scene`，禁止反向依赖。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 新建独立 `Engine.Scripting` 模块。
  - 依赖方向：`Engine.Scripting -> Engine.Scene`，禁止 `Engine.Scene -> Engine.Scripting`。
  - Suggested public types：
    - `IScriptBehavior`
    - `ScriptContext`
    - `ScriptRegistry`
    - `ScriptRuntime`
    - `ScriptBindingResult` 或等价显式失败结果
    - `ScriptUpdateResult` 或等价显式失败结果
  - 生命周期：
    - `Initialize(ScriptContext context)` 仅在 scene load / bind 后调用一次
    - `Update(ScriptContext context)` 每个 App runtime update frame 调用一次
  - duplicate script id registration fails；missing script id creation fails。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把脚本系统塞回 `Engine.Scene` 或 `Engine.App` 内部实现。
  - 不允许实现外部 DLL 加载、源码编译、热重载或 sandbox。
  - 不允许让脚本 runtime 访问任意场景对象或 runtime component 集合。
  - 不允许用异常吞没替代显式 binding/update failure 结果。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M17-2026-05-02 > ScriptingDesign` 已定稿 registry/runtime/context 的最小公开形状与生命周期方向，执行时不得自创另一套“脚本系统主入口”。
  - `PLAN-M17-2026-05-02 > ScriptingDesign > Suggested property value model` 已定稿首版 property value 只支持 `double` / `bool` / `string`，不得在本卡扩到数组、对象、`Vector3`。

## 实施说明（ImplementationNotes）
- 先建立 `Engine.Scripting` 与 `Engine.Scripting.Tests` 工程骨架，并接入解决方案。
- 再落地最小公开接口和值对象：
  - `IScriptBehavior`
  - `ScriptContext`
  - registry/runtime 与显式结果类型
- 然后实现 runtime 的三段式地基：
  - register script factory
  - create/bind script instance
  - initialize once / update per frame
- 最后补 Scripting tests，覆盖：
  - registry register/create success
  - duplicate id registration fails
  - missing id creation fails
  - Initialize exactly once
  - Update per frame
  - script exception produces diagnostic failure

## 设计约束（DesignConstraints）
- 不允许在本卡内直接读取 SceneData JSON、实现 Script component schema 或触碰 sample scene 文件。
- 不允许让 `Engine.Scripting` 依赖 `Engine.Render`、`Engine.Editor`、`Engine.Editor.App`。
- 不允许顺手扩展脚本访问面为“查询其他对象”或“批量场景访问”。
- 不允许把 property binding 做成弱类型 `object`/任意 JSON 节点主路径而丢失明确支持类型边界。

## 失败与降级策略（FallbackBehavior）
- 若结果类型命名需要与现有仓库风格稍作调整，允许等价命名，但必须仍保持显式 binding/update failure 语义。
- 若 `ScriptContext` 的内部字段组织需要局部调整，可调整实现，但对外最小事实必须仍包括 object identity、自身 Transform 访问、delta/total seconds 与只读 properties。
- 若实现中发现只有让 `Engine.Scene` 反向依赖 `Engine.Scripting` 才能继续，必须停工回退。
- 若为了通过测试需要偷偷支持外部 DLL、源码编译或任意属性类型，也必须回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `AnsEngine.sln`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-018.md`
  - `.ai-workflow/tasks/task-app-010.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M17-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M17-2026-05-02 > PlanningDecisions`
  - `PLAN-M17-2026-05-02 > ScriptingDesign`
  - `PLAN-M17-2026-05-02 > Milestones > M17.1`
  - `PLAN-M17-2026-05-02 > TestPlan`
  - 上述 scripting design 引用属于“参考实现约束”，生命周期与 property value 支持范围已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Scripting
- AllowedFiles:
  - 新模块工程
  - script registry/runtime/lifecycle
  - Scripting tests
  - solution/project wiring
- AllowedPaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `AnsEngine.sln`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 SceneData `Script` component schema
- 不实现 Scene self-transform access bridge
- 不实现 App `RotateSelf` sample 组合
- 不实现外部 DLL、源码编译、热重载、sandbox
- OutOfScopePaths:
  - `src/Engine.SceneData/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `ScriptBindingResult` / `ScriptUpdateResult` 的具体类型命名可按仓库风格调整，但必须保持显式结果而非异常吞没。
- 处理规则：
  - 若问题影响依赖方向、生命周期语义或 property value 支持范围，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 新模块、最小公开类型、生命周期、失败语义和禁止路线都已下沉到卡面。
  - 执行者无需回看里程碑也能知道这是 foundation 卡，不是完整脚本系统。
  - 失败分流、非范围和边界约束已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是全新模块和全新运行时子系统的起点，若模块边界或生命周期选错，后续 Scene/App 集成都会连锁返工。
  - 同时要守住内置注册表、无外部加载和显式失败语义，认知复杂度高。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scripting -> Engine.Scene`
  - `Engine.Scripting -> Engine.Core`
  - `Engine.Scripting -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Scripting -> Engine.Render`
  - `Engine.Scripting -> Engine.Editor`
  - `Engine.Scripting -> Engine.Editor.App`
  - `Engine.Scripting -> Engine.Asset`
  - `Engine.Scene -> Engine.Scripting`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scripting.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 register/create/lifecycle/failure
- Smoke: script runtime 可创建 script instance、Initialize 仅一次、Update 按帧调用；script exception 返回可诊断失败结果
- Perf: 不引入逐帧程序集扫描、源码编译、全场景对象查询或 JSON 解析

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCRIPT-001.md`
- ClosedAt: `2026-05-02 14:04`
- Summary:
  - 2026-05-02: Started after reading full task card; AllowedPaths, dependency direction, lifecycle constraints, and no external script loading constraints checked.
  - 2026-05-02: Added `Engine.Scripting` and `Engine.Scripting.Tests` projects to the solution.
  - 2026-05-02: Implemented built-in script registry, explicit failure results, script context/property model, runtime bind/initialize/update lifecycle, and narrow self-transform access interface.
  - 2026-05-02: Added tests for register/create, duplicate id, missing id, initialize once, update per frame, invalid properties, script exceptions, and self-transform mutation.
- FilesChanged:
  - `src/Engine.Scripting/Engine.Scripting.csproj`
  - `src/Engine.Scripting/IScriptBehavior.cs`
  - `src/Engine.Scripting/IScriptSelfTransform.cs`
  - `src/Engine.Scripting/ScriptBindingDescription.cs`
  - `src/Engine.Scripting/ScriptBindingResult.cs`
  - `src/Engine.Scripting/ScriptContext.cs`
  - `src/Engine.Scripting/ScriptFailure.cs`
  - `src/Engine.Scripting/ScriptFailureKind.cs`
  - `src/Engine.Scripting/ScriptPropertyValue.cs`
  - `src/Engine.Scripting/ScriptRegistry.cs`
  - `src/Engine.Scripting/ScriptRegistryResult.cs`
  - `src/Engine.Scripting/ScriptRuntime.cs`
  - `src/Engine.Scripting/ScriptUpdateResult.cs`
  - `tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj`
  - `tests/Engine.Scripting.Tests/ScriptRegistryTests.cs`
  - `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
  - `AnsEngine.sln`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/tasks/task-script-001.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --no-restore --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Test: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal` passed, 10 tests.
  - Smoke: runtime creates registered script instances, calls Initialize once during bind, calls Update per frame, returns diagnostic failure for script exceptions, and lets scripts mutate only the provided self-transform handle.
  - Perf: no external assembly scanning, source compilation, JSON/file IO, or all-scene object query path added.
  - Boundary: `rg` check found no forbidden Scripting dependencies, external loading/compilation, JSON serializer, or file IO references; `Engine.Scene.csproj` has no `Engine.Scripting` reference.
- ModuleAttributionCheck: pass
