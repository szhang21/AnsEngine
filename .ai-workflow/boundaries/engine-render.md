# Engine.Render 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Render`
- 版本：`v1.0`
- 负责人：`待指定`
- 生效日期：`2026-04-04`
- 关联任务卡：`待关联`

## 2) 目标与范围

- 模块目标：负责渲染后端调用与渲染指令提交，确保渲染流程稳定、可扩展、可验证。
- 适用范围：OpenGL 调用封装、Shader/Material/Mesh 资源绑定、渲染队列执行。
- 非适用范围：玩法状态管理、输入处理、场景编辑策略、资源导入流程编排。

## 3) 职责（Responsibilities）

- 负责渲染帧生命周期（BeginFrame/Draw/EndFrame）。
- 负责 GPU 资源绑定与状态切换控制。
- 负责 Shader 编译/链接错误信息输出。
- 负责渲染提交性能基础指标（draw call、state change）记录。

## 4) 非职责（Non-Responsibilities）

- 不负责实体生命周期管理（归属 `Engine.Scene`）。
- 不负责资源加载策略与缓存策略（归属 `Engine.Asset`）。
- 不负责窗口事件和输入语义（归属 `Engine.Platform`）。
- 不负责应用流程编排（归属 `Engine.App`）。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Core`
  - `Engine.Platform`（仅上下文/窗口相关抽象）
  - `Engine.Contracts`（仅消费渲染输入契约）
- 可使用基础库/第三方：
  - `OpenTK`
  - `System.Numerics`（如项目采用）

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.Scene`（禁止编译期项目引用；仅允许通过 `Engine.Contracts` 交互）
  - `Engine.Asset` 的导入器实现细节
- 禁止跨层调用模式：
  - 从 `Engine.Render` 直接修改场景图结构
  - 在渲染后端内执行玩法逻辑判断

## 7) 公开接口（Public Interfaces）

- `IRenderer`
  - 用途：渲染生命周期与绘制提交入口
  - 输入/输出：输入渲染上下文和绘制数据，输出渲染结果状态
  - 错误语义：不可恢复错误抛异常，可恢复错误返回状态并记录日志
  - 生命周期约束：初始化后方可使用；M3 起每帧至少执行一次可见清屏与基础 draw 提交；释放后不可再次调用

- `IShaderProgram`
  - 用途：Shader 程序封装与参数绑定
  - 输入/输出：输入源码或句柄，输出可绑定程序对象
  - 错误语义：编译/链接失败必须提供可定位信息
  - 生命周期约束：创建成功后可复用，析构时释放 GPU 资源

## 8) 数据与状态边界

- 模块内部可变状态：渲染队列、GL 状态缓存、资源句柄映射。
- 外部可观察状态：渲染统计信息、错误日志、帧完成状态。
- 线程模型与并发约束：OpenGL 上下文线程内调用；跨线程提交需走受控队列。
- 资源生命周期：显式 `Create -> Use -> Dispose`，禁止隐式悬挂资源。

## 9) 质量门禁与验收

- Build 验收：`dotnet build -c Debug/Release` 通过。
- Test 验收：渲染相关受影响测试通过。
- Smoke 验收：Demo 场景可稳定渲染 3-5 分钟。
- Perf 验收：相对基线无明显帧时间退化。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-04-17
  - 变更人：Exec-Render
  - 变更内容：渲染主路径从“CPU 直接写最终裁剪空间顶点”切换为“模型空间顶点 + shader `uMvp` uniform”，并在提交构建器中收敛 `MeshId -> 统一 mesh 入口`。
  - 变更原因：支撑 `TASK-REND-009`（合并 `TASK-REND-010` 目标），落地 M6 MVP uniform 最小真实渲染链路。
  - 风险与回滚方案：若发现个别驱动上的 uniform 兼容问题，可在提交层保留调试开关比对 CPU/GPU 路径，不回退 `Render -> Contracts` 依赖方向。

