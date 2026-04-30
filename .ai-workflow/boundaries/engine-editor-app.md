# Engine.Editor.App 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Editor.App`
- 版本：`v0.1`
- 负责人：`待指定`
- 生效日期：`2026-04-30`
- 关联任务卡：`TASK-EAPP-001`

## 2) 目标与范围

- 模块目标：提供独立可执行的 GUI 编辑器宿主，将用户输入编排到 `Engine.Editor.SceneEditorSession`。
- 适用范围：窗口主循环、GUI 宿主、工具栏、Hierarchy、Inspector、状态栏、路径解析、用户操作到 editor session 的编排。
- 非适用范围：底层 JSON 编辑规则、SceneData normalizer 规则、运行时游戏入口、Render 主路径、Gizmo、Undo/Redo、资源浏览器、Prefab、热重载或 Play Mode。

## 3) 职责（Responsibilities）

- 负责创建并运行编辑器窗口。
- 负责装配 `SceneEditorSession` 并默认打开源码目录 sample scene。
- 负责承载 GUI 依赖，包括 OpenTK 与 ImGui.NET。
- 负责把 GUI 操作转发给 `SceneEditorSession`，并展示路径、dirty、selection 与 last error。

## 4) 非职责（Non-Responsibilities）

- 不负责 `SceneData` 文档 DTO、JSON loader/store、normalizer 或校验规则。
- 不负责 `Engine.App` 运行时启动路径、运行时主循环或 demo 场景装配。
- 不负责 `Engine.Editor` headless core 的窗口、输入、OpenTK 或 ImGui 依赖。
- 不负责资源浏览器、Prefab、Undo/Redo、Gizmo、Play Mode 或渲染器主路径改造。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Editor`
  - `Engine.SceneData`
  - `Engine.Contracts`
  - `Engine.Platform`
- 可使用基础库/第三方：
  - `.NET` 标准库
  - `OpenTK`
  - `ImGui.NET`

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.App`
  - `Engine.Render`
  - `Engine.Asset`
- 禁止跨层调用模式：
  - 绕过 `SceneEditorSession` 直接实现 scene JSON 编辑业务规则
  - 将 OpenTK、ImGui、窗口或输入依赖加入 `Engine.Editor`
  - 改写 `Engine.App` 默认启动路径或运行时主循环

## 7) 公开接口（Public Interfaces）

- `EditorAppController`
  - 用途：承载 GUI 操作到 `SceneEditorSession` 的编排入口。
  - 输入/输出：打开启动 scene、打开指定 scene、保存、另存为、选择、编辑、增删对象，失败时记录 `LastError`。
  - 错误语义：以 `SceneEditorSessionResult` 为真实来源，GUI 只展示最后失败信息。

- `EditorScenePathResolver`
  - 用途：解析启动 scene 路径。
  - 输入/输出：优先读取 `ANS_ENGINE_EDITOR_SCENE_PATH`，否则定位仓库源码 sample scene。
  - 错误语义：无法定位仓库根或 sample scene 时返回可诊断异常信息。

- `EditorAppWindow`
  - 用途：OpenTK 编辑器窗口宿主。
  - 生命周期约束：窗口拥有 GUI 主循环，不拥有 scene 文档业务规则。

## 8) 数据与状态边界

- 模块内部可变状态：GUI last error、输入缓冲、窗口生命周期、路径输入、临时 UI 状态。
- 外部可观察状态：当前 scene path、dirty、selection、last error 和 GUI 操作结果。
- 线程模型与并发约束：默认单窗口、单 session、单 UI 线程。
- 资源生命周期：`Program -> EditorAppController -> EditorAppWindow.Run -> Shutdown`。

## 9) 质量门禁与验收

- Build 验收：`dotnet build AnsEngine.sln` 通过。
- Test 验收：`Engine.Editor.App.Tests` 覆盖边界、路径解析与 GUI 编排状态。
- Smoke 验收：`dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj` 能启动并关闭最小编辑器窗口。
- Perf 验收：GUI 宿主不改变 `Engine.App` 主路径，不做逐帧文件 IO。
- 边界验证项（必须逐条通过）：
  - [ ] `Engine.Editor.App` 不引用 `Engine.App`、`Engine.Render`、`Engine.Asset`
  - [ ] `Engine.Editor` 不引用 OpenTK、ImGui 或 `Engine.Editor.App`
  - [ ] `Engine.App` 不引用 `Engine.Editor.App`

