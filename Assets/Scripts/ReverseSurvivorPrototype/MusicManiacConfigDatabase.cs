using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReverseSurvivorPrototype
{
    [CreateAssetMenu(menuName = "Defeat Music Maniac/配置数据库", fileName = "MusicManiacConfigDatabase")]
    public sealed class MusicManiacConfigDatabase : ScriptableObject
    {
        public List<CharacterAnimationConfig> characterAnimations = new List<CharacterAnimationConfig>();
        public List<ProjectileVisualConfig> projectileVisuals = new List<ProjectileVisualConfig>();
        public List<DamageVisualConfig> damageVisuals = new List<DamageVisualConfig>();
        public List<AoeVfxConfig> aoeVfx = new List<AoeVfxConfig>();
        public List<MonsterBalanceConfig> monsterValues = new List<MonsterBalanceConfig>();
        public List<CreatorSkillValueConfig> skillValues = new List<CreatorSkillValueConfig>();
        public List<UiLayoutConfig> uiLayouts = new List<UiLayoutConfig>();
        public List<UiTextConfig> uiTexts = new List<UiTextConfig>();
        public List<UiButtonGroupConfig> uiButtonGroups = new List<UiButtonGroupConfig>();
    }

    [Serializable]
    public sealed class CharacterAnimationConfig
    {
        public string displayName;
        public string assetId;
        public MonsterKind monsterKind;
        public bool isHero;
        public float worldHeight = 1.25f;
        public List<UnitActionAnimationConfig> actions = new List<UnitActionAnimationConfig>();
    }

    public enum UnitAnimationAction
    {
        Idle,
        Move,
        Attack,
        Hit,
        Death,
        Spawn,
        Cast
    }

    [Serializable]
    public sealed class UnitActionAnimationConfig
    {
        public UnitAnimationAction action = UnitAnimationAction.Idle;
        public string displayName = "待机";
        public float framesPerSecond = 8f;
        public bool loop = true;
        public List<UnitAnimationFrameConfig> frames = new List<UnitAnimationFrameConfig>();
    }

    [Serializable]
    public sealed class UnitAnimationFrameConfig
    {
        public string frameName = "第1帧";
        public Sprite sprite;
        public Vector2 worldSize = new Vector2(1f, 1f);
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        public Vector2 offset;
        public float duration = 0.12f;
    }

    [Serializable]
    public sealed class ProjectileVisualConfig
    {
        public string displayName;
        public ElementModule element;
        public Sprite projectileSprite;
        public float sizeMultiplier = 1.25f;
        public bool rotateToDirection = true;
        public Sprite trailSprite;
        public Sprite hitVfx;
        public Color tint = Color.white;
    }

    [Serializable]
    public sealed class DamageVisualConfig
    {
        public string displayName;
        public DamageFeedbackType damageType;
        public Color textColor = Color.white;
        public int fontSize = 18;
        public float floatHeight = 0.92f;
        public float lifetime = 0.78f;
        public bool mergeFrequentNumbers = true;
        public bool showKillPrefix = true;
        public Sprite hitVfx;
        public AudioClip hitSfx;
    }

    [Serializable]
    public sealed class AoeVfxConfig
    {
        public string displayName;
        public string effectId;
        public Sprite sprite;
        public float radiusMultiplier = 1f;
        public float duration = 0.35f;
        public Color tint = Color.white;
        public bool loop;
        public int sortingOrder = 16;
        public bool matchDamageRadius = true;
        public bool disableLensDistortion = true;
    }

    [Serializable]
    public sealed class MonsterBalanceConfig
    {
        public MonsterKind kind;
        public string displayName;
        public float cost;
        public float health;
        public float damage;
        public float moveSpeed;
        public float attackRange;
        public float attackCooldown;
        public float poisonDamage;
        public Color color = Color.white;
        public string tag;
    }

    [Serializable]
    public sealed class CreatorSkillValueConfig
    {
        public CreatorSkillId id;
        public string displayName;
        public CreatorSkillType type;
        public float cost;
        public float cooldown;
        public float warningTime;
        public float radius;
        public float duration;
        public float damage;
        public float tickDamage;
        public float slowMultiplier = 1f;
        public float antiHealSeconds;
        public float shieldBreakSeconds;
        public float danger;
        public Color warningColor = Color.white;
        public Color effectColor = Color.white;
        public string tag;
    }

    [Serializable]
    public sealed class UiLayoutConfig
    {
        public string id;
        public string displayName;
        public UiAnchorPreset anchor = UiAnchorPreset.TopLeft;
        public Vector2 position;
        public Vector2 size = new Vector2(100f, 40f);
        public Color backgroundColor = Color.white;
        public bool visible = true;
    }

    [Serializable]
    public sealed class UiTextConfig
    {
        public string id;
        public string displayName;
        public string overrideText;
        public int fontSize = 14;
        public Color color = Color.white;
        public TextAnchor alignment = TextAnchor.MiddleCenter;
        public bool visible = true;
    }

    [Serializable]
    public sealed class UiButtonGroupConfig
    {
        public string id;
        public string displayName;
        public Vector2 buttonSize = new Vector2(132f, 50f);
        public Vector2 firstPosition;
        public Vector2 spacing = new Vector2(144f, -56f);
        public int columns = 2;
        public int labelFontSize = 10;
        public int metaFontSize = 9;
        public int statusFontSize = 8;
        public Vector2 iconPosition = new Vector2(22f, 0f);
        public Vector2 iconSize = new Vector2(28f, 28f);
    }

    public enum UiAnchorPreset
    {
        TopLeft,
        TopRight,
        BottomLeft,
        TopCenter,
        BottomStretch,
        TopStretch,
        Stretch
    }
}
