# 轻量卡: QINV-001 立方体渲染形态异常调查

## QuickCardId
`QINV-001`

## Type
`BugInvestigation`

## Goal
定位“当前 mesh 是立方体但画面结果明显异常”的首个主要原因，并给出可执行结论：`CloseAsNotRepro | CloseAsByDesign | EscalateToTaskCard | ConvertToQuickBug`。

## Reporter
`Human`

## Owner
`pending`

## Priority
`P1`

## PrimaryModule
`Engine.Render`

## RelatedPlan
`PLAN-M10-2026-04-25`

## RelatedTaskId
`TASK-SCENE-009`

## Summary
- 当前样例 mesh 逻辑上是立方体，但最终渲染结果“不像正常立方体”，需要先判断问题更接近摄像机参数、MVP 变换、深度状态、线框绘制还是 mesh 数据消费路径。
- 先限定为渲染侧入口调查，优先检查相机矩阵、投影参数、裁剪/深度状态和线框可视化路径。

## Expected
- 输出一个明确调查结论，并附最小证据。
- 若根因局限在 `Engine.Render` 单模块且修复范围小，可转成 `QuickBug`。
- 若问题扩散到 `Scene/App/Contracts/Asset`、需要重设计或风险升高，必须升正式任务卡。

## Actual
- 当前显示结果与“正常可辨识的立方体”预期不符，肉眼观察上形态怪异，用户已怀疑可能与摄像机参数有关。

## Repro
- 运行当前默认场景。
- 观察屏幕中的立方体输出结果。
- 对比“预期立方体轮廓/朝向/近远关系”与当前实际画面。

## ScopeLimit
- 仅允许先在 `Engine.Render` 范围内调查与验证：
  - 相机 view/projection 消费路径
  - MVP 计算与裁剪空间结果
  - 线框绘制与深度状态
  - mesh 顶点/索引在 Render 侧的消费结果
- 明确不做：
  - 不改 `Engine.Contracts` 公共契约
  - 不改 `Engine.SceneData` schema
  - 不做 lighting 新特性顺手改造
  - 不做跨模块大重构

## Acceptance
- Build: 至少完成一次最小编译/构建验证
- Test: 至少补充一条调查证据，或明确“无自动化测试，仅人工复现/矩阵打印/截图比对”
- Smoke: 至少完成一次本地复现与结论记录

## EscalationRule
- 若触发以下任一条件，必须升级为正式任务卡：
  - 根因涉及 `Engine.Scene`、`Engine.App`、`Engine.Contracts`、`Engine.Asset`
  - 需要修改公开契约或依赖方向
  - 需要边界文档更新
  - 预计超过半天
  - 风险达到 `medium/high`
  - 根因不清且调查路径继续扩散

## Evidence
- 用户描述：当前对象应为立方体，但渲染结果异常
- 初始怀疑点：摄像机参数 / 视图投影消费路径

## Status
`Todo`

## Completion
`0`

## Escalation
- EscalatedToTaskId:
- EscalationReason:
