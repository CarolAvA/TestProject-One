# 反幸存者项目：全面配置界面规则文档

## 0. 文档目标

本文档用于设计一套完整的 **游戏配置界面 / 编辑器界面**。

该配置界面需要支持配置以下内容：

1. AI角色相关配置  
2. AI角色BD成长配置  
3. AI角色普通攻击、弹幕、音效、Icon、特效配置  
4. 玩家操控的造物主方怪物列表  
5. 玩家操控的造物主方技能列表  
6. 技能效果配置  
7. Buff / Debuff效果配置  
8. 弹道类型配置  
9. 弹幕形态配置  
10. 单位序列帧动画配置  
11. 出生、待机、移动、攻击、受击、死亡等动画配置  
12. 弹道特效、技能特效、命中特效、地面预警特效配置  
13. 音效文件配置  
14. Icon资源配置  
15. 数据校验、预览、测试和导出配置  

核心目标：

> 让策划、美术、程序都能通过一个统一配置界面，完成角色、怪物、技能、弹幕、BD、Buff、动画、音效、特效的完整配置，并能实时预览和测试。

---

# 1. 配置界面整体结构

## 1.1 编辑器总布局

推荐采用五区结构：

```text
┌──────────────────────────────────────────────────────────────┐
│ 顶部工具栏：保存 / 导入 / 导出 / 校验 / 运行测试 / 搜索 / 版本信息 │
├──────────────┬───────────────────────────────┬───────────────┤
│ 左侧配置导航 │ 中央主配置区                    │ 右侧资源预览区 │
│              │                               │               │
│ AI角色       │ 表单字段 / 表格 / 节奏时间轴     │ Icon预览       │
│ BD配置       │ 技能效果编辑                    │ 音效试听       │
│ 怪物配置     │ Buff效果编辑                    │ 动画预览       │
│ 技能配置     │ 弹道编辑                        │ 特效预览       │
│ 弹道配置     │ 动画配置                        │ 依赖引用       │
│ Buff配置     │                               │               │
├──────────────┴───────────────────────────────┴───────────────┤
│ 底部状态栏：错误提示 / 缺失资源 / 引用关系 / 当前选中对象路径     │
└──────────────────────────────────────────────────────────────┘
```

---

## 1.2 页面模块划分

左侧导航分为以下模块：

```text
配置中心
├── AI角色配置
│   ├── 角色基础信息
│   ├── 基础属性
│   ├── AI行为逻辑
│   ├── AI攻击能力
│   ├── AI音效节奏
│   ├── AI BD成长
│   ├── AI Buff抗性
│   └── AI资源绑定
│
├── 造物主方配置
│   ├── 怪物列表
│   ├── 怪物详细配置
│   ├── 怪物动画配置
│   ├── 怪物技能配置
│   ├── 怪物Buff配置
│   ├── 召唤规则
│   ├── 精英怪配置
│   ├── Boss配置
│   └── 造物主技能配置
│
├── 公共战斗配置
│   ├── 技能效果库
│   ├── Buff效果库
│   ├── 弹道类型库
│   ├── 弹幕形态库
│   ├── 伤害公式库
│   ├── 目标选择规则库
│   ├── 预警范围库
│   └── 状态标签库
│
├── 资源配置
│   ├── Icon资源库
│   ├── 音效资源库
│   ├── 特效资源库
│   ├── 序列帧动画库
│   ├── 弹道贴图库
│   ├── UI资源库
│   └── 资源引用检查
│
├── 预览测试
│   ├── 单位预览
│   ├── 技能预览
│   ├── 弹幕预览
│   ├── Buff预览
│   ├── 节奏音轨预览
│   ├── 召唤测试
│   └── 战斗模拟
│
└── 数据管理
    ├── 数据校验
    ├── 批量导入
    ├── 批量导出
    ├── 配置版本
    ├── 引用关系
    └── 缺失资源检查
```

---

# 2. 通用配置字段规范

所有可配置对象都必须有统一基础字段。

## 2.1 通用基础字段

| 字段 | 类型 | 说明 |
|---|---|---|
| ConfigID | string | 唯一ID，不允许重复 |
| DisplayName | string | 游戏内显示名称 |
| InternalName | string | 内部名称，方便搜索 |
| Description | string | 策划说明 |
| Category | enum | 分类 |
| Tags | string[] | 标签 |
| Icon | asset | 图标资源 |
| Quality | enum | 品质 |
| IsEnabled | bool | 是否启用 |
| UnlockCondition | ref | 解锁条件 |
| SortOrder | int | 显示排序 |
| Notes | string | 备注 |
| LastModifiedBy | string | 最后修改人 |
| LastModifiedTime | datetime | 最后修改时间 |

---

## 2.2 通用资源字段

| 字段 | 类型 | 说明 |
|---|---|---|
| IconPath | asset path | Icon路径 |
| PrefabPath | asset path | 预制体路径 |
| VFXPath | asset path | 特效路径 |
| SFXPath | asset path | 音效路径 |
| SpriteSheetPath | asset path | 序列帧图路径 |
| MaterialPath | asset path | 材质路径 |
| AnimationClipPath | asset path | 动画资源路径 |
| UIAtlasPath | asset path | UI图集路径 |

---

## 2.3 通用预览按钮

所有配置页右上角统一放置预览按钮：

```text
[保存] [复制] [删除] [校验] [预览] [运行测试]
```

按钮规则：

| 按钮 | 功能 |
|---|---|
| 保存 | 保存当前配置 |
| 复制 | 复制当前配置生成新ID |
| 删除 | 删除配置，需要二次确认 |
| 校验 | 检查字段和资源是否缺失 |
| 预览 | 打开右侧预览区 |
| 运行测试 | 在测试沙盒中运行该配置 |

---

# 3. AI角色配置界面

## 3.1 AI角色列表页

### 页面用途

管理所有可被AI控制的英雄角色。

### 页面结构

```text
AI角色配置
├── 搜索栏
├── 筛选栏
│   ├── 全部
│   ├── 近战
│   ├── 远程
│   ├── 法师
│   ├── 召唤
│   ├── 防御
│   └── Boss型AI
├── 角色列表
│   ├── Icon
│   ├── 名称
│   ├── ID
│   ├── 默认BD池
│   ├── 默认音轨风格
│   └── 是否启用
└── 新建AI角色按钮
```

