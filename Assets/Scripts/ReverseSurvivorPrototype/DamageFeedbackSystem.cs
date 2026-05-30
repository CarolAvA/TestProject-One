using System.Collections.Generic;
using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public enum DamageFeedbackType
    {
        Physical,
        Fire,
        Ice,
        Lightning,
        Poison,
        Sonic,
        Shadow,
        ShieldBreak,
        Heal
    }

    public enum BubbleTalkEvent
    {
        SkillHit,
        HeavyHit,
        Controlled,
        Slowed,
        AntiHeal,
        ShieldBreak,
        LowHealth,
        LevelUp,
        DodgedSkill,
        Surrounded
    }

    public sealed class DamageFeedbackSystem : MonoBehaviour
    {
        private static DamageFeedbackSystem instance;

        private readonly Dictionary<DamageFeedbackType, float> sfxCooldowns = new Dictionary<DamageFeedbackType, float>();
        private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
        private readonly Dictionary<int, float> targetNumberCooldowns = new Dictionary<int, float>();
        private readonly Dictionary<BubbleTalkEvent, float> bubbleCooldowns = new Dictionary<BubbleTalkEvent, float>();
        private readonly List<string> recentBubbleLines = new List<string>();

        private AudioSource audioSource;
        private float globalHitStopCooldown;
        private float bubbleGlobalCooldown;
        private float heroMinorFeedbackCooldown;
        private int activeBubbleCount;

        public static DamageFeedbackSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    Create();
                }

                return instance;
            }
        }

        public static DamageFeedbackSystem Create()
        {
            if (instance != null)
            {
                return instance;
            }

            var feedbackObject = new GameObject("Damage Feedback System");
            instance = feedbackObject.AddComponent<DamageFeedbackSystem>();
            return instance;
        }

        public static void ResetInstance()
        {
            if (instance == null)
            {
                return;
            }

            var current = instance;
            instance = null;
            if (current != null)
            {
                Destroy(current.gameObject);
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.36f;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
            var keys = new List<DamageFeedbackType>(sfxCooldowns.Keys);
            foreach (var key in keys)
            {
                sfxCooldowns[key] = Mathf.Max(0f, sfxCooldowns[key] - Time.unscaledDeltaTime);
            }

            var targetKeys = new List<int>(targetNumberCooldowns.Keys);
            foreach (var key in targetKeys)
            {
                targetNumberCooldowns[key] = Mathf.Max(0f, targetNumberCooldowns[key] - Time.unscaledDeltaTime);
            }

            var bubbleKeys = new List<BubbleTalkEvent>(bubbleCooldowns.Keys);
            foreach (var key in bubbleKeys)
            {
                bubbleCooldowns[key] = Mathf.Max(0f, bubbleCooldowns[key] - Time.unscaledDeltaTime);
            }

            globalHitStopCooldown = Mathf.Max(0f, globalHitStopCooldown - Time.unscaledDeltaTime);
            bubbleGlobalCooldown = Mathf.Max(0f, bubbleGlobalCooldown - Time.unscaledDeltaTime);
            heroMinorFeedbackCooldown = Mathf.Max(0f, heroMinorFeedbackCooldown - Time.unscaledDeltaTime);
        }

        public void ReportMonsterDamage(MonsterUnit monster, float amount, DamageFeedbackType type, Vector2 sourcePosition, bool isDot, bool isAoe, bool isCritical, bool isKill)
        {
            if (monster == null)
            {
                return;
            }

            var color = GetDamageColor(type);
            var position = monster.Position + new Vector2(Random.Range(-0.16f, 0.16f), 0.52f + Random.Range(0f, 0.22f));
            var label = BuildDamageLabel(amount, type, isDot, isCritical, isKill);
            var targetId = monster.GetInstanceID();
            var canShowNumber = isCritical || isKill || !targetNumberCooldowns.TryGetValue(targetId, out var numberCooldown) || numberCooldown <= 0f;
            if (canShowNumber)
            {
                targetNumberCooldowns[targetId] = isDot ? 0.2f : 0.08f;
                CreateDamageNumber(position, label, color, isCritical, isDot, isKill);
            }

            CreateHitVfx(monster.Position, type, isCritical || isKill, isAoe);
            PlayHitSfx(type, isCritical || isKill, isDot);
            MusicManiacAudioSystem.Instance.PlayHit(type, isCritical || isKill || isAoe, monster.Position, isDot ? 0.45f : 0.8f);
            if (!isDot)
            {
                MusicManiacAudioSystem.Instance.PlayMonster(monster.Config.Kind, "hit", monster.Position, isKill ? 0.35f : 0.55f);
            }

            var flashTime = isDot ? 0.06f : isCritical ? 0.2f : type == DamageFeedbackType.Physical ? 0.1f : 0.15f;
            var hitStop = GetHitStop(type, isDot, isCritical, isKill);
            var knockback = GetKnockback(type, isDot, isAoe, isCritical, isKill);
            monster.ApplyHitReaction(Color.Lerp(color, Color.white, isCritical ? 0.5f : 0.25f), sourcePosition, knockback, hitStop, flashTime);

            if (!isDot)
            {
                var level = isKill
                    ? FeelImpactSystem.LevelForMonster(monster.Config, true)
                    : isCritical || isAoe
                        ? FeelImpactLevel.Light
                        : FeelImpactLevel.Micro;
                var impactEvent = isKill
                    ? monster.Config.Kind == MonsterKind.BoneKing ? FeelImpactEvent.BossDeath : FeelImpactEvent.MonsterDeath
                    : isCritical ? FeelImpactEvent.CriticalHit : FeelImpactEvent.Hit;
                FeelImpactSystem.Instance.Play(impactEvent, level, monster.Position, color);
            }
        }

        public void ReportHeroDamage(HeroController hero, float amount, DamageFeedbackType type, Vector2 sourcePosition, bool isSkill, bool isControl, bool isShieldBreak, bool isLowHealth, bool isHeavy)
        {
            if (hero == null || amount <= 0f)
            {
                return;
            }

            if (amount < 1f && !isSkill && !isHeavy && !isShieldBreak)
            {
                if (heroMinorFeedbackCooldown > 0f)
                {
                    return;
                }

                heroMinorFeedbackCooldown = 0.22f;
            }

            var color = isShieldBreak ? GetDamageColor(DamageFeedbackType.ShieldBreak) : GetDamageColor(type);
            CreateDamageNumber(hero.Position + new Vector2(Random.Range(-0.18f, 0.18f), 0.74f), BuildHeroDamageLabel(amount, type, isShieldBreak, isHeavy), color, isHeavy, false, false);
            CreateHitVfx(hero.Position, isShieldBreak ? DamageFeedbackType.ShieldBreak : type, isHeavy, false);
            PlayHitSfx(type, isHeavy, false);
            MusicManiacAudioSystem.Instance.PlayHit(isShieldBreak ? DamageFeedbackType.ShieldBreak : type, isHeavy, hero.Position, 0.85f);
            hero.ApplyHitReaction(Color.Lerp(color, Color.white, 0.35f), sourcePosition, isHeavy ? 0.18f : 0.08f, isHeavy ? 0.08f : 0.04f, isHeavy ? 0.22f : 0.14f);
            FeelImpactSystem.Instance.Play(FeelImpactEvent.HeroHit, isSkill ? (isHeavy ? FeelImpactLevel.Heavy : FeelImpactLevel.Medium) : isHeavy ? FeelImpactLevel.Medium : FeelImpactLevel.Light, hero.Position, color);

            if (isLowHealth)
            {
                TryShowBubble(hero, BubbleTalkEvent.LowHealth, true);
            }
            else if (isShieldBreak)
            {
                TryShowBubble(hero, BubbleTalkEvent.ShieldBreak, true);
            }
            else if (isControl)
            {
                TryShowBubble(hero, BubbleTalkEvent.Controlled, true);
            }
            else if (isSkill)
            {
                TryShowBubble(hero, isHeavy ? BubbleTalkEvent.HeavyHit : BubbleTalkEvent.SkillHit, isHeavy);
            }
        }

        public void ReportHeroHeal(HeroController hero, float amount)
        {
            if (hero == null || amount <= 0f)
            {
                return;
            }

            CreateDamageNumber(hero.Position + new Vector2(0f, 0.72f), $"+{amount:0}", GetDamageColor(DamageFeedbackType.Heal), false, false, false);
            MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.HeroLevelUp, hero.Position, 0.45f);
            FeelImpactSystem.Instance.Play(FeelImpactEvent.LevelUp, FeelImpactLevel.Light, hero.Position, GetDamageColor(DamageFeedbackType.Heal));
        }

        public void ReportHeroBubble(HeroController hero, BubbleTalkEvent eventType, bool highPriority)
        {
            TryShowBubble(hero, eventType, highPriority);
        }

        public void ReportSpecialText(Vector2 position, string text, DamageFeedbackType type, bool important)
        {
            CreateDamageNumber(position + new Vector2(Random.Range(-0.18f, 0.18f), 0.48f), text, GetDamageColor(type), important, false, important);
            PlayHitSfx(type, important, false);
            MusicManiacAudioSystem.Instance.Play(text == "MISS" ? MusicManiacAudioEvent.SkillMiss : MusicManiacAudioEvent.ProjectileHit, position, important ? 0.9f : 0.55f);
            if (important)
            {
                FeelImpactSystem.Instance.Play(FeelImpactEvent.CriticalHit, FeelImpactLevel.Light, position, GetDamageColor(type));
            }
        }

        private void CreateDamageNumber(Vector2 position, string text, Color color, bool isCritical, bool isDot, bool isKill)
        {
            var numberObject = new GameObject($"Damage Number - {text}");
            numberObject.transform.position = new Vector3(position.x, position.y, -0.78f);

            var shadow = new GameObject("Shadow");
            shadow.transform.SetParent(numberObject.transform, false);
            shadow.transform.localPosition = new Vector3(0.035f, -0.035f, 0.01f);
            var shadowMesh = shadow.AddComponent<TextMesh>();
            ConfigureTextMesh(shadowMesh, text, Color.black, isCritical, isDot, isKill);

            var mesh = numberObject.AddComponent<TextMesh>();
            ConfigureTextMesh(mesh, text, color, isCritical, isDot, isKill);

            var item = numberObject.AddComponent<DamageNumberItem>();
            item.Initialize(mesh, shadowMesh, color, isCritical, isDot, isKill);
        }

        private static void ConfigureTextMesh(TextMesh mesh, string text, Color color, bool isCritical, bool isDot, bool isKill)
        {
            mesh.text = text;
            mesh.characterSize = isKill ? 0.34f : isCritical ? 0.31f : isDot ? 0.17f : 0.23f;
            mesh.fontStyle = isCritical || isKill ? FontStyle.Bold : FontStyle.Normal;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
        }

        private void CreateHitVfx(Vector2 position, DamageFeedbackType type, bool strong, bool isAoe)
        {
            var color = GetDamageColor(type);
            var radius = isAoe ? 0.72f : strong ? 0.46f : 0.28f;
            VisualFactory.CreatePulseRing(position, radius, color, strong ? 0.26f : 0.18f);
            VisualFactory.CreateAnimatedSpriteBurst(position, type == DamageFeedbackType.Sonic ? "vfx/vfx_sonic_ring" : "vfx/vfx_hit_spark", radius * 1.8f, color, strong ? 0.32f : 0.24f, 18, 8, 20f);

            var sparkObject = new GameObject($"Hit Spark - {type}");
            sparkObject.transform.position = new Vector3(position.x, position.y, -0.62f);
            var spark = sparkObject.AddComponent<HitSpark>();
            spark.Initialize(color, strong ? 7 : 4, strong ? 0.42f : 0.24f);
        }

        private void PlayHitSfx(DamageFeedbackType type, bool strong, bool isDot)
        {
            if (audioSource == null)
            {
                return;
            }

            var cooldown = isDot ? 0.24f : 0.08f;
            if (sfxCooldowns.TryGetValue(type, out var remaining) && remaining > 0f && !strong)
            {
                return;
            }

            sfxCooldowns[type] = strong ? 0.14f : cooldown;
            var key = $"{type}_{strong}_{isDot}";
            if (!clipCache.TryGetValue(key, out var clip))
            {
                clip = CreateTone(type, strong, isDot);
                clipCache[key] = clip;
            }

            audioSource.pitch = Random.Range(0.94f, 1.08f);
            audioSource.PlayOneShot(clip, isDot ? 0.16f : strong ? 0.52f : 0.34f);
        }

        private static AudioClip CreateTone(DamageFeedbackType type, bool strong, bool isDot)
        {
            var sampleRate = 22050;
            var duration = isDot ? 0.07f : strong ? 0.16f : 0.1f;
            var samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            var frequency = GetToneFrequency(type);

            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var normalized = i / Mathf.Max(1f, samples - 1f);
                var envelope = Mathf.Sin(normalized * Mathf.PI);
                var noise = Random.Range(-0.22f, 0.22f) * (1f - normalized);
                var wave = Mathf.Sin(Mathf.PI * 2f * frequency * t) * 0.6f + Mathf.Sin(Mathf.PI * 2f * frequency * 1.5f * t) * 0.25f + noise;
                data[i] = wave * envelope * (strong ? 0.75f : 0.48f);
            }

            var clip = AudioClip.Create($"HitTone_{type}_{strong}_{isDot}", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void TryShowBubble(HeroController hero, BubbleTalkEvent eventType, bool highPriority)
        {
            if (hero == null || activeBubbleCount >= 2)
            {
                return;
            }

            if (!highPriority && bubbleGlobalCooldown > 0f)
            {
                return;
            }

            if (bubbleCooldowns.TryGetValue(eventType, out var remaining) && remaining > 0f && !highPriority)
            {
                return;
            }

            var line = ChooseBubbleLine(eventType);
            bubbleCooldowns[eventType] = highPriority ? 2.2f : 4f;
            bubbleGlobalCooldown = highPriority ? 1.1f : 3.4f;
            activeBubbleCount++;
            MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.BubbleVoice, hero.Position, highPriority ? 0.9f : 0.62f);

            var bubbleObject = new GameObject($"Bubble Talk - {eventType}");
            var offset = new Vector2(Random.Range(-0.85f, 0.85f), Random.Range(0.95f, 1.35f));
            bubbleObject.transform.SetParent(hero.transform, false);
            bubbleObject.transform.localPosition = new Vector3(offset.x, offset.y, -0.85f);
            var bubble = bubbleObject.AddComponent<BubbleTalkItem>();
            bubble.Initialize(line, GetBubbleColor(eventType), () => activeBubbleCount = Mathf.Max(0, activeBubbleCount - 1));
        }

        private string ChooseBubbleLine(BubbleTalkEvent eventType)
        {
            var lines = GetBubbleLines(eventType);
            for (var i = 0; i < 6; i++)
            {
                var candidate = lines[Random.Range(0, lines.Length)];
                if (!recentBubbleLines.Contains(candidate))
                {
                    recentBubbleLines.Add(candidate);
                    while (recentBubbleLines.Count > 5)
                    {
                        recentBubbleLines.RemoveAt(0);
                    }

                    return candidate;
                }
            }

            return lines[Random.Range(0, lines.Length)];
        }

        private static string[] GetBubbleLines(BubbleTalkEvent eventType)
        {
            switch (eventType)
            {
                case BubbleTalkEvent.DodgedSkill:
                    return new[] { "打空了吧？", "就这？", "下次瞄准点！", "我早看穿了！" };
                case BubbleTalkEvent.Controlled:
                    return new[] { "动不了了！", "别控我啊！", "这不公平！", "等我能动的！" };
                case BubbleTalkEvent.Slowed:
                    return new[] { "怎么这么慢？", "脚下什么东西？", "别拖我节奏！", "我跑不动了！" };
                case BubbleTalkEvent.AntiHeal:
                    return new[] { "我怎么回不上血？", "这招真脏！", "治疗被断了！", "我还没输！" };
                case BubbleTalkEvent.ShieldBreak:
                    return new[] { "我的盾裂了！", "盾呢？！", "有点东西……", "我还有血！" };
                case BubbleTalkEvent.LowHealth:
                    return new[] { "有点疼……", "别高兴太早！", "还没结束！", "我不会倒在这！" };
                case BubbleTalkEvent.LevelUp:
                    return new[] { "力量来了！", "节奏变快了！", "我开始认真了！", "别眨眼！" };
                case BubbleTalkEvent.Surrounded:
                    return new[] { "你就会堆怪？", "别挤我！", "我杀得完！", "来多少都一样！" };
                case BubbleTalkEvent.HeavyHit:
                    return new[] { "这也太阴了吧！", "你是不是盯着我打？", "差一点就躲掉了！", "我记住你了！" };
                default:
                    return new[] { "你又来这招？", "别老往我脚下扔！", "这招太烦了！", "别以为我躲不开！" };
            }
        }

        private static Color GetDamageColor(DamageFeedbackType type)
        {
            switch (type)
            {
                case DamageFeedbackType.Fire: return new Color(1f, 0.34f, 0.08f);
                case DamageFeedbackType.Ice: return new Color(0.58f, 0.9f, 1f);
                case DamageFeedbackType.Lightning: return new Color(0.45f, 0.68f, 1f);
                case DamageFeedbackType.Poison: return new Color(0.36f, 0.95f, 0.24f);
                case DamageFeedbackType.Sonic: return new Color(0.88f, 0.46f, 1f);
                case DamageFeedbackType.Shadow: return new Color(0.5f, 0.2f, 0.78f);
                case DamageFeedbackType.ShieldBreak: return new Color(0.22f, 0.86f, 1f);
                case DamageFeedbackType.Heal: return new Color(0.36f, 1f, 0.48f);
                default: return new Color(0.94f, 0.96f, 0.92f);
            }
        }

        private static Color GetBubbleColor(BubbleTalkEvent eventType)
        {
            switch (eventType)
            {
                case BubbleTalkEvent.LowHealth: return new Color(1f, 0.36f, 0.32f);
                case BubbleTalkEvent.ShieldBreak: return new Color(0.35f, 0.85f, 1f);
                case BubbleTalkEvent.Controlled: return new Color(0.8f, 0.48f, 1f);
                case BubbleTalkEvent.LevelUp: return new Color(1f, 0.78f, 0.26f);
                case BubbleTalkEvent.DodgedSkill: return new Color(1f, 0.88f, 0.42f);
                default: return new Color(0.96f, 0.96f, 0.9f);
            }
        }

        private static float GetToneFrequency(DamageFeedbackType type)
        {
            switch (type)
            {
                case DamageFeedbackType.Fire: return 170f;
                case DamageFeedbackType.Ice: return 520f;
                case DamageFeedbackType.Lightning: return 720f;
                case DamageFeedbackType.Poison: return 230f;
                case DamageFeedbackType.Sonic: return 110f;
                case DamageFeedbackType.ShieldBreak: return 680f;
                case DamageFeedbackType.Heal: return 440f;
                default: return 300f;
            }
        }

        private static float GetHitStop(DamageFeedbackType type, bool isDot, bool isCritical, bool isKill)
        {
            if (isDot)
            {
                return 0f;
            }

            if (isKill || isCritical)
            {
                return 0.08f;
            }

            return type == DamageFeedbackType.Physical ? 0.03f : 0.045f;
        }

        private static float GetKnockback(DamageFeedbackType type, bool isDot, bool isAoe, bool isCritical, bool isKill)
        {
            if (isDot || type == DamageFeedbackType.Poison || type == DamageFeedbackType.Ice)
            {
                return 0f;
            }

            if (type == DamageFeedbackType.Sonic)
            {
                return isAoe ? 0.75f : 0.4f;
            }

            if (type == DamageFeedbackType.Fire)
            {
                return isAoe ? 0.45f : 0.24f;
            }

            if (isKill || isCritical)
            {
                return 0.5f;
            }

            return type == DamageFeedbackType.Lightning ? 0.12f : 0.14f;
        }

        private static string BuildDamageLabel(float amount, DamageFeedbackType type, bool isDot, bool isCritical, bool isKill)
        {
            if (isKill)
            {
                return $"KILL {amount:0}";
            }

            if (isCritical)
            {
                return $"CRIT {amount:0}";
            }

            if (isDot)
            {
                return $"{ElementShort(type)} {amount:0}";
            }

            return amount < 1f ? amount.ToString("0.0") : amount.ToString("0");
        }

        private static string BuildHeroDamageLabel(float amount, DamageFeedbackType type, bool isShieldBreak, bool isHeavy)
        {
            if (isShieldBreak)
            {
                return $"BREAK {amount:0}";
            }

            if (isHeavy)
            {
                return $"HIT {amount:0}";
            }

            return $"{ElementShort(type)} {amount:0}";
        }

        private static string ElementShort(DamageFeedbackType type)
        {
            switch (type)
            {
                case DamageFeedbackType.Fire: return "FIRE";
                case DamageFeedbackType.Ice: return "ICE";
                case DamageFeedbackType.Lightning: return "ZAP";
                case DamageFeedbackType.Poison: return "TOX";
                case DamageFeedbackType.Sonic: return "WAVE";
                case DamageFeedbackType.ShieldBreak: return "SHIELD";
                default: return "DMG";
            }
        }

        private sealed class DamageNumberItem : MonoBehaviour
        {
            private TextMesh mesh;
            private TextMesh shadow;
            private Color color;
            private float lifetime;
            private float duration;
            private float peakScale;
            private float endScale;
            private float floatHeight;
            private float sway;
            private bool jitter;
            private Vector3 startPosition;

            public void Initialize(TextMesh numberMesh, TextMesh shadowMesh, Color baseColor, bool isCritical, bool isDot, bool isKill)
            {
                mesh = numberMesh;
                shadow = shadowMesh;
                color = baseColor;
                duration = isKill ? 1.45f : isCritical ? 1.25f : isDot ? 0.62f : 0.92f;
                peakScale = isKill ? 1.7f : isCritical ? 1.55f : isDot ? 1.05f : 1.15f;
                endScale = isDot ? 0.8f : 0.9f;
                floatHeight = isKill ? 1.2f : isCritical ? 0.95f : isDot ? 0.42f : 0.7f;
                sway = Random.Range(-0.25f, 0.25f);
                jitter = isCritical || isKill;
                startPosition = transform.position;
                transform.localScale = Vector3.zero;
            }

            private void Update()
            {
                lifetime += Time.deltaTime;
                var t = Mathf.Clamp01(lifetime / Mathf.Max(0.01f, duration));
                var pop = t < 0.12f ? Mathf.Lerp(0f, peakScale, t / 0.12f) : t < 0.24f ? Mathf.Lerp(peakScale, 1f, (t - 0.12f) / 0.12f) : Mathf.Lerp(1f, endScale, t);
                var lateral = Mathf.Sin(t * Mathf.PI * 2f) * sway;
                var jitterOffset = jitter ? new Vector3(Random.Range(-0.018f, 0.018f), Random.Range(-0.012f, 0.012f), 0f) : Vector3.zero;
                transform.localScale = Vector3.one * pop;
                transform.position = startPosition + new Vector3(lateral, Mathf.SmoothStep(0f, floatHeight, t), 0f) + jitterOffset;

                var fade = t < 0.58f ? 1f : Mathf.InverseLerp(1f, 0.58f, t);
                var faded = color;
                faded.a = fade;
                if (mesh != null)
                {
                    mesh.color = faded;
                }

                if (shadow != null)
                {
                    shadow.color = new Color(0f, 0f, 0f, fade * 0.72f);
                }

                if (lifetime >= duration)
                {
                    Destroy(gameObject);
                }
            }
        }

        private sealed class HitSpark : MonoBehaviour
        {
            private readonly List<LineRenderer> lines = new List<LineRenderer>();
            private Color color;
            private float lifetime;
            private float duration;
            private float radius;

            public void Initialize(Color sparkColor, int count, float targetRadius)
            {
                color = sparkColor;
                duration = 0.18f;
                radius = targetRadius;

                for (var i = 0; i < count; i++)
                {
                    var lineObject = new GameObject("Spark Line");
                    lineObject.transform.SetParent(transform, false);
                    var line = lineObject.AddComponent<LineRenderer>();
                    line.positionCount = 2;
                    line.startWidth = 0.025f;
                    line.endWidth = 0.005f;
                    line.material = VisualFactory.CreateMaterial(color);
                    lines.Add(line);
                }
            }

            private void Update()
            {
                lifetime += Time.deltaTime;
                var t = Mathf.Clamp01(lifetime / Mathf.Max(0.01f, duration));
                for (var i = 0; i < lines.Count; i++)
                {
                    var angle = Mathf.PI * 2f * i / Mathf.Max(1, lines.Count) + lifetime * 3f;
                    var inner = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius * 0.14f;
                    var outer = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * Mathf.Lerp(radius * 0.25f, radius, t);
                    lines[i].SetPosition(0, transform.position + inner);
                    lines[i].SetPosition(1, transform.position + outer);
                    var faded = color;
                    faded.a = 1f - t;
                    lines[i].material.color = faded;
                }

                if (lifetime >= duration)
                {
                    Destroy(gameObject);
                }
            }
        }

        private sealed class BubbleTalkItem : MonoBehaviour
        {
            private TextMesh textMesh;
            private MeshRenderer backgroundRenderer;
            private System.Action onComplete;
            private Color borderColor;
            private float lifetime;
            private float duration = 2.15f;
            private Vector3 startScale;
            private Vector3 startLocalPosition;

            public void Initialize(string text, Color color, System.Action complete)
            {
                borderColor = color;
                onComplete = complete;

                var back = GameObject.CreatePrimitive(PrimitiveType.Quad);
                back.name = "Bubble Back";
                back.transform.SetParent(transform, false);
                back.transform.localPosition = new Vector3(0f, 0f, 0.04f);
                back.transform.localScale = new Vector3(Mathf.Clamp(1.15f + text.Length * 0.055f, 1.35f, 2.25f), 0.48f, 1f);
                Object.Destroy(back.GetComponent<Collider>());
                backgroundRenderer = back.GetComponent<MeshRenderer>();
                backgroundRenderer.material = VisualFactory.CreateMaterial(new Color(0.98f, 0.98f, 0.92f, 0.92f));

                var textObject = new GameObject("Bubble Text");
                textObject.transform.SetParent(transform, false);
                textObject.transform.localPosition = new Vector3(0f, -0.02f, -0.02f);
                textMesh = textObject.AddComponent<TextMesh>();
                textMesh.text = text;
                textMesh.characterSize = 0.16f;
                textMesh.fontStyle = FontStyle.Bold;
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.alignment = TextAlignment.Center;
                textMesh.color = Color.Lerp(Color.black, borderColor, 0.35f);

                var pointer = GameObject.CreatePrimitive(PrimitiveType.Quad);
                pointer.name = "Bubble Pointer";
                pointer.transform.SetParent(transform, false);
                pointer.transform.localPosition = new Vector3(0f, -0.31f, 0.03f);
                pointer.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                pointer.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
                Object.Destroy(pointer.GetComponent<Collider>());
                pointer.GetComponent<MeshRenderer>().material = VisualFactory.CreateMaterial(new Color(0.98f, 0.98f, 0.92f, 0.92f));

                startScale = Vector3.one;
                startLocalPosition = transform.localPosition;
                transform.localScale = Vector3.zero;
            }

            private void Update()
            {
                lifetime += Time.deltaTime;
                var t = Mathf.Clamp01(lifetime / duration);
                var pop = t < 0.12f ? Mathf.Lerp(0f, 1.12f, t / 0.12f) : t < 0.22f ? Mathf.Lerp(1.12f, 1f, (t - 0.12f) / 0.1f) : Mathf.Lerp(1f, 0.92f, t);
                transform.localScale = startScale * pop;
                transform.localPosition = startLocalPosition + Vector3.up * (0.12f * t);

                var fade = t < 0.72f ? 1f : Mathf.InverseLerp(1f, 0.72f, t);
                if (textMesh != null)
                {
                    var textColor = textMesh.color;
                    textColor.a = fade;
                    textMesh.color = textColor;
                }

                if (backgroundRenderer != null)
                {
                    var backColor = backgroundRenderer.material.color;
                    backColor.a = fade * 0.92f;
                    backgroundRenderer.material.color = Color.Lerp(backColor, borderColor, 0.08f * Mathf.Sin(Time.time * 8f));
                }

                if (lifetime >= duration)
                {
                    onComplete?.Invoke();
                    Destroy(gameObject);
                }
            }
        }
    }
}
