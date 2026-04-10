# 任务: TASK-BOOT-001 初始化 AnsEngine 多项目骨架

## 目标（Goal）
在严格任务流下完成 .NET 8 多项目骨架初始化，并达到 Debug/Release 可编译、单测可运行。

## 负责人（Owner）
Codex

## 计划引用（兼容别名：PlanRef）
`PLAN-M1M2-2026-04-08`

## 里程碑引用（兼容别名：MilestoneRef）
`M1-WindowLoopVisible`

## 优先级（Priority）
P1

## 主模块归属（PrimaryModule）
Engine.App

## 次级模块（SecondaryModules）
- Engine.Core
- Engine.Platform
- Engine.Render
- Engine.Scene
- Engine.Asset

## 边界合同路径（BoundaryContractPath）
- `.ai-workflow/boundaries/engine-app.md`

## 基线引用（BaselineRef）
- `references/project-baseline.md`

## 范围（Scope）
- AllowedModules:
  - Engine.App
  - Engine.Core
  - Engine.Platform
  - Engine.Render
  - Engine.Scene
  - Engine.Asset
- AllowedFiles:
  - `AnsEngine.sln`
  - `.ai-workflow/board.md`
  - `.ai-workflow/tasks/task-boot-001.md`
  - `.ai-workflow/archive/archive-index.md`
  - `.ai-workflow/archive/2026-04/TASK-BOOT-001.md`
- AllowedPaths:
  - `src/**`
  - `tests/**`
  - `.ai-workflow/**`
  - `AnsEngine.sln`

## 跨模块标记（CrossModule）
true

## 非范围（OutOfScope）
- 不实现窗口循环逻辑
- 不实现首帧三角形渲染
- 不做基线变更（仍保持 .NET 8 + OpenTK 4.x + xUnit）
- OutOfScopePaths:
  - `.codex/**`

## 依赖约束（DependencyContract）
- AllowedDependsOn:
  - App -> Core/Platform/Render/Scene/Asset
  - Render -> Core/Platform 抽象
  - Platform -> Core
  - Scene -> Core
  - Asset -> Core/Platform 抽象
- ForbiddenDependsOn:
  - Scene -> Render 具体实现
  - Platform -> Render
  - Asset -> Scene

## 验收标准（Acceptance）
- Build: `dotnet build -c Debug` 与 `dotnet build -c Release` 通过
- Test: `dotnet test` 通过
- Smoke: 本任务不要求（N/A，留待窗口循环任务）
- Perf: 本任务不引入帧循环，记录为 N/A

## 交付物（Deliverables）
- Minimal patch
- Self-check notes
- Risk list (high|medium|low)
- Change summary (what changed and why)

## GatePlan
- Verify:
  - 运行 Build/Test 门禁命令并记录结果
  - 校验 `FilesChanged` 全部命中 `AllowedPaths`
- Review:
  - 检查边界依赖是否符合合同
  - 记录归档与 `ModuleAttributionCheck`

## 状态（Status）
Done

## 完成度（Completion）
`100`

## 归档（Archive）
- ArchivePath: `.ai-workflow/archive/2026-04/TASK-BOOT-001.md`
- ClosedAt: `2026-04-06 20:33`
- Summary:
  - 完成多项目 solution 初始化（6 个 src 模块 + 3 个 tests 项目）
  - 建立最小公开接口占位（App/Platform/Render/Asset）与组合根启动路径
  - 完成门禁验证：Debug/Release build 与 dotnet test 均通过
- FilesChanged:
  - `AnsEngine.sln`
  - `src/**`
  - `tests/**`
  - `.ai-workflow/board.md`
  - `.ai-workflow/tasks/task-boot-001.md`
  - `.ai-workflow/archive/**`
- ValidationEvidence:
  - Build(Debug): pass（存在环境级 CS1668 警告，不阻塞构建）
  - Build(Release): pass（存在环境级 CS1668 警告，不阻塞构建）
  - Test: pass（3 个测试项目各 1 用例通过）
  - Smoke: N/A（本任务未实现窗口循环）
  - Perf: N/A（本任务未引入帧循环）
- ModuleAttributionCheck: pass