---

## 3.2 AI角色基础信息页

| 配置项 | 类型 | 说明 |
|---|---|---|
| AI角色ID | string | 唯一ID |
| 角色名称 | string | 游戏显示名称 |
| 角色Icon | asset | UI头像 |
| 角色立绘 | asset | 详情页显示图 |
| 角色阵营 | enum | 人族、法师、游侠、圣骑等 |
| 默认流派 | enum | 火焰、冰霜、雷电、毒气、音波等 |
| 默认AI等级 | int | 初始等级 |
| 默认攻击方式 | ref | 初始子弹攻击 |
| 默认节奏器 | ref | 初始音效节奏配置 |
| 默认BD池 | ref list | AI可抽取BD列表 |
| 角色说明 | text | 策划描述 |

---

## 3.3 AI角色基础属性页

| 属性 | 说明 |
|---|---|
| 最大生命值 | AI角色初始血量 |
| 移动速度 | AI自动走位速度 |
| 转向速度 | AI改变方向速度 |
| 护甲 | 降低物理伤害 |
| 魔抗 | 降低法术伤害 |
| 闪避率 | 躲避造物主技能概率基础值 |
| 拾取范围 | 自动拾取经验范围 |
| 经验倍率 | 击杀怪物获得经验效率 |
| 初始伤害 | 所有伤害基础值 |
| 弹体飞行速度 | 初始子弹速度 |
| 节奏速度 | 所有伤害释放CD / 音效节奏间隔 |
| 发射数量 | 每次释放额外发射数量 |
| 暴击率 | 造成暴击概率 |
| 暴击伤害 | 暴击倍率 |
| 攻击后摇 | 每段攻击后的可反制窗口 |
| 受击硬直 | 被控制或命中时的停顿 |

---

## 3.4 AI行为逻辑页

### 3.4.1 行为权重配置

| 行为 | 权重 | 说明 |
|---|---:|---|
| 远离怪物 | 0~100 | 周围怪物多时是否优先逃离 |
| 追击经验球 | 0~100 | 是否优先拾取经验 |
| 攻击召唤门 | 0~100 | 是否优先摧毁玩家召唤点 |
| 躲避造物主技能 | 0~100 | 是否优先躲避技能预警 |
| 攻击远程怪 | 0~100 | 是否优先清理远程怪 |
| 攻击精英怪 | 0~100 | 是否优先打精英 |
| 规避陷阱 | 0~100 | 是否避开毒沼、火区等 |
| 保持距离 | 0~100 | 是否保持安全距离 |
| 低血逃生 | 0~100 | 低血量时优先逃跑 |

---

### 3.4.2 AI躲避造物主技能逻辑

| 配置项 | 说明 |
|---|---|
| 是否允许躲避 | true / false |
| 预警识别时间 | AI看到预警后的反应延迟 |
| 躲避优先级 | 技能伤害越高优先级越高 |
| 躲避路径计算 | 选择离开危险区的最短路径 |
| 允许吃小伤害 | AI可以承受低威胁区域，不一定躲 |
| 被控制时能否躲避 | 默认不能 |
| 攻击后摇时能否躲避 | 可配置 |
| 蓄力期间能否取消攻击躲避 | 可配置 |

---

# 4. AI攻击能力配置界面

## 4.1 初始攻击配置

AI初始能力是发射一个子弹。

| 字段 | 说明 |
|---|---|
| AttackID | 攻击ID |
| AttackName | 攻击名称 |
| BaseProjectile | 默认子弹 |
| Damage | 基础伤害 |
| RhythmInterval | 释放节奏间隔 |
| ProjectileSpeed | 弹体速度 |
| ProjectileCount | 发射数量 |
| FireAngle | 发射角度 |
| SpreadAngle | 散射角度 |
| TargetRule | 目标选择规则 |
| AudioEvent | 发射音效 |
| MuzzleVFX | 发射口特效 |
| HitVFX | 命中特效 |
| Icon | 攻击Icon |

---

## 4.2 AI音效节奏配置页

### 页面用途

配置AI每次攻击对应的音效、节拍点、弹幕触发时间、UI进度条表现。

### 页面结构

```text
AI音效节奏配置
├── 节奏基础信息
│   ├── RhythmID
│   ├── 名称
│   ├── 总时长
│   ├── 循环方式
│   ├── 主音色
│   └── 危险等级
│
├── 音轨时间轴
│   ├── 低音轨
│   ├── 中音轨
│   ├── 高音轨
│   └── 当前播放指针
│
├── 节拍事件列表
│   ├── 时间点
│   ├── 音轨类型
│   ├── 音效文件
│   ├── 触发弹幕
│   ├── 触发特效
│   └── UI闪烁
│
└── 预览测试
    ├── 播放音轨
    ├── 发射弹幕
    ├── 显示预警
    └── 循环测试
```

---

## 4.3 节拍事件字段

| 字段 | 类型 | 说明 |
|---|---|---|
| TimePoint | float | 节拍触发时间 |
| TrackType | enum | Low / Mid / High |
| BeatStrength | enum | 小 / 中 / 大 / 重音 |
| SFX | asset | 当前节拍音效 |
| TriggerProjectile | ref | 触发弹道 |
| TriggerVFX | ref | 触发特效 |
| TriggerBuff | ref | 触发Buff |
| CameraShake | float | 镜头震动强度 |
| UIFlash | float | 进度条闪烁强度 |
| WarningShape | ref | 战场预警形状 |
| TriggeredBDList | ref list | 本节拍触发哪些BD |

---

# 5. AI BD成长配置界面

## 5.1 BD列表页

```text
AI BD配置
├── 搜索
├── 分类筛选
│   ├── 基础属性
│   ├── 额外弹体
│   ├── 路径区域
│   ├── AOE音波
│   ├── 节奏变化
│   ├── 状态伤害
│   ├── 特殊机制
│   └── 核心流派
└── BD表格
    ├── Icon
    ├── BD名称
    ├── 类型
    ├── 元素
    ├── 最大等级
    ├── 权重
    ├── 是否核心
    └── 是否启用
```

