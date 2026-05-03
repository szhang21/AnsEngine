# 任务归档索引

> 本文件为已关闭任务的索引入口。详细快照请放在 `.ai-workflow/archive/<yyyy-mm>/<task-id>.md`。

## 字段约定

- TaskId
- Title
- Priority（P0/P1/P2/P3）
- PrimaryModule
- BoundaryContractPath
- Owner
- ClosedAt
- Status（Review/Done/Cancelled）
- ModuleAttributionCheck（pass/fail）
- Summary
- FilesChanged
- ValidationEvidence
- SnapshotPath

## 归档记录

### 模板

```md
- TaskId: <TASK-ID>
  Title: <任务标题>
  Priority: <P0|P1|P2|P3>
  PrimaryModule: <Engine.Render|Engine.Scene|...>
  BoundaryContractPath: <.ai-workflow/boundaries/xxx.md>
  Owner: <name>
  ClosedAt: <YYYY-MM-DD HH:mm>
  Status: <Review|Done|Cancelled>
  ModuleAttributionCheck: <pass|fail>
  Summary:
    - <改动点1>
    - <改动点2>
  FilesChanged:
    - <path1>
    - <path2>
  ValidationEvidence:
    - Build: <pass/fail + note>
    - Test: <pass/fail + note>
    - Smoke: <pass/fail + note>
    - Perf: <pass/fail + note>
  SnapshotPath: <.ai-workflow/archive/yyyy-mm/task-id.md>
```

### 当前记录

- TaskId: `TASK-APP-011`
  Title: `M17 App scripting runtime integration and RotateSelf sample`
  Priority: `P1`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-05-02 17:41`
  Status: `Review`
  ModuleAttributionCheck: `pass`
  Summary:
    - Composed `ScriptRegistry` / `ScriptRuntime` in `Engine.App` and registered built-in `RotateSelf`
    - Bound Script components after scene initialization and ran script update before render
    - Added App tests for valid script flow, unknown script id failure, script exception failure, and default rotation removal
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `src/Engine.App/Engine.App.csproj`
    - `src/Engine.App/SceneRuntimeContracts.cs`
    - `tests/Engine.App.Tests/Engine.App.Tests.csproj`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-011.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL 与本机 `LIB` 路径 warning）
    - Test: pass（`dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`，12/12 passed）
    - Smoke: pass（headless 默认 sample scene 运行退出码 0，`RotateSelf` 主路径由 App 测试验证 render 前更新）
    - Perf: pass（无逐帧程序集加载、源码编译、热重载轮询、重复脚本绑定或 render side effect）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-APP-011.md`

- TaskId: `TASK-SCENE-019`
  Title: `M17 Scene script access bridge`
  Priority: `P1`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-02 15:00`
  Status: `Review`
  ModuleAttributionCheck: `pass`
  Summary:
    - Added narrow self-transform script bridge bound to one runtime object
    - Removed M15 default rotation smoke behavior from runtime update
    - Kept `Engine.Scene` free of `Engine.Scripting` dependency
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `src/Engine.Scene/Runtime/RuntimeScene.cs`
    - `src/Engine.Scene/Runtime/SceneScriptObjectHandle.cs`
    - `src/Engine.Scene/Runtime/SceneScriptObjectBindFailure.cs`
    - `src/Engine.Scene/Runtime/SceneScriptObjectBindFailureKind.cs`
    - `src/Engine.Scene/Runtime/SceneScriptObjectBindResult.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-019.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（Engine.Scene.Tests 52 条通过；Engine.Scripting.Tests 10 条通过）
    - Smoke: pass（self-transform bridge 修改可被 snapshot/render frame 观察；默认 update 不再旋转）
    - Perf: pass（无逐帧 JSON、任意对象查询或 render side effect）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-019.md`

- TaskId: `TASK-SDATA-008`
  Title: `M17 SceneData Script component schema and validation`
  Priority: `P0`
  PrimaryModule: `Engine.SceneData`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `Exec-SceneData`
  ClosedAt: `2026-05-02 14:09`
  Status: `Review`
  ModuleAttributionCheck: `pass`
  Summary:
    - Added repeatable `Script` component schema with `scriptId` and number/bool/string properties
    - Added normalized `SceneScriptComponentDescription` and preserved Script component order
    - Updated default sample scene with `RotateSelf` declaration without runtime binding
  FilesChanged:
    - `src/Engine.SceneData/FileModel/SceneFileComponentDefinition.cs`
    - `src/Engine.SceneData/FileModel/SceneFileScriptPropertyValue.cs`
    - `src/Engine.SceneData/Descriptions/SceneComponentDescription.cs`
    - `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
    - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
    - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
    - `src/Engine.App/SampleScenes/default.scene.json`
    - `.ai-workflow/boundaries/engine-scenedata.md`
    - `.ai-workflow/tasks/task-sdata-008.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（Engine.SceneData.Tests 37 条通过）
    - Smoke: pass（Script serialize/deserialize、order preservation、failure paths、sample declaration 覆盖）
    - Perf: pass（无 registry 查询、反射绑定、逐帧 JSON 或跨模块 runtime 查询）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SDATA-008.md`

