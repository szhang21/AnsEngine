# 任务卡实施字段示例（Illustrative Examples Only）

用途：展示正式任务卡中的实施字段可以如何写。  
注意：本文件**仅作为写法示例，不作为充分性上限**。  
`Dispatch Agent` 只能把它当作“至少应达到的表达粒度参考”，**不能**因为“看起来像示例”就停止补充信息。

## 强提醒

- 示例是参考下限，不是合格上限。
- 若实际任务比示例更复杂，任务卡必须写得更细。
- 若示例未覆盖某项关键约束、失败语义、边界、参考点，必须按真实任务补齐。
- 禁止出现以下错误心态：
  - “已经像示例了，所以够了”
  - “示例只有 3 条，我也只写 3 条”
  - “示例没写这个约束，所以我也不写”

## 示例 A：Render 接入真实 mesh provider

### MilestoneContext

- M10 当前目标之一是让 `Engine.Render` 摆脱内置 demo 几何作为生产主路径，转为消费受控 mesh provider。
- 本卡承担的是“把 Render 从常量几何消费切换到真实 CPU mesh 消费入口”的局部职责，不负责磁盘导入，也不负责 App 装配。
- 上游已经确定 `Asset` 管 CPU mesh、`Render` 管 GPU 资源；本卡不得重新讨论该边界。

### DecisionCarryOver

- 继承决策：
  - `Engine.Render` 只能依赖稳定契约，不得直接依赖 `Engine.Asset` 实现。
  - fallback mesh 只允许作为缺失/失败路径，不得继续作为默认主路径。
- 不得推翻的取舍：
  - 不把 OBJ 或磁盘路径语义泄漏进 Render 公开接口。
  - 不在本卡内引入材质系统扩展或纹理加载。

### ImplementationNotes

- 先定位 Render 当前几何解析入口，识别内置 fallback 与真实 provider 的切换点。
- 再补 provider 消费路径，使 `SceneMeshRef -> IMeshAssetProvider -> SceneRenderMeshGeometry` 可稳定贯通。
- 最后补共享 mesh cache 命中测试和 fallback 测试，确保缺失 mesh 不会让主循环崩溃。

### DesignConstraints

- 不允许在 Render 中解析磁盘 catalog。
- 不允许在 Render 中引入 Asset 实现类型。
- 不允许把 fallback 路径重新写成主绘制路径。

### FallbackBehavior

- 当 provider 返回失败结果或无效几何时，降级为受控 fallback mesh。
- 当共享 mesh 已有 GPU cache 命中时，必须复用，不得重复创建资源。
- 若发现 provider 契约不足以表达本卡所需语义，必须回退修卡，不得私自扩契约。

### ExamplesOrReferences

- 相关源码入口：
  - `src/Engine.Render/SceneRenderSubmission.cs`
  - `src/Engine.Render/SceneRenderMeshGeometryCache.cs`
- 相关测试入口：
  - `tests/Engine.Render.Tests/`
- 相关任务/归档：
  - `TASK-ASSET-001`

## 示例 B：App 组合根装配 provider

### MilestoneContext

- M10 需要真实资源链路在 App 层完成组合，而不是让下游模块偷偷定位依赖。
- 本卡承担的是“组合根装配与样例运行路径接线”，不是资源导入实现。

### DecisionCarryOver

- 继承决策：
  - provider 由 `Engine.App` 显式注入到 Render/Asset 相关入口。
  - 不使用全局定位器或静态单例绕过装配。
- 不得推翻的取舍：
  - App 只负责装配，不承载 OBJ 解析或 GPU cache 逻辑。

### ImplementationNotes

- 先梳理 `RuntimeBootstrap` 当前装配链路。
- 把样例资源入口、mesh provider、headless/native window 两条运行路径串起来。
- 补 App 层回归测试，确认启动、运行、退出路径不因新装配而破坏。

### DesignConstraints

- 不允许把资源解析逻辑塞进 App。
- 不允许让 Render/Scene 自己去 new 具体 provider。
- 不允许通过环境变量分支把核心依赖注入变成隐式行为。

### FallbackBehavior

- 若样例资源缺失，应明确失败并保持退出路径可控。
- 若 headless 路径无法创建真实窗口，不应阻塞基本 smoke 验证。
- 若装配中发现契约方向不够支撑，必须回退给 Dispatch/Plan，不得在 App 中临时打洞。

### ExamplesOrReferences

- 相关源码入口：
  - `src/Engine.App/ApplicationBootstrap.cs`
  - `src/Engine.App/Program.cs`
- 相关测试入口：
  - `tests/Engine.App.Tests/`
