# 任务: TASK-EDITOR-006 M21 Editor component authoring core APIs

## TaskId
`TASK-EDITOR-006`

## 目标（Goal）
在 `Engine.Editor.SceneEditorSession` 中补齐 `Script`、`RigidBody`、`BoxCollider` 的更新与移除 API，使 headless editor core 能稳定支撑 M21 的组件 authoring、normalize、dirty 和 selection 语义。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M21-2026-05-05`

## 里程碑引用（兼容别名：MilestoneRef）
`M21.2`

## 执行代理（ExecutionAgent）
Exec-Editor

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.Editor

## 次级模块（SecondaryModules）
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M21-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EAPP-009`

## 里程碑上下文（MilestoneContext）
- M21.2 是本轮 editor authoring 的 headless 地基卡；如果 `SceneEditorSession` 不先承接 Script/Physics 组件编辑，GUI 层只能绕过 session 直接改 SceneData，边界会立刻做坏。
- 本卡承担的是会话级更新/移除 API、normalize 与失败语义编排，不承担 GUI 输入缓冲、JSON 文本解析 UI、preview 渲染或 runtime physics 接线。
- 直接影响本卡实现的上游背景包括：M16 已将 Editor 迁移到 component-oriented path；M17/M19 已在 SceneData 中落地 `SceneFileScriptComponentDefinition`、`SceneFileRigidBodyComponentDefinition`、`SceneFileBoxColliderComponentDefinition`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `SceneEditorSession` 需要新增以下方法：
    - `UpdateObjectScriptComponent(string objectId, SceneFileScriptComponentDefinition script)`
    - `RemoveObjectScriptComponent(string objectId, string scriptId)`
    - `UpdateObjectRigidBodyComponent(string objectId, SceneFileRigidBodyComponentDefinition rigidBody)`
    - `RemoveObjectRigidBodyComponent(string objectId)`
    - `UpdateObjectBoxColliderComponent(string objectId, SceneFileBoxColliderComponentDefinition boxCollider)`
    - `RemoveObjectBoxColliderComponent(string objectId)`
  - 实现模式必须沿用现有 `UpdateObjectTransformComponent` / `UpdateObjectMeshRendererComponent` 路线：
    - validate document exists
    - update component list through `UpdateObjectComponents(...)`
    - normalize document
    - update `mScene`
    - set `IsDirty`
    - preserve selection where possible
  - `RigidBody` 和 `BoxCollider` 可以独立存在并成功保存；这不等于对象一定参与 runtime physics。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 `Engine.Editor` 内重新实现 JSON 解析、property 文本解析或 SceneData normalizer 规则。
  - 不允许让 `Engine.Editor` 依赖 GUI、Render、Asset、Scene、App 或 OpenTK/ImGui。
  - 不允许移除 `Transform` 作为 editor 基础组件假设。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Session Component Editing API` 已定稿方法签名与步骤骨架，执行时不得自行改成模糊的通用 `UpdateComponent(object)` 风格。
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Editor.App Controller And Inspector` 已定稿 file-model 组件定义复用要求，执行时不得新造平行 DTO。

## 实施说明（ImplementationNotes）
- 先在 `SceneEditorSession` 内补 Script/RigidBody/BoxCollider 的 update/remove 公开入口，并沿用现有结果类型与失败语义。
- 再把内部组件列表更新逻辑收敛到现有 `UpdateObjectComponents(...)` 路径，避免为每种组件复制不同的 normalize/save 状态机。
- 然后补 editor tests，至少覆盖：
  - Script add/update/remove
  - Script properties round-trip 保留 number/bool/string
  - RigidBody `Static|Dynamic` 与 mass normalize
  - BoxCollider size/center save-reload
  - invalid collider size / invalid mass 的稳定失败
  - RigidBody-only 与 BoxCollider-only 可保存但不视为 physics-ready

## 设计约束（DesignConstraints）
- 不允许在 `Engine.Editor` 内新增任何 OpenTK、ImGui、Render、Asset、Scene 或 App 依赖。
- 不允许把多个组件编辑 API 抽成过度泛化的“任意组件编辑总线”，从而模糊 M21 需要的显式失败语义。
- 不允许把 Script properties JSON 文本解析逻辑放进 `Engine.Editor`；这里最多消费已构造好的 `SceneFileScriptComponentDefinition`。
- 不允许顺手扩展 Undo/Redo、批量编辑、typed property grid 或 runtime physics 参与判定。

