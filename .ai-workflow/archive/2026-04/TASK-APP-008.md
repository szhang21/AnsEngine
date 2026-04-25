# TASK-APP-008 归档快照

- TaskId: `TASK-APP-008`
- Title: `M9 Mesh provider 装配与样例运行路径接线`
- Priority: `P1`
- PrimaryModule: `Engine.App`
- BoundaryContractPath: `.ai-workflow/boundaries/engine-app.md`
- Owner: `Exec-App`
- ClosedAt: `2026-04-25 18:33`
- Status: `Done`
- HumanSignoff: `pass`
- ModuleAttributionCheck: `pass`

## Summary

- 组合根新增 `DiskMeshAssetProvider` 与 sample mesh 资源目录装配，native 渲染路径显式注入 mesh provider。
- `ApplicationHost` 在进入主循环前预热一次 bootstrap mesh 解析，确保 headless/真实窗口路径都走到真实磁盘 mesh 主链路。
- 补齐 App 装配测试、样例资源文件与边界文档，保持 App 只做装配不承载 OBJ/GPU 逻辑。

## FilesChanged

- `src/Engine.App/ApplicationBootstrap.cs`
- `src/Engine.App/Engine.App.csproj`
- `src/Engine.App/SampleAssets/cube.obj`
- `src/Engine.App/SampleAssets/mesh-catalog.txt`
- `tests/Engine.App.Tests/RuntimeBootstrapTests.cs`
- `.ai-workflow/boundaries/engine-app.md`
- `.ai-workflow/tasks/task-app-008.md`
- `.ai-workflow/archive/2026-04/TASK-APP-008.md`
- `.ai-workflow/archive/archive-index.md`
- `.ai-workflow/board.md`

## ValidationEvidence

- Build(Debug): `pass`（M9 App 装配路径已进入当前代码基线；Human 于 `2026-04-25` 确认验收通过）
- Build(Release): `pass`（同上）
- Test: `pass`（`tests/Engine.App.Tests` 覆盖 mesh provider 注入、bootstrap 运行与异常收口路径）
- Smoke: `pass`（Human 于 `2026-04-25` 确认真实 mesh 主链路可在 headless/真实窗口路径启动并稳定退出）
- Perf: `pass`（provider/sample 资源只在启动阶段装配与预热，不引入逐帧重复初始化）

## Risks

- `low`：当前 sample 资源目录按组合根相对路径装配；后续若入口改为配置文件或命令行参数，应继续由 App 负责路径解析与依赖注入，不回退为子模块自定位。
