# 任务: TASK-QA-023 M22 Runtime Abstractions gate review and archive

## TaskId
`TASK-QA-023`

## 目标（Goal）
对 M22 Runtime Abstractions 与 Scene/Scripting 对齐执行门禁复验，确认依赖方向、接口最小性、Scene render/script/physics 主路径行为和边界合同同步均满足计划要求，并准备归档证据。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M22-2026-05-11`

## 里程碑引用（兼容别名：MilestoneRef）
`M22.6`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.Runtime.Abstractions

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Scripting
- Engine.App
- Engine.Render
- Engine.SceneData

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-runtime-abstractions.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M22-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-RABS-001`
  - `TASK-SCENE-021`
  - `TASK-SCENE-022`
  - `TASK-SCENE-023`
  - `TASK-SCRIPT-004`

## 里程碑上下文（MilestoneContext）
- M22.6 是 M22 关闭门禁，不新增功能，只验证 Runtime.Abstractions foundation 是否稳定、边界是否同步、主路径是否无回归。
- 本卡覆盖 Runtime.Abstractions、Scene、Scripting、App bridge、Render/SceneData 反向依赖检查。
- 上游直接影响本卡的背景包括：M22 不承诺新 visible feature，但必须通过现有 runtime behavior 验证 Scene load、script transform update、physics writeback before render 和 render frame 输出不退化。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Runtime.Abstractions 只依赖 Contracts。
  - Scene implements runtime object/component abstractions。
  - Scripting consumes runtime abstractions and still forbids Scene dependency。
  - Render and SceneData must not reference Runtime.Abstractions in M22。
  - Transform remains independent core spatial field；MeshRenderer is first generic runtime component。
- 本卡执行时不得推翻的既定取舍：
  - QA 不实现功能修复。
  - 不允许以“测试通过”放行边界漂移。
  - 不允许 Execution/QA 自行关单到 Done。
- 计划结构约定：
  - QA 必须验证 `dotnet build AnsEngine.sln --nologo -v minimal` 与 `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`。

## 实施说明（ImplementationNotes）
- 执行 build/test 门禁并记录 warning，尤其注意当前 SDK/target framework warning 是否为既有噪音。
- 执行依赖边界检查：
  - Runtime.Abstractions only references Contracts。
  - Scene may reference Runtime.Abstractions but not Scripting/Physics。
  - Scripting may reference Runtime.Abstractions but not Scene。
  - Render and SceneData do not reference Runtime.Abstractions。
- 执行 API shape 检查：Runtime.Abstractions 不包含 Update、FindObject、AddComponent、RemoveComponent、Parent、Children、Scene 或 cross-object query API。
- 执行主路径回归：Scene load produces same render frame output；script update modifies self Transform；physics writeback resolves script-driven movement before render；Render still consumes `SceneRenderFrame` only。
- 检查边界合同更新：
  - `.ai-workflow/boundaries/engine-runtime-abstractions.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - 必要时 `.ai-workflow/boundaries/engine-app.md`
- 输出 `CodeQuality`、`DesignQuality`、风险摘要和归档准备结论。

## 设计约束（DesignConstraints）
- 不允许在 QA 卡中修改业务源码或补实现。
- 不允许接受 Runtime.Abstractions API 过宽作为 “future proofing”。
- 不允许忽略 Render/SceneData 对 Runtime.Abstractions 的反向依赖。
- 不允许 QA 执行最终 Done 或看板关单。

## 失败与降级策略（FallbackBehavior）
- Build/Test 任一失败，本卡必须回退，不能进入 Review。
- 若发现 MustFix（依赖漂移、API 过宽、Transform double ownership、Render/SceneData 引用 Runtime.Abstractions），必须要求回退原卡或转 follow-up，不得口头放行。
- 若图形/runtime smoke 环境受限，必须记录替代证据和人工补验缺口，由 Human 决定是否接受。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Runtime.Abstractions/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Scripting/**`
  - `src/Engine.App/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`
  - `AnsEngine.sln`