---

## 5.2 BD详细配置字段

| 字段 | 说明 |
|---|---|
| BDID | 唯一ID |
| BD名称 | 显示名称 |
| Icon | BD图标 |
| 类型 | 基础属性 / 弹体 / 路径 / AOE / 节奏 / 特殊 |
| 元素 | 火 / 冰 / 雷 / 毒 / 音波 / 无 |
| 品质 | 普通 / 稀有 / 史诗 / 核心 |
| 最大等级 | 可升级上限 |
| 抽取权重 | AI升级时出现概率 |
| 前置BD | 需要先获得哪些BD |
| 互斥BD | 与哪些BD不能同时出现 |
| 当前等级效果 | 每一级对应效果 |
| 音效表现 | 对音轨产生什么变化 |
| 特效表现 | 对弹幕和技能特效产生什么变化 |
| Tips文案 | 游戏内悬停说明 |
| 反制建议 | 给玩家看的反制提示 |
| 是否影响节奏器 | 是否改变音效进度条 |
| 是否影响弹体 | 是否改变弹体行为 |
| 是否影响Buff | 是否附加状态效果 |

---

## 5.3 BD等级效果配置

```text
BD等级配置
├── Lv.1
│   ├── 数值变化
│   ├── 新增效果
│   ├── 音效变化
│   └── 特效变化
├── Lv.2
├── Lv.3
├── Lv.4
└── Lv.5
```

示例：

| 等级 | 效果 |
|---|---|
| Lv.1 | 闪电弹命中后弹射1次 |
| Lv.2 | 弹射次数+1 |
| Lv.3 | 弹射伤害提高到65% |
| Lv.4 | 弹射范围提高20% |
| Lv.5 | 最后一次弹射产生小范围电爆 |

---

# 6. 造物主方怪物配置界面

## 6.1 怪物列表页

```text
怪物配置
├── 搜索栏
├── 类型筛选
│   ├── 小怪
│   ├── 远程
│   ├── 坦克
│   ├── 刺客
│   ├── 控制
│   ├── 法术
│   ├── 精英
│   └── Boss
├── 元素筛选
│   ├── 无
│   ├── 火
│   ├── 冰
│   ├── 雷
│   ├── 毒
│   ├── 暗影
│   └── 物理
└── 怪物表格
    ├── Icon
    ├── 名称
    ├── ID
    ├── 类型
    ├── 召唤消耗
    ├── 召唤冷却
    ├── 场上上限
    ├── 克制标签
    └── 是否启用
```

---

## 6.2 怪物基础配置

| 字段 | 说明 |
|---|---|
| MonsterID | 怪物唯一ID |
| MonsterName | 怪物名称 |
| Icon | 怪物Icon |
| MonsterType | 小怪 / 远程 / 坦克 / 刺客 / 精英 / Boss |
| ElementType | 元素类型 |
| Description | 怪物描述 |
| Prefab | 怪物预制体 |
| SpriteSheet | 主序列帧图 |
| SummonCost | 召唤消耗 |
| SummonCooldown | 召唤冷却 |
| MaxAliveCount | 场上最大数量 |
| SpawnLimitPerWave | 每波最大召唤数量 |
| UnlockCondition | 解锁条件 |
| CounterTags | 克制标签 |
| WeaknessTags | 被克制标签 |

---

## 6.3 怪物属性配置

| 属性 | 说明 |
|---|---|
| 最大生命 | 怪物血量 |
| 攻击力 | 基础伤害 |
| 攻击间隔 | 普攻CD |
| 移动速度 | 移动速度 |
| 转向速度 | 转向速度 |
| 攻击距离 | 进入攻击状态的距离 |
| 索敌范围 | 寻找AI角色的范围 |
| 护甲 | 物理减伤 |
| 魔抗 | 法术减伤 |
| 受击硬直 | 被打后的停顿 |
| 碰撞半径 | 角色碰撞大小 |
| 死亡经验 | AI击杀后获得经验 |
| 是否可被击退 | true / false |
| 是否可被冰冻 | true / false |
| 是否可被毒伤 | true / false |
| 是否可被点燃 | true / false |
| 是否飞行单位 | true / false |

---

## 6.4 怪物召唤规则配置

| 字段 | 说明 |
|---|---|
| CanSummonNearHero | 是否允许靠近AI召唤 |
| MinDistanceToHero | 距离AI最近召唤距离 |
| MaxDistanceToHero | 距离AI最远召唤距离 |
| CanSummonOnObstacle | 是否能在障碍物上召唤 |
| CanSummonInTrapArea | 是否能在陷阱区召唤 |
| SpawnDelay | 点击后多久出生 |
| SpawnWarningVFX | 出生前地面提示 |
| SpawnVFX | 出生特效 |
| SpawnSFX | 出生音效 |
| SpawnAnimation | 出生动画 |
| FormationRule | 多个单位生成阵型 |
| InitialState | 出生后初始状态 |

---

# 7. 怪物序列帧动画配置界面

## 7.1 动画列表

每个怪物必须配置完整动画组。

```text
怪物动画配置
├── 出生 Birth
├── 待机 Idle
├── 移动 Move
├── 攻击 Attack
├── 技能 Skill
├── 受击 Hit
├── 死亡 Death
├── 冰冻 Frozen
├── 中毒 Poisoned
├── 燃烧 Burning
├── 眩晕 Stun
└── 胜利 / 特殊表情 Special
```

---

## 7.2 单个动画字段

| 字段 | 说明 |
|---|---|
| AnimationID | 动画ID |
| AnimationType | Birth / Idle / Move / Attack / Death等 |
| SpriteSheet | 序列帧图 |
| FrameWidth | 单帧宽度 |
| FrameHeight | 单帧高度 |
| FrameCount | 帧数 |
| FPS | 播放帧率 |
| Loop | 是否循环 |
| PlayOnStateEnter | 进入状态时播放 |
| CanBeInterrupted | 是否可被打断 |
| Pivot | 轴心点 |
| SortingOffset | 渲染排序偏移 |
| EventFrameList | 动画事件帧 |
| HitFrame | 攻击判定帧 |
| SFXFrame | 播放音效帧 |
| VFXFrame | 播放特效帧 |

