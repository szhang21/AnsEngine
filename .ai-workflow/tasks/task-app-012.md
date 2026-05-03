# 任务: TASK-APP-012 M18 App input-to-scripting integration and MoveOnInput

## TaskId
`TASK-APP-012`

## 目标（Goal）
让 `Engine.App` 把 Platform 输入快照转换为 Scripting 输入快照，并在主循环中于 render 前调用脚本 update，注册内置 `MoveOnInput`，打通 `W/A/S/D -> script -> Transform.Position` 的最小交互主链路。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M18-2026-05-03`

## 里程碑引用（兼容别名：MilestoneRef）
`M18.3`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Platform
- Engine.Scripting
- Engine.Scene
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M18-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCRIPT-003`

## 里程碑上下文（MilestoneContext）
- M18.3 是 interaction scripting MVP 真正对用户可见的主链路接线卡；没有 App conversion、loop ordering 和 `MoveOnInput` 注册，前两张卡仍只是 Platform/Scripting 内部能力。
- 本卡承担的是 `InputSnapshot -> ScriptInputSnapshot` 转换、主循环时序校准、内置 `MoveOnInput` 注册和 sample/fixture 级联验证，不承担 Platform 输入 contract 或 Scripting 输入模型设计。
- 直接影响本卡实现的上游背景包括：`Engine.App` 是 Platform 与 Scripting 之间唯一转换层；`ScriptRuntime.Update(...)` 必须在 render 前执行；`MoveOnInput` 在世界 XZ 平面移动，斜向必须归一化，且只改 position 不改 rotation/scale。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.App` 是唯一 `InputSnapshot -> ScriptInputSnapshot` 转换层。
  - main loop 固定顺序：
    1. `ProcessEvents`
    2. `IInputService.GetSnapshot`
    3. `ITimeService.Current`
    4. `SceneRuntime.Update(time, input)`
    5. `ScriptRuntime.Update(time.DeltaSeconds, time.TotalSeconds, scriptInput)`
    6. `RenderFrame`
    7. `Present`
  - built-in script registration 必须包含：
    - `RotateSelf`
    - `MoveOnInput`
  - `MoveOnInput` 必需属性：
    - `speedUnitsPerSecond: number`
  - 世界方向约定：
    - `W`: `(0, 0, -1)`
    - `S`: `(0, 0, 1)`
    - `A`: `(-1, 0, 0)`
    - `D`: `(1, 0, 0)`
  - 非零方向必须归一化；仅修改 `Transform.Position`，保留 rotation/scale。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 `ScriptInputSnapshot` 传给 `Engine.Scene`。
  - 不允许把 `InputSnapshot` 直接传进 `Engine.Scripting`。
  - 不允许把 `MoveOnInput` 做成相机相对移动、物理移动、阻尼移动或跨对象访问脚本。
  - 不允许改变 unknown script / invalid property / script update exception 的 fail-fast 与 shutdown/dispose 语义。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M18-2026-05-03 > AppIntegrationDesign` 已定稿 `ApplicationHost.Run()` 的固定顺序和唯一转换边界，执行时不得把 script update 放到 render 后或把 Platform 类型泄露到 `Engine.Scripting`。
  - `PLAN-M18-2026-05-03 > MoveOnInputDesign` 已定稿 `MoveOnInput` 的 `scriptId`、属性名、方向向量与“normalize then position += direction * speed * delta”语义，执行时不得改成别的字段名或运动模型。

## 实施说明（ImplementationNotes）
- 先在 `Engine.App` 增加输入转换辅助逻辑，把 `InputSnapshot` 中的 `EngineKey.W/A/S/D` 映射到 `ScriptKey.W/A/S/D`。
- 再调整 `ApplicationHost.Run()` 或等价主循环入口，确保在 Scene base update 之后、Render 之前执行 `ScriptRuntime.Update(..., scriptInput)`。
- 然后注册 built-in `MoveOnInput`，并实现其最小行为：
  - 读取 `context.Input`
  - 合成世界 XZ 方向
  - 非零方向归一化
  - 只更新 `context.Self.Transform` 的 position
- 接着更新 sample scene 或 App test fixture，让其声明 `MoveOnInput` 所需属性并可验证 no-input / single-key / diagonal movement。
- 最后补 App tests，覆盖：
  - `W/A/S/D` reaches script
  - no input does not move
  - diagonal normalized
  - update before render
  - fail-fast and shutdown/dispose unchanged

