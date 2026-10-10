# Untitled

Unity 俯视角（2.5D）经营 / 冒险Demo。

玩法循环：**白天经营（Bussiness）→ 购买装备 → 夜晚搜打撤（Adventure）→ 战利品结算 → 进入下一天**。

---

## 环境要求

| 项 | 版本 |
| --- | --- |
| Unity | 2022.3.62f1 |
| 渲染管线 | URP |

---

## 目录结构

```
Untitled/
├─ Assets/
│  ├─ Art/                            美术资源
│  │  └─ Audio/ Materials/ Models/ Prefabs/ Sprites/ Textures/
│  ├─ Input/                          输入配置
│  ├─ Prefabs/                        预制体
│  ├─ Scenes/
│  │  ├─ Develop.unity                开发/测试场景
│  │  ├─ Adventure.unity              夜晚冒险场景
│  │  └─ Bussiness.unity              白天经营场景
│  ├─ Scripts/
│  │  ├─ Core/                        运行时核心
│  │  ├─ Data/                        数据模型
│  │  ├─ UI/                          UI系统控制
│  │  └─ Utils/                       通用工具
│  ├─ Shaders/                        着色器
│  └─ Settings/ 
├─ Packages/                  
└─ ProjectSettings/           
```

---

## 快速入门

### 1. 管理器（Manager）

所有纯C#管理器继承 `ManagerBase<T>`，由 `Bootstrap` 统一注册，之后通过静态 `Instance` 访问：

```6:16:Assets/Scripts/Core/Boot/Bootstrap.cs
public class Bootstrap : MonoBehaviour
{
    private void Awake()
    {
        RegisterManager(new GameManager());
        RegisterManager(new InputManager());
    }

    private void RegisterManager<T>(ManagerBase<T> manager) where T : ManagerBase<T>
    {
        manager.Initialize();
    }
}
```

### 2. 数据（Data）

纯 C# 数据类，不依赖 `MonoBehaviour`，便于测试与序列化：

- `GameData` 聚合 **金钱**、`InventoryData`（仓库）、`PackageData`（冒险背包）。
- 冒险结束时将背包内物品结算进仓库，下一轮冒险开始时清空背包。

### 3. 事件中心（EventCenter）

静态全局事件总线，按 `GameEventType` 订阅，支持 0~3 个参数：

```csharp
EventCenter.Subscribe(GameEventType.Collect, OnCollect);
EventCenter.Subscribe<int, Vector2>(GameEventType.PlayerDamaged, OnPlayerDamaged);
EventCenter.Unsubscribe(GameEventType.Collect, OnCollect);
EventCenter.Invoke(GameEventType.Collect);
```

### 4. 实体（Entity）

- `EntityBase`：提供 `Coord`，将世界坐标与地图坐标间映射。
- `CharacterBase`：封装胶囊碰撞体与移动逻辑。
- `PlayerEntity`：封装玩家控制。

---
