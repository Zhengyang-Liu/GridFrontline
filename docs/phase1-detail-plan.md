# Phase 1 — 核心原型 Detail Plan

## 目标

产出一个可运行的最小原型：
> 6×8 格子地图 → 放置建筑 → 经济建筑产金币 → 军事建筑产兵 → 士兵在集结区等待 → 玩家派兵 → 士兵在走廊2D自由移动

**不包含：** 战斗逻辑、敌方、防御建筑、卡牌系统、法术（这些属于 Phase 2/3）

---

## 项目结构

```
GridFrontline/
├── project.godot
├── GridFrontline.csproj
├── GridFrontline.sln
├── assets/
│   ├── textures/         # 占位图（格子、建筑、士兵）
│   └── fonts/
├── scenes/
│   ├── main/             # Main.tscn（入口场景）
│   ├── board/            # GameBoard.tscn（地图）
│   ├── buildings/        # 各建筑场景
│   ├── units/            # 士兵场景
│   └── ui/               # HUD 场景
├── scripts/
│   ├── Core/             # 全局管理器
│   │   ├── GameManager.cs
│   │   └── EconomyManager.cs
│   ├── Board/            # 地图与格子
│   │   ├── GameBoard.cs
│   │   ├── GridCell.cs
│   │   └── PlayerZone.cs
│   ├── Buildings/        # 建筑系统
│   │   ├── Building.cs          (抽象基类)
│   │   ├── EconomyBuilding.cs
│   │   └── MilitaryBuilding.cs
│   ├── Units/            # 士兵系统
│   │   ├── Unit.cs              (基类)
│   │   └── BasicSoldier.cs
│   ├── Rally/            # 集结系统
│   │   └── RallyZone.cs
│   └── UI/               # 界面
│       ├── HUD.cs
│       └── BuildPanel.cs
├── data/                 # 数据定义（Resource）
│   ├── buildings/
│   └── units/
└── docs/                 # 设计文档（已有）
```

---

## 任务分解

### Step 1：项目初始化

**1.1 创建 Godot 4 C# 项目**
- 初始化 project.godot、.csproj、.sln
- 创建文件夹结构
- 配置 .gitignore

**1.2 创建入口场景 Main.tscn**
- 包含 GameManager（AutoLoad 或场景根节点）
- 包含 Camera2D（固定视角，能看到整个地图）
- 基础窗口分辨率设定（建议 1920×1080）

---

### Step 2：格子地图系统

**2.1 GridCell — 单个格子**
- 属性：行（row）、列（col）、是否被占用（occupied）、持有建筑引用
- 视觉：矩形色块，hover 高亮，可点击
- 场景：Area2D + CollisionShape2D + Sprite2D（或 ColorRect）

**2.2 PlayerZone — 玩家区域（6×8）**
- 根据行列数动态生成 GridCell
- 格子尺寸参数化（如 64×64 px 或 80×80 px）
- 格子间距可调
- 坐标系：row 0 在上，col 0 在左

**2.3 GameBoard — 完整地图**
- 包含 PlayerZone（左侧）
- 包含走廊区域（中间，Phase 1 先用空白区域占位）
- 包含敌方基地区域（右侧，Phase 1 先用色块占位）
- 三个区域水平排列，可见边界

---

### Step 3：建筑基础系统

**3.1 Building 抽象基类**
```csharp
public abstract partial class Building : Node2D
{
    public int Cost { get; set; }          // 建造费用
    public int MaxHp { get; set; }         // 最大生命值
    public int CurrentHp { get; set; }     // 当前生命值
    public GridCell Cell { get; set; }     // 所在格子
    public abstract void OnTick(double delta);  // 周期逻辑
}
```

**3.2 建筑数据定义（Resource）**
- BuildingData.cs（Godot Resource 子类）
- 字段：名称、费用、生命值、图标、产出周期等
- 为农场和兵营各创建一个 .tres 文件

---

### Step 4：经济系统

**4.1 EconomyManager（全局/AutoLoad）**
- 管理当前金币数
- 提供 AddGold()、SpendGold()、CanAfford() 接口
- 金币变化时发出信号（signal），UI 监听

**4.2 EconomyBuilding — 经济建筑**
- 继承 Building
- 每隔 N 秒产出 X 金币（通过 Timer 或 OnTick 累计）
- 产出时播放简单视觉反馈（如数字飘字 "+5"）

**4.3 初始金币**
- 开局给予固定金币（如 100），足够放 1~2 个建筑

---

### Step 5：建筑放置交互

