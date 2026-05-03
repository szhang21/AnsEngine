# 任务: TASK-SCENE-019 M17 Scene script access bridge

## TaskId
`TASK-SCENE-019`

## 目标（Goal）
让 `Engine.Scene` 暴露窄的 self-transform script access bridge，使 `Engine.Scripting` 能修改绑定对象自身 Transform，同时保持 `Engine.Scene` 不反向依赖 `Engine.Scripting`，并移除或禁用 M15 默认旋转 smoke behavior。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M17-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M17.3`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Contracts
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M17-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SDATA-008`

## 里程碑上下文（MilestoneContext）
- M17.3 的职责是把脚本 runtime 与 Scene runtime 接起来，但只能通过受限、自身对象级的访问桥，不允许把脚本系统反向塞进 `Engine.Scene`。
- 本卡承担的是 Scene 自身 Transform 访问桥、脚本可观察的 transform 修改和 M15 默认旋转 smoke behavior 的移除或禁用，不承担脚本 registry/runtime 自身实现，也不承担 App 中的 `RotateSelf` 绑定。
- 直接影响本卡实现的上游背景包括：脚本第一版只能访问自身对象 Transform；Scene bridge 不能暴露可变 runtime object/component 集合；`BuildRenderFrame()` 仍必须是 current runtime state 的纯读取。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.Scene` 暴露窄 script access API，例如：
    - bind script to object id
    - get object id/name
    - get/set self local transform
  - API 不暴露 mutable runtime object/component collections。
  - API 不允许查询任意对象。
  - Transform-only object 可以 host scripts。
  - M15 默认旋转 smoke behavior 在 M17 中移除或禁用，由内置 `RotateSelf` 接管可见行为。
- 本卡执行时不得推翻的既定取舍：
  - 不允许 `Engine.Scene` 引用 `Engine.Scripting`。
  - 不允许把 Scene bridge 做成“任意对象查找/修改”接口。
  - 不允许通过 `BuildRenderFrame()` 隐式推进脚本结果或更新逻辑。
  - 不允许保留 M15 默认旋转作为主要可见行为，和脚本行为并行混用。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M17-2026-05-02 > SceneBridgeDesign` 已定稿 Scene 只暴露 narrow self-transform bridge，执行时不得扩大为通用场景查询接口。
  - `PLAN-M17-2026-05-02 > PlanningDecisions` 已定稿“第一版脚本只能访问自身对象 Transform”和“M15 默认旋转被移除/禁用”。

## 实施说明（ImplementationNotes）
- 先在 Scene 内定义不反向依赖脚本模块的桥接接口/适配面，围绕 object id/name 和 self local transform 组织。
- 再把该桥接面绑定到 runtime object 上，使脚本运行时后续能够安全读取/写回自身 local transform。
- 然后移除或禁用 M15 默认旋转 smoke behavior，确保脚本成为新的可见 runtime 行为来源。
- 最后补 Scene tests，覆盖：
  - 脚本桥接可修改自身 Transform
  - 不能通过桥接访问/修改其他对象
  - snapshot 与 render frame 可观察脚本修改结果
  - `Engine.Scene` 不引用 `Engine.Scripting`

## 设计约束（DesignConstraints）
- 不允许在本卡内实现 `Engine.Scripting` registry/runtime 或 App 组合。
- 不允许引入 `Engine.Scene -> Engine.Scripting` 编译时依赖。
- 不允许扩大 bridge 到跨对象访问、全场景枚举、资源访问或脚本上下文管理。
- 不允许改变 Render contract 或让 Render 感知 Script component。

