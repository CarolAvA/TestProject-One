using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public sealed class MonsterUnit : MonoBehaviour
    {
        private float health;
        private float attackTimer;
        private bool dead;
        private float freezeTimer;
        private float poisonTimer;
        private float poisonDamagePerSecond;
        private float hitStopTimer;
        private float hitFlashTimer;
        private float hitFlashDuration;
        private float knockbackTimer;
        private float knockbackDuration;
        private Vector2 knockbackVelocity;
        private Color hitFlashColor;
        private MeshRenderer meshRenderer;
        private SpriteRenderer spriteRenderer;
        private Vector3 spriteBaseScale = Vector3.one;
        private UnitSpriteAnimator spriteAnimator;
        private UnitAnimationAction requestedAction = UnitAnimationAction.Idle;
        private UnitAnimationAction forcedAction = UnitAnimationAction.Idle;
        private float forcedActionTimer;
        private float moveSfxTimer;

        public MonsterConfig Config { get; private set; }
        public Vector2 Position => transform.position;

        public static MonsterUnit Create(MonsterConfig config, Vector2 position)
        {
            var primitive = config.Kind == MonsterKind.Archer || config.Kind == MonsterKind.HexPriest
                ? PrimitiveType.Cylinder
                : PrimitiveType.Cube;

            var monsterObject = GameObject.CreatePrimitive(primitive);
            monsterObject.name = config.DisplayName;
            monsterObject.transform.position = new Vector3(position.x, position.y, -0.1f);
            monsterObject.transform.localScale = Vector3.one * (config.Kind == MonsterKind.BoneKing ? 1.25f : 0.65f);

            var collider = monsterObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var monster = monsterObject.AddComponent<MonsterUnit>();
            monster.Initialize(config);
            GameDirector.Instance.RegisterMonster(monster);
            return monster;
        }

        private void Initialize(MonsterConfig config)
        {
            Config = config;
            health = config.Health;
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = VisualFactory.CreateMaterial(config.Color);
            meshRenderer.enabled = false;
            spriteRenderer = MusicManiacArtLibrary.AttachSprite(
                gameObject,
                MusicManiacArtLibrary.MonsterSprite(config.Kind),
                config.Kind == MonsterKind.BoneKing ? 2.75f : config.Kind == MonsterKind.Stoneguard ? 1.75f : 1.35f,
                config.Kind == MonsterKind.BoneKing ? 8 : 7,
                "Monster Pixel Art",
                Color.white);
            if (spriteRenderer != null)
            {
                spriteBaseScale = spriteRenderer.transform.localScale;
                spriteAnimator = UnitSpriteAnimator.AttachMonster(gameObject, spriteRenderer, config.Kind);
                ForceAction(UnitAnimationAction.Spawn, 0.35f);
            }
        }

        private void Update()
        {
            var director = GameDirector.Instance;
            if (director == null || director.IsRestarting)
            {
                return;
            }

            TickHitReaction();
            TickStatusEffects();
            if (dead)
            {
                requestedAction = UnitAnimationAction.Death;
                PulseWhenReady();
                return;
            }

            if (hitStopTimer > 0f)
            {
                requestedAction = UnitAnimationAction.Hit;
                PulseWhenReady();
                return;
            }

            if (freezeTimer > 0f)
            {
                requestedAction = UnitAnimationAction.Idle;
                PulseWhenReady();
                return;
            }

            var hero = director.Hero;
            if (hero == null || hero.Health <= 0f)
            {
                return;
            }

            attackTimer -= Time.deltaTime;
            var toHero = hero.Position - Position;
            var distance = toHero.magnitude;
            var attackRange = EffectiveAttackRange();

            if (distance > attackRange)
            {
                var movement = toHero.normalized * GetCurrentMoveSpeed() * Time.deltaTime;
                transform.position += (Vector3)movement;
                requestedAction = movement.sqrMagnitude > 0.0001f ? UnitAnimationAction.Move : UnitAnimationAction.Idle;
                TickMoveSfx();
            }
            else if (attackTimer <= 0f)
            {
                attackTimer = Config.AttackCooldown;
                ForceAction(UnitAnimationAction.Attack, 0.22f);
                AttackHero(hero);
            }
            else
            {
                requestedAction = UnitAnimationAction.Idle;
            }

            PulseWhenReady();
        }

        public void TakeDamage(float amount)
        {
            TakeDamage(amount, DamageFeedbackType.Physical, Position, false, false, false);
        }

        public void TakeDamage(float amount, DamageFeedbackType feedbackType, Vector2 sourcePosition, bool isDot = false, bool isAoe = false, bool isCritical = false)
        {
            var director = GameDirector.Instance;
            if (dead || director == null || director.IsRestarting)
            {
                return;
            }

            health -= amount;
            var isKill = health <= 0f;
            if (DamageFeedbackSystem.Instance != null)
            {
                DamageFeedbackSystem.Instance.ReportMonsterDamage(this, amount, feedbackType, sourcePosition, isDot, isAoe, isCritical, isKill);
            }

            if (isKill)
            {
                Die();
            }
        }

        public void ApplyFreeze(float seconds)
        {
            freezeTimer = Mathf.Max(freezeTimer, seconds);
        }

        public void ApplyPoison(float seconds, float damagePerSecond)
        {
            poisonTimer = Mathf.Max(poisonTimer, seconds);
            poisonDamagePerSecond = Mathf.Max(poisonDamagePerSecond, damagePerSecond);
        }

        public float GetCurrentMoveSpeed()
        {
            return freezeTimer > 0f ? 0f : Config.MoveSpeed;
        }

        private void AttackHero(HeroController hero)
        {
            var antiHeal = Config.Kind == MonsterKind.HexPriest;
            var shieldbreaker = Config.Kind == MonsterKind.Shieldbreaker;
            var damage = Config.Kind == MonsterKind.Assassin ? Config.Damage * 1.8f : Config.Damage;
            if (MusicManiacAudioSystem.Instance != null)
            {
                MusicManiacAudioSystem.Instance.PlayMonster(Config.Kind, "attack", Position, 1f);
            }

            hero.TakeDamage(damage, Config.Kind, antiHeal, shieldbreaker);

            if (Config.AttackRange > 1.5f)
            {
                VisualFactory.CreateFlash(Position, hero.Position, Config.Color);
            }
        }

        private float EffectiveAttackRange()
        {
            var bodyPadding = Config.Kind == MonsterKind.BoneKing
                ? 0.85f
                : Config.Kind == MonsterKind.Stoneguard
                    ? 0.62f
                    : 0.48f;
            return Config.AttackRange + bodyPadding;
        }

        private void TickStatusEffects()
        {
            freezeTimer = Mathf.Max(0f, freezeTimer - Time.deltaTime);

            if (poisonTimer > 0f)
            {
                poisonTimer -= Time.deltaTime;
                TakeDamage(poisonDamagePerSecond * Time.deltaTime, DamageFeedbackType.Poison, Position, true, false, false);
            }
            else
            {
                poisonDamagePerSecond = 0f;
            }
        }

        public void ApplyHitReaction(Color flashColor, Vector2 sourcePosition, float knockbackDistance, float hitStopSeconds, float flashSeconds)
        {
            hitFlashColor = flashColor;
            hitFlashDuration = Mathf.Max(0.01f, flashSeconds);
            hitFlashTimer = Mathf.Max(hitFlashTimer, flashSeconds);
            hitStopTimer = Mathf.Max(hitStopTimer, hitStopSeconds);
            ForceAction(UnitAnimationAction.Hit, flashSeconds);

            if (knockbackDistance <= 0f)
            {
                return;
            }

            var direction = ((Vector2)transform.position - sourcePosition).normalized;
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = Random.insideUnitCircle.normalized;
            }

            if (Config.Kind == MonsterKind.BoneKing)
            {
                knockbackDistance *= 0.32f;
            }
            else if (Config.Kind == MonsterKind.Stoneguard)
            {
                knockbackDistance *= 0.55f;
            }

            knockbackDuration = 0.13f;
            knockbackTimer = knockbackDuration;
            knockbackVelocity = direction * (knockbackDistance / knockbackDuration);
        }

        private void TickHitReaction()
        {
            if (hitStopTimer > 0f)
            {
                hitStopTimer -= Time.deltaTime;
            }

            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.deltaTime;
                transform.position += (Vector3)(knockbackVelocity * Time.deltaTime);
            }
        }

        private void Die()
        {
            dead = true;
            ForceAction(UnitAnimationAction.Death, 0.25f);
            var director = GameDirector.Instance;
            if (director == null || director.IsRestarting)
            {
                Destroy(gameObject, Config.Kind == MonsterKind.BoneKing ? 0.9f : 0.62f);
                return;
            }

            var hero = director.Hero;
            if (hero != null)
            {
                hero.HealFromKill(Config.Health * 0.08f);
            }

            if (MusicManiacAudioSystem.Instance != null)
            {
                MusicManiacAudioSystem.Instance.PlayMonster(Config.Kind, "death", Position, Config.Kind == MonsterKind.BoneKing ? 1.2f : 1f);
            }

            var xp = Config.Kind == MonsterKind.BoneKing ? 80f : Mathf.Max(8f, Config.Cost * 0.55f);
            VisualFactory.CreateAnimatedSpriteBurst(Position, "vfx/vfx_death_noise", Config.Kind == MonsterKind.BoneKing ? 2.2f : 1.15f, Config.Color, 0.52f, 18, 8, 16f);
            director.DropExperience(Position, xp);
            Destroy(gameObject, Config.Kind == MonsterKind.BoneKing ? 0.9f : 0.62f);
        }

        private void TickMoveSfx()
        {
            moveSfxTimer -= Time.deltaTime;
            if (moveSfxTimer > 0f)
            {
                return;
            }

            moveSfxTimer = Config.Kind == MonsterKind.Stoneguard || Config.Kind == MonsterKind.BoneKing ? 0.62f : 0.38f;
            if (MusicManiacAudioSystem.Instance != null)
            {
                MusicManiacAudioSystem.Instance.PlayMonster(Config.Kind, "move", Position, 0.42f);
            }
        }

        private void PulseWhenReady()
        {
            if (meshRenderer == null && spriteRenderer == null)
            {
                return;
            }

            var pulse = attackTimer <= 0f ? 1f : 0.72f;
            var baseColor = Color.Lerp(Color.black, Config.Color, pulse);
            if (hitFlashTimer > 0f)
            {
                hitFlashTimer -= Time.deltaTime;
                var flash = Mathf.Clamp01(hitFlashTimer / Mathf.Max(0.01f, hitFlashDuration));
                baseColor = Color.Lerp(baseColor, hitFlashColor, flash);
            }

            if (meshRenderer != null)
            {
                meshRenderer.material.color = baseColor;
            }

            if (spriteRenderer != null)
            {
                var pulseScale = 1f + Mathf.Sin(Time.time * 8f) * 0.025f;
                if (spriteAnimator != null)
                {
                    var hero = GameDirector.Instance != null ? GameDirector.Instance.Hero : null;
                    if (hero != null)
                    {
                        spriteAnimator.SetFacing(hero.Position.x - Position.x);
                    }

                    if (forcedActionTimer > 0f)
                    {
                        forcedActionTimer -= Time.deltaTime;
                    }

                    var action = forcedActionTimer > 0f ? forcedAction : requestedAction;
                    spriteAnimator.Play(action);
                    spriteAnimator.Tick(Color.Lerp(baseColor, Color.white, 0.45f), pulseScale);
                }
                else
                {
                    spriteRenderer.color = Color.Lerp(baseColor, Color.white, 0.45f);
                    spriteRenderer.transform.localScale = spriteBaseScale * pulseScale;
                }
            }
        }

        private void ForceAction(UnitAnimationAction action, float seconds)
        {
            forcedAction = action;
            requestedAction = action;
            forcedActionTimer = Mathf.Max(forcedActionTimer, seconds);
            if (spriteAnimator != null)
            {
                spriteAnimator.Play(action, true);
            }
        }
    }
}
