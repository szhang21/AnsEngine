# Engine.Runtime 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Runtime`
- 版本：`v0.1`
- 负责人：`待指定`
- 生效日期：`2026-05-13`
- 关联任务卡：`TASK-RUNTIME-001`

## 2) 目标与范围

- 模块目标：集中承载运行时 session 初始化、runtime tick pipeline、script update component traversal 与 physics writeback orchestration。
- 适用范围：`SceneDescription` 到 runtime session、script component binding consumption、runtime update component dispatch、physics world creation、physics resolved transform writeback、render 前状态收口。
- 非适用范围：window lifecycle、platform input polling、renderer initialize/shutdown/present、asset warmup、Editor integration、external script loading、hot reload、animation/audio/signal/fixed update systems。

## 3) 职责（Responsibilities）

- 提供 `EngineRuntimeSession.Initialize(SceneDescription)` 和 `Tick(RuntimeTickContext)` public entry。
- 拥有 first pipeline：Scene base update/statistics -> runtime update components -> physics writeback -> scene state ready for render。
- 使用 `Engine.Scripting` 的 registry/binding/update component 输出，不复制 scripting factory/property diagnostics。
- 从 `SceneData` physics components 创建 `PhysicsWorld`，并将 resolved dynamic body transform 写回 `Engine.Scene` runtime state。
- 在 script 或 physics failure 时返回 deterministic failure before render。

## 4) 非职责（Non-Responsibilities）

- 不负责 renderer、window、present、asset bootstrap、platform input polling 或 App exit policy。
- 不解析 platform input；调用方必须传入 `RuntimeInputSnapshot`。
- 不实现 dynamic physics solver redesign。
- 不实现 external DLL loading、source compilation、hot reload、Editor Script UI、cross-object query、signal/event、animation、audio 或 fixed update API。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Contracts`
  - `Engine.Core`
  - `Engine.Scene`
  - `Engine.SceneData`
  - `Engine.Scripting`
  - `Engine.Physics`
  - `Engine.Runtime.Abstractions`
- 可使用基础库/第三方：
  - `.NET` 标准库

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.App`
  - `Engine.Platform`
  - `Engine.Render`
  - `Engine.Editor`
  - `Engine.Editor.App`
  - `Engine.Asset`
- 禁止跨层调用模式：
  - 在 Runtime 中持有 renderer/window/present lifecycle。
  - 在 Runtime 中读取 keyboard/platform input。
  - 在 Runtime 中复制 Scripting registry/factory/property binding 诊断逻辑。

## 7) 公开接口（Public Interfaces）

- `EngineRuntimeSession`
  - `Initialize(SceneDescription sceneDescription)`
  - `Tick(RuntimeTickContext context)`
  - `CreateRuntimeSnapshot()`
  - `SceneRenderProvider`
- `RuntimeTickContext`
  - `DeltaSeconds`
  - `TotalSeconds`
  - `RuntimeInputSnapshot Input`
- `RuntimeInitializationResult` / `RuntimeTickResult`
  - success/failure result shape with deterministic `RuntimeFailure`.

## 8) 数据与状态边界

- 模块内部可变状态：Scene runtime owner、PhysicsWorld、bound runtime update components。
- 外部可观察状态：runtime snapshot and render provider after successful initialization/tick.
- 线程模型与并发约束：默认主线程 tick；本模块不定义 multithreaded scheduler。
- 资源生命周期：不拥有 GPU/window/assets; only runtime scene/script/physics session state.

## 9) 质量门禁与验收

- Build 验收：`dotnet build AnsEngine.sln --nologo -v minimal` 通过。
- Test 验收：`dotnet test tests/Engine.Runtime.Tests/Engine.Runtime.Tests.csproj --no-restore --nologo -v minimal` 通过。
- Smoke 验收：headless Runtime tick proves script update -> physics writeback -> scene state ready before render.
- Perf 验收：runtime tick does not rebuild PhysicsWorld each frame and does not add broad scene/global query beyond current MVP writeback.
- 边界验证项：
  - [ ] `Engine.Runtime` 不引用 `Engine.App` / `Engine.Platform` / `Engine.Render` / `Engine.Editor` / `Engine.Editor.App` / `Engine.Asset`
  - [ ] App host/window/render/present remain outside Runtime
  - [ ] Scripting owns registry/factory/property binding diagnostics

## 10) 变更记录（Boundary Change Log）

- 2026-05-13
  - 变更人：Execution-Agent
  - 变更内容：M24 QA gate 复验确认 `Engine.Runtime` 仍是 runtime tick pipeline 唯一调度模块，负责 Scene base update、runtime update components、physics writeback 和 render 前 deterministic failure；未引用 App/Platform/Render/Editor/Asset。
  - 变更原因：支撑 `TASK-QA-025`，汇总 M24 runtime component lifecycle gate evidence。
  - 风险与回滚方案：当前 MustFixCount=0；后续若新增 fixed update、hot reload、cross-object query 或 render ownership，必须另立计划并更新边界。
- 2026-05-13
  - 变更人：Execution-Agent
  - 变更内容：新增 `Engine.Runtime` 边界合同，定义 runtime session/tick API、允许依赖、禁止依赖、script/physics orchestration 职责和 render 前 failure 收口语义。
  - 变更原因：支撑 `TASK-RUNTIME-001` 与 M24 Engine.Runtime module and tick pipeline。
  - 风险与回滚方案：新模块不接管 App host、Render、Platform 或 Asset；如 runtime session orchestration 出现回归，可回退 `Engine.Runtime` 项目和 tests，不影响已有 App 主路径，直到 `TASK-APP-023` 接线。
