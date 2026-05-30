# 《击败音乐狂》完整音效与BGM配置文档
## 反幸存者｜音乐狂人主题｜搞怪像素风｜音效资源总表

---

# 0. 文档用途

本文档用于配置和制作《击败音乐狂》项目中的全部声音资源，包括：

1. BGM背景音乐  
2. BGM随机播放规则  
3. BGM强度切换规则  
4. AI音乐狂人弹幕音效  
5. AI音乐狂人节奏器音效  
6. 怪物出生、移动、攻击、受击、死亡音效  
7. 精英怪与Boss音效  
8. 造物主技能释放、预警、命中音效  
9. 各种元素受击音效  
10. 跳字、暴击、破盾、闪避、免疫音效  
11. Buff / Debuff音效  
12. UI点击、滑动、打开、关闭、错误提示音效  
13. 冒泡吐槽拟声  
14. 音效命名规范  
15. 音效配置字段  
16. Unity中推荐配置方式  

整体音频风格要服务项目核心概念：

> 玩家扮演造物主，击败一个越来越疯、越来越强、像在舞台上开失控演唱会的AI音乐狂人。

---

# 1. 总体音频风格

## 1.1 核心关键词

音频整体关键词：

- 搞怪
- 荒诞
- 节奏感强
- 电子感
- 像素感
- 暴漫感
- 地下Livehouse
- 失控演唱会
- Chiptune
- 8bit / 16bit合成器
- 低保真Lo-Fi
- 破音吉他
- 鼓点强
- 音响失真
- 夸张拟声
- 漫画式打击音

---

## 1.2 音频基调

项目不要做成严肃史诗音乐，也不要做成纯恐怖。  
应该像：

```text
地下演唱会 + 搞怪漫画 + 像素街机 + 疯狂音乐Boss战
```

声音可以有一点吵、有一点失真、有一点滑稽，但不能真的刺耳。

---

## 1.3 音效设计原则

| 原则 | 说明 |
|---|---|
| 每个元素有独立音色 | 火、冰、雷、毒、音波要一听就能区分 |
| AI攻击必须有节奏 | AI弹幕不是普通开火，而是音乐节拍触发 |
| 低音代表危险 | 低频重音通常对应大招、AOE、重击 |
| 高音代表高频弹幕 | 高频叮响通常代表快速小弹幕 |
| 中音代表普通攻击 | 中频短音用于常规弹幕 |
| 怪物音效要搞怪 | 不要太写实，可以有夸张拟声 |
| UI音效要像音乐控制台 | 点击、滑动、切换像DJ设备按钮 |
| 同类音效要做变体 | 避免重复疲劳 |
| 高频音效必须限频 | 防止满屏弹幕时声音糊成一团 |

---

# 2. 音频技术规格

## 2.1 文件格式

| 类型 | 推荐格式 | 说明 |
|---|---|---|
| BGM | OGG / WAV | Unity内建议OGG压缩流式播放 |
| 短音效 | WAV | 响应快、质量稳定 |
| UI音效 | WAV | 文件短，加载快 |
| 循环环境音 | OGG | 可循环压缩 |
| 语音拟声 | WAV | 方便快速触发 |

---

## 2.2 采样率与声道

| 资源类型 | 采样率 | 声道 |
|---|---:|---|
| BGM | 44.1kHz | Stereo |
| 重要技能音效 | 44.1kHz | Stereo |
| 普通SFX | 44.1kHz | Mono / Stereo |
| UI音效 | 44.1kHz | Mono |
| 怪物短叫声 | 44.1kHz | Mono |
| 环境音 | 44.1kHz | Stereo |

---

## 2.3 响度建议

| 类型 | 音量建议 |
|---|---:|
| BGM | -16 ~ -12 LUFS |
| 普通SFX | -12 ~ -9 LUFS |
| 重要技能SFX | -9 ~ -6 LUFS |
| UI音效 | -18 ~ -14 LUFS |
| 暴击 / Boss重击 | 可短暂更响，但不能爆音 |

---

## 2.4 音效长度建议

| 类型 | 长度 |
|---|---:|
| UI点击 | 0.05 ~ 0.15秒 |
| 普通子弹发射 | 0.08 ~ 0.2秒 |
| 普通命中 | 0.08 ~ 0.25秒 |
| 元素命中 | 0.15 ~ 0.5秒 |
| 技能释放 | 0.3 ~ 1.2秒 |
| 大招预警 | 0.8 ~ 2.5秒 |
| 怪物出生 | 0.4 ~ 1.2秒 |
| 怪物死亡 | 0.4 ~ 1.5秒 |
| Boss技能 | 1.0 ~ 3.0秒 |
| BGM | 60 ~ 180秒，可循环 |

---

# 3. 音频总线设计

## 3.1 Mixer分组

```text
Master
├── BGM
│   ├── MenuBGM
│   ├── BattleBGM
│   ├── BossBGM
│   └── ResultBGM
│
├── SFX
│   ├── AISFX
│   ├── MonsterSFX
│   ├── CreatorSkillSFX
│   ├── ProjectileSFX
│   ├── HitSFX
│   ├── BuffSFX
│   └── EnvironmentSFX
│
├── UI
│   ├── Click
│   ├── Confirm
│   ├── Warning
│   └── Popup
│
└── Voice
    ├── AIBubbleVoice
    ├── MonsterVoice
    └── BossVoice
```

---

## 3.2 音量默认值

| 总线 | 默认音量 |
|---|---:|
| Master | 100% |
| BGM | 70% |
| SFX | 85% |
| UI | 80% |
| Voice | 85% |
| Environment | 50% |

---

## 3.3 Ducking规则

当重要音效播放时，需要自动压低BGM。

| 触发 | BGM压低 |
|---|---:|
| AI大招 | -4dB |
| Boss出场 | -6dB |
| 造物主大技能命中 | -4dB |
| 胜利 / 失败音效 | -8dB |
| UI普通点击 | 不压低 |
| 普通弹幕 | 不压低 |

---

# 4. BGM系统

## 4.1 BGM播放规则

BGM需要支持随机播放。

基本规则：

```text
进入场景
↓
根据场景类型读取BGM池
↓
随机选择一首
↓
播放到结束或循环点
↓
如果设置为循环，循环播放
↓
如果设置为随机连续播放，当前曲结束后随机下一首
↓
同一首BGM不要连续重复
```

---

## 4.2 BGM模式

| 模式 | 说明 |
|---|---|
| RandomLoop | 从池子随机选一首循环 |
| RandomSequence | 播完一首随机下一首 |
| IntensityAdaptive | 根据战斗强度切换不同层 |
| BossOverride | Boss出现后强制切Boss BGM |
| ResultOverride | 结算时强制播放胜败音乐 |

---

## 4.3 BGM强度层

战斗BGM建议分为3层：

```text
低强度：开局、准备、怪物少
中强度：常规战斗
高强度：AI强势、Boss出现、玩家压制
```

可以做成三首，也可以做成同一首的三层轨道。

---

## 4.4 BGM资源表

