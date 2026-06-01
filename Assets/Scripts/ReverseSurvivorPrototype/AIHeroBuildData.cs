using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public enum ElementModule
    {
        None,
        Lightning,
        Ice,
        Fire,
        Poison
    }

    public enum TrailModule
    {
        None,
        Poison,
        Ice,
        Fire
    }

    public sealed class AIHeroBuildData
    {
        private readonly List<string> cards = new List<string>();
        private readonly Dictionary<AIHeroBDCard, int> cardLevels = new Dictionary<AIHeroBDCard, int>();
        private AIHeroBDCard latestCard;

        public float ProjectileSpeedMultiplier { get; private set; } = 1f;
        public float RhythmSpeedMultiplier { get; private set; } = 1f;
        public float DamageMultiplier { get; private set; } = 1f;
        public int ProjectileCountBonus { get; private set; }
        public float ProjectileSizeMultiplier { get; private set; } = 1f;
        public int PierceCount { get; private set; }
        public bool LightningUnlocked { get; private set; }
        public bool IceUnlocked { get; private set; }
        public bool FireUnlocked { get; private set; }
        public bool PoisonUnlocked { get; private set; }
        public bool PoisonTrailUnlocked { get; private set; }
        public bool IceTrailUnlocked { get; private set; }
        public bool FireTrailUnlocked { get; private set; }
        public bool SonicBurstUnlocked { get; private set; }
        public bool SyncopationUnlocked { get; private set; }
        public bool HeavyBeatUnlocked { get; private set; }
        public bool EchoBeatUnlocked { get; private set; }
        public float AreaDurationBonus { get; private set; }
        public float AoeRadiusMultiplier { get; private set; } = 1f;
        public IReadOnlyList<string> Cards => cards;
        public AIHeroBDCard LatestCard => latestCard;

        public string BuildName
        {
            get
            {
                var elementCount = (LightningUnlocked ? 1 : 0) + (IceUnlocked ? 1 : 0) + (FireUnlocked ? 1 : 0) + (PoisonUnlocked ? 1 : 0);
                if (elementCount >= 3)
                {
                    return "多元素和弦";
                }

                if (SonicBurstUnlocked && HeavyBeatUnlocked)
                {
                    return "音波震击";
                }

                if (PoisonUnlocked && PoisonTrailUnlocked)
                {
                    return "毒性安魂曲";
                }

                if (FireUnlocked)
                {
                    return "火焰鼓点";
                }

                if (LightningUnlocked)
                {
                    return "闪电断奏";
                }

                if (IceUnlocked)
                {
                    return "冰霜旋律";
                }

                return "单音弹幕";
            }
        }

        public string ElementTags
        {
            get
            {
                var builder = new StringBuilder();
                AppendTag(builder, LightningUnlocked, "闪电");
                AppendTag(builder, IceUnlocked, "冰霜");
                AppendTag(builder, FireUnlocked, "火焰");
                AppendTag(builder, PoisonUnlocked, "毒性");
                AppendTag(builder, SonicBurstUnlocked, "音波");
                AppendTag(builder, PoisonTrailUnlocked || IceTrailUnlocked || FireTrailUnlocked, "路径");
                return builder.Length == 0 ? "基础" : builder.ToString();
            }
        }

        public void LearnNextCard(int level, int monsterCount, HeroBuild currentBuild)
        {
            var card = ChooseCard(level, monsterCount, currentBuild);
            Apply(card);
            latestCard = card;
            cardLevels.TryGetValue(card, out var currentLevel);
            cardLevels[card] = currentLevel + 1;
            cards.Add(AIHeroBDCardExtensions.DisplayName(card));
        }

        public List<AIBDDisplayData> GetDisplayCards()
        {
            var result = new List<AIBDDisplayData>();
            foreach (var pair in cardLevels)
            {
                result.Add(AIBDDisplayData.FromCard(pair.Key, pair.Value, pair.Key == latestCard));
            }

            result.Sort((left, right) =>
            {
                var coreCompare = right.IsCore.CompareTo(left.IsCore);
                if (coreCompare != 0) return coreCompare;
                var categoryCompare = left.SortPriority.CompareTo(right.SortPriority);
                if (categoryCompare != 0) return categoryCompare;
                return string.CompareOrdinal(left.Name, right.Name);
            });

            return result;
        }

        public RhythmBeatEvent ScaleBeat(RhythmBeatEvent beat)
        {
            var count = Mathf.Max(1, beat.SpawnCount + ProjectileCountBonus);
            var damage = beat.Damage * DamageMultiplier;
            var speed = beat.Speed * ProjectileSpeedMultiplier;
            return beat.WithCombat(count, speed, damage);
        }

        public ProjectilePayload CreatePayload(RhythmBeatEvent beat, int shotIndex)
        {
            var module = ChooseElement(beat, shotIndex);
            var trail = ChooseTrail(module);
            var color = ElementColor(module);
            return new ProjectilePayload(
                module,
                trail,
                Mathf.Max(0.1f, ProjectileSizeMultiplier),
                PierceCount,
                FireUnlocked ? 1.2f * AoeRadiusMultiplier : 0f,
                IceUnlocked ? 0.55f : 0f,
                PoisonUnlocked ? 3f : 0f,
                LightningUnlocked ? 1 + Mathf.Min(3, cards.Count / 6) : 0,
                1.6f + AreaDurationBonus,
                color);
        }

        public float GetDurationMultiplier()
        {
            return Mathf.Clamp(1f / RhythmSpeedMultiplier, 0.55f, 1.15f);
        }

        public bool ShouldAddSyncopation(RhythmBeatEvent beat)
        {
            return SyncopationUnlocked && beat.Pitch != RhythmPitch.Low;
        }

        private AIHeroBDCard ChooseCard(int level, int monsterCount, HeroBuild currentBuild)
        {
            if (level == 2) return AIHeroBDCard.RhythmSpeed;
            if (level == 3) return AIHeroBDCard.ProjectileCount;
            if (level == 4) return currentBuild == HeroBuild.FlameAura ? AIHeroBDCard.FireBullet : AIHeroBDCard.LightningBullet;
            if (level == 5) return currentBuild == HeroBuild.Lifesteal ? AIHeroBDCard.PoisonBullet : AIHeroBDCard.IceBullet;
            if (level == 6) return AIHeroBDCard.Damage;
            if (level == 7) return monsterCount > 8 ? AIHeroBDCard.SonicBurst : AIHeroBDCard.Pierce;
            if (level == 8) return PoisonUnlocked ? AIHeroBDCard.PoisonTrail : AIHeroBDCard.FireTrail;
            if (level == 9) return AIHeroBDCard.Syncopation;
            if (level == 10) return AIHeroBDCard.HeavyBeat;
            if (level == 11) return AIHeroBDCard.EchoBeat;
            if (level % 4 == 0) return AIHeroBDCard.ProjectileSpeed;
            if (level % 4 == 1) return AIHeroBDCard.Damage;
            if (level % 4 == 2) return AIHeroBDCard.ProjectileCount;
            return AIHeroBDCard.AreaDuration;
        }

        private void Apply(AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.ProjectileSpeed:
                    ProjectileSpeedMultiplier += 0.1f;
                    break;
                case AIHeroBDCard.RhythmSpeed:
                    RhythmSpeedMultiplier += 0.08f;
                    break;
                case AIHeroBDCard.Damage:
                    DamageMultiplier += 0.1f;
                    break;
                case AIHeroBDCard.ProjectileCount:
                    ProjectileCountBonus += 1;
                    break;
                case AIHeroBDCard.BigNote:
                    ProjectileSizeMultiplier += 0.12f;
                    break;
                case AIHeroBDCard.Pierce:
                    PierceCount += 1;
                    break;
                case AIHeroBDCard.LightningBullet:
                    LightningUnlocked = true;
                    break;
                case AIHeroBDCard.IceBullet:
                    IceUnlocked = true;
                    break;
                case AIHeroBDCard.FireBullet:
                    FireUnlocked = true;
                    break;
                case AIHeroBDCard.PoisonBullet:
                    PoisonUnlocked = true;
                    break;
                case AIHeroBDCard.PoisonTrail:
                    PoisonTrailUnlocked = true;
                    break;
                case AIHeroBDCard.IceTrail:
                    IceTrailUnlocked = true;
                    break;
                case AIHeroBDCard.FireTrail:
                    FireTrailUnlocked = true;
                    break;
                case AIHeroBDCard.AreaDuration:
                    AreaDurationBonus += 0.5f;
                    break;
                case AIHeroBDCard.SonicBurst:
                    SonicBurstUnlocked = true;
                    break;
                case AIHeroBDCard.AoeRange:
                    AoeRadiusMultiplier += 0.15f;
                    break;
                case AIHeroBDCard.Syncopation:
                    SyncopationUnlocked = true;
                    break;
                case AIHeroBDCard.HeavyBeat:
                    HeavyBeatUnlocked = true;
                    break;
                case AIHeroBDCard.EchoBeat:
                    EchoBeatUnlocked = true;
                    break;
            }
        }

        private ElementModule ChooseElement(RhythmBeatEvent beat, int shotIndex)
        {
            if (LightningUnlocked && (beat.Pitch == RhythmPitch.High || shotIndex % 4 == 1))
            {
                return ElementModule.Lightning;
            }

            if (IceUnlocked && shotIndex % 4 == 2)
            {
                return ElementModule.Ice;
            }

            if (FireUnlocked && (beat.Pitch == RhythmPitch.Low || shotIndex % 4 == 3))
            {
                return ElementModule.Fire;
            }

            if (PoisonUnlocked && shotIndex % 4 == 0)
            {
                return ElementModule.Poison;
            }

            return ElementModule.None;
        }

        private TrailModule ChooseTrail(ElementModule module)
        {
            if (module == ElementModule.Poison && PoisonTrailUnlocked) return TrailModule.Poison;
            if (module == ElementModule.Ice && IceTrailUnlocked) return TrailModule.Ice;
            if (module == ElementModule.Fire && FireTrailUnlocked) return TrailModule.Fire;
            if (PoisonTrailUnlocked) return TrailModule.Poison;
            if (IceTrailUnlocked) return TrailModule.Ice;
            if (FireTrailUnlocked) return TrailModule.Fire;
            return TrailModule.None;
        }

        public static Color ElementColor(ElementModule module)
        {
            switch (module)
            {
                case ElementModule.Lightning:
                    return new Color(0.45f, 0.65f, 1f);
                case ElementModule.Ice:
                    return new Color(0.62f, 0.92f, 1f);
                case ElementModule.Fire:
                    return new Color(1f, 0.38f, 0.1f);
                case ElementModule.Poison:
                    return new Color(0.34f, 0.95f, 0.26f);
                default:
                    return new Color(0.72f, 0.86f, 1f);
            }
        }

        private static void AppendTag(StringBuilder builder, bool condition, string tag)
        {
            if (!condition)
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append(" / ");
            }

            builder.Append(tag);
        }
    }

    public enum AIHeroBDCard
    {
        ProjectileSpeed,
        RhythmSpeed,
        Damage,
        ProjectileCount,
        BigNote,
        Pierce,
        LightningBullet,
        IceBullet,
        FireBullet,
        PoisonBullet,
        PoisonTrail,
        IceTrail,
        FireTrail,
        AreaDuration,
        SonicBurst,
        AoeRange,
        Syncopation,
        HeavyBeat,
        EchoBeat
    }

    public enum BDRarity
    {
        Common,
        Rare,
        Epic,
        Core
    }

    public enum BDCategory
    {
        BaseAttribute,
        Projectile,
        PathArea,
        AOE,
        Rhythm,
        Status,
        Special,
        CoreBuild
    }

    public readonly struct AIBDDisplayData
    {
        public AIBDDisplayData(AIHeroBDCard card, string name, int level, BDCategory category, BDRarity rarity, ElementModule element, string iconText, Color color, string effect, string audio, string vfx, string nextLevel, string counter, bool isNew)
        {
            Card = card;
            Name = name;
            Level = level;
            Category = category;
            Rarity = rarity;
            Element = element;
            IconText = iconText;
            Color = color;
            Effect = effect;
            Audio = audio;
            Vfx = vfx;
            NextLevel = nextLevel;
            Counter = counter;
            IsNew = isNew;
        }

        public AIHeroBDCard Card { get; }
        public string Name { get; }
        public int Level { get; }
        public BDCategory Category { get; }
        public BDRarity Rarity { get; }
        public ElementModule Element { get; }
        public string IconText { get; }
        public Color Color { get; }
        public string Effect { get; }
        public string Audio { get; }
        public string Vfx { get; }
        public string NextLevel { get; }
        public string Counter { get; }
        public bool IsNew { get; }
        public bool IsCore => Rarity == BDRarity.Core || Category == BDCategory.CoreBuild;
        public int SortPriority => Category == BDCategory.CoreBuild ? 0 : Category == BDCategory.Projectile ? 1 : Category == BDCategory.PathArea ? 2 : Category == BDCategory.AOE ? 3 : Category == BDCategory.Rhythm ? 4 : Category == BDCategory.BaseAttribute ? 5 : 6;
        public string CategoryName => DisplayCategory(Category);
        public string RarityName => DisplayRarity(Rarity);
        public string ElementName => DisplayElement(Element);

        public static AIBDDisplayData FromCard(AIHeroBDCard card, int level, bool isNew)
        {
            var name = AIHeroBDCardExtensions.DisplayName(card);
            var category = GetCategory(card);
            var rarity = GetRarity(card);
            var element = GetElement(card);
            var color = GetColor(category, element, rarity);
            return new AIBDDisplayData(
                card,
                name,
                level,
                category,
                rarity,
                element,
                GetIconText(card),
                color,
                GetEffect(card, level),
                GetAudio(card),
                GetVfx(card),
                GetNextLevel(card),
                GetCounter(card),
                isNew);
        }

        private static BDCategory GetCategory(AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.LightningBullet:
                case AIHeroBDCard.IceBullet:
                case AIHeroBDCard.FireBullet:
                case AIHeroBDCard.PoisonBullet:
                    return BDCategory.Projectile;
                case AIHeroBDCard.PoisonTrail:
                case AIHeroBDCard.IceTrail:
                case AIHeroBDCard.FireTrail:
                case AIHeroBDCard.AreaDuration:
                    return BDCategory.PathArea;
                case AIHeroBDCard.SonicBurst:
                case AIHeroBDCard.AoeRange:
                    return BDCategory.AOE;
                case AIHeroBDCard.RhythmSpeed:
                case AIHeroBDCard.Syncopation:
                case AIHeroBDCard.HeavyBeat:
                case AIHeroBDCard.EchoBeat:
                    return BDCategory.Rhythm;
                case AIHeroBDCard.Pierce:
                    return BDCategory.Special;
                default:
                    return BDCategory.BaseAttribute;
            }
        }

        private static BDRarity GetRarity(AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.Syncopation:
                case AIHeroBDCard.HeavyBeat:
                case AIHeroBDCard.EchoBeat:
                    return BDRarity.Epic;
                case AIHeroBDCard.SonicBurst:
                    return BDRarity.Core;
                case AIHeroBDCard.LightningBullet:
                case AIHeroBDCard.IceBullet:
                case AIHeroBDCard.FireBullet:
                case AIHeroBDCard.PoisonBullet:
                case AIHeroBDCard.PoisonTrail:
                case AIHeroBDCard.IceTrail:
                case AIHeroBDCard.FireTrail:
                case AIHeroBDCard.Pierce:
                    return BDRarity.Rare;
                default:
                    return BDRarity.Common;
            }
        }

        private static ElementModule GetElement(AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.LightningBullet:
                    return ElementModule.Lightning;
                case AIHeroBDCard.IceBullet:
                case AIHeroBDCard.IceTrail:
                    return ElementModule.Ice;
                case AIHeroBDCard.FireBullet:
                case AIHeroBDCard.FireTrail:
                    return ElementModule.Fire;
                case AIHeroBDCard.PoisonBullet:
                case AIHeroBDCard.PoisonTrail:
                    return ElementModule.Poison;
                default:
                    return ElementModule.None;
            }
        }

        private static string GetIconText(AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.ProjectileSpeed: return "速";
                case AIHeroBDCard.RhythmSpeed: return "拍";
                case AIHeroBDCard.Damage: return "伤";
                case AIHeroBDCard.ProjectileCount: return "+1";
                case AIHeroBDCard.BigNote: return "大";
                case AIHeroBDCard.Pierce: return "穿";
                case AIHeroBDCard.LightningBullet: return "雷";
                case AIHeroBDCard.IceBullet: return "冰";
                case AIHeroBDCard.FireBullet: return "火";
                case AIHeroBDCard.PoisonBullet: return "毒";
                case AIHeroBDCard.PoisonTrail: return "毒径";
                case AIHeroBDCard.IceTrail: return "冰径";
                case AIHeroBDCard.FireTrail: return "火径";
                case AIHeroBDCard.AreaDuration: return "延";
                case AIHeroBDCard.SonicBurst: return "波";
                case AIHeroBDCard.AoeRange: return "域";
                case AIHeroBDCard.Syncopation: return "切";
                case AIHeroBDCard.HeavyBeat: return "重";
                case AIHeroBDCard.EchoBeat: return "回";
                default: return "构";
            }
        }

        private static Color GetColor(BDCategory category, ElementModule element, BDRarity rarity)
        {
            if (rarity == BDRarity.Core) return new Color(1f, 0.78f, 0.28f);
            if (element != ElementModule.None) return AIHeroBuildData.ElementColor(element);
            switch (category)
            {
                case BDCategory.PathArea: return new Color(0.35f, 0.85f, 0.45f);
                case BDCategory.AOE: return new Color(0.72f, 0.45f, 1f);
                case BDCategory.Rhythm: return new Color(1f, 0.86f, 0.25f);
                case BDCategory.Special: return new Color(1f, 0.34f, 0.34f);
                default: return new Color(0.86f, 0.88f, 0.92f);
            }
        }

        private static string GetEffect(AIHeroBDCard card, int level)
        {
            switch (card)
            {
                case AIHeroBDCard.ProjectileSpeed: return $"子弹速度 +{level * 10}%。";
                case AIHeroBDCard.RhythmSpeed: return $"攻击节奏间隔约缩短 {level * 8}%。";
                case AIHeroBDCard.Damage: return $"全部角色伤害 +{level * 10}%。";
                case AIHeroBDCard.ProjectileCount: return $"每个节拍额外发射 {level} 枚子弹。";
                case AIHeroBDCard.Pierce: return $"子弹可穿透 {level} 个目标。";
                case AIHeroBDCard.LightningBullet: return "高音节拍可转为闪电子弹，并在近距离单位间弹射。";
                case AIHeroBDCard.IceBullet: return "部分节拍转为冰霜子弹，短暂冻结目标。";
                case AIHeroBDCard.FireBullet: return "低音或错位节拍会爆成火焰范围伤害。";
                case AIHeroBDCard.PoisonBullet: return "部分节拍使目标中毒，造成持续伤害。";
                case AIHeroBDCard.PoisonTrail: return "子弹飞行后留下毒气路径区域。";
                case AIHeroBDCard.IceTrail: return "子弹飞行后留下减速冰霜路径。";
                case AIHeroBDCard.FireTrail: return "子弹飞行后留下燃烧路径。";
                case AIHeroBDCard.AreaDuration: return $"路径区域持续时间 +{level * 0.5f:0.0}秒。";
                case AIHeroBDCard.SonicBurst: return "低音节拍会在音乐疯子周围触发音波范围脉冲。";
                case AIHeroBDCard.AoeRange: return $"范围半径 +{level * 15}%。";
                case AIHeroBDCard.Syncopation: return "主节拍之间插入额外高音子弹。";
                case AIHeroBDCard.HeavyBeat: return "每第四拍变为更重的低音攻击。";
                case AIHeroBDCard.EchoBeat: return "主攻击会生成延迟回声脉冲。";
                default: return "强化当前角色构筑。";
            }
        }

        private static string GetAudio(AIHeroBDCard card)
        {
            switch (GetCategory(card))
            {
                case BDCategory.Projectile: return "为节奏轨加入清晰的元素音色。";
                case BDCategory.PathArea: return "在主节拍下方加入持续铺底声。";
                case BDCategory.AOE: return "加入低频脉冲打击感。";
                case BDCategory.Rhythm: return "改变节拍密度或重音时机。";
                default: return "让基础节奏更强或更快。";
            }
        }

        private static string GetVfx(AIHeroBDCard card)
        {
            switch (GetCategory(card))
            {
                case BDCategory.Projectile: return "元素色子弹与命中特效。";
                case BDCategory.PathArea: return "子弹路径上留下可见区域。";
                case BDCategory.AOE: return "音乐疯子周围展开脉冲圆环。";
                case BDCategory.Rhythm: return "更密集或更重的节拍闪光。";
                default: return "子弹大小、速度或命中力度变化。";
            }
        }

        private static string GetNextLevel(AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.ProjectileCount: return "每拍再增加一枚子弹。";
                case AIHeroBDCard.Damage: return "继续提升全局伤害。";
                case AIHeroBDCard.RhythmSpeed: return "进一步缩短节拍间隔。";
                case AIHeroBDCard.AreaDuration: return "路径区域持续更久。";
                default: return "强化当前构筑，或后续解锁更强变体。";
            }
        }

        private static string GetCounter(AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.LightningBullet: return "分散召唤，避免怪物密集被弹射。";
                case AIHeroBDCard.IceBullet: return "用低费单位分摊控制效果。";
                case AIHeroBDCard.FireBullet:
                case AIHeroBDCard.FireTrail: return "避免低血量单位扎堆。";
                case AIHeroBDCard.PoisonBullet:
                case AIHeroBDCard.PoisonTrail: return "不要让精英单位长时间站在毒区。";
                case AIHeroBDCard.SonicBurst: return "从远处召唤，等音波后再压上。";
                case AIHeroBDCard.RhythmSpeed: return "等待收招窗口，再投入脆弱单位。";
                default: return "用时机、站位和更肉的单位降低收益。";
            }
        }

        private static string DisplayCategory(BDCategory category)
        {
            switch (category)
            {
                case BDCategory.BaseAttribute: return "基础属性";
                case BDCategory.Projectile: return "弹体";
                case BDCategory.PathArea: return "路径区域";
                case BDCategory.AOE: return "范围";
                case BDCategory.Rhythm: return "节奏";
                case BDCategory.Status: return "状态";
                case BDCategory.Special: return "特殊";
                case BDCategory.CoreBuild: return "核心流派";
                default: return "构筑";
            }
        }

        private static string DisplayRarity(BDRarity rarity)
        {
            switch (rarity)
            {
                case BDRarity.Common: return "普通";
                case BDRarity.Rare: return "稀有";
                case BDRarity.Epic: return "史诗";
                case BDRarity.Core: return "核心";
                default: return "普通";
            }
        }

        private static string DisplayElement(ElementModule element)
        {
            switch (element)
            {
                case ElementModule.Lightning: return "闪电";
                case ElementModule.Ice: return "冰霜";
                case ElementModule.Fire: return "火焰";
                case ElementModule.Poison: return "毒性";
                default: return "无元素";
            }
        }
    }

    public static class AIHeroBDCardExtensions
    {
        public static string DisplayName(this AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.ProjectileSpeed: return "迅捷弹道";
                case AIHeroBDCard.RhythmSpeed: return "快速节拍";
                case AIHeroBDCard.Damage: return "增幅音色";
                case AIHeroBDCard.ProjectileCount: return "双重音符";
                case AIHeroBDCard.BigNote: return "巨大音符";
                case AIHeroBDCard.Pierce: return "穿透音色";
                case AIHeroBDCard.LightningBullet: return "闪电音符";
                case AIHeroBDCard.IceBullet: return "冰晶音符";
                case AIHeroBDCard.FireBullet: return "火焰鼓点";
                case AIHeroBDCard.PoisonBullet: return "毒性低语";
                case AIHeroBDCard.PoisonTrail: return "毒气路径";
                case AIHeroBDCard.IceTrail: return "冰霜路径";
                case AIHeroBDCard.FireTrail: return "燃烧路径";
                case AIHeroBDCard.AreaDuration: return "延音层";
                case AIHeroBDCard.SonicBurst: return "音波爆发";
                case AIHeroBDCard.AoeRange: return "扩音半径";
                case AIHeroBDCard.Syncopation: return "切分节奏";
                case AIHeroBDCard.HeavyBeat: return "重拍";
                case AIHeroBDCard.EchoBeat: return "回声拍";
                default: return card.ToString();
            }
        }
    }

    public readonly struct ProjectilePayload
    {
        public ProjectilePayload(ElementModule element, TrailModule trail, float sizeMultiplier, int pierceCount, float fireRadius, float freezeSeconds, float poisonSeconds, int lightningBounces, float trailDuration, Color color)
        {
            Element = element;
            Trail = trail;
            SizeMultiplier = sizeMultiplier;
            PierceCount = pierceCount;
            FireRadius = fireRadius;
            FreezeSeconds = freezeSeconds;
            PoisonSeconds = poisonSeconds;
            LightningBounces = lightningBounces;
            TrailDuration = trailDuration;
            Color = color;
        }

        public ElementModule Element { get; }
        public TrailModule Trail { get; }
        public float SizeMultiplier { get; }
        public int PierceCount { get; }
        public float FireRadius { get; }
        public float FreezeSeconds { get; }
        public float PoisonSeconds { get; }
        public int LightningBounces { get; }
        public float TrailDuration { get; }
        public Color Color { get; }
    }
}
