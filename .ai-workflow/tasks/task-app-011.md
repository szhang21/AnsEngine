# 任务: TASK-APP-011 M17 App scripting runtime integration and RotateSelf sample

## TaskId
`TASK-APP-011`

## 目标（Goal）
让 `Engine.App` 组合 `ScriptRegistry` 与 `ScriptRuntime`，注册内置 `RotateSelf` script，并在 scene load 后绑定/initialize 脚本、在每帧 render 前执行 script update，打通 M17 的 scripting runtime 主链路。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M17-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M17.4`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Scripting
- Engine.Scene
- Engine.SceneData
- Engine.Platform

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M17-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-019`

## 里程碑上下文（MilestoneContext）
- M17.4 是把 scripting foundation 真正接入运行主路径的一张卡；没有 App 组合根接线、`RotateSelf` 注册和 update 顺序保证，前面的模块只能停留在孤立能力。
- 本卡承担的是 App 组合、scene load 后脚本绑定/initialize、每帧 script update 和 sample scene 主路径，不承担 Script component schema 或 Scene 自身 bridge 设计。
- 直接影响本卡实现的上游背景包括：App 每帧顺序必须是 Scene base update 后再 script update，再 render；unknown script id / script exception 必须 fail fast 且保持 shutdown/dispose 稳定；Editor 在 M17 只做 Script component preserve，不做 UI 编辑。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `RuntimeBootstrap` 组合：
    - `SceneGraphService`
    - `ScriptRegistry`
    - `ScriptRuntime`
    - built-in `RotateSelf` registration
  - Scene load flow：
    1. Load SceneData
    2. Initialize Scene runtime
    3. Bind Script components to runtime objects
    4. Initialize script instances
  - Per-frame flow：
    1. Process events
    2. Read input
    3. Read time
    4. Scene base update
    5. Script update
    6. Render frame
    7. Present
  - `RotateSelf`：
    - property `speedRadiansPerSecond`
    - 读取为 number
    - update 按 Y-axis delta rotation 修改自身 rotation
- 本卡执行时不得推翻的既定取舍：
  - 不允许把脚本绑定或 update 塞进 `Engine.Scene` 内部。
  - 不允许恢复 M15 默认旋转作为可见行为主路径。
  - 不允许在 App 卡中引入外部 DLL 加载、源码编译或热重载。
  - 不允许改变 unknown script id / script exception 的 fail-fast 收口语义。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M17-2026-05-02 > AppIntegrationDesign` 已定稿 scene load flow 与 per-frame flow 的固定顺序，执行时不得调整 script update 在 render 后或 scene update 前。
  - `PLAN-M17-2026-05-02 > ScriptingDesign > Built-in sample` 已定稿 `RotateSelf` 的 `scriptId`、属性名和行为方向。

## 实施说明（ImplementationNotes）
- 先在 App 组合根加入 `Engine.Scripting` 项目装配和内置 `RotateSelf` 注册。
- 再把 scene load 成功路径扩展为：
  - Scene 初始化
  - Script component 绑定
  - script instances initialize
- 然后把每帧运行路径扩展为：
  - Scene base update
  - ScriptRuntime.Update
  - RenderFrame
- 同步迁移默认 sample scene，使其声明 `RotateSelf` Script component，并能以脚本取代 M15 默认旋转行为。
- 最后补 App tests，覆盖：
  - valid RotateSelf scene runs and updates before render
  - unknown script id fails run cleanly
  - script update exception fails run cleanly
  - shutdown/dispose remains stable

## 设计约束（DesignConstraints）
- 不允许在本卡实现 Script component schema 或 Scene self-transform bridge。
- 不允许让 App 直接操纵 runtime object/component 集合而绕过 Scene bridge。
- 不允许改变 `ApplicationHost.Run()` 既有失败收口责任归属。
- 不允许顺手扩展到 Editor Script UI、Play Mode、热重载或外部程序集加载。

## 失败与降级策略（FallbackBehavior）
- 若 `RotateSelf` 的具体工厂注册位置需要随当前 bootstrap 结构微调，允许局部实现调整，但必须仍由 App 组合根负责。
- 若 sample scene 需要增加 `Script` component 来承载可见行为，允许改动 sample scene 文件，但不得借此绕回 M15 默认旋转。
- 若 unknown script id 或 script exception 不能 clean fail 并稳定 shutdown/dispose，必须停工回退，而不是降格为日志警告继续运行。
- 若实现中发现只有让 `Engine.App` 直接写 Scene runtime object 才能驱动脚本，也必须回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/**`
  - `src/Engine.Scripting/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/SampleScenes/default.scene.json`