| BGMID | 文件名 | 场景 | 风格 | BPM | 时长 | 循环 | 随机权重 |
|---|---|---|---|---:|---:|---|---:|
| BGM_Menu_01 | bgm_menu_backstage_groove.ogg | 主菜单 | 搞怪后台Funk+像素合成器 | 96 | 90s | 是 | 100 |
| BGM_Menu_02 | bgm_menu_broken_radio.ogg | 主菜单 | 破收音机Lo-Fi+轻鼓点 | 88 | 75s | 是 | 80 |
| BGM_Prepare_01 | bgm_prepare_stage_setup.ogg | 赛前布置 | 轻节奏、调音台、鼓棒敲击 | 92 | 80s | 是 | 100 |
| BGM_Prepare_02 | bgm_prepare_sneaky_creator.ogg | 赛前布置 | 造物主阴谋感、搞怪低音 | 86 | 70s | 是 | 80 |
| BGM_Battle_Low_01 | bgm_battle_low_noise_walk.ogg | 战斗低强度 | 像素鼓点+轻贝斯 | 108 | 100s | 是 | 100 |
| BGM_Battle_Low_02 | bgm_battle_low_livehouse_warmup.ogg | 战斗低强度 | 地下Livehouse热场 | 112 | 100s | 是 | 80 |
| BGM_Battle_Mid_01 | bgm_battle_mid_manic_beat.ogg | 战斗中强度 | 疯狂节拍+电吉他点缀 | 128 | 120s | 是 | 100 |
| BGM_Battle_Mid_02 | bgm_battle_mid_pixel_moshpit.ogg | 战斗中强度 | 像素朋克+合成器Bass | 132 | 120s | 是 | 90 |
| BGM_Battle_High_01 | bgm_battle_high_rhythm_riot.ogg | 战斗高强度 | 高速鼓组+失真合成器 | 150 | 130s | 是 | 100 |
| BGM_Battle_High_02 | bgm_battle_high_audio_collapse.ogg | 战斗高强度 | 混乱演唱会+破音吉他 | 156 | 130s | 是 | 90 |
| BGM_Boss_01 | bgm_boss_distortion_king.ogg | Boss | 重低音、舞台感、压迫 | 140 | 150s | 是 | 100 |
| BGM_Boss_02 | bgm_boss_final_dj_tyrant.ogg | Boss | DJ战斗曲、切片鼓点 | 148 | 160s | 是 | 100 |
| BGM_Boss_03 | bgm_boss_bad_metronome.ogg | Boss | 断拍、错拍、怪异节奏 | 126 | 150s | 是 | 80 |
| BGM_Victory_01 | bgm_result_victory_mocking_fanfare.ogg | 胜利 | 搞怪胜利小号+像素鼓 | 120 | 20s | 否 | 100 |
| BGM_Defeat_01 | bgm_result_defeat_ai_laugh.ogg | 失败 | AI嘲讽感、滑稽失败 | 80 | 18s | 否 | 100 |
| BGM_Pause_01 | bgm_pause_lofi_backstage.ogg | 暂停 | 低保真后台氛围 | 75 | 60s | 是 | 100 |

---

## 4.5 BGM切换逻辑

```text
战斗开始 → 播放低强度BGM
AI等级达到Lv.5 → 切中强度BGM
AI威胁值达到70% → 切高强度BGM
Boss出现 → 强制切Boss BGM
Boss死亡 / AI死亡 → 切胜利BGM
玩家失败 → 切失败BGM
```

---

## 4.6 BGM切换方式

| 切换类型 | 规则 |
|---|---|
| 普通强度切换 | 2秒淡出旧曲，2秒淡入新曲 |
| Boss切换 | 0.5秒快速压低旧曲，直接进入Boss起拍 |
| 胜负切换 | 立刻停止战斗BGM，播放结算音效后进BGM |
| 暂停 | 战斗BGM加低通滤波并降低音量 |

---

# 5. AI音乐狂人音效

## 5.1 AI通用角色音效

| SFXID | 文件名 | 触发 | 声音描述 | 变体 |
|---|---|---|---|---:|
| AI_Spawn_01 | sfx_ai_spawn_stage_entry_01.wav | AI登场 | 舞台灯打开+麦克风啸叫+短促欢呼 | 3 |
| AI_Idle_Mumble_01 | sfx_ai_idle_mumble_01.wav | 待机偶发 | 疯狂小声哼唱、碎碎念 | 5 |
| AI_Move_Step_01 | sfx_ai_move_step_01.wav | 移动脚步 | 轻快舞台鞋踩地，带节拍 | 4 |
| AI_Dash_01 | sfx_ai_dash_whoosh_01.wav | 闪避/冲刺 | 音符拖尾+风声 | 3 |
| AI_LevelUp_01 | sfx_ai_levelup_power_chord_01.wav | 升级 | 电吉他短扫弦+合成器上升 | 3 |
| AI_LowHP_01 | sfx_ai_lowhp_glitch_breath_01.wav | 低血 | 呼吸急促+破音电流 | 3 |
| AI_Death_01 | sfx_ai_death_record_scratch_01.wav | 死亡 | 唱片刮停+滑稽惨叫 | 3 |
| AI_Taunt_01 | sfx_ai_taunt_gibberish_01.wav | 嘲讽冒泡 | 短促怪叫、像暴漫吐槽 | 8 |

---

## 5.2 AI基础音符弹

| SFXID | 文件名 | 触发 | 声音描述 | 音轨 |
|---|---|---|---|---|
| AI_Bullet_Base_Fire_01 | sfx_ai_bullet_base_fire_01.wav | 基础弹发射 | 短促“哔！”电子音符弹 | 中音 |
| AI_Bullet_Base_Fly_01 | sfx_ai_bullet_base_fly_loop.wav | 弹体飞行 | 很轻的电子拖尾，不要太明显 | 中音 |
| AI_Bullet_Base_Hit_01 | sfx_ai_bullet_base_hit_01.wav | 基础弹命中 | 轻快像素碰撞音 | 中音 |
| AI_Bullet_Base_Multi_01 | sfx_ai_bullet_base_multishot_01.wav | 多重发射 | 连续两三个短哔音 | 中音 |

---

## 5.3 弹射闪电弹音效

| SFXID | 文件名 | 触发 | 声音描述 | 音轨 |
|---|---|---|---|---|
| AI_Bullet_Lightning_Fire_01 | sfx_ai_lightning_fire_01.wav | 闪电弹发射 | 高音电流“叮滋” | 高音 |
| AI_Bullet_Lightning_Fly_01 | sfx_ai_lightning_fly_loop.wav | 飞行 | 细小电流抖动 | 高音 |
| AI_Bullet_Lightning_Hit_01 | sfx_ai_lightning_hit_01.wav | 命中 | 短促电击“滋啦” | 高音 |
| AI_Bullet_Lightning_Bounce_01 | sfx_ai_lightning_bounce_01.wav | 弹射 | 快速跳电音，每弹一次一声 | 高音 |
| AI_Bullet_Lightning_Final_01 | sfx_ai_lightning_final_pop_01.wav | 最后一次弹射 | 小电爆 | 高音 |

---

## 5.4 冰冻弹音效

| SFXID | 文件名 | 触发 | 声音描述 | 音轨 |
|---|---|---|---|---|
| AI_Bullet_Ice_Fire_01 | sfx_ai_ice_fire_01.wav | 冰冻弹发射 | 清脆冰晶“叮” | 中高音 |
| AI_Bullet_Ice_Fly_01 | sfx_ai_ice_fly_loop.wav | 飞行 | 轻微寒风+冰粒声 | 中高音 |
| AI_Bullet_Ice_Hit_01 | sfx_ai_ice_hit_crack_01.wav | 命中 | 冰裂“咔嚓” | 中音 |
| AI_Bullet_Ice_Freeze_01 | sfx_ai_ice_freeze_apply_01.wav | 冰冻生效 | 冻结扩散声 | 中音 |
| AI_Bullet_Ice_Break_01 | sfx_ai_ice_break_end_01.wav | 冰冻结束 | 碎冰裂开 | 中音 |