---

## 7.3 动画事件帧配置

| 事件类型 | 说明 |
|---|---|
| PlaySFX | 播放音效 |
| SpawnVFX | 生成特效 |
| SpawnProjectile | 发射弹道 |
| ApplyDamage | 造成伤害 |
| ApplyBuff | 添加Buff |
| ShakeCamera | 镜头震动 |
| ChangeCollider | 修改碰撞体 |
| DestroySelf | 销毁单位 |
| SpawnChildUnit | 生成子单位 |
| EnableTrail | 开启拖尾 |
| DisableTrail | 关闭拖尾 |

---

## 7.4 动画预览区

右侧预览区必须支持：

| 功能 | 说明 |
|---|---|
| 播放 / 暂停 | 播放当前动画 |
| 单帧前进 | 查看具体帧 |
| 循环播放 | 查看循环效果 |
| 速度倍率 | 0.25x / 0.5x / 1x / 2x |
| 事件帧显示 | 在时间轴上显示事件点 |
| 攻击判定框显示 | 显示攻击范围 |
| 碰撞框显示 | 显示碰撞范围 |
| 轴心点显示 | 显示Pivot |
| 特效叠加预览 | 查看动画触发特效 |
| 音效同步试听 | 查看动画事件音效 |

---

# 8. 造物主技能配置界面

## 8.1 造物主技能列表页

```text
造物主技能配置
├── 搜索栏
├── 分类筛选
│   ├── 伤害
│   ├── 控制
│   ├── 减速
│   ├── 禁疗
│   ├── 破盾
│   ├── 召唤强化
│   ├── 地形改变
│   ├── 陷阱
│   └── Boss辅助
└── 技能列表
    ├── Icon
    ├── 技能名称
    ├── 消耗
    ├── 冷却
    ├── 预警时间
    ├── AI可躲避
    ├── 技能类型
    └── 是否启用
```

---

## 8.2 技能基础配置

| 字段 | 说明 |
|---|---|
| SkillID | 技能ID |
| SkillName | 技能名称 |
| Icon | 技能Icon |
| SkillType | 伤害 / 控制 / Buff / 地形 / 召唤 |
| CostType | 造物主能量 / 特殊资源 |
| CostValue | 消耗数值 |
| Cooldown | 冷却时间 |
| CastMode | 点选 / 拖拽 / 指向 / 范围 / 全屏 |
| TargetRule | 目标选择规则 |
| RangeShape | 范围形状 |
| WarningTime | 预警时间 |
| CanAIDodge | AI是否有机会躲避 |
| DamageEffect | 伤害效果引用 |
| BuffEffect | Buff效果引用 |
| VFX | 技能主特效 |
| WarningVFX | 预警特效 |
| HitVFX | 命中特效 |
| SFX | 释放音效 |
| HitSFX | 命中音效 |
| Description | 技能说明 |
| CounterTags | 克制标签 |

---

## 8.3 技能释放流程配置

```text
玩家选择技能
↓
检测资源是否足够
↓
检测冷却是否完成
↓
进入瞄准状态
↓
显示技能范围
↓
玩家点击释放位置
↓
显示预警区域
↓
AI根据预警进行躲避判断
↓
预警结束
↓
技能生效
↓
造成伤害 / 添加Buff / 生成区域 / 召唤单位
↓
播放命中特效和音效
↓
进入冷却
```

---

## 8.4 技能预警配置

| 字段 | 说明 |
|---|---|
| WarningShape | 圆形 / 扇形 / 矩形 / 直线 / 多点 |
| WarningColor | 预警颜色 |
| WarningAlpha | 透明度 |
| WarningDuration | 预警持续时间 |
| WarningPulse | 是否脉冲闪烁 |
| WarningSFX | 预警音效 |
| ShowCountdown | 是否显示倒计时 |
| LockPosition | 预警后是否锁定位置 |
| FollowTarget | 是否跟随AI目标 |
| AIDodgeDifficulty | AI躲避难度 |

---

# 9. 技能效果库配置界面

## 9.1 技能效果类型

| 类型 | 说明 |
|---|---|
| DirectDamage | 直接伤害 |
| PeriodicDamage | 持续伤害 |
| AreaDamage | 范围伤害 |
| HealBlock | 禁疗 |
| ShieldBreak | 破盾 |
| Slow | 减速 |
| Stun | 眩晕 |
| Knockback | 击退 |
| Pull | 牵引 |
| Silence | 沉默 |
| Summon | 召唤单位 |
| SpawnArea | 生成区域 |
| ChangeTerrain | 改变地形 |
| AddBuff | 添加Buff |
| RemoveBuff | 移除Buff |

---

## 9.2 技能效果字段

| 字段 | 说明 |
|---|---|
| EffectID | 效果ID |
| EffectName | 效果名称 |
| EffectType | 效果类型 |
| DamageType | 物理 / 法术 / 火 / 冰 / 雷 / 毒 / 音波 |
| BaseValue | 基础数值 |
| ScalingType | 是否受属性加成 |
| Duration | 持续时间 |
| TickInterval | 间隔触发时间 |
| MaxStack | 最大层数 |
| ApplyRule | 命中后 / 进入区域 / 每次节拍 |
| TargetFilter | 目标过滤 |
| VFXOnApply | 应用时特效 |
| VFXOnTick | Tick特效 |
| VFXOnEnd | 结束特效 |
| SFXOnApply | 应用音效 |
| SFXOnTick | Tick音效 |
| SFXOnEnd | 结束音效 |
| BuffRef | 关联Buff |
| ProjectileRef | 关联弹道 |
| AreaRef | 关联区域 |

---

# 10. Buff / Debuff配置界面

## 10.1 Buff列表页

```text
Buff效果库
├── 搜索栏
├── 类型筛选
│   ├── 正面Buff
│   ├── 负面Debuff
│   ├── 控制
│   ├── 持续伤害
│   ├── 属性变化
│   ├── 免疫
│   └── 特殊机制
└── Buff表格
    ├── Icon
    ├── 名称
    ├── 类型
    ├── 持续时间
    ├── 最大层数
    ├── 是否可驱散
    └── 是否启用
```