- TaskId: `TASK-SCRIPT-001`
  Title: `M17 Engine.Scripting module and script lifecycle foundation`
  Priority: `P0`
  PrimaryModule: `Engine.Scripting`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scripting.md`
  Owner: `Exec-Scripting`
  ClosedAt: `2026-05-02 14:04`
  Status: `Review`
  ModuleAttributionCheck: `pass`
  Summary:
    - Added `Engine.Scripting` module and tests to the solution
    - Implemented built-in script registry, context/property model, lifecycle runtime, and explicit diagnostic failures
    - Added narrow self-transform access surface without external DLL/source compilation/hot reload support
  FilesChanged:
    - `src/Engine.Scripting/**`
    - `tests/Engine.Scripting.Tests/**`
    - `AnsEngine.sln`
    - `.ai-workflow/boundaries/engine-scripting.md`
    - `.ai-workflow/tasks/task-script-001.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（Engine.Scripting.Tests 10 条通过）
    - Smoke: pass（register/create/lifecycle/failure/self-transform mutation 覆盖）
    - Perf: pass（无逐帧程序集扫描、源码编译、全场景查询或 JSON 解析）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCRIPT-001.md`

- TaskId: `TASK-EAPP-008`
  Title: `M16 Inspector component groups integration`
  Priority: `P1`
  PrimaryModule: `Engine.Editor.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
  Owner: `Exec-EditorApp`
  ClosedAt: `2026-05-02 12:43`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Inspector migrated to `Object` / `Transform` / `MeshRenderer` component groups
    - GUI submissions route through component session APIs and preserve Transform-only objects
    - Editor.App tests/fixtures migrated to M16 `version: "2.0"` component arrays
  FilesChanged:
    - `src/Engine.Editor.App/EditorInspectorSnapshot.cs`
    - `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
    - `src/Engine.Editor.App/EditorInspectorInputState.cs`
    - `src/Engine.Editor.App/EditorGuiRenderer.cs`
    - `src/Engine.Editor.App/EditorAppController.cs`
    - `src/Engine.Editor.App/EditorDefaultObjectFactory.cs`
    - `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
    - `tests/Engine.Editor.App.Tests/EditorInspectorInputStateTests.cs`
    - `tests/Engine.Editor.App.Tests/EditorObjectWorkflowStateTests.cs`
    - `tests/Engine.Editor.App.Tests/EditorFileWorkflowStateTests.cs`
    - `.ai-workflow/boundaries/engine-editor-app.md`
    - `.ai-workflow/tasks/task-eapp-008.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（Engine.Editor.App.Tests 33 条通过）
    - Smoke: pass（auto-exit Editor.App run 退出码 0；Transform-only Inspector 空状态与不自动补 MeshRenderer 已覆盖）
    - Perf: pass（无逐帧文件写入或 sample scene reload polling）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-EAPP-008.md`

- TaskId: `TASK-EDITOR-005`
  Title: `M16 Editor component API migration`
  Priority: `P1`
  PrimaryModule: `Engine.Editor`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor.md`
  Owner: `Exec-Editor`
  ClosedAt: `2026-05-02 12:37`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneEditorSession` headless edit path migrated to component operations for Transform and MeshRenderer
    - Transform-only objects can open/select/save without auto-adding MeshRenderer
    - Default add-object factory creates Transform + MeshRenderer; GUI Inspector migration deferred to `TASK-EAPP-008`
  FilesChanged:
    - `src/Engine.Editor/Session/SceneEditorSession.cs`
    - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
    - `.ai-workflow/boundaries/engine-editor.md`
    - `.ai-workflow/tasks/task-editor-005.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（Engine.Editor.Tests 30 条通过）
    - Smoke: pass（Transform-only object 可打开、选择、保存且不自动补 MeshRenderer；新增对象默认可见）
    - Perf: pass（无逐帧 IO 或 GUI 依赖渗入）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-EDITOR-005.md`

- TaskId: `TASK-SCENE-018`
  Title: `M16 Scene runtime component bridge`
  Priority: `P1`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-02 11:48`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `RuntimeScene.LoadFromDescription(...)` 从 normalized component descriptions 构建 runtime Transform/MeshRenderer components
    - Transform-only object 进入 runtime snapshot，`HasMeshRenderer=false`，且不进入 render items
    - M15 默认旋转仍只选择首个同时具备 Transform 与 MeshRenderer 的可渲染 object
  FilesChanged:
    - `src/Engine.Scene/Runtime/RuntimeScene.cs`
    - `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
    - `src/Engine.Scene/Runtime/SceneMeshRendererComponent.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-018.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（Scene.Tests 48 条通过；Render.Tests 18 条通过；App.Tests 9 条通过）
    - Smoke: pass（Transform-only snapshot/render filtering/update behavior 均有覆盖）
    - Perf: pass（无逐帧 JSON/file IO、对象重建或 render side effect）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-018.md`

- TaskId: `TASK-QA-017`
  Title: `M16 Component Serialization Bridge 门禁复验与归档`
  Priority: `P3`
  PrimaryModule: `QA`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-05-02 15:00`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - M16 全量 Build/Test 与 SceneData/Scene/Editor/Editor.App/App/Render 回归通过
    - 确认 `2.0` component array schema、normalized component model、runtime bridge、Editor headless component API 与 Inspector component groups 全链路打通
    - Transform-only object 可加载/编辑/保存但不渲染，且未越界到脚本/物理/动画/Prefab/Play Mode
  FilesChanged:
    - `.ai-workflow/boundaries/engine-scenedata.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-editor.md`
    - `.ai-workflow/boundaries/engine-editor-app.md`
    - `.ai-workflow/tasks/task-qa-017.md`
    - `.ai-workflow/archive/2026-05/TASK-QA-017.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（沿用 `TASK-SDATA-006`、`TASK-SDATA-007`、`TASK-SCENE-018`、`TASK-EDITOR-005`、`TASK-EAPP-008` 的构建通过证据）
    - Test: pass（沿用 SceneData/Scene/Editor/Editor.App/App/Render 相关测试通过证据）
    - Smoke: pass（综合 `2.0` sample scene 运行、Transform-only object load/edit/save but not render、Inspector component groups、headless app smoke 与 Editor.App auto-exit 证据）
    - Perf: pass（无新增逐帧 JSON 解析、双重 normalize、自动补组件轮询或 render side effect）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-QA-017.md`

- TaskId: `TASK-SCENE-013`
  Title: `M14 RuntimeScene 到 SceneRenderFrame 输出`
  Priority: `P2`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-01 14:18`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 回流修复 `Architecture/LegacyPath`，移除 legacy render item list 与逐帧 demo frame generator
    - `BuildRenderFrame` 无条件从 `RuntimeScene` runtime objects/components 输出 render frame
    - `AddRootNode` 改为写入 runtime components，RuntimeScene 成为单一 render-frame 状态源
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（Scene.Tests 30 条通过；Render.Tests 16 条通过；App.Tests 6 条通过；整解测试通过）
    - Smoke: pass（render frame 来自 RuntimeScene 单一状态源，runtime component 修改可反映到 frame）
    - Boundary: pass（legacy path 符号无残留，未新增禁止依赖）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-013.md`

- TaskId: `TASK-SCENE-012`
  Title: `M14 SceneDescription 到 RuntimeScene 映射`
  Priority: `P1`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-01 05:51`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `RuntimeScene.LoadFromDescription` 映射 objects/components/camera
    - `SceneGraphService.LoadSceneDescription` 改以 runtime scene 为主状态源
    - 重复 load 清空旧 runtime state，NodeId 从 1 稳定分配
  FilesChanged:
    - `src/Engine.Scene/Runtime/RuntimeScene.cs`
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 22 条通过；整解测试通过）
    - Smoke: pass（空/单/多对象 scene 可映射，重复 load 清空旧 runtime state）
    - Boundary: pass（未新增 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL 依赖）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-012.md`

- TaskId: `TASK-SCENE-011`
  Title: `M14 Transform/MeshRenderer/Camera 组件`
  Priority: `P1`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-01 05:49`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 transform、mesh renderer、camera runtime state 组件
    - 支持 description 到 runtime component/camera state 映射
    - 默认 camera 语义保持当前行为兼容
  FilesChanged:
    - `src/Engine.Scene/Runtime/SceneTransformComponent.cs`
    - `src/Engine.Scene/Runtime/SceneMeshRendererComponent.cs`
    - `src/Engine.Scene/Runtime/SceneCameraRuntimeState.cs`
    - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 19 条通过；整解测试通过）
    - Smoke: pass（transform/mesh/material/camera 映射均通过测试，默认 camera 与当前行为一致）
    - Boundary: pass（未新增 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL 依赖）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-011.md`

- TaskId: `TASK-SCENE-010`
  Title: `M14 Runtime Object 基础模型`
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-01 05:46`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 internal `SceneRuntimeObject` 与 `RuntimeScene`
    - `SceneGraphService.NodeCount` 改从 runtime scene object count 返回
    - 重新 load scene 时旧 runtime objects 清空并重建
  FilesChanged:
    - `src/Engine.Scene/Runtime/SceneRuntimeObject.cs`
    - `src/Engine.Scene/Runtime/RuntimeScene.cs`
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`；Scene.Tests 14 条通过；整解测试通过）
    - Smoke: pass（重新 load scene 后 runtime object count 清空并重建，`NodeCount` 与 runtime scene object count 一致）
    - Boundary: pass（未新增 Render/Asset/App/Editor/OpenTK/ImGui/OpenGL 依赖）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-010.md`

- TaskId: `TASK-QA-014`
  Title: `M13 最小 GUI 编辑器门禁复验与归档`
  Priority: `P3`
  PrimaryModule: `QA`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-05-01 10:00`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 复验 `TASK-EAPP-001~007` 的 GUI 宿主、布局、Hierarchy、Inspector、Open/Save/Save As、Add/Remove 与固定布局收口结果
    - 确认最小 GUI 编辑器可启动、可编辑、可保存，并保持 `Engine.App` 继续作为运行时入口
    - 按人工验收结果完成 M13 任务卡、看板与归档索引收口
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-014.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-014.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（沿用 `TASK-EAPP-001~007` 的构建通过证据）
    - Test: pass（沿用 `TASK-EAPP-001~007` 的测试通过证据）
    - Smoke: pass（综合 GUI 启动、对象选择、Inspector 编辑、Open/Save/Save As、Add/Remove 与固定布局 smoke 证据）
    - Perf: pass（无新增逐帧文件 IO、重复 session open 或热重载轮询）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-014.md`

- TaskId: `TASK-EAPP-007`
  Title: `M13 Docked Editor Layout`
  Priority: `P3`
  PrimaryModule: `Engine.Editor.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
  Owner: `Exec-EditorApp`
  ClosedAt: `2026-05-01 03:24`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - Editor GUI 收敛为固定停靠布局
    - Toolbar 顶部、Hierarchy 左侧、Inspector 右侧、Status Bar 底部稳定可见
    - 布局尺寸进入 GUI snapshot 并补充测试
  FilesChanged:
    - `src/Engine.Editor.App/EditorGuiSnapshot.cs`
    - `src/Engine.Editor.App/EditorGuiSnapshotFactory.cs`
    - `src/Engine.Editor.App/EditorGuiRenderer.cs`
    - `tests/Engine.Editor.App.Tests/**`
    - `.ai-workflow/boundaries/engine-editor-app.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`，`Engine.Editor.App.Tests` 31 条通过）
    - Smoke: pass（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`，ExitCode=0）
    - Boundary: pass（未改 `Engine.Editor`、`SceneData`、`Engine.App` 或 `Render`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EAPP-007.md`

- TaskId: `TASK-QA-013`
  Title: `M12 GUI 编辑器前置底座门禁复验与归档`
  Priority: `P3`
  PrimaryModule: `Engine.Editor`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-30 01:21`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 复验 `TASK-EDITOR-001~004` 的 M12 Editor core 交付结果
    - 确认打开、编辑、保存、reload 闭环和边界约束均通过
    - 完成 M12 看板、任务归档索引与计划归档状态收口
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-013.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-013.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/plan-archive/2026-04/PLAN-M12-2026-04-30.md`
    - `.ai-workflow/plan-archive/plan-archive-index.md`
  ValidationEvidence:
    - Build: pass（`dotnet test AnsEngine.sln --no-restore` 完成构建与测试；仅既有 `net7.0` EOL warning）
    - Test: pass（整解测试通过；Editor.Tests 26 条通过；Render.Tests 16 条专项通过）
    - Smoke: pass（M12 已覆盖 `open -> edit -> save -> reload`；保存成功清 dirty；失败路径保留内存状态）
    - Boundary: pass（`Engine.Editor` 保持 headless core；未接入 App 默认启动路径，Render 不感知 Editor）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-013.md`

- TaskId: `TASK-EDITOR-004`
  Title: `M12 保存、另存为与 reload 验证`
  Priority: `P3`
  PrimaryModule: `Engine.Editor`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor.md`
  Owner: `Exec-Editor`
  ClosedAt: `2026-04-30 01:07`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneEditorSession` 新增保存与另存为入口
    - 保存成功必须经过写盘后 reload/normalize 验证
    - 保存失败或 reload 失败保留内存修改、路径和 dirty
  FilesChanged:
    - `src/Engine.Editor/Session/SceneEditorSession.cs`
    - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
    - `.ai-workflow/boundaries/engine-editor.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln` 通过，仅既有 `net7.0` EOL warning）
    - Test: pass（`dotnet test AnsEngine.sln` 通过，Editor.Tests 26 条通过）
    - Smoke: pass（`open -> edit -> save -> reload` 成功；保存成功清 dirty；失败保留 dirty 与内存修改）
    - Boundary: pass（未新增禁止依赖，未改 SceneData/App/Render/Platform/Asset）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EDITOR-004.md`

- TaskId: `TASK-EDITOR-003`
  Title: `M12 编辑命令编排与 selection/dirty 语义`
  Priority: `P2`
  PrimaryModule: `Engine.Editor`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor.md`
  Owner: `Exec-Editor`
  ClosedAt: `2026-04-30 01:04`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneEditorSession` 新增 selection 与对象编辑命令编排入口
    - 编辑成功同步 document/scene 并置 dirty
    - 选择、selection 跟随和编辑失败回滚语义已测试覆盖
  FilesChanged:
    - `src/Engine.Editor/Session/SceneEditorSession.cs`
    - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
    - `.ai-workflow/boundaries/engine-editor.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln` 通过，仅既有 `net7.0` EOL warning）
    - Test: pass（`dotnet test AnsEngine.sln` 通过，Editor.Tests 21 条通过）
    - Smoke: pass（无文档 select/edit 显式失败；选择不置 dirty；编辑成功置 dirty；失败不污染状态）
    - Boundary: pass（未新增禁止依赖，未改 SceneData/App/Render/Platform/Asset）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EDITOR-003.md`

- TaskId: `TASK-EDITOR-002`
  Title: `M12 SceneEditorSession 打开场景与会话状态`
  Priority: `P1`
  PrimaryModule: `Engine.Editor`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor.md`
  Owner: `Exec-Editor`
  ClosedAt: `2026-04-30 00:56`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneEditorSession` 支持打开、关闭、状态查询和 reload validate
    - 打开成功持有文档快照与规范化场景快照
    - 打开失败不污染已有 session
  FilesChanged:
    - `src/Engine.Editor/Session/SceneEditorSession.cs`
    - `tests/Engine.Editor.Tests/SceneEditorSessionTests.cs`
    - `.ai-workflow/boundaries/engine-editor.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln` 通过，仅既有 `net7.0` EOL warning）
    - Test: pass（`dotnet test AnsEngine.sln` 通过，Editor.Tests 11 条通过）
    - Smoke: pass（打开合法 scene 后状态正确，关闭后清空）
    - Perf: pass（仅显式打开阶段 IO/normalize，无逐帧轮询）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EDITOR-002.md`

- TaskId: `TASK-EDITOR-001`
  Title: `M12 Engine.Editor 模块与边界合同落地`
  Priority: `P0`
  PrimaryModule: `Engine.Editor`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor.md`
  Owner: `Exec-Editor`
  ClosedAt: `2026-04-30 00:50`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `Engine.Editor` 与 `Engine.Editor.Tests` 工程并接入 solution
    - 新增 `SceneEditorSession` 与显式结果/失败类型种子
    - 边界测试确认无禁止依赖、无 OpenTK/OpenGL 路线
  FilesChanged:
    - `AnsEngine.sln`
    - `src/Engine.Editor/**`
    - `tests/Engine.Editor.Tests/**`
    - `.ai-workflow/boundaries/engine-editor.md`
    - `.ai-workflow/boundaries/README.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln` 通过，仅既有 `net7.0` EOL warning）
    - Test: pass（`dotnet test AnsEngine.sln` 通过，Editor.Tests 4 条通过）
    - Smoke: pass（solution 可加载新模块，边界测试确认无禁止依赖）
    - Perf: pass（未改运行时路径，仅新增编译与测试成本）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EDITOR-001.md`

- TaskId: `TASK-QA-012`
  Title: `M11 SceneData 编辑底座门禁复验与收口`
  Priority: `P1`
  PrimaryModule: `QA`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `QA`
  ClosedAt: `2026-04-26 16:00`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 复验 `TASK-SDATA-003/004/005` 的构建、测试、smoke 与失败语义证据
    - 确认 M11 SceneData 编辑底座链路通过人工验收并完成归档收口
  FilesChanged:
    - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
    - `.ai-workflow/tasks/task-qa-012.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-012.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（沿用 `TASK-SDATA-003/004/005` 的 SceneData 构建证据）
    - Test: pass（沿用 `Engine.SceneData.Tests` 28 条通过证据）
    - Smoke: pass（覆盖保存后 reload、对象编辑后 reload 与失败语义）
    - Perf: pass（显式调用路径，无运行时逐帧 IO）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-012.md`

- TaskId: `TASK-SDATA-005`
  Title: `M11 对象级文档编辑操作与失败语义`
  Priority: `P2`
  PrimaryModule: `Engine.SceneData`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `Exec-SceneData`
  ClosedAt: `2026-04-28 01:00`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增文档级对象编辑服务与显式编辑失败语义
    - 支持对象增删、id/name、mesh/material 与 transform 修改
    - 编辑后文档可保存并重新加载为 `SceneDescription`
  FilesChanged:
    - `src/Engine.SceneData/Editing/**`
    - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
    - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
    - `.ai-workflow/boundaries/engine-scenedata.md`
  ValidationEvidence:
    - Build: pass（SceneData Debug/Release 构建通过）
    - Test: pass（SceneData.Tests 28 条通过）
    - Smoke: pass（编辑后保存并 reload 为 `SceneDescription`）
    - Perf: pass（文档级小步编辑，无运行时逐帧 IO）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SDATA-005.md`

- TaskId: `TASK-SDATA-004`
  Title: `M11 校验复用与 load-save-load 往返稳定`
  Priority: `P1`
  PrimaryModule: `Engine.SceneData`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `Exec-SceneData`
  ClosedAt: `2026-04-28 01:00`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 抽出 `SceneFileDocumentNormalizer` 统一校验、默认值与规范化规则
    - `JsonSceneDescriptionLoader` 复用 document store 与 normalizer
    - 保存后的 `.scene.json` 可重新加载为语义等价 `SceneDescription`
  FilesChanged:
    - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
    - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
    - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
    - `.ai-workflow/boundaries/engine-scenedata.md`
  ValidationEvidence:
    - Build: pass（SceneData Debug/Release 构建通过）
    - Test: pass（SceneData.Tests 28 条通过）
    - Smoke: pass（load-save-load 等价测试通过）
    - Perf: pass（复用 normalizer，未引入多套重复规范化主路径）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SDATA-004.md`

- TaskId: `TASK-SDATA-003`
  Title: `M11 SceneData 文档读写接口与 JSON store`
  Priority: `P0`
  PrimaryModule: `Engine.SceneData`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `Exec-SceneData`
  ClosedAt: `2026-04-28 01:00`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `ISceneDocumentStore` 与 JSON document store
    - `SceneFileDocument` 支持显式读取、保存与读写失败语义
    - 保持 `ISceneDescriptionLoader` 作为运行时规范化加载入口
  FilesChanged:
    - `src/Engine.SceneData/Abstractions/ISceneDocumentStore.cs`
    - `src/Engine.SceneData/DocumentStore/**`
    - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
    - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
    - `.ai-workflow/boundaries/engine-scenedata.md`
  ValidationEvidence:
    - Build: pass（SceneData Debug/Release 构建通过）
    - Test: pass（SceneData.Tests 28 条通过）
    - Smoke: pass（`SceneFileDocument` 保存后可读取）
    - Perf: pass（读写仅由显式 document store 操作触发）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SDATA-003.md`

- TaskId: `TASK-QA-011`
  Title: `M10 数据驱动场景链路门禁复验`
  Priority: `P2`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-26 16:00`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 复核 M10 数据驱动场景链路的 Build/Test/Smoke/Perf 与边界职责
    - 汇总 `TASK-SDATA-002`、`TASK-SCENE-009`、`TASK-APP-009` 的测试与样例运行证据
    - Human 于 `2026-04-26` 完成 M10 全量人工验收并批准关单
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-011.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-011.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（Human 于 `2026-04-26` 确认 M10 全链路验收通过）
    - Test: pass（SceneData/Scene/App 对应测试已覆盖描述加载、运行时映射与装配路径）
    - Smoke: pass（样例场景 JSON 已完成加载、初始化并稳定退出）
    - Perf: pass（初始化阶段一次性加载与映射，无逐帧解析或重载）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-011.md`

- TaskId: `TASK-APP-009`
  Title: `M10 场景文件选择与 SceneData loader 装配`
  Priority: `P1`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-26 15:45`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.App` 新增 `SceneData` loader 装配，并在启动路径中显式消费 `SceneDescriptionLoadResult`
    - 新增默认样例场景文件与 `ANS_ENGINE_SCENE_PATH` 覆盖入口，修改场景文件即可影响运行结果
    - 启动时先加载场景描述、再初始化 Scene；加载失败显式收口，不再隐式回退到硬编码场景
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `src/Engine.App/Engine.App.csproj`
    - `src/Engine.App/SceneRuntimeContracts.cs`
    - `src/Engine.App/SampleScenes/default.scene.json`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/boundaries/engine-scenedata.md`
  ValidationEvidence:
    - Build: pass（`/Users/ans/.dotnet/dotnet build src/Engine.App/Engine.App.csproj -c Debug` 与 `-c Release --no-restore`）
    - Test: pass（`/Users/ans/.dotnet/dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --nologo -v minimal --no-restore`；6 条通过）
    - Smoke: pass（headless 启动路径已成功加载默认场景并稳定退出）
    - Perf: pass（场景文件仅在启动阶段加载，无重复 loader 创建或场景重载）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-009.md`

- TaskId: `TASK-SCENE-009`
  Title: `M10 SceneDescription 到运行时场景初始化入口`
  Priority: `P1`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-26 15:28`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.Scene` 新增对 `Engine.SceneData` 的依赖，并在 `SceneGraphService` 中新增 `LoadSceneDescription(SceneDescription)` 初始化入口
    - `SceneDescription` 中的对象、材质、`LocalTransform` 与相机现在可稳定映射为 `SceneRenderFrame` 输出
    - 补齐描述驱动初始化、多对象稳定输出和默认相机回退测试，保持 Scene 不做 JSON 解析或文件 IO
  FilesChanged:
    - `src/Engine.Scene/Engine.Scene.csproj`
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-scenedata.md`
  ValidationEvidence:
    - Build: pass（`/Users/ans/.dotnet/dotnet build src/Engine.Scene/Engine.Scene.csproj -c Debug/Release --nologo -v minimal`）
    - Test: pass（`/Users/ans/.dotnet/dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --nologo -v minimal`；11 条通过）
    - Smoke: pass（样例式 `SceneDescription` 已被成功映射为 `SceneRenderFrame`）
    - Perf: pass（初始化时一次性映射；稳定帧循环不重复解析场景描述）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-009.md`

- TaskId: `TASK-SDATA-002`
  Title: `M10 场景 JSON 描述模型、加载与规范化`
  Priority: `P0`
  PrimaryModule: `Engine.SceneData`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `Exec-SceneData`
  ClosedAt: `2026-04-26 15:12`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `JsonSceneDescriptionLoader`，在 `Engine.SceneData` 内完成 `JSON -> FileModel -> 校验/默认值/规范化 -> SceneDescriptionLoadResult` 主路径
    - 保持 `FileModel` 与 `Descriptions` 双层结构，并以显式失败结果表达缺失文件、非法 JSON、重复对象 `id` 与默认值规范化语义
    - 补齐样例场景 JSON 与回归测试，覆盖合法加载、非法 JSON、缺失 `mesh`、重复对象 `id`、默认材质、默认 transform 与默认相机回填
  FilesChanged:
    - `src/Engine.SceneData/Loading/JsonSceneDescriptionLoader.cs`
    - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
    - `tests/Engine.SceneData.Tests/SampleScenes/sample.scene.json`
    - `tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj`
    - `.ai-workflow/boundaries/engine-scenedata.md`
    - `.ai-workflow/tasks/task-sdata-002.md`
    - `.ai-workflow/archive/2026-04/TASK-SDATA-002.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`/Users/ans/.dotnet/dotnet build src/Engine.SceneData/Engine.SceneData.csproj -c Debug/Release --nologo -v minimal`）
    - Test: pass（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --nologo -v minimal`；12 条通过）
    - Smoke: pass（样例场景 JSON 已被成功加载为 `SceneDescription`）
    - Perf: pass（加载阶段一次性解析，无逐帧 JSON 反序列化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SDATA-002.md`

- TaskId: `TASK-SDATA-001`
  Title: `M10 SceneData 模块与边界落地`
  Priority: `P0`
  PrimaryModule: `Engine.SceneData`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `Exec-SceneData`
  ClosedAt: `2026-04-26 00:00`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新建独立 `Engine.SceneData` 模块并接入 solution，固定 `SceneData -> Contracts` 依赖方向
    - 落地最小公开入口与描述模型占位，为后续 JSON 主路径提供稳定落点
    - 补齐最小模块依赖测试与边界文档，确认不反向耦合运行时层
  FilesChanged:
    - `AnsEngine.sln`
    - `src/Engine.SceneData/**`
    - `tests/**`
    - `.ai-workflow/boundaries/engine-scenedata.md`
    - `.ai-workflow/boundaries/README.md`
  ValidationEvidence:
    - Build: pass（Human 于 `2026-04-26` 确认 M10.1 模块落地验收通过）
    - Test: pass（Human 于 `2026-04-26` 确认最小模块依赖与边界测试通过）
    - Smoke: pass（组合根可引用 `SceneData` loader 抽象且未破坏现有运行路径）
    - Perf: pass（模块引入后未增加逐帧初始化或运行期开销）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SDATA-001.md`

- TaskId: `TASK-QA-010`
  Title: `M9 真实 mesh 资产链路门禁复验`
  Priority: `P2`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-25 18:34`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 复核 M9 真实磁盘 mesh 主链路的 Build/Test/Smoke/Perf 与边界职责
    - 汇总 Asset、Render、Scene、App 相关测试与样例运行证据
    - Human 于 `2026-04-25` 完成 M9 全量人工验收并批准关单
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-010.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-010.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（Human 于 `2026-04-25` 确认 M9 全链路验收通过）
    - Test: pass（Asset/Render/Scene/App 对应测试已覆盖导入、cache、fallback 与装配路径）
    - Smoke: pass（真实磁盘 mesh 链路运行并稳定退出）
    - Perf: pass（Asset provider 缓存与 Render GPU cache 语义已覆盖）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-010.md`

- TaskId: `TASK-APP-008`
  Title: `M9 Mesh provider 装配与样例运行路径接线`
  Priority: `P1`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-25 18:33`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `DiskMeshAssetProvider` 与 sample mesh 资源目录装配
    - 主循环前预热 bootstrap mesh 解析，保证 headless/真实窗口路径都进入真实磁盘 mesh 主链路
    - 保持 App 只做装配，不承载 OBJ 或 GPU 逻辑
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `src/Engine.App/Engine.App.csproj`
    - `src/Engine.App/SampleAssets/cube.obj`
    - `src/Engine.App/SampleAssets/mesh-catalog.txt`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/boundaries/engine-app.md`
  ValidationEvidence:
    - Build: pass（Human 于 `2026-04-25` 确认 M9 App 装配路径验收通过）
    - Test: pass（`tests/Engine.App.Tests` 覆盖 provider 注入与异常收口）
    - Smoke: pass（真实 mesh 主链路可在 headless/真实窗口路径启动并稳定退出）
    - Perf: pass（装配与预热仅发生在启动阶段）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-008.md`

- TaskId: `TASK-SCENE-008`
  Title: `M9 Scene 真实 mesh 引用收敛`
  Priority: `P1`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-25 18:32`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneGraphService` 默认输出真实 `mesh://cube` 引用
    - 多对象路径保留 `mesh://missing` 以供下游 fallback 验证
    - 保持 Scene 仅输出稳定 `meshId`，不泄漏路径或导入器语义
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
  ValidationEvidence:
    - Build: pass（Human 于 `2026-04-25` 确认 Scene M9 验收通过）
    - Test: pass（`tests/Engine.Scene.Tests` 覆盖真实/共享/缺失 mesh 引用语义）
    - Smoke: pass（新引用语义未破坏下游运行路径）
    - Perf: pass（未引入逐帧路径解析或 IO）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-008.md`

- TaskId: `TASK-REND-013`
  Title: `M9 Render mesh provider 接入与 GPU cache 主路径`
  Priority: `P1`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-25 18:31`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - Render 接入 `IMeshAssetProvider` 并通过 mesh geometry cache 消费真实 mesh CPU 资产
    - 新增 GPU resource cache，保证共享 mesh 不重复创建 GPU 资源
    - 内置三角形降级为 provider 失败 fallback
  FilesChanged:
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `src/Engine.Render/SceneRenderMeshGeometry.cs`
    - `src/Engine.Render/SceneRenderMeshGeometryCache.cs`
    - `src/Engine.Render/SceneRenderGpuMeshResource.cs`
    - `src/Engine.Render/SceneRenderGpuMeshResourceCache.cs`
    - `tests/Engine.Render.Tests/NullRendererTests.cs`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `tests/Engine.Render.Tests/SceneRenderGpuMeshResourceCacheTests.cs`
    - `.ai-workflow/boundaries/engine-render.md`
  ValidationEvidence:
    - Build: pass（Human 于 `2026-04-25` 确认 Render M9 验收通过）
    - Test: pass（`tests/Engine.Render.Tests` 覆盖 provider 接入、fallback 与共享 mesh GPU cache）
    - Smoke: pass（真实磁盘 mesh 可被应用渲染链路稳定消费并退出）
    - Perf: pass（同 mesh 不重复创建 GPU 资源）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-013.md`

- TaskId: `TASK-ASSET-001`
  Title: `M9 OBJ 磁盘导入与 mesh CPU 资产缓存主路径`
  Priority: `P0`
  PrimaryModule: `Engine.Asset`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-asset.md`
  Owner: `Exec-Asset`
  ClosedAt: `2026-04-23 23:35`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `DiskMeshAssetProvider`，在 Asset 内完成 `meshId -> catalog -> OBJ -> MeshAssetData -> cache` 主路径
    - 新增 `MeshCatalog` 与 `ObjMeshFileLoader`，显式返回缺失、损坏与格式不支持等失败语义
    - 保持 `NullAssetService` 兼容，并补齐缓存命中与 headless 真实磁盘 mesh 专项测试
  FilesChanged:
    - `src/Engine.Asset/AssetHandle.cs`
    - `src/Engine.Asset/DiskMeshAssetProvider.cs`
    - `src/Engine.Asset/Engine.Asset.csproj`
    - `src/Engine.Asset/MeshCatalog.cs`
    - `src/Engine.Asset/NullAssetService.cs`
    - `src/Engine.Asset/ObjMeshFileLoader.cs`
    - `src/Engine.Asset/AssetPlaceholders.cs`
    - `tests/Engine.Asset.Tests/AssetServiceTests.cs`
    - `.ai-workflow/boundaries/engine-asset.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug --nologo` 与 `dotnet build AnsEngine.sln -c Release --nologo`）
    - Test: pass（`dotnet test AnsEngine.sln --nologo -v minimal`）
    - Smoke: pass（headless 专项测试通过，真实磁盘 mesh 已加载）
    - Perf: pass（重复请求命中 provider 缓存，不重复解析已缓存成功结果）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-ASSET-001.md`

- TaskId: `TASK-CONTRACT-005`
  Title: `M9 Mesh CPU 资产契约与失败语义定稿`
  Priority: `P0`
  PrimaryModule: `Engine.Contracts`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
  Owner: `Exec-Contracts`
  ClosedAt: `2026-04-23 01:14`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `MeshAssetVertex` 与 `MeshAssetData`，定义规范化 mesh CPU 数据模型
    - 新增 `IMeshAssetProvider`、`MeshAssetLoadResult` 与失败语义类型，收敛跨模块查询桥接面
    - 补齐契约测试与边界文档，为 M9 后续 Asset/Render/App 任务提供稳定输入面
  FilesChanged:
    - `src/Engine.Contracts/IMeshAssetProvider.cs`
    - `src/Engine.Contracts/MeshAssetData.cs`
    - `src/Engine.Contracts/MeshAssetLoadFailure.cs`
    - `src/Engine.Contracts/MeshAssetLoadFailureKind.cs`
    - `src/Engine.Contracts/MeshAssetLoadResult.cs`
    - `src/Engine.Contracts/MeshAssetVertex.cs`
    - `tests/Engine.Contracts.Tests/MeshAssetContractsTests.cs`
    - `.ai-workflow/boundaries/engine-contracts.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug --nologo` 与 `dotnet build AnsEngine.sln -c Release --nologo`）
    - Test: pass（`dotnet test AnsEngine.sln --nologo -v minimal`）
    - Smoke: pass（headless 启动/退出成功，ExitCode=0）
    - Perf: pass（契约层仅新增只读数据模型与显式结果类型，无逐帧 IO）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-005.md`

- TaskId: `TASK-BOOT-001`
  Title: 初始化 `AnsEngine` 多项目骨架
  Priority: `P1`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Codex`
  ClosedAt: `2026-04-06 20:33`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 建立 .NET 8 多项目骨架与 solution
    - 建立跨模块最小接口占位与组合根启动路径
    - 完成 Debug/Release build 与 test 门禁
  FilesChanged:
    - `AnsEngine.sln`
    - `src/**`
    - `tests/**`
    - `.ai-workflow/board.md`
    - `.ai-workflow/tasks/task-boot-001.md`
    - `.ai-workflow/archive/**`
  ValidationEvidence:
    - Build: pass（Debug/Release 均通过，存在环境级 CS1668 警告）
    - Test: pass（3 个测试项目各 1 条用例通过）
    - Smoke: N/A（未实现窗口循环）
    - Perf: N/A（未引入帧循环）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-BOOT-001.md`

- TaskId: `TASK-PLAT-001`
  Title: 真实窗口生命周期落地
  Priority: `P0`
  PrimaryModule: `Engine.Platform`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-platform.md`
  Owner: `Exec-Platform`
  ClosedAt: `2026-04-07 10:35`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 扩展 `IWindowService` 生命周期接口，补齐事件轮询、关闭请求和释放能力
    - 基于 OpenTK `NativeWindow` 实现真实窗口服务，替代纯占位行为
    - 提供 `useNativeWindow` 兼容开关，保障无图形环境测试稳定
    - 同步更新平台边界合同与 `Boundary Change Log`
  FilesChanged:
    - `src/Engine.Platform/PlatformContracts.cs`
    - `src/Engine.Platform/PlatformPlaceholders.cs`
    - `tests/Engine.Asset.Tests/AssetServiceTests.cs`
    - `.ai-workflow/boundaries/engine-platform.md`
  ValidationEvidence:
    - Build: pass（Debug/Release 均通过，存在环境级 CS1668 警告）
    - Test: pass（`dotnet test` 全部通过）
    - Smoke: pass（`dotnet run --project src/Engine.App/Engine.App.csproj` 成功退出）
    - Perf: pass（基础观察无卡死或阻塞）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-PLAT-001.md`

- TaskId: `TASK-APP-001`
  Title: 最小主循环与退出编排
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-07 11:10`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.App` 由单次 `Run` 升级为持续主循环并消费窗口关闭信号
    - 统一退出收口，保障渲染器与窗口服务按顺序释放
    - 增加自动退出开关用于 smoke/perf 自动化验证
    - 同步更新 `Engine.App` 边界合同与变更日志
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-001.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug` / `dotnet build -c Release`）
    - Test: pass（`dotnet test`）
    - Smoke: pass（自动退出验证，退出码 `0`）
    - Perf: pass（连续运行 30s+ 无明显阻塞）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-001.md`

- TaskId: `TASK-REND-001`
  Title: 最小清屏可视反馈
  Priority: `P1`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-07 12:20`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - `NullRenderer` 初始化设置非默认清屏色，每帧执行 `GL.Clear`，提供持续可见背景反馈
    - 保持最小补丁，不引入三角形/Shader/Mesh 复杂渲染链路
    - 同步更新 `Engine.Render` 边界合同与变更日志
  FilesChanged:
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/tasks/task-rend-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-001.md`
  ValidationEvidence:
    - Build: pass（Debug/Release 通过，存在环境级 CS1668 警告）
    - Test: pass（`dotnet test`）
    - Smoke: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=30` 下稳定退出，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=45` 下运行约 55s，无明显阻塞）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-001.md`

- TaskId: `TASK-QA-001`
  Title: 可见反馈门禁证据补齐
  Priority: `P1`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-07 13:10`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 复核并补齐 M2 阶段 Build/Test/Smoke/Perf 门禁证据
    - 明确记录“可启动、可见、可退出”执行步骤与退出码结果
    - 同步更新 `Engine.App` 边界合同变更日志（验证口径）
  FilesChanged:
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-qa-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-001.md`
  ValidationEvidence:
    - Build: pass（Debug/Release 均通过）
    - Test: pass（`dotnet test`）
    - Smoke: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=10` 下稳定退出，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=45` 下运行约 55s，无明显异常）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-001.md`

- TaskId: `TASK-REND-002`
  Title: 首帧三角形最小渲染链路
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-11 10:30`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - 任务经修复后再次提交人工验收，确认“稳定可见三角形”达成
    - 关闭流程完成：任务卡、归档快照、归档索引与看板状态已对齐
    - 保留回流与复验记录，满足审计追溯
  FilesChanged:
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/tasks/task-rend-002.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-002.md`
  ValidationEvidence:
    - Build: pass（Debug/Release 通过，存在环境级 CS1668 警告）
    - Test: pass（`dotnet test`，首次因锁文件失败后重试通过）
    - Smoke: pass（人工复验通过：稳定可见三角形并可正常退出）
    - Perf: pass（`ANS_ENGINE_AUTO_EXIT_SECONDS=45`，运行约 48.68s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-002.md`

- TaskId: `TASK-APP-002`
  Title: M3 运行装配与生命周期配套
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-11 10:45`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 将渲染初始化纳入统一 `try/finally`，初始化失败也可执行收口
    - 异常路径补充 `RequestClose`，避免窗口生命周期悬挂
    - 增加 `ANS_ENGINE_USE_NATIVE_WINDOW` 装配开关，默认仍为真实窗口路径
    - 新增 `HeadlessRenderer` 适配无图形环境 smoke/perf 验证
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-002.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-002.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，约 `15.64s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，约 `45.14s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-002.md`

- TaskId: `TASK-SCENE-002`
  Title: M4 最小场景渲染数据输出
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-11 11:40`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 场景提交数据增加最小动态变化（首节点材质按帧切换）
    - 保持 Scene-Render 契约稳定，不扩展跨模块依赖面
    - 补充场景输出测试并通过
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-002.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-002.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 4/4）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.15s，退出码 `0`）
    - Perf: pass（45.12s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-002.md`

- TaskId: `TASK-REND-004`
  Title: M4 场景驱动渲染消费
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-11 11:40`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 渲染模块改为消费 `SceneRenderFrame`，替代固定 demo 提交路径
    - 新增提交构建器并按场景提交动态更新 VBO 绘制
    - 新增渲染消费链路最小测试项目并通过
  FilesChanged:
    - `src/Engine.Render/Engine.Render.csproj`
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/Engine.Render.Tests.csproj`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/tasks/task-rend-004.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.15s，退出码 `0`）
    - Perf: pass（45.12s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-004.md`

- TaskId: `TASK-APP-003`
  Title: M4 提交流程编排配套
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-11 11:55`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 组合根注入 Scene 提交提供器到 Render，完成 M4 场景提交链路编排
    - 主循环保持编排职责边界，不引入渲染后端或场景内部实现
    - 同步更新 App 边界合同与任务归档资料
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-003.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-003.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: fail -> pass（`dotnet test -m:1` 首次 `CS2012`，清理进程后复跑通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.15s，退出码 `0`）
    - Perf: pass（45.15s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-003.md`

- TaskId: `TASK-QA-003`
  Title: M4 验证与关单收敛
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-11 12:05`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M4 Build/Test/Smoke/Perf 全量门禁复核与证据回填
    - 纳入 `Engine.Render.Tests` 独立测试链路验证并通过
    - 质量结论收敛：NoNewHighRisk=true，MustFixCount=0
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-003.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-003.md`
    - `.ai-workflow/boundaries/engine-app.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.14s，退出码 `0`）
    - Perf: pass（45.13s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-003.md`

- TaskId: `TASK-REND-005`
  Title: M4 渲染边界文档对齐当前依赖
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-13 20:38`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 对齐 Render 边界文档到当前真实依赖状态（当前为 `Engine.Render -> Engine.Scene`）
    - 修正 `Engine.Contracts` 依赖表述为后续目标态，消除文档与实现漂移
    - 记录后续解耦回切条件与路径
  FilesChanged:
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/tasks/task-rend-005.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-005.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，15.17s，退出码 `0`）
    - Perf: pass（30.17s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-005.md`

- TaskId: `TASK-QA-002`
  Title: M3 双轨门禁证据与归档收口
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-11 11:00`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M3 Build/Test/Smoke/Perf 证据复核与汇总
    - 对齐 `TASK-REND-002`（可视验收）与 `TASK-APP-002`（运行收口）的联合验收链
    - 完成归档三件套与看板 Review 推进，待 Human 最终关单
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-002.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-002.md`
    - `.ai-workflow/boundaries/engine-app.md`
  ValidationEvidence:
    - Build: fail -> pass（Debug 首次 `CS2012` 文件占用；复跑 Debug 通过；Release 通过）
    - Test: pass（`dotnet test -m:1`）
    - Smoke: pass（图形口径引用 `TASK-REND-002` 人工复验；当前环境口径 30.19s，退出码 `0`）
    - Perf: pass（45.24s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-002.md`

- TaskId: `TASK-SCENE-001`
  Title: M4 Scene-Render 最小契约定义
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-11 11:25`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 在 `Engine.Scene` 定义最小 Scene-Render 提交契约基线
    - `SceneGraphService` 输出只读提交快照，维持模块边界不侵入渲染实现
    - 新增契约测试并通过，形成 M4 后续任务统一依赖面
  FilesChanged:
    - `src/Engine.Scene/SceneRenderContracts.cs`
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-001.md`
  ValidationEvidence:
    - Build: fail -> pass（Debug 首次 `CS2012` 文件占用；复跑 Debug 通过；Release 通过）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 3/3 通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.22s，退出码 `0`）
    - Perf: pass（45.17s，退出码 `0`，无明显退化）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-001.md`

- TaskId: `TASK-CONTRACT-001`
  Title: M4 独立契约层建立与边界落盘
  Priority: `P0`
  PrimaryModule: `Engine.Contracts`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-13 21:00`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新建独立契约层 `src/Engine.Contracts` 并定义最小渲染输入契约类型
    - 新建 `Engine.Contracts.Tests` 并补充最小契约用例，验证契约可直接消费
    - 同步边界文档 `engine-contracts` 变更日志，完成归档三件套准备
  FilesChanged:
    - `AnsEngine.sln`
    - `src/Engine.Contracts/Engine.Contracts.csproj`
    - `src/Engine.Contracts/SceneRenderContracts.cs`
    - `tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj`
    - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/tasks/task-contract-001.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-CONTRACT-001.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Contracts.Tests` 2/2）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，22.82s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，37.83s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-001.md`

- TaskId: `TASK-SCENE-003`
  Title: M4 渲染输入契约下沉到独立层
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-14 00:40`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.Scene` 接入 `Engine.Contracts` 引用，建立 `Scene -> Contracts` 稳定依赖方向
    - `SceneGraphService` 完成双接口适配，内部以契约层类型输出并保持旧接口兼容
    - 新增契约接口链路测试并通过，同步更新 Scene 边界变更日志
  FilesChanged:
    - `src/Engine.Scene/Engine.Scene.csproj`
    - `src/Engine.Scene/SceneRenderContracts.cs`
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-003.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-003.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，24.74s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，62.53s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-003.md`

- TaskId: `TASK-REND-006`
  Title: M4 Render 依赖反转与解耦
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-14 00:49`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.Render` 编译期依赖由 `Engine.Scene` 切换到 `Engine.Contracts`
    - `Render` 提交构建链路统一消费契约层 `SceneRenderFrame/SceneRenderItem`
    - `Engine.Render.Tests` 切换契约层引用并验证消费路径通过
  FilesChanged:
    - `src/Engine.Render/Engine.Render.csproj`
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/Engine.Render.Tests.csproj`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-rend-006.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-006.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，Render 契约消费测试通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，20.68s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，35.66s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-006.md`

- TaskId: `TASK-APP-004`
  Title: M4 App 契约 Provider 装配
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-14 01:06`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `Engine.App` 显式装配 `Engine.Contracts.ISceneRenderContractProvider` 并注入 Render
    - 抽出可测试渲染器创建路径，规避 GLFW 主线程限制
    - 新增 `Engine.App.Tests` 覆盖装配路径并通过
  FilesChanged:
    - `AnsEngine.sln`
    - `src/Engine.App/Engine.App.csproj`
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `tests/Engine.App.Tests/Engine.App.Tests.csproj`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-004.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.App.Tests` 1/1）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，18.88s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，33.85s，退出码 `0`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-004.md`

- TaskId: `TASK-QA-004`
  Title: M4 解耦门禁与质量复验
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-14 01:28`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 “Render 不得直接引用 Scene” 的门禁复验并通过
    - 完成 Build/Test/Smoke/Perf 全量复核并回填证据
    - 质量结论收敛：NoNewHighRisk=true，MustFixCount=0
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-004.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，30.87s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，45.83s，退出码 `0`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-004.md`

- TaskId: `TASK-QA-005`
  Title: M4b MustFix 关口复验与双轨门禁收口
  Priority: `P0`
  PrimaryModule: `QA`
  BoundaryContractPath:
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-14 17:19`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成三张 MustFix 修复卡统一复验并形成关口收口证据
    - Build/Test/Smoke/Perf 全量门禁通过
    - 依赖门禁通过：Render 不直接引用 Scene，仅消费 Contracts
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-005.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-005.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，31.15s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，46.09s，退出码 `0`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-005.md`

- TaskId: `TASK-SCENE-004`
  Title: M4b Scene 单契约出口收敛
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-14 10:43`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Scene 渲染出口收敛为单一 `Engine.Contracts` 契约，移除双轨兼容层
    - 删除 `SceneRenderContracts.cs` 并清除 `FromContracts` 每帧转换路径
    - 保持场景输出行为一致，同时降低热路径分配与语义分叉风险
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `src/Engine.Scene/SceneRenderContracts.cs`（删除）
    - `.ai-workflow/tasks/task-scene-004.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，26.59s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，41.48s，退出码 `0`）
    - HotPathEvidence: pass（`src/Engine.Scene` 无 `FromContracts` 调用，仅单契约构建）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-004.md`

- TaskId: `TASK-REND-007`
  Title: M4b Render 默认回退 Provider 清理
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-14 15:24`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 移除 `NullRenderer` 默认回退 provider 构造入口，渲染器必须显式注入 `ISceneRenderContractProvider`
    - 删除 `DefaultSceneRenderContractProvider`，防止生产路径静默兜底掩盖装配遗漏
    - 新增漏注入失败测试，验证 provider 为 `null` 时快速失败并抛出可诊断异常
  FilesChanged:
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/NullRendererTests.cs`
    - `.ai-workflow/tasks/task-rend-007.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-007.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`，存在环境级 CS1668 警告）
    - Test: pass（`dotnet test -m:1`，`NullRendererTests` 通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`51.83s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ExitCode=0`，`67.36s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-007.md`

- TaskId: `TASK-APP-005`
  Title: M4b App 场景运行时抽象依赖修复
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-14 17:14`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 在 `Engine.App` 新增 `ISceneRuntime` 并将 `ApplicationHost` 改为依赖该接口
    - 组合根新增 `SceneRuntimeAdapter` 绑定 `SceneGraphService`，保持 Render 契约 provider 注入不变
    - 新增抽象驱动测试，验证主循环可在场景替身下完成初始化与退出
  FilesChanged:
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `src/Engine.App/SceneRuntimeContracts.cs`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/tasks/task-app-005.md`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-005.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.App.Tests` 2/2）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`18.63s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`32.37s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-005.md`

- TaskId: `TASK-CONTRACT-002`
  Title: M5 渲染变换契约兼容扩展
  Priority: `P0`
  PrimaryModule: `Engine.Contracts`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
  Owner: `Exec-Contracts`
  ClosedAt: `2026-04-15 13:46`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `SceneTransform`（Position/Scale/Rotation）并提供 `Identity`
    - `SceneRenderItem` 扩展 `Transform` 字段并保留旧三参构造默认 identity 兼容行为
    - 新增契约测试覆盖默认兼容路径与 Rotation 保真路径
  FilesChanged:
    - `src/Engine.Contracts/SceneRenderContracts.cs`
    - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
    - `.ai-workflow/tasks/task-contract-002.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-CONTRACT-002.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Contracts.Tests` 4/4）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`19.37s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`34.38s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-002.md`

- TaskId: `TASK-SCENE-005`
  Title: M5 Scene 变换渲染帧输出
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-15 19:32`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneGraphService` 输出项显式携带 `SceneTransform`，首帧保持 identity 兼容
    - 连续帧对首节点输出轻量 transform 动态（Position.X 与 Rotation.Yaw）
    - 场景测试补充 transform/rotation 验证并通过
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/tasks/task-scene-005.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-005.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Scene.Tests` 5/5）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`25.50s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`40.36s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-005.md`

- TaskId: `TASK-REND-008`
  Title: M5 Render 变换消费与提交应用
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-15 19:32`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 提交顶点应用 `Scale -> Rotation -> Translation` 变换
    - identity 路径保持旧布局兼容
    - 渲染测试新增 identity 回归与 rotation 生效断言并通过
  FilesChanged:
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/tasks/task-rend-008.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-008.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Render.Tests` 6/6）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`25.50s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`40.36s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-008.md`

- TaskId: `TASK-APP-006`
  Title: M5 App 装配兼容校准
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-15 15:23`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 保持 App 仅装配不计算边界，维持 `Scene -> Contracts -> Render` 链路
    - 补充装配测试校准，验证 provider 初始化后可输出含 rotation 的连续帧 transform
    - 复核 M5 链路下主循环生命周期稳定性
  FilesChanged:
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/tasks/task-app-006.md`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-006.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`）
    - Smoke: pass（`dotnet run --no-build` + `ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.80s`）
    - Perf: pass（`dotnet run --no-build` + `ANS_ENGINE_USE_NATIVE_WINDOW=false` + `ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.80s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-006.md`

- TaskId: `TASK-CONTRACT-003`
  Title: M6 相机与 MVP 最小契约扩展
  Priority: `P0`
  PrimaryModule: `Engine.Contracts`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
  Owner: `Exec-Contracts`
  ClosedAt: `2026-04-17 10:58`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `SceneCamera(View, Projection)` 契约并提供 identity 默认值
    - `SceneRenderFrame` 扩展 `Camera` 字段并保留旧双参构造兼容路径
    - 新增契约测试覆盖默认相机兼容与显式相机保真
  FilesChanged:
    - `src/Engine.Contracts/SceneRenderContracts.cs`
    - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
    - `.ai-workflow/tasks/task-contract-003.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-CONTRACT-003.md`
  ValidationEvidence:
    - Build: pass（`dotnet build -c Debug -m:1` / `dotnet build -c Release -m:1`）
    - Test: pass（`dotnet test -m:1`，`Engine.Contracts.Tests` 6/6）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`19.04s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`34.21s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-003.md`

- TaskId: `TASK-QA-006`
  Title: M5 变换链路门禁与回归复验（含 Rotation）
  Priority: `P1`
  PrimaryModule: `QA`
  BoundaryContractPath:
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-16 22:55`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M5 变换链路（Position/Scale/Rotation）门禁复验与兼容回归检查
    - Build/Test/Smoke/Perf 全量证据补齐，并补充 Render 专项测试
    - 依赖方向复验通过：Render 仅依赖 Contracts，不直接引用 Scene
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-006.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-006.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`，存在 MSB3101 写缓存告警）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1 --no-restore --no-build`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，31.45s，退出码 `0`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，46.25s，退出码 `0`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-006.md`

- TaskId: `TASK-REND-010`
  Title: M6 Mesh 数据统一入口收敛（已并入 TASK-REND-009）
  Priority: `P1`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-17 00:47`
  Status: `Cancelled`
  ModuleAttributionCheck: `pass`
  Summary:
    - 已并入 `TASK-REND-009`，不再作为独立执行卡
    - M6 Render 只保留一个主执行单元，mesh 数据统一入口收敛收进主卡范围
  FilesChanged:
    - `.ai-workflow/tasks/task-rend-010.md`
    - `.ai-workflow/tasks/task-rend-009.md`
    - `.ai-workflow/plan-archive/2026-04/PLAN-M6-2026-04-17.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: N/A（卡片合并，无独立实现）
    - Test: N/A（卡片合并，无独立实现）
    - Smoke: N/A（卡片合并，无独立实现）
    - Perf: N/A（卡片合并，无独立实现）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-010.md`

- TaskId: `TASK-QA-007`
  Title: M6 MVP 渲染链路门禁与回归复验
  Priority: `P1`
  PrimaryModule: `QA`
  BoundaryContractPath:
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-17 17:50`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M6 MVP 渲染链路（含 Camera 与 mesh 统一入口）的 Build/Test/Smoke/Perf 全量门禁复验
    - 回归验证通过：Render 仅依赖 Contracts、不直接依赖 Scene；MVP uniform 路径在渲染主链路生效
    - 边界文档与实现一致性复核通过，未发现新的高风险问题
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-007.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-007.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + M6 关键专项测试通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`16.22s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.80s`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-007.md`

- TaskId: `TASK-QA-009`
  Title: M7 门禁复验与关单收敛（含多对象与回退验证）
  Priority: `P1`
  PrimaryModule: `QA`
  BoundaryContractPath:
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-04-20 17:36`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 完成 M7 全链路 Build/Test/Smoke/Perf 门禁复验，结果均通过
    - 完成 mesh/material 解析、多对象与回退路径专项回归，结果均通过
    - 依赖与边界复验通过：Render 仅依赖 Contracts，Scene 仅输出资源标识并由 Render 统一解析
  FilesChanged:
    - `.ai-workflow/tasks/task-qa-009.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-QA-009.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`，环境级 `CS1668` 警告）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + fallback/multi-object 专项测试通过）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.61s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
    - DependencyGate: pass（RenderSceneRef=absent，RenderContractsRef=present）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-QA-009.md`

- TaskId: `TASK-SCENE-006`
  Title: M6 Scene 对象与相机语义输出
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-17 12:45`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Scene 输出帧新增 `SceneCamera(View/Projection)`，补齐最小真实相机语义
    - 保持 transform/material 动态输出，并新增轻量相机视图变化
    - 场景测试补充相机语义与有效性断言
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/tasks/task-scene-006.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-006.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.94s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-006.md`

- TaskId: `TASK-REND-009`
  Title: M6 Render MVP Uniform 渲染改造（合并 M6 Mesh 数据统一入口收敛）
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-17 12:45`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Render 提交链路改为“模型空间顶点 + MVP uniform”并由 shader 执行变换
    - 收敛 `MeshId` 统一 mesh 入口，吸收 `TASK-REND-010` 目标
    - Render 测试补齐 identity/rotation/camera/multi-batch 回归
  FilesChanged:
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `src/Engine.Render/RenderPlaceholders.cs`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/tasks/task-rend-009.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-009.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.94s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-009.md`

- TaskId: `TASK-APP-007`
  Title: M6 App 装配与生命周期校准
  Priority: `P0`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-04-17 13:20`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 保持 App “仅装配不计算”边界，M6 MVP 计算仍由 Scene/Render 消费
    - 装配测试新增相机语义校准断言（连续帧 `Camera.View` 变化）
    - 生命周期测试覆盖渲染异常收口（`RequestClose -> Shutdown -> Dispose`）
  FilesChanged:
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/tasks/task-app-007.md`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-APP-007.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.63s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-APP-007.md`

- TaskId: `TASK-CONTRACT-004`
  Title: M7 资源输入契约收敛
  Priority: `P0`
  PrimaryModule: `Engine.Contracts`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-contracts.md`
  Owner: `Exec-Contracts`
  ClosedAt: `2026-04-18 11:05`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增结构化资源引用契约 `SceneMeshRef` 与 `SceneMaterialRef`
    - `SceneRenderItem` 支持结构化资源构造并保留 `meshId/materialId` 兼容路径
    - 合同测试覆盖结构化资源兼容与非法标识拦截
  FilesChanged:
    - `src/Engine.Contracts/SceneResourceContracts.cs`
    - `src/Engine.Contracts/SceneRenderContracts.cs`
    - `tests/Engine.Contracts.Tests/SceneRenderContractsTests.cs`
    - `.ai-workflow/tasks/task-contract-004.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-CONTRACT-004.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Contracts.Tests/Engine.Contracts.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.79s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.58s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-CONTRACT-004.md`

- TaskId: `TASK-REND-011`
  Title: M7 Mesh 数据入口落地
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-18 00:58`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneRenderSubmissionBuilder` 主路径统一走 `ResolveMesh(meshId)`，收敛 mesh 数据解析入口
    - 未知 mesh 标识回退到默认三角形，避免渲染链路中断
    - Render 测试补充 mesh 命中与回退断言
  FilesChanged:
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/tasks/task-rend-011.md`
    - `.ai-workflow/tasks/task-rend-012.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-011.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-012.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.70s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-011.md`

- TaskId: `TASK-REND-012`
  Title: M7 Material 参数入口落地
  Priority: `P0`
  PrimaryModule: `Engine.Render`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-render.md`
  Owner: `Exec-Render`
  ClosedAt: `2026-04-18 00:58`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneRenderSubmissionBuilder` 新增 `ResolveMaterial(materialId)` 最小材质参数入口
    - 以显式映射替换 hash 派生颜色语义，支持可观察的材质差异
    - 对未知材质启用默认参数回退，保证渲染稳定性
  FilesChanged:
    - `src/Engine.Render/SceneRenderSubmission.cs`
    - `tests/Engine.Render.Tests/SceneRenderSubmissionBuilderTests.cs`
    - `.ai-workflow/tasks/task-rend-011.md`
    - `.ai-workflow/tasks/task-rend-012.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/boundaries/engine-render.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-011.md`
    - `.ai-workflow/archive/2026-04/TASK-REND-012.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1` + `dotnet test tests/Engine.Render.Tests/Engine.Render.Tests.csproj -m:1`）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`15.66s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.70s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-REND-012.md`

- TaskId: `TASK-SCENE-007`
  Title: M7 Scene 资源引用输出对齐
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-04-19 21:15`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Scene 资源输出新增候选值解析与回退，`meshId/materialId` 对齐 Render M7 已支持入口
    - 材质周期覆盖 `default/pulse/highlight`，缺失材质在 Scene 侧回退为 `default`
    - 多对象路径下缺失 mesh 不外泄，输出统一回退到 `mesh://triangle`
  FilesChanged:
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/tasks/task-scene-007.md`
    - `.ai-workflow/archive/2026-04/TASK-SCENE-007.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-contracts.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln -c Debug -m:1` / `dotnet build AnsEngine.sln -c Release -m:1`）
    - Test: pass（`dotnet test AnsEngine.sln -m:1`，`Engine.Scene.Tests` 7/7）
    - Smoke: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=15`，`ExitCode=0`，`16.07s`）
    - Perf: pass（`ANS_ENGINE_USE_NATIVE_WINDOW=false`，`ANS_ENGINE_AUTO_EXIT_SECONDS=30`，`ExitCode=0`，`30.84s`）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-SCENE-007.md`

- TaskId: `TASK-EAPP-001`
  Title: M13 Editor GUI 宿主入口
  Priority: `P0`
  PrimaryModule: `Engine.Editor.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
  Owner: `Exec-EditorApp`
  ClosedAt: `2026-04-30 14:28`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增独立 `Engine.Editor.App` 可执行宿主与 OpenTK/ImGui 边界
    - 启动时装配 `SceneEditorSession` 并默认解析源码 sample scene
    - 新增边界测试确认 `Engine.Editor` 仍保持 headless，`Engine.App` 不引用 Editor App
  FilesChanged:
    - `AnsEngine.sln`
    - `src/Engine.Editor.App/**`
    - `tests/Engine.Editor.App.Tests/**`
    - `.ai-workflow/boundaries/engine-editor-app.md`
    - `.ai-workflow/boundaries/README.md`
    - `.ai-workflow/tasks/task-eapp-001.md`
    - `.ai-workflow/archive/2026-04/TASK-EAPP-001.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`）
    - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`）
    - Smoke: pass（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`，ExitCode=0）
    - Perf: pass（仅新增 GUI 宿主启动成本；无逐帧文件 IO）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EAPP-001.md`

- TaskId: `TASK-EAPP-002`
  Title: M13 编辑器基础布局与状态栏
  Priority: `P1`
  PrimaryModule: `Engine.Editor.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
  Owner: `Exec-EditorApp`
  ClosedAt: `2026-04-30 15:35`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 Toolbar、Hierarchy、Inspector、Status Bar 的 GUI snapshot 模型
    - 补齐真实 ImGui/OpenGL 渲染后端，修复 native NewFrame 崩溃
    - 状态栏从 session 读取 scene path、dirty、selected object id 和 last error
  FilesChanged:
    - `src/Engine.Editor.App/**`
    - `tests/Engine.Editor.App.Tests/**`
    - `.ai-workflow/boundaries/engine-editor-app.md`
    - `.ai-workflow/tasks/task-eapp-002.md`
    - `.ai-workflow/archive/2026-04/TASK-EAPP-002.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`）
    - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`）
    - Smoke: pass（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`，ExitCode=0）
    - Perf: pass（无逐帧文件 IO 或重复 session open）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EAPP-002.md`

- TaskId: `TASK-EAPP-003`
  Title: M13 Hierarchy 面板与选择联动
  Priority: `P2`
  PrimaryModule: `Engine.Editor.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
  Owner: `Exec-EditorApp`
  ClosedAt: `2026-04-30 15:40`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Hierarchy 点击调用 `EditorAppController.SelectObject`
    - 选中高亮、Inspector 选中态和 Status Bar selected id 均从 session 生成
    - 选择成功不 dirty，选择失败保留原 selection 并显示 last error
  FilesChanged:
    - `src/Engine.Editor.App/EditorGuiRenderer.cs`
    - `tests/Engine.Editor.App.Tests/EditorGuiSnapshotFactoryTests.cs`
    - `.ai-workflow/boundaries/engine-editor-app.md`
    - `.ai-workflow/tasks/task-eapp-003.md`
    - `.ai-workflow/archive/2026-04/TASK-EAPP-003.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`）
    - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`）
    - Smoke: pass（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`，ExitCode=0）
    - Perf: pass（Hierarchy 每帧只消费 session 快照）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EAPP-003.md`

- TaskId: `TASK-QA-015`
  Title: M14 Runtime Object Model 门禁复验与归档
  Priority: `P3`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-05-01 13:29`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - M14 全量 Build/Test 与 Scene/App/Render/SceneData 回归通过
    - 复核 runtime object/component model 未越界到 Render/App/Editor/SceneData
    - CodeQuality: NoNewHighRisk=true, MustFixCount=0, MustFixDisposition=none
    - DesignQuality: DQ-1/DQ-2/DQ-3/DQ-4 均 pass
  FilesChanged:
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-qa-015.md`
    - `.ai-workflow/archive/2026-05/TASK-QA-015.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/plan-archive/2026-05/PLAN-M14-2026-05-01.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（整解测试通过；Scene.Tests 30 条、App.Tests 6 条、Render.Tests 16 条、SceneData.Tests 28 条均通过）
    - Smoke: pass（空/单/多对象、重复 load、render frame 输出、camera 与 snapshot/query 只读性通过）
    - Boundary: pass（未发现禁止依赖、runtime component 泄露或 M14 非范围能力）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-QA-015.md`

- TaskId: `TASK-SCENE-014`
  Title: M14 Runtime Snapshot 查询与边界测试
  Priority: `P2`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-01 10:39`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `SceneGraphService.CreateRuntimeSnapshot()` 与 `FindObject(string objectId)` 只读查询面
    - snapshot 覆盖 runtime object identity、transform、mesh/material 与 camera state
    - 新增 snapshot/query 行为测试与 Scene/Render/SceneData 边界测试
  FilesChanged:
    - `src/Engine.Scene/**`
    - `tests/Engine.Scene.Tests/**`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-014.md`
    - `.ai-workflow/archive/2026-05/TASK-SCENE-014.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（Scene.Tests 30 条通过；Render.Tests 16 条通过；SceneData.Tests 28 条通过；整解测试通过）
    - Smoke: pass（object id 查询可用；snapshot 不暴露内部可变集合；Render 不引用 runtime component 类型）
    - Boundary: pass（AllowedPaths 内变更，无禁止依赖）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-014.md`

- TaskId: `TASK-EAPP-004`
  Title: M13 Inspector 对象编辑
  Priority: `P3`
  PrimaryModule: `Engine.Editor.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
  Owner: `Exec-EditorApp`
  ClosedAt: `2026-05-01 03:19`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Inspector 支持 Id、Name、Mesh、Material、Position、Rotation、Scale 输入
    - 显式 Apply 提交全部通过 `SceneEditorSession` update API
    - 成功 dirty=true，失败显示 last error 并回滚到 session 当前有效值
    - 回流修复 native ImGui 文本/键盘输入桥接，Position/Rotation/Scale 输入框可编辑
  FilesChanged:
    - `src/Engine.Editor.App/**`
    - `tests/Engine.Editor.App.Tests/**`
    - `.ai-workflow/boundaries/engine-editor-app.md`
    - `.ai-workflow/tasks/task-eapp-004.md`
    - `.ai-workflow/archive/2026-04/TASK-EAPP-004.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`，仅既有 `net7.0` EOL warning）
    - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`，`Engine.Editor.App.Tests` 30 条通过）
    - Smoke: pass（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`，ExitCode=0）
    - Perf: pass（Inspector 无逐帧文件写入或重新加载）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EAPP-004.md`

- TaskId: `TASK-SCENE-017`
  Title: M15 Runtime snapshot 诊断与边界测试
  Priority: `P2`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-01 20:36`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `RuntimeSceneSnapshot` 新增 `UpdateFrameCount` 与 `AccumulatedUpdateSeconds`
    - snapshot 与 render frame 均可观察 update 后 rotation，且 update count 与 render frame number 分离
    - 补齐 Scene/App/Render 边界回归测试
  FilesChanged:
    - `src/Engine.Scene/Runtime/RuntimeSceneSnapshot.cs`
    - `src/Engine.Scene/Runtime/RuntimeScene.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `tests/Engine.Scene.Tests/SceneBoundaryTests.cs`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `tests/Engine.Render.Tests/RenderBoundaryTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-017.md`
    - `.ai-workflow/archive/2026-05/TASK-SCENE-017.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`）
    - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`；`Engine.Render.Tests` 18 条通过）
    - Smoke: pass（snapshot 可观察 update 统计；Render 不感知 update context/runtime scene 类型）
    - Perf: pass（snapshot 诊断不引入 scene rebuild、文件 IO 或 render side effect）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-017.md`

- TaskId: `TASK-SDATA-007`
  Title: M16 normalized component descriptions and validation
  Priority: `P0`
  PrimaryModule: `Engine.SceneData`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `Exec-SceneData`
  ClosedAt: `2026-05-02 11:44`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `SceneObjectDescription` 迁移为 normalized component descriptions
    - Transform 必需，MeshRenderer 可选
    - duplicate/unknown component fail，material 空白默认 `material://default`
  FilesChanged:
    - `src/Engine.SceneData/Descriptions/SceneComponentDescription.cs`
    - `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
    - `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
    - `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
    - `.ai-workflow/boundaries/engine-scenedata.md`
    - `.ai-workflow/tasks/task-sdata-007.md`
    - `.ai-workflow/archive/2026-05/TASK-SDATA-007.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`）
    - Test: pass（`dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`，33 条通过）
    - Smoke: pass（Transform-only normalize 成功；缺 Transform 失败；默认材质回退）
    - Perf: pass（无逐帧解析或跨模块回调）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SDATA-007.md`

- TaskId: `TASK-SDATA-006`
  Title: M16 SceneData component file schema
  Priority: `P0`
  PrimaryModule: `Engine.SceneData`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
  Owner: `Exec-SceneData`
  ClosedAt: `2026-05-02 11:40`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Scene JSON 文件层迁移到 `version: "2.0"` component array schema
    - 支持固定 `Transform` / `MeshRenderer` Pascal `type` 读写
    - 旧 `version: "1.0"` 扁平对象格式加载失败
  FilesChanged:
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
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`）
    - Test: pass（`dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`，31 条通过）
    - Smoke: pass（sample scenes 为 `2.0` + `components`，保存不写旧 object-level 字段）
    - Perf: pass（无逐帧 JSON 解析）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SDATA-006.md`

- TaskId: `TASK-QA-016`
  Title: M15 Runtime Update Pipeline 门禁复验与归档
  Priority: `P3`
  PrimaryModule: `QA`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-QA`
  ClosedAt: `2026-05-02 10:00`
  Status: `Done`
  ModuleAttributionCheck: `pass`
  Summary:
    - M15 全量 Build/Test 与 Scene/App/Render/SceneData 回归通过
    - 确认 App 在 render 前驱动 Scene update，默认样例场景可观察到 rotation 推进
    - snapshot 与 render frame 均可观察 update 后状态，且未越界到脚本/物理/动画/Play Mode 或 schema/contract 扩张
  FilesChanged:
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-qa-016.md`
    - `.ai-workflow/archive/2026-05/TASK-QA-016.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（沿用 `TASK-SCENE-015`、`TASK-SCENE-016`、`TASK-APP-010`、`TASK-SCENE-017` 的构建通过证据）
    - Test: pass（沿用 Scene/App/Render/SceneData 测试通过证据）
    - Smoke: pass（综合默认样例场景 rotation 推进、update-before-render、snapshot 可观察性与 headless app 退出码 0 的执行证据）
    - Perf: pass（无新增逐帧文件 IO、双重 update、scene rebuild、render side effect 或热重载轮询）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-QA-016.md`

- TaskId: `TASK-APP-010`
  Title: M15 App 主循环 runtime update 接线
  Priority: `P1`
  PrimaryModule: `Engine.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
  Owner: `Exec-App`
  ClosedAt: `2026-05-01 20:33`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - App 主循环在 render 前显式调用 scene runtime update
    - `SceneRuntimeAdapter` 将 `TimeSnapshot` / `InputSnapshot` 翻译为 `SceneUpdateContext`
    - loader failure 与 render failure 收口语义保持稳定
  FilesChanged:
    - `src/Engine.App/SceneRuntimeContracts.cs`
    - `src/Engine.App/ApplicationBootstrap.cs`
    - `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
    - `.ai-workflow/boundaries/engine-app.md`
    - `.ai-workflow/tasks/task-app-010.md`
    - `.ai-workflow/archive/2026-05/TASK-APP-010.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`）
    - Test: pass（`dotnet test tests/Engine.App.Tests/Engine.App.Tests.csproj --no-restore --nologo -v minimal`，8 条通过）
    - Smoke: pass（headless app run 退出码 0）
    - Perf: pass（无逐帧文件 IO、重复 scene initialize 或双重 update）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-APP-010.md`

- TaskId: `TASK-SCENE-016`
  Title: M15 Runtime update 默认旋转 smoke behavior
  Priority: `P1`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-01 20:20`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - `RuntimeScene.Update(...)` 新增第一个可渲染对象默认旋转 smoke behavior
    - rotation 只由 `DeltaSeconds` 驱动，position/scale 与资源引用保持不变
    - `BuildRenderFrame()` 与 snapshot 观察同一 update 后 rotation
  FilesChanged:
    - `src/Engine.Scene/Runtime/RuntimeScene.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-016.md`
    - `.ai-workflow/archive/2026-05/TASK-SCENE-016.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`）
    - Test: pass（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`，41 条通过）
    - Smoke: pass（首个可渲染对象 rotation 变化；render frame 与 snapshot 观察一致）
    - Perf: pass（无逐帧对象重建、文件读取或 render side effect）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-016.md`

- TaskId: `TASK-SCENE-015`
  Title: M15 Runtime update context 与统计地基
  Priority: `P0`
  PrimaryModule: `Engine.Scene`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-scene.md`
  Owner: `Exec-Scene`
  ClosedAt: `2026-05-01 20:16`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - 新增 `SceneUpdateContext` 与 `SceneGraphService.UpdateRuntime(...)`
    - `RuntimeScene.Update(...)` 维护 `UpdateFrameCount` 与 `AccumulatedUpdateSeconds`
    - `Clear()` / `LoadFromDescription(...)` 重置 update 统计
  FilesChanged:
    - `src/Engine.Scene/SceneUpdateContext.cs`
    - `src/Engine.Scene/Runtime/RuntimeScene.cs`
    - `src/Engine.Scene/SceneGraphService.cs`
    - `tests/Engine.Scene.Tests/SceneGraphServiceTests.cs`
    - `.ai-workflow/boundaries/engine-scene.md`
    - `.ai-workflow/tasks/task-scene-015.md`
    - `.ai-workflow/archive/2026-05/TASK-SCENE-015.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --no-restore --nologo -v minimal`）
    - Test: pass（`dotnet test tests/Engine.Scene.Tests/Engine.Scene.Tests.csproj --no-restore --nologo -v minimal`，36 条通过）
    - Smoke: pass（update 入口可调用，空 scene 不崩溃，统计可观察）
    - Perf: pass（无逐帧文件 IO、scene rebuild 或 render frame number 隐式依赖）
  SnapshotPath: `.ai-workflow/archive/2026-05/TASK-SCENE-015.md`

- TaskId: `TASK-EAPP-005`
  Title: M13 Open/Save/Save As 工作流
  Priority: `P3`
  PrimaryModule: `Engine.Editor.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
  Owner: `Exec-EditorApp`
  ClosedAt: `2026-04-30 15:53`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Toolbar Open、Save、Save As 接入路径输入工作流
    - 文件操作全部通过 `SceneEditorSession.Open/Save/SaveAs`
    - 验证环境变量覆盖、保存写盘、Save As 路径更新和 Open 失败不污染 session
  FilesChanged:
    - `src/Engine.Editor.App/**`
    - `tests/Engine.Editor.App.Tests/**`
    - `.ai-workflow/boundaries/engine-editor-app.md`
    - `.ai-workflow/tasks/task-eapp-005.md`
    - `.ai-workflow/archive/2026-04/TASK-EAPP-005.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`）
    - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`）
    - Smoke: pass（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`，ExitCode=0）
    - Perf: pass（Open/Save 仅用户触发，无逐帧文件 IO）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EAPP-005.md`

- TaskId: `TASK-EAPP-006`
  Title: M13 Add/Remove Object GUI 工作流
  Priority: `P3`
  PrimaryModule: `Engine.Editor.App`
  BoundaryContractPath: `.ai-workflow/boundaries/engine-editor-app.md`
  Owner: `Exec-EditorApp`
  ClosedAt: `2026-04-30 15:58`
  Status: `Done`
  HumanSignoff: `pass`
  ModuleAttributionCheck: `pass`
  Summary:
    - Add Object 创建默认 cube 对象并自动选中
    - Remove Selected 删除当前选中对象并清 selection
    - 保存后 reload 验证增删结果持久化
  FilesChanged:
    - `src/Engine.Editor.App/**`
    - `tests/Engine.Editor.App.Tests/**`
    - `.ai-workflow/boundaries/engine-editor-app.md`
    - `.ai-workflow/tasks/task-eapp-006.md`
    - `.ai-workflow/archive/2026-04/TASK-EAPP-006.md`
    - `.ai-workflow/archive/archive-index.md`
    - `.ai-workflow/board.md`
  ValidationEvidence:
    - Build: pass（`dotnet build AnsEngine.sln --nologo -v minimal`）
    - Test: pass（`dotnet test AnsEngine.sln --no-restore --nologo -v minimal`）
    - Smoke: pass（`ANS_ENGINE_EDITOR_AUTO_EXIT_SECONDS=1 dotnet run --project src/Engine.Editor.App/Engine.Editor.App.csproj --no-build`，ExitCode=0）
    - Perf: pass（id 生成只扫描当前对象集合，无逐帧文件 IO）
  SnapshotPath: `.ai-workflow/archive/2026-04/TASK-EAPP-006.md`