---

## 5.5 火焰弹音效

| SFXID | 文件名 | 触发 | 声音描述 | 音轨 |
|---|---|---|---|---|
| AI_Bullet_Fire_Fire_01 | sfx_ai_fireball_fire_01.wav | 火焰弹发射 | 火焰喷出+短促鼓点 | 中低音 |
| AI_Bullet_Fire_Fly_01 | sfx_ai_fireball_fly_loop.wav | 飞行 | 小火球燃烧声 | 中低音 |
| AI_Bullet_Fire_Hit_01 | sfx_ai_fireball_hit_01.wav | 命中 | 火焰爆开“砰” | 低音 |
| AI_Bullet_Fire_AOE_01 | sfx_ai_fireball_aoe_01.wav | AOE爆炸 | 舞台喷火器爆燃 | 低音 |
| AI_Bullet_Fire_BurnTick_01 | sfx_ai_fire_burn_tick_01.wav | 燃烧Tick | 小火苗噼啪 | 中音 |

---

## 5.6 毒气弹音效

| SFXID | 文件名 | 触发 | 声音描述 | 音轨 |
|---|---|---|---|---|
| AI_Bullet_Poison_Fire_01 | sfx_ai_poison_fire_01.wav | 毒气弹发射 | 黏糊喷射+气泡声 | 中音 |
| AI_Bullet_Poison_Fly_01 | sfx_ai_poison_fly_loop.wav | 飞行 | 毒泡咕噜声 | 中音 |
| AI_Bullet_Poison_Hit_01 | sfx_ai_poison_hit_01.wav | 命中 | 毒雾破裂 | 中低音 |
| AI_Bullet_Poison_Tick_01 | sfx_ai_poison_tick_01.wav | 毒伤Tick | 腐蚀小气泡 | 中音 |
| AI_Bullet_Poison_Area_01 | sfx_ai_poison_area_loop.wav | 毒区持续 | 低频翻滚毒雾 | 低音 |

---

## 5.7 音波AOE音效

| SFXID | 文件名 | 触发 | 声音描述 | 音轨 |
|---|---|---|---|---|
| AI_Sonic_Charge_01 | sfx_ai_sonic_charge_01.wav | 音波蓄力 | 低音上升、音箱蓄能 | 低音 |
| AI_Sonic_Release_01 | sfx_ai_sonic_release_01.wav | 音波释放 | “咚！”低音冲击 | 低音 |
| AI_Sonic_Hit_01 | sfx_ai_sonic_hit_01.wav | 音波命中 | 低频闷响+漫画冲击 | 低音 |
| AI_Sonic_Ring_01 | sfx_ai_sonic_ring_expand_01.wav | 环形扩散 | 声波扩散“嗡” | 低音 |
| AI_Sonic_End_01 | sfx_ai_sonic_end_tail_01.wav | 音波结束 | 低音余波衰减 | 低音 |

---

## 5.8 AI节奏器音效

这些声音用于下方音效进度条和AI攻击同步。

| SFXID | 文件名 | 用途 | 声音描述 |
|---|---|---|---|
| Rhythm_High_Beat_01 | sfx_rhythm_high_tick_01.wav | 高音节拍点 | 叮、短促高音 |
| Rhythm_High_Beat_02 | sfx_rhythm_high_tick_02.wav | 高音变体 | 更尖的电子点 |
| Rhythm_Mid_Beat_01 | sfx_rhythm_mid_tap_01.wav | 中音节拍点 | 哒、木鱼/鼓棒感 |
| Rhythm_Mid_Beat_02 | sfx_rhythm_mid_tap_02.wav | 中音变体 | 电子鼓短音 |
| Rhythm_Low_Beat_01 | sfx_rhythm_low_boom_01.wav | 低音节拍点 | 咚、低音鼓 |
| Rhythm_Low_Beat_02 | sfx_rhythm_low_boom_02.wav | 低音变体 | 更重低音 |
| Rhythm_Warning_01 | sfx_rhythm_warning_rise_01.wav | 预警上升 | 升调电子噪音 |
| Rhythm_EndLag_01 | sfx_rhythm_endlag_soft_01.wav | 后摇提示 | 轻微断拍、空拍 |
| Rhythm_CritAccent_01 | sfx_rhythm_crit_accent_01.wav | 暴击重音 | 强烈重拍+闪光感 |
| Rhythm_Glitch_01 | sfx_rhythm_glitch_01.wav | 节奏错乱 | 破音、卡顿、错拍 |

---

# 6. 造物主技能音效

## 6.1 通用技能音效

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_Skill_Select_01 | sfx_creator_skill_select_01.wav | 选择技能 | 黑暗按钮+轻微魔法音 |
| Creator_Skill_Aim_01 | sfx_creator_skill_aim_01.wav | 进入瞄准 | 低频悬停声 |
| Creator_Skill_Cancel_01 | sfx_creator_skill_cancel_01.wav | 取消释放 | 反向吸回声 |
| Creator_Skill_Invalid_01 | sfx_creator_skill_invalid_01.wav | 不能释放 | 错误低音“咚” |
| Creator_Skill_Cooldown_01 | sfx_creator_skill_cooldown_01.wav | 冷却未完成 | 短促卡壳声 |
| Creator_Skill_NoEnergy_01 | sfx_creator_skill_no_energy_01.wav | 能量不足 | 干瘪低音提示 |

---

## 6.2 落雷技能

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_Thunder_Warning_01 | sfx_creator_thunder_warning_01.wav | 预警 | 电流聚集、上升音 |
| Creator_Thunder_Cast_01 | sfx_creator_thunder_cast_01.wav | 释放 | 天空劈雷 |
| Creator_Thunder_Hit_01 | sfx_creator_thunder_hit_01.wav | 命中 | 雷击爆点+短暂停顿 |
| Creator_Thunder_Miss_01 | sfx_creator_thunder_miss_01.wav | AI躲开 | 雷打空、地面电弧消散 |

---

## 6.3 毒圈技能

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_PoisonCircle_Warning_01 | sfx_creator_poisoncircle_warning_01.wav | 预警 | 毒泡从地下冒出 |
| Creator_PoisonCircle_Cast_01 | sfx_creator_poisoncircle_cast_01.wav | 释放 | 毒雾喷开 |
| Creator_PoisonCircle_Loop_01 | sfx_creator_poisoncircle_loop.ogg | 持续 | 低频毒气翻滚 |
| Creator_PoisonCircle_Tick_01 | sfx_creator_poisoncircle_tick_01.wav | 毒伤Tick | 轻腐蚀泡泡 |
| Creator_PoisonCircle_End_01 | sfx_creator_poisoncircle_end_01.wav | 结束 | 毒雾散去 |

---

## 6.4 冰圈技能

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_IceCircle_Warning_01 | sfx_creator_icecircle_warning_01.wav | 预警 | 寒气聚集 |
| Creator_IceCircle_Cast_01 | sfx_creator_icecircle_cast_01.wav | 释放 | 地面冻结 |
| Creator_IceCircle_Hit_01 | sfx_creator_icecircle_hit_01.wav | 命中 | 结冰“咔” |
| Creator_IceCircle_Loop_01 | sfx_creator_icecircle_loop.ogg | 持续 | 冰面细碎声 |
| Creator_IceCircle_End_01 | sfx_creator_icecircle_end_01.wav | 结束 | 冰裂消散 |

---

