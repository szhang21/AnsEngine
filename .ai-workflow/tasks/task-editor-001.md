# 任务: TASK-EDITOR-001 M12 Engine.Editor 模块与边界合同落地

## TaskId
`TASK-EDITOR-001`

## 目标（Goal）
新增 `Engine.Editor` 模块、测试工程、解决方案接线与边界合同，使 M12 的 headless editor core 有稳定的模块边界和可验证的工程入口，同时确保新模块不依赖 `Engine.App`、`Engine.Render`、`Engine.Platform`、`Engine.Asset`、OpenTK 或 OpenGL。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M12-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M12.1`

## 执行代理（ExecutionAgent）
Exec-Editor

## 优先级（Priority）
P0

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
- DependsOn: `[]`

## 里程碑上下文（MilestoneContext）
- M12 的首要目标不是做 GUI，而是先把 M13 将直接消费的无界面编辑器核心立起来，避免会话状态、dirty 语义和 selection 逻辑继续分散在 `SceneData` 或未来被提前塞进 `App`。
- 本卡承担的是“新模块与边界”这一个结果：解决方案里有 `Engine.Editor` / `Engine.Editor.Tests`，并明确职责、非职责、允许依赖、禁止依赖和公开接口种子。
- 上游直接影响本卡实现的背景包括：M12 明确不接入 `Engine.App` 默认启动路径；`Engine.Editor` 只组织会话和工作流，不重复实现 `SceneData` 的 JSON 读写与文档编辑规则；后续所有 Editor 卡都依赖本卡边界站位。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.Editor` 是 M12 的默认模块命名，不使用 `Engine.Tools`。
  - `Engine.Editor` 负责编辑会话、dirty、selection、保存与 reload 验证编排，不负责 GUI、窗口、输入、OpenGL、运行时场景对象或资源导入。
  - `Engine.Editor` 允许依赖 `Engine.SceneData`、`Engine.Contracts`，必要时允许依赖 `Engine.Core`。
  - `Engine.Editor` 禁止依赖 `Engine.App`、`Engine.Render`、`Engine.Platform`、`Engine.Asset`、OpenTK、OpenGL。
- 本卡执行时不得推翻的既定取舍：
  - 不允许为了“先跑起来”把 Editor session 直接接到 App 主路径。
  - 不允许把 dirty/selection/session 状态塞回 `Engine.SceneData`。
  - 不允许在本卡内预埋 GUI、面板、拾取、Gizmo、Undo/Redo 或热重载接口。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSession` 已定稿为后续主入口命名，本卡边界合同必须把它列为预期公开接口种子，而不是留给执行者自定其他宿主形状。
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSessionResult / SceneEditorFailure / SceneEditorFailureKind` 已定稿为显式结果语义方向，本卡不得把公开操作基线改回 `bool/null/exception` 主表达。

## 实施说明（ImplementationNotes）
- 先新增 `src/Engine.Editor/Engine.Editor.csproj` 与 `tests/Engine.Editor.Tests/Engine.Editor.Tests.csproj`，并接入 `AnsEngine.sln`。
- 再建立最小目录骨架与测试占位，保证后续 `SceneEditorSession`、结果类型和 session 状态机测试有固定落点。
- 同步新增 `.ai-workflow/boundaries/engine-editor.md`，写清职责、非职责、允许/禁止依赖、预期公开接口和 `Boundary Change Log`。
- 最后更新 `.ai-workflow/boundaries/README.md` 的模块映射，确保边界目录索引与新模块一致。

## 设计约束（DesignConstraints）
- 不允许在本卡内实现真正的 `Open/Save/Edit` 业务逻辑，本卡只建立模块、测试工程和边界站位。
- 不允许引入对 `Engine.App`、`Engine.Render`、`Engine.Platform`、`Engine.Asset`、OpenTK、OpenGL 的项目引用或 using 路线。
- 不允许通过“先放一个临时静态工具类”绕过后续 `SceneEditorSession` 入口设计。
- 不允许顺手改动 `Engine.App` 启动路径或 Render 主链路。

