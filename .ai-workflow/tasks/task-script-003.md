# 任务: TASK-SCRIPT-003 M18 Scripting input context and property helper

## TaskId
`TASK-SCRIPT-003`

## 目标（Goal）
在 `Engine.Scripting` 中加入 scripting-owned 输入快照、逐帧 input 上下文和统一 property helper，并让 `ScriptRuntime.Update(...)` 把同一帧输入传给所有脚本，为 `MoveOnInput` 与后续内置脚本提供稳定输入消费面。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M18-2026-05-03`

## 里程碑引用（兼容别名：MilestoneRef）
`M18.2`

## 执行代理（ExecutionAgent）
Exec-Scripting

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Scripting

## 次级模块（SecondaryModules）
- Engine.Core
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scripting.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M18-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-PLAT-002`

## 里程碑上下文（MilestoneContext）
- M18.2 是 interaction scripting MVP 的脚本侧核心卡；没有 scripting-owned input snapshot、`ScriptContext.Input` 和统一 property helper，App 无法安全把 Platform 输入翻译给脚本，`MoveOnInput` 也没有稳定消费面。
- 本卡承担的是 `Engine.Scripting` 自有输入模型、frame context 扩展、runtime update 签名扩展和 built-in property helper，不承担 Platform 键采集、App conversion 或 `MoveOnInput` 自身接线。
- 直接影响本卡实现的上游背景包括：`Engine.Scripting` 不能依赖 `Engine.Platform`；脚本访问面已在 `TASK-SCRIPT-002` 收敛为 `SelfObject + Transform`；M18 继续保持 fail-fast 语义，invalid property 必须在 bind/initialize 前后有稳定诊断。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 新增 scripting-owned 输入类型：
    - `ScriptKey`
    - `ScriptInputSnapshot`
  - M18 `ScriptKey` 集合只包含：
    - `W`
    - `A`
    - `S`
    - `D`
  - `ScriptInputSnapshot` 必须公开：
    - `AnyInputDetected`
    - `IsKeyDown(ScriptKey key)`
    - empty input factory/default
  - `ScriptRuntime.Update(...)` 推荐形状：
    - `Update(double deltaSeconds, double totalSeconds, ScriptInputSnapshot input)`
  - property helper 至少支持：
    - required number
    - required boolean
    - required string
  - `RotateSelf` 必须迁移到同一 property helper。
- 本卡执行时不得推翻的既定取舍：
  - 不允许让 `Engine.Scripting` 直接引用 `Engine.Platform` 或 `InputSnapshot`。
  - 不允许绕过 `TASK-SCRIPT-002` 的 `SelfObject + Transform` 访问面回退到旧 `SelfTransform` 公开主入口。
  - 不允许把 property helper 扩成任意 JSON/object graph 读取器。
  - 不允许把输入上下文做成脚本可变全局状态。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M18-2026-05-03 > ScriptingInputDesign` 已定稿 `ScriptKey`、`ScriptInputSnapshot`、`ScriptContext.Input`、`WithFrame(double deltaSeconds, double totalSeconds, ScriptInputSnapshot input)` 与 `ScriptRuntime.Update(...)` 的方向，执行时不得改成 platform-owned 输入或把 input 塞进其他全局单例。
  - `PLAN-M18-2026-05-03 > PropertyHelperDesign` 已定稿 helper 的最低支持类型与 failure 语义；缺失属性、类型不匹配、非有限数必须稳定失败。
  - 计划文本里旧的 `SelfTransform` 表述以当前已落地的 `TASK-SCRIPT-002` 结果为准，M18 本卡必须延续 `SelfObject + Transform` 访问面，不得倒退。

## 实施说明（ImplementationNotes）
- 先在 `Engine.Scripting` 中新增 `ScriptKey` / `ScriptInputSnapshot`，并保持其为 scripting-owned readonly value object。
- 再扩展 `ScriptContext` 的 frame-time 字段，使其在保持 `Self`/`Properties` 等绑定期上下文的同时新增 `Input`，并将 `WithTiming(...)` 收敛为 `WithFrame(...)` 或等价更新入口。
- 然后扩展 `ScriptRuntime.Update(...)`，保证同一帧同一个 `ScriptInputSnapshot` 按既有绑定顺序传给所有脚本。
- 接着实现统一 property helper，并把内置 `RotateSelf` 迁移到 helper，补齐 missing/type mismatch/non-finite failure 诊断。
- 最后补 `Engine.Scripting.Tests`，覆盖：
  - empty/non-empty frame input
  - update order with shared frame input
  - required number/bool/string helpers
  - bad property deterministic failure
  - `RotateSelf` helper migration 不退化

## 设计约束（DesignConstraints）
- 不允许在本卡引入 `Engine.Platform` 项目引用或复制 Platform 类型定义。
- 不允许让 `Engine.Scripting` 知道 OpenTK、窗口服务或原始平台事件。
- 不允许在本卡实现 `MoveOnInput` 最终 sample 行为或 App input conversion。
- 不允许顺手扩展到 mouse/gamepad/action mapping、跨对象查询、更多组件访问或 Editor Script UI。

## 失败与降级策略（FallbackBehavior）
- 若 `WithTiming(...)` 兼容层需要暂时保留以减少改动范围，允许短期保留过渡入口，但对外主路径必须已切到携带 input 的 frame update 语义。
- 若 property helper 的具体类型命名需要按仓库风格微调，允许等价命名，但 failure 语义和最低支持类型不得削弱。
- 若实现中发现只有让 `Engine.Scripting` 直接依赖 `Engine.Platform` 才能继续，必须停工回退。
- 若 invalid property 只能靠运行时异常字符串拼装且无法稳定诊断，也必须回退修卡，而不是留下脆弱失败语义。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scripting/**`
  - `src/Engine.App/**`
  - `src/Engine.Platform/**`
