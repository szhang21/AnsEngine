# 任务: TASK-SDATA-007 M16 normalized component descriptions and validation

## TaskId
`TASK-SDATA-007`

## 目标（Goal）
将 `SceneObjectDescription` 迁移为 component-based normalized model，并在 `Engine.SceneData` 收口 Transform/MeshRenderer 的必需性、重复性、未知类型与默认材质等 validation 规则。

## 任务来源（TaskSource）
DispatchAgent

## 计划引用（兼容别名：PlanRef）
`PLAN-M16-2026-05-02`

## 里程碑引用（兼容别名：MilestoneRef）
`M16.2`

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
  - `TASK-SDATA-006`

## 里程碑上下文（MilestoneContext）
- M16.2 负责把新文件 schema 真正收敛成稳定的 normalized component descriptions；没有这层，runtime bridge 和 editor component API 只能继续围绕文件 DTO 或旧扁平模型工作。
- 本卡承担的是 normalizer、normalized description 模型与 validation 规则，不承担 JSON 文件层多态解析，也不承担 runtime component 构建或 GUI 展示。
- 上游直接影响本卡实现的背景包括：`Transform` 是必需组件，`MeshRenderer` 可选；duplicate/unknown component 要失败；Camera 仍保持 scene-level；`Engine.SceneData` 不得引用 runtime component types。

## 决策继承（DecisionCarryOver）
- 从计划/里程碑继承的关键决策：
  - `SceneObjectDescription` 不再直接持有 `Mesh` / `Material` / `LocalTransform`。
  - 应改为持有 normalized component descriptions，例如 `SceneComponentDescription`、`SceneTransformComponentDescription`、`SceneMeshRendererComponentDescription`。
  - `Transform` component description 包含 normalized finite position / rotation / scale。
  - `MeshRenderer` component description 包含 normalized `SceneMeshRef` 与 `SceneMaterialRef`。
  - `MeshRenderer.material` 为 null/blank 时回退到 `material://default`。
- 本卡执行时不得推翻的既定取舍：
  - 不允许继续把旧对象级 `Mesh/Material/LocalTransform` 作为 normalized 主模型保留。
  - 不允许让 unknown component type 被忽略后继续成功 normalize。
  - 不允许在 SceneData 内引用或创建 `SceneTransformComponent` / `SceneMeshRendererComponent` 等 runtime 类型。
  - 不允许把 Camera 组件化或把 M15 默认旋转 smoke behavior 序列化进文档模型。
- 若计划/里程碑已给出示例数据结构、字段草图、DTO/record/class 形状、关系图或字段命名约定：
  - `PLAN-M16-2026-05-02 > NormalizedModelDesign` 已定稿 `SceneObjectDescription` 改为持有 normalized component descriptions，执行时不得改回对象扁平 normalized 字段。
  - `PLAN-M16-2026-05-02 > ValidationRules` 已定稿：
    - 每个 object 必须且仅有一个 `Transform`
    - `MeshRenderer` 零或一
    - duplicate component fail
    - unknown component fail
    - Transform 数值必须 finite
    - MeshRenderer.material 空白时默认 `material://default`

## 实施说明（ImplementationNotes）
- 先迁移 normalized model 类型：
  - 给 `SceneObjectDescription` 换成 component-based 描述集合
  - 增加 Transform / MeshRenderer normalized description 值对象
- 再迁移 `SceneFileDocumentNormalizer` 或等价入口，把 file model components 映射到 normalized descriptions。
- 然后在 normalizer 中显式落地 validation：
  - missing Transform
  - duplicate Transform
  - duplicate MeshRenderer
  - unknown component type
  - non-finite transform
  - invalid mesh ref
  - material defaulting
- 最后补 SceneData 测试，覆盖 Transform-only object normalize 成功、默认材质回退、各种失败路径和 load-save-load 结构稳定。

## 设计约束（DesignConstraints）
- 不允许在本卡直接处理 runtime object/component 构建。
- 不允许让 Editor 或 App 成为 normalized validation 的新真相来源。
- 不允许顺手增加第三种 component type、脚本、物理、动画、Camera 组件化。
- 不允许回退到“normalize 时偷偷修复缺失 Transform 并继续成功”的宽松路线。

## 失败与降级策略（FallbackBehavior）
- 若 component description 的具体类名或层次稍有调整，允许在不改变“component-based normalized model”事实的前提下局部命名优化。
- 若 material defaulting 需要复用现有 reference parser，可复用，但必须保持空白时默认、非空时严格校验的语义。
- 若实现中发现某些错误只能靠忽略重复/未知 component 才能让样例通过，必须停工回退，不得放宽 validation。
- 若 normalizer 迁移需要 runtime 类型协助，必须回退修卡，因为这已越过 SceneData 边界。