---

## 10.2 Buff基础字段

| 字段 | 说明 |
|---|---|
| BuffID | Buff唯一ID |
| BuffName | Buff名称 |
| Icon | Buff图标 |
| BuffType | 正面 / 负面 / 控制 / DOT |
| ElementType | 元素 |
| Duration | 持续时间 |
| TickInterval | Tick间隔 |
| MaxStack | 最大层数 |
| StackRule | 刷新时间 / 叠加层数 / 独立计时 |
| CanDispel | 是否可驱散 |
| IsControl | 是否控制 |
| IsDOT | 是否持续伤害 |
| AttributeModifiers | 属性修改列表 |
| PeriodicEffects | 周期效果 |
| OnApplyEffects | 获得时效果 |
| OnRemoveEffects | 移除时效果 |
| VFXLoop | 持续特效 |
| SFXLoop | 持续音效 |
| UIIconState | UI显示状态 |
| Description | Tips说明 |

---

## 10.3 常用Buff模板

| Buff | 效果 |
|---|---|
| 冰冻 | 无法移动、无法攻击 |
| 减速 | 移动速度降低 |
| 中毒 | 每秒受到毒伤 |
| 燃烧 | 每0.5秒受到火焰伤害 |
| 眩晕 | 无法行动 |
| 禁疗 | 回复效果降低或无效 |
| 破甲 | 护甲降低 |
| 破盾 | 护盾值额外受到伤害 |
| 易伤 | 受到伤害提高 |
| 沉默 | 无法释放技能 |
| 恐惧 | 强制远离目标 |
| 定身 | 无法移动但可攻击 |
| 狂暴 | 攻速提高但防御降低 |

---

# 11. 弹道类型配置界面

## 11.1 弹道类型列表

```text
弹道类型库
├── 直线弹道
├── 追踪弹道
├── 曲线弹道
├── 弹射弹道
├── 穿透弹道
├── 回旋弹道
├── 环绕弹道
├── 抛物线弹道
├── 延迟落点弹道
├── 激光弹道
├── 音波扩散
└── 区域生成弹道
```

---

## 11.2 弹道基础字段

| 字段 | 说明 |
|---|---|
| ProjectileID | 弹道ID |
| ProjectileName | 弹道名称 |
| ProjectileType | 弹道类型 |
| Icon | 弹道Icon |
| Sprite / Mesh | 弹体显示资源 |
| TrailVFX | 拖尾特效 |
| LaunchVFX | 发射特效 |
| HitVFX | 命中特效 |
| LaunchSFX | 发射音效 |
| FlyLoopSFX | 飞行循环音 |
| HitSFX | 命中音效 |
| Speed | 飞行速度 |
| Acceleration | 加速度 |
| Lifetime | 生命周期 |
| CollisionRadius | 碰撞半径 |
| CanPierce | 是否穿透 |
| PierceCount | 穿透次数 |
| CanBounce | 是否弹射 |
| BounceCount | 弹射次数 |
| CanTrack | 是否追踪 |
| TrackStrength | 追踪强度 |
| DamageEffect | 伤害效果 |
| BuffEffect | 命中Buff |
| SpawnAreaOnPath | 路径区域 |
| SpawnAreaOnHit | 命中区域 |

---

## 11.3 弹道预览功能

右侧必须能测试：

| 功能 | 说明 |
|---|---|
| 单发测试 | 发射一枚弹体 |
| 多发测试 | 测试发射数量 |
| 散射角测试 | 查看弹幕角度 |
| 移速测试 | 调整速度即时预览 |
| 碰撞测试 | 放置假目标测试命中 |
| 弹射测试 | 放多个目标测试弹射 |
| 追踪测试 | 移动假目标测试追踪 |
| 路径区域测试 | 查看毒气 / 火焰 / 冰霜轨迹 |
| 命中特效测试 | 查看HitVFX |
| 音效同步测试 | 听发射、飞行、命中音效 |

---

# 12. 弹幕形态配置界面

## 12.1 弹幕形态类型

| 类型 | 说明 |
|---|---|
| SingleShot | 单发 |
| MultiShot | 多发 |
| Spread | 扇形散射 |
| Circle | 环形弹幕 |
| Spiral | 螺旋弹幕 |
| RandomScatter | 随机散射 |
| TargetBurst | 对目标连射 |
| WaveLine | 波浪直线 |
| Cross | 十字弹幕 |
| Star | 星形弹幕 |
| Orbit | 环绕弹 |
| Rain | 从天而降 |
| LaserSweep | 激光扫射 |
| RhythmSequence | 节奏序列弹幕 |

---

## 12.2 弹幕形态字段

| 字段 | 说明 |
|---|---|
| PatternID | 弹幕形态ID |
| PatternName | 弹幕形态名称 |
| ProjectileRef | 使用的弹道 |
| SpawnCount | 发射数量 |
| WaveCount | 波次数 |
| IntervalBetweenWaves | 波次间隔 |
| SpreadAngle | 散射角度 |
| StartAngle | 起始角度 |
| RotatePerWave | 每波旋转角度 |
| RandomAngleOffset | 随机偏移 |
| TargetMode | 朝向目标 / 朝向移动方向 / 固定方向 |
| SpawnRadius | 生成半径 |
| SpawnOffset | 生成偏移 |
| RhythmEventBinding | 绑定节拍事件 |
| PreviewDuration | 预览持续时间 |

---

# 13. 特效配置界面

## 13.1 特效类型

| 类型 | 说明 |
|---|---|
| CastVFX | 释放特效 |
| WarningVFX | 预警特效 |
| ProjectileVFX | 弹体特效 |
| TrailVFX | 拖尾特效 |
| HitVFX | 命中特效 |
| AreaVFX | 区域持续特效 |
| BuffVFX | Buff持续特效 |
| DeathVFX | 死亡特效 |
| SpawnVFX | 出生特效 |
| UpgradeVFX | 升级特效 |
| RhythmVFX | 音效节拍特效 |

