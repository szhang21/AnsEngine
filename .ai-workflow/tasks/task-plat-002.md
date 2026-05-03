# 任务: TASK-PLAT-002 M18 Platform key-state input snapshot foundation

## TaskId
`TASK-PLAT-002`

## 目标（Goal）
在 `Engine.Platform` 中建立最小可验证的按键状态输入快照地基，让 `InputSnapshot` 能稳定表达 `W/A/S/D` 键状态并保持空输入、多键输入与 `AnyInputDetected` 语义一致。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M18-2026-05-03`

## 里程碑引用（兼容别名：MilestoneRef）
`M18.1`

## 执行代理（ExecutionAgent）
Exec-Platform

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Platform

## 次级模块（SecondaryModules）
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-platform.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M18-G1`
- CanRunParallel: `false`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M18.1 是 interaction scripting MVP 的最底层输入地基；如果 Platform 侧没有稳定、只读、与上层无枚举泄漏的输入快照，后续 Scripting 和 App 都只能用假输入或直接耦合 OpenTK。
- 本卡承担的是 `Engine.Platform` 自身的 key-state snapshot contract 与空输入语义，不承担 Scripting input 类型、App conversion、`MoveOnInput` 或任何 scene/runtime 行为。
- 直接影响本卡实现的上游背景包括：M18 第一版只支持 `W/A/S/D`；若原生 OpenTK 轮询风险较高，可以先落 contract + stub/test path；但 Platform 内部仍要为后续原生接线保留稳定形状。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 在 `Engine.Platform` 新增 `EngineKey`，M18 key set 只包含：
    - `W`
    - `A`
    - `S`
    - `D`
  - `InputSnapshot` 必须保留：
    - `AnyInputDetected`
    - `IsKeyDown(EngineKey key)`
  - `InputSnapshot` 必须支持：
    - empty input
    - single key input
    - multiple key input
  - `NullInputService` 返回 empty input。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 OpenTK 枚举或窗口库类型泄露到 `Engine.App` / `Engine.Scripting`。
  - 不允许在本卡扩展 mouse、gamepad、action mapping 或事件总线。
  - 不允许让 `InputSnapshot` 暴露可变集合。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M18-2026-05-03 > PlatformInputDesign` 已定稿 `EngineKey`、`InputSnapshot.IsKeyDown(...)`、`AnyInputDetected`、readonly value object 和 empty/single/multiple key 支持，执行时不得改成另一套输入模型。
  - `PLAN-M18-2026-05-03 > PlanningDecisions` 已定稿 M18 第一版只支持 `W/A/S/D`，不得擅自扩张按键集合。

## 实施说明（ImplementationNotes）
- 先在 `Engine.Platform` 中建立 `EngineKey` 与扩展后的 `InputSnapshot` 公开形状。
- 再实现 empty input、单键输入、多键输入和 `AnyInputDetected` 一致性逻辑，优先保持其为小型只读值对象。
- 然后校准 `NullInputService` 返回空快照，并补 Platform tests 覆盖：
  - empty input
  - single key
  - multiple keys
  - `AnyInputDetected`
  - `IsKeyDown` 正反断言
- 若已有原生 OpenTK 输入轮询入口可安全接线，可顺带让 native path 填充该快照；若风险过高，允许先保持 contract + stub/test path。

## 设计约束（DesignConstraints）
- 不允许让 `Engine.Platform` 依赖 `Engine.Scene`、`Engine.Scripting`、`Engine.App` 或 `Engine.Render`。
- 不允许在本卡把 `InputSnapshot` 做成面向事件回放、映射表或可序列化 gameplay 数据模型。
- 不允许把 `AnyInputDetected` 与 pressed keys 状态做成不一致的双事实源。
- 不允许顺手加入鼠标、滚轮、Gamepad、文本输入或输入动作映射。

## 失败与降级策略（FallbackBehavior）
- 若 native OpenTK key polling 接线会引入不稳定或超出 `AllowedPaths` 的扩散，允许只交付 contract + `NullInputService` + tests，不阻塞后续 App conversion 卡。
- 若为了复用现有代码必须泄露 OpenTK 键枚举到上层，必须停工回退，不得接受该路线。
- 若实现中发现 `InputSnapshot` 需要可变集合或引用类型共享状态才能工作，必须先回退修卡，而不是留下隐藏可变性。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Platform/**`
  - `src/Engine.App/**`
