# 任务: TASK-QA-025 M24 Runtime component lifecycle gate review and archive

## 目标（Goal）
对 M24 全链路进行 QA gate review，确认 runtime component lifecycle refactor 的 build/test/smoke/perf、边界、归档三件套和 Human 复验材料完整。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE`

## 里程碑引用（兼容别名：MilestoneRef）
`M24.QA`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Runtime.Abstractions
- Engine.Scene
- Engine.Scripting
- Engine.Runtime
- tests

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M24-G6`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-RABS-002`
  - `TASK-SCENE-024`
  - `TASK-SCRIPT-005`
  - `TASK-RUNTIME-001`
  - `TASK-APP-023`

## 里程碑上下文（MilestoneContext）
- M24 改动真实 runtime 主路径，完成标准不能只看单元测试，必须证明 App-level headless smoke 中 script -> physics -> scene state -> render 前状态不退化。
- 本卡只做 QA 复验、门禁证据汇总、归档准备和设计质量质疑，不实现功能。
- QA 不拥有关单权，`Review -> Done` 仍需 Human 显式签收后由 Workflow Steward 机械同步。

## 决策继承（DecisionCarryOver）
- 继承决策：
  - M24 required real path 必须成立：SceneData loads -> Engine.Runtime initializes -> App input/time -> Runtime.Tick -> update components -> physics writeback -> App render post-tick Scene state。
  - M24 no-goals 不得滑入：external scripting、hot reload、Editor Script UI、cross-object query、signal/event、animation、audio、fixed update。
  - QA 必须区分自动测试与真实主路径 smoke。
- 不得推翻的既定取舍：
  - QA 不修业务代码，不改任务目标语义，不代 Human 关单。
  - MustFixCount > 0 时不得进入 Review，必须转卡或打回原实施卡。
- 上游已定结构约束：
  - CodeQuality 至少包含 `NoNewHighRisk=true`、`MustFixCount=0` 或转卡说明。
  - DesignQuality 至少覆盖 DQ-1 SRP、DQ-2 DIP、DQ-3 OCP-oriented。

## 实施说明（ImplementationNotes）
- 读取全部 M24 implementation cards 和归档准备状态，核对任务依赖链与状态一致性。
- 执行 full gate：`dotnet build AnsEngine.sln --nologo -v minimal` 与 `dotnet test AnsEngine.sln --no-restore --nologo -v minimal`。
- 做边界检查：Runtime.Abstractions 无 concrete 依赖；Scene 不依赖 Scripting/Runtime/Physics/App；Scripting 不依赖 Scene/App；Runtime 依赖方向符合计划；App 不再直接拥有 per-frame script/physics orchestration。
- 做真实主路径 smoke：确认 App-level/headless path 证明 runtime tick 后 scene state 更新可被 render 前观察。
- 汇总 must-fix、风险、性能说明和归档证据，准备 `.ai-workflow/archive/2026-05/TASK-QA-025.md` 与 archive-index 条目，但不自行 Done。

## 设计约束（DesignConstraints）
- 不允许 QA 修改业务源码或补实现。
- 不允许因测试通过而跳过 real path smoke。
- 不允许忽略边界文档与实际 csproj dependency 的偏差。
- 不允许把既有 `.NET net7.0` residual warning 作为 M24 blocker，除非本轮引入了新的基线冲突。

## 失败与降级策略（FallbackBehavior）
- Build/Test/Smoke/Perf 任一失败，任务回退到 `InProgress` 并列明失败证据。
- MustFixCount > 0 时输出转卡建议或打回对应任务，不得进入 `Review`。
- 若归档三件套缺失，标记 `ArchiveIncomplete`，Owner 指向对应 Execution/Steward 流程，不得声称 M24 closed。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Runtime.Abstractions/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Scripting/**`
  - `src/Engine.Runtime/**`
  - `src/Engine.App/**`
- 相关测试入口：
  - `tests/Engine.Runtime.Abstractions.Tests/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.Scripting.Tests/**`
  - `tests/Engine.Runtime.Tests/**`
  - `tests/Engine.App.Tests/**`