## 6.5 火焰爆裂技能

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_FireBlast_Warning_01 | sfx_creator_fireblast_warning_01.wav | 预警 | 舞台喷火器蓄压 |
| Creator_FireBlast_Cast_01 | sfx_creator_fireblast_cast_01.wav | 释放 | 爆燃喷火 |
| Creator_FireBlast_Hit_01 | sfx_creator_fireblast_hit_01.wav | 命中 | 火焰爆点 |
| Creator_FireBlast_Miss_01 | sfx_creator_fireblast_miss_01.wav | 未命中 | 火焰空爆 |

---

## 6.6 破盾诅咒

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_ShieldBreak_Warning_01 | sfx_creator_shieldbreak_warning_01.wav | 预警 | 玻璃裂纹预告音 |
| Creator_ShieldBreak_Cast_01 | sfx_creator_shieldbreak_cast_01.wav | 释放 | 青蓝裂响 |
| Creator_ShieldBreak_Hit_01 | sfx_creator_shieldbreak_hit_01.wav | 命中 | 护盾碎裂 |
| Creator_ShieldBreak_Apply_01 | sfx_creator_shieldbreak_apply_01.wav | Debuff挂上 | 细碎裂纹持续音 |

---

## 6.7 禁疗诅咒

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_HealBlock_Warning_01 | sfx_creator_healblock_warning_01.wav | 预警 | 低沉封印音 |
| Creator_HealBlock_Cast_01 | sfx_creator_healblock_cast_01.wav | 释放 | 封口、锁链感 |
| Creator_HealBlock_Hit_01 | sfx_creator_healblock_hit_01.wav | 命中 | 治疗被截断的“咔” |
| Creator_HealBlock_End_01 | sfx_creator_healblock_end_01.wav | 结束 | 封印松开 |

---

## 6.8 地刺陷阱

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_SpikeTrap_Warning_01 | sfx_creator_spiketrap_warning_01.wav | 预警 | 地面裂开细响 |
| Creator_SpikeTrap_Cast_01 | sfx_creator_spiketrap_cast_01.wav | 释放 | 尖刺破土 |
| Creator_SpikeTrap_Hit_01 | sfx_creator_spiketrap_hit_01.wav | 命中 | 尖锐穿刺+漫画打击 |
| Creator_SpikeTrap_Retract_01 | sfx_creator_spiketrap_retract_01.wav | 收回 | 尖刺缩回 |

---

## 6.9 噪音封锁

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_NoiseLock_Warning_01 | sfx_creator_noiselock_warning_01.wav | 预警 | 失真音墙逐渐升起 |
| Creator_NoiseLock_Cast_01 | sfx_creator_noiselock_cast_01.wav | 释放 | 紫色音波墙形成 |
| Creator_NoiseLock_Loop_01 | sfx_creator_noiselock_loop.ogg | 持续 | 扭曲噪音低频 |
| Creator_NoiseLock_End_01 | sfx_creator_noiselock_end_01.wav | 结束 | 失真关闭 |

---

## 6.10 怪物强化

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Creator_BuffMonster_Cast_01 | sfx_creator_buffmonster_cast_01.wav | 释放 | 造物主能量注入 |
| Creator_BuffMonster_Apply_01 | sfx_creator_buffmonster_apply_01.wav | 强化生效 | 红黑能量爆点 |
| Creator_BuffMonster_Loop_01 | sfx_creator_buffmonster_loop.ogg | Buff持续 | 轻微狂暴心跳 |
| Creator_BuffMonster_End_01 | sfx_creator_buffmonster_end_01.wav | 结束 | 能量熄灭 |

---

# 7. 怪物音效

## 7.1 通用怪物音效规则

每个怪物至少需要：

```text
出生音效
待机叫声
移动音效
攻击音效
受击音效
死亡音效
特殊技能音效
```

每类音效建议至少做2~4个变体。

---

## 7.2 杂音团子

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_NoiseBlob_Spawn | sfx_mon_noiseblob_spawn_01.wav | 出生 | 噪音泡从地面弹出 | 3 |
| Monster_NoiseBlob_Idle | sfx_mon_noiseblob_idle_01.wav | 待机 | 小声哼哼、电子杂音 | 5 |
| Monster_NoiseBlob_Move | sfx_mon_noiseblob_move_01.wav | 移动 | 软弹跳+噪点 | 4 |
| Monster_NoiseBlob_Attack | sfx_mon_noiseblob_attack_01.wav | 攻击 | 咬一口+短促噪声 | 3 |
| Monster_NoiseBlob_Hit | sfx_mon_noiseblob_hit_01.wav | 受击 | 软泥被拍扁 | 4 |
| Monster_NoiseBlob_Death | sfx_mon_noiseblob_death_01.wav | 死亡 | 噪声破泡 | 4 |

---

## 7.3 鼓棒疯仔

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_DrumImp_Spawn | sfx_mon_drumimp_spawn_01.wav | 出生 | 小鼓“咚”一下跳出 | 3 |
| Monster_DrumImp_Move | sfx_mon_drumimp_move_01.wav | 移动 | 快速小鼓步点 | 4 |
| Monster_DrumImp_Attack | sfx_mon_drumimp_attack_01.wav | 攻击 | 鼓棒敲击+怪叫 | 4 |
| Monster_DrumImp_Hit | sfx_mon_drumimp_hit_01.wav | 受击 | 鼓皮被打瘪 | 3 |
| Monster_DrumImp_Death | sfx_mon_drumimp_death_01.wav | 死亡 | 小鼓破裂+哀叫 | 3 |

---

## 7.4 卡带投手

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_CassetteThrower_Spawn | sfx_mon_cassette_spawn_01.wav | 出生 | 卡带插入“咔哒” | 3 |
| Monster_CassetteThrower_Move | sfx_mon_cassette_move_01.wav | 移动 | 塑料盒摇晃声 | 4 |
| Monster_CassetteThrower_Attack | sfx_mon_cassette_attack_01.wav | 攻击 | 磁带飞盘甩出 | 4 |
| Monster_CassetteThrower_ProjectHit | sfx_mon_cassette_projectile_hit_01.wav | 弹体命中 | 塑料片撞击 | 3 |
| Monster_CassetteThrower_Hit | sfx_mon_cassette_hit_01.wav | 受击 | 磁带卡壳 | 3 |
| Monster_CassetteThrower_Death | sfx_mon_cassette_death_01.wav | 死亡 | 磁带散架、倒带声 | 3 |

---

## 7.5 音箱壮汉

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_SpeakerBrute_Spawn | sfx_mon_speakerbrute_spawn_01.wav | 出生 | 重低音落地 | 3 |
| Monster_SpeakerBrute_Move | sfx_mon_speakerbrute_step_01.wav | 移动 | 沉重脚步+低音震动 | 4 |
| Monster_SpeakerBrute_Attack | sfx_mon_speakerbrute_attack_01.wav | 攻击 | 音箱重锤 | 4 |
| Monster_SpeakerBrute_Slam | sfx_mon_speakerbrute_slam_01.wav | 技能 | 低频砸地波 | 3 |
| Monster_SpeakerBrute_Hit | sfx_mon_speakerbrute_hit_01.wav | 受击 | 木箱/音箱被打 | 3 |
| Monster_SpeakerBrute_Death | sfx_mon_speakerbrute_death_01.wav | 死亡 | 音箱断电爆裂 | 3 |

---

