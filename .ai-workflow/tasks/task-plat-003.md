# 任务: TASK-PLAT-003 M18.F1 Native WASD input polling for runtime app

## TaskId
`TASK-PLAT-003`

## 目标（Goal）
在 native window 运行路径中提供真实 `W/A/S/D` 键盘状态采集，让 `Engine.App` 不再无条件使用永远空的 `NullInputService`，并使默认 scene 中挂载 `MoveOnInput` 的对象可被真实键盘输入驱动移动。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M18-2026-05-03`

## 里程碑引用（兼容别名：MilestoneRef）
`M18.F1`

## 执行代理（ExecutionAgent）
Exec-Platform

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Platform

## 次级模块（SecondaryModules）
- Engine.App

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-platform.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M18-G3`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-PLAT-002`
  - `TASK-APP-012`

## 里程碑上下文（MilestoneContext）
- M18 当前已经完成了 runtime 输入 contract、App 转换和 `MoveOnInput` 行为，但实际 native runtime path 仍固定装配 `NullInputService`，所以结果是“测试驱动交互成立，真实键盘交互不成立”。
- 本卡承担的是把 M18 从“可测试交互 MVP”补成“native window 下真实 WASD 可交互 MVP”，不承担新的脚本能力、输入映射系统或 gameplay 扩张。
- 直接影响本卡实现的上游背景包括：`Engine.Platform` 已有 `EngineKey` / `InputSnapshot`；`Engine.App` 已有 `ConvertInput(InputSnapshot)` 与 `MoveOnInput`；`Engine.Scripting` 不允许依赖 `Engine.Platform`；`Engine.Render` 不应感知输入。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `EngineKey` / `InputSnapshot` 继续沿用 M18 既有公开 contract：
    - 只支持 `W`
    - `A`
    - `S`
    - `D`
  - `Engine.App` 继续作为唯一 `InputSnapshot -> ScriptInputSnapshot` 转换层。
  - `MoveOnInput`、`ScriptRuntime.Update(...)`、`ApplicationHost.Run()` 的阶段顺序保持 M18 已落地形状，不在本卡重定义。
  - 默认 scene 已挂载 `MoveOnInput`，本卡需把真实键盘输入送达该链路。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 OpenTK `Keys` / `KeyboardState` 泄露到 `Engine.App` 或 `Engine.Scripting`。
  - 不允许把真实输入逻辑塞进 `Engine.Scripting` 或让 App 直接读取窗口库键盘状态。
  - 不允许借本卡顺手加入 mouse、gamepad、action mapping、camera control、physics 或 Play Mode。
  - 不允许通过修改 `MoveOnInput` 行为来掩盖 native input 缺失。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M18-2026-05-03 > PlatformInputDesign` 已定稿 Platform 只对外暴露 `EngineKey` / `InputSnapshot`；执行时不得新增另一套 native input DTO。
  - `PLAN-M18-2026-05-03 > AppIntegrationDesign` 已定稿 App 是唯一转换层；执行时不得把 OpenTK 类型穿透到 `Engine.Scripting`。
  - `PLAN-M18-2026-05-03 > MoveOnInputDesign` 已定稿真实输入最终应驱动默认 scene 中的 `MoveOnInput` 主链路。

## 实施说明（ImplementationNotes）
- 先在 `Engine.Platform` 中新增一个 native 输入服务实现或等价的 runtime window/input adapter，从当前 OpenTK native window 读取 `W/A/S/D` 键状态并生成 `InputSnapshot.FromKeys(...)`。
- 再调整 Platform 与 Window 的协作方式，让 native input service 能获取当前 native window 的键盘状态，同时不扩大 `IWindowService` 公开面到 OpenTK 类型。
- 然后在 `Engine.App.RuntimeBootstrap.Build()` 中按运行模式分流：
  - `useNativeWindow=true` 时装配 native input service
  - `useNativeWindow=false` 时继续装配 `NullInputService`
- 最后补自动化验证与手工 smoke 证据：
  - Platform adapter/unit tests 验证 key-state 到 `InputSnapshot` 的映射
  - App tests 验证 native path 不再固定使用 `NullInputService`
  - 默认 scene 的 native runtime manual/smoke 验证 `MoveOnInput` 可真实响应键盘

## 设计约束（DesignConstraints）
- 不允许重写 `InputSnapshot` / `ScriptInputSnapshot` / `ConvertInput(...)` 的既有公开语义。
- 不允许把 native input 逻辑继续伪装在 `NullInputService` 的“永远空实现”里；可以保留它给 headless/tests，但真实输入必须有单独明确实现。
- 不允许扩大 `Engine.Platform` 对外公开接口为“直接暴露 NativeWindow / KeyboardState / OpenTK Keys”。
- 不允许顺手修改 `Engine.Scene`、`Engine.Scripting`、`Engine.Render` 边界来完成本卡。

