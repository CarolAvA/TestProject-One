using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public sealed class RhythmAreaEffect : MonoBehaviour
    {
        private TrailModule trail;
        private float radius;
        private float remaining;
        private float tickTimer;
        private MeshRenderer meshRenderer;
        private SpriteRenderer spriteRenderer;

        public static void Create(Vector2 position, TrailModule trail, float radius, float duration)
        {
            if (trail == TrailModule.None)
            {
                return;
            }

            var areaObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            areaObject.name = $"Rhythm Trail - {trail}";
            areaObject.transform.position = new Vector3(position.x, position.y, -0.42f);
            areaObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            areaObject.transform.localScale = new Vector3(radius * 2f, 0.025f, radius * 2f);
            Object.Destroy(areaObject.GetComponent<CapsuleCollider>());

            var area = areaObject.AddComponent<RhythmAreaEffect>();
            area.Initialize(trail, radius, duration);
        }

        private void Initialize(TrailModule trailModule, float effectRadius, float duration)
        {
            trail = trailModule;
            radius = effectRadius;
            remaining = duration;
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = VisualFactory.CreateMaterial(ColorForTrail(trail));
            meshRenderer.enabled = false;
            spriteRenderer = MusicManiacArtLibrary.AttachSprite(
                gameObject,
                SpriteForTrail(trail),
                radius * 2.1f,
                -6,
                "Rhythm Trail Pixel Art",
                ColorForTrail(trail));
            if (spriteRenderer != null)
            {
                SpriteSequenceAnimator.Attach(spriteRenderer, AnimationPrefixForTrail(trail), 8, 10f);
            }
        }

        private void Update()
        {
            remaining -= Time.deltaTime;
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                tickTimer = 0.35f;
                TickMonsters();
            }

            if (meshRenderer != null)
            {
                var color = ColorForTrail(trail);
                color.a = Mathf.Clamp01(remaining * 0.45f);
                meshRenderer.material.color = color;
            }

            if (spriteRenderer != null)
            {
                var color = ColorForTrail(trail);
                color.a = Mathf.Clamp01(remaining * 0.5f);
                spriteRenderer.color = color;
                spriteRenderer.transform.Rotate(0f, 0f, Time.deltaTime * 28f);
            }

            if (remaining <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void TickMonsters()
        {
            foreach (var monster in GameDirector.Instance.Monsters)
            {
                if (monster == null || Vector2.Distance(monster.Position, transform.position) > radius)
                {
                    continue;
                }

                switch (trail)
                {
                    case TrailModule.Poison:
                        monster.ApplyPoison(1.2f, 5f);
                        break;
                    case TrailModule.Ice:
                        monster.ApplyFreeze(0.22f);
                        break;
                    case TrailModule.Fire:
                        monster.TakeDamage(7f, DamageFeedbackType.Fire, transform.position, true, false, false);
                        break;
                }
            }
        }

        private static Color ColorForTrail(TrailModule trail)
        {
            switch (trail)
            {
                case TrailModule.Poison:
                    return new Color(0.28f, 0.85f, 0.22f, 0.38f);
                case TrailModule.Ice:
                    return new Color(0.35f, 0.75f, 1f, 0.35f);
                case TrailModule.Fire:
                    return new Color(1f, 0.38f, 0.08f, 0.4f);
                default:
                    return new Color(1f, 1f, 1f, 0.2f);
            }
        }

        private static Sprite SpriteForTrail(TrailModule trail)
        {
            switch (trail)
            {
                case TrailModule.Poison:
                    return MusicManiacArtLibrary.Vfx("vfx_aoe_poison");
                case TrailModule.Ice:
                    return MusicManiacArtLibrary.Vfx("vfx_aoe_ice");
                case TrailModule.Fire:
                    return MusicManiacArtLibrary.Vfx("vfx_aoe_fire");
                default:
                    return MusicManiacArtLibrary.Vfx("vfx_sonic_ring");
            }
        }

        private static string AnimationPrefixForTrail(TrailModule trail)
        {
            switch (trail)
            {
                case TrailModule.Poison:
                    return "vfx/vfx_aoe_poison";
                case TrailModule.Ice:
                    return "vfx/vfx_aoe_ice";
                case TrailModule.Fire:
                    return "vfx/vfx_aoe_fire";
                default:
                    return "vfx/vfx_aoe_sonic";
            }
        }
    }
}
