# 任务: TASK-APP-023 M24 App host contraction to Runtime session

## 目标（Goal）
让 `Engine.App` 从 script update / physics writeback 编排中收缩为 platform/app host，通过 `EngineRuntimeSession` 驱动 runtime tick 后再 render。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE`

## 里程碑引用（兼容别名：MilestoneRef）
`M24.5`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- tests

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M24-G5`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-RUNTIME-001`

## 里程碑上下文（MilestoneContext）
- M20/M23 后 App 已直接拥有 script update 和 physics writeback 顺序，M24 要把这部分 runtime behavior 交给 `Engine.Runtime`。
- 本卡在 runtime session 已可用后收缩 App：保留窗口、输入、时间、资源/bootstrap、render/present、退出和 shutdown。
- 本卡不再设计 runtime pipeline，只做 App host 到 Runtime session 的接线与旧 orchestration 清理。

## 决策继承（DecisionCarryOver）
- 继承决策：
  - App loop 顺序保持：process events -> input/time -> runtime tick -> render -> present。
  - App 将 `Engine.Platform.InputSnapshot` 转为 `RuntimeInputSnapshot`，不再转为 scripting input。
  - App 不再直接调用 `ScriptRuntime.Update(...)` 或 `RuntimePhysicsOrchestrator.ResolveAndWriteBack(...)`。
- 不得推翻的既定取舍：
  - App 仍是 composition root 和 platform host。
  - Render initialize/shutdown、scene file load、asset warmup、window event processing、present、exit code 仍归 App。
  - 不把 render ownership 移到 `Engine.Runtime`。
- 上游已定结构约束：
  - App 应依赖 `Engine.Runtime`。
  - Built-in script placement 应从 `ApplicationBootstrap.cs` 收敛到 Runtime/Scripting 适当入口，App 不再成为 script catalog owner。

## 实施说明（ImplementationNotes）
- 定位 `ApplicationBootstrap.cs`、`ApplicationHost.Run()` 或当前等价主循环，替换直接 script/physics 编排为 `EngineRuntimeSession.Initialize(...)` 和 `Tick(...)` 调用。
- 新增 App 层 input adapter：Platform input -> `RuntimeInputSnapshot`，不直接生成 `ScriptInputSnapshot`。
- 移除或降级 App 中旧 script update / physics writeback helpers 的主路径使用；如果文件已由 `TASK-RUNTIME-001` 迁出，可删除 App 侧残留或保留非主路径兼容 facade。
- 更新 `Engine.App.csproj` 依赖 `Engine.Runtime`，并检查不再需要直接依赖 `Engine.Physics` / `Engine.Scripting` 的情况；若仍因 bootstrap 类型需要保留，必须在自检中说明原因。
- 补 App tests：App calls runtime tick、无直接 script update/physics orchestrator 主路径、headless valid script scene updates before render、failure shutdown stable。
- 更新 `.ai-workflow/boundaries/engine-app.md`，记录 App host contraction。

## 设计约束（DesignConstraints）
- 不允许 App 继续拥有 runtime tick ordering。
- 不允许 App 直接调用 Scripting per-frame update 或 Physics writeback orchestration。
- 不允许把 renderer/window/present 迁入 Engine.Runtime。
- 不允许扩大到 Editor Play Mode、Resource Browser、Prefab、Undo/Redo。
- 不允许通过 static global session/service locator 绕过 composition root。

## 失败与降级策略（FallbackBehavior）
- Runtime tick failure 必须转为 App 可控 shutdown/exit diagnostic，不得继续 render 陈旧 state。
- Scene file load 或 asset warmup failure 仍保持 App 现有失败语义，不混入 Runtime tick failure。
- 若 App 必须保留某个 direct dependency 过渡，必须记录原因和后续清理建议；不得保留直接 per-frame orchestration。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/Program.cs`
  - `src/Engine.App/ApplicationContracts.cs`
  - `src/Engine.App/Engine.App.csproj`
