# 工程基线决策

> 本文件定义项目初始化与持续开发必须遵守的统一技术基线。  
> 若需变更，请先创建“基线变更任务卡”，评审通过后再执行实现改动。

## 1) 技术版本基线

- `DotNetVersion`：`.NET 8`
- `OpenTKVersion`：`4.x（锁定到项目统一次版本）`
- `TestFramework`：`xUnit`

## 2) 仓库目录结构基线

- `src/`：引擎与应用源代码
  - `src/Engine.Core/`
  - `src/Engine.Platform/`
  - `src/Engine.Render/`
  - `src/Engine.Scene/`
  - `src/Engine.Asset/`
  - `src/Engine.App/`
- `tests/`：测试工程
  - `tests/Engine.Core.Tests/`
  - `tests/Engine.Scene.Tests/`
  - `tests/Engine.Asset.Tests/`
- `.ai-workflow/`：任务、边界、归档体系

## 3) 统一验证命令基线

- Build（Debug）：`dotnet build -c Debug`
- Build（Release）：`dotnet build -c Release`
- Test：`dotnet test`
- Smoke：运行 Demo 场景 3-5 分钟无崩溃
- Perf：相对基线无明显帧时间退化

## 4) 任务卡引用规则

每张任务卡必须包含：

- `BaselineRef`: `references/project-baseline.md`

若任务实现与本基线冲突：

1. 先创建“基线变更任务卡”
2. 在评审中说明变更原因、影响范围、迁移策略
3. 通过后再更新工程与任务派发规则

## 5) 变更记录

- 2026-04-04：初始化基线（OpenTK + C# 学习型引擎）
