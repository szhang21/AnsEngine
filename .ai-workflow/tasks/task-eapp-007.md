# 任务: TASK-EAPP-007 M13 Docked Editor Layout

## TaskId
`TASK-EAPP-007`

## 目标（Goal）
将 `Engine.Editor.App` 的编辑器工作区收敛为稳定的 Unity-like 停靠布局：`Toolbar` 顶部停靠、`Hierarchy` 左侧停靠、`Inspector` 右侧停靠、`Status Bar` 底部停靠，并保证现有选择、编辑、保存和对象增删工作流在该布局下继续可用。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M13-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M13.2`

## 执行代理（ExecutionAgent）
Exec-EditorApp

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.Editor.App

## 次级模块（SecondaryModules）
- Engine.Editor

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M13-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EAPP-006`

## 里程碑上下文（MilestoneContext）
- M13 当前功能链路已经覆盖宿主、布局、Hierarchy、Inspector、Open/Save 和 Add/Remove，但这些卡的目标主要是“功能可用”，并没有把最终工作区骨架明确收敛成固定停靠布局。
- 本卡承担的是最终布局收口，只处理四个主区域的停靠关系、尺寸分配和可见性，不新增新的编辑业务能力。
- 上游直接影响本卡实现的背景包括：Toolbar、Hierarchy、Inspector、Status Bar 都已经有功能语义；所有真实编辑仍必须走 `SceneEditorSession`；`Engine.Editor.App` 是 GUI 宿主，`Engine.Editor` 继续保持 headless core。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - M13 第一版接受工具型 ImGui 风格，不追求产品级复杂布局系统。
  - `Toolbar`、`Hierarchy`、`Inspector`、`Status Bar` 是既定主区域，不能被 viewport、资源浏览器或其他新面板稀释。
  - 所有真实编辑继续走 `SceneEditorSession`，GUI 布局只能影响展示与交互承载位置，不能改变业务真相来源。
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 Docked Layout 做成新的多窗口系统、可视 viewport、复杂 docking 框架或主题工程。
  - 不允许为了布局方便回退为“自由漂浮窗口 + 面板错位”，从而破坏 Unity-like 工作区预期。
  - 不允许在本卡中改变 `Hierarchy`、`Inspector`、`Save`、`Add/Remove` 的底层业务语义。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M13-2026-04-30 > Milestones > M13.2` 已定稿四个主区域及其职责，本卡是在该结构上进一步把停靠关系显式固定下来，而不是另起新面板体系。
  - `PLAN-M13-2026-04-30 > PlanningDecisions` 中“工具型 ImGui 风格”与“所有真实编辑必须走 SceneEditorSession”是本卡不可改写的上游约束。

## 实施说明（ImplementationNotes）
- 先梳理现有 `Engine.Editor.App` GUI frame/render 组织方式，识别 Toolbar、Hierarchy、Inspector、Status Bar 的当前渲染入口和尺寸分配点。
- 再把布局收敛为稳定停靠骨架：
  - `Toolbar` 固定顶部横向停靠
  - `Hierarchy` 固定左侧纵向面板
  - `Inspector` 固定右侧纵向面板
  - `Status Bar` 固定底部横向停靠
  - 中央剩余区域保持为当前主工作区或留白，不新增 viewport 职责
- 明确在窗口尺寸变化时四个区域仍保持语义稳定，不发生 Toolbar/Status Bar 被遮挡、Hierarchy/Inspector 反向漂移或主区域完全挤压。
- 最后补最小布局验证，至少覆盖：窗口启动后四区位置正确、默认场景状态可见、现有选择/编辑/保存/Add/Remove 在新布局下仍能操作。

## 设计约束（DesignConstraints）
- 不允许在本卡内新增 viewport、dock tabs、多窗口、资源浏览器或主题系统。
- 不允许把布局实现依赖下沉到 `Engine.Editor` 或改动 `SceneEditorSession` 公开接口。
- 不允许顺手重写已有 Toolbar、Hierarchy、Inspector、Status Bar 的业务逻辑，只允许为布局承载做最小适配。
- 不允许因为布局调整而让状态栏信息、Inspector 表单或 Hierarchy 列表失去可见性或可操作性。

## 失败与降级策略（FallbackBehavior）
- 若当前 ImGui 组织方式不适合复杂 docking，允许采用固定分栏/分区布局实现 Unity-like 观感，只要停靠关系明确稳定即可。
- 若布局调整导致现有选择、编辑、保存或增删对象工作流失效，必须回退并记录为门禁失败，不得以“只是 UI 变化”放行。
- 若实现中发现必须修改 `Engine.Editor`、`Engine.SceneData` 或 `Engine.App` 才能继续，必须停工回退修卡，不得越界扩张。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/Session/SceneEditorSession.cs`
  - `src/Engine.Editor.App/EditorGuiSnapshot.cs`
  - `src/Engine.Editor.App/EditorAppWindow.cs`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-002.md`
  - `.ai-workflow/tasks/task-eapp-003.md`
  - `.ai-workflow/tasks/task-eapp-004.md`
  - `.ai-workflow/tasks/task-eapp-005.md`
  - `.ai-workflow/tasks/task-eapp-006.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M13-2026-04-30.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - `PLAN-M13-2026-04-30 > Milestones > M13.2`
  - `PLAN-M13-2026-04-30 > PlanningDecisions`
  - 上述引用在本卡属于“字段关系已定的参考实现约束”，尤其是四个主区域的固定职责和工具型 ImGui 风格方向。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
