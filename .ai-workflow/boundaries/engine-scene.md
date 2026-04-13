# Engine.Scene 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Scene`
- 版本：`v1.0`
- 负责人：`待指定`
- 生效日期：`2026-04-04`
- 关联任务卡：`待关联`

## 2) 目标与范围

- 模块目标：负责场景数据组织与实体层级管理，向渲染与系统模块提供稳定、可查询的场景状态。
- 适用范围：实体创建与销毁、Transform 层级、可见性数据准备、场景查询接口。
- 非适用范围：底层 OpenGL 调用、资源导入与缓存策略、窗口输入处理、应用主流程调度。

## 3) 职责（Responsibilities）

- 负责 Entity 与组件关系维护（若采用 ECS/轻 ECS 由本模块主导）。
- 负责 Transform 更新与层级变换传播。
- 负责提供可见对象查询结果给渲染模块。
- 负责场景状态一致性校验（如父子关系有效性）。

## 4) 非职责（Non-Responsibilities）

- 不负责 GPU 资源创建与绘制提交（归属 `Engine.Render`）。
- 不负责资源加载与热重载（归属 `Engine.Asset`）。
- 不负责输入设备语义解释（归属 `Engine.Platform`）。
- 不负责程序启动流程与模块装配（归属 `Engine.App`）。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Core`
  - `Engine.Contracts`（渲染输入契约层）
- 可使用基础库/第三方：
  - `System.Numerics`（如项目采用）
  - 项目内部数学工具（若在 `Engine.Core`）

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.Render` 的 OpenGL 实现细节
  - `Engine.Asset` 的导入器实现
- 禁止跨层调用模式：
  - 在 `Scene` 内直接调用 OpenGL API
  - 在 `Scene` 内执行资源导入与文件 IO 流程编排

## 7) 公开接口（Public Interfaces）

- `IScene`
  - 用途：场景对象管理与查询入口
  - 输入/输出：输入实体/组件操作请求，输出场景状态或查询结果
  - 错误语义：非法状态返回可诊断错误；不可恢复异常抛出
  - 生命周期约束：初始化后可用，释放后不可访问

- `ITransformSystem`
  - 用途：处理层级变换与世界矩阵更新
  - 输入/输出：输入局部变换，输出世界变换缓存
  - 错误语义：循环依赖或非法父子关系需显式报错
  - 生命周期约束：随场景生命周期创建与释放

## 8) 数据与状态边界

- 模块内部可变状态：实体表、组件表、层级关系、可见性缓存。
- 外部可观察状态：场景快照、可见对象列表、调试统计信息。
- 线程模型与并发约束：默认主线程写入；并发读取需通过快照或只读视图。
- 资源生命周期：场景内引用资源句柄，不持有资源导入生命周期主控权。

## 9) 质量门禁与验收

- Build 验收：`dotnet build -c Debug/Release` 通过。
- Test 验收：场景结构与变换传播相关测试通过。
- Smoke 验收：Demo 场景实体增删与层级变化运行稳定。
- Perf 验收：场景更新相对基线无明显退化。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-04-14
  - 变更人：Exec-Render
  - 变更内容：确认 `Engine.Render` 已切换为仅消费 `Engine.Contracts`，`Engine.Scene` 不再被渲染模块编译期直接引用。
  - 变更原因：对应 `TASK-REND-006` 的依赖反转落地，巩固 Scene 模块边界隔离。
  - 风险与回滚方案：若后续出现接口兼容问题，通过契约层适配器处理，避免恢复跨模块直接引用。

- 2026-04-13
  - 变更人：Exec-Scene
  - 变更内容：`SceneGraphService` 新增 `Engine.Contracts.ISceneRenderContractProvider` 适配实现，并将 `Engine.Scene` 项目依赖接入 `Engine.Contracts`；保留 `Engine.Scene` 旧契约返回用于过渡兼容。
  - 变更原因：支撑 `TASK-SCENE-003`，将渲染输入契约消费方向从 Scene 内部类型下沉到独立契约层。
  - 风险与回滚方案：若下游迁移节奏不一致，继续维持双接口兼容窗口；待 `TASK-REND-006` 完成后可清理旧兼容路径。

- 2026-04-04
  - 变更人：初始化
  - 变更内容：创建 `Engine.Scene` 初版边界合同
  - 变更原因：建立任务派发与边界追踪基线
  - 风险与回滚方案：若查询接口与性能冲突，评审后分层拆分并版本化记录
- 2026-04-11
  - 变更人：Exec-Scene
  - 变更内容：新增 `Scene -> Render` 最小提交契约（`SceneRenderItem`、`SceneRenderFrame`、`ISceneRenderContractProvider`），明确仅输出渲染数据快照，不承载渲染实现
  - 变更原因：支撑 `TASK-SCENE-001` 的 M4 契约基线
  - 风险与回滚方案：若后续提交字段扩展过快，拆分 `RenderSubmissionV2` 并保持旧契约兼容窗口
- 2026-04-11
  - 变更人：Exec-Scene
  - 变更内容：在不扩展契约结构的前提下补充最小动态输出口径（首节点材质参数按帧变化）
  - 变更原因：支撑 `TASK-SCENE-002` 的“至少一个场景节点驱动画面变化”验收
  - 风险与回滚方案：若后续需要显式动画曲线参数，新增可选字段并保持默认行为兼容
- 2026-04-12
  - 变更人：Workflow
  - 变更内容：补充 `Engine.Scene -> Engine.Contracts` 为允许依赖，明确 Scene 仅输出契约层数据，不直接面向 Render 实现层。
  - 变更原因：配合 M4 解耦治理，建立 `Scene -> Contracts -> Render` 依赖方向基线。
  - 风险与回滚方案：若后续契约拆分为 `Engine.Render.Contracts`，保留一段兼容窗口并同步更新任务卡依赖约束。
