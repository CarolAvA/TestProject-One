using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public enum CreatorSkillId
    {
        LightningStrike,
        FrostField,
        AntiHealCurse,
        ShieldBrand,
        BoneWall,
        DemonHand
    }

    public enum CreatorSkillType
    {
        Damage,
        Control,
        Curse,
        Terrain,
        Finisher
    }

    public sealed class SkillWarning : MonoBehaviour
    {
        private CreatorSkillConfig config;
        private Vector2 position;
        private float remaining;
        private MeshRenderer meshRenderer;
        private SpriteRenderer spriteRenderer;
        private Vector3 spriteBaseScale = Vector3.one;
        private LineRenderer outerRing;
        private LineRenderer innerRing;

        public CreatorSkillConfig Config => config;
        public Vector2 Position => position;
        public float Radius => config.Radius;
        public float Danger => config.Danger;

        public static SkillWarning Create(CreatorSkillConfig config, Vector2 position)
        {
            var warningObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            warningObject.name = $"Warning - {config.DisplayName}";
            warningObject.transform.position = new Vector3(position.x, position.y, -0.45f);
            warningObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            warningObject.transform.localScale = Vector3.one;
            Object.Destroy(warningObject.GetComponent<CapsuleCollider>());

            var warning = warningObject.AddComponent<SkillWarning>();
            warning.Initialize(config, position);
            return warning;
        }

        private void Initialize(CreatorSkillConfig skillConfig, Vector2 warningPosition)
        {
            config = skillConfig;
            position = warningPosition;
            remaining = config.WarningTime;
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = VisualFactory.CreateMaterial(config.WarningColor);
            meshRenderer.enabled = false;
            spriteRenderer = MusicManiacArtLibrary.AttachSprite(
                gameObject,
                MusicManiacArtLibrary.Vfx("vfx_warning_ring"),
                config.Radius * 2.15f,
                20,
                "Skill Warning Pixel Art",
                config.WarningColor);
            if (spriteRenderer != null)
            {
                spriteBaseScale = spriteRenderer.transform.localScale;
                SpriteSequenceAnimator.Attach(spriteRenderer, "vfx/vfx_warning_ring", 8, 14f);
            }

            outerRing = CreateWarningRing("Warning Outer Ring", config.Radius, 0.055f, 48);
            innerRing = CreateWarningRing("Warning Inner Ring", config.Radius * 0.72f, 0.032f, 40);
        }

        private void Update()
        {
            remaining -= Time.deltaTime;
            var pulse = 0.55f + Mathf.PingPong(Time.time * (4f + config.Danger * 3f), 0.45f);
            var color = Color.Lerp(config.WarningColor, Color.white, pulse * 0.35f);
            meshRenderer.material.color = color;
            if (spriteRenderer != null)
            {
                color.a = Mathf.Lerp(0.42f, 0.92f, pulse);
                spriteRenderer.color = color;
                spriteRenderer.transform.localScale = spriteBaseScale * (0.9f + pulse * 0.16f);
                spriteRenderer.transform.Rotate(0f, 0f, Time.deltaTime * (16f + config.Danger * 28f));
            }

            UpdateWarningLines(color, pulse);

            if (remaining <= 0f)
            {
                VisualFactory.CreateAnimatedSpriteBurst(position, SkillVfxLibrary.Prefix(config.Id), config.Radius * 2.25f, config.EffectColor, 0.46f, 24, 8, 18f);
                MusicManiacAudioSystem.Instance.PlaySkill(config.Id, "cast", position, 1f);
                GameDirector.Instance.ResolveSkillImpact(config, position);
                Destroy(gameObject);
            }
        }

        private LineRenderer CreateWarningRing(string lineName, float radius, float width, int points)
        {
            var lineObject = new GameObject(lineName);
            lineObject.transform.SetParent(transform, false);
            lineObject.transform.localPosition = new Vector3(0f, 0f, -0.08f);
            var line = lineObject.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = points;
            line.startWidth = width;
            line.endWidth = width;
            line.sortingOrder = 22;
            line.material = VisualFactory.CreateMaterial(config.WarningColor);
            for (var i = 0; i < points; i++)
            {
                var angle = Mathf.PI * 2f * i / points;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }

            return line;
        }

        private void UpdateWarningLines(Color color, float pulse)
        {
            var alpha = Mathf.Lerp(0.45f, 0.95f, pulse);
            var outerColor = Color.Lerp(color, Color.white, 0.12f);
            outerColor.a = alpha;
            var innerColor = Color.Lerp(config.WarningColor, Color.white, 0.35f);
            innerColor.a = alpha * 0.72f;

            if (outerRing != null)
            {
                outerRing.material.color = outerColor;
                outerRing.startWidth = outerRing.endWidth = Mathf.Lerp(0.04f, 0.075f, pulse);
            }

            if (innerRing != null)
            {
                innerRing.material.color = innerColor;
                innerRing.startWidth = innerRing.endWidth = Mathf.Lerp(0.022f, 0.046f, pulse);
            }
        }
    }

    public sealed class SkillEffectZone : MonoBehaviour
    {
        private CreatorSkillConfig config;
        private Vector2 position;
        private float remaining;
        private float tickTimer;
        private MeshRenderer meshRenderer;
        private SpriteRenderer spriteRenderer;

        public Vector2 Position => position;
        public float Radius => config.Radius;

        public static SkillEffectZone Create(CreatorSkillConfig config, Vector2 position)
        {
            var zoneObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            zoneObject.name = $"Effect - {config.DisplayName}";
            zoneObject.transform.position = new Vector3(position.x, position.y, -0.35f);
            zoneObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            zoneObject.transform.localScale = new Vector3(config.Radius * 2f, 0.035f, config.Radius * 2f);
            Object.Destroy(zoneObject.GetComponent<CapsuleCollider>());

            var zone = zoneObject.AddComponent<SkillEffectZone>();
            zone.Initialize(config, position);
            return zone;
        }

        private void Initialize(CreatorSkillConfig skillConfig, Vector2 zonePosition)
        {
            config = skillConfig;
            position = zonePosition;
            remaining = config.Duration;
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = VisualFactory.CreateMaterial(config.EffectColor);
            meshRenderer.enabled = false;
            spriteRenderer = MusicManiacArtLibrary.AttachSprite(
                gameObject,
                EffectSpriteForSkill(config),
                config.Radius * 2.1f,
                -7,
                "Skill Effect Pixel Art",
                config.EffectColor);
            if (spriteRenderer != null)
            {
                SpriteSequenceAnimator.Attach(spriteRenderer, EffectAnimationPrefix(config), 8, 12f);
            }
        }

        private void Update()
        {
            remaining -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer = 0.35f;
                TickHero();
            }

            if (meshRenderer != null)
            {
                meshRenderer.material.color = Color.Lerp(config.EffectColor, Color.black, Mathf.PingPong(Time.time * 1.6f, 0.2f));
            }

            if (spriteRenderer != null)
            {
                var color = Color.Lerp(config.EffectColor, Color.white, Mathf.PingPong(Time.time * 1.6f, 0.18f));
                color.a = Mathf.Clamp01(remaining / Mathf.Max(0.1f, config.Duration)) * 0.82f;
                spriteRenderer.color = color;
                spriteRenderer.transform.Rotate(0f, 0f, Time.deltaTime * (18f + config.Danger * 22f));
            }

            if (remaining <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private static Sprite EffectSpriteForSkill(CreatorSkillConfig config)
        {
            switch (config.Id)
            {
                case CreatorSkillId.FrostField:
                    return MusicManiacArtLibrary.Vfx("vfx_aoe_ice");
                case CreatorSkillId.AntiHealCurse:
                    return MusicManiacArtLibrary.Vfx("vfx_aoe_poison");
                case CreatorSkillId.ShieldBrand:
                    return MusicManiacArtLibrary.Vfx("vfx_aoe_sonic");
                default:
                    return MusicManiacArtLibrary.Vfx("vfx_warning_ring");
            }
        }

        private static string EffectAnimationPrefix(CreatorSkillConfig config)
        {
            switch (config.Id)
            {
                case CreatorSkillId.FrostField:
                    return "vfx/vfx_aoe_ice";
                case CreatorSkillId.AntiHealCurse:
                    return "vfx/vfx_aoe_poison";
                case CreatorSkillId.ShieldBrand:
                    return "vfx/vfx_aoe_sonic";
                default:
                    return "vfx/vfx_warning_ring";
            }
        }

        private void TickHero()
        {
            var hero = GameDirector.Instance.Hero;
            if (hero == null || Vector2.Distance(hero.Position, position) > config.Radius)
            {
                return;
            }

            if (config.SlowMultiplier < 1f)
            {
                hero.ApplySkillSlow(config.SlowMultiplier, 0.5f);
            }

            if (config.AntiHealSeconds > 0f)
            {
                hero.ApplyAntiHeal(config.AntiHealSeconds);
            }

            if (config.ShieldBreakSeconds > 0f)
            {
                hero.ApplyShieldBreak(config.ShieldBreakSeconds);
            }

            if (config.TickDamage > 0f)
            {
                hero.TakeSkillDamage(config.TickDamage * 0.35f, config.AntiHealSeconds > 0f, config.ShieldBreakSeconds > 0f);
            }
        }
    }

    public static class SkillVfxLibrary
    {
        public static string Prefix(CreatorSkillId skillId)
        {
            switch (skillId)
            {
                case CreatorSkillId.LightningStrike:
                    return "vfx/vfx_skill_lightning";
                case CreatorSkillId.FrostField:
                    return "vfx/vfx_skill_frost_field";
                case CreatorSkillId.AntiHealCurse:
                    return "vfx/vfx_skill_anti_heal";
                case CreatorSkillId.ShieldBrand:
                    return "vfx/vfx_skill_shield_brand";
                case CreatorSkillId.BoneWall:
                    return "vfx/vfx_skill_bone_wall";
                case CreatorSkillId.DemonHand:
                    return "vfx/vfx_skill_demon_hand";
                default:
                    return "vfx/vfx_hit_spark";
            }
        }
    }

    public sealed class TemporaryWall : MonoBehaviour
    {
        private float remaining;
        private MeshRenderer meshRenderer;
        private SpriteRenderer spriteRenderer;

        public Vector2 Position => transform.position;
        public Vector2 Size { get; private set; }

        public static TemporaryWall Create(Vector2 position, Vector2 size, float duration)
        {
            var wallObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallObject.name = "Temporary Bone Wall";
            wallObject.transform.position = new Vector3(position.x, position.y, -0.2f);
            wallObject.transform.localScale = new Vector3(size.x, size.y, 0.35f);
            Object.Destroy(wallObject.GetComponent<BoxCollider>());

            var wall = wallObject.AddComponent<TemporaryWall>();
            wall.Initialize(size, duration);
            return wall;
        }

        private void Initialize(Vector2 size, float duration)
        {
            Size = size;
            remaining = duration;
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = VisualFactory.CreateMaterial(new Color(0.78f, 0.76f, 0.62f));
            meshRenderer.enabled = false;
            spriteRenderer = MusicManiacArtLibrary.AttachSprite(
                gameObject,
                MusicManiacArtLibrary.Icon("skill_bone_wall"),
                Mathf.Max(size.x, size.y),
                4,
                "Bone Wall Pixel Art",
                Color.white);
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);
            }
        }

        private void Update()
        {
            remaining -= Time.deltaTime;
            if (meshRenderer != null)
            {
                meshRenderer.material.color = Color.Lerp(new Color(0.78f, 0.76f, 0.62f), Color.black, Mathf.PingPong(Time.time * 2f, 0.18f));
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(new Color(0.78f, 0.76f, 0.62f), Color.white, Mathf.PingPong(Time.time * 2f, 0.18f));
            }

            if (remaining <= 0f)
            {
                Destroy(gameObject);
            }
        }

        public bool IsNear(Vector2 point, float padding)
        {
            var delta = point - Position;
            return Mathf.Abs(delta.x) < Size.x * 0.5f + padding && Mathf.Abs(delta.y) < Size.y * 0.5f + padding;
        }

        public Vector2 EscapeDirection(Vector2 point)
        {
            var delta = point - Position;
            if (Mathf.Abs(delta.x / Mathf.Max(Size.x, 0.01f)) > Mathf.Abs(delta.y / Mathf.Max(Size.y, 0.01f)))
            {
                return new Vector2(Mathf.Sign(delta.x), 0f);
            }

            return new Vector2(0f, Mathf.Sign(delta.y));
        }
    }

    public readonly struct CreatorSkillConfig
    {
        public CreatorSkillConfig(
            CreatorSkillId id,
            string displayName,
            CreatorSkillType type,
            float cost,
            float cooldown,
            float warningTime,
            float radius,
            float duration,
            float damage,
            float tickDamage,
            float slowMultiplier,
            float antiHealSeconds,
            float shieldBreakSeconds,
            float danger,
            Color warningColor,
            Color effectColor,
            string tag)
        {
            Id = id;
            DisplayName = displayName;
            Type = type;
            Cost = cost;
            Cooldown = cooldown;
            WarningTime = warningTime;
            Radius = radius;
            Duration = duration;
            Damage = damage;
            TickDamage = tickDamage;
            SlowMultiplier = slowMultiplier;
            AntiHealSeconds = antiHealSeconds;
            ShieldBreakSeconds = shieldBreakSeconds;
            Danger = danger;
            WarningColor = warningColor;
            EffectColor = effectColor;
            Tag = tag;
        }

        public CreatorSkillId Id { get; }
        public string DisplayName { get; }
        public CreatorSkillType Type { get; }
        public float Cost { get; }
        public float Cooldown { get; }
        public float WarningTime { get; }
        public float Radius { get; }
        public float Duration { get; }
        public float Damage { get; }
        public float TickDamage { get; }
        public float SlowMultiplier { get; }
        public float AntiHealSeconds { get; }
        public float ShieldBreakSeconds { get; }
        public float Danger { get; }
        public Color WarningColor { get; }
        public Color EffectColor { get; }
        public string Tag { get; }
    }
}