## 10) 变更记录（Boundary Change Log）

- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：Toolbar 的 Add Object 与 Remove Selected 接入 `EditorObjectWorkflowState`，默认对象工厂生成 `mesh://cube`、`material://default`、identity transform 的 `object-XXX` 对象。
  - 变更原因：支撑 `TASK-EAPP-006`，补齐最小 GUI 编辑器的对象增删工作流，并验证保存后 reload 仍保留增删结果。
  - 风险与回滚方案：id 生成从当前对象集合推导最小可用编号，不持久化外部状态；若后续需要模板库或资源选择器，应另立卡扩展。
- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：Toolbar 的 Open、Save、Save As 接入 `EditorFileWorkflowState` 与 `EditorAppController`，通过 `SceneEditorSession.Open/Save/SaveAs` 完成文件工作流。
  - 变更原因：支撑 `TASK-EAPP-005`，让 GUI 编辑形成文件闭环并支持 `ANS_ENGINE_EDITOR_SCENE_PATH` 覆盖默认 scene。
  - 风险与回滚方案：当前路径选择为文本输入，不引入原生文件对话框；若后续需要系统对话框，应在 `Engine.Editor.App` 内增强，不修改 `Engine.App` 或 `SceneData` 契约。
- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：Inspector 增加 Id、Name、Mesh、Material、Position、Rotation、Scale 输入缓冲与显式 Apply 提交，所有提交通过 `SceneEditorSession` 更新 API。
  - 变更原因：支撑 `TASK-EAPP-004`，让 GUI 能编辑选中对象并保持 dirty、selection、last error 与失败回滚语义。
  - 风险与回滚方案：当前不实现 Save/Open/Add/Remove；若后续发现字段提交需要更细粒度 UX，应继续在 `Engine.Editor.App` 内演进输入缓冲，不绕过 `SceneEditorSession`。
- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：Hierarchy 列表项点击接入 `EditorAppController.SelectObject`，选中高亮、Inspector 选中态和 Status Bar selected id 均从 session 当前状态生成。
  - 变更原因：支撑 `TASK-EAPP-003`，确保 GUI selection 不复制独立真相，成功选择不改变 dirty，失败保留原 selection 并显示 last error。
  - 风险与回滚方案：当前仅实现对象选择联动，不做 Inspector 字段提交或对象增删；若后续交互扩展，应继续通过 `SceneEditorSession` API 编排。
- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：补齐最小 ImGui/OpenGL 渲染后端，负责 ImGui context、字体纹理、shader、VAO/VBO/EBO、IO 输入状态和 draw data 提交。
  - 变更原因：修复 `TASK-EAPP-002` smoke 中 `ImGui.NewFrame()` 缺少后端资源导致的 native 崩溃，并满足 Toolbar/Hierarchy/Inspector/Status Bar 真实可见验收。
  - 风险与回滚方案：该渲染后端仅归属 `Engine.Editor.App`；若后续输入或多 viewport 需求增加，应继续局部演进，不把 OpenTK/ImGui 依赖引入 `Engine.Editor`。
- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：新增 GUI 快照与渲染分层，定义 Toolbar、Hierarchy、Inspector、Status Bar 的最小可测试状态模型。
  - 变更原因：支撑 `TASK-EAPP-002`，让基础布局与状态栏从 `SceneEditorSession` 读取路径、dirty、selection 与 last error，避免 GUI 复制独立真相。
  - 风险与回滚方案：当前按钮行为仍按占位错误处理；后续 Open/Save/Add/Remove 应继续在 `Engine.Editor.App` 内接入 session API，不修改 `Engine.Editor` headless 边界。
- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：新增 `Engine.Editor.App` 边界合同，定义独立 GUI 宿主职责、允许依赖、禁止依赖和最小公开接口。
  - 变更原因：支撑 `TASK-EAPP-001`，为 M13 最小 GUI 场景编辑器建立独立入口，避免污染 `Engine.Editor` headless core 或 `Engine.App` 运行时入口。
  - 风险与回滚方案：当前仅建立最小窗口宿主和 session 装配；若 OpenTK/ImGui 接线需要调整，应限制在 `Engine.Editor.App` 内回滚或替换，不修改 `Engine.Editor` 与 `Engine.App`。