---

## 13.2 特效字段

| 字段 | 说明 |
|---|---|
| VFXID | 特效ID |
| VFXName | 特效名称 |
| VFXType | 特效类型 |
| AssetPath | 特效资源路径 |
| Duration | 持续时间 |
| Scale | 缩放 |
| FollowTarget | 是否跟随目标 |
| AttachPoint | 挂点 |
| Offset | 位置偏移 |
| RotationRule | 旋转规则 |
| SortingLayer | 渲染层级 |
| Loop | 是否循环 |
| StopOnOwnerDeath | 角色死亡是否停止 |
| ColorOverride | 可选颜色覆盖 |
| Intensity | 强度 |
| PoolingKey | 对象池Key |

---

## 13.3 特效预览区

必须支持：

1. 单独播放特效。
2. 绑定到单位身上播放。
3. 绑定到弹道上播放。
4. 在地面指定位置播放。
5. 循环播放。
6. 调整缩放。
7. 调整持续时间。
8. 查看透明度和层级。
9. 同步音效试听。
10. 查看性能开销提示。

---

# 14. 音效配置界面

## 14.1 音效资源类型

| 类型 | 说明 |
|---|---|
| AttackSFX | 普攻音效 |
| SkillSFX | 技能释放音效 |
| ProjectileSFX | 弹体飞行音效 |
| HitSFX | 命中音效 |
| DeathSFX | 死亡音效 |
| SpawnSFX | 出生音效 |
| WarningSFX | 预警音效 |
| BuffSFX | Buff音效 |
| RhythmBeatSFX | 节拍音效 |
| UIConfirmSFX | UI确认音 |
| UICancelSFX | UI取消音 |

---

## 14.2 音效字段

| 字段 | 说明 |
|---|---|
| SFXID | 音效ID |
| SFXName | 音效名称 |
| AudioClipPath | 音频文件路径 |
| Category | 音效分类 |
| Volume | 音量 |
| Pitch | 音高 |
| RandomPitchRange | 随机音高范围 |
| Loop | 是否循环 |
| SpatialBlend | 2D / 3D音效 |
| MaxDistance | 最大听距 |
| Priority | 优先级 |
| Cooldown | 同音效最小播放间隔 |
| CanOverlap | 是否允许重叠播放 |
| TrackType | 高音 / 中音 / 低音 |
| RhythmTag | 节奏标签 |
| StopCondition | 停止条件 |

---

## 14.3 音效播放器测试

右侧音效预览区需要支持：

| 功能 | 说明 |
|---|---|
| 试听 | 播放当前音效 |
| 循环试听 | 循环播放 |
| 音量调节 | 即时调整 |
| 音高调节 | 即时调整 |
| 节拍试听 | 按节奏播放多个音效 |
| 高中低音轨试听 | 同时模拟音效进度条 |
| 音效与弹幕同步测试 | 节拍点触发弹幕 |
| 音效重叠测试 | 测试多发弹体音效是否混乱 |

---

# 15. Icon资源配置界面

## 15.1 Icon类型

| 类型 | 用途 |
|---|---|
| AI角色Icon | AI头像 |
| BDIcon | BD图标 |
| 怪物Icon | 怪物召唤按钮 |
| 技能Icon | 造物主技能按钮 |
| BuffIcon | Buff状态图标 |
| 弹道Icon | 弹道配置显示 |
| 元素Icon | 火冰雷毒音波 |
| 分类Icon | 筛选分类 |
| 警告Icon | 危险提示 |
| 资源Icon | 召唤能量等 |

---

## 15.2 Icon字段

| 字段 | 说明 |
|---|---|
| IconID | Icon唯一ID |
| IconName | 名称 |
| IconType | 类型 |
| TexturePath | 贴图路径 |
| AtlasPath | 所属图集 |
| SpriteRect | 图集中区域 |
| DefaultSize | 默认尺寸 |
| ColorTag | 元素颜色标签 |
| QualityFrame | 默认品质框 |
| IsTransparent | 是否带透明通道 |
| PreviewBackground | 预览背景 |

---

# 16. 目标选择规则库

## 16.1 目标规则类型

| 规则 | 说明 |
|---|---|
| NearestTarget | 最近目标 |
| LowestHP | 血量最低 |
| HighestThreat | 威胁最高 |
| RandomTarget | 随机目标 |
| ForwardDirection | 朝前方发射 |
| MousePosition | 玩家指定位置 |
| HeroPosition | AI当前位置 |
| AreaCenter | 区域中心 |
| SummonPoint | 召唤点 |
| TaggedTarget | 带指定标签目标 |

---

## 16.2 目标过滤字段

| 字段 | 说明 |
|---|---|
| TargetFaction | 目标阵营 |
| IncludeTags | 必须包含标签 |
| ExcludeTags | 排除标签 |
| MaxTargetCount | 最大目标数 |
| SearchRadius | 搜索半径 |
| AngleLimit | 角度限制 |
| LineOfSight | 是否需要视线 |
| PrioritizeElite | 是否优先精英 |
| PrioritizeLowHP | 是否优先残血 |
| AllowDeadTarget | 是否允许死亡目标 |

---

# 17. 预警范围配置界面

## 17.1 预警类型

| 类型 | 用途 |
|---|---|
| Circle | 圆形范围 |
| Rectangle | 矩形范围 |
| Line | 直线范围 |
| Sector | 扇形范围 |
| Ring | 环形范围 |
| MultiPoint | 多点落雷 |
| FollowTarget | 跟随目标 |
| ExpandingCircle | 扩散圆 |
| DelayedArea | 延迟区域 |

---

## 17.2 预警字段

| 字段 | 说明 |
|---|---|
| WarningID | 预警ID |
| ShapeType | 范围形状 |
| Radius | 半径 |
| Width | 宽度 |
| Length | 长度 |
| Angle | 扇形角度 |
| Duration | 持续时间 |
| Color | 颜色 |
| Alpha | 透明度 |
| PulseSpeed | 闪烁速度 |
| EdgeVFX | 边缘特效 |
| FillVFX | 填充特效 |
| CountdownText | 是否显示倒计时 |
| SFX | 预警音效 |
| CanAIDodge | 是否触发AI躲避 |

