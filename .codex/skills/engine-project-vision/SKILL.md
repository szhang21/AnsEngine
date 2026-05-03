---
name: engine-project-vision
description: AnsEngine 项目长期目标与里程碑规划参照。用于讨论路线图、里程碑、模块优先级、系统边界、长期架构取舍、计划技术设计、真实 runtime/editor 验收。触发词包括：最终目标、项目目标、路线图、里程碑、规划、技术设计、代码模块设计、设计模式、M14、M15、脚本系统、物理系统、动画系统、编辑器、runtime、game engine。
---

# AnsEngine 项目愿景

当讨论项目路线、里程碑计划、模块优先级或长期架构取舍时，使用本 Skill。

## 最终目标

AnsEngine 的最终目标不是只做一个编辑器，也不是只做一个渲染 demo，而是逐步构建一个小型完整游戏引擎与配套编辑器。

长期目标包括：

- 可运行的 engine runtime
- 可编辑、可保存、可预览的 editor
- 数据驱动的 scene / asset 工作流
- 统一的 runtime object / component model
- 脚本系统
- 物理系统
- 动画系统
- 渲染系统
- 资源系统
- 未来的 Play Mode、Prefab、Undo/Redo、Resource Browser、Audio 等扩展能力

## 核心认知

- Editor 是引擎的工具入口，不是项目最终目标本身。
- Runtime 能力和 Editor 能力必须协同演进。
- 脚本、物理、动画不能各自造对象模型，必须挂在统一 runtime object/component model 上。
- 大系统不能被压缩成单个里程碑，应拆成多轮：
  - foundation / boundary
  - runtime MVP
  - editor integration
  - QA / close
- 里程碑计划必须避免只沿着编辑器体验一路推进，而忽略 runtime engine 的核心能力。

## 长期系统蓝图

### Engine Runtime

负责游戏运行时能力：

- Scene / Object / Component
- Transform
- Render submission
- Update pipeline
- Scripting
- Physics
- Animation
- Input integration
- Asset references
- Play Mode runtime behavior

### Engine Editor

负责编辑与调试能力：

- GUI editor shell
- Hierarchy
- Inspector
- Scene save/load
- Viewport preview
- Picking
- Gizmo
- Resource Browser
- Undo/Redo
- Play Mode controls

### Data / Asset

负责数据文件和资源链路：

- SceneData
- JSON scene documents
- Mesh assets
- Material references
- Future prefab / animation clip / physics data

## 当前阶段路线参照

近期路线应优先补 runtime 地基：

- M14：Runtime Object / Component Foundation
- M15：Runtime Update Pipeline
- M16：Component Serialization Bridge
- M17+：Scripting foundation / MVP / editor bridge
- 后续：Physics foundation / MVP / editor bridge
- 后续：Animation foundation / MVP / editor bridge

Editor Viewport、Picking、Gizmo、Undo/Redo 等能力也重要，但应与 runtime 主线交替推进，不能让编辑器路线吞掉 runtime 系统建设。

## 规划约束

讨论里程碑时必须检查：

- 这一步服务的是 runtime、editor，还是二者的桥接？
- 是否过早做了 UI，而缺少 runtime 地基？
- 是否把脚本/物理/动画这类大系统压缩得过小？
- 是否引入了与当前模块边界不匹配的依赖？
- 是否需要先建立数据模型或生命周期，再做可视化工具？
- 是否能拆成任务卡，并有明确 Build/Test/Smoke/Boundary 验收？
- 是否只是建立 contract/schema/test stub，而没有接入真实 runtime/editor 主路径？
- 如果里程碑名称承诺 Interaction、Runtime、Playable、Editor Integration 等用户可见能力，验收是否覆盖真实触发路径，而不仅是单元测试或桩实现？

## 计划技术设计要求

里程碑计划不能只给方向和任务名，还必须包含足够的代码设计信息，便于 Dispatch/Executor 不自行脑补关键实现。

计划中应明确：

- 模块职责、允许依赖、禁止依赖和依赖方向。
- 新增或变更的 public API / internal API / DTO / schema。
- runtime 或 editor 的数据流、生命周期和调用顺序。
- 关键设计模式或结构取舍，例如 adapter、bridge、composition root、snapshot、registry、factory、fail-fast result。
- 错误语义、失败回退、诊断证据和 shutdown/dispose 行为。
- 自动测试与真实主路径验收的区别。

如果计划目标是 foundation，可以接受 contract + tests 为主；如果计划目标是 MVP、interaction、integration 或用户可见能力，则必须定义真实主路径 smoke/manual 验收。stub/headless/unit test 可以作为自动验证，但不能替代已承诺的真实行为。

## 与其他 Skill 的关系

- `engine-coding-standards`：负责 C# 命名、字段风格、文件组织。
- `engine-task-dispatch`：负责任务卡、看板、WIP、归档、门禁。
- `engine-project-vision`：负责长期方向、里程碑取舍和系统路线参照。
