# Phase 2 — 战斗系统 Detail Plan

## 目标

在 Phase 1 基础上，实现完整的单局战斗：
> 士兵自动寻敌战斗 → 技能系统 → 防御建筑 → 敌方基地与敌兵 → 胜负判定

完成后玩家可以体验完整的 **防守 → 发展 → 反攻 → 胜利/失败** 流程。

---

## 任务分解

### Step 1：战斗核心 — 士兵AI与攻击

**1.1 目标选择系统**
- Unit 新增 `FindTarget()` 方法
- 搜索范围内最近的敌方单位（或建筑）
- 玩家单位目标优先级：最近敌兵 > 敌方建筑 > 敌方基地核心
- 敌方单位目标优先级：最近玩家士兵 > 玩家建筑 > 玩家基地
- 需要阵营标识：`Team` 枚举（Player / Enemy）

**1.2 战斗状态机**
扩展 Unit.UnitState：
```
InRally → Moving → Fighting → Dead
                ↑         ↓
                ←─ 目标死亡 ←─
```
- Moving：向目标移动，进入攻击范围后切换 Fighting
- Fighting：攻击目标，目标死亡后重新 FindTarget → Moving
- Dead：播放死亡效果，移除

**1.3 攻击逻辑**
- 攻击间隔 = 1 / AttackSpeed
- 每次攻击对目标造成 AttackDamage 伤害
- 目标 CurrentHp <= 0 → 死亡
- 近战：攻击范围小（如 30px）
- 远程（预留）：攻击范围大，需要发射投射物

**1.4 血条显示**
- 每个 Unit 上方显示血条（ColorRect 底条 + 绿色/红色当前血量条）
- 血量低于 50% 变黄，低于 25% 变红

**1.5 死亡处理**
- 播放简单效果（闪烁后消失）
- 从场景树中移除（QueueFree）
- 通知相关系统（如被该 Unit 作为目标的其他 Unit 需要重新寻敌）

---

### Step 2：兵种数据化

**2.1 UnitData Resource**
```csharp
[GlobalClass]
public partial class UnitData : Resource
{
    [Export] public string UnitName { get; set; }
    [Export] public int MaxHp { get; set; }
    [Export] public float MoveSpeed { get; set; }
    [Export] public float AttackDamage { get; set; }
    [Export] public float AttackSpeed { get; set; }
    [Export] public float AttackRange { get; set; }  // 近战 30, 远程 200+
    [Export] public Color UnitColor { get; set; }
    [Export] public Team UnitTeam { get; set; }
}
```

**2.2 UnitDatabase — Phase 2 兵种**
| 兵种 | 类型 | HP | 攻击 | 攻速 | 范围 | 特点 |
|------|------|-----|------|------|------|------|
| 剑士 | 近战 | 120 | 15 | 1.0 | 30 | 均衡步兵 |
| 弓箭手 | 远程 | 70 | 12 | 1.2 | 200 | 远程输出 |
| 骑兵 | 近战 | 100 | 20 | 0.8 | 35 | 高速高攻 |

**2.3 军事建筑关联 UnitData**
- BuildingData 新增 `UnitData` 字段
- MilitaryBuilding 用 UnitData 初始化生产的 Unit
- BuildPanel 增加更多建筑选项

---

### Step 3：技能系统

**3.1 Skill 基类**
```csharp
public abstract partial class Skill : Node
{
    public Unit Owner { get; set; }
    public float Cooldown { get; set; }
    public abstract bool CanActivate();
    public abstract void Activate();
}
```

**3.2 技能触发器（5种）**

| 类型 | 类名 | 触发条件 |
|------|------|----------|
| 蓄能 | ChargeSkill | energy >= maxEnergy（攻击/受击积累） |
| 冷却 | CooldownSkill | timer >= cooldown |
| 血量 | ThresholdSkill | CurrentHp / MaxHp < threshold |
| 死亡 | DeathSkill | 阵亡时触发 |
| 被动 | PassiveSkill | 始终生效（_Process） |

**3.3 Phase 2 示例技能**
| 技能 | 类型 | 效果 |
|------|------|------|
| 旋风斩 | 蓄能 | 剑士蓄满后对周围敌人造成 AOE 伤害 |
| 火箭 | 冷却 | 弓箭手每 8 秒发射一发高伤害箭矢 |
| 狂暴 | 血量阈值 | 骑兵血量 < 30% 时攻击力翻倍 |

