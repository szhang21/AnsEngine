# 任务: TASK-APP-021 M20 Runtime physics order and writeback integration

## TaskId
`TASK-APP-021`

## 目标（Goal）
在 `Engine.App` 主循环中接入 `Script -> Physics -> Scene writeback -> Render` 顺序，让 Physics 成为本帧最终位置约束者，并使 Render 看到 resolved transform。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M20-2026-05-04`

## 里程碑引用（兼容别名：MilestoneRef）
`M20.4`

## 执行代理（ExecutionAgent）
Exec-App

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Scene
- Engine.Scripting
- Engine.Physics

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M20-G2`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCENE-020`
  - `TASK-APP-020`
  - `TASK-PHYS-003`

## 里程碑上下文（MilestoneContext）
- M20.4 是 Physics Runtime Collision MVP 真正让用户可见的运行时接线卡；没有这张卡，前面的 writeback、bridge 和 resolve 仍只是孤立能力。
- 本卡承担的是主循环顺序、runtime bridge、Scene candidate transform 采集、Physics resolve 调用和 writeback 调度，不承担 Physics 核心算法或 Scene writeback API 自身设计。
- 直接影响本卡实现的上游背景包括：本帧顺序固定为 `SceneRuntime.Update -> ScriptRuntime.Update -> Physics resolve/writeback -> Render`；Physics 是最终位置约束者；没有 physics collider 的场景必须保持既有 M18/M19 行为。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - 主循环固定顺序：
    - `ProcessEvents`
    - `Input`
    - `Time`
    - `SceneRuntime.Update`
    - `ScriptRuntime.Update`
    - `PhysicsRuntime.ResolveAndWriteBack`
    - `RenderFrame`
    - `Present`
  - 运行时数据流：
    - scripts first update Scene transforms
    - App reads candidate kinematic transforms from Scene runtime
    - App calls Physics resolve/query
    - App writes resolved transforms back to Scene
    - Render observes Scene after writeback
  - scene without physics components preserves existing movement behavior
  - bridge failure / writeback failure 必须是 deterministic run failure 或可测试诊断
- 本卡执行时不得推翻的既定取舍：
  - 不允许把 Physics resolve 放到 Script update 前或 Render 后。
  - 不允许让 Physics 直接写 Scene，App 必须仍是唯一 composition/runtime bridge。
  - 不允许顺手实现 dynamic gravity、solver、Editor UI 或 Play Mode。
  - 不允许让没有 physics 组件的 scene 退化为被错误阻挡或 movement 丢失。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M20-2026-05-04 > Runtime Data Flow` 和 `Milestones > M20.4` 已定稿 App 的采集/resolve/writeback 顺序，执行时不得自创别的 ownership flow。
  - `PLAN-M20-2026-05-04 > Failure And Fallback` 已定稿 bridge/writeback failure 必须可见，M20 不支持 runtime 对象动态创建/销毁。

## 实施说明（ImplementationNotes）
- 先在 App 内增加小型 runtime physics adapter/orchestrator，负责：
  - 从 Scene runtime 读取候选 mover transform
  - 调用 Physics resolve
  - 把 resolved transform 写回 Scene
- 再把该 adapter 接进 `ApplicationHost.Run()` 主循环，严格放在 script update 后、render 前。
- 然后补无-physics-path 保持行为的守护逻辑，确保没有 physics components 的对象仍按既有 movement/render 行为前进。
- 最后补 App tests 和 headless smoke，至少覆盖：
  - update order is Script -> Physics -> Render
  - `MoveOnInput` cannot move through static collider
  - scene without physics preserves old behavior
  - bridge/writeback failure visible

## 设计约束（DesignConstraints）
- 不允许把 runtime physics 采集/写回逻辑散落回 `ApplicationBootstrap.cs` 主循环巨石中，应收敛到小型 internal App helper。
- 不允许 App 直接依赖 Scene runtime object internal collection；只能通过显式 Scene API / snapshot / writeback contract。
- 不允许修改 Physics core 以适配 App runtime shortcuts。
- 不允许顺手引入 object creation/destruction support 或 body ownership 漂移。

