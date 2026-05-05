# Engine.Editor.App 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Editor.App`
- 版本：`v0.1`
- 负责人：`待指定`
- 生效日期：`2026-04-30`
- 关联任务卡：`TASK-EAPP-001`

## 2) 目标与范围

- 模块目标：提供独立可执行的 GUI 编辑器宿主，将用户输入编排到 `Engine.Editor.SceneEditorSession`，并承载 edit-time Scene View preview composition。
- 适用范围：窗口主循环、GUI 宿主、工具栏、Hierarchy、Scene View preview、component-group Inspector、状态栏、路径解析、用户操作到 editor session 的编排。
- 非适用范围：底层 JSON 编辑规则、SceneData normalizer 规则、运行时游戏入口、游戏运行时 Render 主路径、Gizmo、Undo/Redo、资源浏览器、Prefab、热重载或 Play Mode。

## 3) 职责（Responsibilities）

- 负责创建并运行编辑器窗口。
- 负责装配 `SceneEditorSession` 并默认打开源码目录 sample scene。
- 负责承载 GUI 依赖，包括 OpenTK 与 ImGui.NET。
- 负责把 GUI 操作转发给 `SceneEditorSession`，并展示路径、dirty、selection、component groups 与 last error。
- 负责在 edit-time preview host 中消费当前 normalized scene，经 `Engine.Scene` / `Engine.Render` / `Engine.Asset` 生成非空 Scene View 预览状态。

## 4) 非职责（Non-Responsibilities）

- 不负责 `SceneData` 文档 DTO、JSON loader/store、normalizer 或校验规则。
- 不负责 `Engine.App` 运行时启动路径、运行时主循环或 demo 场景装配。
- 不负责 `Engine.Editor` headless core 的窗口、输入、OpenTK 或 ImGui 依赖。
- 不负责资源浏览器、Prefab、Undo/Redo、Gizmo、Play Mode、script execution、physics simulate 或游戏运行时渲染器主路径改造。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Editor`
  - `Engine.SceneData`
  - `Engine.Contracts`
  - `Engine.Platform`
  - `Engine.Scene`
  - `Engine.Render`
  - `Engine.Asset`
- 可使用基础库/第三方：
  - `.NET` 标准库
  - `OpenTK`
  - `ImGui.NET`

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.App`
- 禁止跨层调用模式：
  - 绕过 `SceneEditorSession` 直接实现 scene JSON 编辑业务规则
  - 将 OpenTK、ImGui、窗口或输入依赖加入 `Engine.Editor`
  - 改写 `Engine.App` 默认启动路径或运行时主循环
  - 在 edit-time preview 中执行 script、physics、Play Mode 或调用 `ApplicationHost`

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

- `EditorScenePreviewHost`
  - 用途：Editor.App 内部 edit-time preview composition helper。
  - 输入/输出：输入 `SceneEditorSession.Scene`，输出 `EditorScenePreviewSnapshot`，通过 Scene/Render/Asset 现有契约构造非空预览状态。
  - 错误语义：无 scene 时输出 empty preview；合法 renderable scene 输出 nonblank preview snapshot。
  - 生命周期约束：只属于 Editor.App，不调用 `ApplicationHost`，不执行 script 或 physics。

## 8) 数据与状态边界

- 模块内部可变状态：GUI last error、输入缓冲、窗口生命周期、路径输入、临时 UI 状态。
- 模块内部可变状态：edit-time preview refresh version 与 preview snapshot。
- 外部可观察状态：当前 scene path、dirty、selection、last error、GUI 操作结果和 Scene View preview 状态。
- 线程模型与并发约束：默认单窗口、单 session、单 UI 线程。
- 资源生命周期：`Program -> EditorAppController -> EditorAppWindow.Run -> Shutdown`。

## 9) 质量门禁与验收

- Build 验收：`dotnet build AnsEngine.sln` 通过。
- Test 验收：`Engine.Editor.App.Tests` 覆盖边界、路径解析与 GUI 编排状态。
- Smoke 验收：`dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj` 能启动并关闭最小编辑器窗口。
- Perf 验收：GUI 宿主不改变 `Engine.App` 主路径，不做逐帧文件 IO 或逐帧 scene/asset cold reload。
- 边界验证项（必须逐条通过）：
  - [ ] `Engine.Editor.App` 不引用 `Engine.App`
  - [ ] `Engine.Editor.App` 对 `Engine.Scene`、`Engine.Render`、`Engine.Asset` 的依赖仅用于 edit-time preview composition
  - [ ] `Engine.Editor` 不引用 OpenTK、ImGui 或 `Engine.Editor.App`
  - [ ] `Engine.App` 不引用 `Engine.Editor.App`

## 10) 变更记录（Boundary Change Log）

- 2026-05-05
  - 变更人：Execution-Agent
  - 变更内容：完成 M21 QA 边界复验，确认 `Engine.Editor.App` 的 Scene/Render/Asset 依赖仅用于 edit-time preview composition，未引用 `Engine.App`，未引入 Play Mode、script execution、physics simulate、picking 或 gizmo。
  - 变更原因：支撑 `TASK-QA-022`，为 M21 Editor authoring MVP gate review and archive 准备边界证据。
  - 风险与回滚方案：若后续 preview 需要 runtime 行为或交互拾取，应另立任务卡并重新评审边界，不回写 M21 QA 范围。
