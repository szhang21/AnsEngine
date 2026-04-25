# 轻量卡模板（QuickCard）

适用目标：为简单迭代单、局部修复单、明确可复现的小 bug 提供低建卡成本入口。  
定位：`QuickCard` 只负责“快速登记 + 快速分流 + 必要时升级”，不替代正式任务卡。

## 卡类型

- `QuickTask`
  - 用于：简单迭代、命名修正、局部逻辑修补、测试补齐、小型体验优化
- `QuickBug`
  - 用于：已知模块、可复现、预期/实际明确的局部 bug 修复
- `BugInvestigation`
  - 用于：现象明确但根因未明，需要先调查再决定是否修复

## 建议文件路径

- `.ai-workflow/quickcards/<quick-card-id>.md`

## 模板

```md
# 轻量卡: <ID> <短标题>

## QuickCardId
`<QTK-... | QBUG-... | QINV-...>`

## Type
`QuickTask | QuickBug | BugInvestigation`

## Goal
一句话、可验证的目标。

## Reporter
`<name>`

## Owner
`<name | pending>`

## Priority
`P0 | P1 | P2 | P3`

## PrimaryModule
`Engine.Render | Engine.Scene | Engine.Asset | Engine.Platform | Engine.App | Engine.Contracts | tests | other`

## RelatedPlan
`<plan-id | optional>`

## RelatedTaskId
`<TASK-ID | optional>`

## Summary
- 问题或需求的最小描述

## Expected
- 期望结果

## Actual
- 实际结果（`QuickBug` / `BugInvestigation` 必填）

## Repro
- 复现步骤（`QuickBug` 建议必填，`BugInvestigation` 必填）

## ScopeLimit
- 限定只允许改动的模块/文件/行为
- 明确不做的事情

## Acceptance
- Build:
- Test:
- Smoke:

## EscalationRule
- 若触发以下任一条件，必须升级为正式任务卡：
  - 跨模块改动
  - 修改公开契约或依赖方向
  - 需要边界文档更新
  - 预计超过半天
  - 风险达到 `medium/high`
  - 根因不清或出现范围扩张

## Evidence
- 截图/日志/测试名/复现链接（可选）

## Status
`Todo | InProgress | Review | Done | Escalated | Rejected`

## Completion
`0-100`

## Escalation
- EscalatedToTaskId:
- EscalationReason:
```

## 使用约束

- 一张 `QuickCard` 只允许单一结果、单一负责人。
- 默认只允许单主模块；如执行中发现跨模块，必须升卡。
- `QuickBug` 必须具备明确 `Expected/Actual`；缺任一则退化为 `BugInvestigation`。
- `BugInvestigation` 的结果必须是以下之一：
  - `CloseAsNotRepro`
  - `CloseAsByDesign`
  - `EscalateToTaskCard`
  - `ConvertToQuickBug`
- `Status=Todo` 时 `Completion=0`。
- `Status=Done` 时 `Completion=100`。
- `Status=InProgress|Review|Escalated` 时 `Completion=10-99`。
