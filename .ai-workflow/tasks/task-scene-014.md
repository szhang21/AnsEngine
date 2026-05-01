# 任务: TASK-SCENE-014 M14 Runtime Snapshot 查询与边界测试

## TaskId
`TASK-SCENE-014`

## 目标（Goal）
在 `SceneGraphService` 增加只读 runtime 查询面，例如 `CreateRuntimeSnapshot()` / `FindObject(...)`，并补齐 snapshot 与边界测试，确保测试和未来系统可读 runtime state，而不暴露内部可变集合。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M14-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M14.5`

## 执行代理（ExecutionAgent）
Exec-Scene

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.Core
- Engine.Contracts
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M14-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-013`

## 里程碑上下文（MilestoneContext）
- M14.5 的价值在于给测试和后续系统一个只读可观察面，证明 runtime scene 已经成为稳定内部事实，而不是仅靠 render frame 旁证。
- 本卡承担的是 snapshot/query 和边界测试，不承担新的业务功能或 contract 变更。
- 上游直接影响本卡的背景包括：snapshot 是只读值对象；默认通过 `SceneGraphService` 的只读查询验证，不暴露内部可变集合；`Engine.Render` 不引用 runtime component 类型，`Engine.SceneData` 不感知 runtime object/component。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 建议在 `SceneGraphService` 增加 `CreateRuntimeSnapshot()` 和 `FindObject(string objectId)`。
  - 建议 snapshot 类型为 `RuntimeSceneSnapshot`、`SceneRuntimeObjectSnapshot`。
  - Snapshot 至少包含 node id、object id、object name、local transform、mesh/material 或 `HasMeshRenderer` + refs、camera snapshot。
  - Snapshot 是只读值对象，外部不能通过 snapshot 修改 runtime state。
- 本卡执行时不得推翻的既定取舍：
  - 不允许为了测试方便直接公开 runtime object 可变集合。
  - 不允许让 `Engine.Render`、`Engine.SceneData`、`Engine.App` 开始依赖 runtime component 类型。
  - 不允许把 snapshot 设计成新的跨模块公共可变模型。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M14-2026-05-01 > ModuleDesign` 中 `SceneRuntimeObjectSnapshot`、`RuntimeSceneSnapshot` 已定稿为上游只读查询命名方向。
  - `PLAN-M14-2026-05-01 > PublicQuerySurface` 已定稿 snapshot 最小字段集合和 `FindObject(string objectId)` 方向，本卡执行时不得把查询面偷换成暴露内部容器。

## 实施说明（ImplementationNotes）
- 先为 runtime scene 和 runtime object 设计只读 snapshot 值对象。
- 在 `SceneGraphService` 上增加只读查询面，例如：
  - `CreateRuntimeSnapshot()`
  - `FindObject(string objectId)`
- 确保查询面只返回值对象或只读投影，不泄露内部可变引用。
- 补 Scene 测试与边界测试，覆盖：snapshot 字段完整、可按 object id 查询、外部无法通过 snapshot 修改内部 state、`Engine.Render` / `Engine.SceneData` 不感知 runtime component 类型。

## 设计约束（DesignConstraints）
- 不允许在本卡内暴露 `internal` runtime object 集合作为公开/准公开 API。
- 不允许把 snapshot 设计成跨模块共享的行为对象或带 mutation 方法的对象。
- 不允许顺手扩到 editor、app 或 render 的新查询入口。
- 不允许在本卡内引入 `Engine.Scene -> Engine.Editor` 或其他禁止依赖。

## 失败与降级策略（FallbackBehavior）
- 若 `FindObject` 的返回形式在值对象还是可空值上有实现差异，允许局部选择，但必须保持“只读、无可变集合泄露”的主约束。
- 若测试确需观察更多字段，可在 snapshot 上补只读字段，但不得反向暴露 runtime component 本体。
- 若实现中发现测试只能通过直接访问内部集合完成，必须停工回退修卡，不得绕过只读查询面设计。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Scene/**`
  - `src/Engine.Contracts/SceneRenderContracts.cs`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
  - `tests/Engine.Render.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-010.md`
  - `.ai-workflow/tasks/task-scene-011.md`
  - `.ai-workflow/tasks/task-scene-012.md`
  - `.ai-workflow/tasks/task-scene-013.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M14-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M14-2026-05-01 > PublicQuerySurface`
  - `PLAN-M14-2026-05-01 > Milestones > M14.5`
  - `PLAN-M14-2026-05-01 > BoundaryUpdates`
  - `PLAN-M14-2026-05-01 > TestPlan`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
- AllowedFiles:
  - runtime snapshot/query
  - Scene 测试
  - 边界回归测试
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.Render.Tests/**`
  - `tests/Engine.SceneData.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 Render 或 SceneData 的业务实现
- 不新增新的 runtime update/persistence 能力
- 不扩展 scene JSON schema
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `FindObject(...)` 是返回可空 snapshot 还是显式查询结果类型；两者都可，但必须保持只读和值对象语义。
- 处理规则：
  - 若问题影响只读查询面、边界隔离或测试是否能避免直接接触内部集合，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 查询面、snapshot 最小字段、边界测试目标和禁止路线都已明确。
  - 本卡和前序 runtime 构建卡、后序 QA 卡切分清楚。
  - 无需回看计划全文也能知道本卡不能以“测试方便”为由暴露内部可变集合。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及 runtime state 可观察面、只读值对象设计和跨模块边界防泄露。
  - 若查询面设计失守，会把 runtime object/component 变成新的跨模块公共事实。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.Asset`
  - `Engine.Scene -> Engine.App`
  - `Engine.Scene -> Engine.Editor`
  - `Engine.Scene -> Engine.Editor.App`

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
- Build: `dotnet build AnsEngine.sln` 通过
- Test: `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj` 通过，snapshot/query/边界测试通过；`Engine.Render.Tests` 和 `Engine.SceneData.Tests` 不退化
- Smoke: 可按 object id 查询 runtime object snapshot；snapshot 不暴露可变 runtime 集合；`Engine.Render` 不引用 runtime component 类型
- Perf: 查询面基于只读 snapshot，不引入逐帧额外 scene rebuild 或文件读取

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCENE-014.md`
- ClosedAt: `2026-05-01 10:39`
- Summary:
  - `SceneGraphService` 新增 `CreateRuntimeSnapshot()` 与 `FindObject(string objectId)` 只读查询面
  - runtime snapshot 覆盖 object identity、transform、mesh/material 与 camera state
  - snapshot/query 不暴露内部 runtime object/component 可变集合
  - 新增边界测试确认 `Engine.Scene` 不依赖 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL，Render/SceneData 不反向依赖 runtime component 类型
- FilesChanged:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-scene-014.md`
  - `.ai-workflow/archive/2026-05/TASK-SCENE-014.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`，Scene.Tests 30 条通过；`Engine.Render.Tests` 16 条通过；`Engine.SceneData.Tests` 28 条通过；`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过）
  - Smoke: pass（可按 object id 查询 runtime object snapshot；snapshot 是只读值对象，不暴露 runtime 内部集合；Render 不引用 runtime component 类型）
  - Boundary: pass（仅改 AllowedPaths 与边界/任务/归档文档；未新增禁止依赖）
  - Perf: pass（snapshot 遍历 runtime object 当前状态，不触发 scene rebuild 或文件读取）
- ModuleAttributionCheck: pass