## 7.6 耳机线潜袭者

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_CableStalker_Spawn | sfx_mon_cablestalker_spawn_01.wav | 出生 | 电缆从阴影滑出 | 3 |
| Monster_CableStalker_Move | sfx_mon_cablestalker_move_01.wav | 移动 | 细线滑动声 | 4 |
| Monster_CableStalker_Dash | sfx_mon_cablestalker_dash_01.wav | 突进 | 快速线缆抽动 | 3 |
| Monster_CableStalker_Attack | sfx_mon_cablestalker_attack_01.wav | 攻击 | 线缆抽击 | 4 |
| Monster_CableStalker_Hit | sfx_mon_cablestalker_hit_01.wav | 受击 | 线缆弹断小声 | 3 |
| Monster_CableStalker_Death | sfx_mon_cablestalker_death_01.wav | 死亡 | 线缆纠缠断裂 | 3 |

---

## 7.7 节拍器巫师

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_MetronomeMage_Spawn | sfx_mon_metromage_spawn_01.wav | 出生 | 节拍器启动 | 3 |
| Monster_MetronomeMage_Idle | sfx_mon_metromage_idle_tick_01.wav | 待机 | 嘀嗒摆动 | 4 |
| Monster_MetronomeMage_Cast | sfx_mon_metromage_cast_01.wav | 施法 | 指挥棒划过+节拍加重 | 4 |
| Monster_MetronomeMage_Control | sfx_mon_metromage_control_01.wav | 控制技能 | 锁拍、断拍 | 3 |
| Monster_MetronomeMage_Hit | sfx_mon_metromage_hit_01.wav | 受击 | 节拍器歪掉 | 3 |
| Monster_MetronomeMage_Death | sfx_mon_metromage_death_01.wav | 死亡 | 节拍器停摆 | 3 |

---

## 7.8 反馈泡泡妖

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_FeedbackBubble_Spawn | sfx_mon_feedbackbubble_spawn_01.wav | 出生 | 泡泡膨胀 | 3 |
| Monster_FeedbackBubble_Move | sfx_mon_feedbackbubble_float_01.wav | 移动 | 轻漂浮泡泡 | 4 |
| Monster_FeedbackBubble_Attack | sfx_mon_feedbackbubble_attack_01.wav | 攻击 | 泡泡喷射 | 4 |
| Monster_FeedbackBubble_Pop | sfx_mon_feedbackbubble_pop_01.wav | 泡泡破裂 | 啵+噪音反馈 | 4 |
| Monster_FeedbackBubble_Hit | sfx_mon_feedbackbubble_hit_01.wav | 受击 | 泡泡被压扁 | 3 |
| Monster_FeedbackBubble_Death | sfx_mon_feedbackbubble_death_01.wav | 死亡 | 大泡泡爆开 | 3 |

---

## 7.9 爆音炸弹仔

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_BoomBomb_Spawn | sfx_mon_boombomb_spawn_01.wav | 出生 | 炸弹音箱启动 | 3 |
| Monster_BoomBomb_Move | sfx_mon_boombomb_move_01.wav | 移动 | 紧张小鼓点 | 4 |
| Monster_BoomBomb_Warning | sfx_mon_boombomb_warning_01.wav | 自爆前 | 倒计时哔哔 | 3 |
| Monster_BoomBomb_Explode | sfx_mon_boombomb_explode_01.wav | 自爆 | 爆音轰鸣 | 4 |
| Monster_BoomBomb_Hit | sfx_mon_boombomb_hit_01.wav | 受击 | 空心金属被打 | 3 |
| Monster_BoomBomb_Death | sfx_mon_boombomb_death_01.wav | 被击杀 | 小爆破或泄气 | 3 |

---

## 7.10 毒雾歌者

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_PoisonSinger_Spawn | sfx_mon_poisonsinger_spawn_01.wav | 出生 | 诡异滑音+毒雾 | 3 |
| Monster_PoisonSinger_Idle | sfx_mon_poisonsinger_idle_01.wav | 待机 | 小声哼唱 | 5 |
| Monster_PoisonSinger_Attack | sfx_mon_poisonsinger_attack_01.wav | 攻击 | 唱出毒雾 | 4 |
| Monster_PoisonSinger_Debuff | sfx_mon_poisonsinger_debuff_01.wav | 禁疗/中毒 | 腐蚀和低语 | 3 |
| Monster_PoisonSinger_Hit | sfx_mon_poisonsinger_hit_01.wav | 受击 | 尖叫短音 | 3 |
| Monster_PoisonSinger_Death | sfx_mon_poisonsinger_death_01.wav | 死亡 | 破音高歌后散去 | 3 |

---

## 7.11 音叉重锤手

| SFXID | 文件名 | 触发 | 描述 | 变体 |
|---|---|---|---|---:|
| Monster_TuningHammer_Spawn | sfx_mon_tuninghammer_spawn_01.wav | 出生 | 金属音叉共鸣 | 3 |
| Monster_TuningHammer_Move | sfx_mon_tuninghammer_move_01.wav | 移动 | 重金属步伐 | 4 |
| Monster_TuningHammer_Charge | sfx_mon_tuninghammer_charge_01.wav | 蓄力 | 音叉震动增强 | 3 |
| Monster_TuningHammer_Attack | sfx_mon_tuninghammer_attack_01.wav | 攻击 | 金属重击 | 4 |
| Monster_TuningHammer_ShieldBreak | sfx_mon_tuninghammer_shieldbreak_01.wav | 破盾 | 玻璃/护盾碎裂 | 3 |
| Monster_TuningHammer_Hit | sfx_mon_tuninghammer_hit_01.wav | 受击 | 金属闷响 | 3 |
| Monster_TuningHammer_Death | sfx_mon_tuninghammer_death_01.wav | 死亡 | 音叉失谐下落 | 3 |

---

# 8. 精英怪与Boss音效

## 8.1 副炮低音巨怪

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Elite_SubwooferGiant_Spawn | sfx_elite_subwoofer_spawn_01.wav | 出生 | 巨大低音炮落地 |
| Elite_SubwooferGiant_Step | sfx_elite_subwoofer_step_01.wav | 脚步 | 地面低频震动 |
| Elite_SubwooferGiant_Slam | sfx_elite_subwoofer_slam_01.wav | 重击 | 低频冲击波 |
| Elite_SubwooferGiant_Roar | sfx_elite_subwoofer_roar_01.wav | 咆哮 | 低音喇叭吼 |
| Elite_SubwooferGiant_Death | sfx_elite_subwoofer_death_01.wav | 死亡 | 音箱爆裂断电 |

---

## 8.2 霓虹唱片女巫

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Elite_RecordWitch_Spawn | sfx_elite_recordwitch_spawn_01.wav | 出生 | 唱片反转+诡异笑声 |
| Elite_RecordWitch_Attack | sfx_elite_recordwitch_attack_01.wav | 攻击 | 唱片飞盘甩出 |
| Elite_RecordWitch_Spin | sfx_elite_recordwitch_spin_01.wav | 旋转技能 | 唱片高速旋转 |
| Elite_RecordWitch_Curse | sfx_elite_recordwitch_curse_01.wav | 诅咒 | 倒放人声+低语 |
| Elite_RecordWitch_Death | sfx_elite_recordwitch_death_01.wav | 死亡 | 唱片碎裂 |

---

## 8.3 灯架执行者

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Elite_LightRig_Spawn | sfx_elite_lightrig_spawn_01.wav | 出生 | 舞台灯架展开 |
| Elite_LightRig_LockOn | sfx_elite_lightrig_lockon_01.wav | 锁定 | 聚光灯锁定哔声 |
| Elite_LightRig_Beam | sfx_elite_lightrig_beam_01.wav | 光束攻击 | 舞台灯高能扫射 |
| Elite_LightRig_Hit | sfx_elite_lightrig_hit_01.wav | 受击 | 金属架被打 |
| Elite_LightRig_Death | sfx_elite_lightrig_death_01.wav | 死亡 | 灯泡爆裂+金属坍塌 |

