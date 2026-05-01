# 任务: TASK-QA-015 M14 Runtime Object Model 门禁复验与归档

## TaskId
`TASK-QA-015`

## 目标（Goal）
对 M14 的 runtime object/component foundation 执行全量测试、Scene/App/Render 回归和边界复验，确认 `Engine.Scene` 已成为轻量 runtime scene owner，同时未引入脚本、物理、动画、Editor Viewport 或 Play Mode。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M14-2026-05-01`

## 里程碑引用（兼容别名：MilestoneRef）
`M14.6`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P3

## 主模块归属（PrimaryModule）
Engine.Scene

## 次级模块（SecondaryModules）
- Engine.App
- Engine.Render
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scene.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M14-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-010`
  - `TASK-SCENE-011`
  - `TASK-SCENE-012`
  - `TASK-SCENE-013`
  - `TASK-SCENE-014`

## 里程碑上下文（MilestoneContext）
- M14.6 是运行时对象模型里程碑的关闭门禁，不再新增功能，而是验证 `Engine.Scene` 的内部形态迁移是否已经稳定完成，且外部链路没有退化。
- 本卡承担的是全量测试、Scene/App/Render 回归、边界文档复核和归档证据，不承担实现功能。
- 上游直接影响本卡的背景包括：Render 仍只消费 `SceneRenderFrame`；App 继续用现有 `SceneGraphService` facade；SceneData 不感知 runtime object/component；M14 不引入脚本、物理、动画、Editor Viewport 或 Play Mode。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.Scene` 开始落地轻量 runtime object/component model，但不新增模块。
  - `SceneGraphService` 作为过渡期 runtime scene owner。
  - `Engine.Render` 仍只消费 `Engine.Contracts.SceneRenderFrame`。
  - M14 不扩到 ECS、hierarchy、world transform propagation、runtime update loop、脚本、物理、动画、editor viewport 或 play mode。
- 本卡执行时不得推翻的既定取舍：
  - 不允许因为测试通过就忽略 runtime component 泄露到 Render/App/Editor 的边界漂移。
  - 不允许在 QA 卡中补实现或改业务源码。
  - 不允许把 Scene 内部 snapshot/query 当作新的跨模块公共可变模型接受放行。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M14-2026-05-01 > ModuleDesign`、`RuntimeModel`、`PublicQuerySurface` 和 `SceneGraphServiceChanges` 是本卡判定 runtime model 是否按计划落地的固定参照。
  - `PLAN-M14-2026-05-01 > OutOfScope` 是本卡判断是否越界的重要硬约束。

## 实施说明（ImplementationNotes）
- 先复核 `TASK-SCENE-010~014` 的改动文件、`AllowedPaths`、边界文档更新和禁止依赖情况。
- 执行自动验证：
  - `dotnet test AnsEngine.sln`
  - `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj`
- 核验关键场景：
  - 空对象 scene、单对象 scene、多对象 scene
  - 重复 load 清空旧 runtime state
  - `BuildRenderFrame` item count 与 runtime mesh renderer object count 一致
  - transform/camera 语义稳定
  - snapshot/query 为只读值对象
- 单独做边界复验，确认：
  - `Engine.Scene` 无 `Render/Asset/App/Editor/Editor.App/OpenTK/ImGui/OpenGL` 禁止依赖
  - `Engine.Render` 不引用 runtime component 类型
  - `Engine.SceneData` 不感知 runtime object/component
  - M14 未引入脚本、物理、动画、Editor Viewport 或 Play Mode
- 最后产出 `CodeQuality`、`DesignQuality`、风险摘要和归档证据缺口。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中实现功能修复或顺手重构。
- 不允许把“Render/App 没报错”当作内部 runtime model 质量通过的替代证据。
- 不允许忽略只读 snapshot/query 是否泄露可变 runtime 集合。
- 不允许跳过 `Engine.App.Tests`、`Engine.Render.Tests`、`Engine.SceneData.Tests` 的回归验证。

