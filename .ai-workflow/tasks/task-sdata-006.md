# 任务: TASK-SDATA-006 M16 SceneData component file schema

## TaskId
`TASK-SDATA-006`

## 目标（Goal）
将 `Engine.SceneData` 的场景文档文件模型迁移到 `version: "2.0"` component array schema，并完成 JSON serializer/deserializer、默认 sample scene 与 SceneData sample tests 的同步迁移。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M16-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M16.1`

## 执行代理（ExecutionAgent）
Exec-SceneData

## 优先级（Priority）
P0

## 主模块归属（PrimaryModule）
Engine.SceneData

## 次级模块（SecondaryModules）
- Engine.Contracts

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-scenedata.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 并行计划（ParallelPlan）
- ParallelGroup: `M16-G1`
- CanRunParallel: `false`
- DependsOn:
  - `none`

## 里程碑上下文（MilestoneContext）
- M16.1 是整个 component serialization bridge 的第一块地基；没有新的文件 schema，后续 normalized model、runtime bridge 和 editor component workflow 都没有稳定输入。
- 本卡承担的是文件层 DTO 与 JSON 多态反序列化迁移，以及 sample scene 文件同步；不承担 normalized validation 规则全量收口，也不承担 runtime 或 editor 适配。
- 直接影响本卡实现的上游背景包括：M16 选择直接 breaking migration，不做 `1.0`/`2.0` 双轨兼容；component payload 用扁平字段，不做 `value` 包装；Camera 继续保持 scene-level，不组件化。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - Scene JSON `version` 固定升级到 `"2.0"`。
  - 旧 `version: "1.0"` 扁平对象格式加载失败，不做生产兼容。
  - Object schema 使用 `components: [...]` 数组。
  - 每个 component 用 Pascal 简名 `type`。
  - 首批仅支持 `Transform` 与 `MeshRenderer`。
  - component payload 使用扁平字段，不使用 `value` 包装。
- 本卡执行时不得推翻的既定取舍：
  - 不允许保留旧对象级 `mesh/material/transform` 字段作为写回主路径。
  - 不允许引入双轨读写兼容层，让 `1.0` 与 `2.0` 同时成功加载。
  - 不允许把 runtime component 类型带入 `Engine.SceneData` 文件模型。
  - 不允许顺手把 Camera 也改成 object component。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M16-2026-05-02 > SchemaDesign` 已定稿 JSON 形状：
    - object 使用 `id`、`name`、`components`
    - `Transform` payload 包含 `position`、`rotation`、`scale`
    - `MeshRenderer` payload 包含 `mesh`、`material`
  - `PLAN-M16-2026-05-02 > SchemaDesign > File model guidance` 已定稿 `SceneFileObjectDefinition.Id/Name/Components` 和建议文件模型 `SceneFileComponentDefinition`、`SceneFileTransformComponentDefinition`、`SceneFileMeshRendererComponentDefinition`，执行时不得改回扁平对象字段模型。

## 实施说明（ImplementationNotes）
- 先迁移文件层 DTO：
  - `SceneFileObjectDefinition` 从扁平资源/transform 字段迁移到 `Components`
  - 增加 component 文件模型基类/区分类型的 payload 记录
- 再实现 serializer/deserializer 的 type-based concrete payload 选择，只覆盖 `Transform` 与 `MeshRenderer` 两种首批类型。
- 然后迁移 sample scenes：
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
- 最后补 SceneData 文件层测试，覆盖：
  - `2.0` component array 文档可 load/save/load
  - `1.0` 文档失败
  - save 后不回写旧 `mesh/material/transform` object 字段

## 设计约束（DesignConstraints）
- 不允许在本卡中实现 normalized validation 的最终业务语义收口，那是下一张卡的职责。
- 不允许把 component 多态解析设计成开放注册表或脚本式扩展点；M16 首批只支持两种固定类型。
- 不允许在 `Engine.SceneData` 中引入 `Engine.Scene`、`Engine.Editor` 或 `Engine.Editor.App` 依赖。
- 不允许新增与 `SceneFileTransformDefinition` 并行但语义重复的脏模型，除非是清晰替换路径的一部分。