- 相关测试入口：
  - `tests/Engine.Runtime.Abstractions.Tests/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.Scripting.Tests/**`
  - `tests/Engine.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-rabs-001.md`
  - `.ai-workflow/tasks/task-scene-021.md`
  - `.ai-workflow/tasks/task-scene-022.md`
  - `.ai-workflow/tasks/task-scene-023.md`
  - `.ai-workflow/tasks/task-script-004.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M22-2026-05-11.md`
- 计划结构引用：
  - `PLAN-M22-2026-05-11 > Milestones > M22.6`
  - `PLAN-M22-2026-05-11 > RuntimeRealityCheck`
  - `PLAN-M22-2026-05-11 > TestPlan`

## 范围（Scope）
- AllowedModules:
  - Engine.Runtime.Abstractions
  - Engine.Scene
  - Engine.Scripting
  - Engine.App
  - Engine.Render
  - Engine.SceneData
- AllowedFiles:
  - QA 只读验证证据、任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Runtime.Abstractions/**`
  - `tests/Engine.Runtime.Abstractions.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `src/Engine.Render/**`
  - `src/Engine.SceneData/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现功能修复
- 不修改 public API
- 不执行关单或 Done 流转
- 不引入 M23 updateable component lifecycle
- OutOfScopePaths:
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`
  - `src/Engine.Physics/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 若本地环境无法执行部分 runtime smoke，QA 必须记录环境限制和替代证据。
- 处理规则：
  - 任何影响 gate 结论的问题必须回退或转卡，不得自行脑补为 pass。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确 build/test、依赖检查、API shape、主路径回归和边界合同同步口径。
  - QA 不需要回看计划全文即可判定 M22 是否可进入 Review。
  - MustFix 和非范围处理已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - QA 横跨 Runtime.Abstractions、Scene、Scripting、App、Render、SceneData 多个边界。
  - M22 是 foundation milestone，边界错误比可见功能缺失更危险。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Runtime.Abstractions -> Engine.Contracts`
  - `Engine.Scene -> Engine.Runtime.Abstractions`
  - `Engine.Scripting -> Engine.Runtime.Abstractions`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Scripting`
  - `Engine.App -> Engine.Runtime.Abstractions`
- ForbiddenDependsOn:
  - `Engine.Runtime.Abstractions -> Engine.Scene`
  - `Engine.Scripting -> Engine.Scene`
  - `Engine.Scene -> Engine.Scripting`
  - `Engine.Scene -> Engine.Physics`
  - `Engine.Render -> Engine.Runtime.Abstractions`
  - `Engine.SceneData -> Engine.Runtime.Abstractions`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-runtime-abstractions.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过
- Smoke: Scene load/render frame、script self Transform update、physics writeback before render、Render/SceneData no Runtime.Abstractions dependency 均通过
- Perf: 无明显 runtime path 退化；Runtime.Abstractions 仅为 interface layer，无逐帧额外 IO 或反射扫描
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
100

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-023.md`
- ClosedAt: `2026-05-11`
- Summary:
  - M22 gate review passed with no MustFix findings.
  - Runtime.Abstractions depends only on Contracts and exposes only the planned narrow API shape.
  - Scene, Scripting, App, Render, and SceneData dependency directions match the M22 dependency contract.
  - Scene render, script self-transform, physics writeback before render, and Render/SceneData no-runtime-abstractions smoke coverage are present.
- FilesChanged:
  - `.ai-workflow/boundaries/engine-runtime-abstractions.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-qa-023.md`
  - `.ai-workflow/archive/2026-05/TASK-QA-023.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL warning on App/Editor.App only)
  - Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed)
  - Dependency: pass (`dotnet list ... reference`; Runtime.Abstractions only references Contracts; Scene/Scripting/App allowed references present; Render/SceneData do not reference Runtime.Abstractions)
  - API Shape: pass (`rg` review found no update/traversal/add/remove/cross-object API in Runtime.Abstractions beyond planned `SceneTransform` contract type)
  - Smoke: pass (Scene render frame, script self Transform update, physics writeback before render, and Render/SceneData dependency guards covered by existing tests)
  - CodeQuality: pass (`NoNewHighRisk=true`, `MustFixCount=0`)
  - DesignQuality: pass (`DQ-1 SRP`, `DQ-2 DIP`, `DQ-3 OCP-oriented`, `DQ-4 closure readiness`)
- ModuleAttributionCheck: `pass`
