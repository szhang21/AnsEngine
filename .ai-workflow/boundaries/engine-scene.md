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
  - `Engine.SceneData`（规范化场景描述输入）
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

- 2026-05-02
  - 变更人：Execution-Agent
  - 变更内容：新增 Scene script self-transform bridge：`SceneGraphService.BindScriptObject` 返回绑定到单个 runtime object 的 `SceneScriptObjectHandle`，只暴露 object id/name 与自身 local transform get/set；移除 M15 默认旋转 smoke behavior。
  - 变更原因：支撑 `TASK-SCENE-019`，让 M17 scripting runtime 后续可通过窄 Scene bridge 修改绑定对象自身 Transform，同时保持 `Engine.Scene` 不引用 `Engine.Scripting`。
  - 风险与回滚方案：当前不实现 script registry/runtime 或 App 执行，只提供 Scene 侧访问桥；若后续需要脚本绑定顺序或 runtime 调度，应在 `Engine.Scripting`/`Engine.App` 侧接线，不把 `Engine.Scripting` 依赖引入 Scene。
- 2026-05-02
  - 变更人：Execution-Agent
  - 变更内容：`RuntimeScene.LoadFromDescription(...)` 改为消费 SceneData normalized component descriptions 构建 runtime transform/mesh renderer components；Transform-only object 会进入 runtime snapshot 且 `HasMeshRenderer=false`，但不会进入 render frame。
  - 变更原因：支撑 `TASK-SCENE-018`，把 M16 component descriptions 桥接到 Scene runtime object/component model，同时保持 Render 只消费 `SceneRenderFrame`。
  - 风险与回滚方案：bridge 不读取 JSON/file DTO，不改变 M15 update 选择规则；若后续组件类型扩展，应继续在 Scene 内部做 normalized description 到 runtime component 的映射，不让 Render 感知 schema。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：`RuntimeSceneSnapshot` 新增只读诊断字段 `UpdateFrameCount` 与 `AccumulatedUpdateSeconds`，并补充 Scene/App/Render 边界测试，确认 update context 与 runtime scene/object/component 类型不泄露到 Render，App 仍通过 runtime abstraction 驱动 update。
  - 变更原因：支撑 `TASK-SCENE-017`，让 M15 runtime update 状态可被 snapshot 只读观察，同时补齐跨模块边界证据。
  - 风险与回滚方案：snapshot 诊断字段只读且不改 render contract；若后续需要更多诊断，应继续通过 Scene snapshot 增量扩展，不要求 Render 感知 update 统计。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：`RuntimeScene.Update(...)` 新增最小默认旋转 smoke behavior：每次 update 只选择第一个同时具备 transform 与 mesh renderer 的 runtime object，按传入 `DeltaSeconds` 绕 Y 轴推进 local rotation，并保留 position、scale、mesh/material 与 camera 语义不变。
  - 变更原因：支撑 `TASK-SCENE-016`，证明 M15 update pipeline 能推进 runtime state，且 `BuildRenderFrame()` 与 snapshot 只读取 update 后状态。
  - 风险与回滚方案：该行为仅作为 smoke behavior，不引入 animation/script/system 抽象；若后续正式动画系统落地，可将该最小行为替换为受控 update system，同时保留 update/render 分离边界。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：新增 `SceneUpdateContext`、`SceneGraphService.UpdateRuntime(...)` 与 `RuntimeScene.Update(...)` 统计地基；runtime update 统计包含 `UpdateFrameCount` 与 `AccumulatedUpdateSeconds`，并在 `Clear()` / `LoadFromDescription(...)` 时重置。
  - 变更原因：支撑 `TASK-SCENE-015`，为 M15 runtime update pipeline 建立 Scene 自有 update 入口和可诊断统计状态。
  - 风险与回滚方案：当前只做记账和参数校验，不引入 Platform/App/Render 依赖、不推进 transform 行为；若后续 update 行为异常，可回退 `RuntimeScene.Update(...)` 内部实现而保持 `SceneUpdateContext` 边界入口稳定。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：回流修复 `TASK-SCENE-013`，移除 `SceneGraphService` 中的 legacy render item list 与逐帧 demo frame generator；`AddRootNode` 改为直接写入 `RuntimeScene` runtime object/components，`BuildRenderFrame` 无条件从 `RuntimeScene` 生成 `SceneRenderFrame`。
  - 变更原因：修复 M14 Review 发现的 `Architecture/LegacyPath` 缺陷，确保 `RuntimeScene` 是 render frame 的单一 runtime state source。
  - 风险与回滚方案：App 回归测试同步改为验证稳定 runtime state 而非旧 demo 动画；若后续需要动画或 runtime update，应在后续 update pipeline 任务中实现，不回退为 `BuildRenderFrame` 内部逐帧造数据。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：完成 M14 QA 复验，确认 runtime object/component model、SceneDescription 映射、SceneRenderFrame 输出与 runtime snapshot/query 均留在 `Engine.Scene` 边界内；`Engine.Render` 仍只消费 `Engine.Contracts.SceneRenderFrame`，`Engine.SceneData` 不感知 runtime object/component。
  - 变更原因：支撑 `TASK-QA-015`，为 M14 runtime object model 收口提供边界证据。
  - 风险与回滚方案：当前未发现 MustFix；若人工复验发现 runtime component 泄露到 App/Render/Editor，应打回对应 Scene 任务而非在 QA 卡中补实现。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：`SceneGraphService` 新增 `CreateRuntimeSnapshot()` 与 `FindObject(string objectId)` 只读查询面，`RuntimeScene` 输出 `RuntimeSceneSnapshot`、`SceneRuntimeObjectSnapshot` 与 `SceneCameraRuntimeSnapshot` 值对象，不暴露内部 runtime object/component 集合。
  - 变更原因：支撑 `TASK-SCENE-014`，为测试和后续系统提供稳定可观察面，同时保持 runtime state 仍归属 `Engine.Scene` 内部。
  - 风险与回滚方案：snapshot 仅含只读值与契约引用，不引入 Render/SceneData 反向依赖；若后续查询字段扩展，继续通过 snapshot 增量扩展，不开放内部可变集合。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：`SceneGraphService.BuildRenderFrame` 在 scene description 路径下改为从 `RuntimeScene` 的 runtime objects/components 输出 `SceneRenderFrame`，mesh/material/transform 来自 runtime components，camera 来自 runtime camera state。
  - 变更原因：支撑 `TASK-SCENE-013`，完成 M14 内部状态源迁移，同时保持 `Engine.Contracts.SceneRenderFrame` 对外 contract 不变。
  - 风险与回滚方案：legacy `AddRootNode` demo path 已在回流修复中移除；若后续 snapshot 查询异常，不需要改 Render/App contract。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：`RuntimeScene` 新增 `LoadFromDescription`，将 `SceneDescription` 映射为 runtime objects、transform/mesh renderer components 与 camera state；`SceneGraphService.LoadSceneDescription` 改为以 runtime scene 为主状态源。
  - 变更原因：支撑 `TASK-SCENE-012`，把 SceneData 输入先落到 Engine.Scene runtime model，再由后续任务输出 render frame。
  - 风险与回滚方案：当前 `mRenderItems` 仍作为 013 前的兼容输出缓存保留；若后续 render 输出迁移失败，可回退 `BuildRenderFrame` 而不改变 SceneData schema 或跨模块依赖。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：新增 `SceneTransformComponent`、`SceneMeshRendererComponent` 与 `SceneCameraRuntimeState`，支持从 `SceneData` description 映射到 runtime component/camera state，并复用 camera runtime state 生成 `SceneCamera`。
  - 变更原因：支撑 `TASK-SCENE-011`，让 M14 runtime object model 从 identity 壳推进到 transform、mesh/material 与 camera 最小运行时语义。
  - 风险与回滚方案：当前不实现 world transform、层级、资源加载或 camera-on-object；若后续映射调整，应保持组件仍仅依赖 Contracts/SceneData，不引入 Render/Asset/App。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：新增 `SceneRuntimeObject` 与 `RuntimeScene` 内部 runtime 基础模型，`SceneGraphService` 持有 runtime scene owner，并让 `NodeCount` 从 runtime object count 返回。
  - 变更原因：支撑 `TASK-SCENE-010`，为 M14 轻量 GameObject/Component 风格 runtime object model 建立稳定落点。
  - 风险与回滚方案：当前仅承载 object identity 与 object count，不实现组件、层级、update loop 或 render 输出重写；如后续组件映射异常，可回退到 runtime scene 内部实现，不扩散到 Render/App/Editor。
