# 任务: TASK-QA-010 M9 真实 mesh 资产链路门禁复验

## TaskId
`TASK-QA-010`

## 目标（Goal）
完成 M9 Build/Test/Smoke/Perf 与设计边界复验，确认真实磁盘 mesh 资产主链路可运行，且 `Asset 管 CPU 资产 / Render 管 GPU 资产 / Scene 只持引用 / App 只做装配` 的职责边界成立。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M9-2026-04-22`

## 里程碑引用（兼容别名：MilestoneRef）
`M9.4`

## 执行代理（ExecutionAgent）
Exec-QA

## 优先级（Priority）
P2

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Asset
- Engine.Contracts
- Engine.Render
- Engine.Scene

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M9-G4`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-ASSET-001`
  - `TASK-REND-013`
  - `TASK-SCENE-008`
  - `TASK-APP-008`

## 范围（Scope）
- AllowedModules:
  - tests
  - workflow evidence
- AllowedFiles:
  - 全链路门禁证据
  - 共享 mesh 缓存与 fallback 专项回归
- AllowedPaths:
  - `src/**`
  - `tests/**`

## 编码与文件组织约定（Skill Guard）
- 若执行中需要为验证补充源码/测试代码，命名约定为：`private`/`protected` 字段使用 `camelCase`（禁止前导下划线），参数/局部变量使用 `camelCase`，公共类型/属性/方法使用 `PascalCase`。
- 若执行中新增类型或接口，默认一个类一个文件、一个接口一个文件；仅小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外，且需在说明中标注。

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不新增实现功能
- 不重排 M9 优先级
- 不以临时 mock 替代真实磁盘 mesh 主路径

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - 已完成 M9 主链路任务卡输出
- ForbiddenDependsOn:
  - 未验证共享 mesh CPU/GPU cache 命中即直接关单
  - 未验证边界职责即宣称完成

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
  - `.ai-workflow/boundaries/engine-asset.md`
  - `.ai-workflow/boundaries/engine-contracts.md`
  - `.ai-workflow/boundaries/engine-render.md`
  - `.ai-workflow/boundaries/engine-scene.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 全量通过；导入、缓存、fallback、共享 mesh 复用与装配回归通过
- Smoke: 至少一个真实磁盘 mesh 在真实窗口或 headless 链路被验证可运行并稳定退出
- Perf: 稳定帧循环阶段无重复磁盘读取；同 mesh 多实例无重复 GPU 资源创建
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
- Gate evidence summary
- Regression checklist
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
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-QA-010.md`
- ClosedAt: `2026-04-25 18:34`
- Summary:
  - 复核 M9 真实磁盘 mesh 主链路的 Build/Test/Smoke/Perf 与边界职责，确认 `Asset 管 CPU 资产 / Render 管 GPU 资产 / Scene 只持引用 / App 只做装配` 成立。
  - 汇总 Asset、Render、Scene、App 相关测试与样例运行证据，确认共享 mesh cache、fallback 与装配链路均已覆盖。
  - Human 于 `2026-04-25` 完成 M9 全量人工验收并批准关单。
- FilesChanged:
  - `.ai-workflow/tasks/task-qa-010.md`
  - `.ai-workflow/archive/2026-04/TASK-QA-010.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（Human 于 `2026-04-25` 确认 M9 全链路验收通过；当前代码基线已包含 Contracts/Asset/Render/Scene/App 全链路实现）
  - Test: `pass`（`tests/Engine.Asset.Tests`、`tests/Engine.Render.Tests`、`tests/Engine.Scene.Tests`、`tests/Engine.App.Tests` 已覆盖导入、cache、fallback、共享 mesh 与装配路径）
  - Smoke: `pass`（Human 于 `2026-04-25` 确认至少一个真实磁盘 mesh 已在链路中运行并稳定退出）
  - Perf: `pass`（Asset provider 缓存与 Render GPU cache 语义已具备专项测试覆盖）
  - CodeQuality: `pass`（未发现新增 MustFix）
  - DesignQuality: `pass`（SRP/DIP/OCP-oriented 口径与当前实现一致）
- ModuleAttributionCheck: pass