---

# 18. 伤害公式配置界面

## 18.1 伤害公式字段

| 字段 | 说明 |
|---|---|
| FormulaID | 公式ID |
| FormulaName | 公式名称 |
| BaseDamage | 基础伤害 |
| DamageScale | 伤害倍率 |
| AttributeSource | 取哪个属性加成 |
| ElementType | 元素类型 |
| CriticalAllowed | 是否能暴击 |
| ArmorReduceType | 护甲减免方式 |
| ResistanceReduceType | 抗性减免方式 |
| MinDamage | 最小伤害 |
| MaxDamage | 最大伤害 |
| RandomRange | 随机浮动 |
| LevelScale | 等级成长倍率 |
| StackScale | 层数成长倍率 |

---

# 19. 状态标签系统配置

## 19.1 标签用途

标签用于控制克制、筛选、触发效果。

示例：

```text
Fire
Ice
Lightning
Poison
Sound
Projectile
AOE
PathArea
Summon
Elite
Boss
Shield
Heal
Slow
Control
Flying
Ground
HighThreat
LowHP
```

---

## 19.2 标签配置字段

| 字段 | 说明 |
|---|---|
| TagID | 标签ID |
| TagName | 标签名称 |
| TagGroup | 标签组 |
| Description | 说明 |
| Color | UI显示颜色 |
| Icon | 标签Icon |
| IsSystemTag | 是否系统标签 |
| CanStack | 是否可叠加 |

---

# 20. 配置界面右侧预览区

## 20.1 预览区结构

```text
PreviewPanel
├── PreviewViewport
│   ├── 单位显示区
│   ├── 弹道显示区
│   ├── 技能范围显示区
│   └── 特效显示区
├── PreviewControls
│   ├── 播放
│   ├── 暂停
│   ├── 重置
│   ├── 单帧
│   ├── 速度倍率
│   └── 循环
├── AudioPreview
│   ├── 播放音效
│   ├── 音量
│   ├── 音高
│   └── 节拍测试
└── DebugInfo
    ├── 当前帧
    ├── 当前状态
    ├── 当前事件
    ├── 当前伤害
    └── 当前Buff
```

---

## 20.2 预览模式

| 模式 | 说明 |
|---|---|
| 单位预览 | 查看怪物或AI角色动画 |
| 技能预览 | 查看技能预警、释放、命中 |
| 弹道预览 | 查看弹体飞行和命中 |
| Buff预览 | 查看Buff特效和UI |
| 音效节奏预览 | 查看音轨和弹幕同步 |
| 战斗模拟 | AI角色和怪物在小场景中测试 |
| 性能预览 | 查看当前配置可能造成的性能压力 |

---

# 21. 数据校验规则

## 21.1 必填字段校验

所有配置必须检查：

| 校验项 | 说明 |
|---|---|
| ID不能为空 | 每个配置必须有ID |
| ID不能重复 | 全局唯一 |
| 名称不能为空 | 用于显示 |
| Icon不能为空 | 游戏内需要展示 |
| 资源路径有效 | 不能引用不存在文件 |
| 数值不能为负 | 消耗、冷却、伤害等 |
| 最大等级合法 | BD等级不能小于1 |
| 引用对象存在 | 技能引用的Buff、弹道必须存在 |
| 循环引用检查 | Buff、技能不能无限递归 |
| 动画帧数合法 | 帧数必须匹配图片尺寸 |

---

## 21.2 资源缺失校验

| 缺失类型 | 处理 |
|---|---|
| Icon缺失 | 显示红色错误 |
| 音效缺失 | 显示黄色警告 |
| 特效缺失 | 显示黄色警告 |
| 动画缺失 | 显示红色错误 |
| 命中特效缺失 | 显示黄色警告 |
| 弹道资源缺失 | 显示红色错误 |
| Buff图标缺失 | 显示黄色警告 |

---

## 21.3 数值风险校验

| 风险 | 提示 |
|---|---|
| 冷却为0 | 可能造成无限释放 |
| 伤害过高 | 可能秒杀目标 |
| 弹体数量过多 | 可能造成性能问题 |
| Buff持续时间过长 | 可能影响平衡 |
| 召唤消耗为0 | 可能无限召唤 |
| 召唤数量过高 | 可能卡顿 |
| 音效允许无限重叠 | 可能造成噪音 |
| 特效持续时间过长 | 可能堆积 |

---

# 22. 导入导出规则

## 22.1 导出格式

推荐支持：

| 格式 | 用途 |
|---|---|
| JSON | 程序读取 |
| CSV | 策划批量编辑 |
| Excel | 大规模数值配置 |
| ScriptableObject | Unity项目内资源 |
| Addressable引用表 | 资源加载 |

---

## 22.2 导出内容

```text
导出包
├── AI角色配置
├── AI BD配置
├── 怪物配置
├── 造物主技能配置
├── 技能效果库
├── Buff效果库
├── 弹道类型库
├── 弹幕形态库
├── 音效资源引用
├── Icon资源引用
├── 特效资源引用
└── 动画配置
```

---

# 23. 推荐数据结构示例

## 23.1 怪物配置结构

```json
{
  "MonsterID": "monster_skeleton_01",
  "MonsterName": "骷髅兵",
  "MonsterType": "Melee",
  "Icon": "icons/monster_skeleton.png",
  "SummonCost": 10,
  "SummonCooldown": 1.5,
  "MaxAliveCount": 60,
  "Stats": {
    "HP": 100,
    "Damage": 12,
    "MoveSpeed": 3.5,
    "AttackInterval": 1.2,
    "AttackRange": 1.1
  },
  "Animations": {
    "Birth": "anim/skeleton_birth",
    "Idle": "anim/skeleton_idle",
    "Move": "anim/skeleton_move",
    "Attack": "anim/skeleton_attack",
    "Hit": "anim/skeleton_hit",
    "Death": "anim/skeleton_death"
  },
  "Skills": [
    "skill_skeleton_slash"
  ],
  "SpawnVFX": "vfx/summon_bone_smoke",
  "SpawnSFX": "sfx/summon_bone"
}
```

