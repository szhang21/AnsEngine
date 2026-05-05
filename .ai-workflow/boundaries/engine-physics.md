# Engine.Physics 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Physics`
- 版本：`v0.1`
- 负责人：`待指定`
- 生效日期：`2026-05-03`
- 关联任务卡：`TASK-PHYS-001`

## 2) 目标与范围

- 模块目标：为引擎提供最小可验证的独立 physics core foundation，负责从 physics-owned definitions 构建 physics world、执行固定步进统计、输出只读 snapshot / query 结果，并提供 kinematic desired transform 的 static AABB 约束结果。
- 适用范围：physics world、physics-owned body/collider definitions、fixed-step statistics、AABB 计算、overlap / ground query、kinematic move resolve、显式失败语义。
- 非适用范围：Transform 回写、App 主循环调度、可见物理反馈、重力、dynamic solver、力/冲量/摩擦/反弹、Editor physics UI。

## 3) 职责（Responsibilities）

- 负责从 `PhysicsWorldDefinition` 中消费 body/collider/transform definitions 并构建 physics world。
- 负责执行固定步进记账与 world snapshot 生成。
- 负责输出 AABB、overlap query、ground query 等只读查询结果。
- 负责对 Dynamic body 的 kinematic desired transform 执行 X -> Y -> Z 单轴保守 static AABB 约束，并返回 resolved transform。
- 负责对 malformed physics input 提供可诊断的显式失败结果或被测试钉死的稳定异常语义。

## 4) 非职责（Non-Responsibilities）