- 相关测试入口：
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Scripting.Tests/**`
  - `tests/Engine.Scene.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-app-010.md`
  - `.ai-workflow/tasks/task-script-001.md`
  - `.ai-workflow/tasks/task-sdata-008.md`
  - `.ai-workflow/tasks/task-scene-019.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M17-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M17-2026-05-02 > AppIntegrationDesign`
  - `PLAN-M17-2026-05-02 > ScriptingDesign > Built-in sample`
  - `PLAN-M17-2026-05-02 > Milestones > M17.4`
  - `PLAN-M17-2026-05-02 > TestPlan`
  - 上述 integration/sample 引用属于“实现约束”，运行顺序和 `RotateSelf` 字段名已定。

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - App bootstrap/composition
  - runtime loop integration
  - sample scene
  - App tests
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Scripting registry/runtime 内核
- 不实现 Scene self-transform bridge
- 不实现 Editor Script UI
- 不实现外部 DLL、源码编译、热重载
- OutOfScopePaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `RotateSelf` 注册落在 bootstrap 还是组合辅助工厂可随现有 App 组织调整，但职责必须仍在 App 组合根。
- 处理规则：
  - 若问题影响 update 顺序、fail-fast 语义或 App 对 Scene runtime 的越界访问，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - scene load flow、每帧顺序、`RotateSelf` 约束和失败收口都已下沉到卡面。
  - 执行者无需回看计划也能知道 App 只负责组合与调度，不负责脚本系统内部逻辑。
  - 风险点和停工条件已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡跨越 bootstrap、load flow、update flow、sample scene 和失败收口，是 M17 真正的主链路接线点。
  - 若顺序或依赖方向做偏，会直接破坏 App/Scene/Scripting 边界。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Scripting`
- ForbiddenDependsOn:
  - `Engine.App` 直接依赖 Scene runtime object/component 内部集合
  - `Engine.App -> Engine.Editor`
  - `Engine.App -> Engine.Editor.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `true`
- Status: `approved`
- RequestReason: `M17.4 需要由 App 组合根显式装配 ScriptRegistry 与 ScriptRuntime，当前 engine-app 边界合同尚未声明对 Engine.Scripting 的允许依赖。`
- ImpactModules:
  - `Engine.App`
  - `Engine.Scripting`
- HumanApprovalRef: `Human approved via “拆卡m17” on 2026-05-02`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 valid script、unknown script id、script exception、shutdown/dispose
- Smoke: 默认 sample scene 中 `RotateSelf` 驱动对象 rotation；script update 发生在 render 前；unknown script scene clean fail
- Perf: 不引入逐帧程序集加载、源码编译、双重脚本绑定或 render side effect

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-APP-011.md`
- ClosedAt: `2026-05-02 17:41`
- Summary:
  - `Engine.App` now composes `ScriptRegistry` / `ScriptRuntime` and registers built-in `RotateSelf`.
  - `ApplicationHost.Run()` binds Script components after scene initialization and runs script update before render.
  - App tests cover valid `RotateSelf`, unknown script id clean failure, script update exception clean failure, and removal of default Scene update rotation.
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/Engine.App.csproj`
  - `src/Engine.App/SceneRuntimeContracts.cs`
  - `tests/Engine.App.Tests/Engine.App.Tests.csproj`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-011.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL and local `LIB` path warnings.
  - Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` passed, 12/12 tests.
  - Smoke: `ANS_ENGINE_USE_NATIVE_WINDOW=false; ANS_ENGINE_AUTO_EXIT_SECONDS=0.05; dotnet run --project src/Engine.App/Engine.App.csproj --no-restore --nologo` exited 0 with default sample scene using `RotateSelf`.
  - Perf: pass; no per-frame assembly loading, source compilation, hot reload polling, repeated script binding, or render side effect introduced.
- ModuleAttributionCheck: pass