- 2026-04-26
  - 变更人：Execution-Agent
  - 变更内容：新增 `Engine.Scene -> Engine.SceneData` 允许依赖，并在 `SceneGraphService` 中接入 `SceneDescription` 到运行时渲染帧的初始化入口。
  - 变更原因：支撑 `TASK-SCENE-009`，让 Scene 从 M10 起消费规范化场景描述，而不是继续依赖硬编码对象与相机。
  - 风险与回滚方案：若后续 `SceneDescription` 扩展层级或更多场景语义，继续由 Scene 在运行时内部映射，不回退为 Scene 直接解析 JSON 或文件路径。
- 2026-04-24
  - 变更人：Exec-Scene
  - 变更内容：`SceneGraphService` 默认输出真实 `mesh://cube` 引用，并在多对象路径保留 `mesh://missing` 供下游 fallback 验证；补充共享 mesh 与缺失 mesh 引用测试，明确 Scene 仅持稳定 `meshId`，不持有磁盘路径或导入器细节。
  - 变更原因：支撑 `TASK-SCENE-008`，让 Scene 为 M9 真实磁盘 mesh 主链路输出可复用、可回退的资源引用语义。
  - 风险与回滚方案：若后续样例 mesh 集合扩展，继续在 Scene 侧维护语义化 `meshId` 集合；不回退为裸文件路径或导入器语义泄漏。

