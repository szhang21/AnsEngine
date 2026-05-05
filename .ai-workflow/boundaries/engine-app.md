# Engine.App 模块边界合同

## 1) 模块信息

- 模块名：`Engine.App`
- 版本：`v1.0`
- 负责人：`待指定`
- 生效日期：`2026-04-04`
- 关联任务卡：`待关联`

## 2) 目标与范围

- 模块目标：作为应用组合根，负责模块初始化顺序、运行主循环编排与生命周期收口。
- 适用范围：依赖注入/装配、系统启动与关闭流程、主循环驱动、运行模式配置。
- 非适用范围：渲染实现细节、场景内部数据结构实现、资源导入细节、平台底层接口实现。

## 3) 职责（Responsibilities）

- 负责核心模块创建与依赖装配。
- 负责系统启动顺序与关闭顺序管理。
- 负责主循环阶段编排（输入/更新/渲染）。
- 负责 runtime physics orchestration：从 Scene 读取候选 Transform、调用 Physics resolve、将 resolved Transform 写回 Scene。
- 负责应用级配置加载与运行模式切换。

## 4) 非职责（Non-Responsibilities）

- 不负责 OpenGL 调用与绘制细节（归属 `Engine.Render`）。
- 不负责实体与场景图内部逻辑（归属 `Engine.Scene`）。
- 不负责资源解析与缓存内部机制（归属 `Engine.Asset`）。
- 不负责窗口/输入底层适配（归属 `Engine.Platform`）。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Core`
  - `Engine.Platform`
  - `Engine.Scene`
  - `Engine.SceneData`
  - `Engine.Scripting`
  - `Engine.Physics`（仅由 App 作为 composition root 持有 production bridge / world initialization / runtime orchestration）
  - `Engine.Asset`
  - `Engine.Render`
- 可使用基础库/第三方：
  - `.NET` 标准库

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - 各子模块内部私有实现细节（仅依赖公开接口）
- 禁止跨层调用模式：
  - 在 `Engine.App` 中写入底层 OpenGL 操作
  - 在 `Engine.App` 中承载场景/资源内部业务逻辑

## 7) 公开接口（Public Interfaces）

- `IApplication`
  - 用途：统一应用启动、运行、关闭入口
  - 输入/输出：输入启动配置，输出运行状态与退出码
  - 错误语义：启动失败抛异常并记录关键日志
  - 生命周期约束：单实例运行；运行态持续轮询窗口事件并消费关闭信号；关闭后不可复用

- `IRuntimeBootstrap`
  - 用途：模块依赖装配与初始化编排
  - 输入/输出：输入配置与服务注册表，输出可运行应用实例
  - 错误语义：装配失败需可诊断
  - 生命周期约束：启动阶段执行一次

## 8) 数据与状态边界

- 模块内部可变状态：运行模式、系统注册表、主循环控制标记。
- 外部可观察状态：应用运行状态、启动日志、退出原因。
- 线程模型与并发约束：主循环在主线程；后台任务通过受控调度接口接入。
- 资源生命周期：统一收口关闭各模块，保证逆序释放。

## 9) 质量门禁与验收

- Build 验收：`dotnet build -c Debug/Release` 通过。
- Test 验收：应用启动/关闭流程与装配测试通过。
- Smoke 验收：应用可完整启动、进入主循环并正常退出。
- Perf 验收：主循环编排开销可控，无明显调度抖动。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-05-05
  - 变更人：Execution-Agent
  - 变更内容：完成 M20 QA 复验，确认 App 作为唯一 production bridge / runtime orchestrator 持有 `Engine.Physics` 依赖；全量 build/test/headless smoke 通过，`SceneData load -> App bridge -> Script update -> Physics resolve/writeback -> Render` 主链路有测试与 smoke 证据。
  - 变更原因：支撑 `TASK-QA-021`，确认 M20 Physics Runtime Collision MVP 可归档准备，且 App 没有引入逐帧 world 重建、Scene 内部集合直连、Dynamic gravity/solver 或 Editor UI。
  - 风险与回滚方案：当前无 MustFix；后续若扩展 dynamic simulation、Play Mode 或 Editor physics UI，必须另立 M21+ 任务并重新更新 App/Physics/Editor 边界。
- 2026-05-05
  - 变更人：Execution-Agent
  - 变更内容：允许 `Engine.App -> Engine.Physics` 依赖；新增 App-owned production bridge，将 `SceneDescription` 中具备 Transform + RigidBody + BoxCollider 的对象映射为 `PhysicsWorldDefinition`，并在 `ApplicationHost.Run()` 初始化阶段创建 `PhysicsWorld`。
  - 变更原因：支撑 `TASK-APP-020`，把 M19 test-only SceneData adapter 升级为生产主路径，同时保持 `Engine.Physics` 零 Engine 模块依赖，bridge 归属 App composition root。
  - 风险与回滚方案：当前仅做初始化桥接，不做逐帧 resolve、Scene writeback、gravity 或 solver；若后续初始化失败，可回退 `ScenePhysicsWorldDefinitionBridge` 与 App 初始化接线，不影响 SceneData schema 或 Physics core public shape。
- 2026-05-05
  - 变更人：Execution-Agent
  - 变更内容：新增 App-owned `RuntimePhysicsOrchestrator`，主循环在 `ScriptRuntime.Update(...)` 后、`RenderFrame()` 前执行 `PhysicsWorld.ResolveKinematicMove(...)` 并通过 `ISceneRuntime.TrySetObjectTransform(...)` 写回 Scene；`ISceneRuntime` 显式暴露 runtime snapshot 与 transform writeback bridge。
  - 变更原因：支撑 `TASK-APP-021`，落实 `SceneRuntime.Update -> ScriptRuntime.Update -> Physics resolve/writeback -> Render` 顺序，让 Render 观察到 Physics 约束后的最终 Transform。
  - 风险与回滚方案：当前不做逐帧重建 world、gravity、solver、Editor UI 或 Scene/Physics 互相直连；若 writeback/resolve 异常，App 以 deterministic failure 停止本帧运行并报告。
- 2026-05-03
  - 变更人：Execution-Agent
  - 变更内容：`RuntimeBootstrap.Build()` 按 `ANS_ENGINE_USE_NATIVE_WINDOW` 装配输入服务：native window path 使用 `NativeWindowInputService`，headless path 继续使用 `NullInputService`。
  - 变更原因：支撑 `TASK-PLAT-003`，补齐 M18 真实窗口运行路径的 W/A/S/D 输入采集接线，让默认 scene 中的 `MoveOnInput` 可接收 Platform 真实输入，同时保持 App 只负责组合根分流与 Platform-to-Scripting 转换。
  - 风险与回滚方案：若 native window smoke 在无图形环境不可验证，保留 headless 空输入路径作为 CI 稳定路径；若未来扩展输入系统，应继续由 Platform 封装底层输入、App 只做运行模式装配与类型转换。
- 2026-05-03
  - 变更人：Execution-Agent
  - 变更内容：App 主循环新增 `InputSnapshot -> ScriptInputSnapshot` 转换并传入 `ScriptRuntime.Update(...)`；组合根注册内置 `MoveOnInput`；默认样例场景新增 `MoveOnInput` Script component；`Engine.App` 项目设置 `UseAppHost=false` 以避免当前 macOS 环境 apphost code signing 阻塞验收。
  - 变更原因：支撑 `TASK-APP-012`，打通 M18 `W/A/S/D -> script -> Transform.Position` 的最小交互主链路，同时保持 Platform/Scripting 输入转换唯一归属 App。
  - 风险与回滚方案：若未来恢复 native apphost，应先解决本机 code signing 环境问题；若扩展输入系统，继续在 App 层完成 Platform 到 Scripting 的边界转换，不把 Platform 类型泄露进 Scripting。
- 2026-05-03
  - 变更人：Execution-Agent
  - 变更内容：App scripting adapter 从单一 `SceneScriptSelfTransform` 调整为 `SceneScriptSelfObject` + `SceneScriptTransformComponent`，由组合根把 `SceneScriptObjectHandle` 包装为 `Engine.Scripting` 的 self-object/transform 抽象。
  - 变更原因：支撑 `TASK-SCRIPT-002`，移除 `Engine.Scripting` 对 `Engine.Scene` 的直接依赖，同时保持 App 负责 Scene 与 Scripting 的边界桥接。
  - 风险与回滚方案：若后续脚本访问面扩展到更多组件，应继续在 App/Scene 明确桥接，不把 Scene runtime 内部集合泄露给 Scripting。
- 2026-05-02
  - 变更人：Execution-Agent
  - 变更内容：允许 `Engine.App` 直接依赖 `Engine.Scripting`，组合根新增 `ScriptRegistry` / `ScriptRuntime` 装配与内置 `RotateSelf` 注册；`ApplicationHost.Run()` 在 scene load 后绑定并 initialize Script components，并在每帧 `SceneRuntime.Update` 后、`RenderFrame` 前执行 script update。
  - 变更原因：支撑 `TASK-APP-011`，打通 M17 App scripting runtime integration and RotateSelf sample 主链路，同时保持脚本绑定与 update 编排归属 App。
  - 风险与回滚方案：若后续脚本加载扩展到外部程序集、源码编译或热重载，必须另开任务更新边界；如需回滚，本次变更可退回到 App 不注册 `ScriptRuntime`，但不得把 scripting runtime 反向塞入 `Engine.Scene`。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：`ISceneRuntime` 新增显式 update 入口，`ApplicationHost.Run()` 每帧按 ProcessEvents -> Input -> Time -> SceneRuntime.Update -> RenderFrame -> Present 顺序执行；`SceneRuntimeAdapter` 负责把 `TimeSnapshot` / `InputSnapshot` 翻译成 `SceneUpdateContext` 后转发给 `SceneGraphService.UpdateRuntime(...)`。
  - 变更原因：支撑 `TASK-APP-010`，打通 M15 App 主循环到 Scene runtime update 的主链路，同时保持 Scene 不直接依赖 Platform 类型。
  - 风险与回滚方案：当前不改变 loader failure 与 render failure 收口；若后续主循环阶段增加，应继续通过 App 层编排与 adapter 翻译，不把 Platform 类型泄露到 Scene 内部。
- 2026-04-26
  - 变更人：Execution-Agent
  - 变更内容：组合根新增 `Engine.SceneData` loader 装配、默认样例场景路径与 `ANS_ENGINE_SCENE_PATH` 覆盖入口；`ApplicationHost` 启动时先加载 `SceneDescription`，再初始化 Scene 运行时。
  - 变更原因：支撑 `TASK-APP-009`，让默认场景文件进入真实启动路径，并保持 App 只负责路径选择、loader 注入和错误收口。
  - 风险与回滚方案：若后续配置入口从环境变量扩展到配置文件或 CLI，继续由 App 负责来源解析与注入，不回退为 Scene/SceneData 自定位文件路径。
- 2026-04-24
  - 变更人：Exec-App
  - 变更内容：组合根新增 sample mesh 资源目录与 `DiskMeshAssetProvider` 装配；native 渲染路径显式注入 mesh provider，运行时在进入主循环前预热一次 bootstrap mesh 解析，保证 headless/真实窗口路径都走到真实磁盘 mesh 主链路。
  - 变更原因：支撑 `TASK-APP-008`，让 App 负责 provider/service 装配与样例运行路径接线，但不承载 OBJ 解析或 GPU 资源逻辑。
  - 风险与回滚方案：若后续样例资源入口改为外部配置文件或命令行参数，保持组合根负责路径解析与依赖注入，不回退到子模块内部自定位。

- 2026-04-17
  - 变更人：Exec-App
  - 变更内容：补充 M6 装配与生命周期校准测试：验证 App native 路径注入的 Scene provider 可产出相机语义（View 连续帧变化），并验证渲染异常路径仍完成 `RequestClose -> Shutdown -> Dispose` 收口。
  - 变更原因：支撑 `TASK-APP-007`，确保 M6 MVP 链路下 App 保持“仅装配不计算”职责且生命周期稳定。
  - 风险与回滚方案：若后续异常收口策略变化，统一在 `ApplicationHost` 测试集中调整断言，不在 App 中引入渲染/矩阵计算实现。

- 2026-04-15
  - 变更人：Exec-App
  - 变更内容：补充 M5 装配校准测试，验证 App 注入的 Scene 契约 provider 在初始化后可输出含 rotation 的连续帧 transform；主循环保持“仅装配不计算”边界。
  - 变更原因：支撑 `TASK-APP-006`，确保 M5 transform 链路在 App 组合根下可用且职责不越界。
  - 风险与回滚方案：若后续装配验证继续扩展，统一沉淀到组合根测试套件，不在 App 层引入 transform 计算逻辑。

- 2026-04-14
  - 变更人：Exec-App
  - 变更内容：`ApplicationHost` 从直接依赖 `SceneGraphService` 调整为依赖 `ISceneRuntime` 最小运行时接口；组合根新增 `SceneRuntimeAdapter` 绑定 Scene 实现。
  - 变更原因：支撑 `TASK-APP-005`，消除 App 运行主循环对 Scene 具体实现的硬编码耦合。
  - 风险与回滚方案：若后续出现跨模块初始化时序问题，可临时回滚到适配器内部兼容层，不恢复 `ApplicationHost` 对具体类型依赖。

- 2026-04-14
  - 变更人：Exec-App
  - 变更内容：组合根显式装配 `Engine.Contracts.ISceneRenderContractProvider` 并注入 `Engine.Render`，补充可测试的渲染器创建路径。
  - 变更原因：支撑 `TASK-APP-004`，确保 Render 仅消费契约且不感知 Scene 具体实现。
  - 风险与回滚方案：若后续装配链路进一步复杂化，可拆分 `RendererCompositionFactory` 并保持现有入口兼容。

- 2026-04-04
  - 变更人：初始化
  - 变更内容：创建 `Engine.App` 初版边界合同
  - 变更原因：明确组合根职责，防止系统逻辑散落
  - 风险与回滚方案：若编排职责膨胀，可拆分启动器与运行时管理器
- 2026-04-07
  - 变更人：Exec-App
  - 变更内容：补充应用主循环边界，明确事件泵驱动、关闭信号消费与统一资源收口要求
  - 变更原因：支撑 `TASK-APP-001` 最小持续主循环与退出编排落地
  - 风险与回滚方案：若主循环职责继续增长，拆分 LoopOrchestrator 保持组合根收敛
- 2026-04-07
  - 变更人：Exec-QA
  - 变更内容：补充 M2 验证口径，明确“可启动/可见/可退出”与 30-60 秒稳定运行证据要求
  - 变更原因：支撑 `TASK-QA-001` 可见反馈门禁证据补齐
  - 风险与回滚方案：若后续验证维度增加，拆分为独立 QA 运行手册并保留合同级最小门禁
- 2026-04-11
  - 变更人：Exec-App
  - 变更内容：明确应用运行收口需覆盖初始化失败路径，并补充无图形环境运行装配开关约束
  - 变更原因：支撑 `TASK-APP-002` 的 M3 运行装配与生命周期配套
  - 风险与回滚方案：若 headless 与真实窗口路径出现行为漂移，保留默认真实窗口装配并将验证脚本分流
- 2026-04-11
  - 变更人：Exec-QA
  - 变更内容：补充 M3 双轨验收口径，明确“图形可视证据 + 无图形环境运行证据”并行记录策略
  - 变更原因：支撑 `TASK-QA-002` 的门禁证据闭环与归档追溯
  - 风险与回滚方案：若环境口径差异扩大，按里程碑拆分“图形验收报告”与“CI 运行报告”
- 2026-04-11
  - 变更人：Exec-App
  - 变更内容：明确 M4 编排衔接要求：`Engine.App` 仅负责连接 `Scene -> Render` 契约流，不承载场景内部更新与渲染后端实现
  - 变更原因：支撑 `TASK-APP-003` 的提交流程编排配套
  - 风险与回滚方案：若后续阶段节点继续增多，拆分 `FramePipelineOrchestrator` 保持组合根简洁
- 2026-04-11
  - 变更人：Exec-QA
  - 变更内容：补充 M4 验证口径，明确默认采用“图形可视口径 + headless 稳定性口径 + Render 独立测试链路”三轨证据
  - 变更原因：支撑 `TASK-QA-003` 的门禁收敛与关单追溯
  - 风险与回滚方案：若后续环境口径差异扩大，拆分可视验收与CI验收报告并保持统一指标字段