- 相关测试入口：
  - `tests/Engine.Platform.Tests/**`
  - `tests/Engine.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-app-010.md`
  - `.ai-workflow/tasks/task-app-011.md`
  - `.ai-workflow/boundaries/engine-platform.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M18-2026-05-03.md`
- 计划结构引用：
  - `PLAN-M18-2026-05-03 > PlatformInputDesign`
  - `PLAN-M18-2026-05-03 > PlanningDecisions`
  - `PLAN-M18-2026-05-03 > Milestones > M18.1`
  - `PLAN-M18-2026-05-03 > TestPlan`
  - 上述 PlatformInputDesign 引用属于“参考实现约束”，键集合、只读快照形状和 `IsKeyDown` 字段关系已定。

## 范围（Scope）
- AllowedModules:
  - Engine.Platform
- AllowedFiles:
  - input snapshot contract
  - platform input service
  - platform tests
- AllowedPaths:
  - `src/Engine.Platform/**`
  - `tests/Engine.Platform.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Scripting input 类型
- 不实现 App input conversion
- 不实现 `MoveOnInput`
- 不扩展到 mouse、gamepad、action mapping
- OutOfScopePaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - native OpenTK polling 是否在本卡直接接线，可按风险决定；但对外 `InputSnapshot` 形状必须先定稳。
- 处理规则：
  - 若问题影响键枚举泄漏、快照只读性或 Platform 边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已把最小按键集合、只读快照形状、可延后 native polling 的策略和禁止扩张方向都下沉了。
  - 执行者无需回看计划也能知道这是一张 Platform contract foundation 卡，而不是完整输入系统卡。
  - 失败回退与停工条件已明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 以单模块为主，但同时要守住对上层无枚举泄漏、空输入语义一致和 native/stub 两条可选落地路径。
  - 若快照形状做偏，会直接影响后续 Scripting/App 输入桥。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Platform -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Platform -> Engine.Scene`
  - `Engine.Platform -> Engine.Scripting`
  - `Engine.Platform -> Engine.App`
  - `Engine.Platform -> Engine.Render`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-platform.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Platform.Tests/Engine.Platform.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 empty/single/multiple key 与 `IsKeyDown`/`AnyInputDetected`
- Smoke: `NullInputService` 返回 empty input；`InputSnapshot` 可独立表达 `W/A/S/D` 与多键组合
- Perf: 不引入逐帧可变集合复制、跨模块回调链或窗口库枚举向上泄漏

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-PLAT-002.md`
- ClosedAt: `2026-05-03 14:27`
- Summary:
  - Added `EngineKey` with M18 W/A/S/D key set.
  - Extended `InputSnapshot` into a readonly key-state snapshot with `Empty`, `FromKeys(...)`, `AnyInputDetected`, and `IsKeyDown(...)`.
  - Updated `NullInputService` to return empty input.
  - Added Platform tests for empty/single/multiple key snapshots, null input, legacy constructor compatibility, and forbidden dependency checks.
- FilesChanged:
  - `src/Engine.Platform/PlatformContracts.cs`
  - `src/Engine.Platform/PlatformPlaceholders.cs`
  - `tests/Engine.Platform.Tests/Engine.Platform.Tests.csproj`
  - `tests/Engine.Platform.Tests/InputSnapshotTests.cs`
  - `tests/Engine.Platform.Tests/PlatformBoundaryTests.cs`
  - `.ai-workflow/boundaries/engine-platform.md`
  - `.ai-workflow/tasks/task-plat-002.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Test: `dotnet restore tests/Engine.Platform.Tests/Engine.Platform.Tests.csproj --nologo -v minimal` then `dotnet test tests/Engine.Platform.Tests/Engine.Platform.Tests.csproj --no-restore --nologo -v minimal` passed, 7/7 tests.
  - Smoke: `NullInputService` returns `InputSnapshot.Empty`; `InputSnapshot.FromKeys(...)` independently expresses W/A/S/D and multi-key combinations.
  - Boundary: `src/Engine.Platform` has no `Engine.Scene`, `Engine.Scripting`, `Engine.App`, or `Engine.Render` references.
  - Perf: pass; key states are stored as a small value snapshot without per-frame mutable collection copying.
- ModuleAttributionCheck: pass
