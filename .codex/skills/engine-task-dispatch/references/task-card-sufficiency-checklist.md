# 任务卡充分性检查清单（Task Card Sufficiency Checklist）

用途：供 `Dispatch Agent` 在落正式任务卡前执行“执行充分性检查”，判断该卡是否已经足够让 `Execution Agent` 脱离里程碑全文独立实施。

## 使用原则

- 本清单只用于正式任务卡，不用于 `QuickCard`。
- 任一关键项回答为 `No`，该卡默认不得进入执行流。
- 若问题属于“可以靠执行者临场判断”，默认仍视为 `No`，因为这意味着任务卡没有把关键信息落下来。

## 核心检查项

### A. 目标与结果

1. 执行者只看任务卡，是否能明确知道这张卡最终要交付什么？
2. 这张卡是否仍然只有单一结果，而不是多个结果混在一起？
3. 验收标准是否能对应到本卡的具体实现，而不是只描述里程碑目标？

### B. 上下文与决策

4. `MilestoneContext` 是否只保留与本卡直接相关的背景，而不是空话或整段复制？
5. `DecisionCarryOver` 是否写清了本卡不能推翻的既定取舍？
6. 执行者是否无需再回看里程碑全文，就能理解“为什么要这样做”？
6.1 若计划/里程碑已给出示例数据结构或字段草图，任务卡是否已把这些结构约束明确写入 `DecisionCarryOver`？

### C. 实施与边界

7. `ImplementationNotes` 是否足以指导执行者找到实现入口和关键路径？
8. `DesignConstraints` 是否写清了明确禁止的做法和不可突破的边界？
9. `OutOfScope` 是否足以防止执行者顺手扩张？
10. `AllowedPaths` 是否与真实预期改动范围一致？

### D. 失败语义与风险

11. `FallbackBehavior` 是否说明了失败时该如何降级、报错或回退？
12. 执行者是否能判断哪些异常属于可继续，哪些必须停下修卡？
13. 关键风险是否已经下沉到卡里，而不是仅存在于计划或里程碑文本？

### E. 参考点与验证

14. `ExamplesOrReferences` 是否给出了足够的源码入口、测试入口或已有卡片/归档参考？
14.1 若计划/里程碑存在示例数据结构，`ExamplesOrReferences` 是否给出了对应计划段落/小节/标题定位，而不是模糊地写“见计划”？
15. 执行者是否能据此快速定位相关代码，而不是自己重新全仓搜索？
16. `Acceptance` 是否清楚说明了 Build/Test/Smoke/Perf 的本卡口径？

### F. 未决问题与执行充分性

17. `OpenQuestions` 是否把真正还没定的点列出来了，而不是假装没有问题？
18. 对这些未决问题，是否明确写了“遇到即回退，不得自行脑补”？
19. `ExecutionReadiness.ExecutionReady` 是否确实应该为 `true`，而不是为了流转方便硬写成 `true`？
20. 如果把里程碑文件藏起来，执行者是否仍能稳定开工并避免关键误判？

## 判定规则

- 以下任一项为 `No`，判定为 `TaskCardInsufficient`：
  - 1
  - 3
  - 5
  - 6.1
  - 7
  - 8
  - 11
  - 14
  - 14.1
  - 16
  - 19
  - 20
- 若非关键项累计 `No >= 3`，也判定为 `TaskCardInsufficient`。

## 输出建议

当清单未通过时，`Dispatch Agent` 应返回最小修卡结论：

```md
FailureType: TaskCardInsufficient
BlockedBy: task-card-sufficiency-checklist.md
RequiredFix:
- 补齐缺失的实施上下文
- 补齐缺失的设计约束 / 失败语义 / 参考点
- 重新执行 ExecutionReadiness 自检
Owner: DispatchAgent
RetryCommand: 请按 Dispatch Agent 职责重试并补齐任务卡充分性字段
Evidence:
- FailedChecks: [5, 7, 11, 14]
```
