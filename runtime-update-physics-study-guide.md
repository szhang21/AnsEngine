# AnsEngine Runtime Update 与 Physics 学习问题清单

这份清单用于按代码路径理解 AnsEngine 当前 runtime 主循环、脚本更新、物理碰撞与 Scene 写回机制。

建议阅读顺序：

1. 主循环顺序
2. Script 如何改 Scene
3. Physics 和 Scene 写回
4. PhysicsWorld 自身
5. 从数据到 runtime

预计总耗时：约 2.5 - 4 小时。

## 第一组：主循环顺序

阅读文件：

- `src/Engine.App/ApplicationBootstrap.cs`

重点代码：

```csharp
while (mWindowService.Exists && !mWindowService.IsCloseRequested)
{
    mWindowService.ProcessEvents();
    var input = mInputService.GetSnapshot();
    var time = mTimeService.Current;

    mSceneRuntime.Update(time, input);

    var scriptUpdateResult = mScriptRuntime.Update(
        time.DeltaSeconds,
        time.TotalSeconds,
        ConvertInput(input));

    var physicsUpdateResult = mPhysicsOrchestrator.ResolveAndWriteBack(
        mPhysicsWorld!,
        mSceneRuntime);

    mRenderer.RenderFrame();
    mWindowService.Present();
}
```

需要回答的问题：

1. `ApplicationHost.Run()` 里 scene load 成功后，初始化顺序是什么？
2. `mPhysicsWorld = ScenePhysicsWorldDefinitionBridge.CreateWorld(loadResult.Scene)` 为什么要发生在 loop 前？
3. 每帧里 `SceneRuntime.Update`、`ScriptRuntime.Update`、`Physics ResolveAndWriteBack`、`RenderFrame` 的顺序分别解决什么问题？
4. 如果把 Physics 放到 Script 前面，会发生什么？

预计耗时：15 - 25 分钟。

目标：理解每帧顺序是 `Scene update -> Script update -> Physics writeback -> Render`。

## 第二组：Script 如何改 Scene

阅读文件：

- `src/Engine.App/ApplicationBootstrap.cs`
- `src/Engine.Scene/Runtime/SceneScriptObjectHandle.cs`
- `src/Engine.Scripting/ScriptRuntime.cs`

需要回答的问题：

1. `MoveOnInputScript` 拿到的是 Scene object 本体，还是一个窄接口？
2. `context.Self.Transform.SetLocalTransform(...)` 最终写到了哪里？
3. 为什么 `Engine.Scripting` 不应该直接依赖 `RuntimeScene` 内部集合？
4. `ScriptRuntime.Update(...)` 失败时，App 怎么收口？

预计耗时：25 - 40 分钟。

目标：理解脚本不是直接操作 Scene 内部对象，而是通过窄接口修改自身 Transform。

## 第三组：PhysicsWorld 自身

阅读文件：

- `src/Engine.Physics/PhysicsWorld.cs`

需要回答的问题：

1. `PhysicsWorld.Load(...)` 会拒绝哪些非法 body？
2. `CalculateAabb(...)` 怎么处理 collider size、center、transform scale？
3. `ResolveKinematicMove(...)` 为什么按 X/Y/Z 三个轴分别尝试？
4. 现在 Dynamic body 会不会主动移动？谁给它 desired transform？
5. Static body 会不会被移动？为什么？

预计耗时：35 - 55 分钟。

目标：理解当前 Physics 不是完整刚体模拟，而是 AABB 约束和 kinematic movement resolve。

## 第四组：Physics 和 Scene 写回

阅读文件：

- `src/Engine.App/RuntimePhysicsOrchestrator.cs`
- `src/Engine.Scene/Runtime/RuntimeScene.cs`
- `src/Engine.Scene/SceneGraphService.cs`

需要回答的问题：

1. Orchestrator 怎么判断一个 physics body 能不能写回 Scene？
2. 如果 Physics body id 在 Scene 里找不到，会怎么失败？
3. `TrySetObjectTransform(...)` 做了哪些防御检查？
4. 为什么 writeback 放在 App 层，而不是 PhysicsWorld 直接引用 Scene？
5. Render 最终读到的是 script 修改前、script 修改后，还是 physics 修正后的 Transform？

预计耗时：25 - 40 分钟。

目标：理解 App 是组合根，负责 `Scene -> Physics -> Scene` 的桥接，而 Physics 不直接依赖 Scene。

## 第五组：从数据到 runtime

阅读文件：

- `src/Engine.App/ScenePhysicsWorldDefinitionBridge.cs`
- `src/Engine.SceneData/Descriptions/SceneComponentDescription.cs`
- `src/Engine.SceneData/Loading/SceneFileDocumentNormalizer.cs`

需要回答的问题：

1. 一个 scene JSON 里的 `RigidBody` / `BoxCollider` 怎么变成 `PhysicsBodyDefinition`？
2. 哪些对象会进入 PhysicsWorld？
3. 只有 `RigidBody` 没有 `BoxCollider` 会怎样？
4. 只有 `BoxCollider` 没有 `RigidBody` 会怎样？
5. Editor 里所谓 `PhysicsParticipation` 和 runtime 真正参与 physics 的条件是否一致？

预计耗时：40 - 70 分钟。

目标：理解 Scene JSON 组件如何经过 SceneData normalize，最终进入 PhysicsWorld。

## 推荐学习路线

### 第一天

建议先看：

1. 第一组：主循环顺序
2. 第二组：Script 如何改 Scene
3. 第四组：Physics 和 Scene 写回

预计耗时：约 1.5 小时。

第一天目标：

- 能说清每帧更新顺序。
- 能说清 Script 如何改 Scene Transform。
- 能说清 Physics 如何把修正结果写回 Scene。

### 第二天

建议再看：

1. 第三组：PhysicsWorld 自身
2. 第五组：从数据到 runtime

预计耗时：约 1.5 - 2 小时。

第二天目标：

- 能说清 AABB 碰撞约束逻辑。
- 能说清哪些 Scene object 会进入 PhysicsWorld。
- 能说清 Editor authoring、SceneData、runtime physics 之间的关系。

## 最小掌握目标

看完后，至少应该能回答这条完整链路：

```text
用户输入
-> InputSnapshot
-> ScriptRuntime.Update
-> MoveOnInputScript 修改 Scene Transform
-> RuntimePhysicsOrchestrator 读取 Scene 当前 Transform
-> PhysicsWorld.ResolveKinematicMove 做碰撞修正
-> TrySetObjectTransform 写回 Scene
-> RenderFrame 读取修正后的 Scene 状态
```

如果能把这条链路讲清楚，就说明你已经掌握了当前 runtime update + physics MVP 的核心机制。