- 2026-04-15
  - 变更人：Exec-Render
  - 变更内容：`SceneRenderSubmissionBuilder` 新增顶点 transform 应用（Scale -> Rotation -> Translation），并保留 identity 快路径兼容旧布局。
  - 变更原因：支撑 `TASK-REND-008`，实现 Render 对 M5 变换契约（含 Rotation）的消费落地。
  - 风险与回滚方案：若后续出现提交抖动，可在构建器层增加阈值与缓存策略；不恢复到忽略 transform 的提交行为。

- 2026-04-14
  - 变更人：Exec-Render
  - 变更内容：移除 `NullRenderer` 默认回退 provider 与 `DefaultSceneRenderContractProvider`，Render 生产路径改为强制外部显式注入 `ISceneRenderContractProvider`。
  - 变更原因：支撑 `TASK-REND-007`，暴露组合根漏注入问题，避免静默兜底掩盖装配缺陷。
  - 风险与回滚方案：若临时脚手架需本地调试兜底，可在测试工厂显式提供 provider，不恢复生产默认回退。

- 2026-04-14
  - 变更人：Exec-Render
  - 变更内容：`Engine.Render` 从 `Engine.Scene` 编译期依赖切换为 `Engine.Contracts`，`Render` 内部消费类型统一为契约层类型。
  - 变更原因：落实 `TASK-REND-006` 依赖反转目标，恢复 `Scene -> Contracts <- Render` 解耦方向。
  - 风险与回滚方案：若下游装配未及时切换，可短期保留适配器桥接；禁止回退到 `Render -> Scene` 项目引用。

- 2026-04-04
  - 变更人：初始化
  - 变更内容：创建 `Engine.Render` 初版边界合同
  - 变更原因：建立任务派发与归档可追溯边界基线
  - 风险与回滚方案：若约束过严阻碍迭代，可通过评审后小步放宽并记录版本差异
- 2026-04-07
  - 变更人：Exec-Render
  - 变更内容：补充 `IRenderer` 最小可见反馈约束（逐帧清屏或绘制提交）
  - 变更原因：支撑 `TASK-REND-001` 的首个可见渲染反馈验收
  - 风险与回滚方案：若后续引入完整渲染管线，可将“清屏保底”降级为开发模式兜底策略
- 2026-04-08
  - 变更人：Exec-Render
  - 变更内容：补充 M3 最小三角形链路约束（shader 编译/链接、顶点提交、draw 调用）
  - 变更原因：支撑 `TASK-REND-002` 首帧三角形可见验收
  - 风险与回滚方案：若后续引入材质/资源系统，保留该链路作为渲染回归最小样例
- 2026-04-11
  - 变更人：Exec-Render
  - 变更内容：明确渲染模块可消费 `Engine.Scene` 契约层提交（`SceneRenderFrame`），并将固定 demo 提交路径替换为场景驱动提交构建器
  - 变更原因：支撑 `TASK-REND-004` 的 M4 场景驱动渲染消费链路
  - 风险与回滚方案：若提交字段扩展导致构建器复杂度上升，拆分 `SubmissionTranslator` 分层并保留最小回归路径
- 2026-04-12
  - 变更人：Workflow
  - 变更内容：补充 `Engine.Render -> Engine.Contracts` 为允许依赖，明确 Render 只消费契约层输入。
  - 变更原因：落实 `Render` 与 `Scene` 解耦，避免直接模块编译期耦合固化。
  - 风险与回滚方案：若契约层字段调整频繁，采用版本化契约并保留兼容解析层。
- 2026-04-13
  - 变更人：Exec-Render
  - 变更内容：按当前真实实现回对边界依赖：`Engine.Render` 直接依赖 `Engine.Scene`（契约消费），暂未落地 `Engine.Contracts` 物理模块。
  - 变更原因：支撑 `TASK-REND-005` 文档与实现一致性修正，消除边界文档漂移。
  - 风险与回滚方案：在后续 `TASK-SCENE-003/TASK-REND-006` 完成契约下沉后，再将依赖回切到 `Engine.Contracts`。