## 失败与降级策略（FallbackBehavior）
- 若现有 transform 文件模型可安全复用到 component payload，允许复用，但必须保持最终文件语义清晰，不留下重复语义类型并存的长期状态。
- 若 serializer 多态落地方式有多种可选，可采用当前仓库最稳定的 `System.Text.Json` 方案，但必须保持未知 type 不被静默吞掉。
- 若实现中发现必须引入 `1.0` 兼容或保留旧字段写回才能通过测试，必须停工回退，不得自行放宽计划。
- 若 sample scene 迁移暴露出 normalized/runtime/editor 假设冲突，只修文件层可见问题；超出 schema/file model 的冲突留给后续卡处理。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/**`
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-003.md`
  - `.ai-workflow/tasks/task-sdata-004.md`
  - `.ai-workflow/tasks/task-sdata-005.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M16-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M16-2026-05-02 > PlanningDecisions`
  - `PLAN-M16-2026-05-02 > SchemaDesign`
  - `PLAN-M16-2026-05-02 > Milestones > M16.1`
  - `PLAN-M16-2026-05-02 > TestPlan`
  - 上述 schema 引用属于“参考实现约束”，字段关系与命名方向已定，不是可自由改写的示意图。

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
- AllowedFiles:
  - SceneData file model
  - JSON serializer/deserializer
  - sample scene JSON files
  - SceneData file-schema tests
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.App/SampleScenes/default.scene.json`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不完成 normalized validation 收口
- 不修改 runtime bridge
- 不修改 editor session 或 inspector GUI
- 不实现旧 `1.0` 生产兼容
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - `SceneFileTransformDefinition` 是复用还是被更专门的 component DTO 替换，只要最终不存在长期双语义脏模型即可。
- 处理规则：
  - 若问题影响 schema 形状、字段命名、版本语义或 `1.0` 失败语义，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - `2.0` schema、component array、固定类型集合和 sample scene 迁移范围都已明确。
  - 执行者无需回看里程碑全文也能知道本卡不能保留旧字段写回或双轨兼容。
  - 文件层与后续 normalized/runtime/editor 责任边界已拆清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 breaking schema migration 的起点，若文件模型和 serializer 方向选错，后续所有模块都会连锁返工。
  - 需要同时守住版本语义、类型选择、sample scene 迁移和“不回写旧字段”的硬约束。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.SceneData -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.SceneData -> Engine.Scene`
  - `Engine.SceneData -> Engine.App`
  - `Engine.SceneData -> Engine.Editor`
  - `Engine.SceneData -> Engine.Editor.App`
  - `Engine.SceneData -> Engine.Render`

## 边界变更请求（BoundaryChangeRequest）
- Required: `false`
- Status: `none`
- RequestReason:
- ImpactModules:
- HumanApprovalRef:

## 边界同步计划（BoundarySyncPlan）
- NewFilesExpected: `true`
- BoundaryDocsToUpdate:
  - `.ai-workflow/boundaries/engine-scenedata.md`
- ChangeLogRequired: `true`

## 验收标准（Acceptance）
- Build: `dotnet build AnsEngine.sln --nologo -v minimal` 通过
- Test: `dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 `2.0` component array load/save/load 与 `1.0` 失败
- Smoke: 默认 sample scene 和 SceneData sample scene 都以 `version: "2.0"` + `components` 数组落盘，保存后不写旧扁平字段
- Perf: 仅发生显式 load/save 序列化，不引入逐帧 JSON 解析或额外兼容分支轮询

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)
- 文件组织约定：默认一个类一个文件、一个接口一个文件；仅在小型强耦合辅助类型、嵌套实现细节、测试桩或迁移过渡期允许例外

## 状态（Status）
Done

## 完成度（Completion）
`100`

## 缺陷回流字段（Defect Triage）
- FailureType: `Other`
- DetectedAt:
- ReopenReason:
- OriginTaskId:
- HumanSignoff: `pass`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SDATA-006.md`
- ClosedAt: `2026-05-02 11:40`
- Summary:
  - Verify: 文件层 DTO 已迁移到 `version: "2.0"` component array，默认样例和测试样例已同步迁移。
  - Review: Build/Test/Smoke/Boundary/Perf 通过，归档三件套已准备，等待 Human 复验。
- FilesChanged:
  - `src/Engine.SceneData/FileModel/SceneFileComponentDefinition.cs`
  - `src/Engine.SceneData/FileModel/SceneFileObjectDefinition.cs`
  - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-sdata-006.md`
  - `.ai-workflow/archive/2026-05/TASK-SDATA-006.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`；SceneData.Tests 31 条通过）
  - Smoke: `pass`（默认 app sample 与 SceneData sample 均为 `version: "2.0"` + `components`；保存后 object 层不写旧 `mesh/material/transform` 字段）
  - Boundary: `pass`（仅改 AllowedPaths 与边界/任务/归档文档；`Engine.SceneData` 未新增 Scene/App/Editor/Render 依赖）
  - Perf: `pass`（仅显式 load/save 序列化，无逐帧 JSON 解析或兼容分支轮询）
- ModuleAttributionCheck: pass