**5.1 BuildPanel — 建筑选择面板**
- 屏幕底部/侧边显示可放置的建筑列表（Phase 1 只有农场和兵营）
- 每个按钮显示：建筑图标 + 名称 + 费用
- 点击选中一个建筑类型

**5.2 放置流程**
```
点击 BuildPanel 选中建筑 → 鼠标移到格子上高亮预览 →
点击空格子 → 检查金币足够 → 扣金币 → 实例化建筑 → 格子标记 occupied
```

**5.3 放置规则**
- 格子未被占用
- 金币足够
- 不能放在非玩家区域

---

### Step 6：军事建筑与士兵生成

**6.1 MilitaryBuilding — 军事建筑**
- 继承 Building
- 生产计时器：每隔 N 秒尝试生成一个士兵
- **关键规则：如果集结区有该建筑的士兵等待，暂停生产**
- 生产完成时，士兵出现在关联的集结区

**6.2 Unit 基类**
```csharp
public partial class Unit : CharacterBody2D
{
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public float MoveSpeed { get; set; }
    public float AttackDamage { get; set; }
    public float AttackSpeed { get; set; }
    public enum UnitState { InRally, Moving, Fighting, Dead }
    public UnitState State { get; set; }
}
```

**6.3 BasicSoldier — 基础士兵**
- 继承 Unit
- Phase 1 只需实现：InRally 和 Moving 状态
- Moving 状态下向右移动（直线，Phase 1 无战斗目标）
- 用简单的 Sprite2D 或 ColorRect 表示

---

### Step 7：集结派兵系统

**7.1 RallyZone — 集结区**
- 位于玩家区域右侧、走廊左侧
- 每个军事建筑关联一个集结槽位
- 显示当前等待的士兵（图标/小人）
- 容量限制（如每个建筑最多集结 1 个，后续可调）

**7.2 派兵交互**
- 方案A：点击集结区的"出发"按钮，派出该集结区所有士兵
- 方案B：点击单个集结区派出，或"全部出发"按钮
- Phase 1 先用方案A（简单按钮）

**7.3 派兵后**
- 士兵状态从 InRally → Moving
- 士兵从集结区位置开始，向右移动进入走廊
- 关联的军事建筑恢复生产

---

### Step 8：走廊与士兵移动

**8.1 走廊区域**
- Phase 1 用空白的 2D 区域表示
- 士兵进入后 2D 自由移动（Phase 1 只是向右直线移动）
- 走廊右侧放一个占位目标点（代表敌方基地入口）

**8.2 士兵移动逻辑**
- 使用 CharacterBody2D + MoveAndSlide
- Phase 1 目标：向右移动到走廊终点后消失（或停留）
- 预留寻路接口（Phase 2 加入 NavigationAgent2D）

---

### Step 9：HUD 与调试

**9.1 HUD**
- 显示当前金币数（实时更新）
- 显示游戏速度/暂停按钮（可选）

**9.2 调试工具**
- 格子坐标显示（hover 时显示 row, col）
- 金币作弊按钮（+100 金币，方便测试）

---

## 任务依赖关系

```
Step 1 (项目初始化)
  ↓
Step 2 (格子地图) ──────→ Step 5 (建筑放置交互)
  ↓                              ↓
Step 3 (建筑基类) ──────→ Step 4 (经济系统)
  ↓                              ↓
Step 6 (军事建筑+士兵) ──→ Step 7 (集结派兵)
                                 ↓
                           Step 8 (走廊+移动)
                                 ↓
                           Step 9 (HUD+调试)
```

---

## 完成标准（Definition of Done）

Phase 1 原型完成时，你应该能：

1. ✅ 看到 6×8 格子地图 + 走廊 + 敌方基地占位
2. ✅ 从面板选择农场，点击格子放置，看到金币减少
3. ✅ 农场每隔几秒产出金币，HUD 数字增长
4. ✅ 从面板选择兵营，放置后开始倒计时生产士兵
5. ✅ 士兵出现在集结区，兵营停止生产
6. ✅ 点击"出发"，士兵向右移动进入走廊
7. ✅ 兵营恢复生产下一个士兵
8. ✅ 士兵在走廊中自由移动直到到达终点

---

## 用到的占位美术

Phase 1 全部使用色块/简单形状，不需要正式美术：
- 格子：浅灰色方块，hover 变绿
- 农场：黄色方块 + "F" 文字
- 兵营：红色方块 + "B" 文字
- 士兵：蓝色小圆形
- 走廊：深灰色背景
- 敌方基地：暗红色区域
