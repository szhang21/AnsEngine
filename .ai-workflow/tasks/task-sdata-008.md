# 任务: TASK-SDATA-008 M17 SceneData Script component schema and validation

## TaskId
`TASK-SDATA-008`

## 目标（Goal）
在 `Engine.SceneData` 中新增 repeatable `Script` component schema 与 `SceneScriptComponentDescription`，并收口 `scriptId`、properties 类型和顺序保持等 validation 规则。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M17-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M17.2`

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
- ParallelGroup: `M17-G1`
- CanRunParallel: `false`
- DependsOn:
  - `TASK-SCRIPT-001`

## 里程碑上下文（MilestoneContext）
- M17.2 负责把 Script component 正式引入 SceneData schema 与 normalized model；没有这层，脚本 runtime 只能依赖硬编码样例或私有桥接，无法成为正式场景数据的一部分。
- 本卡承担的是 Script component file model、normalized description 与 validation，不承担脚本 runtime 生命周期、Scene self-transform bridge 或 App 注册/执行。
- 直接影响本卡实现的上游背景包括：`Script` component 使用 `scriptId + properties`；`properties` 第一版只支持 number/bool/string；多 Script component 允许且顺序必须保持；SceneData 不得依赖 `Engine.Scripting` 类型。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `Script` component 形状：
    - `type: "Script"`
    - `scriptId`
    - `properties`
  - `Script` component 是 repeatable。
  - `scriptId` 必须非空。
  - `properties` 可为空。
  - `properties` 值只支持 number / bool / string。
  - unknown property names 在 SceneData 层允许，是否符合脚本期望由 binding 决定。
  - Script component 顺序必须从 scene file 保留到 normalized description。
- 本卡执行时不得推翻的既定取舍：
  - 不允许在 SceneData 层依赖 `Engine.Scripting` 的 runtime/context/behavior 类型。
  - 不允许把 Script property 扩展到数组、对象、`Vector3` 或任意 JSON。
  - 不允许在 SceneData 层验证“是否注册了 scriptId”或“某脚本要求哪些属性”。
  - 不允许因为 Editor 暂不编辑 Script component 就忽略顺序和原样保留语义。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M17-2026-05-02 > SceneDataDesign` 已定稿示例 JSON：
    - `type: "Script"`
    - `scriptId: "RotateSelf"`
    - `properties.speedRadiansPerSecond: 1.5708`
  - `PLAN-M17-2026-05-02 > SceneDataDesign` 已定稿 normalized model 需要新增 `SceneScriptComponentDescription`，执行时不得把脚本重新编码回 generic object bag 而丢失明确组件语义。

## 实施说明（ImplementationNotes）
- 先在 SceneData file model 中增加 `Script` component DTO 和 property value 承载模型。
- 再迁移 normalizer，使其输出 `SceneScriptComponentDescription` 并保留 Script component 顺序。
- 然后落地 validation：
  - missing/blank `scriptId` fails
  - unsupported property value type fails
  - multiple Script components preserve order and normalize success
- 最后补 SceneData tests，覆盖 serialize/deserialize、顺序保持、失败路径和“不依赖 Engine.Scripting”的边界证据。

## 设计约束（DesignConstraints）
- 不允许在本卡实现 script binding、registry lookup 或脚本属性语义校验。
- 不允许让 `Script` component 破坏 M16 现有 `Transform` / `MeshRenderer` 规则。
- 不允许顺手加入 Script component editor UI。
- 不允许修改 Scene runtime 或 App 主循环。

