# 任务: TASK-EDITOR-002 M12 SceneEditorSession 打开场景与会话状态

## TaskId
`TASK-EDITOR-002`

## 目标（Goal）
在 `Engine.Editor` 落地 `SceneEditorSession` 的打开/关闭与状态查询主路径，使其可以打开 `.scene.json`、持有当前文档路径与文档快照、暴露 `HasDocument/IsDirty/SelectedObjectId/Document/Scene/Objects/SelectedObject` 等查询，并保证打开失败不会污染已有 session。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M12-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M12.2`

## 执行代理（ExecutionAgent）
Exec-Editor

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Editor

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Contracts
- Engine.Core

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M12-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EDITOR-001`

## 里程碑上下文（MilestoneContext）
- M12.2 的核心价值是让 GUI 未来首先面对的不是裸 JSON 文件，而是一个“打开中的场景”会话模型。
- 本卡承担的是打开、关闭、查询当前 session 状态和 reload 校验入口，不承担对象编辑命令与保存写回。
- 上游直接影响本卡的背景包括：`Engine.Editor` 必须消费 M11 的 `Engine.SceneData` 文档读写、规范化与失败语义；selection 只是逻辑 `object id` 状态；打开时必须做 normalize/reload 验证，保证当前文档可转换为 `SceneDescription`。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 主入口默认使用 `SceneEditorSession`。
  - 所有公开操作返回显式结果类型，不用 `bool/null` 表达失败。
  - 打开场景时需要同时持有当前文档快照与规范化场景快照，供后续 GUI 直接消费。
  - `SelectedObjectId` 默认是纯逻辑状态，打开成功后应为 `null`，`IsDirty=false`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 Editor 内重新实现 JSON 解析、默认值规范化或文档 DTO 规则。
  - 不允许把 session 设计成依赖 GUI 事件、窗口、输入、渲染高亮或拾取。
  - 不允许打开失败后半更新 session 状态。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSession` 已定稿以下查询/操作集合：`HasDocument`、`SceneFilePath`、`IsDirty`、`SelectedObjectId`、`Document`、`Scene`、`Objects`、`SelectedObject`、`Open`、`Close`、`ReloadValidate`；本卡至少要把“打开/关闭/查询/reload 校验入口”围绕这些命名落地。
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSessionResult / SceneEditorFailure / SceneEditorFailureKind` 已定稿显式结果结构方向；失败类型至少要能覆盖 `NoDocumentOpen`、`OpenFailed`、`InvalidDocument`、`ReloadValidationFailed` 或等价命名。

## 实施说明（ImplementationNotes）
- 先在 `Engine.Editor` 内建立 `SceneEditorSession`、结果类型与 failure kind/type 的最小骨架。
- 通过注入 `ISceneDocumentStore`、`ISceneDescriptionLoader` 或等价 `SceneData` 原语实现：
  - 打开 `.scene.json`
  - 持有当前路径、原始文档快照、规范化场景快照
  - 提供对象列表与选中对象只读查询
- 明确 `Open` 的状态机顺序：先读取文档，再做规范化/重载验证，全部成功后原子替换 session；任一步失败则保留旧 session。
- 实现 `Close` 清空路径、文档、规范化场景、selection，并把 `IsDirty` 复位为 false。
- 补 Editor session 测试，覆盖合法场景打开、缺失文件/非法 JSON/非法 scene 失败、不污染旧 session、关闭清空状态。

## 设计约束（DesignConstraints）
- 不允许在本卡内实现对象级编辑命令、save/save as 或 dirty 改写逻辑。
- 不允许让 `SceneEditorSession` 暴露可变集合给外部直接篡改文档。
- 不允许绕过 `SceneData` 原语直接读写 JSON 或手写第二套 normalizer。
- 不允许在 session 中持有 App、Render、Platform 或 Asset 依赖。

## 失败与降级策略（FallbackBehavior）
- 缺失文件、非法 JSON、非法 scene 等都应返回显式失败结果，而不是抛异常作为主流程控制。
- 打开失败必须保持旧 session 原封不动；若当前本来无文档，则保持空 session。
- `ReloadValidate` 失败应明确表明是文档无法转换为运行时描述，而不是把 session 置于半有效状态。
- 若实现中发现需要扩展 `SceneData` 公开契约才能继续，必须停工回退修卡，不得在 Editor 内复制逻辑绕开。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/Abstractions/ISceneDocumentStore.cs`
  - `src/Engine.SceneData/Abstractions/ISceneDescriptionLoader.cs`
  - `src/Engine.SceneData/DocumentStore/JsonSceneDocumentStore.cs`
  - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
  - `src/Engine.SceneData/FileModel/SceneFileDocument.cs`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
  - `tests/Engine.Editor.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-003.md`
  - `.ai-workflow/tasks/task-sdata-004.md`
  - `.ai-workflow/tasks/task-editor-001.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M12-2026-04-30.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - `PLAN-M12-2026-04-30 > Milestones > M12.2`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > 主入口`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > 结果语义`
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSession`
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSessionResult`
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorFailureKind`
  - 上述引用在本卡属于“字段关系已定的参考实现约束”，特别是 session 查询面和显式 failure kind 方向。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor
- AllowedFiles:
  - SceneEditorSession 与结果类型
  - Editor session 打开/关闭/查询测试
- AllowedPaths:
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现对象级编辑命令
- 不实现保存、另存为与写回磁盘
- 不修改 `Engine.SceneData` 的 JSON/normalizer 规则
- 不接入 `Engine.App` 或 GUI
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Scene/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `SceneEditorSessionResult` 成功时返回完整 session、快照对象还是仅返回 `SceneEditorFailure.None`；只要保留显式结果语义即可。
- 处理规则：
  - 若问题影响公开接口稳定性、状态原子性或失败语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - session 状态职责、打开原子性和失败语义都已落卡。
  - 查询面、禁止路线和依赖入口都已明确。
  - 执行者无需回看计划全文也能知道打开失败不能污染旧 session。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 涉及 session 状态机、原子替换、双快照持有和显式失败语义。
  - 若打开路径或状态更新顺序错了，会直接破坏后续 dirty/selection/save 的全部基础。
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

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln` 通过
- Test: Editor session 打开/关闭/失败回滚测试通过；`dotnet test AnsEngine.sln` 通过
- Smoke: 打开合法 sample scene 成功后 `HasDocument=true`、`IsDirty=false`、`SelectedObjectId=null`；关闭后全部清空
- Perf: 打开阶段允许显式 IO/normalize；不得引入逐帧状态轮询或运行时热重载

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EDITOR-002.md`
- ClosedAt: `2026-04-30 00:56`
- Summary: `SceneEditorSession` 已支持打开 `.scene.json`、原子持有当前路径/文档/规范化场景快照、查询对象列表与选中对象、关闭清空状态和 `ReloadValidate`；打开失败不会污染已有 session。
- FilesChanged:
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/tasks/task-editor-002.md`
  - `.ai-workflow/archive/2026-04/TASK-EDITOR-002.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test AnsEngine.sln --nologo -v minimal`；Editor.Tests 11 条通过，整解测试通过，仅既有 `net7.0` EOL warning）
  - Smoke: `pass`（合法 sample scene 打开后 `HasDocument=true`、`IsDirty=false`、`SelectedObjectId=null`，关闭后状态清空）
  - Perf: `pass`（打开阶段显式 IO/normalize；未引入逐帧轮询或运行时热重载）
- ModuleAttributionCheck: pass