## 失败与降级策略（FallbackBehavior）
- 若 Scene bridge 的具体接口命名与计划建议略有差异，允许按现有代码风格调整，但必须保持“仅自身 Transform、不可查询其他对象”的事实。
- 若 Transform-only object 通过桥可被脚本修改但不渲染，这是合法行为；不得为了“看得见”而强制补 MeshRenderer。
- 若实现中发现只能通过 `Engine.Scene -> Engine.Scripting` 反向依赖才能完成绑定，必须停工回退。
- 若移除 M15 默认旋转导致旧测试失败，应同步改测试语义到脚本驱动，而不是保留双行为并存。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/**`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
  - `src/Engine.Scripting/**`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.Scripting.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-018.md`
  - `.ai-workflow/tasks/task-scene-016.md`
  - `.ai-workflow/tasks/task-scene-017.md`
  - `.ai-workflow/tasks/task-script-001.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M17-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M17-2026-05-02 > SceneBridgeDesign`
  - `PLAN-M17-2026-05-02 > PlanningDecisions`
  - `PLAN-M17-2026-05-02 > Milestones > M17.3`
  - `PLAN-M17-2026-05-02 > TestPlan`
  - 上述 scene bridge 引用属于“实现约束”，访问范围和 M15 行为替换方向已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - scene script bridge
  - runtime update behavior adjustment
  - Scene tests
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Script registry/runtime
- 不实现 App 注册/绑定/执行
- 不实现 Editor Script UI
- 不修改 SceneData `Script` schema
- OutOfScopePaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.App/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - narrow bridge 的具体接口命名可随当前 Scene 代码组织调整，但必须保持不暴露 runtime 集合和不可跨对象访问。
- 处理规则：
  - 若问题影响 `Engine.Scene` 依赖方向、自身对象访问边界或默认旋转的移除语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - self-transform bridge、反向依赖禁令、M15 默认旋转替换和测试口径都已下沉。
  - 执行者无需回看计划也能知道这张卡不能把脚本系统塞进 Scene 内部。
  - 失败分流和非范围明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡直接决定 Scene 与 Scripting 的边界是否被破坏，同时还要替换现有可见 runtime 行为。
  - 若写短，很容易把 bridge 做成通用查询面或保留双更新路径并存。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Scripting`
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.App`
  - `Engine.Scene -> Engine.Editor`
  - `Engine.Scene -> Engine.Editor.App`
  - `Engine.Scene -> Engine.Platform`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 self-transform 修改、跨对象访问禁止和 render/snapshot 可观察性
- Smoke: 脚本桥可修改自身 Transform；snapshot 和 render frame 可观察修改；M15 默认旋转不再作为主可见行为
- Perf: 不引入逐帧 JSON 读取、任意对象查询或 render side effect

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-019.md`
- ClosedAt: `2026-05-02 15:00`
- Summary:
  - 2026-05-02: Started after `TASK-SDATA-008` reached Review; Scene-only AllowedPaths, no `Engine.Scene -> Engine.Scripting` dependency, self-transform bridge scope, and M15 rotation removal constraints checked.
  - 2026-05-02: Added narrow `SceneGraphService.BindScriptObject(...)` bridge returning a handle bound to one runtime object only.
  - 2026-05-02: Added `SceneScriptObjectHandle` with object id/name and self local transform get/set; no runtime object collection or cross-object query exposed.
  - 2026-05-02: Removed M15 default rotation smoke behavior from `RuntimeScene.Update`; update now only maintains diagnostics until App scripting drives behavior.
  - 2026-05-02: Updated Scene tests for self-transform bridge, Transform-only script host behavior, missing object/transform failures, render/snapshot observability, and no `Engine.Scene -> Engine.Scripting` dependency.
- FilesChanged:
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Scene/Runtime/RuntimeScene.cs`
  - `src/Engine.Scene/Runtime/SceneScriptObjectHandle.cs`
  - `src/Engine.Scene/Runtime/SceneScriptObjectBindFailure.cs`
  - `src/Engine.Scene/Runtime/SceneScriptObjectBindFailureKind.cs`
  - `src/Engine.Scene/Runtime/SceneScriptObjectBindResult.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-019.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --no-restore --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal` passed, 52 tests.
  - Regression: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal` passed, 10 tests.
  - Smoke: script bridge modifies bound object self Transform; snapshot and render frame observe changes; Transform-only object can be modified via bridge but remains excluded from render frame; default update no longer rotates objects.
  - Perf: bridge lookup is explicit bind by object id; update has no script/runtime/query loop yet; no per-frame JSON, arbitrary object query, or render side effect added.
  - Boundary: `rg` check found no `Engine.Scene -> Engine.Scripting` project/source dependency; hits are only boundary test assertions.
- ModuleAttributionCheck: pass