- 2026-05-05
  - 变更人：Execution-Agent
  - 变更内容：按 M21.4 已批准边界变更，允许 `Engine.Editor.App` 直接依赖 `Engine.Scene`、`Engine.Render`、`Engine.Asset`，新增 `EditorScenePreviewHost` / `EditorScenePreviewSnapshot` 作为 edit-time Scene View preview composition；preview 使用 normalized scene、SceneGraphService、DiskMeshAssetProvider 与 SceneRenderSubmissionBuilder 生成非空预览状态。
  - 变更原因：支撑 `TASK-EAPP-011`，让 Editor 中央 Scene View 从容器升级为可消费当前 scene 的非空 edit-time preview。
  - 风险与回滚方案：preview 不调用 `ApplicationHost`，不执行 script/physics，不进入 Play Mode，不修改 Render/Scene public contract；若后续要做 picking/gizmo/play-mode，应另立卡，不扩大当前 preview host。
- 2026-05-05
  - 变更人：Execution-Agent
  - 变更内容：Inspector 扩展 `Scripts`、`RigidBody`、`BoxCollider` 与 `PhysicsParticipation` component groups；`EditorAppController` 增加对应 session wrapper；输入缓冲支持 Script JSON object、RigidBody body type/mass、BoxCollider size/center 的 Apply 与 add/remove。
  - 变更原因：支撑 `TASK-EAPP-010`，让 M21 Editor.App 能 author runtime 已支持的 Script/Physics 组件，同时保持 GUI 只通过 `SceneEditorSession` 编排。
  - 风险与回滚方案：当前不实现 Scene View preview、Play Mode、typed property grid 或复杂多脚本排序；Script properties 只做最小 JSON object 解析，失败时保留 session 文档不变。
- 2026-05-05
  - 变更人：Execution-Agent
  - 变更内容：Editor shell 布局语义从 `MainWorkspace` 收敛为 `SceneView`，Toolbar 高度调整为 56px，并新增可测试的专业深色 ImGui theme snapshot；中央区域删除旧 viewport 占位文案，改为 Scene View 容器。
  - 变更原因：支撑 `TASK-EAPP-009`，为 M21 后续 Inspector authoring 与 edit-time preview 建立稳定 Unity-like shell/theme 基线。
  - 风险与回滚方案：当前不引入 Render/Asset/Scene 依赖，也不实现 preview 渲染；若后续 preview 嵌入需要更改依赖，只能由 `TASK-EAPP-011` 的已批准边界变更承接。
- 2026-05-02
  - 变更人：Execution-Agent
  - 变更内容：Inspector 迁移为 `Object` / `Transform` / `MeshRenderer` component groups，GUI snapshot 和输入缓冲按组件组织；Transform-only object 显示明确 no-`MeshRenderer` 状态，提交时不自动补 MeshRenderer；默认 Add Object 仍创建 Transform + MeshRenderer。
  - 变更原因：支撑 `TASK-EAPP-008`，完成 M16 GUI 侧 component schema 展示与编辑链路，修复旧扁平 Inspector 与 `version: "1.0"` 测试夹具滞后问题。
  - 风险与回滚方案：当前不新增 MeshRenderer 组件添加按钮，仅显示合法缺省并避免自动修复；若后续需要从 GUI 显式添加/移除组件，应另立卡扩展，并继续通过 `SceneEditorSession` component API。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：将 Editor GUI 收敛为固定停靠布局，Toolbar 顶部、Hierarchy 左侧、Workspace 中央留白、Inspector 右侧、Status Bar 底部，并将布局尺寸写入可测试 GUI snapshot。
  - 变更原因：支撑 `TASK-EAPP-007`，让 M13 最小 GUI 编辑器具备稳定 Unity-like 工作区骨架，不再依赖自由漂浮窗口位置。
  - 风险与回滚方案：当前采用固定分区而非复杂 docking 框架；若后续需要可拖拽 dock tabs 或 viewport，应另立卡扩展，不改变现有编辑业务语义。
- 2026-05-01
  - 变更人：Execution-Agent
  - 变更内容：修复 Inspector 输入回流缺陷，`ImGuiOpenGlRenderer` 将 OpenTK 文本输入与键盘导航/编辑键转发给 ImGui；补充输入缓冲同选中对象跨帧不被 snapshot 覆盖、切换选中对象时重载有效值的回归测试。
  - 变更原因：`TASK-EAPP-004` 人工复验发现 Position/Rotation/Scale 输入框不可编辑，根因是 native ImGui frame 未接收键盘字符输入。
  - 风险与回滚方案：该修复只限 `Engine.Editor.App` GUI 输入桥接，不改变显式 Apply 提交语义；若后续要支持快捷键/复制粘贴完整矩阵，应继续局部扩展输入映射。
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
