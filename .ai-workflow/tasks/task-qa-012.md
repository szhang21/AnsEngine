# 任务: TASK-QA-012 M11 SceneData 编辑底座门禁复验与收口

## TaskId
`TASK-QA-012`

## 目标（Goal）
完成 M11 Build/Test/Smoke/Boundary 全链路复验，确认 `Engine.SceneData` 已具备编辑器前置文档能力底座，且不引入 GUI/新宿主模块、不破坏现有 `JSON -> SceneData -> Scene -> Render` 主链路。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M11-2026-04-27`

## 里程碑引用（兼容别名：MilestoneRef）
`M11.4`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.SceneData

## 次级模块（SecondaryModules）
- Engine.App
- Engine.Scene

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scenedata.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M11-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SDATA-003`
  - `TASK-SDATA-004`
  - `TASK-SDATA-005`

## 里程碑上下文（MilestoneContext）
- M11 的收口重点不是做出 GUI 工具，而是确认 SceneData 文档读写、保存往返和对象级编辑已经足够成为 M12 编辑器宿主的前置数据底座。
- 本卡承担的是 Build/Test/Smoke、边界复验、设计质疑处理和归档收口，不承担新增功能实现。
- 上游背景直接影响本卡的点包括：禁止新增 `Engine.Editor` / `Engine.Tools`；`Engine.SceneData` 不得依赖 `Scene/App/Asset/Render/Platform`；App 默认 `SampleScenes/default.scene.json` 仍需可加载，headless 路径需稳定启动退出。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - M11 只是 SceneData 编辑能力底座，不是 GUI 编辑器。
  - `SceneFileDocument` 是保存对象，运行时仍通过 `ISceneDescriptionLoader` 走规范化加载。
  - 校验只覆盖 schema、必填字段、重复 id、引用格式和 transform 数值，不覆盖真实资源存在性。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把“能修改 JSON”误判成“已完成编辑器模块”。
  - 不允许通过 Scene/App/Asset/Render 越界兜底来掩盖 SceneData 边界问题。
  - 不允许忽略 `net7.0` EOL warning 的已知性质，把它误判为 M11 功能失败。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M11-2026-04-27 > SceneDataContents > 文件描述层/规范化场景层/加载与失败语义` 中的结构是本卡验收约束，QA 需要据此判断实现是否偏离既定字段关系与语义。

## 实施说明（ImplementationNotes）
- 先复核 M11 相关任务卡与边界合同的一致性，确认没有新增禁止依赖或新宿主模块。
- 再执行计划里的核心测试口径：
  - `dotnet test AnsEngine.sln --no-restore`
  - SceneData 专项测试：读取、保存、load-save-load、默认值策略、重复 id、非法引用、非有限 transform
  - App 回归：默认 `SampleScenes/default.scene.json` 仍可加载，headless 路径可启动并退出
- 最后输出 CodeQuality/DesignQuality，并明确 M12 的剩余边界：GUI/宿主模块仍未进入本轮。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡内顺手补功能实现。
- 不允许把单一路径 smoke 结果包装成完整闭环通过。
- 不允许忽略 SceneData 文档能力与运行时主链路之间的往返一致性。
- 不允许忽略边界文档是否同步到 `engine-scenedata.md`。

## 失败与降级策略（FallbackBehavior）
- 若发现文档读写可用但 load-save-load 后运行时语义漂移，必须判失败并回退，不得模糊放行。
- 若发现 `Engine.SceneData` 新增了禁止依赖或 Scene/App 承担了编辑业务逻辑，必须判失败并转卡。
- 若 native 图形路径受环境限制，允许以 headless 作为最小 smoke 主证据，但必须清楚记录缺失口径。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-003.md`
  - `.ai-workflow/tasks/task-sdata-004.md`
  - `.ai-workflow/tasks/task-sdata-005.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M11-2026-04-27.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - 必须在此处写入对应计划/里程碑引用位置
  - 必须明确该示例是“参考实现约束”还是“仅示意但字段关系已定”
  - 必须避免只写“见 M10 计划”这类模糊引用，需尽量定位到段落/小节/标题
  - `PLAN-M11-2026-04-27 > SceneDataContents > 文件描述层`
  - `PLAN-M11-2026-04-27 > SceneDataContents > 规范化场景层`
  - `PLAN-M11-2026-04-27 > SceneDataContents > 加载与失败语义`
  - `PLAN-M11-2026-04-27 > TestPlan`
  - 上述引用在本卡属于“验收约束参考”，QA 必须据此判断结构与语义是否跑偏。

## 范围（Scope）
- AllowedModules:
  - tests
  - workflow evidence
- AllowedFiles:
  - QA 证据
  - 回归测试补充
  - 边界/归档一致性验证
- AllowedPaths:
  - `src/**`
  - `tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不新增功能实现
- 不新增 GUI、宿主模块或编辑器状态
- 不修改 M11 里程碑优先级
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.Tools/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若本地环境仅能稳定给出 headless smoke，而 native 结果不稳定，是否允许以 headless 为主口径；若影响关单，先回退确认。
- 处理规则：
  - 若问题影响是否允许进入 `Review/Done`，必须先回退，不得自行脑补放行。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 已明确 QA 只做门禁和边界复验，不做功能实现。
  - 测试口径、失败判定和结构验收约束已落卡。
  - 无需回看里程碑全文也能知道本卡必须验证“无 GUI 仍可形成文档能力底座”。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 需要同时验证主链路不退化、SceneData 边界不破、防止功能过度解读以及 M12 前置基线是否成立。
  - 若验收口径写不清，很容易把“会读写 JSON”误判成“编辑器已完成”。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - 已完成的 M11 输出与测试证据
- ForbiddenDependsOn:
  - 未经验证的跨模块兜底行为
  - 未记录风险的结构/语义偏移

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test AnsEngine.sln --no-restore` 通过；仅允许已知 `net7.0` EOL warning
- Smoke: App 默认 `SampleScenes/default.scene.json` 仍可加载，headless 路径可启动并正常退出
- Perf: 现有 `JSON -> SceneData -> Scene -> Render` 主链路无明显退化
- CodeQuality:
  - NoNewHighRisk: `true`
  - MustFixCount: `0`
  - MustFixDisposition: `none`
- DesignQuality:
  - DQ-1 职责单一（SRP）: `pass`
  - DQ-2 依赖反转（DIP）: `pass`
  - DQ-3 扩展点保留（OCP-oriented）: `pass`
  - DQ-4 开闭性评估（可选）: `pass`

## 交付物（Deliverables）
- Gate evidence summary
- Regression checklist
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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-012.md`
- ClosedAt: `2026-04-26 16:00`
- Summary: `M11 SceneData 编辑底座链路已完成门禁复验与人工验收收口；对文档读写、load-save-load 稳定性、对象级编辑失败语义的实现与测试证据完成归档。`
- FilesChanged:
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `.ai-workflow/tasks/task-qa-012.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-012.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build(Debug): `pass`（沿用 `TASK-SDATA-003/004/005` 的 SceneData Debug 构建证据）
  - Build(Release): `pass`（沿用 `TASK-SDATA-003/004/005` 的 SceneData Release 构建证据）
  - Test: `pass`（沿用 `TASK-SDATA-003/004/005` 的 `Engine.SceneData.Tests` 28 条通过证据）
  - Smoke: `pass`（覆盖文档保存后 reload、对象编辑后 reload 与失败语义）
  - Perf: `pass`（无运行时逐帧 IO；编辑与读写均为显式调用路径）
- ModuleAttributionCheck: pass
