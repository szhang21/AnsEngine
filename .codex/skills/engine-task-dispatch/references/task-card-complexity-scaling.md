# 任务卡复杂度与信息量联动规则（Complexity Scaling Rule）

用途：约束 `Dispatch Agent` 在正式任务卡中，必须让“任务复杂度”和“卡片信息密度”同步增长。  
目标：避免高复杂度任务被压缩成低信息量任务卡，迫使 `Execution Agent` 回头翻计划、里程碑或靠经验脑补。

## 核心原则

- 任务越复杂，任务卡必须越详细。
- 信息量不足不是“风格问题”，而是 `TaskCardInsufficient`。
- 不允许出现“复杂任务 + 简短卡片 + 让执行者自己理解”的组合。

## 复杂度维度

以下维度用于评估正式任务卡复杂度；并非要求全部命中，按实际情况累计：

1. 边界复杂度
   - 是否涉及模块边界、依赖方向、职责划分
2. 实现复杂度
   - 是否存在多步骤实施顺序、多个关键路径或切换点
3. 失败语义复杂度
   - 是否需要明确 fallback、错误收口、回退策略
4. 验证复杂度
   - 是否需要多种验证方式才能证明完成
5. 参考复杂度
   - 是否必须同时引用多个源码入口、测试入口、已有任务或归档
6. 认知复杂度
   - 是否容易误做、误扩张、误解边界或误判取舍

## 复杂度等级

### Level 1: 低复杂度

特征：
- 单模块、单入口、单关键路径
- 失败语义简单
- 边界清晰，不容易误解

最低要求：
- `MilestoneContext` 至少回答“为什么做”和“承担什么作用”
- `DecisionCarryOver` 至少 1-2 条
- `ImplementationNotes` 至少覆盖实现入口与关键步骤
- `DesignConstraints` 至少 2 条
- `ExamplesOrReferences` 至少 1 个源码入口 + 1 个测试或参考点

### Level 2: 中复杂度

特征：
- 仍以单模块为主，但存在多个关键路径、fallback、共享状态或多个验证面
- 容易误做成别的东西

最低要求：
- `MilestoneContext` 必须写清背景、作用、直接影响因素
- `DecisionCarryOver` 必须区分“继承决策”和“不可推翻取舍”
- `ImplementationNotes` 必须写明顺序、切换点、关键覆盖路径
- `DesignConstraints` 必须覆盖边界、禁止实现方式、禁止顺手扩张
- `FallbackBehavior` 必须明确哪些失败可降级、哪些必须回退
- `ExamplesOrReferences` 至少 2 个源码入口 + 1 个测试入口 + 1 个相关任务/归档
- `OpenQuestions` 不得留空白，必须写 `none` 或真实未决问题

### Level 3: 高复杂度

特征：
- 虽然仍应拆卡，但这张卡本身仍然认知负担高
- 包含明显的边界风险、错误路线风险、失败收口风险或多入口协作风险
- 如果写短，执行者很容易做偏

最低要求：
- `MilestoneContext` 必须足以替代回看里程碑全文中的相关段落
- `DecisionCarryOver` 必须完整列出不可推翻的上游设计取舍
- `ImplementationNotes` 必须体现明确实施阶段、阶段目标、关键保护点
- `DesignConstraints` 必须能阻止常见错误路线
- `FallbackBehavior` 必须分清降级、报错、停工回退三类
- `ExamplesOrReferences` 必须给出多入口源码定位、测试定位和至少一个历史任务/归档关联
- `Acceptance` 必须体现本卡级别的验证口径，而不是只复述里程碑口号
- `OpenQuestions` 若非 `none`，必须明确哪些问题一旦触发就停工回退

## Dispatch 判定规则

`Dispatch Agent` 在落正式任务卡前，必须先判断复杂度等级，再检查信息量是否与等级匹配。

### 必须判定为 Level 2 或更高的典型信号

- 有 fallback 或错误收口要求
- 有共享 cache / 生命周期 / 状态复用要求
- 有多条关键实现路径
- 容易误做成跨边界实现
- 需要同时参考多个代码入口

### 必须判定为 Level 3 的典型信号

- 若实现路线选错，会直接破坏模块边界
- 若失败语义写不清，会导致运行时行为错误
- 若执行者不看上游设计取舍就很容易做偏
- 需要多个关键约束同时成立才能算完成

## 失败判定

以下情况直接判定为 `TaskCardInsufficient`：

- 复杂度已达 Level 2，但任务卡仍接近 Level 1 的信息量
- 复杂度已达 Level 3，但任务卡没有体现阶段、保护点、失败分流
- 任务卡明显短于完成该任务所需的最小局部真相
- Dispatch 以“示例差不多这样写”为由拒绝继续补充

## 输出建议

建议在正式任务卡或 Dispatch 自检中附带：

```md
ComplexityAssessment:
- Level: `L1 | L2 | L3`
- Why:
  - `<reason 1>`
  - `<reason 2>`
- SufficiencyMatch: `pass | fail`
```