---

## 8.4 Boss：失真之王

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Boss_DistortionKing_Intro | sfx_boss_distortionking_intro_01.wav | 出场 | 巨型音箱启动+失真吼叫 |
| Boss_DistortionKing_Idle | sfx_boss_distortionking_idle_loop.ogg | 待机 | 低频嗡鸣 |
| Boss_DistortionKing_Slam | sfx_boss_distortionking_slam_01.wav | 重击 | 超低音砸地 |
| Boss_DistortionKing_Scream | sfx_boss_distortionking_scream_01.wav | 音波吼 | 失真喇叭尖啸 |
| Boss_DistortionKing_Phase2 | sfx_boss_distortionking_phase2_01.wav | 二阶段 | 爆音加强 |
| Boss_DistortionKing_Death | sfx_boss_distortionking_death_01.wav | 死亡 | 设备过载爆炸 |

---

## 8.5 Boss：终极DJ暴君

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Boss_DJTyrant_Intro | sfx_boss_djtyrant_intro_01.wav | 出场 | DJ搓盘+观众假欢呼 |
| Boss_DJTyrant_Scratch | sfx_boss_djtyrant_scratch_01.wav | 技能 | 唱盘刮碟攻击 |
| Boss_DJTyrant_Drop | sfx_boss_djtyrant_drop_01.wav | 大招 | Drop前静音再爆低音 |
| Boss_DJTyrant_Summon | sfx_boss_djtyrant_summon_01.wav | 召唤 | 舞台灯爆闪+召唤音 |
| Boss_DJTyrant_Death | sfx_boss_djtyrant_death_01.wav | 死亡 | 唱盘炸裂、音乐断掉 |

---

## 8.6 Boss：坏拍子巨偶

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Boss_BadMetronome_Intro | sfx_boss_badmetro_intro_01.wav | 出场 | 巨大节拍器错拍启动 |
| Boss_BadMetronome_Tick | sfx_boss_badmetro_tick_01.wav | 普通节拍 | 巨大嘀嗒声 |
| Boss_BadMetronome_Offbeat | sfx_boss_badmetro_offbeat_01.wav | 错拍技能 | 断拍、卡顿、错位音 |
| Boss_BadMetronome_Stop | sfx_boss_badmetro_stop_01.wav | 停顿技能 | 突然静音+重拍 |
| Boss_BadMetronome_Death | sfx_boss_badmetro_death_01.wav | 死亡 | 节拍器崩坏停摆 |

---

# 9. 受击与打击音效

## 9.1 通用受击音效

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Hit_Physical_Light_01 | sfx_hit_physical_light_01.wav | 轻物理命中 | 小冲击 |
| Hit_Physical_Mid_01 | sfx_hit_physical_mid_01.wav | 中物理命中 | 肉感+漫画打击 |
| Hit_Physical_Heavy_01 | sfx_hit_physical_heavy_01.wav | 重击 | 低频冲击 |
| Hit_Crit_01 | sfx_hit_crit_01.wav | 暴击 | 加厚爆点 |
| Hit_Kill_01 | sfx_hit_kill_01.wav | 击杀 | 碎裂+消散 |
| Hit_Block_01 | sfx_hit_block_01.wav | 格挡 | 盾牌闷响 |
| Hit_Dodge_01 | sfx_hit_dodge_01.wav | 闪避 | 风声滑过 |
| Hit_Immune_01 | sfx_hit_immune_01.wav | 免疫 | 空打无效声 |
| Hit_ShieldBreak_01 | sfx_hit_shieldbreak_01.wav | 破盾 | 玻璃盾碎裂 |

---

## 9.2 元素受击音效

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Hit_Fire_Light_01 | sfx_hit_fire_light_01.wav | 火焰轻命中 | 小火花 |
| Hit_Fire_Heavy_01 | sfx_hit_fire_heavy_01.wav | 火焰重命中 | 爆燃 |
| Hit_Ice_Light_01 | sfx_hit_ice_light_01.wav | 冰霜轻命中 | 结霜 |
| Hit_Ice_Heavy_01 | sfx_hit_ice_heavy_01.wav | 冰霜重命中 | 冰裂 |
| Hit_Lightning_Light_01 | sfx_hit_lightning_light_01.wav | 雷电轻命中 | 滋啦 |
| Hit_Lightning_Heavy_01 | sfx_hit_lightning_heavy_01.wav | 雷电重命中 | 电爆 |
| Hit_Poison_Light_01 | sfx_hit_poison_light_01.wav | 毒轻命中 | 毒泡 |
| Hit_Poison_Heavy_01 | sfx_hit_poison_heavy_01.wav | 毒重命中 | 毒雾喷开 |
| Hit_Sonic_Light_01 | sfx_hit_sonic_light_01.wav | 音波轻命中 | 小波纹 |
| Hit_Sonic_Heavy_01 | sfx_hit_sonic_heavy_01.wav | 音波重命中 | 低音冲击 |

---

## 9.3 跳字音效

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| DamageNumber_Normal_01 | sfx_dmgnum_normal_01.wav | 普通跳字出现 | 极轻小弹音 |
| DamageNumber_Crit_01 | sfx_dmgnum_crit_01.wav | 暴击跳字 | 强调爆点 |
| DamageNumber_DOT_01 | sfx_dmgnum_dot_01.wav | DOT跳字 | 很轻Tick |
| DamageNumber_Weakness_01 | sfx_dmgnum_weakness_01.wav | 弱点 | 尖锐提示 |
| DamageNumber_Immune_01 | sfx_dmgnum_immune_01.wav | 免疫 | 灰色无效音 |
| DamageNumber_Dodge_01 | sfx_dmgnum_dodge_01.wav | 闪避 | 轻滑音 |
| DamageNumber_ShieldBreak_01 | sfx_dmgnum_shieldbreak_01.wav | 破盾 | 碎裂强调 |

---

# 10. Buff / Debuff音效

## 10.1 通用Buff音效

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Buff_Apply_Positive_01 | sfx_buff_apply_positive_01.wav | 正面Buff获得 | 上升小音 |
| Buff_Apply_Negative_01 | sfx_buff_apply_negative_01.wav | 负面Debuff获得 | 低沉贴附 |
| Buff_End_01 | sfx_buff_end_01.wav | Buff结束 | 轻微消散 |
| Buff_Stack_01 | sfx_buff_stack_01.wav | Buff叠层 | 连续叠加音 |
| Buff_Dispel_01 | sfx_buff_dispel_01.wav | 驱散 | 反向抽离 |

---

## 10.2 具体状态音效

| SFXID | 文件名 | 状态 | 触发 | 描述 |
|---|---|---|---|---|
| Buff_Frozen_Apply | sfx_buff_frozen_apply_01.wav | 冰冻 | 生效 | 结冰 |
| Buff_Frozen_Loop | sfx_buff_frozen_loop.ogg | 冰冻 | 持续 | 冰层细裂 |
| Buff_Frozen_End | sfx_buff_frozen_end_01.wav | 冰冻 | 结束 | 冰破 |
| Buff_Poison_Apply | sfx_buff_poison_apply_01.wav | 中毒 | 生效 | 毒雾附着 |
| Buff_Poison_Tick | sfx_buff_poison_tick_01.wav | 中毒 | Tick | 腐蚀泡 |
| Buff_Burn_Apply | sfx_buff_burn_apply_01.wav | 燃烧 | 生效 | 点燃 |
| Buff_Burn_Tick | sfx_buff_burn_tick_01.wav | 燃烧 | Tick | 火苗噼啪 |
| Buff_Slow_Apply | sfx_buff_slow_apply_01.wav | 减速 | 生效 | 黏住脚步 |
| Buff_Stun_Apply | sfx_buff_stun_apply_01.wav | 眩晕 | 生效 | 星星转圈 |
| Buff_HealBlock_Apply | sfx_buff_healblock_apply_01.wav | 禁疗 | 生效 | 治疗锁住 |
| Buff_ShieldBreak_Apply | sfx_buff_shieldbreak_apply_01.wav | 破盾 | 生效 | 裂纹扩散 |
| Buff_Silence_Apply | sfx_buff_silence_apply_01.wav | 沉默 | 生效 | 音量被掐断 |
| Buff_Rage_Apply | sfx_buff_rage_apply_01.wav | 狂暴 | 生效 | 心跳+低吼 |

