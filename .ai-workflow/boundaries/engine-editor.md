# Engine.Editor 模块边界合同

## 1) 模块信息

- 模块名：`Engine.Editor`
- 版本：`v0.1`
- 负责人：`待指定`
- 生效日期：`2026-04-30`
- 关联任务卡：`TASK-EDITOR-001`

## 2) 目标与范围

- 模块目标：为 M13 GUI 编辑器提供可直接消费的无界面编辑器核心，负责场景编辑会话、dirty/selection 状态、编辑命令编排、保存与 reload 验证。
- 适用范围：打开中的场景 session、当前路径、当前文档快照、规范化场景快照、显式编辑结果、显式失败语义、保存与另存为工作流。
- 非适用范围：GUI、窗口、输入事件、鼠标拾取、渲染高亮、OpenGL、运行时场景对象、Undo/Redo、资源浏览器、asset existence 校验、App 默认启动路径改造。

## 3) 职责（Responsibilities）

- 负责组织 `Engine.SceneData` 文档原语形成可消费的编辑会话模型。
- 负责维护 `HasDocument`、`SceneFilePath`、`IsDirty`、`SelectedObjectId` 等会话状态。
- 负责对外暴露显式结果类型和失败种类，而不是使用 `bool/null` 作为主失败表达。
- 负责保存后 reload/normalize 验证，确保保存结果仍可被运行时链路消费。

## 4) 非职责（Non-Responsibilities）

- 不负责 JSON 文档 DTO、JSON 解析、默认值规范化和底层文档编辑规则（归属 `Engine.SceneData`）。
- 不负责窗口、面板、输入、拾取、渲染高亮、OpenGL 或视口交互。
- 不负责运行时场景对象、Render 提交、资源导入或 GPU 资源。
- 不负责未保存关闭确认流程、Undo/Redo 或 GUI 视图模型。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.SceneData`
  - `Engine.Contracts`
  - `Engine.Core`
- 可使用基础库/第三方：
  - `.NET` 标准库

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.App`
  - `Engine.Render`
  - `Engine.Platform`
  - `Engine.Asset`
- 禁止跨层调用模式：
  - 在 `Engine.Editor` 内重新实现 JSON loader/store/normalizer 规则
  - 在 `Engine.Editor` 内持有窗口、OpenGL、鼠标拾取或渲染高亮状态
  - 在 `Engine.Editor` 内改写 App 默认启动路径或运行时主循环

## 7) 公开接口（Public Interfaces）

- `SceneEditorSession`
  - 用途：承载打开中的场景编辑会话与显式工作流入口
  - 输入/输出：查询 `HasDocument`、`SceneFilePath`、`IsDirty`、`SelectedObjectId`、`Document`、`Scene`、`Objects`、`SelectedObject`；操作 `Open`、`Close`、`SelectObject`、`ClearSelection`、`AddObject`、`RemoveObject`、`RemoveSelectedObject`、`UpdateObjectId`、`UpdateObjectName`、`UpdateObjectResources`、`UpdateObjectTransform`、`Save`、`SaveAs`、`ReloadValidate`
  - 错误语义：所有失败通过显式 `SceneEditorSessionResult` / `SceneEditorFailure` 表达，不以 `bool/null` 作为主语义
  - 生命周期约束：单 session 持有当前文档、当前规范化场景与逻辑选中态，不持有 GUI 或运行时渲染资源

- `SceneEditorSessionResult`
  - 用途：统一表达 editor 操作成功/失败
  - 输入/输出：成功时可返回 session 更新后的核心状态，失败时返回 `SceneEditorFailure`
  - 错误语义：覆盖打开失败、无文档、保存失败、reload 失败、对象不存在、重复 id、非法引用、非法 transform 等显式场景
  - 生命周期约束：短生命周期结果对象，不持有外部资源

- `SceneEditorFailure`
  - 用途：承载 editor 失败种类、消息和诊断字段
  - 输入/输出：输入为失败 kind 与可选 path/object id；输出为可被 GUI 或测试直接消费的错误信息
  - 错误语义：与 `SceneEditorFailureKind` 对齐
  - 生命周期约束：无状态值对象

## 8) 数据与状态边界