- 不负责 Scene runtime object/component 所有权与 Transform 更新（归属 `Engine.Scene`）。
- 不负责 Scene JSON 解析、schema 校验和文档 round-trip（归属 `Engine.SceneData`）。
- 不负责理解或直接消费 `SceneDescription`；SceneData 到 Physics definitions 的生产桥接属于后续 App/Scene integration。
- 不负责应用装配、输入、时间调度与运行模式（归属 `Engine.App`）。
- 不负责渲染、OpenGL、OpenTK、ImGui 或 GPU 资源。
- 不负责脚本、编辑器或 Play Mode。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - 无
- 可使用基础库/第三方：
  - `.NET` 标准库
  - `System.Numerics`

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.SceneData`
  - `Engine.Contracts`
  - `Engine.Core`
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
  - 输入/输出：输入 `PhysicsWorldDefinition`、`PhysicsStepContext` 与 kinematic desired transform，输出 world state、snapshot、query result 与 resolved transform
  - 错误语义：malformed physics input 必须可诊断
  - 生命周期约束：`Load -> Step* -> Snapshot/Query/ResolveKinematicMove`

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
- Smoke 验收：`PhysicsWorldDefinition -> PhysicsWorld -> Step -> Snapshot/Query` 主路径通过；测试可另用 test-only adapter 验证真实 SceneData fixture 能映射为 Physics definitions，但该 adapter 不属于 `Engine.Physics` 生产代码。
- Perf 验收：不引入逐帧 JSON 解析、App loop 绑定、Transform 回写或渲染 side path。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-05-05
  - 变更人：Execution-Agent
  - 变更内容：完成 M20 QA 复验，确认 `Engine.Physics` 生产代码仍无 ProjectReference/PackageReference，源码未引用 Scene/App/Render/Scripting/SceneData/Core/Contracts/Editor 等 Engine 模块；kinematic resolve 仅返回 resolved transform/result，不直接写回 Scene。
  - 变更原因：支撑 `TASK-QA-021`，确认 M20 Runtime Collision MVP 没有滑入 Engine 反向依赖、Dynamic gravity、velocity、force、impulse、solver、CCD、Editor UI 或渲染 side path。
  - 风险与回滚方案：当前无 MustFix；后续真实 dynamic simulation、solver 或更多 collider 类型必须另立任务并保持 Physics 独立输入/输出边界。
- 2026-05-05
  - 变更人：Execution-Agent
  - 变更内容：新增 `PhysicsWorld.ResolveKinematicMove(...)` 与 `PhysicsKinematicMoveResult`，按 X -> Y -> Z 单轴保守策略从 current transform 尝试 desired 位移；仅 static AABB 可阻挡 Dynamic body，命中时返回 first blocking body id，不移动 world snapshot。
  - 变更原因：支撑 `TASK-PHYS-003`，为 M20 Runtime Collision MVP 提供 Physics core 内的 deterministic kinematic collision resolve，同时保持 Physics 不依赖 Scene/App/SceneData/Contracts/Core。
  - 风险与回滚方案：当前不实现 gravity、velocity、force、impulse、solver、sweep、MTV、摩擦或反弹；若后续需要更真实物理行为，应在 M21+ 另立 solver/CCD 任务，不在本 MVP 中静默扩张。
- 2026-05-04
  - 变更人：Execution-Agent
  - 变更内容：完成 M19 Physics foundation QA 复验，确认全量 build/test 通过，`PhysicsWorldDefinition -> PhysicsWorld -> Step -> Snapshot/Query` 主路径有测试证据，SceneData fixture 仅通过 `tests/Engine.Physics.Tests/**` 中的 test-only adapter 映射，生产 `Engine.Physics` 保持零 Engine 模块依赖。
  - 变更原因：支撑 `TASK-QA-020`，为 M19 foundation 归档准备提供边界证据与质量结论。
  - 风险与回滚方案：当前未发现 MustFix；M19 不声明 visible runtime physics、Transform writeback、App fixed-step 调度、gravity 或 solver，相关能力应进入 M20。
- 2026-05-04
  - 变更人：Execution-Agent
  - 变更内容：实现 `PhysicsWorld.Load(PhysicsWorldDefinition)`、fixed-step 统计、只读 snapshot、AABB overlap query 与 ground query；AABB 按 position + collider center、positive size、absolute transform scale 计算并忽略 rotation；`Engine.Physics.Tests` 增加 test-only SceneData adapter 证据，但生产 `Engine.Physics` 仍不依赖 SceneData 或任何其他 Engine 模块。
  - 变更原因：支撑 `TASK-PHYS-002`，让 M19 Physics foundation 拥有独立 `PhysicsWorldDefinition -> PhysicsWorld -> Step -> Snapshot/Query` 主路径，同时不滑入 App loop、Scene Transform writeback、gravity 或 solver。
  - 风险与回滚方案：当前 query 只读且不修改 body state；若后续要做 runtime MVP、collision response 或 Transform writeback，必须在 M20 另立 integration 任务并更新边界。
- 2026-05-04
  - 变更人：Execution-Agent
  - 变更内容：新增 `Engine.Physics` / `Engine.Physics.Tests` 项目骨架与 solution 接线，落地 `PhysicsWorld`、`PhysicsWorldDefinition`、body/collider/transform definitions、step context、snapshot、AABB 与 query result 的最小公开形状，并补充边界测试确认 Physics core 无 Engine 模块、OpenTK、OpenGL 或 ImGui 依赖。
  - 变更原因：支撑 `TASK-PHYS-001`，为后续 `PhysicsWorldDefinition -> PhysicsWorld` 主路径提供独立 Physics core 落点，同时固定 M19 foundation 不接入 SceneData、Scene、App、Render、Scripting 或 Editor。
  - 风险与回滚方案：当前仅为 foundation public shape，不实现 runtime load/query 行为；如后续实现发现公开形状不足，应在 `TASK-PHYS-002` 内保持 Physics-owned definitions 输入，不回退为 SceneData 或 Scene runtime 依赖。
- 2026-05-03
  - 变更人：Dispatch-Agent
  - 变更内容：建立 `Engine.Physics` 初版边界合同种子，预留 M19 Physics foundation 的 world、fixed step、snapshot 与 query 能力边界，并明确 Physics core 不直接依赖任何其他 Engine 模块。
  - 变更原因：支撑 `TASK-PHYS-001`，在进入实现前先固定独立 Physics core 的依赖方向，以及“无 SceneData 直接消费、无 Transform 回写、无 App 接线、无渲染感知”的硬约束。
  - 风险与回滚方案：若后续需要 Runtime MVP、Scene Transform writeback 或 App fixed-step 调度，必须另立任务和边界变更；当前不允许在此合同内静默扩张。
