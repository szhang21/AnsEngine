# 任务: TASK-QA-014 M13 最小 GUI 编辑器门禁复验与归档

## TaskId
`TASK-QA-014`

## 目标（Goal）
对 M13 最小 GUI 场景编辑器执行门禁复验，确认 Editor GUI 能启动、默认打开 sample scene、选择并编辑对象、保存写回、运行时 `Engine.App` 可读取修改后的 scene，且 `Engine.Editor` / `Engine.App` / `Engine.Render` 边界未漂移。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M13-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M13.7`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.Editor.App

## 次级模块（SecondaryModules）
- Engine.Editor
- Engine.App
- Engine.Render
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M13-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EAPP-001`
  - `TASK-EAPP-002`
  - `TASK-EAPP-003`
  - `TASK-EAPP-004`
  - `TASK-EAPP-005`
  - `TASK-EAPP-006`

## 里程碑上下文（MilestoneContext）
- M13.7 是 M13 关闭门禁，不新增功能，只验证最小 GUI 编辑器工作流与模块边界。
- 本卡承担全链路 QA、人工 smoke、边界复验和归档证据准备，不承担功能修复。
- 上游直接影响本卡的背景包括：M13 目标是独立 `Engine.Editor.App`，所有真实编辑走 `SceneEditorSession`，`Engine.Editor` 保持 headless，`Engine.App` 继续作为运行时入口。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - M13 成功标准是运行 Editor GUI，默认打开 sample scene，选择对象，在 Inspector 修改字段，Save 写回 `.scene.json`。
  - 保存后运行 `Engine.App` 必须能读取修改后的场景。
  - `Engine.Editor` 不得引入 GUI、OpenTK、ImGui、窗口或输入依赖。
  - `Engine.App` 默认运行时入口不变，`Engine.Render` 不感知 Editor GUI。
- 本卡执行时不得推翻的既定取舍：
  - 不允许用“GUI 能启动”替代完整 open -> select -> edit -> save -> runtime load smoke。
  - 不允许在 QA 卡中实现功能修复；发现 must-fix 必须回退或转卡。
- 计划结构约定：
  - `PLAN-M13-2026-04-30 > Milestones > M13.7` 和 `TestPlan` 已定稿自动测试、GUI smoke、人工验收路径和边界测试。

## 实施说明（ImplementationNotes）
- 先执行自动门禁：`dotnet test AnsEngine.sln`，记录仅允许既有 `net7.0` EOL warning。
- 执行 Editor GUI smoke：`dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj`，确认窗口可启动、默认 scene 加载、关闭可控。
- 执行人工工作流：
  - 启动 Editor GUI
  - 默认打开 sample scene
  - 在 Hierarchy 选择对象
  - 在 Inspector 修改 name 或 transform
  - Save
  - 关闭 Editor
  - 运行 `Engine.App`
  - 确认运行时读取修改后的 scene
- 复核边界：
  - `Engine.Editor` 无 GUI/OpenTK/ImGui/窗口依赖
  - `Engine.Editor.App` 不复制 SceneData JSON/normalizer 规则
  - `Engine.App` 默认运行入口不变
  - `Engine.Render` 不感知 Editor GUI
- 输出 `CodeQuality`、`DesignQuality`、风险摘要和归档证据缺口清单。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中修改业务源码或补功能。
- 不允许把边界漂移标记为可接受通过；必须回退或转 follow-up。
- 不允许 QA 执行关单或把任务置为 Done；最终 Done 仍由 Human 复验后触发。
- QA 发现 must-fix 时必须保持 `MustFixDisposition=follow-up-created` 或回退原卡，不得口头放行。

## 失败与降级策略（FallbackBehavior）
- Build/Test/Smoke/Perf 任一失败，本卡不得进入 Review，必须记录失败证据并回退。
- GUI smoke 无法在当前环境运行时，必须记录环境限制、最小替代证据和剩余人工验证缺口，不能直接判 pass。
- 若发现 GUI 绕过 `SceneEditorSession` 直接改 JSON，判定高风险边界失败。
- 若保存写回后 `Engine.App` 无法读取修改 scene，判定 M13 主验收失败。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor.App/**`
  - `src/Engine.Editor/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`
  - `AnsEngine.sln`
- 相关测试入口：
  - `tests/Engine.Editor.App.Tests/**`
  - `tests/Engine.Editor.Tests/**`
  - `tests/Engine.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-eapp-001.md`
  - `.ai-workflow/tasks/task-eapp-002.md`
  - `.ai-workflow/tasks/task-eapp-003.md`
  - `.ai-workflow/tasks/task-eapp-004.md`
  - `.ai-workflow/tasks/task-eapp-005.md`
  - `.ai-workflow/tasks/task-eapp-006.md`
  - `.ai-workflow/boundaries/engine-editor-app.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M13-2026-04-30.md`
- 计划结构引用：
  - `PLAN-M13-2026-04-30 > Milestones > M13.7`
  - `PLAN-M13-2026-04-30 > TestPlan`
  - `PLAN-M13-2026-04-30 > Risks`

## 范围（Scope）
- AllowedModules:
  - Engine.Editor.App
  - Engine.Editor
  - Engine.App
  - Engine.Render
  - Engine.SceneData
- AllowedFiles:
  - QA 只读验证证据
  - 任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Editor.App/**`
  - `tests/Engine.Editor.App.Tests/**`
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不改 GUI、窗口、输入或渲染功能
- 不执行关单或看板 Done 更新
- OutOfScopePaths:
  - `src/Engine.Asset/**`
  - `src/Engine.Platform/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - GUI smoke 可能受当前图形环境影响；若无法运行，必须记录环境限制和人工补验要求。
- 处理规则：
  - 若图形环境问题影响最终验收，必须由 Human 决定是否接受替代证据，不得由 QA 自行签收。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确自动门禁、GUI smoke、人工验收路径、边界复验和质量结论字段。
  - 能直接指导 QA 判 pass/fail，而不需要再反推 M13 目标。
  - 高风险漂移点、must-fix 处理和非范围都已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - QA 覆盖 GUI 启动、文件写回、运行时加载和多模块边界复验。
  - 若边界或人工 smoke 证据不足，M13 不能可靠关闭。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Editor.App -> Engine.Editor`
  - `Engine.Editor.App -> Engine.SceneData`
  - `Engine.Editor.App -> Engine.Contracts`
  - `Engine.Editor.App -> Engine.Platform`
  - `Engine.App -> Engine.SceneData`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Editor -> Engine.Editor.App`
  - `Engine.Editor -> ImGui.NET`
  - `Engine.App -> Engine.Editor.App`
  - `Engine.Render -> Engine.Editor.App`

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
- Test: `dotnet test AnsEngine.sln` 通过；只允许既有 `net7.0` EOL warning 或已记录的本机环境 warning
- Smoke: Editor GUI 启动 -> 默认打开 sample scene -> 选择对象 -> 修改 name 或 transform -> Save -> 关闭 Editor -> 运行 `Engine.App` -> 确认运行时读取修改后的 scene
- Perf: GUI 工作流无逐帧文件 IO、无明显帧时间退化；若未测量帧时间，需记录“无新增逐帧重加载/热重载轮询”说明
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
- QAReport
- Build/Test/Smoke/Perf evidence
- CodeQuality and DesignQuality conclusions
- Risk list (high|medium|low)
- Archive readiness notes

## 状态（Status）
Todo

## 完成度（Completion）
0

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending`

## 归档（Archive）
- ArchivePath:
- ClosedAt:
- Summary:
- FilesChanged:
- ValidationEvidence:
- ModuleAttributionCheck: pass | fail