## 设计约束（DesignConstraints）
- 不允许在本卡内改 `Engine.Platform` 输入 contract 或 `Engine.Scripting` 输入类型形状。
- 不允许把输入方向映射、加速度、重力、碰撞或 camera-relative movement 混进 `MoveOnInput`。
- 不允许让 App 直接遍历 Scene runtime object 集合或绕过 self-object bridge 改对象。
- 不允许顺手扩展到 Editor Script UI、Play Mode、action mapping、mouse/gamepad 或对象查询 API。

## 失败与降级策略（FallbackBehavior）
- 若 sample scene 直接验证路径改动过大，允许先用 App 测试 fixture 精确验证 `MoveOnInput`，但仍必须保留至少一个真实 App 运行主路径 smoke 证据。
- 若 `MoveOnInput` 注册位置需随现有 bootstrap 结构微调，允许局部调整，但责任必须仍在 App 组合根。
- 若要完成本卡必须把 Platform 类型传进 `Engine.Scripting` 或把 script update 挪到 render 后，必须停工回退。
- 若 diagonal normalization 无法稳定验证，也不得先放“直向能动、斜向以后再说”的半成品进主路径。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/**`
  - `src/Engine.Scripting/**`
  - `src/Engine.Platform/**`
  - `src/Engine.App/SampleScenes/default.scene.json`
- 相关测试入口：
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Scripting.Tests/**`
  - `tests/Engine.Platform.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-app-011.md`
  - `.ai-workflow/tasks/task-script-003.md`
  - `.ai-workflow/tasks/task-plat-002.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M18-2026-05-03.md`
- 计划结构引用：
  - `PLAN-M18-2026-05-03 > AppIntegrationDesign`
  - `PLAN-M18-2026-05-03 > MoveOnInputDesign`
  - `PLAN-M18-2026-05-03 > PlanningDecisions`
  - `PLAN-M18-2026-05-03 > Milestones > M18.3`
  - `PLAN-M18-2026-05-03 > TestPlan`
  - 上述 integration/move design 引用属于“实现约束”，主循环顺序、方向映射和属性名已定。

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - app bootstrap/main loop integration
  - input conversion helper
  - built-in MoveOnInput registration
  - sample scene or app fixture
  - app tests
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Platform key-state foundation
- 不实现 Scripting input/public helper 内核
- 不实现 Editor script UI
- 不实现 camera-relative movement、physics、gravity、action mapping、mouse/gamepad
- OutOfScopePaths:
  - `src/Engine.Platform/**`
  - `tests/Engine.Platform.Tests/**`
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - sample scene 还是 App test fixture 作为主验证载体可按改动最小原则选择，但至少一条真实 App 主路径 smoke 证据必须保留。
- 处理规则：
  - 若问题影响主循环顺序、Platform/Scripting 边界或 `MoveOnInput` 世界方向语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 唯一转换边界、方向映射、归一化规则、固定 loop 顺序和 fail-fast 约束都已落卡。
  - 执行者无需回看计划也能明确这是一张 App 组合与行为接线卡，不是输入系统重构卡。
  - 降级方式和停工条件已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡是 M18 主链路的唯一组合点，涉及 Platform/Scripting/Scene/App 交汇但又必须保持 App 单模块实现归属。
  - 若顺序、方向或 fail-fast 语义做错，会直接把整个 interaction MVP 做偏。
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
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 no-input、单键、斜向归一化、update-before-render 与 fail-fast/shutdown
- Smoke: `W/A/S/D` 输入可抵达脚本并驱动 `MoveOnInput` 在 render 前更新自身 position；无输入时对象不移动
- Perf: 不引入逐帧双重输入转换、Platform 类型泄露、物理/碰撞模拟或 render side effect

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-APP-012.md`
- ClosedAt: `2026-05-03 14:53`
- Summary:
  - App now converts Platform `InputSnapshot` W/A/S/D state into Scripting `ScriptInputSnapshot` before script update.
  - `ApplicationHost.Run()` passes script input to `ScriptRuntime.Update(...)` after Scene update and before render.
  - Built-in `MoveOnInput` is registered and moves only self Transform.Position in world XZ, with normalized diagonal movement.
  - Default sample scene declares `MoveOnInput`; App tests cover no input, single-key movement, diagonal normalization, loop ordering, input conversion, and fail-fast stability.
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/Engine.App.csproj`
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-012.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` passed, 16/16 tests.
  - Smoke: `ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=0.05 dotnet run --project src/Engine.App/Engine.App.csproj --no-restore --nologo` exited 0.
  - Boundary: Scene/Render do not mention input/script runtime/MoveOnInput; Scripting does not reference Platform; Scene does not reference Scripting.
  - Perf: pass; App performs one Platform-to-Scripting input conversion per frame and introduces no physics/collision/render side effect.
- ModuleAttributionCheck: pass