- 相关文档/任务：
  - `.ai-workflow/plan-archive/2026-05/PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE.md` 的 `RuntimeRealityCheck`、`TestPlan`、`M24.5 App Host Contraction And QA Close`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/boundaries/`
- 示例结构定位：
  - M24 `RuntimeRealityCheck` 的路径是 smoke 验收主线，不能被 unit tests 替代。

## 范围（Scope）
- AllowedModules:
  - Engine.App
  - Engine.Runtime.Abstractions
  - Engine.Scene
  - Engine.Scripting
  - Engine.Runtime
  - tests
- AllowedFiles:
  - QA 只读验证证据、任务卡/归档证据补充说明
- AllowedPaths:
  - `src/Engine.Runtime.Abstractions/**`
  - `tests/Engine.Runtime.Abstractions.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.Runtime/**`
  - `tests/Engine.Runtime.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- Feature implementation
- Business source edits
- Test rewrites to mask failure
- Human final Done signoff
- Boundary contract semantic rewrites beyond QA evidence notes
- OutOfScopePaths:
  - `src/**`
  - `tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - none
- 处理规则：
  - 若 QA 发现 implementation card insufficient or violated, produce QAReport and return to Dispatch/Execution; do not patch implementation.

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 所有 M24 upstream cards and gate criteria are explicitly listed.
  - QA-specific CodeQuality/DesignQuality requirements are embedded.
  - QA can execute without making design decisions.
- MissingInfo:
  - none

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `.ai-workflow/**`
  - read-only `src/**`
  - read-only `tests/**`
- ForbiddenDependsOn:
  - Business source mutation
  - Non-QA implementation ownership

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
  - `.ai-workflow/boundaries/engine-runtime.md`
  - `.ai-workflow/boundaries/engine-runtime-abstractions.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过，记录 warnings 摘要。
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，记录失败则打回。
- Smoke: App-level/headless runtime path 证明 post-runtime-tick Scene state 在 render 前已更新；若无可执行 smoke，必须标为 failure 并转卡。
- Perf: 记录无明显退化说明；若新增 runtime tick 有显著额外 rebuild/allocation 风险，MustFix 或 follow-up。
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
- CodeQuality and DesignQuality conclusion
- Boundary/dependency review notes
- Archive snapshot and index preparation
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：QA 不新增业务源码；如需元数据归档，按 workflow UTF-8 编码规则写入

## 复杂度评估（ComplexityAssessment）
- Level: `L3`
- Why:
  - QA 覆盖跨模块 runtime 主路径与多项边界。
  - Must-fix 分流和归档一致性要求高。
  - 真实 smoke 不能被单测替代。
- SufficiencyMatch: `pass`

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-QA-025.md`
- ClosedAt: `2026-05-27`
- Summary: Completed M24 full-chain QA gate review for runtime component lifecycle; build/test/smoke/perf, dependency boundaries, implementation-card archive readiness, and Human signoff materials are prepared with MustFixCount=0.
- FilesChanged:
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/boundaries/engine-runtime.md`
  - `.ai-workflow/boundaries/engine-runtime-abstractions.md`
  - `.ai-workflow/boundaries/engine-scene.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/tasks/task-qa-025.md`
  - `.ai-workflow/archive/2026-05/TASK-QA-025.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed; 0 errors, existing `net7.0` EOL warnings.
  - Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` passed across solution visible test assemblies.
  - Smoke: App focused real `EngineRuntimeSession` headless `MoveOnInput` scene proves post-runtime-tick Scene state is observable by render before `RenderFrame` completes.
  - Boundary: dependency scans confirm Runtime.Abstractions no concrete dependencies, Scene no App/Scripting/Physics, Scripting no Scene/App, Runtime no App/Platform/Render/Editor/Asset, App no direct Scripting/Physics orchestration.
  - Perf: no per-frame PhysicsWorld rebuild, source loading, hot reload, external compilation, or App-side duplicate scheduler was introduced.
- ModuleAttributionCheck: pass