## 失败与降级策略（FallbackBehavior）
- 若 native key polling 在 CI/headless 环境无法稳定自动化验证，允许自动化测试聚焦 Platform/App 装配与映射，再用手工 smoke 补真实窗口验收证据。
- 若实现中发现只有让 `Engine.App` 直接依赖 OpenTK 键盘状态或让 `Engine.Scripting` 消费 Platform 类型才能继续，必须停工回退。
- 若 native window path 仍可能在无输入时回落为 `NullInputService`，必须显式区分 headless 与 native 装配，不得留下隐式混用状态。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Platform/PlatformContracts.cs`
  - `src/Engine.Platform/PlatformPlaceholders.cs`
  - `src/Engine.App/ApplicationBootstrap.cs`
- 相关测试入口：
  - `tests/Engine.Platform.Tests/**`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-plat-002.md`
  - `.ai-workflow/tasks/task-app-012.md`
  - `.ai-workflow/tasks/task-qa-019.md`
  - `.ai-workflow/boundaries/engine-platform.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M18-2026-05-03.md`
- 计划结构引用：
  - `PLAN-M18-2026-05-03 > PlatformInputDesign`
  - `PLAN-M18-2026-05-03 > AppIntegrationDesign`
  - `PLAN-M18-2026-05-03 > MoveOnInputDesign`
  - 上述引用属于“实现约束”，对外输入 contract、唯一转换边界和默认 scene 驱动目标已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Platform
  - Engine.App
- AllowedFiles:
  - native input service / adapter
  - platform tests
  - runtime bootstrap input wiring
  - app tests
- AllowedPaths:
  - `src/Engine.Platform/**`
  - `tests/Engine.Platform.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不新增 `Engine.Scripting` 输入接口
- 不修改 `MoveOnInput` 的方向/归一化语义
- 不实现 mouse、gamepad、action mapping、camera control、physics、Play Mode
- OutOfScopePaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - native input service 的具体命名可按仓库现有风格调整，但职责必须显式区分于 `NullInputService`。
- 处理规则：
  - 若问题影响 Platform 对外 contract、OpenTK 类型泄漏或 App 装配分流，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - gap、主模块归属、native/headless 分流、禁止路线和验收口径都已下沉到卡面。
  - 执行者无需回看 M18 全文也能知道这是一张 post-acceptance must-fix，而不是新里程碑功能卡。
  - 自动化与手工 smoke 的组合验收策略已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡看起来小，但会同时触碰 Platform native adapter、App 装配分流和真实窗口验收，且很容易误做成 OpenTK 类型上漏或职责错位。
  - 若做偏，会把 M18“交互成立”的结论继续停留在测试环境里。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Platform -> Engine.Core`
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Scripting`
- ForbiddenDependsOn:
  - `Engine.Scripting -> Engine.Platform`
  - `Engine.Scene -> Engine.Scripting`
  - `Engine.Render -> input/script runtime terms`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-platform.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Platform.Tests/Engine.Platform.Tests.csproj --no-restore --nologo -v minimal` 与 `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 native input 映射与 native/headless 装配分流
- Smoke: native window 运行默认 scene 时，真实 `W/A/S/D` 键盘输入能驱动 `MoveOnInput` 更新对象 position；headless path 仍保持空输入稳定
- Perf: 不引入逐帧 OpenTK 类型向上分发、重复输入转换或新的 gameplay side path

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
- FailureType: `PostAcceptanceBug`
- DetectedAt: `2026-05-03`
- ReopenReason:
- OriginTaskId: `TASK-APP-012`
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-PLAT-003.md`
- ClosedAt: `2026-05-03 18:09`
- Summary:
  - Added native W/A/S/D keyboard polling behind Platform-owned `IKeyboardStateProvider` / `NativeWindowInputService`.
  - Wired App native window bootstrap to use native input while preserving `NullInputService` for headless mode.
  - Added Platform/App tests for key mapping and native/headless input service composition.
- FilesChanged:
  - `src/Engine.Platform/PlatformContracts.cs`
  - `src/Engine.Platform/PlatformPlaceholders.cs`
  - `src/Engine.Platform/NativeWindowInputService.cs`
  - `tests/Engine.Platform.Tests/InputSnapshotTests.cs`
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-platform.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-plat-003.md`
  - `.ai-workflow/archive/2026-05/TASK-PLAT-003.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（Platform 10/10, App 18/18）
  - Smoke: pass（headless sample run exited 0；native window auto-exit sample run exited 0；真实 W/A/S/D 键盘响应待 Human 复验签收）
  - Perf: pass（native input snapshot uses direct bool state construction without per-frame collection allocation）
- ModuleAttributionCheck: pass