- 相关测试入口：
  - `tests/Engine.Scripting.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Platform.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-script-001.md`
  - `.ai-workflow/tasks/task-script-002.md`
  - `.ai-workflow/tasks/task-plat-002.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M18-2026-05-03.md`
- 计划结构引用：
  - `PLAN-M18-2026-05-03 > ScriptingInputDesign`
  - `PLAN-M18-2026-05-03 > PropertyHelperDesign`
  - `PLAN-M18-2026-05-03 > PlanningDecisions`
  - `PLAN-M18-2026-05-03 > Milestones > M18.2`
  - `PLAN-M18-2026-05-03 > TestPlan`
  - 上述 scripting/property design 引用属于“参考实现约束”，键集合、helper failure 语义与 frame update 形状已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Scripting
- AllowedFiles:
  - scripting input snapshot
  - script context/runtime update path
  - property helper
  - scripting tests
- AllowedPaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Platform key polling
- 不实现 App input conversion 与 `MoveOnInput` 注册
- 不扩展到 mouse、gamepad、action mapping
- 不扩展脚本对象查询或更多 native component 访问
- OutOfScopePaths:
  - `src/Engine.Platform/**`
  - `tests/Engine.Platform.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `WithTiming(...)` 是否保留一层兼容过渡可按实现最小扰动决定，但最终主路径必须明确承载 `Input`。
- 处理规则：
  - 若问题影响 `Engine.Scripting` 对 `Engine.Platform` 的零依赖、`SelfObject + Transform` 访问面或 property helper failure 语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 脚本侧输入模型、helper 最低支持范围、旧 `SelfTransform` 文本的修正规则和 update 主路径都已下沉到卡面。
  - 执行者无需回看计划也能知道本卡是 Scripting 内核收口，不是 App 行为卡。
  - 停工条件与可接受过渡策略已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这张卡会同时改脚本公开上下文、runtime update 签名、property failure 语义和既有 built-in script 行为入口，认知负担高。
  - 若把 input ownership 或 `SelfObject + Transform` 方向做偏，会直接破坏 M17 后续边界。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scripting -> Engine.Core`
  - `Engine.Scripting -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Scripting -> Engine.Platform`
  - `Engine.Scripting -> Engine.Scene`
  - `Engine.Scripting -> Engine.Render`
  - `Engine.Scripting -> Engine.Editor`
  - `Engine.Scripting -> Engine.Editor.App`
  - `Engine.Scripting -> Engine.Asset`

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
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 frame input、update order、helper failure 与 `RotateSelf` helper migration
- Smoke: `ScriptRuntime.Update(...)` 将同一帧 `ScriptInputSnapshot` 传给所有已绑定脚本；empty input 不报错；invalid property 在绑定/初始化前后能稳定诊断失败
- Perf: 不引入逐帧平台类型转换、反射属性扫描、JSON 解析或全局可变脚本输入状态

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCRIPT-003.md`
- ClosedAt: `2026-05-03 14:30`
- Summary:
  - Added scripting-owned `ScriptKey` with W/A/S/D and `ScriptInputSnapshot` with empty/non-empty key-state semantics.
  - Added `ScriptContext.Input` and `WithFrame(...)`; `ScriptRuntime.Update(...)` now has an input-bearing overload while preserving the old empty-input compatibility path.
  - Added `ScriptPropertyReader` required number/boolean/string helpers with stable missing/type mismatch/non-finite failures.
  - Added Scripting tests for frame input propagation, shared input across bound scripts, helper behavior, invalid property failure, and RotateSelf helper usage in the scripting test fixture.
- FilesChanged:
  - `src/Engine.Scripting/ScriptKey.cs`
  - `src/Engine.Scripting/ScriptInputSnapshot.cs`
  - `src/Engine.Scripting/ScriptPropertyReader.cs`
  - `src/Engine.Scripting/ScriptContext.cs`
  - `src/Engine.Scripting/ScriptRuntime.cs`
  - `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/tasks/task-script-003.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Test: `dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal` passed, 17/17 tests.
  - Smoke: `ScriptRuntime.Update(..., ScriptInputSnapshot)` passes the same frame input to all bound scripts; empty input compatibility path still succeeds.
  - Boundary: `Engine.Scripting` has no `Engine.Platform` project/source dependency; actual `Engine.Scene` references are absent from `src/Engine.Scripting`.
  - Perf: pass; no platform conversion, reflection property scanning, JSON parsing, or global mutable input state introduced.
- ModuleAttributionCheck: pass