## 失败与降级策略（FallbackBehavior）
- 若运行时 adapter 的具体命名需按 App 风格微调，允许等价命名，但职责必须仍是“采集 candidate -> resolve -> writeback”的单一 orchestrator。
- 若某些 physics-free objects 不易区分，可优先用“只有 bridge world 中存在的 body 才 resolve/writeback，其余对象保持现状”的 conservative 路线，并在测试中钉死。
- 若实现中发现必须让 `Engine.Scene -> Engine.Physics` 或 `Engine.Physics -> Engine.Scene` 才能继续，必须停工回退。
- 若 writeback failure 被吞掉才能让 run 继续，不得接受；必须暴露 deterministic failure。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.App/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Physics/**`
  - `src/Engine.App/SampleScenes/default.scene.json`
- 相关测试入口：
  - `tests/Engine.App.Tests/**`
  - `tests/Engine.Scene.Tests/**`
  - `tests/Engine.Physics.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-scene-020.md`
  - `.ai-workflow/tasks/task-app-020.md`
  - `.ai-workflow/tasks/task-phys-003.md`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M20-2026-05-04.md`
- 计划结构引用：
  - `PLAN-M20-2026-05-04 > Milestones > M20.4`
  - `PLAN-M20-2026-05-04 > Runtime Data Flow`
  - `PLAN-M20-2026-05-04 > Failure And Fallback`
  - `PLAN-M20-2026-05-04 > TestPlan`
  - 上述 runtime/failure 引用属于“实现约束”，顺序、所有权和 failure visibility 已定。

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - runtime physics adapter/orchestrator
  - main loop wiring
  - App tests
  - sample/fixture updates limited to App-owned scene path
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/Engine.App.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现 Physics core resolve 算法
- 不实现 Scene writeback API
- 不实现 gravity / solver / Editor UI / Play Mode
- OutOfScopePaths:
  - `src/Engine.Physics/**`
  - `tests/Engine.Physics.Tests/**`
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - runtime adapter 是读取 Scene snapshot 还是经由更窄 API 拉取 candidate transform，可按最小耦合原则选择，但不能直连内部集合。
- 处理规则：
  - 若问题影响主循环顺序、App/Physics/Scene ownership 或 failure visibility，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - 顺序、所有权、physics-free 保持行为和 failure 处理要求都已下沉到卡面。
  - 执行者无需回看计划全文也能明确这是一张 runtime orchestration 卡，不是 Physics 算法卡。
  - 边界审批与停工条件明确。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 M20 真正的主链路汇合点，Scene、Script、Physics、Render 的帧内顺序和 ownership 都在这里收敛。
  - 若写短，非常容易做成顺序错位、physics-free 场景回归或 writeback 吞错。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Core`
  - `Engine.App -> Engine.Platform`
  - `Engine.App -> Engine.Scene`
  - `Engine.App -> Engine.SceneData`
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Asset`
  - `Engine.App -> Engine.Scripting`
  - `Engine.App -> Engine.Physics`
- ForbiddenDependsOn:
  - `Engine.App` 直接依赖 Scene runtime object/component 内部集合
  - `Engine.App -> Engine.Editor`
  - `Engine.App -> Engine.Editor.App`

## 边界变更请求（BoundaryChangeRequest）
- Required: `true`
- Status: `approved`
- RequestReason: `M20.4 需要 App 在主循环中持有 Physics runtime adapter/orchestrator，并调用 Physics resolve + Scene writeback，当前 engine-app 边界合同尚未声明对 Engine.Physics 的允许依赖。`
- ImpactModules:
  - `Engine.App`
  - `Engine.Physics`
  - `Engine.Scene`
- HumanApprovalRef: `Human approved via “拆卡m20” on 2026-05-04`

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-app.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 Script -> Physics -> Render 顺序、collider block、physics-free preserve 和 failure visibility
- Smoke: headless App run 能证明 script movement -> physics resolve -> scene writeback -> render transform
- Perf: 不引入逐帧重新建 world、跨模块内部集合泄露或 render side effect

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Review

## 完成度（Completion）
`95`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-APP-021.md`
- ClosedAt: `2026-05-05 00:34`
- Summary:
  - Added App runtime physics orchestrator for candidate Scene transform -> Physics resolve -> Scene writeback.
  - Wired main loop order as `SceneRuntime.Update -> ScriptRuntime.Update -> Physics resolve/writeback -> Render`.
  - Extended `ISceneRuntime` with explicit runtime snapshot and transform writeback bridge methods.
  - Covered Script -> Physics -> Render order, collider blocking, physics-free movement preservation, and writeback failure visibility.
- FilesChanged:
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/RuntimePhysicsOrchestrator.cs`
  - `src/Engine.App/SceneRuntimeContracts.cs`
  - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
  - `.ai-workflow/boundaries/engine-app.md`
  - `.ai-workflow/tasks/task-app-021.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/2026-05/TASK-APP-021.md`
  - `.ai-workflow/archive/archive-index.md`
- ValidationEvidence:
  - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
  - Test: pass（`dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`，27/27 passed）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false ANS_ENGINE_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.App/Engine.App.csproj --nologo`，exit 0；默认 scene 走 script movement -> physics resolve -> scene writeback -> render）
  - Perf: pass（未引入逐帧重新建 world、跨模块内部集合泄露或 render side effect）
- ModuleAttributionCheck: pass