- 相关测试入口：
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `tests/Engine.App.Tests/Engine.App.Tests.csproj`
- 相关文档/计划：
  - `.ai-workflow/plan-archive/2026-05/PLAN-M24-RUNTIME-COMPONENT-LIFECYCLE.md` 的 `App Host`、`RuntimeRealityCheck`、`M24.5 App Host Contraction And QA Close`
  - `.ai-workflow/boundaries/engine-app.md`
  - `TASK-APP-020`、`TASK-APP-021`、`TASK-APP-022`
- 示例结构定位：
  - M24 `App Host` 小节中的保留职责清单是边界约束；App loop order 是 smoke 验收约束。

## 范围（Scope）
- AllowedModules:
  - Engine.App
  - tests
- AllowedFiles:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `AnsEngine.sln`
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`
  - `AnsEngine.sln`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- Engine.Runtime pipeline redesign
- Scripting behavior internals
- Physics solver changes
- Render ownership migration
- Platform input polling implementation changes beyond adapter consumption
- Editor UI / Play Mode
- OutOfScopePaths:
  - `src/Engine.Runtime/**`
  - `src/Engine.Scripting/**`
  - `src/Engine.Physics/**`
  - `src/Engine.Render/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - App 是否可完全移除 direct `Engine.Physics` / `Engine.Scripting` project references 取决于 `TASK-RUNTIME-001` 的 migration result。
- 处理规则：
  - 若 removal 不安全，保留最小依赖并记录原因；不得保留 direct per-frame orchestration。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - Runtime session 已由前置卡提供，App contraction 目标和保留职责已明确。
  - 本卡已定义 App loop、输入转换、禁止调用和失败语义。
  - 执行者可只按本卡进行 App 层接线和测试。
- MissingInfo:
  - none

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Contracts`
  - `Engine.Core`
  - `Engine.Platform`
  - `Engine.Render`
  - `Engine.Runtime`
  - `Engine.SceneData`
  - `Engine.Asset`
  - `Engine.Scene`
- ForbiddenDependsOn:
  - Direct per-frame orchestration dependency on `Engine.Scripting`
  - Direct per-frame orchestration dependency on `Engine.Physics`

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
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过。
- Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 runtime tick delegation、no direct script/physics orchestration、failure shutdown。
- Smoke: headless valid script scene proves post-runtime-tick Scene state is rendered/observable after script movement or rotation。
- Perf: App contraction 不增加明显 frame loop overhead；记录无明显退化说明。
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
- App runtime session delegation
- Input adapter to RuntimeInputSnapshot
- App tests proving contraction
- Boundary change log
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 复杂度评估（ComplexityAssessment）
- Level: `L3`
- Why:
  - 容易把 runtime spaghetti 留在 App 或迁移过度。
  - 需要保护 App host 职责和 Runtime scheduler 职责边界。
  - 需要 App-level smoke 验证真实主路径。
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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-APP-023.md`
- ClosedAt: `2026-05-27`
- Summary: Contracted `Engine.App` to a host/composition root by delegating runtime initialization and ticks to `EngineRuntimeSession`, adapting Platform input to runtime input, and removing App-owned script update / physics writeback orchestration.
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/ApplicationContracts.cs`
  - `src/Engine.App/Engine.App.csproj`
  - `src/Engine.App/SceneRuntimeContracts.cs`
  - `src/Engine.App/ScenePhysicsWorldDefinitionBridge.cs`
  - `src/Engine.App/RuntimePhysicsOrchestrator.cs`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `tests/Engine.App.Tests/Engine.App.Tests.csproj`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-023.md`
  - `.ai-workflow/archive/2026-05/TASK-APP-023.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --nologo -v minimal` passed with existing `net7.0` EOL and Windows Kits `LIB` warnings; 0 errors.
  - Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` passed; 15 passed, 0 failed.
  - Smoke: Real `EngineRuntimeSession` headless MoveOnInput scene test proved post-runtime-tick Scene state is rendered/observable before render.
  - Perf: App now performs one Runtime session tick per frame and removed App-side script/physics traversal; no per-frame world rebuild or extra App orchestration introduced.
- ModuleAttributionCheck: pass