---

# 11. UI音效

## 11.1 通用UI音效

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| UI_Click_01 | sfx_ui_click_01.wav | 普通点击 | 像素按钮“哒” |
| UI_Click_02 | sfx_ui_click_02.wav | 点击变体 | 更轻的按钮音 |
| UI_Hover_01 | sfx_ui_hover_01.wav | 鼠标悬停 | 轻微电子点 |
| UI_Confirm_01 | sfx_ui_confirm_01.wav | 确认 | 小上升音 |
| UI_Cancel_01 | sfx_ui_cancel_01.wav | 取消 | 小下降音 |
| UI_Back_01 | sfx_ui_back_01.wav | 返回 | 唱片倒转短音 |
| UI_Error_01 | sfx_ui_error_01.wav | 错误 | 错拍低音 |
| UI_OpenPanel_01 | sfx_ui_open_panel_01.wav | 打开面板 | 音响开机 |
| UI_ClosePanel_01 | sfx_ui_close_panel_01.wav | 关闭面板 | 音响关闭 |
| UI_TabSwitch_01 | sfx_ui_tab_switch_01.wav | 页签切换 | 切换调音台按钮 |
| UI_Scroll_01 | sfx_ui_scroll_01.wav | 滚动 | 滑杆轻响 |
| UI_Drag_01 | sfx_ui_drag_01.wav | 拖动 | 电缆拖动轻音 |
| UI_Drop_01 | sfx_ui_drop_01.wav | 放下 | 设备放下 |
| UI_Tooltip_01 | sfx_ui_tooltip_01.wav | Tips弹出 | 贴纸弹出声 |

---

## 11.2 战斗UI音效

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| UI_SummonDrawer_Open | sfx_ui_summondrawer_open_01.wav | 打开怪物抽屉 | 抽屉滑出+设备音 |
| UI_SummonDrawer_Close | sfx_ui_summondrawer_close_01.wav | 关闭怪物抽屉 | 抽屉收回 |
| UI_SelectMonster | sfx_ui_select_monster_01.wav | 选择怪物 | 小怪叫+按钮音 |
| UI_SummonConfirm | sfx_ui_summon_confirm_01.wav | 召唤确认 | 召唤印记亮起 |
| UI_SummonInvalid | sfx_ui_summon_invalid_01.wav | 不能召唤 | 低音错误 |
| UI_EnergyGain | sfx_ui_energy_gain_01.wav | 能量增加 | 小节拍上升 |
| UI_EnergyFull | sfx_ui_energy_full_01.wav | 能量满 | 合成器亮音 |
| UI_CooldownReady | sfx_ui_cooldown_ready_01.wav | 技能冷却完成 | 清脆完成音 |
| UI_BD_New | sfx_ui_bd_new_01.wav | AI获得新BD | 效果器开启 |
| UI_BD_Upgrade | sfx_ui_bd_upgrade_01.wav | BD升级 | 上升电音 |
| UI_BD_Hover | sfx_ui_bd_hover_01.wav | 悬停BD | 小音符 |
| UI_RhythmBeatFlash | sfx_ui_rhythm_flash_01.wav | 节拍点UI闪 | 很轻的点音 |

---

## 11.3 设置界面UI音效

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| UI_Settings_Open | sfx_ui_settings_open_01.wav | 打开设置 | 设备面板打开 |
| UI_Settings_Close | sfx_ui_settings_close_01.wav | 关闭设置 | 设备面板合上 |
| UI_Slider_Move | sfx_ui_slider_move_01.wav | 滑条拖动 | 调音台滑杆 |
| UI_Toggle_On | sfx_ui_toggle_on_01.wav | 开关打开 | 开关“咔” |
| UI_Toggle_Off | sfx_ui_toggle_off_01.wav | 开关关闭 | 低一点的咔 |
| UI_Save | sfx_ui_save_01.wav | 保存 | 磁带存档 |
| UI_Load | sfx_ui_load_01.wav | 读取 | 磁带读取 |
| UI_Reset | sfx_ui_reset_01.wav | 重置 | 唱片回拨 |

---

# 12. 冒泡吐槽拟声音效

## 12.1 设计规则

AI冒泡不一定需要真人语音，可以用“拟声碎碎念”表达情绪。

风格：

- 暴漫感
- 快速咕哝
- 夸张哼声
- 像素变调人声
- 不需要完整语言，避免重复疲劳

---

## 12.2 AI冒泡拟声

| SFXID | 文件名 | 情绪 | 描述 | 变体 |
|---|---|---|---|---:|
| Voice_AI_Complain_01 | sfx_voice_ai_complain_01.wav | 抱怨 | 嘟囔、烦躁 | 6 |
| Voice_AI_Angry_01 | sfx_voice_ai_angry_01.wav | 愤怒 | 短促吼叫 | 6 |
| Voice_AI_Taunt_01 | sfx_voice_ai_taunt_01.wav | 嘲讽 | 贱笑、挑衅 | 6 |
| Voice_AI_Panic_01 | sfx_voice_ai_panic_01.wav | 紧张 | 慌张怪叫 | 6 |
| Voice_AI_Hurt_01 | sfx_voice_ai_hurt_01.wav | 受伤 | 被打痛叫 | 6 |
| Voice_AI_LowHP_01 | sfx_voice_ai_lowhp_01.wav | 低血 | 喘气、嘴硬 | 4 |
| Voice_AI_Death_01 | sfx_voice_ai_deathvoice_01.wav | 死亡 | 滑稽惨叫 | 4 |
| Voice_AI_LevelUp_01 | sfx_voice_ai_levelup_01.wav | 得意 | 兴奋喊叫 | 4 |

---

# 13. 环境音效

## 13.1 地图环境音

| SFXID | 文件名 | 场景 | 描述 |
|---|---|---|---|
| Amb_RehearsalRoom_01 | amb_rehearsal_room_loop.ogg | 破旧排练室 | 远处调音、电流、木地板轻响 |
| Amb_Livehouse_01 | amb_livehouse_loop.ogg | 地下Livehouse | 低频嗡鸣、人群远声、灯光电流 |
| Amb_Rooftop_01 | amb_rooftop_neon_loop.ogg | 霓虹天台 | 夜风、城市远声、霓虹电流 |
| Amb_SonicStreet_01 | amb_sonic_street_loop.ogg | 声波污染街区 | 电磁噪音、远处警报 |
| Amb_MainStage_01 | amb_mainstage_loop.ogg | 音乐狂主舞台 | 巨型音箱低频、灯架机械声 |

---

