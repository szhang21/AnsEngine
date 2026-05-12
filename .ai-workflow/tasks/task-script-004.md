# 任务: TASK-SCRIPT-004 M22 Scripting runtime abstraction alignment

## TaskId
`TASK-SCRIPT-004`

## 目标（Goal）
让 `Engine.Scripting` 消费 `Engine.Runtime.Abstractions`，将 `IScriptSelfObject` 对齐到 `IRuntimeObject` 并让脚本 Transform 访问对齐 `IRuntimeTransformComponent`，同时保持 `context.Self.Transform`、脚本 update 生命周期和禁止跨对象访问语义不变。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M22-2026-05-11`

## 里程碑引用（兼容别名：MilestoneRef）
`M22.5`

## 执行代理（ExecutionAgent）
Exec-Scripting

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.Scripting

## 次级模块（SecondaryModules）
- Engine.Runtime.Abstractions
- Engine.App
- Engine.Scene

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scripting.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M22-G2`
- CanRunParallel: `true`
- DependsOn:
  - `TASK-RABS-001`

## 里程碑上下文（MilestoneContext）
- M22.5 让 scripting 面向共享 runtime abstractions，而不是 Scene concrete types，为未来自定义脚本程序集避免引用 `Engine.Scene` 打地基。
- 本卡只对齐 Scripting public abstraction 和 App bridge，不引入外部脚本加载、源码编译、热重载或新生命周期。
- 上游直接影响本卡的背景包括：`IScriptSelfObject` 推荐继承 `IRuntimeObject`，并保留 `IRuntimeTransformComponent Transform { get; }`，`context.Self.Transform` 仍可用。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Engine.Scripting` may reference `Engine.Runtime.Abstractions`。
  - `Engine.Scripting` continues to forbid `Engine.Scene`。
  - Recommended shape: `public interface IScriptSelfObject : IRuntimeObject { IRuntimeTransformComponent Transform { get; } }`。
  - Current script update lifecycle stays in `ScriptRuntime`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许 scripts query or mutate other objects。
  - 不允许引入 custom script loading、source compilation、hot reload。
  - 不允许改变 App update order 或 ScriptRuntime binding/update lifecycle。
- 计划结构约定：
  - 未来 scripts 应引用 `Engine.Scripting`、`Engine.Runtime.Abstractions`、`Engine.Contracts`，不引用 `Engine.Scene`。

## 实施说明（ImplementationNotes）
- 在 `src/Engine.Scripting/Engine.Scripting.csproj` 添加 `Engine.Runtime.Abstractions` 项目引用。
- 修改 `IScriptSelfObject` 继承 `IRuntimeObject`，并让 `Transform` 返回 `IRuntimeTransformComponent` 或兼容该 abstraction。
- 如现有 `IScriptTransformComponent` 与新 abstraction 重叠，优先做兼容适配，保持 `context.Self.Transform` 调用点不破坏。
- 更新 App 侧 bridge 类型（如 `SceneScriptSelfObject` / `SceneScriptTransformComponent`）以包装 Scene handle 到 Runtime.Abstractions 接口，但 App 仍是组合根桥接者。
- 补 Scripting/App 测试：现有 scripts 使用 `context.Self.Transform` 继续工作；script update 只能修改绑定 self object；Scripting boundary test 确认不引用 Scene。
- 更新 `engine-scripting.md`，必要时更新 `engine-app.md` 记录 App bridge 调整。

## 设计约束（DesignConstraints）
- 不允许 `Engine.Scripting` 项目引用 `Engine.Scene`。
- 不允许在 `Engine.Scripting` 中暴露 FindObject、Scene、Parent/Children 或全局 object query。
- 不允许引入 component update lifecycle 或 script loading。
- 新增 C# 代码遵守 Engine 编码规范和一类一文件约定。

## 失败与降级策略（FallbackBehavior）
- 如果对齐 abstraction 破坏 `context.Self.Transform` ergonomics，必须通过 adapter 保持兼容，不得要求现有脚本改写。
- 如果 App bridge 需要知道 Scene concrete handle，这是允许的，但不得让 Scripting 反向引用 Scene。
- 如果 ScriptRuntime lifecycle 或 App update order 发生变化，必须回退。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.Scripting/IScriptSelfObject.cs`
  - `src/Engine.Scripting/IScriptTransformComponent.cs`
  - `src/Engine.Scripting/ScriptContext.cs`
  - `src/Engine.Scripting/ScriptRuntime.cs`
  - `src/Engine.App/**`
  - `src/Engine.Runtime.Abstractions/**`
