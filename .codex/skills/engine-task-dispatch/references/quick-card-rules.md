# 轻量卡规则（QuickCard Rules）

`QuickCard` 是正式任务卡体系前的轻量入口。  
目标：降低简单迭代单与小 bug 的建卡成本，同时避免绕开现有边界、归档和门禁体系。

## 1) 适用范围

仅当以下条件全部满足时，才允许使用 `QuickCard`：

- 单一结果
- 单一负责人
- 单主模块
- 预计在 `1 小时 - 半天` 内完成并验证
- 不新增公开 API / 契约
- 不改变模块依赖方向
- 不需要边界文档更新
- 不需要新增复杂文件结构
- 验证路径简单直接

任一条件不满足，必须改走正式任务卡。

## 2) 三类轻量卡

- `QuickTask`
  - 简单迭代、局部优化、命名修正、测试补齐、小范围体验改良
- `QuickBug`
  - 已知模块、可稳定复现、Expected/Actual 明确的小 bug
- `BugInvestigation`
  - 现象明确但根因不清，需要先调查

## 3) 分流规则

收到 Human 的简单需求或 bug 时，`Dispatch Agent` 必须先执行分流：

1. 判断是否命中 `QuickCard` 适用范围
2. 判断类型是 `QuickTask`、`QuickBug` 还是 `BugInvestigation`
3. 判断是否应直接升级为正式任务卡

### 默认分流建议

- 简单迭代请求 -> `QuickTask`
- 小 bug 且复现充分 -> `QuickBug`
- bug 现象明确但缺根因 -> `BugInvestigation`
- 超出轻量范围 -> 正式任务卡

## 4) 强制升卡条件

执行前或执行中只要命中以下任一条件，必须从 `QuickCard` 升级为正式任务卡：

- 跨模块改动
- 需要修改 `Engine.Contracts` 或其他公开契约
- 需要边界文档同步
- 需要新增公开 API
- 预计超过半天
- 风险等级达到 `medium` 或 `high`
- 根因不明确，导致修复路径扩散
- 需要专项 QA / 归档 / 波次依赖控制
- 需要新增多个文件且影响模块边界

升卡后：

- 原 `QuickCard.Status` 置为 `Escalated`
- 回填 `EscalatedToTaskId`
- 在 `EscalationReason` 记录触发条件

## 5) 允许的状态流转

- `Todo -> InProgress`
- `InProgress -> Review`
- `Review -> Done`
- `InProgress -> Escalated`
- `Review -> Escalated`
- `Todo -> Rejected`

禁止：

- `Done -> Reopen`（轻量卡不承载复杂回流）
- `Escalated -> Done`（升卡后必须由正式任务卡继续）

## 6) 轻量验证门槛

`QuickCard` 不要求完整的正式任务卡门禁，但至少要有：

- Build 或最小编译验证
- 相关测试结果，或明确说明“无自动化测试，仅人工复现”
- 一条简短自检说明

若无法给出上述最小证据，不得进入 `Done`。

## 7) 与正式任务卡的边界

`QuickCard` 不得承载以下内容：

- 跨模块任务
- 边界/基线变更
- 公开 API 设计
- 复杂性能优化
- 大范围重构
- 正式 QA 验证卡
- 需要归档三件套的里程碑实施项

这些场景一律转正式任务卡。

## 8) 与现有流程衔接

推荐执行顺序：

1. Human 提交简单需求或 bug
2. Dispatch 判断：`QuickCard` 或正式任务卡
3. 若为 `QuickCard`，写入 `.ai-workflow/quickcards/`
4. Human 直接派发 `QuickCardId`
5. Execution 执行轻量闭环
6. 若命中升卡条件，Dispatch 补正式任务卡并接入原主流程

## 9) 归档建议

- `QuickCard` 默认不进入正式 `archive-index.md`
- 已完成轻量卡建议保留原文件并回填：
  - 完成时间
  - 变更摘要
  - 验证摘要
- 若升级为正式任务卡，轻量卡只保留“入口记录”和升卡去向

## 10) Dispatch 输出建议

当 Human 请求的是简单迭代或简单 bug 时，允许 `Dispatch Agent` 输出：

```md
Decision: quickcard | taskcard | reject
Type: QuickTask | QuickBug | BugInvestigation
QuickCardId: <id>
PrimaryModule: <module>
Priority: P0 | P1 | P2 | P3
WhyQuick: <one line>
EscalationGuard:
- <condition 1>
- <condition 2>
NextAction: <exact next step>
```
