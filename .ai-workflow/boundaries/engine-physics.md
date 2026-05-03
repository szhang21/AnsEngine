# Engine.Physics 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Physics`
- 版本：`v0.1`
- 负责人：`待指定`
- 生效日期：`2026-05-03`
- 关联任务卡：`TASK-PHYS-001`

## 2) 目标与范围

- 模块目标：为引擎提供最小可验证的 physics foundation，负责从 SceneData 物理组件构建 physics world、执行固定步进统计并输出只读 snapshot / query 结果。
- 适用范围：physics world、body/collider 描述消费、fixed-step statistics、AABB 计算、overlap / ground query、显式失败语义。
- 非适用范围：Transform 回写、App 主循环调度、可见物理反馈、重力/碰撞求解、力/冲量/摩擦/反弹、Editor physics UI。

## 3) 职责（Responsibilities）

- 负责从 `SceneDescription` 中消费 `Transform`、`RigidBody`、`BoxCollider` 描述并构建 physics world。
- 负责执行固定步进记账与 world snapshot 生成。
- 负责输出 AABB、overlap query、ground query 等只读查询结果。
- 负责对 malformed physics input 提供可诊断的显式失败结果或被测试钉死的稳定异常语义。

## 4) 非职责（Non-Responsibilities）

- 不负责 Scene runtime object/component 所有权与 Transform 更新（归属 `Engine.Scene`）。
- 不负责 Scene JSON 解析、schema 校验和文档 round-trip（归属 `Engine.SceneData`）。
- 不负责应用装配、输入、时间调度与运行模式（归属 `Engine.App`）。
- 不负责渲染、OpenGL、OpenTK、ImGui 或 GPU 资源。
- 不负责脚本、编辑器或 Play Mode。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.SceneData`
  - `Engine.Contracts`
  - `Engine.Core`
- 可使用基础库/第三方：
  - `.NET` 标准库
  - `System.Numerics`

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.Scene`
  - `Engine.App`
  - `Engine.Render`
  - `Engine.Scripting`
  - `Engine.Editor`
  - `Engine.Editor.App`
- 禁止跨层调用模式：
  - 在 `Engine.Physics` 内写回 Scene Transform
  - 在 `Engine.Physics` 内接入 App 主循环
  - 在 `Engine.Physics` 内使用 OpenTK / OpenGL / ImGui

## 7) 公开接口（Public Interfaces）

- `PhysicsWorld`
  - 用途：承载 physics bodies/colliders 状态并提供 load / step / snapshot / query 入口
  - 输入/输出：输入 `SceneDescription` 与 `PhysicsStepContext`，输出 world state、snapshot 与 query result
  - 错误语义：malformed physics input 必须可诊断
  - 生命周期约束：`Load -> Step* -> Snapshot/Query`

- `PhysicsStepContext`
  - 用途：描述固定步进输入
  - 输入/输出：输入 fixed delta / total or equivalent stable step data，输出 world 统计更新
  - 错误语义：非法步进参数需显式失败或稳定异常
  - 生命周期约束：短生命周期值对象

- `PhysicsWorldSnapshot` / `PhysicsBodySnapshot` / `PhysicsAabb`
  - 用途：对外提供只读 physics state 观察面
  - 输入/输出：输出 body 基本信息、body type、collider、AABB 与统计
  - 错误语义：不暴露内部可变集合
  - 生命周期约束：按需创建，不持有外部资源

## 8) 数据与状态边界

- 模块内部可变状态：body/collider internal state、step count、accumulated fixed seconds。
- 外部可观察状态：body count、snapshot、AABB、overlap / ground query result。
- 线程模型与并发约束：默认主线程使用；不提供并发 world mutation。
- 资源生命周期：不持有窗口、GPU、文件句柄或外部运行时对象所有权。

## 9) 质量门禁与验收

- Build 验收：`dotnet build AnsEngine.sln --nologo -v minimal` 通过。
- Test 验收：`Engine.Physics.Tests` 与相关 boundary tests 通过。
- Smoke 验收：真实 SceneData fixture 可进入 `SceneDescription -> PhysicsWorld -> Step -> Snapshot/Query` 主路径。
- Perf 验收：不引入逐帧 JSON 解析、App loop 绑定、Transform 回写或渲染 side path。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-05-03
  - 变更人：Dispatch-Agent
  - 变更内容：建立 `Engine.Physics` 初版边界合同种子，预留 M19 Physics foundation 的 world、fixed step、snapshot 与 query 能力边界。
  - 变更原因：支撑 `TASK-PHYS-001`，在进入实现前先固定 Physics 对 `SceneData/Contracts/Core` 的依赖方向，以及“无 Transform 回写、无 App 接线、无渲染感知”的硬约束。
  - 风险与回滚方案：若后续需要 Runtime MVP、Scene Transform writeback 或 App fixed-step 调度，必须另立任务和边界变更；当前不允许在此合同内静默扩张。
