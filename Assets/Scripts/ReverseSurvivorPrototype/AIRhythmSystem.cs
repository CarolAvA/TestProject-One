using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public enum RhythmPitch
    {
        Low,
        Mid,
        High
    }

    public enum RhythmAttackId
    {
        SpiritTriple,
        SparkBarrage,
        MoonRing,
        ThunderNeedle,
        FallingMeteor
    }

    public sealed class AIRhythmController : MonoBehaviour
    {
        private readonly List<RhythmAttackData> attacks = new List<RhythmAttackData>();
        private readonly Dictionary<string, AudioClip> toneCache = new Dictionary<string, AudioClip>();

        private HeroController hero;
        private AudioSource audioSource;
        private RhythmAttackData currentAttack;
        private int beatIndex;
        private float attackTime;
        private float endLagTime;
        private float chooserTimer;
        private bool hasAttack;

        public string AttackName => hasAttack ? currentAttack.Name : "Reading battlefield";
        public string AttackType => hasAttack && hero != null ? $"{currentAttack.AttackType} / {hero.BuildData.BuildName}" : "Idle";
        public string BuildTags => hero != null ? hero.BuildData.ElementTags : "Basic";
        public int DangerLevel => hasAttack ? currentAttack.DangerLevel : 0;
        public float Progress01 => hasAttack && GetScaledDuration(currentAttack) > 0f ? Mathf.Clamp01(attackTime / GetScaledDuration(currentAttack)) : 0f;
        public float AttackTime => attackTime;
        public float AttackDuration => hasAttack ? GetScaledDuration(currentAttack) : 0f;
        public float EndLagRemaining => endLagTime;
        public bool IsInEndLag => endLagTime > 0f;
        public bool IsAttacking => hasAttack;

        public void Initialize(HeroController owner)
        {
            hero = owner;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.28f;
            BuildAttacks();
            ChooseNextAttack();
        }

        private void Update()
        {
            if (hero == null || hero.Health <= 0f)
            {
                return;
            }

            if (endLagTime > 0f)
            {
                endLagTime -= Time.deltaTime;
                return;
            }

            if (!hasAttack)
            {
                chooserTimer -= Time.deltaTime;
                if (chooserTimer <= 0f)
                {
                    ChooseNextAttack();
                }

                return;
            }

            attackTime += Time.deltaTime;
            while (beatIndex < currentAttack.Beats.Length && GetScaledBeatTime(currentAttack.Beats[beatIndex]) <= attackTime)
            {
                TriggerBeat(currentAttack.Beats[beatIndex]);
                if (hero.BuildData.ShouldAddSyncopation(currentAttack.Beats[beatIndex]))
                {
                    Invoke(nameof(TriggerSyncopationBeat), 0.14f);
                }

                beatIndex++;
            }

            if (attackTime >= GetScaledDuration(currentAttack))
            {
                endLagTime = currentAttack.EndLag;
                hasAttack = false;
                chooserTimer = Mathf.Max(0.18f, currentAttack.EndLag);
            }
        }

        public string BuildTimelineText()
        {
            if (!hasAttack)
            {
                return IsInEndLag ? $"End lag window {endLagTime:0.0}s" : "AI rhythm preparing...";
            }

            return $"{BuildTrack(RhythmPitch.Low)}\n{BuildTrack(RhythmPitch.Mid)}\n{BuildTrack(RhythmPitch.High)}";
        }

        public string GetNextBeatText()
        {
            if (!hasAttack)
            {
                return IsInEndLag ? "Counter window" : "Next attack soon";
            }

            if (beatIndex >= currentAttack.Beats.Length)
            {
                return "No beats left";
            }

            var beat = currentAttack.Beats[beatIndex];
            return $"{beat.Pitch} beat in {Mathf.Max(0f, GetScaledBeatTime(beat) - attackTime):0.0}s";
        }

        private string BuildTrack(RhythmPitch pitch)
        {
            const int cells = 24;
            var builder = new StringBuilder();
            builder.Append(pitch == RhythmPitch.Low ? "Low  " : pitch == RhythmPitch.Mid ? "Mid  " : "High ");

            for (var i = 0; i < cells; i++)
            {
                var scaledDuration = GetScaledDuration(currentAttack);
                var cellTime = scaledDuration * i / Mathf.Max(1, cells - 1);
                var cursorTime = scaledDuration * Progress01;
                var marker = Mathf.Abs(cellTime - cursorTime) < scaledDuration / cells * 0.5f ? "|" : "-";

                foreach (var beat in currentAttack.Beats)
                {
                    if (beat.Pitch == pitch && Mathf.Abs(GetScaledBeatTime(beat) - cellTime) < scaledDuration / cells * 0.55f)
                    {
                        marker = beat.Strength >= 1.8f ? "O" : "o";
                        break;
                    }
                }

                builder.Append(marker);
            }

            return builder.ToString();
        }

        private void TriggerBeat(RhythmBeatEvent beat)
        {
            var scaledBeat = hero.BuildData.ScaleBeat(beat);
            if (hero.BuildData.HeavyBeatUnlocked && beatIndex % 4 == 3)
            {
                scaledBeat = scaledBeat.WithCombat(scaledBeat.SpawnCount, scaledBeat.Speed * 0.92f, scaledBeat.Damage * 1.45f).WithPitch(RhythmPitch.Low, scaledBeat.Strength + 0.8f);
            }

            PlayBeatTone(scaledBeat);
            MusicManiacAudioSystem.Instance.PlayRhythm(scaledBeat.Pitch, scaledBeat.Strength);

            if (scaledBeat.Pattern == RhythmBulletPattern.Meteor)
            {
                SpawnMeteorBeat(scaledBeat);
                return;
            }

            if (scaledBeat.Pattern == RhythmBulletPattern.Ring)
            {
                SpawnRingBeat(scaledBeat);
                return;
            }

            if (scaledBeat.Pattern == RhythmBulletPattern.Spread)
            {
                SpawnSpreadBeat(scaledBeat);
                return;
            }

            SpawnTargetedBeat(scaledBeat);

            if (hero.BuildData.SonicBurstUnlocked && scaledBeat.Pitch == RhythmPitch.Low)
            {
                SpawnSonicBurst(scaledBeat);
            }

            if (hero.BuildData.EchoBeatUnlocked)
            {
                StartCoroutine(DelayedEcho(scaledBeat, 0.38f));
            }
        }

        private void SpawnTargetedBeat(RhythmBeatEvent beat)
        {
            var target = FindBestTarget();
            if (target == null)
            {
                return;
            }

            var payload = hero.BuildData.CreatePayload(beat, 0);
            MusicManiacAudioSystem.Instance.PlayProjectile(payload.Element, "fire", hero.Position, beat.Strength);
            Projectile.Create(hero.Position, target, beat.Damage, payload.Color, beat.Speed, payload);
        }

        private void SpawnSpreadBeat(RhythmBeatEvent beat)
        {
            var target = FindBestTarget();
            var baseDirection = target == null ? Vector2.right : (target.Position - hero.Position).normalized;
            var baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
            var count = Mathf.Max(1, beat.SpawnCount);

            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0.5f : i / (float)(count - 1);
                var angle = baseAngle + Mathf.Lerp(-beat.SpreadAngle * 0.5f, beat.SpreadAngle * 0.5f, t);
                var direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                var payload = hero.BuildData.CreatePayload(beat, i);
                MusicManiacAudioSystem.Instance.PlayProjectile(payload.Element, "fire", hero.Position, Mathf.Clamp01(0.55f + beat.Strength * 0.2f));
                Projectile.CreateDirectional(hero.Position, direction, beat.Damage, payload.Color, beat.Speed, 2.2f, payload);
            }
        }

        private void SpawnRingBeat(RhythmBeatEvent beat)
        {
            var count = Mathf.Max(8, beat.SpawnCount);
            for (var i = 0; i < count; i++)
            {
                var angle = 360f * i / count;
                var direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                var payload = hero.BuildData.CreatePayload(beat, i);
                if (i % 3 == 0)
                {
                    MusicManiacAudioSystem.Instance.PlayProjectile(payload.Element, "fire", hero.Position, 0.55f);
                }

                Projectile.CreateDirectional(hero.Position, direction, beat.Damage, payload.Color, beat.Speed, 2.6f, payload);
            }

            VisualFactory.CreatePulseRing(hero.Position, 1.5f + beat.Strength * 0.35f, BeatColor(beat.Pitch), 0.28f);
        }

        private void SpawnMeteorBeat(RhythmBeatEvent beat)
        {
            var center = hero.Position;
            VisualFactory.CreatePulseRing(center, 2.2f, new Color(1f, 0.15f, 0.08f), 0.38f);
            MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.SonicRelease, center, 1f);

            foreach (var monster in GameDirector.Instance.Monsters)
            {
                if (monster != null && Vector2.Distance(monster.Position, center) < 2.2f)
                {
                    monster.TakeDamage(beat.Damage, DamageFeedbackType.Fire, center, false, true, beat.Strength >= 1.6f);
                }
            }
        }

        private void SpawnSonicBurst(RhythmBeatEvent beat)
        {
            var radius = 1.75f * hero.BuildData.AoeRadiusMultiplier;
            VisualFactory.CreatePulseRing(hero.Position, radius, new Color(0.9f, 0.82f, 0.45f), 0.32f);
            MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.SonicRelease, hero.Position, 0.85f);
            foreach (var monster in GameDirector.Instance.Monsters)
            {
                if (monster != null && Vector2.Distance(monster.Position, hero.Position) <= radius)
                {
                    monster.TakeDamage(beat.Damage * 0.45f, DamageFeedbackType.Sonic, hero.Position, false, true, beat.Strength >= 1.5f);
                }
            }
        }

        private System.Collections.IEnumerator DelayedEcho(RhythmBeatEvent beat, float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnSonicBurst(beat.WithCombat(1, beat.Speed, beat.Damage * 0.22f).WithPitch(RhythmPitch.Mid, 0.6f));
        }

        private void TriggerSyncopationBeat()
        {
            var beat = new RhythmBeatEvent(attackTime, RhythmPitch.High, 0.65f, RhythmBulletPattern.Targeted, 1, 0f, 9.5f, 11f);
            var scaledBeat = hero.BuildData.ScaleBeat(beat);
            SpawnTargetedBeat(scaledBeat);
            PlayBeatTone(beat);
        }

        private float GetScaledDuration(RhythmAttackData attack)
        {
            return attack.Duration * hero.BuildData.GetDurationMultiplier();
        }

        private float GetScaledBeatTime(RhythmBeatEvent beat)
        {
            return beat.TimePoint * hero.BuildData.GetDurationMultiplier();
        }

        private MonsterUnit FindBestTarget()
        {
            MonsterUnit best = null;
            var bestScore = float.MinValue;

            foreach (var monster in GameDirector.Instance.Monsters)
            {
                if (monster == null)
                {
                    continue;
                }

                var distance = Vector2.Distance(hero.Position, monster.Position);
                if (distance > 7.2f)
                {
                    continue;
                }

                var score = 10f - distance;
                if (monster.Config.Kind == MonsterKind.Archer || monster.Config.Kind == MonsterKind.HexPriest || monster.Config.Kind == MonsterKind.Shieldbreaker)
                {
                    score += 2.5f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = monster;
                }
            }

            return best;
        }

        private void ChooseNextAttack()
        {
            var monsterCount = GameDirector.Instance.Monsters.Count;
            var choice = 0;

            if (monsterCount >= 10)
            {
                choice = Random.value < 0.55f ? 1 : 2;
            }
            else if (monsterCount >= 5)
            {
                choice = Random.value < 0.45f ? 3 : 0;
            }
            else if (GameDirector.Instance.Threat > 0.62f && Random.value < 0.35f)
            {
                choice = 4;
            }
            else
            {
                choice = Random.Range(0, 4);
            }

            currentAttack = attacks[Mathf.Clamp(choice, 0, attacks.Count - 1)];
            attackTime = 0f;
            beatIndex = 0;
            hasAttack = true;
        }

        private void BuildAttacks()
        {
            attacks.Add(new RhythmAttackData(
                RhythmAttackId.SpiritTriple,
                "Spirit Triple",
                "Mid line shots",
                1.2f,
                1,
                0.35f,
                new[]
                {
                    new RhythmBeatEvent(0.2f, RhythmPitch.Mid, 1f, RhythmBulletPattern.Targeted, 1, 0f, 9f, 24f),
                    new RhythmBeatEvent(0.6f, RhythmPitch.Mid, 1f, RhythmBulletPattern.Targeted, 1, 0f, 9f, 24f),
                    new RhythmBeatEvent(1.0f, RhythmPitch.Mid, 1f, RhythmBulletPattern.Targeted, 1, 0f, 9f, 26f)
                }));

            attacks.Add(new RhythmAttackData(
                RhythmAttackId.SparkBarrage,
                "Spark Barrage",
                "High-frequency clear",
                2.5f,
                2,
                0.55f,
                new[]
                {
                    new RhythmBeatEvent(0.15f, RhythmPitch.High, 0.75f, RhythmBulletPattern.Targeted, 1, 0f, 10f, 13f),
                    new RhythmBeatEvent(0.4f, RhythmPitch.High, 0.75f, RhythmBulletPattern.Targeted, 1, 0f, 10f, 13f),
                    new RhythmBeatEvent(0.65f, RhythmPitch.High, 0.75f, RhythmBulletPattern.Targeted, 1, 0f, 10f, 13f),
                    new RhythmBeatEvent(0.9f, RhythmPitch.High, 0.75f, RhythmBulletPattern.Targeted, 1, 0f, 10f, 13f),
                    new RhythmBeatEvent(1.15f, RhythmPitch.High, 0.75f, RhythmBulletPattern.Targeted, 1, 0f, 10f, 13f),
                    new RhythmBeatEvent(1.4f, RhythmPitch.High, 0.75f, RhythmBulletPattern.Targeted, 1, 0f, 10f, 13f),
                    new RhythmBeatEvent(1.65f, RhythmPitch.High, 0.75f, RhythmBulletPattern.Targeted, 1, 0f, 10f, 13f),
                    new RhythmBeatEvent(1.9f, RhythmPitch.High, 0.75f, RhythmBulletPattern.Targeted, 1, 0f, 10f, 13f),
                    new RhythmBeatEvent(2.15f, RhythmPitch.Mid, 1.1f, RhythmBulletPattern.Spread, 3, 28f, 8.5f, 18f)
                }));

            attacks.Add(new RhythmAttackData(
                RhythmAttackId.MoonRing,
                "Moon Ring",
                "Low ring pulses",
                3f,
                3,
                0.75f,
                new[]
                {
                    new RhythmBeatEvent(0.8f, RhythmPitch.Low, 1.8f, RhythmBulletPattern.Ring, 14, 360f, 4.8f, 20f),
                    new RhythmBeatEvent(1.7f, RhythmPitch.Low, 1.8f, RhythmBulletPattern.Ring, 16, 360f, 5.2f, 22f),
                    new RhythmBeatEvent(2.6f, RhythmPitch.Low, 2f, RhythmBulletPattern.Ring, 18, 360f, 5.6f, 24f)
                }));

            attacks.Add(new RhythmAttackData(
                RhythmAttackId.ThunderNeedle,
                "Thunder Needle",
                "High scatter",
                3.2f,
                2,
                0.65f,
                new[]
                {
                    new RhythmBeatEvent(0.2f, RhythmPitch.High, 0.8f, RhythmBulletPattern.Spread, 5, 80f, 8f, 11f),
                    new RhythmBeatEvent(0.55f, RhythmPitch.High, 0.8f, RhythmBulletPattern.Spread, 4, 65f, 8f, 11f),
                    new RhythmBeatEvent(0.9f, RhythmPitch.High, 0.9f, RhythmBulletPattern.Spread, 6, 95f, 8.5f, 12f),
                    new RhythmBeatEvent(1.4f, RhythmPitch.Mid, 1.1f, RhythmBulletPattern.Spread, 5, 70f, 8.5f, 16f),
                    new RhythmBeatEvent(1.95f, RhythmPitch.High, 0.9f, RhythmBulletPattern.Spread, 6, 100f, 8.5f, 12f),
                    new RhythmBeatEvent(2.5f, RhythmPitch.High, 1f, RhythmBulletPattern.Spread, 7, 110f, 9f, 13f)
                }));

            attacks.Add(new RhythmAttackData(
                RhythmAttackId.FallingMeteor,
                "Falling Meteor",
                "Low finisher",
                5f,
                4,
                1f,
                new[]
                {
                    new RhythmBeatEvent(0.6f, RhythmPitch.Low, 1.2f, RhythmBulletPattern.Ring, 10, 360f, 4.3f, 18f),
                    new RhythmBeatEvent(1.7f, RhythmPitch.Low, 1.5f, RhythmBulletPattern.Ring, 12, 360f, 4.8f, 20f),
                    new RhythmBeatEvent(3.0f, RhythmPitch.Low, 1.8f, RhythmBulletPattern.Ring, 16, 360f, 5.2f, 23f),
                    new RhythmBeatEvent(4.4f, RhythmPitch.Low, 2.4f, RhythmBulletPattern.Meteor, 1, 0f, 0f, 80f)
                }));
        }

        private void PlayBeatTone(RhythmBeatEvent beat)
        {
            if (audioSource == null)
            {
                return;
            }

            var frequency = beat.Pitch == RhythmPitch.Low ? 110f : beat.Pitch == RhythmPitch.Mid ? 330f : 880f;
            var duration = beat.Pitch == RhythmPitch.Low ? 0.18f : 0.075f;
            var key = $"{frequency:0}-{duration:0.000}-{beat.Strength:0.0}";
            if (!toneCache.TryGetValue(key, out var clip))
            {
                clip = CreateToneClip(frequency, duration, beat.Strength);
                toneCache[key] = clip;
            }

            audioSource.PlayOneShot(clip, Mathf.Clamp01(0.32f + beat.Strength * 0.12f));
        }

        private static AudioClip CreateToneClip(float frequency, float duration, float strength)
        {
            const int sampleRate = 22050;
            var samples = Mathf.Max(64, Mathf.CeilToInt(sampleRate * duration));
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = 1f - i / (float)samples;
                var wave = Mathf.Sin(Mathf.PI * 2f * frequency * t);
                data[i] = wave * envelope * Mathf.Clamp01(0.18f + strength * 0.08f);
            }

            var clip = AudioClip.Create("Rhythm Beat", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static Color BeatColor(RhythmPitch pitch)
        {
            switch (pitch)
            {
                case RhythmPitch.Low:
                    return new Color(1f, 0.2f, 0.08f);
                case RhythmPitch.Mid:
                    return new Color(0.7f, 0.86f, 1f);
                case RhythmPitch.High:
                    return new Color(1f, 0.92f, 0.25f);
                default:
                    return Color.white;
            }
        }
    }

    public enum RhythmBulletPattern
    {
        Targeted,
        Spread,
        Ring,
        Meteor
    }

    public readonly struct RhythmAttackData
    {
        public RhythmAttackData(RhythmAttackId id, string name, string attackType, float duration, int dangerLevel, float endLag, RhythmBeatEvent[] beats)
        {
            Id = id;
            Name = name;
            AttackType = attackType;
            Duration = duration;
            DangerLevel = dangerLevel;
            EndLag = endLag;
            Beats = beats;
        }

        public RhythmAttackId Id { get; }
        public string Name { get; }
        public string AttackType { get; }
        public float Duration { get; }
        public int DangerLevel { get; }
        public float EndLag { get; }
        public RhythmBeatEvent[] Beats { get; }
    }

    public readonly struct RhythmBeatEvent
    {
        public RhythmBeatEvent(float timePoint, RhythmPitch pitch, float strength, RhythmBulletPattern pattern, int spawnCount, float spreadAngle, float speed, float damage)
        {
            TimePoint = timePoint;
            Pitch = pitch;
            Strength = strength;
            Pattern = pattern;
            SpawnCount = spawnCount;
            SpreadAngle = spreadAngle;
            Speed = speed;
            Damage = damage;
        }

        public float TimePoint { get; }
        public RhythmPitch Pitch { get; }
        public float Strength { get; }
        public RhythmBulletPattern Pattern { get; }
        public int SpawnCount { get; }
        public float SpreadAngle { get; }
        public float Speed { get; }
        public float Damage { get; }

        public RhythmBeatEvent WithCombat(int spawnCount, float speed, float damage)
        {
            return new RhythmBeatEvent(TimePoint, Pitch, Strength, Pattern, spawnCount, SpreadAngle, speed, damage);
        }

        public RhythmBeatEvent WithPitch(RhythmPitch pitch, float strength)
        {
            return new RhythmBeatEvent(TimePoint, pitch, strength, Pattern, SpawnCount, SpreadAngle, Speed, Damage);
        }
    }
}
