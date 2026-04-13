# 任务: TASK-QA-004 M4 解耦门禁与质量复验

## 目标（Goal）
补齐并执行“Render 不得直接引用 Scene”的门禁校验，完成 M4 解耦改造后的质量与验收证据收敛。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M4B-2026-04-13`

## 里程碑引用（兼容别名：MilestoneRef）
`M4-SceneRenderPipeline`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P0
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Render
- Engine.Scene
- Engine.Platform

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G5`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-CONTRACT-001`
  - `TASK-SCENE-003`
  - `TASK-REND-006`
  - `TASK-APP-004`

## 范围（Scope）
- AllowedModules:
  - Engine.App
- AllowedFiles:
  - QA 证据与门禁校验相关文件
  - 必要测试文件
- AllowedPaths:
  - `src/Engine.App/**`
  - `tests/**`
> 说明：`AllowedPaths` 仅用于源码/测试改动范围，不包含边界文档路径。

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不新增业务功能。
- 不修复 Render/Scene 功能目标（本卡只做验证与证据收敛）。
- 不替代 Human 最终关单裁决。
- OutOfScopePaths:
  - `src/Engine.Render/**`
  - `src/Engine.Scene/**`
  - `src/Engine.Asset/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.App -> Engine.Render`
  - `Engine.App -> Engine.Scene`
- ForbiddenDependsOn:
  - 为过门禁扩大功能范围
  - 隐式修改已验收语义

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `false`
- BoundaryDocsToUpdate:
  - `[]`
- ChangeLogRequired: `true`
> 说明：`BoundaryDocsToUpdate` 为独立规则，不受 `AllowedPaths` 限制。
> 触发条件：仅当 `NewFilesExpected=true` 或执行中实际新增源码/测试文件时，才强制执行边界文档更新。

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过；新契约调整最小链路测试通过
- Smoke: 场景驱动渲染稳定运行 30 秒以上，退出码 `0`
- Perf: 相比 M4 既有实现无明显退化
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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-004.md`
- ClosedAt: `2026-04-14 01:28`
- Summary:
  - 完成 M4 解耦门禁复验，确认 `Engine.Render` 不再直接编译依赖 `Engine.Scene`。
  - 完成 Build/Test/Smoke/Perf 全量复核并回填证据。
  - 输出代码质量与设计质量结论，当前无新增高风险阻塞项。
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-004.md`
  - `.ai-workflow/board.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-004.md`
- ValidationEvidence:
  - Build(Debug): pass（`dotnet build AnsEngine.sln -c Debug -m:1`，0 警告 0 错误）
  - Build(Release): pass（`dotnet build AnsEngine.sln -c Release -m:1`，0 警告 0 错误）
  - Test: pass（`dotnet test AnsEngine.sln -m:1`，全部测试通过）
  - DependencyGate: pass（`RenderSceneRef=absent`，`RenderContractsRef=present`）
  - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.87s`）
  - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，`ExitCode=0`，`45.83s`）
  - CodeQuality: pass（NoNewHighRisk=true，MustFixCount=0，MustFixDisposition=none）
  - DesignQuality: DQ-1 pass / DQ-2 pass / DQ-3 pass / DQ-4 pass
- ModuleAttributionCheck: pass
