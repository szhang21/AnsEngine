# 任务卡模板（Dispatch Agent 专用）

请严格使用以下结构。  
说明：任务卡由 `Dispatch Agent` 生成；`Execution Agent` 只消费任务卡执行，不负责拆分。

```md
# 任务: <ID> <短标题>

## 目标（Goal）
一句话、可验证的目标。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`<plan-id or path>`

## 里程碑引用（兼容别名：MilestoneRef）
`M1 | M2 | ...`

## 执行代理（ExecutionAgent）
<agent-name>

## 优先级（Priority）
P0 | P1 | P2 | P3
> 说明：优先级主定义来自 `计划引用`；Dispatch 仅允许同里程碑内微调。

## 主模块归属（PrimaryModule）
Engine.Render | Engine.Scene | Engine.Asset | Engine.Platform | Engine.App | 其他（需说明）

## 次级模块（SecondaryModules）
- （可选）

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/<module>.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `G1 | G2 | ...`
- CanRunParallel: `true | false`
- DependsOn:
  - `<TASK-ID>`
  - `<TASK-ID>`

## 范围（Scope）
- AllowedModules:
- AllowedFiles:
- AllowedPaths:
> 说明：`AllowedPaths` 仅用于源码/测试改动范围，不包含边界文档路径。

## 跨模块标记（CrossModule）
true | false

## 非范围（OutOfScope）
- 
- 
- OutOfScopePaths:

## 依赖约束（DependencyContract）
- AllowedDependsOn:
- ForbiddenDependsOn:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true | false`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/<module>.md`
- ChangeLogRequired: `true`
> 说明：`BoundaryDocsToUpdate` 为独立规则，不受 `AllowedPaths` 限制。
> 触发条件：仅当 `NewFilesExpected=true` 或执行中实际新增源码/测试文件时，才强制执行边界文档更新。

## 验收标准（Acceptance）
- Build:
- Test:
- Smoke:
- Perf:
- CodeQuality: （QA 验证卡必填；非 QA 卡可选）
  - NoNewHighRisk: `true | false`
  - MustFixCount: `<number>`
  - MustFixDisposition: `none | accepted | follow-up-created`
- DesignQuality: （QA 验证卡必填；非 QA 卡可选）
  - DQ-1 职责单一（SRP）: `pass | fail`
  - DQ-2 依赖反转（DIP）: `pass | fail`
  - DQ-3 扩展点保留（OCP-oriented）: `pass | fail`
  - DQ-4 开闭性评估（可选）: `pass | warn | fail`

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## 状态（Status）
Todo | InProgress | Verify | Review | Done

## 完成度（Completion）
`0-100`（整数百分比）

## 缺陷回流字段（Defect Triage）
- FailureType: `AcceptanceDispute | PostAcceptanceBug | Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pending | pass | fail`

## 归档（Archive）
- ArchivePath:
- ClosedAt:
- Summary:
- FilesChanged:
- ValidationEvidence:
- ModuleAttributionCheck: pass | fail
```

## 校验规则

- 缺少 `TaskSource=DispatchAgent` 视为任务卡无效。
- 缺少 `计划引用`（或 `PlanRef`）视为任务卡无效。
- 缺少 `里程碑引用`（或 `MilestoneRef`）视为任务卡无效。
- 缺少 `ExecutionAgent` 视为任务卡无效。
- 缺少 `ParallelPlan` 任一关键字段（`ParallelGroup`、`CanRunParallel`、`DependsOn`）视为任务卡无效。
- 缺少 `BoundarySyncPlan` 视为任务卡无效。
- 缺少 `OutOfScope` 视为任务卡无效。
- 缺少 `Acceptance` 视为任务卡无效。
- 当任务为 QA 验证卡（`TaskId` 含 `TASK-QA-` 或 `ExecutionAgent=Exec-QA`）时，`Acceptance` 缺少 `CodeQuality` 或 `DesignQuality` 视为无效。
- 缺少 `Priority` 视为任务卡无效。
- 缺少 `PrimaryModule` 视为任务卡无效。
- 缺少 `BoundaryContractPath` 视为任务卡无效。
- 缺少 `BaselineRef` 视为任务卡无效。
- 缺少 `AllowedPaths` 视为任务卡无效。
- 缺少 `Completion` 视为任务卡无效。
- 缺陷回流场景缺少 `DetectedAt` 视为任务卡无效。
- 当 `FailureType=AcceptanceDispute` 且走 `ReopenOriginal` 时，缺少 `ReopenReason` 视为无效。
- 当走 `CreateBugCard|CreateVerifyCard` 时，缺少 `OriginTaskId` 视为无效。
- 缺少 `HumanSignoff` 视为任务卡无效。
- 任务关闭时缺少 `Archive` 字段视为无效。
- 任务涉及跨模块改动但 `CrossModule` 不是 `true` 视为无效。
- 若一张卡包含多个结果，必须拆卡。
- 当 `NewFilesExpected=true` 时，归档若无边界文档更新证据视为无效。
- 代码文件（`src/**`、`tests/**`）必须命中 `AllowedPaths`。
- 边界文档更新必须命中 `BoundaryDocsToUpdate`，不以 `AllowedPaths` 为判定依据。
- 当 `NewFilesExpected=false` 且无新增源码/测试文件时，边界文档更新不作为阻塞条件。
- `Status=Todo` 时 `Completion` 必须为 `0`。
- `Status=Done` 时 `Completion` 必须为 `100`。
- `Status=InProgress|Verify|Review` 时 `Completion` 必须为 `10-99`。
- QA 验证卡若 `CodeQuality.MustFixCount>0` 且 `MustFixDisposition=none`，不得流转到 `Review`。
