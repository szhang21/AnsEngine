# Engine.SceneData 模块边界合同

## 1) 模块信息

- 模块名：`Engine.SceneData`
- 版本：`v0.1`
- 负责人：`待指定`
- 生效日期：`2026-04-25`
- 关联任务卡：`TASK-SDATA-001`

## 2) 目标与范围

- 模块目标：负责场景文档模型、JSON 解析、校验与默认值规范化，为运行时 `Engine.Scene` 提供稳定、可加载的场景描述输入。
- 适用范围：场景文件 DTO、规范化场景描述、显式加载失败语义、默认值填充、格式兼容演进。
- 非适用范围：运行时场景对象、world transform 缓存、mesh 顶点数据、GPU 资源、asset catalog、编辑器状态。

## 3) 职责（Responsibilities）

- 负责场景 JSON 描述模型定义。
- 负责文件描述层到规范化场景层的转换。
- 负责显式加载失败结果与校验错误表达。
- 负责保持场景文档层对运行时层的低耦合。

## 4) 非职责（Non-Responsibilities）

- 不负责 Scene 运行时对象生命周期（归属 `Engine.Scene`）。
- 不负责 mesh/材质资源导入与缓存（归属 `Engine.Asset`）。
- 不负责 GPU 上传与渲染提交（归属 `Engine.Render`）。
- 不负责应用装配、场景文件选择策略与运行模式切换（归属 `Engine.App`）。
- 不负责编辑器选中态、Gizmo、Undo 栈等编辑器状态。

## 5) 允许依赖（Allowed Dependencies）

- 可直接依赖模块：
  - `Engine.Contracts`
- 可使用基础库/第三方：
  - `.NET` 标准库
  - `System.Text.Json`

## 6) 禁止依赖（Forbidden Dependencies）

- 禁止直接依赖模块：
  - `Engine.Scene`
  - `Engine.Asset`
  - `Engine.Render`
  - `Engine.App`
- 禁止跨层调用模式：
  - 在 `SceneData` 内创建运行时场景对象
  - 在 `SceneData` 内解析 asset catalog 或 OBJ
  - 在 `SceneData` 内持有 OpenGL 或窗口相关资源

## 7) 公开接口（Public Interfaces）

- `ISceneDescriptionLoader`
  - 用途：根据场景文件路径加载规范化 `SceneDescription`
  - 输入/输出：输入 `sceneFilePath`，输出 `SceneDescriptionLoadResult`
  - 错误语义：缺失文件、非法 JSON、重复对象 `Id`、缺少必填字段等通过显式失败结果返回
  - 生命周期约束：仅负责单次加载与规范化，不持有运行时状态

## 8) 数据与状态边界

- 模块内部可变状态：可选的格式版本策略与默认值规则；不维护运行时场景缓存作为主职责。
- 外部可观察状态：场景描述对象、加载失败类型、格式版本演进语义。
- 线程模型与并发约束：默认无状态或短生命周期对象；如未来引入缓存，必须保持只读结果语义稳定。
- 资源生命周期：`Read -> Parse -> Validate -> Normalize -> Return Result`，不持有 GPU、文件句柄或运行时对象所有权。

## 9) 质量门禁与验收

- Build 验收：`dotnet build -c Debug/Release` 通过。
- Test 验收：场景 JSON 加载、默认值规范化、错误语义测试通过。
- Smoke 验收：组合根可装配 SceneData loader 并成功加载样例场景。
- Perf 验收：加载仅发生在初始化阶段，稳定运行阶段无逐帧 JSON 解析。
- 边界验证项（必须逐条通过）：
  - [ ] 无禁止依赖
  - [ ] 无越界职责实现
  - [ ] 公开接口与合同一致

## 10) 变更记录（Boundary Change Log）

- 2026-04-26
  - 变更人：Execution-Agent
  - 变更内容：明确 `Engine.App` 会作为组合根显式装配并调用 `ISceneDescriptionLoader`，而 `SceneData` 仍只暴露加载契约与规范化结果，不承载场景文件选择策略。
  - 变更原因：支撑 `TASK-APP-009` 的启动路径接线，保持“路径选择在 App、加载与规范化在 SceneData”的职责分离。
  - 风险与回滚方案：若后续样例路径或配置来源变化，仅调整 App 的装配入口；不把文件路径策略下沉回 `SceneData`。
- 2026-04-26
  - 变更人：Execution-Agent
  - 变更内容：明确 `Engine.SceneData` 的规范化场景层会被 `Engine.Scene` 作为运行时初始化输入消费，但 `SceneData` 仍不持有运行时对象或渲染实现细节。
  - 变更原因：支撑 `TASK-SCENE-009` 的跨模块接线，保持 `SceneData -> Scene` 仅为数据流向而非反向实现依赖。
  - 风险与回滚方案：若后续 `Scene` 需要更多运行时语义，应在 Scene 内部扩展映射而不是把运行时状态倒灌回 `SceneData`。
- 2026-04-26
  - 变更人：Execution-Agent
  - 变更内容：为 `Engine.SceneData` 增加 `JsonSceneDescriptionLoader`、默认值规范化和显式失败语义落地，补齐样例场景 JSON 与回归测试入口。
  - 变更原因：支撑 `TASK-SDATA-002`，让 `SceneData` 从纯模块骨架推进到可消费场景文件的文档层实现。
  - 风险与回滚方案：若后续发现 schema 字段或默认相机策略仍需细化，应在保持 `FileModel -> Descriptions` 两层结构与显式失败结果不变的前提下迭代，不把规范化逻辑外溢到 `Scene/App`。
- 2026-04-25
  - 变更人：Execution-Agent
  - 变更内容：将 `Engine.SceneData` 边界合同从种子文档推进到已落地模块状态，明确文件描述层、规范化场景层和显式加载结果类型的稳定入口。
  - 变更原因：支撑 `TASK-SDATA-001` 的模块骨架、解决方案接线和最小依赖测试，让后续 `TASK-SDATA-002` 可以在既定目录与命名下继续实现 JSON 主路径。
  - 风险与回滚方案：若后续发现加载接口或描述模型仍需细化，可在保持 `SceneFile*` / `Scene*Description` 分层与 `SceneData -> Contracts` 依赖方向不变的前提下迭代，不回退为 `Scene/App` 直接承载场景文档职责。
- 2026-04-25
  - 变更人：Dispatch-Agent
  - 变更内容：建立 `Engine.SceneData` 初版边界合同种子，预留 M10 场景文档层与 JSON 主路径落地。
  - 变更原因：支撑 `TASK-SDATA-001`，让新模块在进入实现前已有明确职责和依赖边界。
  - 风险与回滚方案：若后续发现文档层与规范化层仍需再拆分，可在保持 `SceneData` 外部接口稳定前提下细化内部结构，不回退为 `Scene/App` 直接解析 JSON。