## 失败与降级策略（FallbackBehavior）
- 若 property value 模型在实现上需要一个小型 union/结果类型，允许内部实现选择，但对外支持范围必须仍限定为 number/bool/string。
- 若序列化实现存在多种 `System.Text.Json` 路线，可选择最稳定方案，但必须保证保存后顺序不变。
- 若实现中发现需要引用 `Engine.Scripting` 才能知道合法 `scriptId`，必须停工回退。
- 若某些属性类型只能通过弱化为任意 JSON 才能通过测试，也必须回退修卡。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/**`
  - `src/Engine.App/SampleScenes/default.scene.json`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-006.md`
  - `.ai-workflow/tasks/task-sdata-007.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M17-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M17-2026-05-02 > SceneDataDesign`
  - `PLAN-M17-2026-05-02 > PlanningDecisions`
  - `PLAN-M17-2026-05-02 > Milestones > M17.2`
  - `PLAN-M17-2026-05-02 > TestPlan`
  - 上述 SceneData design 引用属于“参考实现约束”，`Script` component 字段与 property 支持范围已定。

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
- AllowedFiles:
  - Script component file model
  - normalized description
  - SceneData tests
  - sample scene JSON as needed
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`
  - `src/Engine.App/SampleScenes/default.scene.json`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不实现脚本 registry/runtime/binding
- 不实现 Scene self-transform bridge
- 不实现 Editor Script UI
- 不验证脚本属性业务含义
- OutOfScopePaths:
  - `src/Engine.Scripting/**`
  - `tests/Engine.Scripting.Tests/**`
  - `src/Engine.Scene/**`
  - `src/Engine.App/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - property value union 的具体内部建模可按仓库风格调整，但不能突破 number/bool/string 支持边界。
- 处理规则：
  - 若问题影响 `scriptId` 语义、顺序保持或 SceneData 与 Scripting 的边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - Script component 的字段、重复规则、顺序语义和 validation 范围都已明确。
  - 执行者无需回看计划也能知道 SceneData 只负责 schema/normalize，不负责 script binding。
  - 失败分流与非范围已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是脚本系统进入场景数据主路径的关键卡，若 schema 或 normalized model 设计模糊，后续 Scene/App 都会偏。
  - 同时要守住顺序语义、property 类型边界和“不依赖 Engine.Scripting”的约束。
- SufficiencyMatch: `pass`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - `Engine.SceneData -> Engine.Contracts`
- ForbiddenDependsOn:
  - `Engine.SceneData -> Engine.Scripting`
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
- Test: `dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 Script serialize/deserialize、顺序保持、blank scriptId、unsupported property type
- Smoke: 默认 sample scene 可声明 `Script` component；multiple Script components 顺序在 load-save-load 后保持稳定
- Perf: Script schema 支持不引入逐帧 JSON 解析、跨模块 registry 查询或 property 反射开销

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SDATA-008.md`
- ClosedAt: `2026-05-02 14:09`
- Summary:
  - 2026-05-02: Started after `TASK-SCRIPT-001` reached Review; AllowedPaths, SceneData-only dependency constraints, repeatable Script ordering, and property type limits checked.
  - 2026-05-02: Added repeatable file-model `Script` component with `scriptId` and number/bool/string property value union.
  - 2026-05-02: Added normalized `SceneScriptComponentDescription` and preserved Script component file order in normalized object components.
  - 2026-05-02: Added validation/tests for blank scriptId, unsupported property type, serialize/deserialize, and load-save-load order stability.
  - 2026-05-02: Added `RotateSelf` Script declaration to default app sample scene without adding runtime binding.
- FilesChanged:
  - `src/Engine.SceneData/FileModel/SceneFileComponentDefinition.cs`
  - `src/Engine.SceneData/FileModel/SceneFileScriptPropertyValue.cs`
  - `src/Engine.SceneData/Descriptions/SceneComponentDescription.cs`
  - `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
  - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `src/Engine.App/SampleScenes/default.scene.json`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-sdata-008.md`
- ValidationEvidence:
  - Build: `dotnet build AnsEngine.sln --no-restore --nologo -v minimal` passed with existing `net7.0` EOL warnings.
  - Test: `dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal` passed, 37 tests.
  - Smoke: Script component serializes/deserializes; multiple Script components preserve file order through normalized descriptions and document save/load; default sample scene declares `RotateSelf`.
  - Perf: no runtime registry lookup, reflection, per-frame JSON parsing, or cross-module binding added in SceneData.
  - Boundary: `rg` check found no `Engine.SceneData -> Engine.Scripting/Scene/App/Editor/Render` usage; hits were namespace/test assertions only.
- ModuleAttributionCheck: pass
