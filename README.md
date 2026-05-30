# 音浪围猎 / Sound Hunt

> **反向幸存者 · 音乐节奏 · 策略围杀**
>
> 你不再杀怪。你造怪杀人。

---

## 项目介绍

《音浪围猎》是一款**反向幸存者**（Reverse Survivor）游戏。

传统幸存者游戏中，你控制英雄在怪潮中活下去。本作完全反过来：你扮演**造物主**（关卡导演），通过召唤怪物、布置陷阱、释放技能，去击败一个由 AI 控制的、越来越强的**音乐狂人**。

AI 英雄会自动走位、自动升级、自动拾取肉鸽卡牌。你要做的，是通过屏幕下方的**音轨进度条**预判它的弹幕节奏，观察它的流派走向，然后选择克制兵种和时机，像设计一场死亡演唱会一样，把它逼入绝境。

### 核心特色

| 特色 | 说明 |
|---|---|
| **反向身份** | 玩家不是幸存者，是反派造物主 |
| **音乐节奏弹幕** | AI 的每次攻击绑定音乐节拍，玩家通过音轨预判 |
| **流派克制** | 火焰/吸血/护盾/机动等流派，各有明确弱点 |
| **赛前导演** | 开局改造战场：墙体、陷阱、召唤门、资源祭坛 |
| **动态博弈** | AI 越成长，玩家获得的反制手段也越强 |
| **冒泡吐槽** | AI 英雄被打中会弹出暴漫式吐槽气泡 |

---

## 技术栈

- **引擎**: Unity 6000.0.61f1 (Unity 6)
- **渲染管线**: URP (Universal Render Pipeline)
- **输入系统**: Unity Input System
- **反馈系统**: MoreMountains Feel (MMFeedbacks)
- **触觉反馈**: NiceVibrations (Lofelt)
- **版本控制**: Git + GitHub

---

## 项目结构

```
Assets/
├── Editor/              # 编辑器工具（配置编辑器、Build窗口）
├── Feel/                # 第三方插件：MoreMountains Feel（反馈系统）
├── Resources/           # 运行时资源
├── Scenes/              # 场景文件
│   └── SampleScene.unity  ← 主入口场景
├── Scripts/             # 核心代码
│   └── ReverseSurvivorPrototype/
│       ├── GameDirector.cs          # 全局游戏控制
│       ├── HeroController.cs        # AI 英雄控制
│       ├── MonsterUnit.cs           # 怪物单位
│       ├── AIRhythmSystem.cs        # 音乐节奏系统
│       ├── CreatorSkillSystem.cs    # 造物主技能系统
│       ├── DamageFeedbackSystem.cs  # 打击反馈
│       ├── PrototypeBootstrap.cs    # 启动器
│       └── ...
├── Settings/            # URP 渲染设置
Docs/                    # 完整设计文档（策划、美术、音效、UI）
Packages/                # 包清单
ProjectSettings/         # Unity 项目配置
```

---

## 🚀 启动方式

### 方式一：从 Unity Editor 启动（推荐开发/测试）

1. **克隆仓库**
   ```bash
   git clone git@github.com:CarolAvA/TestProject-One.git
   cd TestProject-One
   ```

2. **用 Unity 打开项目**
   - 安装 Unity Hub
   - 添加项目，选择 `TestProject-One` 文件夹
   - 使用 Unity 版本：**6000.0.61f1**（或兼容版本）

3. **打开主场景**
   - 在 Project 窗口中，导航到 `Assets/Scenes/`
   - 双击 `SampleScene.unity`

4. **点击 Play 运行**
   - 按顶部 ▶️ Play 按钮
   - 开始原型体验

### 方式二：重新导入第三方插件（首次打开需补充）

本项目使用了 **MoreMountains Feel** 插件，因版权原因已排除在版本控制外。

如果你需要完整运行反馈系统（屏幕震动、打击反馈等），请从 Unity Asset Store 重新导入：

1. 打开 Unity Asset Store → 搜索 `MoreMountains Feel`
2. 导入到项目中
3. 确认 `Assets/Feel/` 文件夹存在

> ⚠️ 没有 Feel 插件时，核心玩法逻辑仍可运行，但部分视觉/触觉反馈效果会缺失。

---

## 📄 设计文档

项目包含完整的设计文档，位于 `Docs/` 目录：

| 文档 | 内容 |
|---|---|
| `fan_xingcunzhe_zaowuzhu_shilian_gdd.md` | 核心玩法策划案（GDD） |
| `ai_barrage_audio_rhythm_system.md` | AI 弹幕音效节奏系统 |
| `ai_controlled_character_growth_bd_rhythm_system.md` | AI 角色 BD 成长系统 |
| `zaowuzhu_jineng_xitong.md` | 造物主技能系统 |
| `damage_feedback_hit_feel_system.md` | 打击反馈/受击表现系统 |
| `defeat_music_maniac_art_bible.md` | 美术指导文档（像素风） |
| `defeat_music_maniac_audio_sfx_bgm_design.md` | 音效与 BGM 配置 |
| `defeat_music_maniac_complete_ui_layout.md` | 完整 UI 布局规范 |
| `ai_current_bd_list_ui_rules.md` | AI BD 列表 UI 规则 |
| `comprehensive_config_editor_ui_design.md` | 配置编辑器设计 |

---

## 当前进度

- ✅ Unity 核心框架搭建
- ✅ AI 英雄行为系统（自动移动/攻击/升级）
- ✅ 造物主召唤系统（士兵抽屉）
- ✅ 音乐节奏系统（音轨进度条）
- ✅ 造物主技能系统（预警/释放/命中）
- ✅ 打击反馈系统（跳字/闪光/停顿）
- ✅ 完整设计文档覆盖
- 🔄 美术资源制作（待推进）
- 🔄 音效资源制作（待推进）
- 🔄 单局闭环验证（MVP 目标）

---

## 作者

**CarolAvA** — 策划 · 程序 · 项目管理

> 项目名：《音浪围猎》- 反向幸存者
> 联系方式：Niels101@163.com

---

*本项目为游戏原型验证阶段，欢迎 Star / Fork / Issue 反馈。*
