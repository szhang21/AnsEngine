# 模块边界目录规范

本目录用于存放各模块的正式边界合同文档，供任务派发、评审、归档统一引用。

## 1) 目录用途

- 记录每个模块的职责与非职责
- 记录允许/禁止依赖
- 记录对外公开接口契约
- 记录边界变更历史

## 2) 命名规范

- 文件命名统一使用小写中划线格式：`engine-<module>.md`
- 示例：
  - `engine-render.md`
  - `engine-scene.md`
  - `engine-asset.md`
  - `engine-platform.md`
  - `engine-app.md`
  - `engine-contracts.md`
  - `engine-editor.md`

## 3) 任务卡引用规范

每张任务卡必须填写：

- `PrimaryModule`
- `BoundaryContractPath`

其中 `BoundaryContractPath` 必须指向本目录中的某个合同文件，例如：

- `.ai-workflow/boundaries/engine-render.md`

未填写或路径无效时，任务不得进入 `InProgress`。

## 4) 推荐模块与路径映射

- `Engine.Render` -> `src/Engine.Render/**`
- `Engine.Scene` -> `src/Engine.Scene/**`
- `Engine.Asset` -> `src/Engine.Asset/**`
- `Engine.Platform` -> `src/Engine.Platform/**`
- `Engine.App` -> `src/Engine.App/**`
- `Engine.Contracts` -> `src/Engine.Contracts/**`（或 `src/Engine.Render.Contracts/**`，二选一）
- `Engine.SceneData` -> `src/Engine.SceneData/**`
- `Engine.Editor` -> `src/Engine.Editor/**`

## 5) 变更管理

- 修改边界合同时，需同步更新任务卡中的合同版本引用。
- 任务从 `Review -> Done` 前，归档中必须记录：
  - `PrimaryModule`
  - `BoundaryContractPath`
  - `ModuleAttributionCheck`
- 若任务发生跨模块改动，必须在归档中写明审批依据和影响范围。

## 6) 变更记录

- 2026-04-30
  - 变更人：Execution-Agent
  - 变更内容：确认 `Engine.Editor` 边界合同和路径映射已纳入目录索引，支撑 `TASK-EDITOR-001` 新模块落地。
  - 变更原因：M12 新增 headless editor core 模块，需要边界目录能被后续任务卡稳定引用。
