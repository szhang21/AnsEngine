# Engine.Scripting 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Scripting`
- 版本：`v0.1`
- 负责人：`待指定`
- 生效日期：`2026-05-02`
- 关联任务卡：`TASK-SCRIPT-001`

## 2) 目标与范围

- 模块目标：为引擎提供最小可验证的内置 C# script runtime，负责 script 注册、绑定、生命周期调度和受限的自身对象访问抽象。
- 适用范围：script registry、script runtime、script instance lifecycle、script property binding、self-object/transform 访问抽象消费。
- 非适用范围：外部 DLL 加载、源码编译、热重载、sandbox、安全隔离、Editor Script UI、跨对象查询、渲染实现细节。

## 3) 职责（Responsibilities）

- 负责注册内置 `scriptId -> factory` 映射并拒绝重复注册。
- 负责按 scene 中的 Script component 顺序创建、绑定、初始化和逐帧更新 script instance。
- 负责把 `scriptId + properties` 绑定为脚本可消费的上下文与显式失败结果。
- 负责通过宿主提供的 self-object 抽象访问绑定对象自身 Transform。

## 4) 非职责（Non-Responsibilities）

- 不负责 Scene runtime object/component 的所有权与内部集合（归属 `Engine.Scene`）。
- 不负责 Scene JSON 解析、schema 校验和 normalized component descriptions（归属 `Engine.SceneData`）。
- 不负责应用主循环、窗口、输入与运行模式编排（归属 `Engine.App`）。
- 不负责 OpenGL、Render contract 或 GPU 资源。
- 不负责 Editor Script component 编辑 UI。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Core`
  - `Engine.Contracts`
- 可使用基础库/第三方：
  - `.NET` 标准库

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.Scene`
  - `Engine.Render`
  - `Engine.Editor`
  - `Engine.Editor.App`
  - `Engine.Asset`
- 禁止跨层调用模式：
  - 在 `Engine.Scripting` 内直接读取 scene JSON 或文件 IO
  - 在 `Engine.Scripting` 内编译/加载外部程序集
  - 在 `Engine.Scripting` 内访问任意非自身对象 runtime state

## 7) 公开接口（Public Interfaces）

- `IScriptBehavior`
  - 用途：定义 script instance 的最小生命周期
  - 输入/输出：`Initialize(ScriptContext)` 与 `Update(ScriptContext)`
  - 错误语义：实现抛异常时由 script runtime 转为显式失败并中止运行
  - 生命周期约束：每个 Script component 对应一个独立实例

- `ScriptRegistry`
  - 用途：注册和创建内置脚本实例
  - 输入/输出：输入 `scriptId` 与 factory，输出 script instance 或显式失败
  - 错误语义：重复注册、缺失 scriptId、创建失败均需可诊断
  - 生命周期约束：通常在 bootstrap 时构建，运行期只读消费

- `ScriptRuntime`
  - 用途：绑定 scene 中的 Script component 并驱动 Initialize/Update
  - 输入/输出：输入场景脚本描述、Scene bridge、update context；输出绑定结果与更新结果
  - 错误语义：missing script、bad property、script exception 均 fail fast
  - 生命周期约束：随 app runtime 创建与释放

- `IScriptSelfObject` / `IScriptTransformComponent`
  - 用途：定义 script 可消费的自身对象与 Transform 访问面
  - 输入/输出：通过 `Self.Transform` 读取/写入自身对象 local transform
  - 错误语义：不提供跨对象查询或任意组件访问
  - 生命周期约束：由宿主在绑定 Script component 时提供

## 8) 数据与状态边界

- 模块内部可变状态：registry 条目、已绑定的 script instances、绑定顺序和每实例上下文。
- 外部可观察状态：显式 binding/update 结果、diagnostic failure、脚本驱动后的自身 transform 变化。
- 线程模型与并发约束：默认主线程初始化与更新；不提供并发 script execution。
- 资源生命周期：`Register -> Bind -> Initialize -> Update* -> Dispose/Shutdown`。

## 9) 质量门禁与验收

- Build 验收：`dotnet build AnsEngine.sln` 通过。
- Test 验收：registry、binding、lifecycle、property binding 与 failure 语义测试通过。
- Smoke 验收：内置 `RotateSelf` script 可在有效 scene 中驱动对象 rotation。
- Perf 验收：脚本更新不引入逐帧 JSON 解析、外部程序集扫描或全场景对象查询。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-05-03
  - 变更人：Execution-Agent
  - 变更内容：将脚本访问面从 `IScriptSelfTransform` 收敛为 `IScriptSelfObject` / `IScriptTransformComponent`，并移除 `Engine.Scripting` 对 `Engine.Scene` 的项目级依赖；`Engine.Scripting` 只保留对 `Engine.Contracts` 中 `SceneTransform` 的契约消费。
  - 变更原因：支撑 `TASK-SCRIPT-002`，让 Transform 继续归属 Scene 原生 runtime component，同时让 Scripting 仅依赖抽象 self-object 访问面。
  - 风险与回滚方案：若后续需要更多组件访问，必须另立任务设计组件查询边界；不得通过重新引入 `Engine.Scripting -> Engine.Scene` 依赖绕过 App/Scene 边界适配。
- 2026-05-02
  - 变更人：Execution-Agent
  - 变更内容：落地 `Engine.Scripting` 与 `Engine.Scripting.Tests` 工程，新增 `IScriptBehavior`、`ScriptContext`、`ScriptRegistry`、`ScriptRuntime`、显式 binding/update failure 结果与窄 `IScriptSelfTransform` 自身 Transform 访问面。
  - 变更原因：支撑 `TASK-SCRIPT-001`，建立 M17 scripting foundation 的内置 registry、script instance lifecycle 和 fail-fast 诊断语义。
  - 风险与回滚方案：当前不包含 SceneData Script schema、Scene bridge、App sample 或外部脚本加载；后续扩展必须保持 `Engine.Scripting -> Engine.Scene` 单向依赖，不允许 `Engine.Scene -> Engine.Scripting`。
- 2026-05-02
  - 变更人：Dispatch-Agent
  - 变更内容：建立 `Engine.Scripting` 初版边界合同种子，预留 M17 scripting foundation 的 registry、runtime、lifecycle 和 self-transform bridge 消费边界。
  - 变更原因：支撑 `TASK-SCRIPT-001`，在进入实现前先明确 `Engine.Scripting -> Engine.Scene` 的依赖方向与“仅内置注册表、无外部 DLL/源码编译”的硬约束。
  - 风险与回滚方案：若后续脚本能力扩展到外部程序集、热重载或跨对象访问，必须另立边界变更任务；当前不允许在此合同内静默扩张。