---

## 23.2 技能配置结构

```json
{
  "SkillID": "creator_thunder_strike",
  "SkillName": "造物主落雷",
  "Icon": "icons/skill_thunder.png",
  "SkillType": "Damage",
  "CostValue": 80,
  "Cooldown": 8,
  "CastMode": "AreaTarget",
  "WarningTime": 1.2,
  "CanAIDodge": true,
  "WarningShape": "circle_3m",
  "Effects": [
    "effect_thunder_damage",
    "effect_stun_short"
  ],
  "WarningVFX": "vfx/warning_thunder_circle",
  "CastVFX": "vfx/thunder_cast",
  "HitVFX": "vfx/thunder_hit",
  "CastSFX": "sfx/thunder_cast",
  "HitSFX": "sfx/thunder_hit"
}
```

---

## 23.3 弹道配置结构

```json
{
  "ProjectileID": "projectile_fireball_01",
  "ProjectileName": "火焰弹",
  "ProjectileType": "Linear",
  "Speed": 8,
  "Lifetime": 3,
  "CollisionRadius": 0.25,
  "CanPierce": false,
  "CanBounce": false,
  "DamageEffect": "effect_fireball_damage",
  "SpawnAreaOnHit": "area_fire_explosion",
  "Sprite": "projectiles/fireball.png",
  "TrailVFX": "vfx/fireball_trail",
  "HitVFX": "vfx/fireball_hit",
  "LaunchSFX": "sfx/fireball_launch",
  "HitSFX": "sfx/fireball_hit"
}
```

---

## 23.4 序列帧动画配置结构

```json
{
  "AnimationID": "anim_skeleton_attack",
  "AnimationType": "Attack",
  "SpriteSheet": "sprites/skeleton_attack.png",
  "FrameWidth": 128,
  "FrameHeight": 128,
  "FrameCount": 8,
  "FPS": 12,
  "Loop": false,
  "Pivot": {
    "x": 0.5,
    "y": 0.35
  },
  "EventFrameList": [
    {
      "Frame": 4,
      "EventType": "ApplyDamage",
      "EventRef": "effect_skeleton_slash"
    },
    {
      "Frame": 3,
      "EventType": "PlaySFX",
      "EventRef": "sfx_skeleton_attack"
    }
  ]
}
```

---

# 24. Unity UI预制体层级建议

## 24.1 配置主界面层级

```text
ConfigEditorCanvas
├── TopToolbar
│   ├── SaveButton
│   ├── ImportButton
│   ├── ExportButton
│   ├── ValidateButton
│   ├── TestRunButton
│   └── SearchInput
│
├── MainLayout
│   ├── LeftNavigationPanel
│   │   ├── ModuleTreeView
│   │   └── CreateNewButton
│   │
│   ├── CenterConfigPanel
│   │   ├── ConfigListView
│   │   ├── DetailInspector
│   │   ├── FieldGroupContainer
│   │   ├── TimelineEditor
│   │   └── TableEditor
│   │
│   └── RightPreviewPanel
│       ├── PreviewViewport
│       ├── AssetPreview
│       ├── AudioPreview
│       ├── AnimationPreview
│       ├── VFXPreview
│       └── ReferencePanel
│
└── BottomStatusPanel
    ├── ErrorList
    ├── WarningList
    ├── ReferenceInfo
    └── CurrentPathText
```

---

## 24.2 字段编辑组件库

配置界面需要复用以下字段组件：

| 组件 | 用途 |
|---|---|
| TextField | 文本输入 |
| NumberField | 数值输入 |
| ToggleField | bool开关 |
| DropdownField | 枚举选择 |
| AssetReferenceField | 资源引用 |
| ColorField | 颜色选择 |
| Vector2Field | 2D数值 |
| Vector3Field | 3D数值 |
| TagSelector | 标签选择 |
| ListEditor | 列表编辑 |
| TableEditor | 表格编辑 |
| TimelineEditor | 时间轴编辑 |
| CurveEditor | 曲线编辑 |
| PreviewButton | 预览按钮 |

---

# 25. 推荐工作流

## 25.1 配置一个新怪物

```text
新建怪物
↓
填写基础信息
↓
绑定Icon
↓
填写属性
↓
绑定出生、移动、攻击、死亡动画
↓
配置召唤消耗和冷却
↓
配置攻击技能
↓
绑定音效和特效
↓
运行怪物预览
↓
运行战斗测试
↓
校验通过
↓
保存
```

---

## 25.2 配置一个造物主技能

```text
新建技能
↓
选择技能类型
↓
填写消耗和冷却
↓
配置释放方式
↓
配置预警范围
↓
配置AI是否可躲避
↓
配置技能效果
↓
绑定预警特效、释放特效、命中特效
↓
绑定音效
↓
运行技能预览
↓
运行AI躲避测试
↓
校验通过
↓
保存
```

---

## 25.3 配置一个AI弹幕BD

```text
新建BD
↓
选择BD类型
↓
填写Icon和Tips
↓
配置等级效果
↓
绑定弹道或Buff
↓
绑定音效表现
↓
绑定特效表现
↓
加入AI角色BD池
↓
运行节奏弹幕测试
↓
校验通过
↓
保存
```

---

# 26. 最终要求总结

这套配置界面必须满足：

1. AI角色可以完整配置属性、攻击、节奏、BD、音效、特效、Icon。
2. 玩家造物主方可以完整配置怪物、召唤规则、技能、Boss、精英怪。
3. 所有技能效果、Buff效果、弹道类型、弹幕形态都能独立配置。
4. 单位序列帧动画必须支持出生、待机、移动、攻击、受击、死亡等完整状态。
5. 弹道和技能特效必须支持发射、飞行、命中、区域、持续、结束等完整生命周期。
6. 音效必须支持普通播放、命中播放、循环播放、节拍播放、高中低音轨配置。
7. Icon必须统一管理，并支持BD、怪物、技能、Buff等多种类型。
8. 所有配置必须有预览、测试、校验和缺失资源检查。
9. 配置界面要面向策划和美术，不能只给程序看。
10. 数据最终必须能导出给运行时系统读取。