## 失败与降级策略（FallbackBehavior）
- 若 Build/Test/Smoke/Perf 任一门禁失败，本卡不得进入 `Review`，必须记录失败证据并回退。
- 若发现 runtime object/component 已成为 Render/App/Editor 的直接依赖事实，必须判定为高风险边界失败。
- 若 `MustFixCount>0`，必须保持 `MustFixDisposition=follow-up-created` 或回退原卡，不得口头放行。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`
  - `AnsEngine.sln`
- 相关测试入口：
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Render.Tests/**`
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-010.md`
  - `.ai-workflow/tasks/task-scene-011.md`
  - `.ai-workflow/tasks/task-scene-012.md`
  - `.ai-workflow/tasks/task-scene-013.md`
  - `.ai-workflow/tasks/task-scene-014.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M14-2026-05-01.md`
- 计划结构引用：
  - `PLAN-M14-2026-05-01 > TestPlan`
  - `PLAN-M14-2026-05-01 > BoundaryUpdates`
  - `PLAN-M14-2026-05-01 > OutOfScope`
  - `PLAN-M14-2026-05-01 > Risks`

## 范围（Scope）
- AllowedModules:
  - Engine.Scene
  - Engine.App
  - Engine.Render
  - Engine.SceneData
- AllowedFiles:
  - QA 只读验证证据
  - 任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Render/**`
  - `tests/Engine.Render.Tests/**`
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不执行 `Review -> Done`
- 不引入新的 runtime/update/editor 能力
- OutOfScopePaths:
  - `src/Engine.Asset/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若全量测试通过但某些 smoke 只能通过 Scene 测试旁证而非独立窗口验证，需要在 QA 报告里明确证据口径，不能直接降格门禁。
- 处理规则：
  - 若问题影响门禁结论、must-fix 归属或是否可以关闭 M14，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确全量测试、回归验证、边界复验和质量结论字段。
  - 能直接作为 M14 终局 QA 卡执行，不需要再反推运行时对象模型的成功标准。
  - 高风险漂移点、非范围和 must-fix 处理都已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 需要同时验证 runtime model、render contract 稳定性、App/Render/SceneData 回归和边界泄露风险。
  - 若 QA 口径不完整，很容易把“Scene.Tests 通过”误当成“运行时对象模型里程碑已稳”。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scene -> Engine.Core`
  - `Engine.Scene -> Engine.Contracts`
  - `Engine.Scene -> Engine.SceneData`
  - `Engine.App -> Engine.SceneData`
  - `Engine.Render -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.Scene -> Engine.Render`
  - `Engine.Scene -> Engine.Asset`
  - `Engine.Scene -> Engine.App`
  - `Engine.Scene -> Engine.Editor`
  - `Engine.Scene -> Engine.Editor.App`
  - `Engine.Render -> Engine.Scene` runtime component types

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln` 通过
- Test: `dotnet test AnsEngine.sln` 通过；`Engine.Scene.Tests` 覆盖 runtime object model；`Engine.App.Tests`、`Engine.Render.Tests`、`Engine.SceneData.Tests` 不退化
- Smoke: 加载空对象/单对象/多对象 scene、重复 load、render frame 输出、camera 语义、snapshot/query 只读性都能通过验证
- Perf: 不引入逐帧文件 IO、无明显 runtime frame build 退化；若未测量帧时间，需明确“无新增逐帧重解析/热重载轮询”说明
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-015.md`
- ClosedAt: `2026-05-01 13:29`
- Summary:
  - M14 全量 Build/Test 通过，Scene/App/Render/SceneData 回归通过
  - 复核 `TASK-SCENE-010~014` 均处于 Review，归档证据齐全
  - 空/单/多对象 scene、重复 load、render frame 输出、camera 语义、snapshot/query 只读性均由 Scene.Tests 覆盖
  - 边界复验未发现 runtime component 泄露到 Render/App/Editor/SceneData，未发现脚本/物理/动画/Gizmo/Play Mode/update loop 越界
  - CodeQuality: NoNewHighRisk=true, MustFixCount=0, MustFixDisposition=none
  - DesignQuality: DQ-1 pass, DQ-2 pass, DQ-3 pass, DQ-4 pass
- FilesChanged:
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/tasks/task-qa-015.md`
  - `.ai-workflow/archive/2026-05/TASK-QA-015.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M14-2026-05-01.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 整解通过；Scene.Tests 30 条、App.Tests 6 条、Render.Tests 16 条、SceneData.Tests 28 条均通过）
  - Smoke: pass（空/单/多对象、重复 load、render frame item count/transform/camera、snapshot/query 只读性均由 Scene.Tests 与专项回归覆盖）
  - Boundary: pass（禁止依赖与 runtime component 泄露搜索通过；非范围搜索未发现脚本、物理、动画、Gizmo、Play Mode 或 update loop）
  - Perf: pass（无新增逐帧文件 IO、热重载轮询、scene rebuild 或 runtime update loop）
- ModuleAttributionCheck: pass