## 失败与降级策略（FallbackBehavior）
- 若解决方案接线或项目引用设计与既定边界冲突，必须停工回退修卡，不得用额外依赖打洞换取临时通过。
- 若最小测试工程暂时只能放占位测试，也允许以“工程可编译 + 无禁止依赖 + 测试入口存在”为本卡交付，但不得把后续 session 行为偷塞进本卡。
- 若发现必须新增额外跨模块公开契约才能创建模块骨架，必须回退说明，不得在本卡内私自扩张 `Engine.Contracts`。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `AnsEngine.sln`
  - `src/Engine.SceneData/Engine.SceneData.csproj`
  - `tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj`
  - `src/Engine.Core/Engine.Core.csproj`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/boundaries/README.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M12-2026-04-30.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-012.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - `PLAN-M12-2026-04-30 > Milestones > M12.1`
  - `PLAN-M12-2026-04-30 > ModuleBoundaries > Engine.Editor`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > 模块命名`
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces`
  - 上述引用在本卡属于“字段关系已定的参考实现约束”，尤其是 `SceneEditorSession*` 公开入口命名与显式结果语义方向。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor
- AllowedFiles:
  - Editor 模块与测试工程
  - 解决方案接线
  - Editor 边界合同与边界目录索引
- AllowedPaths:
  - `AnsEngine.sln`
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 `SceneEditorSession` 的打开/关闭状态机
- 不实现编辑命令、selection、dirty 或 save/reload
- 不接入 `Engine.App`、GUI、窗口、输入、渲染或运行时场景链路
- OutOfScopePaths:
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.Platform/**`
  - `src/Engine.Asset/**`
  - `src/Engine.Scene/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `Engine.Editor` 是否在本卡内预建 `Session/Results/Failures` 子目录，还是只建立最小根目录；两者都可以，但不能改变后续公开接口方向。
- 处理规则：
  - 若问题影响依赖方向、公开接口形状或模块职责，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡只交付模块落地与边界站位，单一结果明确。
  - 依赖方向、禁止路线、接口种子与新增文件位置都已落卡。
  - 无需回看里程碑全文也能知道本卡不能接 App、不能做 GUI、不能偷实现后续行为。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 涉及新模块、新测试工程、解决方案接线和边界合同同步。
  - 若依赖方向或接口种子放错，会直接污染后续全部 Editor 卡的起点。
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
  - `.ai-workflow/boundaries/README.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln` 通过，新增 `Engine.Editor` / `Engine.Editor.Tests` 均参与解算
- Test: `dotnet test AnsEngine.sln` 通过，至少存在 `Engine.Editor.Tests` 的最小测试入口
- Smoke: 不要求 GUI；最小 smoke 为解决方案可加载新模块且无禁止依赖
- Perf: 无运行时路径改动；仅允许新增编译与测试成本，无明显退化说明

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EDITOR-001.md`
- ClosedAt: `2026-04-30 00:50`
- Summary: `Engine.Editor` 与 `Engine.Editor.Tests` 已落地并接入 `AnsEngine.sln`；新增 `SceneEditorSession` 与显式结果/失败类型种子，边界合同和目录索引已同步，未引入禁止依赖。
- FilesChanged:
  - `AnsEngine.sln`
  - `src/Engine.Editor/Engine.Editor.csproj`
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `src/Engine.Editor/Session/SceneEditorSessionResult.cs`
  - `src/Engine.Editor/Session/SceneEditorFailure.cs`
  - `src/Engine.Editor/Session/SceneEditorFailureKind.cs`
  - `tests/Engine.Editor.Tests/Engine.Editor.Tests.csproj`
  - `tests/Engine.Editor.Tests/EditorModuleBoundaryTests.cs`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/boundaries/README.md`
  - `.ai-workflow/tasks/task-editor-001.md`
  - `.ai-workflow/archive/2026-04/TASK-EDITOR-001.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test AnsEngine.sln --nologo -v minimal`；Editor.Tests 4 条通过，整解测试通过，仅既有 `net7.0` EOL warning）
  - Smoke: `pass`（`Engine.Editor` 与 `Engine.Editor.Tests` 已被 solution 加载、构建和测试；边界测试确认无禁止依赖/OpenTK）
  - Perf: `pass`（未改运行时路径，仅新增编译与测试项目）
- ModuleAttributionCheck: pass