- 模块内部可变状态：当前文档路径、当前文档快照、规范化场景快照、dirty 状态、selection 状态。
- 外部可观察状态：显式 session 查询面、显式操作结果、失败 kind/message。
- 线程模型与并发约束：默认单线程、单 session 语义；如未来引入并发编辑，需另立边界变更。
- 资源生命周期：`Open -> Query/Edit -> Save/SaveAs -> Close`；不持有窗口、GPU、文件句柄或运行时对象所有权。

## 9) 质量门禁与验收

- Build 验收：`dotnet build AnsEngine.sln` 通过。
- Test 验收：`Engine.Editor.Tests` 覆盖 session 状态机、编辑语义和保存/reload 闭环。
- Smoke 验收：合法 `.scene.json` 可完成 `open -> edit -> save -> reload`，失败路径不污染 session。
- Perf 验收：无逐帧 IO、无运行时热重载轮询、无 App 启动路径回归。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：`SceneEditorSession` 新增 `Save` 与 `SaveAs`，保存成功需完成写盘后 reload/normalize 验证，另存为成功后切换当前路径并清 dirty。
  - 变更原因：支撑 `TASK-EDITOR-004`，让 M12 headless editor core 形成 `open -> edit -> save -> reload` 闭环。
  - 风险与回滚方案：当前不包含 GUI 文件对话框、未保存关闭确认或 Undo/Redo；若后续 GUI 接入发现交互语义不足，应在 M13 GUI 层补工作流，不把 App/Render/Platform 依赖引入 Editor。
- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：`SceneEditorSession` 包装 `SceneFileDocumentEditor`，新增选择、清空选择、对象新增/删除、删除选中对象、更新对象 id/name/resources/transform，并固化 selection/dirty 与编辑失败回滚语义。
  - 变更原因：支撑 `TASK-EDITOR-003`，让 M12 headless editor core 具备 GUI 可直接驱动的编辑命令编排入口。
  - 风险与回滚方案：当前仍不包含保存/另存为；若后续保存语义异常，应在 `TASK-EDITOR-004` 内补齐磁盘写回和 reload 验证，不把底层 JSON/normalizer 规则复制到 Editor。
- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：`SceneEditorSession` 落地打开、关闭、当前路径、文档快照、规范化场景快照、对象列表、dirty 与 selection 查询，并新增 `ReloadValidate` 验证入口。
  - 变更原因：支撑 `TASK-EDITOR-002`，让 M12 headless editor core 具备可被后续 GUI 消费的“打开中的场景”会话模型。
  - 风险与回滚方案：当前不包含对象编辑命令和保存写回；若后续状态语义细化，应在 `SceneEditorSession` 内继续编排 `SceneData` 原语，不把 JSON/normalizer 规则复制到 Editor。
- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：落地 `Engine.Editor` 与 `Engine.Editor.Tests` 工程骨架，接入解决方案；新增 `SceneEditorSession`、`SceneEditorSessionResult`、`SceneEditorFailure`、`SceneEditorFailureKind` 作为后续 headless editor core 的公开入口种子。
  - 变更原因：支撑 `TASK-EDITOR-001`，让 M12 后续 session、dirty、selection、保存与 reload 验证任务有稳定模块边界和测试落点。
  - 风险与回滚方案：当前仅建立模块和结果语义种子，不实现打开/保存/编辑状态机；若后续接口形状细化，应在保持无 GUI、无 App/Render/Platform/Asset 依赖前提下演进。
- 2026-04-30
  - 变更人：Dispatch-Agent
  - 变更内容：建立 `Engine.Editor` 初版边界合同种子，预留 `SceneEditorSession`、显式结果类型与 M12 headless editor core 落地边界。
  - 变更原因：支撑 `TASK-EDITOR-001`，在进入实现前先明确 Editor 的职责、非职责与依赖方向，避免 session/dirty/selection 状态回流到 `SceneData` 或提前接入 `App`。
  - 风险与回滚方案：若后续发现 session 公开面或失败种类仍需细化，可在保持 `Engine.Editor -> SceneData/Contracts/Core` 依赖方向与“无 GUI、无 App 接线”前提下演进，不回退为 `SceneData` 或 `App` 承载编辑器状态。
