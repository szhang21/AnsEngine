# TASK-SDATA-008 归档快照

- TaskId: `TASK-SDATA-008`
- Title: `M17 SceneData Script component schema and validation`
- Priority: `P0`
- PrimaryModule: `Engine.SceneData`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-scenedata.md`
- Owner: `Exec-SceneData`
- ClosedAt: `2026-05-02 14:09`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- Added repeatable file-model `Script` component with `scriptId` and number/bool/string properties.
- Added normalized `SceneScriptComponentDescription` and `SceneScriptPropertyValue`.
- Preserved Script component order from scene file to normalized component list and save/load roundtrip.
- Added validation for blank `scriptId` and unsupported property value types.
- Updated default app sample scene to declare `RotateSelf` without adding runtime binding in SceneData.

## FilesChanged

- `src/Engine.SceneData/FileModel/SceneFileComponentDefinition.cs`
- `src/Engine.SceneData/FileModel/SceneFileScriptPropertyValue.cs`
- `src/Engine.SceneData/Descriptions/SceneComponentDescription.cs`
- `src/Engine.SceneData/Descriptions/SceneObjectDescription.cs`
- `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`
- `tests/Engine.SceneData.Tests/SceneDataContractsTests.cs`
- `src/Engine.App/SampleScenes/default.scene.json`
- `.ai-workflow/boundaries/engine-scenedata.md`
- `.ai-workflow/tasks/task-sdata-008.md`
- `.ai-workflow/archive/2026-05/TASK-SDATA-008.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build: `pass`（`/Users/ans/.dotnet/dotnet build AnsEngine.sln --no-restore --nologo -v minimal`；仅既有 `net7.0` EOL warning）
- Test: `pass`（`/Users/ans/.dotnet/dotnet test tests/Engine.SceneData.Tests/Engine.SceneData.Tests.csproj --no-restore --nologo -v minimal`；Engine.SceneData.Tests 37 条通过）
- Smoke: `pass`（Script serialize/deserialize、multiple Script order、blank scriptId、unsupported property type、default sample scene declaration 均覆盖）
- Boundary: `pass`（SceneData 未引用 Engine.Scripting/Scene/App/Editor/Render runtime 类型；只做 schema/normalize）
- Perf: `pass`（无逐帧 JSON 解析、registry 查询、反射绑定或跨模块 runtime 查询）

## Risks

- `low`：Scene/App 尚未消费 Script components，留给 `TASK-SCENE-019` 和 `TASK-APP-011`。