**3.4 技能与 Unit 集成**
- Unit 持有 List<Skill>
- UnitData 定义技能配置
- Unit._Process 中调用 Skill.CanActivate() → Activate()
- 技能释放时显示简单视觉反馈（文字/闪光）

---

### Step 4：防御建筑

**4.1 DefenseBuilding 子类**

| 建筑 | 费用 | HP | 功能 |
|------|------|-----|------|
| 城墙 | 30 | 300 | 阻挡敌兵前进（有碰撞体积），不攻击 |
| 箭塔 | 80 | 120 | 周期攻击范围内敌兵（覆盖走廊） |

**4.2 城墙机制**
- 放置在玩家区格子上
- 有碰撞体积，敌方士兵需要攻击摧毁后才能通过
- 生命值高，但不能攻击

**4.3 箭塔机制**
- 攻击范围覆盖走廊区域
- 自动攻击范围内最近敌兵
- 有攻击间隔和伤害值
- DefenseBuilding._Process 中寻找目标并攻击

**4.4 BuildPanel / BuildingDatabase 更新**
- 新增城墙和箭塔的 BuildingData
- BuildPanel 显示 4 种建筑（农场、兵营、城墙、箭塔）

---

### Step 5：敌方系统

**5.1 EnemyBase — 敌方基地**
- 位于地图最右侧区域
- 有基地核心（生命值 = 通关条件）
- 有预设的敌方建筑（兵营、箭塔等）
- 基地核心被摧毁 = 玩家胜利

**5.2 EnemySpawner — 敌方出兵**
- 取代 Phase 1 的敌方基地占位色块
- 按照关卡配置周期性产生敌方士兵
- 敌方士兵向左移动，目标是玩家基地
- 不同时间段出不同兵种（前期弱兵，后期强兵）

**5.3 敌方士兵**
- 复用 Unit 类，Team = Enemy
- 向左移动（与玩家士兵相反）
- 同样有寻敌、战斗、技能逻辑
- 颜色区分（红色系）

**5.4 EnemyWaveConfig — 波次配置**
```csharp
public class EnemyWave
{
    public float StartTime { get; set; }     // 第几秒开始
    public string UnitType { get; set; }     // 兵种
    public int Count { get; set; }           // 数量
    public float Interval { get; set; }      // 每个间隔
    public int Row { get; set; }             // 从哪行出发（-1 = 随机）
}
```

---

### Step 6：玩家基地与胜负判定

**6.1 PlayerBase**
- 位于地图最左侧
- 有基地核心（生命值，如 500）
- 被敌方士兵攻击时扣血
- 血量归零 = 游戏失败

**6.2 GameState 管理**
```
enum GameState { Preparing, Playing, Victory, Defeat }
```
- Playing：正常游戏
- Victory：敌方基地核心 HP <= 0
- Defeat：玩家基地核心 HP <= 0
- 胜负时暂停游戏 + 显示结果 UI

**6.3 胜负 UI**
- 胜利：显示"胜利！"+ 简单统计（用时、造兵数等）
- 失败：显示"失败…"+ 重试按钮

---

### Step 7：UnitManager 全局管理

**7.1 UnitManager**
- 管理场上所有活跃 Unit（玩家+敌方）
- 提供查询接口：
  - `GetNearestEnemy(Vector2 pos, Team myTeam, float range)`
  - `GetUnitsInRange(Vector2 pos, float range, Team team)`
  - `RemoveUnit(Unit unit)`
- 士兵死亡时从列表移除

**7.2 性能考虑**
- Phase 2 单位数量不会太多（< 50），简单遍历即可
- 预留空间网格优化接口（Phase 4 如果需要）

---

## 任务依赖关系

```
Step 1 (战斗AI) ────→ Step 3 (技能系统)
     ↓
Step 2 (兵种数据) ──→ Step 4 (防御建筑)
     ↓
Step 7 (UnitManager) → Step 5 (敌方系统) → Step 6 (胜负判定)
```

---

## 完成标准（Definition of Done）

Phase 2 完成时，你应该能：

1. ✅ 玩家士兵与敌方士兵在走廊相遇并自动交战
2. ✅ 士兵有血条，受伤变色，死亡消失
3. ✅ 至少 3 个兵种（剑士、弓箭手、骑兵），各有一个技能
4. ✅ 放置城墙阻挡敌兵，放置箭塔攻击走廊敌兵
5. ✅ 敌方基地周期派兵进攻
6. ✅ 摧毁敌方基地核心 → 显示胜利
7. ✅ 玩家基地被摧毁 → 显示失败 + 重试
8. ✅ 完整体验"防守 → 发展 → 反攻"流程
