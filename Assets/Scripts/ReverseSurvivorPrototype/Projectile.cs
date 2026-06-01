using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public sealed class Projectile : MonoBehaviour
    {
        private const float ProjectileVisualScale = 1.95f;

        private MonsterUnit target;
        private Vector2 direction;
        private float damage;
        private float speed;
        private float lifetime = 2f;
        private bool directional;
        private int pierceRemaining;
        private float trailTimer;
        private ProjectilePayload payload;
        private bool hasPayload;
        private SpriteRenderer spriteRenderer;

        public static void Create(Vector2 start, MonsterUnit target, float damage, Color color, float speed)
        {
            var projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Hero Bolt";
            projectileObject.transform.position = new Vector3(start.x, start.y, -0.25f);
            projectileObject.transform.localScale = Vector3.one * 0.16f;
            Destroy(projectileObject.GetComponent<SphereCollider>());
            projectileObject.GetComponent<MeshRenderer>().enabled = false;
            MusicManiacArtLibrary.AttachSprite(projectileObject, MusicManiacArtLibrary.ProjectileSprite(ElementModule.None), 0.4f * ProjectileVisualScale, 13, "Projectile Pixel Art", color);

            var projectile = projectileObject.AddComponent<Projectile>();
            projectile.target = target;
            projectile.damage = damage;
            projectile.speed = speed;
            projectile.spriteRenderer = projectileObject.GetComponentInChildren<SpriteRenderer>();
            projectile.AttachProjectileAnimation(ElementModule.None);
        }

        public static void Create(Vector2 start, MonsterUnit target, float damage, Color color, float speed, ProjectilePayload payload)
        {
            var projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Element Beat";
            projectileObject.transform.position = new Vector3(start.x, start.y, -0.25f);
            projectileObject.transform.localScale = Vector3.one * 0.16f * payload.SizeMultiplier;
            Destroy(projectileObject.GetComponent<SphereCollider>());
            projectileObject.GetComponent<MeshRenderer>().enabled = false;
            MusicManiacArtLibrary.AttachSprite(projectileObject, MusicManiacArtLibrary.ProjectileSprite(payload.Element), 0.44f * payload.SizeMultiplier * ProjectileVisualScale, 13, "Projectile Pixel Art", Color.white);

            var projectile = projectileObject.AddComponent<Projectile>();
            projectile.target = target;
            projectile.damage = damage;
            projectile.speed = speed;
            projectile.payload = payload;
            projectile.hasPayload = true;
            projectile.pierceRemaining = payload.PierceCount;
            projectile.spriteRenderer = projectileObject.GetComponentInChildren<SpriteRenderer>();
            projectile.AttachProjectileAnimation(payload.Element);
        }

        public static void CreateDirectional(Vector2 start, Vector2 direction, float damage, Color color, float speed, float lifetime)
        {
            CreateDirectional(start, direction, damage, color, speed, lifetime, default(ProjectilePayload), false);
        }

        public static void CreateDirectional(Vector2 start, Vector2 direction, float damage, Color color, float speed, float lifetime, ProjectilePayload payload)
        {
            CreateDirectional(start, direction, damage, color, speed, lifetime, payload, true);
        }

        private static void CreateDirectional(Vector2 start, Vector2 direction, float damage, Color color, float speed, float lifetime, ProjectilePayload payload, bool hasPayload)
        {
            var projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.name = "Rhythm Bullet";
            projectileObject.transform.position = new Vector3(start.x, start.y, -0.25f);
            projectileObject.transform.localScale = Vector3.one * 0.13f * (hasPayload ? payload.SizeMultiplier : 1f);
            Destroy(projectileObject.GetComponent<SphereCollider>());
            projectileObject.GetComponent<MeshRenderer>().enabled = false;
            MusicManiacArtLibrary.AttachSprite(
                projectileObject,
                hasPayload ? MusicManiacArtLibrary.ProjectileSprite(payload.Element) : MusicManiacArtLibrary.ProjectileSprite(ElementModule.None),
                0.38f * (hasPayload ? payload.SizeMultiplier : 1f) * ProjectileVisualScale,
                13,
                "Projectile Pixel Art",
                Color.white);

            var projectile = projectileObject.AddComponent<Projectile>();
            projectile.directional = true;
            projectile.direction = direction.sqrMagnitude > 0.01f ? direction.normalized : Vector2.right;
            projectile.damage = damage;
            projectile.speed = speed;
            projectile.lifetime = lifetime;
            projectile.payload = payload;
            projectile.hasPayload = hasPayload;
            projectile.pierceRemaining = hasPayload ? payload.PierceCount : 0;
            projectile.spriteRenderer = projectileObject.GetComponentInChildren<SpriteRenderer>();
            projectile.AttachProjectileAnimation(hasPayload ? payload.Element : ElementModule.None);
            projectile.RotateSpriteTowardDirection(projectile.direction);
        }

        private void Update()
        {
            var director = GameDirector.Instance;
            if (director == null || director.IsRestarting)
            {
                Destroy(gameObject);
                return;
            }

            lifetime -= Time.deltaTime;
            if (directional)
            {
                transform.position += (Vector3)(direction * speed * Time.deltaTime);
                RotateSpriteTowardDirection(direction);
                TickTrail();
                CheckDirectionalHit();
                if (lifetime <= 0f)
                {
                    Destroy(gameObject);
                }

                return;
            }

            if (target == null || lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            var toTarget = target.Position - (Vector2)transform.position;
            if (toTarget.magnitude < 0.24f)
            {
                HitMonster(target);
                return;
            }

            transform.position += (Vector3)(toTarget.normalized * speed * Time.deltaTime);
            RotateSpriteTowardDirection(toTarget);
            TickTrail();
        }

        private void CheckDirectionalHit()
        {
            var director = GameDirector.Instance;
            if (director == null || director.IsRestarting)
            {
                Destroy(gameObject);
                return;
            }

            foreach (var monster in director.Monsters)
            {
                if (monster == null)
                {
                    continue;
                }

                if (Vector2.Distance(transform.position, monster.Position) < 0.34f)
                {
                    HitMonster(monster);
                    return;
                }
            }
        }

        private void HitMonster(MonsterUnit monster)
        {
            if (monster == null)
            {
                Destroy(gameObject);
                return;
            }

            var feedbackType = hasPayload ? FeedbackTypeForElement(payload.Element) : DamageFeedbackType.Physical;
            var isCritical = hasPayload && (payload.Element == ElementModule.Lightning || damage >= 26f);
            if (MusicManiacAudioSystem.Instance != null)
            {
                MusicManiacAudioSystem.Instance.PlayProjectile(hasPayload ? payload.Element : ElementModule.None, "hit", transform.position, isCritical ? 1f : 0.72f);
            }

            monster.TakeDamage(damage, feedbackType, transform.position, false, false, isCritical);

            if (hasPayload)
            {
                ApplyPayload(monster);
            }

            var hitColor = hasPayload ? payload.Color : Color.white;
            var hitRadius = hasPayload ? 0.86f * payload.SizeMultiplier : 0.62f;
            VisualFactory.CreatePulseRing(transform.position, hitRadius * 0.48f, hitColor, 0.2f);
            VisualFactory.CreateAnimatedSpriteBurst(
                transform.position,
                "vfx/vfx_hit_spark",
                hitRadius,
                hitColor,
                0.28f,
                18,
                8,
                24f);

            if (pierceRemaining > 0)
            {
                pierceRemaining--;
                return;
            }

            Destroy(gameObject);
        }

        private void ApplyPayload(MonsterUnit monster)
        {
            switch (payload.Element)
            {
                case ElementModule.Lightning:
                    ChainLightning(monster.Position, payload.LightningBounces, damage * 0.45f);
                    break;
                case ElementModule.Ice:
                    monster.ApplyFreeze(payload.FreezeSeconds);
                    break;
                case ElementModule.Fire:
                    ExplodeFire(monster.Position);
                    break;
                case ElementModule.Poison:
                    monster.ApplyPoison(payload.PoisonSeconds, damage * 0.15f);
                    break;
            }
        }

        private void RotateSpriteTowardDirection(Vector2 lookDirection)
        {
            if (spriteRenderer == null || lookDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
            spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void ChainLightning(Vector2 origin, int bounces, float bounceDamage)
        {
            var director = GameDirector.Instance;
            if (director == null || director.IsRestarting)
            {
                Destroy(gameObject);
                return;
            }

            var current = origin;
            for (var i = 0; i < bounces; i++)
            {
                var next = FindNearestMonster(current, 2.8f);
                if (next == null)
                {
                    return;
                }

                VisualFactory.CreateFlash(current, next.Position, AIHeroBuildData.ElementColor(ElementModule.Lightning));
                if (MusicManiacAudioSystem.Instance != null)
                {
                    MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.LightningBounce, current, 0.75f);
                }

                next.TakeDamage(bounceDamage, DamageFeedbackType.Lightning, current, false, false, false);
                current = next.Position;
            }
        }

        private MonsterUnit FindNearestMonster(Vector2 origin, float radius)
        {
            var director = GameDirector.Instance;
            if (director == null || director.IsRestarting)
            {
                return null;
            }

            MonsterUnit best = null;
            var bestDistance = radius;
            foreach (var monster in director.Monsters)
            {
                if (monster == null)
                {
                    continue;
                }

                var distance = Vector2.Distance(origin, monster.Position);
                if (distance > 0.05f && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = monster;
                }
            }

            return best;
        }

        private void ExplodeFire(Vector2 position)
        {
            var director = GameDirector.Instance;
            if (director == null || director.IsRestarting)
            {
                Destroy(gameObject);
                return;
            }

            var radius = Mathf.Max(0.55f, payload.FireRadius);
            VisualFactory.CreateAnimatedSpriteBurst(position, "vfx/vfx_aoe_fire", radius * 2.65f, AIHeroBuildData.ElementColor(ElementModule.Fire), 0.42f, -4, 8, 20f);
            VisualFactory.CreatePulseRing(position, radius * 1.12f, AIHeroBuildData.ElementColor(ElementModule.Fire), 0.28f);
            VisualFactory.CreatePulseRing(position, radius * 0.72f, new Color(1f, 0.82f, 0.34f), 0.22f);
            if (MusicManiacAudioSystem.Instance != null)
            {
                MusicManiacAudioSystem.Instance.PlayProjectile(ElementModule.Fire, "hit", position, 1f);
            }

            foreach (var monster in director.Monsters)
            {
                if (monster != null && Vector2.Distance(monster.Position, position) <= radius)
                {
                    monster.TakeDamage(damage * 0.35f, DamageFeedbackType.Fire, position, false, true, false);
                }
            }
        }

        private void TickTrail()
        {
            if (!hasPayload || payload.Trail == TrailModule.None)
            {
                return;
            }

            trailTimer -= Time.deltaTime;
            if (trailTimer > 0f)
            {
                return;
            }

            trailTimer = 0.105f;
            RhythmAreaEffect.Create(transform.position, payload.Trail, 0.56f * payload.SizeMultiplier, payload.TrailDuration);
        }

        private static DamageFeedbackType FeedbackTypeForElement(ElementModule element)
        {
            switch (element)
            {
                case ElementModule.Lightning:
                    return DamageFeedbackType.Lightning;
                case ElementModule.Ice:
                    return DamageFeedbackType.Ice;
                case ElementModule.Fire:
                    return DamageFeedbackType.Fire;
                case ElementModule.Poison:
                    return DamageFeedbackType.Poison;
                default:
                    return DamageFeedbackType.Physical;
            }
        }

        private void AttachProjectileAnimation(ElementModule element)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            SpriteSequenceAnimator.Attach(spriteRenderer, ProjectileAnimationPrefix(element), 8, 14f);
        }

        private static string ProjectileAnimationPrefix(ElementModule element)
        {
            switch (element)
            {
                case ElementModule.Lightning:
                    return "projectiles/projectile_lightning_note";
                case ElementModule.Ice:
                    return "projectiles/projectile_ice_note";
                case ElementModule.Fire:
                    return "projectiles/projectile_fire_note";
                case ElementModule.Poison:
                    return "projectiles/projectile_poison_note";
                default:
                    return "projectiles/projectile_basic_note";
            }
        }
    }
}