## 参考点（ExamplesOrReferences）
- 相关源码入口：
  - `src/Engine.SceneData/**`
  - `src/Engine.Contracts/**`
- 相关测试入口：
  - `tests/Engine.SceneData.Tests/**`
- 相关已有任务/归档/文档：
  - `.ai-workflow/tasks/task-sdata-006.md`
  - `.ai-workflow/tasks/task-sdata-004.md`
  - `.ai-workflow/tasks/task-sdata-005.md`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/plan-archive/2026-05/PLAN-M16-2026-05-02.md`
- 计划结构引用：
  - `PLAN-M16-2026-05-02 > NormalizedModelDesign`
  - `PLAN-M16-2026-05-02 > ValidationRules`
  - `PLAN-M16-2026-05-02 > Milestones > M16.2`
  - `PLAN-M16-2026-05-02 > TestPlan`
  - 上述 normalized/validation 引用属于“字段关系与规则已定的参考实现约束”。

## 范围（Scope）
- AllowedModules:
  - Engine.SceneData
- AllowedFiles:
  - normalized component descriptions
  - normalizer / validation logic
  - SceneData tests
- AllowedPaths:
  - `src/Engine.SceneData/**`
  - `tests/Engine.SceneData.Tests/**`

## 跨模块标记（CrossModule）
false

## 非范围（OutOfScope）
- 不修改 runtime bridge
- 不修改 editor session / inspector GUI
- 不新增 schema 兼容层
- 不让 SceneData 感知 runtime component types
- OutOfScopePaths:
  - `src/Engine.Scene/**`
  - `tests/Engine.Scene.Tests/**`
  - `src/Engine.Editor/**`
  - `src/Engine.Editor.App/**`

## 未决问题（OpenQuestions）
- 已明确的不确定点：
  - normalized component description 的具体基类/集合容器形状可做最小命名调整，但不能改变 `SceneObjectDescription` 已组件化这一事实。
- 处理规则：
  - 若问题影响 validation 口径、默认材质语义或 SceneData 与 runtime 的边界，必须先回退，不得自行脑补。

## 执行充分性（ExecutionReadiness）
- ExecutionReady: `true`
- WhyReady:
  - normalized model 方向、validation 规则、默认材质语义和非范围都已明确。
  - 执行者无需回看里程碑也能知道本卡不能继续保留对象扁平 normalized 字段。
  - 失败分流与边界保护点已写清。
- MissingInfo:
  - `none`

## ComplexityAssessment
- Level: `L3`
- Why:
  - 这是 M16 的语义收口卡，选错 normalized model 会直接影响 Scene runtime、Editor session 和后续所有测试。
  - 同时要守住 validation 严格性与模块边界，认知复杂度高。
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
- Test: `dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal` 通过，覆盖 missing/duplicate/unknown/default-material/transform-only
- Smoke: Transform-only object 可 normalize 成功；MeshRenderer 缺 material 自动得到 `material://default`；旧扁平 normalized 字段不再作为主路径存在
- Perf: normalize 仅发生在显式 load/save/reload 阶段，不引入逐帧解析或跨模块校验回调

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
- ArchivePath: `.ai-workflow/archive/2026-05/TASK-SDATA-007.md`
- ClosedAt: `2026-05-02 11:44`
- Summary:
  - Verify: `SceneObjectDescription` 已迁移为 component-based normalized model，normalizer 已收口 Transform/MeshRenderer validation。
  - Review: Build/Test/Smoke/Boundary/Perf 通过，归档三件套已准备，等待 Human 复验。
- FilesChanged:
  - `src/Engine.SceneData/Descriptions/SceneComponentDescription.cs`
  - `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
  - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
  - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
  - `.ai-workflow/boundaries/engine-scenedata.md`
  - `.ai-workflow/tasks/task-sdata-007.md`
  - `.ai-workflow/archive/2026-05/TASK-SDATA-007.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/board.md`
- ValidationEvidence:
  - Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
  - Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`；SceneData.Tests 33 条通过）
  - Smoke: `pass`（Transform-only object normalize 成功；缺 Transform 失败；MeshRenderer 缺 material 得到 `material://default`；旧扁平字段不再作为 normalized 主模型）
  - Boundary: `pass`（仅改 AllowedPaths 与边界/任务/归档文档；`Engine.SceneData` 未新增 runtime/editor/render 依赖）
  - Perf: `pass`（normalize 仅在显式 load/save/reload 阶段发生，无逐帧解析或跨模块回调）
- ModuleAttributionCheck: pass
