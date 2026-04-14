# Engine.Contracts 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Contracts`
- 版本：`v1.0`
- 负责人：`待指定`
- 生效日期：`2026-04-12`
- 关联任务卡：`TASK-CONTRACT-001`

## 2) 目标与范围

- 模块目标：提供跨模块共享的稳定契约（接口与数据结构），用于解耦 `Engine.Scene` 与 `Engine.Render` 的编译期依赖。
- 适用范围：渲染输入契约、只读数据结构、跨模块接口定义、版本化契约类型。
- 非适用范围：渲染实现逻辑、场景运行逻辑、应用编排、平台/资源实现细节。

## 3) 职责（Responsibilities）

- 负责定义可跨模块共享的契约类型（例如 `SceneRenderFrame`、`SceneRenderItem`）。
- 负责定义渲染输入提供者接口（例如 `ISceneRenderContractProvider`）。
- 负责维持契约演进的向后兼容策略（字段新增优先可选、避免破坏性变更）。
- 负责提供最小必要的语义注释，确保调用方理解输入输出约束。

## 4) 非职责（Non-Responsibilities）

- 不负责任何 OpenGL/GPU 调用（归属 `Engine.Render`）。
- 不负责场景实体生命周期与运行态维护（归属 `Engine.Scene`）。
- 不负责主循环、DI 装配与模块编排（归属 `Engine.App`）。
- 不负责资源导入与平台 IO 行为（归属 `Engine.Asset` / `Engine.Platform`）。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Core`（仅基础类型）
- 可使用基础库/第三方：
  - `System`
  - `System.Numerics`（如契约字段需要）

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.Render`
  - `Engine.Scene`
  - `Engine.App`
  - `Engine.Asset`
  - `Engine.Platform`
- 禁止跨层调用模式：
  - 在契约层中引入行为实现或业务流程分支
  - 在契约层中持有运行时资源句柄（GL/窗口/文件句柄等）

## 7) 公开接口（Public Interfaces）

- `ISceneRenderContractProvider`（示例）
  - 用途：向渲染侧提供只读渲染输入快照。
  - 输入/输出：输入无副作用调用上下文，输出 `SceneRenderFrame`。
  - 错误语义：契约数据非法应返回可诊断错误，不得吞错。
  - 生命周期约束：由上层（`Engine.App`）装配并按应用生命周期管理。

- `SceneRenderFrame`（示例）
  - 用途：承载一帧可渲染数据集合。
  - 输入/输出：作为只读传输对象供 `Render` 消费。
  - 错误语义：空集合合法；非法字段值需可检测。
  - 生命周期约束：值语义/只读语义，禁止在消费侧原地修改源数据。

## 8) 数据与状态边界

- 模块内部可变状态：无（应尽量保持无状态）。
- 外部可观察状态：契约类型定义与版本标识。
- 线程模型与并发约束：契约对象默认按不可变/只读语义设计。
- 资源生命周期：仅数据对象生命周期，不持有外部资源所有权。

## 9) 质量门禁与验收

- Build 验收：`dotnet build -c Debug/Release` 通过。
- Test 验收：契约序列化/映射/兼容性相关测试通过（如存在）。
- Smoke 验收：集成链路可通过契约完成 Scene -> Render 数据传递。
- Perf 验收：契约引入后无明显额外分配和链路退化。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-04-14
  - 变更人：Exec-Scene
  - 变更内容：`Engine.Scene` 完成单契约出口收敛，移除 Scene 内部镜像契约定义，Scene->Render 路径仅保留 `Engine.Contracts` 类型。
  - 变更原因：支撑 `TASK-SCENE-004`，巩固契约层作为跨模块唯一共享语义源。
  - 风险与回滚方案：若后续字段演进影响下游，按契约层版本策略扩展字段并保持向后兼容。

- 2026-04-14
  - 变更人：Exec-Render
  - 变更内容：`Engine.Render` 完成契约层消费迁移，移除对 `Engine.Scene` 的编译期依赖，改为只消费 `Engine.Contracts`。
  - 变更原因：支撑 `TASK-REND-006`，闭合 M4 `Scene/Render` 解耦链路。
  - 风险与回滚方案：若后续字段演进频繁，继续采用版本化契约并保持兼容窗口，避免模块回耦。

- 2026-04-13
  - 变更人：Exec-Scene
  - 变更内容：按 `TASK-CONTRACT-001` 新增独立模块 `src/Engine.Contracts`，落地最小渲染契约类型 `SceneRenderItem/SceneRenderFrame/ISceneRenderContractProvider`。
  - 变更原因：将跨模块共享契约从 `Engine.Scene` 抽离，为后续 `TASK-SCENE-003/TASK-REND-006/TASK-APP-004` 提供稳定前置依赖。
  - 风险与回滚方案：若后续消费迁移存在兼容问题，可在过渡期保留 Scene 侧镜像类型并通过适配器桥接，待下游完成后移除。

- 2026-04-12
  - 变更人：Workflow
  - 变更内容：创建 `Engine.Contracts` 初版边界合同，定义契约层职责与依赖边界。
  - 变更原因：为 M4 解耦改造提供正式边界基线，避免 `Render -> Scene` 直接依赖固化。
  - 风险与回滚方案：若契约粒度不合适，后续可拆分为 `Engine.Render.Contracts` 子层并保留兼容类型窗口。
