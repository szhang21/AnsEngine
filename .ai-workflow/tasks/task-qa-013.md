# 任务: TASK-QA-013 M12 GUI 编辑器前置底座门禁复验与归档

## TaskId
`TASK-QA-013`

## 目标（Goal）
对 M12 的 `Engine.Editor` headless core 执行门禁复验与收口，确认打开/编辑/保存闭环可用、边界未漂移为 GUI 或 App 接入、保存与状态语义无回归，并准备归档所需的验证证据。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M12-2026-04-30`

## 里程碑引用（兼容别名：MilestoneRef）
`M12.5`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.Editor

## 次级模块（SecondaryModules）
- Engine.SceneData
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-editor.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M12-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-EDITOR-001`
  - `TASK-EDITOR-002`
  - `TASK-EDITOR-003`
  - `TASK-EDITOR-004`

## 里程碑上下文（MilestoneContext）
- M12.5 是里程碑关闭门禁，不再新增功能，而是验证 `Engine.Editor` 已经形成 M13 GUI 可直接消费的无界面核心。
- 本卡承担的是全链路 QA、边界复验和归档证据收口，不承担功能实现。
- 上游直接影响本卡的背景包括：M12 明确不引入 GUI、窗口、输入、渲染交互或 App 默认接线；保存后的文件必须仍可被现有运行时 loader 加载；新增文件必须有边界文档与 change log 对应记录。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.Editor` 只做 headless core，不做 GUI、鼠标拾取、视口交互、Undo/Redo、资源浏览器或热重载。
  - `Engine.Editor` 消费 `Engine.SceneData` 文档原语，不重复实现 JSON 规则。
  - `Engine.App` 在 M12 不改变默认启动路径，`Engine.Render` 不感知 Editor。
  - 保存成功必须经过 reload/normalize 验证。
- 本卡执行时不得推翻的既定取舍：
  - 不允许用“行为没问题”为理由接受禁止依赖或职责越界。
  - 不允许把 QA 收口扩成新的功能改造卡。
  - 不允许忽略 `CodeQuality` / `DesignQuality` 结论。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces > SceneEditorSession` 与相关结果类型是本卡复核公开面是否跑偏的固定参照。
  - `PLAN-M12-2026-04-30 > PlanningDecisions > Dirty 语义 / Selection 语义 / 保存语义 / App 接入` 是本卡验证行为与边界是否一致的定稿约束。

## 实施说明（ImplementationNotes）
- 先复核 `TASK-EDITOR-001~004` 的代码改动、边界文档更新与 `AllowedPaths` 命中情况。
- 再执行 M12 的核心验证闭环：
  - 打开合法场景
  - 选择与编辑对象
  - 保存与另存为
  - reload 验证保存结果
  - 校验失败路径不会污染 session
- 单独做边界复验，确认：
  - `Engine.Editor` 无禁止依赖
  - `Engine.App` 默认启动路径未接入 Editor
  - `Engine.Render` 主路径未感知 Editor
  - 未引入 GUI/输入/拾取/Undo/Redo
- 最后产出 `CodeQuality`、`DesignQuality`、风险摘要和归档证据缺口清单。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中实现功能修复；发现 must-fix 必须回退并要求转卡或返工。
- 不允许接受“测试过了但边界漂移”这类结果。
- 不允许把 `Engine.Editor` 对 `SceneData` 的调用复制成独立 JSON 规则而仍判通过。
- 不允许省略保存失败后 dirty 保持 true、打开失败不污染旧 session 这两类关键回归点。

## 失败与降级策略（FallbackBehavior）
- 若 Build/Test/Smoke/Perf 任一门禁失败，本卡必须回退并记录缺口，不得进入 `Review`。
- 若发现禁止依赖、越界职责或 `MustFixCount>0`，必须保持 `MustFixDisposition=follow-up-created` 或回退原卡，不能口头放行。
- 若验证中发现 M12 实际已接入 App 或 GUI 路线，必须判定为高风险边界失败，不做“先合后修”。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Editor/**`
  - `src/Engine.SceneData/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `AnsEngine.sln`
- 相关测试入口：
  - `tests/Engine.Editor.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-editor-001.md`
  - `.ai-workflow/tasks/task-editor-002.md`
  - `.ai-workflow/tasks/task-editor-003.md`
  - `.ai-workflow/tasks/task-editor-004.md`
  - `.ai-workflow/boundaries/engine-editor.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M12-2026-04-30.md`
- 若计划/里程碑中存在示例数据结构或字段示例：
  - `PLAN-M12-2026-04-30 > Milestones > M12.5`
  - `PLAN-M12-2026-04-30 > TestPlan`
  - `PLAN-M12-2026-04-30 > ModuleBoundaries > Engine.Editor`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > Dirty 语义`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > Selection 语义`
  - `PLAN-M12-2026-04-30 > PlanningDecisions > 保存语义`
  - `PLAN-M12-2026-04-30 > SuggestedPublicInterfaces`
  - 上述引用在本卡属于“字段关系已定的参考实现约束”，用于判定公开面、状态语义和边界职责是否偏离计划。

## 范围（Scope）
- AllowedModules:
  - Engine.Editor
  - Engine.SceneData
  - Engine.App
  - Engine.Render
- AllowedFiles:
  - QA 只读验证证据
  - 任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Editor/**`
  - `tests/Engine.Editor.Tests/**`
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不修改 GUI、窗口、输入或渲染功能
- OutOfScopePaths:
  - `src/Engine.Platform/**`
  - `src/Engine.Asset/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若 `dotnet test AnsEngine.sln` 仅出现既有 `net7.0` EOL warning，按计划视为可接受噪音，不判 must-fix。
- 处理规则：
  - 若问题影响门禁结论、must-fix 归属或里程碑是否可关闭，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已经明确 QA 要复核的行为闭环、边界闭环和质量结论字段。
  - 能直接指导 QA 判 pass/fail，而不需要再反推 M12 目标。
  - 高风险漂移点、must-fix 处理和非范围都已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 需要同时验证功能闭环、边界闭环、质量结论和归档门禁。
  - 若 QA 口径不完整，容易把“能跑”误当作“里程碑可关”。
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
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-editor.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln` 通过
- Test: `dotnet test AnsEngine.sln` 通过；只允许已有 `net7.0` EOL warning
- Smoke: `open -> edit -> save -> reload` 闭环通过，失败路径不污染 session
- Perf: 无逐帧 IO、无运行时热重载、无明显额外启动路径退化
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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-013.md`
- ClosedAt: `2026-04-30 01:21`
- Summary: `M12 Engine.Editor headless core 已完成门禁复验；打开/编辑/保存/reload 闭环通过，边界保持无 GUI、无 App 接入、无 Render 感知。`
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-013.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-013.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/plan-archive/2026-04/PLAN-M12-2026-04-30.md`
  - `.ai-workflow/plan-archive/plan-archive-index.md`
- ValidationEvidence:
  - Build: `pass`（`dotnet test AnsEngine.sln --no-restore` 完成构建与测试，仅既有 `net7.0` EOL warning）
  - Test: `pass`（整解测试通过；Editor.Tests 26 条通过；Render.Tests 16 条专项通过）
  - Smoke: `pass`（M12 已覆盖 `open -> edit -> save -> reload`，保存成功清 dirty，失败路径保留内存状态）
  - Boundary: `pass`（`Engine.Editor` 保持 headless core；未接入 App 默认启动路径，Render 不感知 Editor）
- ModuleAttributionCheck: pass