## 失败与降级策略（FallbackBehavior）
- 若现有内部 helper 命名与组件 API 不完全匹配，允许局部重构 `SceneEditorSession` 内部结构，但必须保持公开结果语义稳定。
- 若某个组件移除路径会破坏 selection/dirty 约束，必须优先保住 session 状态机正确性，再调整实现细节；不得以“先能保存”为由吞掉状态错误。
- 若实现中发现需要改 `Engine.SceneData` 公开 schema 或引入 GUI 依赖才可继续，必须停工回退；这超出本卡边界。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
- 相关测试入口：
  - `tests/Engine.Editor.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-editor-005.md`
  - `.ai-workflow/tasks/task-sdata-008.md`
  - `.ai-workflow/tasks/task-sdata-009.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M21-2026-05-05.md`
- 计划结构引用：
  - `PLAN-M21-2026-05-05 > TechnicalDesign > Session Component Editing API`
  - `PLAN-M21-2026-05-05 > Milestones > M21.2`
  - `PLAN-M21-2026-05-05 > TestPlan`
  - 上述 API/method 引用属于“实现约束”，方法名、组件定义复用和状态机顺序已定稿。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor
- AllowedFiles:
  - SceneEditorSession and related result/helper files
  - Engine.Editor tests
- AllowedPaths:
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 GUI Inspector 输入缓冲或按钮工作流
- 不实现 Scene View preview
- 不修改 SceneData schema 或 runtime physics bridge
- OutOfScopePaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `RemoveObjectScriptComponent` 对多脚本对象是按 `scriptId` 删除首个匹配项还是要求唯一匹配；可按最小惊讶原则实现为“删除首个 scriptId 匹配项”，但若会破坏现有 Script component 顺序保留语义则需回退。
- 处理规则：
  - 若问题影响 SceneData schema、headless 边界或公开结果语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 方法签名、状态机顺序、允许组件独立存在的语义和测试口径都已写明。
  - 执行者无需再翻计划就能知道本卡是 headless core API 卡，不是 GUI/preview 卡。
  - 停工条件和禁止扩张方向明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 editor authoring 的业务底座，既有公开 API 形状约束，又有状态机和失败语义约束。
  - 如果路线选错，很容易把 JSON/UI/schema 责任重新混进 `Engine.Editor`。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor -> Engine.SceneData`
  - `Engine.Editor -> Engine.Contracts`
  - `Engine.Editor -> Engine.Core`
- ForbiddenDependsOn:
  - `Engine.Editor -> Engine.App`
  - `Engine.Editor -> Engine.Render`
  - `Engine.Editor -> Engine.Platform`
  - `Engine.Editor -> Engine.Asset`
  - `Engine.Editor -> Engine.Scene`
  - `Engine.Editor -> Engine.Editor.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.Editor.Tests/Engine.Editor.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 Script/RigidBody/BoxCollider 增删改、round-trip 和 invalid normalize failure
- Smoke: headless `open -> edit -> save -> reload` 路径可承载新组件，不污染 dirty/selection 语义
- Perf: 无逐帧 IO、无 GUI 依赖、无 duplicated SceneData normalization path

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Review

## 完成度（Completion）
`95`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-EDITOR-006.md`
- ClosedAt: `2026-05-05`
- Summary:
  - Added explicit `SceneEditorSession` APIs for Script, RigidBody and BoxCollider update/remove.
  - Script update replaces the first matching `scriptId` or appends when absent; removal deletes the first matching script.
  - RigidBody and BoxCollider use the existing component update/normalize path and can exist independently.
  - Editor tests cover script property round-trip, physics component save/reload, invalid normalize failures and boundary safety.
- FilesChanged:
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/tasks/task-editor-006.md`
  - `.ai-workflow/archive/2026-05/TASK-EDITOR-006.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL warnings only)
  - Test: pass (`dotnet test tests/Engine.Editor.Tests/Engine.Editor.Tests.csproj --no-restore --nologo -v minimal`; 34 passed)
  - Smoke: pass (headless open -> edit Script/RigidBody/BoxCollider -> save -> reload covered by session tests)
  - Boundary: pass (`Engine.Editor.csproj` still references only Core, Contracts, SceneData; boundary tests assert no App/Render/Platform/Asset/OpenTK)
  - Perf: pass (no GUI dependency, no duplicated normalizer path, no per-frame IO)
- ModuleAttributionCheck: pass