- 2026-04-18
  - 变更人：Exec-Scene
  - 变更内容：`SceneGraphService` 资源输出新增候选值解析与回退规则（`meshId/materialId`），并统一通过 `SceneMeshRef/SceneMaterialRef` 结构化构造输出；多对象路径下缺失 mesh/material 均在 Scene 侧回退。
  - 变更原因：支撑 `TASK-SCENE-007`，确保 Scene 输出的资源引用与 Render M7 资源入口保持一致且可预测。
  - 风险与回滚方案：若 Render 后续扩展资源入口集合，需同步 Scene 支持集合；短期可将新增资源先回退到默认值，避免链路中断。

- 2026-04-17
  - 变更人：Exec-Scene
  - 变更内容：`SceneGraphService` 在 `SceneRenderFrame` 输出中新增真实 `SceneCamera(View/Projection)` 语义，并引入轻量相机轨道变化（连续帧 `View` 变化、`Projection` 稳定）。
  - 变更原因：支撑 `TASK-SCENE-006`，让 Scene 侧不仅输出对象 transform，也提供可被 Render 真实消费的相机参数。
  - 风险与回滚方案：若后续需要固定镜头回归，可将相机轨道动态收敛为固定 View；保留 Camera 字段不回退契约结构。

- 2026-04-15
  - 变更人：Exec-Scene
  - 变更内容：`SceneGraphService` 输出项补充 `SceneTransform`（Position/Scale/Rotation）并在连续帧提供轻量动态 transform；首帧保持 identity 兼容。
  - 变更原因：支撑 `TASK-SCENE-005`，让 Scene 侧稳定产出 M5 变换契约输入。
  - 风险与回滚方案：若下游暂未消费 transform，可继续按 identity 路径运行；必要时仅关闭动态量，不回退契约输出字段。

- 2026-04-14
  - 变更人：Exec-App
  - 变更内容：App 组合根通过 `SceneRuntimeAdapter` 以 `ISceneRuntime` 抽象消费 Scene 运行时能力，Scene 侧保持 `SceneGraphService` 作为具体实现提供方。
  - 变更原因：支撑 `TASK-APP-005`，将主循环调度与场景具体实现解耦。
  - 风险与回滚方案：若后续抽象能力不足，再扩展 `ISceneRuntime` 接口，不回退为 App 直接依赖 Scene 具体类型。

- 2026-04-14
  - 变更人：Exec-Scene
  - 变更内容：`SceneGraphService` 渲染输出收敛为单一 `Engine.Contracts` 契约出口，删除 `SceneRenderContracts.cs` 双轨兼容层与 `FromContracts` 转换路径。
  - 变更原因：支撑 `TASK-SCENE-004`，消除热路径每帧双轨转换分配与语义分叉风险。
  - 风险与回滚方案：若下游仍存在旧类型调用，短期通过适配器桥接，不恢复 Scene 内部契约副本。

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
