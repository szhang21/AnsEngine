# Engine.Contracts 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Contracts`
- 版本：`v1.3`
- 负责人：`待指定`
- 生效日期：`2026-04-12`
- 关联任务卡：`TASK-CONTRACT-001`, `TASK-CONTRACT-002`, `TASK-CONTRACT-003`, `TASK-SCENE-005`, `TASK-REND-008`

## 2) 目标与范围

- 模块目标：提供跨模块共享的稳定契约（接口与只读数据结构），用于解耦 `Engine.Scene` 与 `Engine.Render` 的编译期依赖。
- 适用范围：渲染输入契约、只读数据模型、跨模块接口定义、契约演进兼容策略。
- 非适用范围：渲染实现、场景运行逻辑、应用编排、平台与资源细节。

## 3) 职责（Responsibilities）

- 定义跨模块共享契约类型（如 `SceneRenderItem`、`SceneRenderFrame`、`SceneTransform`、`SceneCamera`）。
- 定义渲染输入提供器接口（`ISceneRenderContractProvider`）。
- 维持向后兼容策略（新增字段优先可选，避免破坏性改动）。
- 提供最小语义注释，确保调用方理解输入输出约束。

## 4) 非职责（Non-Responsibilities）

- 不负责 OpenGL/GPU 调用（归属 `Engine.Render`）。
- 不负责场景实体生命周期与运行态（归属 `Engine.Scene`）。
- 不负责主循环、DI 装配与模块编排（归属 `Engine.App`）。
- 不负责资源导入与平台 IO（归属 `Engine.Asset` / `Engine.Platform`）。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Core`（基础类型）
- 可使用基础库/第三方：
  - `System`
  - `System.Numerics`

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.Render`
  - `Engine.Scene`
  - `Engine.App`
  - `Engine.Asset`
  - `Engine.Platform`
- 禁止跨层调用模式：
  - 在契约层实现业务行为或流程分支
  - 在契约层持有运行时资源句柄（GL/窗口/文件句柄等）

## 7) 公开接口（Public Interfaces）

- `ISceneRenderContractProvider`
  - 用途：向渲染侧提供只读渲染输入快照。
  - 输入/输出：无入参，返回 `SceneRenderFrame`。
  - 错误语义：非法契约数据应可诊断，不得静默吞错。
  - 生命周期约束：由上层（`Engine.App`）装配并管理生命周期。

- `SceneRenderFrame`
  - 用途：承载单帧渲染输入集合与相机语义。
  - 输入/输出：作为只读传输对象供 `Render` 消费。
  - 错误语义：空集合合法；字段值需可校验。
  - 生命周期约束：值语义/只读语义，消费侧不修改源数据。

- `SceneRenderItem`
  - 用途：承载单个可渲染节点输入（网格、材质、变换）。
  - 兼容约束：保留 `new SceneRenderItem(nodeId, meshId, materialId)` 旧构造路径，默认 `Transform=Identity`。

- `SceneTransform`
  - 用途：承载位置、缩放、旋转（四元数）变换输入。
  - 兼容约束：提供 `Identity` 作为无显式 transform 场景的默认值。

- `SceneCamera`
  - 用途：承载最小视图/投影语义（View/Projection）。
  - 兼容约束：提供 `Identity`，保证下游未显式提供相机时的默认兼容路径。

## 8) 数据与状态边界

- 模块内部可变状态：无（尽量无状态）。
- 外部可观察状态：契约类型定义与版本语义。
- 线程模型与并发约束：契约对象默认按不可变/只读语义设计。
- 资源生命周期：仅数据对象生命周期，不持有外部资源所有权。

## 9) 质量门禁与验收

- Build 验收：`dotnet build -c Debug/Release` 通过。
- Test 验收：契约兼容与映射相关测试通过。
- Smoke 验收：Scene -> Render 契约数据传递路径可用。
- Perf 验收：契约演进后无明显额外分配与初始化退化。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-04-17
  - 变更人：Exec-Scene / Exec-Render
  - 变更内容：`Engine.Scene` 与 `Engine.Render` 已在生产链路实消费 `SceneCamera + SceneRenderFrame.Camera`；M6 MVP uniform 路径以该契约字段作为唯一相机输入。
  - 变更原因：同步 `TASK-SCENE-006` 与 `TASK-REND-009` 落地状态，避免契约文档与实际消费链路漂移。
  - 风险与回滚方案：若后续引入多相机策略，保持 `SceneRenderFrame` 单相机字段向后兼容并通过扩展字段演进，不回退现有最小契约。

- 2026-04-17
  - 变更人：Exec-Contracts
  - 变更内容：新增 `SceneCamera(View, Projection)`，`SceneRenderFrame` 扩展 `Camera` 字段并提供 identity 默认兼容路径。
  - 变更原因：支撑 `TASK-CONTRACT-003`，为 M6 MVP 链路提供最小视图/投影契约语义。
  - 风险与回滚方案：若下游尚未消费相机字段，继续使用 identity 相机运行；后续通过可选字段方式演进保持向后兼容。

- 2026-04-15
  - 变更人：Exec-Render
  - 变更内容：确认 Render 提交构建链路已消费 `SceneTransform` 并应用 Position/Scale/Rotation，identity 路径保持兼容。
  - 变更原因：支撑 `TASK-REND-008`，完成 M5 变换契约消费闭环。
  - 风险与回滚方案：若后续出现变换热路径抖动，可在 Render 端优化，不回退契约字段。

- 2026-04-15
  - 变更人：Exec-Scene
  - 变更内容：确认 Scene 输出链路已稳定产出 `SceneTransform`（含 Rotation），首帧维持 identity 兼容语义。
  - 变更原因：支撑 `TASK-SCENE-005`，完成 M5 变换契约生产闭环。
  - 风险与回滚方案：若动态参数需收敛，可仅调整 Scene 运行时输出策略，不变更契约结构。

- 2026-04-14
  - 变更人：Exec-Contracts
  - 变更内容：新增 `SceneTransform(Position/Scale/Rotation)`，`SceneRenderItem` 扩展 `Transform` 字段并保留旧三参构造默认 `Identity` 的兼容行为。
  - 变更原因：支撑 `TASK-CONTRACT-002`，为 M5 变换链路（含 Rotation）提供稳定契约。
  - 风险与回滚方案：若下游对 transform 消费未完成，仍可沿用默认 identity 路径；必要时仅禁用新字段消费，不回退契约结构。

- 2026-04-13
  - 变更人：Exec-Scene
  - 变更内容：按 `TASK-CONTRACT-001` 新建 `Engine.Contracts` 模块并落地最小渲染契约基线。
  - 变更原因：将共享契约自 `Engine.Scene` 抽离，为后续任务提供稳定依赖面。
  - 风险与回滚方案：过渡期可短暂保留适配桥接，待下游迁移完成后清理。

- 2026-04-12
  - 变更人：Workflow
  - 变更内容：创建 `Engine.Contracts` 初版边界合同。
  - 变更原因：为 M4 解耦改造建立正式边界基线。
  - 风险与回滚方案：若契约粒度不合适，可拆分子层并保持兼容窗口。
