# 任务: TASK-QA-011 M10 数据驱动场景链路门禁复验

## TaskId
`TASK-QA-011`

## 目标（Goal）
完成 M10 Build/Test/Smoke/Perf 与设计边界复验，确认 `JSON 场景文件 -> SceneData -> Scene -> Render` 主链路成立，且 `LocalTransform` 语义和 `SceneData` 模块边界为 M11 保留兼容口。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M10-2026-04-25`

## 里程碑引用（兼容别名：MilestoneRef）
`M10.4`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Scene
- Engine.Contracts
- Engine.Render

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M10-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SDATA-002`
  - `TASK-SCENE-009`
  - `TASK-APP-009`

## 里程碑上下文（MilestoneContext）
- M10 的关单标准不是“能读 JSON”这么简单，而是 `JSON 场景文件 -> SceneData -> Scene -> Render` 真正成为主路径，并为 M11 保留 `LocalTransform` 与文档层边界兼容口。
- 本卡承担的是全链路 Build/Test/Smoke/Perf、边界复验、设计质疑处理和 M11 前置基线确认，不承担新增功能实现。
- 上游背景直接影响本卡的点包括：`SceneData` 是文档层不是运行时层；`Scene` 不直接解析 JSON；`App` 不承载场景业务逻辑；修改场景文件应可影响运行结果而无需改 C#。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `SceneData` 负责文件描述层、规范化层和显式失败语义。
  - `Scene` 负责运行时对象与渲染输入输出。
  - `App` 只负责场景文件选择和 loader 装配。
  - 结构约定已被上游定稿：
    - `PLAN-M10-2026-04-25 > SceneDataContents > 文件描述层`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 加载与失败语义`
- 本卡执行时不得推翻的既定取舍：
  - 不允许用硬编码场景回退替代 JSON 主路径通过 QA。
  - 不允许把 `LocalTransform` 误验成 world-only 且不记录风险。
  - 不允许只做单点 build/test 而跳过边界和设计复验。
  - 若实现结果与计划中定义的结构示例不一致，本卡必须把它视为设计偏移风险，而不是当作“实现细节差异”忽略。

## 实施说明（ImplementationNotes）
- 先复核 M10 相关卡的依赖方向、边界合同和归档材料是否一致，确认没有把 JSON/IO 重新塞回 `Scene` 或 `App`。
- 执行全量 Build/Test，并补跑与 SceneData、Scene 初始化、App 路径覆盖直接相关的专项测试。
- 执行 smoke，至少验证样例场景文件变更会影响启动结果、真实 mesh 仍可渲染、headless/native 至少一条主路径稳定退出。
- 最后输出 CodeQuality/DesignQuality 结论，明确 `LocalTransform` 在 M10 中的成立范围与 M11 兼容口是否保留。

## 设计约束（DesignConstraints）
- 不允许把 QA 复验退化成“只看单元测试通过”。
- 不允许以“场景能显示”掩盖模块边界回退问题。
- 不允许在 QA 卡里顺手修改功能实现；发现 must-fix 必须转卡。
- 不允许忽略 `engine-scenedata.md` 与其他边界文档之间的一致性。

## 失败与降级策略（FallbackBehavior）
- 若发现 JSON 主路径未真正生效、仍有硬编码场景后门、或 `Scene/App` 越界承担了解析逻辑，必须判为失败并回退，不得带风险放行。
- 若发现 `LocalTransform` 语义与 M11 兼容口不明确，允许先记录明确阻塞并转卡，不得模糊通过。
- 若 smoke 仅在 headless 或仅在 native 成功，必须明确说明缺失口径及风险，不得把单一路径结果包装成全通过。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.Scene/SceneGraphService.cs`
  - `src/Engine.Render/SceneRenderSubmission.cs`
- 相关测试入口：
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-002.md`
  - `.ai-workflow/tasks/task-scene-009.md`
  - `.ai-workflow/tasks/task-app-009.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-010.md`
  - 结构示例定位：
    - `PLAN-M10-2026-04-25 > SceneDataContents > 文件描述层`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 规范化场景层`
    - `PLAN-M10-2026-04-25 > SceneDataContents > 加载与失败语义`
    - `PLAN-M10-2026-04-25 > PlanningDecisions > Transform 语义`
  - 约束说明：
    - 上述计划段落中的结构示例在本卡属于“验收约束参考”，QA 需要据此判断实现是否偏离既定字段关系与语义，而不是只看运行结果。

## 范围（Scope）
- AllowedModules:
  - tests
  - workflow evidence
- AllowedFiles:
  - 全链路门禁证据
  - schema、边界与 `LocalTransform` 语义复验
- AllowedPaths:
  - `src/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 若执行中需要为验证补充源码/测试代码，命名约定为：`private`/`protected` 字段使用 `camelCase`（禁止前导下划线），参数/局部变量使用 `camelCase`，公共类型/属性/方法使用 `PascalCase`。
- 若执行中新增类型或接口，默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在说明中标注。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不新增功能需求
- 不重排 M10 优先级
- 不以硬编码场景回退替代 JSON 主路径

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若 native 路径受本地图形环境限制，是否以 headless 作为主 smoke 口径并将 native 结果降为补充证据；若会影响最终放行，需先回退确认。
- 处理规则：
  - 若问题影响是否允许进入 `Review/Done`，必须先回退，不得自行脑补放行条件。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 已明确 QA 不做实现、只做门禁、边界与设计复验。
  - 关键失败判定、专项验证面和参考入口已落卡。
  - 执行者无需回看 M10 全文，也能知道本卡必须验证 JSON 主路径是否真正生效。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 需要同时验证行为正确性、模块边界、主路径生效性和 M11 兼容口。
  - 如果验收口径写不清，很容易把“能跑”误判成“符合 M10 设计”。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - 已完成 M10 主链路任务卡输出
- ForbiddenDependsOn:
  - 未验证 `LocalTransform` 兼容语义即直接关单
  - 未验证 `SceneData` 边界即宣称完成

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
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 全量通过；SceneData loader、Scene 初始化、路径覆盖与默认值回填回归通过
- Smoke: 样例场景文件可驱动真实 mesh 渲染并稳定启动/退出
- Perf: 初始化阶段单次加载，稳定运行阶段无重复 JSON 解析或场景重建
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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-011.md`
- ClosedAt: `2026-04-26 16:00`
- Summary:
  - 复核 M10 数据驱动场景链路的 Build/Test/Smoke/Perf 与边界职责，确认 `SceneData` 负责文档层、`Scene` 负责运行时映射、`App` 负责装配与场景文件入口。
  - 汇总 `TASK-SDATA-002`、`TASK-SCENE-009`、`TASK-APP-009` 的测试与样例运行证据，确认样例场景 JSON 已可驱动场景初始化与稳定退出。
  - Human 于 `2026-04-26` 完成 M10 全量人工验收并批准关单。
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-011.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-011.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（Human 于 `2026-04-26` 确认 M10 全链路验收通过）
  - Test: `pass`（`Engine.SceneData.Tests`、`Engine.Scene.Tests`、`Engine.App.Tests` 已覆盖描述加载、运行时映射与装配路径）
  - Smoke: `pass`（样例场景 JSON 已在 headless 启动链路中完成加载、初始化并稳定退出）
  - Perf: `pass`（场景文件加载与映射仅发生在初始化阶段，无逐帧 JSON 解析或场景重载）
  - CodeQuality: `pass`（未发现新增 MustFix）
  - DesignQuality: `pass`（SRP/DIP/OCP-oriented 口径与当前实现一致）
- ModuleAttributionCheck: pass