## 13.2 环境交互音

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Env_SpeakerHum_01 | sfx_env_speaker_hum_loop.ogg | 音箱附近 | 低频嗡鸣 |
| Env_NeonBuzz_01 | sfx_env_neon_buzz_loop.ogg | 霓虹灯附近 | 霓虹电流 |
| Env_CableSpark_01 | sfx_env_cable_spark_01.wav | 电缆火花 | 小电火花 |
| Env_PosterRip_01 | sfx_env_poster_rip_01.wav | 海报破裂 | 纸张撕裂 |
| Env_LightFlicker_01 | sfx_env_light_flicker_01.wav | 灯光闪烁 | 频闪电音 |
| Env_GlassBreak_01 | sfx_env_glass_break_01.wav | 玻璃破碎 | 碎玻璃 |

---

# 14. 结算与提示音效

| SFXID | 文件名 | 触发 | 描述 |
|---|---|---|---|
| Result_Victory_01 | sfx_result_victory_01.wav | 玩家胜利 | 搞怪胜利号角+观众欢呼 |
| Result_Defeat_01 | sfx_result_defeat_01.wav | 玩家失败 | AI狂笑+失败下滑音 |
| Result_Star_01 | sfx_result_star_01.wav | 星级结算 | 亮晶晶像素音 |
| Result_Reward_01 | sfx_result_reward_01.wav | 获得奖励 | 掉落金币+音符 |
| Result_Unlock_01 | sfx_result_unlock_01.wav | 解锁内容 | 效果器开机 |
| Result_ClickContinue_01 | sfx_result_continue_01.wav | 继续 | 轻快确认音 |

---

# 15. 文件命名规范

## 15.1 命名结构

```text
sfx_模块_对象_行为_编号.wav
bgm_场景_风格_编号.ogg
amb_场景_循环_编号.ogg
```

示例：

```text
sfx_ai_fireball_hit_01.wav
sfx_mon_drumimp_attack_02.wav
sfx_creator_thunder_warning_01.wav
bgm_battle_mid_manic_beat.ogg
amb_livehouse_loop.ogg
```

---

## 15.2 编号规则

| 编号 | 用途 |
|---|---|
| 01 | 默认版本 |
| 02~05 | 同类变体 |
| loop | 循环音 |
| intro | 引入段 |
| end | 结束段 |

---

# 16. 音效配置字段

## 16.1 SFX配置结构

```json
{
  "SFXID": "AI_Bullet_Fire_Hit_01",
  "DisplayName": "火焰弹命中",
  "FilePath": "Audio/SFX/AI/sfx_ai_fireball_hit_01.wav",
  "Category": "AISFX",
  "Bus": "SFX/AISFX",
  "Volume": 0.85,
  "Pitch": 1.0,
  "RandomPitchMin": 0.95,
  "RandomPitchMax": 1.05,
  "Cooldown": 0.08,
  "CanOverlap": true,
  "Priority": 70,
  "Loop": false,
  "SpatialBlend": 0.5,
  "MaxDistance": 20,
  "Ducking": false,
  "TrackType": "Low",
  "Tags": ["AI", "Fire", "Hit", "Projectile"]
}
```

---

## 16.2 BGM配置结构

```json
{
  "BGMID": "BGM_Battle_Mid_01",
  "DisplayName": "中强度战斗：疯狂节拍",
  "FilePath": "Audio/BGM/bgm_battle_mid_manic_beat.ogg",
  "SceneType": "Battle",
  "Intensity": "Mid",
  "BPM": 128,
  "Loop": true,
  "LoopStart": 0.0,
  "LoopEnd": 120.0,
  "RandomWeight": 100,
  "AllowRepeat": false,
  "FadeIn": 2.0,
  "FadeOut": 2.0,
  "Bus": "BGM/BattleBGM"
}
```

---

# 17. 音效触发逻辑

## 17.1 AI弹幕音效触发

```text
AI节奏器触发BeatEvent
↓
播放Rhythm节拍音
↓
根据BD生成弹体
↓
播放弹体发射音
↓
弹体飞行期间播放轻量Loop或无Loop
↓
命中目标
↓
播放元素命中音
↓
如果暴击，额外播放暴击音
↓
如果弹射，播放弹射音
```

---

## 17.2 怪物音效触发

```text
怪物被召唤
↓
播放出生音效
↓
进入移动状态，按步频播放移动音
↓
进入攻击状态
↓
攻击判定帧播放攻击音
↓
受到伤害播放受击音
↓
死亡播放死亡音
```

---

## 17.3 造物主技能音效触发

```text
玩家选择技能
↓
播放选择音
↓
进入瞄准状态播放瞄准音
↓
放置预警区域播放预警音
↓
预警结束播放释放音
↓
命中AI播放命中音
↓
AI躲开则播放Miss音
```

---

# 18. 随机播放与变体规则

## 18.1 同类音效随机

每次播放同类音效时，从变体池中随机选择。

```text
sfx_mon_drumimp_attack_01
sfx_mon_drumimp_attack_02
sfx_mon_drumimp_attack_03
```

规则：

```text
同一个变体不要连续播放超过2次
如果变体数 >= 3，最近一次播放的变体权重降低80%
```

---

## 18.2 音高随机

普通SFX建议带轻微Pitch随机。

| 类型 | Pitch范围 |
|---|---|
| UI点击 | 0.98 ~ 1.02 |
| 怪物叫声 | 0.9 ~ 1.1 |
| 普通攻击 | 0.95 ~ 1.05 |
| 元素命中 | 0.96 ~ 1.04 |
| Boss音效 | 0.98 ~ 1.02 |
| 暴击 | 固定或轻微变化 |

---

## 18.3 高频限频

| 音效类型 | 最小间隔 |
|---|---:|
| 普通命中 | 0.08秒 |
| 同目标命中 | 0.12秒 |
| DOT Tick | 0.3秒 |
| UI Hover | 0.05秒 |
| 小怪受击 | 0.08秒 |
| AI冒泡拟声 | 3秒 |
| 低音重击 | 0.4秒 |

---

# 19. 必须制作的MVP音频清单

如果第一版资源量太大，至少要做以下内容。

## 19.1 MVP BGM

| 类型 | 数量 |
|---|---:|
| 主菜单BGM | 1 |
| 赛前布置BGM | 1 |
| 战斗低强度BGM | 1 |
| 战斗中强度BGM | 1 |
| 战斗高强度BGM | 1 |
| Boss BGM | 1 |
| 胜利BGM | 1 |
| 失败BGM | 1 |

---

## 19.2 MVP SFX

| 类型 | 数量 |
|---|---:|
| AI基础弹发射/命中 | 2 |
| 四元素弹发射/命中 | 8 |
| 音波AOE蓄力/释放/命中 | 3 |
| 造物主技能每个至少预警/释放/命中 | 每技能3个 |
| 每个怪物出生/攻击/受击/死亡 | 每怪4个 |
| 通用元素受击 | 10 |
| UI基础点击/确认/取消/错误 | 4 |
| BGM切换/结算音 | 4 |
| 冒泡拟声 | 8 |

---

# 20. 最终音频体验目标

游戏的声音最终应该达到：

```text
听到高音叮叮叮 → 知道AI在快速弹幕
听到低音咚 → 知道危险大招或AOE来了
听到冰裂 → 知道怪物被冻结
听到毒泡 → 知道持续毒伤
听到爆燃 → 知道火焰范围伤害
听到唱片刮停 → 知道AI死亡或重大中断
听到UI像调音台 → 知道自己在操控造物主后台
```

最终感觉应该是：

> 玩家不是在普通战场打怪，而是在一场失控、搞怪、节奏爆炸的像素演唱会里，操控怪物和技能去击败一个疯癫音乐狂人。