- 相关测试入口：
  - `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
  - `tests/Engine.Scripting.Tests/ScriptRegistryTests.cs`
  - `tests/Engine.App.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-script-001.md`
  - `.ai-workflow/tasks/task-script-002.md`
  - `.ai-workflow/tasks/task-app-012.md`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
- 计划结构引用：
  - `PLAN-M22-2026-05-11 > Engine.Scripting Alignment`
  - `PLAN-M22-2026-05-11 > Milestones > M22.5`
  - `PLAN-M22-2026-05-11 > Runtime Data Flow`

## 范围（Scope）
- AllowedModules:
  - Engine.Scripting
- AllowedFiles:
  - Scripting abstraction interfaces、ScriptRuntime compatibility tests、App bridge adapter if required
- AllowedPaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现 custom script loading / source compilation / hot reload
- 不新增 script access to other objects
- 不修改 Scene concrete runtime object storage
- 不改变 App update order
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `src/Engine.Render/**`
  - `src/Engine.Physics/**`
  - `src/Engine.SceneData/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - 是否保留 `IScriptTransformComponent` 作为 compatibility alias/adapter 由 Execution 基于现有调用点决定。
- 处理规则：
  - 若兼容策略会破坏现有 scripts 或扩大 Scripting public API，必须回退修卡。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 本卡已明确接口推荐形状、兼容要求、App bridge 边界和禁止访问面。
  - 测试入口覆盖 Scripting 与 App 现有主路径。
  - 执行者无需回看计划全文即可避免 Scene dependency 回流。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - Scripting public abstraction 变更必须保持脚本 ergonomics 与 App bridge lifecycle 不变。
  - 依赖方向错误会直接破坏 M22 目标。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.Scripting -> Engine.Runtime.Abstractions`
  - `Engine.Scripting -> Engine.Contracts`
  - `Engine.App -> Engine.Scripting`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.Runtime.Abstractions`
- ForbiddenDependsOn:
  - `Engine.Scripting -> Engine.Scene`
  - `Engine.Scripting -> Engine.App`
  - `Engine.Scripting -> Engine.Render`
  - `Engine.Scripting -> Engine.Physics`
  - `Engine.Scripting -> Engine.SceneData`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test AnsEngine.sln --no-restore --nologo -v minimal` 通过，Scripting/App bridge regression tests 通过
- Smoke: existing scripts using `context.Self.Transform` continue to work；scripts cannot cross-query or mutate other objects；Scripting 不引用 Scene
- Perf: ScriptRuntime binding/update lifecycle and App update order unchanged；无明显逐帧开销退化说明

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SCRIPT-004.md`
- ClosedAt: `2026-05-11`
- Summary:
  - `Engine.Scripting` now references `Engine.Runtime.Abstractions`.
  - `IScriptSelfObject` aligns to `IRuntimeObject`, and `Self.Transform` returns `IRuntimeTransformComponent`.
  - `IScriptTransformComponent` remains as a compatibility interface over `IRuntimeTransformComponent`.
  - App's scripting bridge wraps Scene handles as runtime self objects without changing script lifecycle or update order.
- FilesChanged:
  - `src/Engine.Scripting/Engine.Scripting.csproj`
  - `src/Engine.Scripting/IScriptSelfObject.cs`
  - `src/Engine.Scripting/IScriptTransformComponent.cs`
  - `tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj`
  - `tests/Engine.Scripting.Tests/ScriptRuntimeTests.cs`
  - `src/Engine.App/Engine.App.csproj`
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `.ai-workflow/boundaries/engine-scripting.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-script-004.md`
  - `.ai-workflow/archive/2026-05/TASK-SCRIPT-004.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: pass (`dotnet build AnsEngine.sln --nologo -v minimal`; existing `net7.0` EOL and Windows Kits `LIB` path warnings only)
  - Test: pass (`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`; all visible solution test projects passed, including `Engine.Scripting.Tests` 18/18 and `Engine.App.Tests` 27/27)
  - Focused Test: pass (`dotnet test tests/Engine.Scripting.Tests/Engine.Scripting.Tests.csproj --no-restore --nologo -v minimal`; 18/18)
  - App Bridge Test: pass (`dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`; 27/27)
  - Smoke: pass (`context.Self.Transform` scripts continue to work; ScriptRuntime binding/update lifecycle and App update order unchanged)
  - Boundary: pass (`Engine.Scripting` still does not reference `Engine.Scene`; App owns the Scene/Scripting bridge)
  - Perf: pass (adapter adds no per-frame search or script loading work)
- ModuleAttributionCheck: `pass`
