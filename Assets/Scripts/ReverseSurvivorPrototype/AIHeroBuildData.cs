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
                    return "Multi-Element Chord";
                }

                if (SonicBurstUnlocked && HeavyBeatUnlocked)
                {
                    return "Sonic Shock";
                }

                if (PoisonUnlocked && PoisonTrailUnlocked)
                {
                    return "Poison Requiem";
                }

                if (FireUnlocked)
                {
                    return "Fire Drum";
                }

                if (LightningUnlocked)
                {
                    return "Lightning Staccato";
                }

                if (IceUnlocked)
                {
                    return "Frost Melody";
                }

                return "Single-Tone Bullet";
            }
        }

        public string ElementTags
        {
            get
            {
                var builder = new StringBuilder();
                AppendTag(builder, LightningUnlocked, "Lightning");
                AppendTag(builder, IceUnlocked, "Ice");
                AppendTag(builder, FireUnlocked, "Fire");
                AppendTag(builder, PoisonUnlocked, "Poison");
                AppendTag(builder, SonicBurstUnlocked, "Sonic");
                AppendTag(builder, PoisonTrailUnlocked || IceTrailUnlocked || FireTrailUnlocked, "Trail");
                return builder.Length == 0 ? "Basic" : builder.ToString();
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
                case AIHeroBDCard.ProjectileSpeed: return "SPD";
                case AIHeroBDCard.RhythmSpeed: return "BPM";
                case AIHeroBDCard.Damage: return "DMG";
                case AIHeroBDCard.ProjectileCount: return "+1";
                case AIHeroBDCard.BigNote: return "BIG";
                case AIHeroBDCard.Pierce: return "PEN";
                case AIHeroBDCard.LightningBullet: return "LGT";
                case AIHeroBDCard.IceBullet: return "ICE";
                case AIHeroBDCard.FireBullet: return "FIR";
                case AIHeroBDCard.PoisonBullet: return "TOX";
                case AIHeroBDCard.PoisonTrail: return "PTH";
                case AIHeroBDCard.IceTrail: return "ITH";
                case AIHeroBDCard.FireTrail: return "FTH";
                case AIHeroBDCard.AreaDuration: return "DUR";
                case AIHeroBDCard.SonicBurst: return "AOE";
                case AIHeroBDCard.AoeRange: return "RNG";
                case AIHeroBDCard.Syncopation: return "SYN";
                case AIHeroBDCard.HeavyBeat: return "LOW";
                case AIHeroBDCard.EchoBeat: return "ECO";
                default: return "BD";
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
                case AIHeroBDCard.ProjectileSpeed: return $"Projectile speed +{level * 10}%.";
                case AIHeroBDCard.RhythmSpeed: return $"Attack rhythm interval reduced by about {level * 8}%.";
                case AIHeroBDCard.Damage: return $"All AI damage +{level * 10}%.";
                case AIHeroBDCard.ProjectileCount: return $"Each beat fires {level} extra projectile(s).";
                case AIHeroBDCard.Pierce: return $"Projectiles pierce {level} target(s).";
                case AIHeroBDCard.LightningBullet: return $"High notes can become lightning, bouncing between nearby units.";
                case AIHeroBDCard.IceBullet: return $"Some notes become ice bullets and briefly freeze targets.";
                case AIHeroBDCard.FireBullet: return $"Low or offset notes can explode into fire AOE.";
                case AIHeroBDCard.PoisonBullet: return $"Some notes poison targets for sustained damage.";
                case AIHeroBDCard.PoisonTrail: return $"Projectiles leave poison trail zones behind.";
                case AIHeroBDCard.IceTrail: return $"Projectiles leave slowing frost trail zones behind.";
                case AIHeroBDCard.FireTrail: return $"Projectiles leave burning trail zones behind.";
                case AIHeroBDCard.AreaDuration: return $"Trail areas last +{level * 0.5f:0.0}s.";
                case AIHeroBDCard.SonicBurst: return "Low beats trigger an AOE sonic pulse around the hero.";
                case AIHeroBDCard.AoeRange: return $"AOE radius +{level * 15}%.";
                case AIHeroBDCard.Syncopation: return "Extra high-note bullets are inserted between main beats.";
                case AIHeroBDCard.HeavyBeat: return "Every fourth beat becomes a heavier low-note attack.";
                case AIHeroBDCard.EchoBeat: return "Main attacks create a delayed echo pulse.";
                default: return "Improves the AI build.";
            }
        }

        private static string GetAudio(AIHeroBDCard card)
        {
            switch (GetCategory(card))
            {
                case BDCategory.Projectile: return "Adds a distinct element tone to the rhythm track.";
                case BDCategory.PathArea: return "Adds a sustained layer under the main beat.";
                case BDCategory.AOE: return "Adds low-frequency pulse hits.";
                case BDCategory.Rhythm: return "Changes beat density or accent timing.";
                default: return "Makes the base rhythm sound stronger or faster.";
            }
        }

        private static string GetVfx(AIHeroBDCard card)
        {
            switch (GetCategory(card))
            {
                case BDCategory.Projectile: return "Element-colored projectile and hit feedback.";
                case BDCategory.PathArea: return "Visible trail left along projectile paths.";
                case BDCategory.AOE: return "Expanding pulse ring around the AI hero.";
                case BDCategory.Rhythm: return "More frequent or accented beat flashes.";
                default: return "Projectile scale, speed, or impact intensity changes.";
            }
        }

        private static string GetNextLevel(AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.ProjectileCount: return "Adds one more projectile per beat.";
                case AIHeroBDCard.Damage: return "Increases global damage again.";
                case AIHeroBDCard.RhythmSpeed: return "Further shortens beat interval.";
                case AIHeroBDCard.AreaDuration: return "Trail zones persist longer.";
                default: return "Strengthens this BD or unlocks a stronger variant later.";
            }
        }

        private static string GetCounter(AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.LightningBullet: return "Spread summons out; avoid dense clusters.";
                case AIHeroBDCard.IceBullet: return "Use many low-value units to split control.";
                case AIHeroBDCard.FireBullet:
                case AIHeroBDCard.FireTrail: return "Avoid clustered low-health summons.";
                case AIHeroBDCard.PoisonBullet:
                case AIHeroBDCard.PoisonTrail: return "Do not let elites sit in poison for long.";
                case AIHeroBDCard.SonicBurst: return "Summon from range and punish after the pulse.";
                case AIHeroBDCard.RhythmSpeed: return "Wait for end-lag before committing fragile units.";
                default: return "Use timing, spacing, and tougher units to reduce value.";
            }
        }
    }

    public static class AIHeroBDCardExtensions
    {
        public static string DisplayName(this AIHeroBDCard card)
        {
            switch (card)
            {
                case AIHeroBDCard.ProjectileSpeed: return "Swift Ballistics";
                case AIHeroBDCard.RhythmSpeed: return "Quick Tempo";
                case AIHeroBDCard.Damage: return "Amplified Tone";
                case AIHeroBDCard.ProjectileCount: return "Double Note";
                case AIHeroBDCard.BigNote: return "Big Note";
                case AIHeroBDCard.Pierce: return "Piercing Tone";
                case AIHeroBDCard.LightningBullet: return "Lightning Note";
                case AIHeroBDCard.IceBullet: return "Ice Crystal Note";
                case AIHeroBDCard.FireBullet: return "Fire Drum Note";
                case AIHeroBDCard.PoisonBullet: return "Poison Whisper";
                case AIHeroBDCard.PoisonTrail: return "Poison Trail";
                case AIHeroBDCard.IceTrail: return "Frost Trail";
                case AIHeroBDCard.FireTrail: return "Burning Trail";
                case AIHeroBDCard.AreaDuration: return "Sustain Layer";
                case AIHeroBDCard.SonicBurst: return "Sonic Burst";
                case AIHeroBDCard.AoeRange: return "Amplified Radius";
                case AIHeroBDCard.Syncopation: return "Syncopation";
                case AIHeroBDCard.HeavyBeat: return "Heavy Beat";
                case AIHeroBDCard.EchoBeat: return "Echo Beat";
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
