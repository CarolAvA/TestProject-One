using System.Collections.Generic;
using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public sealed class HeroController : MonoBehaviour
    {
        private const float MaxHealthBase = 720f;

        private readonly List<string> cards = new List<string>();
        private readonly AIHeroBuildData buildData = new AIHeroBuildData();

        private float maxHealth;
        private float health;
        private float shield;
        private float experience;
        private float nextLevelExperience = 45f;
        private float auraTimer;
        private float shieldTimer;
        private float buildTimer;
        private float antiHealTimer;
        private float shieldBreakTimer;
        private float skillSlowTimer;
        private float skillSlowMultiplier = 1f;
        private float poisonTimer;
        private float poisonDamagePerSecond;
        private float hitStunTimer;
        private float hitFlashTimer;
        private float hitFlashDuration;
        private Color hitFlashColor;
        private Vector2 velocity;
        private HeroBuild build;
        private HeroBuild nextBuild;
        private MeshRenderer meshRenderer;
        private SpriteRenderer spriteRenderer;
        private Vector3 spriteBaseScale = Vector3.one;
        private UnitSpriteAnimator spriteAnimator;
        private AIRhythmController rhythmController;

        public Vector2 Position => transform.position;
        public float Health => health;
        public float MaxHealth => maxHealth;
        public float Health01 => maxHealth <= 0f ? 0f : health / maxHealth;
        public float Shield => shield;
        public float AntiHealTime => antiHealTimer;
        public float ShieldBreakTime => shieldBreakTimer;
        public float SkillSlowTime => skillSlowTimer;
        public int Level { get; private set; } = 1;
        public HeroBuild Build => build;
        public IReadOnlyList<string> Cards => cards;
        public AIRhythmController Rhythm => rhythmController;
        public AIHeroBuildData BuildData => buildData;

        public void PlayDeathAnimation()
        {
            if (spriteAnimator == null)
            {
                return;
            }

            spriteAnimator.UseUnscaledTime = true;
            spriteAnimator.Play(UnitAnimationAction.Death, true);
        }

        public void Initialize(HeroBuild startingBuild)
        {
            maxHealth = MaxHealthBase;
            health = maxHealth;
            build = startingBuild;
            nextBuild = startingBuild;
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteBaseScale = spriteRenderer.transform.localScale;
                spriteAnimator = UnitSpriteAnimator.AttachHero(gameObject, spriteRenderer);
            }

            rhythmController = gameObject.AddComponent<AIRhythmController>();
            rhythmController.Initialize(this);
            ApplyBuildCard(build);
        }

        private void Update()
        {
            var director = GameDirector.Instance;
            TickStatusEffects();
            if (hitStunTimer > 0f)
            {
                hitStunTimer -= Time.deltaTime;
                velocity *= 0.35f;
                if (spriteAnimator != null)
                {
                    spriteAnimator.Play(UnitAnimationAction.Hit);
                }

                UpdateVisuals();
                return;
            }

            ChooseMovement(director);
            PickUpExperience(director);
            ApplyBuildPowers(director);
            ClampToArena();
            UpdateVisuals();
        }

        public void OverrideNextBuild(HeroBuild forcedBuild)
        {
            nextBuild = forcedBuild;
        }

        public void TakeDamage(float amount, MonsterKind sourceKind, bool antiHeal = false, bool shieldbreaker = false)
        {
            if (antiHeal)
            {
                antiHealTimer = Mathf.Max(antiHealTimer, 4f);
            }

            if (sourceKind == MonsterKind.VenomBug)
            {
                poisonTimer = Mathf.Max(poisonTimer, 4f);
                var config = GameDirector.Instance.GetMonsterConfig(sourceKind);
                poisonDamagePerSecond = Mathf.Max(poisonDamagePerSecond, config.PoisonDamage);
            }

            var adjusted = amount;
            if (build == HeroBuild.ShieldWall && !shieldbreaker)
            {
                adjusted *= 0.82f;
            }
            else if (shieldbreaker)
            {
                adjusted *= 1.35f;
            }

            if (shield > 0f)
            {
                var shieldDamage = Mathf.Min(shield, adjusted);
                shield -= shieldDamage;
                adjusted -= shieldDamage;
            }

            if (adjusted > 0f)
            {
                health = Mathf.Max(0f, health - adjusted);
                GameDirector.Instance.AwardDamage(adjusted);
            }

            var feedbackType = shieldbreaker
                ? DamageFeedbackType.ShieldBreak
                : sourceKind == MonsterKind.VenomBug
                    ? DamageFeedbackType.Poison
                    : sourceKind == MonsterKind.HexPriest
                        ? DamageFeedbackType.Shadow
                        : DamageFeedbackType.Physical;
            DamageFeedbackSystem.Instance.ReportHeroDamage(this, Mathf.Max(0f, amount), feedbackType, Position + Random.insideUnitCircle.normalized, false, sourceKind == MonsterKind.VenomBug, shieldbreaker, Health01 < 0.35f, sourceKind == MonsterKind.BoneKing || sourceKind == MonsterKind.Assassin);
        }

        public void TakeSkillDamage(float amount, bool antiHeal, bool shieldbreaker)
        {
            if (antiHeal)
            {
                ApplyAntiHeal(4f);
            }

            if (shieldbreaker)
            {
                ApplyShieldBreak(4f);
            }

            var adjusted = amount;
            if (build == HeroBuild.ShieldWall && !shieldbreaker && shieldBreakTimer <= 0f)
            {
                adjusted *= 0.82f;
            }

            if (shieldbreaker || shieldBreakTimer > 0f)
            {
                adjusted *= 1.45f;
            }

            if (shield > 0f)
            {
                var shieldDamage = Mathf.Min(shield, adjusted);
                shield -= shieldDamage;
                adjusted -= shieldDamage;
            }

            if (adjusted > 0f)
            {
                health = Mathf.Max(0f, health - adjusted);
                GameDirector.Instance.AwardDamage(adjusted);
            }
        }

        public void ApplySkillSlow(float multiplier, float duration)
        {
            skillSlowMultiplier = Mathf.Min(skillSlowMultiplier, multiplier);
            skillSlowTimer = Mathf.Max(skillSlowTimer, duration);
        }

        public void ApplyAntiHeal(float duration)
        {
            antiHealTimer = Mathf.Max(antiHealTimer, duration);
        }

        public void ApplyShieldBreak(float duration)
        {
            shieldBreakTimer = Mathf.Max(shieldBreakTimer, duration);
        }

        private void TickStatusEffects()
        {
            antiHealTimer = Mathf.Max(0f, antiHealTimer - Time.deltaTime);
            shieldBreakTimer = Mathf.Max(0f, shieldBreakTimer - Time.deltaTime);

            if (skillSlowTimer > 0f)
            {
                skillSlowTimer -= Time.deltaTime;
            }
            else
            {
                skillSlowMultiplier = 1f;
            }

            if (poisonTimer > 0f)
            {
                poisonTimer -= Time.deltaTime;
                TakeRawDamage(poisonDamagePerSecond * Time.deltaTime, DamageFeedbackType.Poison);
            }
            else
            {
                poisonDamagePerSecond = 0f;
            }
        }

        private void TakeRawDamage(float amount)
        {
            TakeRawDamage(amount, DamageFeedbackType.Physical);
        }

        private void TakeRawDamage(float amount, DamageFeedbackType type)
        {
            health = Mathf.Max(0f, health - amount);
            GameDirector.Instance.AwardDamage(amount);
            DamageFeedbackSystem.Instance.ReportHeroDamage(this, amount, type, Position, false, false, false, Health01 < 0.35f, false);
        }

        private void ChooseMovement(GameDirector director)
        {
            var danger = Vector2.zero;
            var nearestOrb = default(ExperienceOrb);
            var nearestOrbDistance = float.MaxValue;

            foreach (var monster in director.Monsters)
            {
                if (monster == null)
                {
                    continue;
                }

                var offset = Position - monster.Position;
                var distance = Mathf.Max(0.2f, offset.magnitude);
                var isHeavy = monster.Config.Kind == MonsterKind.Stoneguard || monster.Config.Kind == MonsterKind.BoneKing;
                var pressure = isHeavy ? 1.6f : 1f;
                danger += offset.normalized * pressure / distance;
            }

            foreach (var hazard in director.Hazards)
            {
                var offset = Position - hazard.Position;
                var distance = offset.magnitude;
                if (distance < hazard.Radius + 1.1f)
                {
                    danger += offset.normalized * 2.8f;
                }
            }

            foreach (var warning in director.SkillWarnings)
            {
                if (warning == null)
                {
                    continue;
                }

                var offset = Position - warning.Position;
                var distance = offset.magnitude;
                if (distance < warning.Radius + 1.5f)
                {
                    var safeDistance = Mathf.Max(0.15f, warning.Radius + 1.5f - distance);
                    danger += offset.normalized * safeDistance * (2.2f + warning.Danger * 4f);
                }
            }

            foreach (var wall in director.TemporaryWalls)
            {
                if (wall != null && wall.IsNear(Position, 0.85f))
                {
                    danger += wall.EscapeDirection(Position) * 2.5f;
                }
            }

            foreach (var orb in director.ExperienceOrbs)
            {
                if (orb == null)
                {
                    continue;
                }

                var distance = Vector2.Distance(Position, orb.Position);
                if (distance < nearestOrbDistance)
                {
                    nearestOrbDistance = distance;
                    nearestOrb = orb;
                }
            }

            var desire = danger;
            if (nearestOrb != null && nearestOrbDistance < 6f && danger.magnitude < 2.4f)
            {
                desire += (nearestOrb.Position - Position).normalized * 0.9f;
            }

            if (desire.sqrMagnitude < 0.05f)
            {
                desire = new Vector2(Mathf.Sin(Time.time * 0.8f), Mathf.Cos(Time.time * 0.6f));
            }

            var speedMultiplier = 1f;
            foreach (var hazard in director.Hazards)
            {
                if (Vector2.Distance(Position, hazard.Position) < hazard.Radius)
                {
                    speedMultiplier = Mathf.Min(speedMultiplier, hazard.SpeedMultiplier);
                    TakeRawDamage(hazard.DamagePerSecond * Time.deltaTime, DamageFeedbackType.Poison);
                }
            }

            var speed = (2.15f + Level * 0.035f) * speedMultiplier * skillSlowMultiplier;
            velocity = Vector2.Lerp(velocity, desire.normalized * speed, Time.deltaTime * 5f);
            transform.position += (Vector3)(velocity * Time.deltaTime);

            foreach (var wall in director.TemporaryWalls)
            {
                if (wall != null && wall.IsNear(Position, 0.2f))
                {
                    transform.position += (Vector3)(wall.EscapeDirection(Position) * speed * Time.deltaTime * 1.8f);
                }
            }
        }

        private void PickUpExperience(GameDirector director)
        {
            for (var i = director.ExperienceOrbs.Count - 1; i >= 0; i--)
            {
                var orb = director.ExperienceOrbs[i];
                if (orb == null)
                {
                    continue;
                }

                var distance = Vector2.Distance(Position, orb.Position);
                if (distance < 2.2f)
                {
                    orb.MoveToward(Position, 8f * Time.deltaTime);
                }

                if (distance < 0.32f)
                {
                    experience += orb.Value;
                    director.UnregisterExperience(orb);
                    Destroy(orb.gameObject);
                    TryLevelUp();
                }
            }
        }

        private void TryLevelUp()
        {
            while (experience >= nextLevelExperience)
            {
                experience -= nextLevelExperience;
                nextLevelExperience += 24f + Level * 8f;
                Level += 1;
                maxHealth += 38f;
                health = Mathf.Min(maxHealth, health + 90f);

                var chosen = nextBuild;
                build = chosen;
                nextBuild = (HeroBuild)(((int)nextBuild + 1) % 3);
                ApplyBuildCard(chosen);
                buildData.LearnNextCard(Level, GameDirector.Instance.Monsters.Count, build);
                cards.Add($"BD: {buildData.Cards[buildData.Cards.Count - 1]}");
                DamageFeedbackSystem.Instance.ReportHeroBubble(this, BubbleTalkEvent.LevelUp, true);
                FeelImpactSystem.Instance.Play(FeelImpactEvent.LevelUp, FeelImpactLevel.Medium, Position, new Color(1f, 0.78f, 0.26f));
            }
        }

        private void ApplyBuildCard(HeroBuild chosen)
        {
            buildTimer = 5f;
            switch (chosen)
            {
                case HeroBuild.FlameAura:
                    cards.Add("Flame Aura");
                    break;
                case HeroBuild.Lifesteal:
                    cards.Add("Lifesteal");
                    break;
                case HeroBuild.ShieldWall:
                    cards.Add("Shield Wall");
                    shield += 95f + Level * 12f;
                    break;
            }
        }

        private void ApplyBuildPowers(GameDirector director)
        {
            buildTimer = Mathf.Max(0f, buildTimer - Time.deltaTime);
            auraTimer -= Time.deltaTime;
            shieldTimer -= Time.deltaTime;

            if (build == HeroBuild.FlameAura && auraTimer <= 0f)
            {
                auraTimer = 0.35f;
                foreach (var monster in director.Monsters)
                {
                    if (monster != null && Vector2.Distance(Position, monster.Position) < 1.65f)
                    {
                        monster.TakeDamage(12f + Level * 1.7f, DamageFeedbackType.Fire, Position, false, true, false);
                    }
                }
            }

            if (build == HeroBuild.ShieldWall && shieldTimer <= 0f)
            {
                shieldTimer = 5f;
                shield += 24f + Level * 3f;
            }
        }

        public void HealFromKill(float amount)
        {
            if (build != HeroBuild.Lifesteal || antiHealTimer > 0f)
            {
                return;
            }

            health = Mathf.Min(maxHealth, health + amount);
            DamageFeedbackSystem.Instance.ReportHeroHeal(this, amount);
        }

        public void ApplyHitReaction(Color flashColor, Vector2 sourcePosition, float knockbackDistance, float hitStopSeconds, float flashSeconds)
        {
            hitFlashColor = flashColor;
            hitFlashDuration = Mathf.Max(0.01f, flashSeconds);
            hitFlashTimer = Mathf.Max(hitFlashTimer, flashSeconds);
            hitStunTimer = Mathf.Max(hitStunTimer, hitStopSeconds);

            if (knockbackDistance > 0f)
            {
                var direction = ((Vector2)transform.position - sourcePosition).normalized;
                if (direction.sqrMagnitude < 0.01f)
                {
                    direction = Random.insideUnitCircle.normalized;
                }

                velocity += direction * knockbackDistance / Mathf.Max(0.08f, hitStopSeconds + 0.08f);
            }
        }

        private void ClampToArena()
        {
            var clamped = transform.position;
            clamped.x = Mathf.Clamp(clamped.x, -GameDirector.ArenaHalfWidth + 0.4f, GameDirector.ArenaHalfWidth - 0.4f);
            clamped.y = Mathf.Clamp(clamped.y, -GameDirector.ArenaHalfHeight + 0.4f, GameDirector.ArenaHalfHeight - 0.4f);
            transform.position = clamped;
        }

        private void UpdateVisuals()
        {
            if (meshRenderer == null)
            {
                return;
            }

            var baseColor = Color.white;
            switch (build)
            {
                case HeroBuild.FlameAura:
                    baseColor = new Color(1f, 0.54f, 0.28f);
                    break;
                case HeroBuild.Lifesteal:
                    baseColor = new Color(0.92f, 0.18f, 0.26f);
                    break;
                case HeroBuild.ShieldWall:
                    baseColor = new Color(0.34f, 0.72f, 1f);
                    break;
            }

            var visualColor = Color.Lerp(baseColor, Color.white, buildTimer > 0f ? Mathf.PingPong(Time.time * 5f, 0.45f) : 0.15f);
            if (hitFlashTimer > 0f)
            {
                hitFlashTimer -= Time.deltaTime;
                var flash = Mathf.Clamp01(hitFlashTimer / Mathf.Max(0.01f, hitFlashDuration));
                visualColor = Color.Lerp(visualColor, hitFlashColor, flash);
            }

            meshRenderer.material.color = visualColor;
            if (spriteRenderer != null)
            {
                var pulse = 1f + Mathf.Sin(Time.time * (buildTimer > 0f ? 12f : 5f)) * 0.025f;
                if (spriteAnimator != null)
                {
                    spriteAnimator.SetFacing(velocity.x);
                    var action = rhythmController != null && rhythmController.IsAttacking
                        ? UnitAnimationAction.Attack
                        : velocity.sqrMagnitude > 0.04f
                            ? UnitAnimationAction.Move
                            : UnitAnimationAction.Idle;
                    spriteAnimator.Play(action);
                    spriteAnimator.Tick(visualColor, pulse);
                }
                else
                {
                    spriteRenderer.color = visualColor;
                    var facing = velocity.x < -0.05f ? -1f : velocity.x > 0.05f ? 1f : spriteRenderer.transform.localScale.x >= 0f ? 1f : -1f;
                    spriteRenderer.transform.localScale = new Vector3(Mathf.Abs(spriteBaseScale.x) * facing, spriteBaseScale.y, spriteBaseScale.z) * pulse;
                }
            }
        }
    }
}