- AllowedFiles:
  - GUI 布局与面板停靠组织
  - 必要的布局验证测试
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不新增 viewport、dock tabs、多窗口或资源浏览器
- 不新增新的编辑功能或修改现有保存/编辑业务语义
- 不修改 `Engine.Editor`、`Engine.SceneData`、`Engine.App` 的公开接口或默认路径策略
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 左右分栏的具体默认宽度比例可由 Execution 在 `Engine.Editor.App` 内决定，只要 Toolbar/Hierarchy/Inspector/Status Bar 的停靠关系稳定且不遮挡内容。
- 处理规则：
  - 若问题影响四区职责、现有工作流可用性或模块边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡的结果、停靠关系、禁止路线和依赖位置都已明确。
  - 现有功能链路作为前置已经完成，本卡只做最终布局收口。
  - 无需回看里程碑全文也能理解为什么这张卡不能扩成 viewport/docking 系统或改写业务逻辑。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L2`
- Why:
  - 单模块 GUI 收口任务，但涉及多个已落地交互面的共同承载和窗口尺寸变化下的稳定性。
  - 如果布局路线选错，会直接破坏当前 M13 已完成的交互可用性和最终编辑器骨架观感。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
  - `Engine.Editor.App -> Engine.Contracts`
  - `Engine.Editor.App -> Engine.Platform`
- ForbiddenDependsOn:
  - `Engine.Editor.App -> Engine.App`
  - `Engine.Editor.App -> Engine.Render`
  - `Engine.Editor.App -> Engine.Asset`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln` 通过
- Test: `dotnet test AnsEngine.sln` 通过，布局/面板停靠相关测试通过
- Smoke: 启动 Editor GUI 后 `Toolbar` 顶部、`Hierarchy` 左侧、`Inspector` 右侧、`Status Bar` 底部的停靠关系稳定可见，且现有选择/编辑/保存/Add/Remove 在该布局下继续可用
- Perf: 布局收口不引入逐帧文件 IO、重复 session open 或明显 GUI 帧时间退化

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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-EAPP-007.md`
- ClosedAt: `2026-05-01 03:24`
- Summary: Editor GUI 收敛为固定停靠布局：Toolbar 顶部、Hierarchy 左侧、Workspace 中央、Inspector 右侧、Status Bar 底部；布局尺寸进入 GUI snapshot 并补测试。
- FilesChanged:
  - `src/Engine.Editor.App/EditorGuiSnapshot.cs`
  - `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
  - `src/Engine.Editor.App/EditorGuiRenderer.cs`
  - `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
  - `tests/Engine.Editor.App.Tests/EditorObjectWorkflowStateTests.cs`
  - `tests/Engine.Editor.App.Tests/EditorInspectorInputStateTests.cs`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/tasks/task-eapp-007.md`
  - `.ai-workflow/archive/2026-04/TASK-EAPP-007.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet build AnsEngine.sln --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；`Engine.Editor.App.Tests` 31 条通过）
  - Smoke: `pass`（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`；真实 ImGui frame 启动并关闭，退出码 0）
  - Boundary: `pass`（仅改 `src/Engine.Editor.App/**`、`tests/Engine.Editor.App.Tests/**` 与任务指定边界/归档文档；未改 `Engine.Editor`、`SceneData`、`Engine.App` 或 `Render`）
  - Perf: `pass`（布局只消费 GUI snapshot 与 ImGui window bounds，无逐帧文件 IO、重复 session open 或新业务轮询）
- ModuleAttributionCheck: pass
