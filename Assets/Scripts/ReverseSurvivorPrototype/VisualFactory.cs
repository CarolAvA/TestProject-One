using System.Collections;
using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public static class VisualFactory
    {
        private static Shader cachedColorShader;

        public static Material CreateMaterial(Color color)
        {
            var shader = GetColorShader();
            var material = shader != null ? new Material(shader) : new Material(Shader.Find("Sprites/Default"));
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            return material;
        }

        private static Shader GetColorShader()
        {
            if (cachedColorShader != null)
            {
                return cachedColorShader;
            }

            cachedColorShader =
                Shader.Find("Sprites/Default") ??
                Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ??
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Hidden/Internal-Colored");
            return cachedColorShader;
        }

        public static void CreateLine(Vector2 from, Vector2 to, Color color, float width, string name)
        {
            var lineObject = new GameObject(name);
            var line = lineObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(from.x, from.y, 0f));
            line.SetPosition(1, new Vector3(to.x, to.y, 0f));
            line.startWidth = width;
            line.endWidth = width;
            line.material = CreateMaterial(color);
            line.sortingOrder = -5;
            line.useWorldSpace = true;
        }

        public static void CreateFlash(Vector2 from, Vector2 to, Color color)
        {
            var flashObject = new GameObject("Attack Flash");
            var flash = flashObject.AddComponent<AttackFlash>();
            flash.Initialize(from, to, color);
        }

        public static void CreateFloatingText(Vector2 position, string text, Color color)
        {
            var textObject = new GameObject($"Floating Text - {text}");
            textObject.transform.position = new Vector3(position.x, position.y + 0.5f, -0.7f);
            var mesh = textObject.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = 0.24f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;

            var floating = textObject.AddComponent<FloatingText>();
            floating.Initialize(color);
        }

        public static void CreatePulseRing(Vector2 position, float radius, Color color, float lifetime)
        {
            var ringObject = new GameObject("Pulse Ring");
            ringObject.transform.position = new Vector3(position.x, position.y, -0.5f);
            var ring = ringObject.AddComponent<PulseRing>();
            ring.Initialize(radius, color, lifetime);
        }

        public static void CreateSpriteBurst(Vector2 position, Sprite sprite, float worldHeight, Color color, float lifetime)
        {
            if (sprite == null)
            {
                CreatePulseRing(position, worldHeight * 0.5f, color, lifetime);
                return;
            }

            var burstObject = MusicManiacArtLibrary.CreateSpriteObject("Pixel Burst", sprite, position, worldHeight, 16, color);
            burstObject.transform.position = new Vector3(position.x, position.y, -0.72f);
            var burst = burstObject.AddComponent<SpriteBurst>();
            burst.Initialize(lifetime, color);
        }

        public static void CreateAnimatedSpriteBurst(Vector2 position, string resourcePrefix, float worldHeight, Color color, float lifetime, int sortingOrder = 16, int frameCount = 8, float framesPerSecond = 18f)
        {
            var firstFrame = MusicManiacArtLibrary.LoadSprite($"{resourcePrefix}_anim_00");
            if (firstFrame == null)
            {
                CreatePulseRing(position, worldHeight * 0.5f, color, lifetime);
                return;
            }

            var burstObject = MusicManiacArtLibrary.CreateSpriteObject("Animated Pixel Burst", firstFrame, position, worldHeight, sortingOrder, color);
            burstObject.transform.position = new Vector3(position.x, position.y, -0.72f);
            var renderer = burstObject.GetComponentInChildren<SpriteRenderer>();
            SpriteSequenceAnimator.Attach(renderer, resourcePrefix, frameCount, framesPerSecond, false);
            var burst = burstObject.AddComponent<SpriteBurst>();
            burst.Initialize(lifetime, color);
        }

        private sealed class AttackFlash : MonoBehaviour
        {
            private LineRenderer line;
            private float lifetime = 0.12f;

            public void Initialize(Vector2 from, Vector2 to, Color color)
            {
                line = gameObject.AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.SetPosition(0, new Vector3(from.x, from.y, -0.35f));
                line.SetPosition(1, new Vector3(to.x, to.y, -0.35f));
                line.startWidth = 0.035f;
                line.endWidth = 0.035f;
                line.material = CreateMaterial(color);
            }

            private void Update()
            {
                lifetime -= Time.deltaTime;
                if (lifetime <= 0f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private sealed class FloatingText : MonoBehaviour
        {
            private TextMesh mesh;
            private Color color;
            private float lifetime = 0.9f;

            public void Initialize(Color textColor)
            {
                color = textColor;
                mesh = GetComponent<TextMesh>();
            }

            private void Update()
            {
                lifetime -= Time.deltaTime;
                transform.position += Vector3.up * Time.deltaTime * 0.7f;

                if (mesh != null)
                {
                    var faded = color;
                    faded.a = Mathf.Clamp01(lifetime / 0.9f);
                    mesh.color = faded;
                }

                if (lifetime <= 0f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private sealed class PulseRing : MonoBehaviour
        {
            private LineRenderer line;
            private Color color;
            private float radius;
            private float lifetime;
            private float maxLifetime;

            public void Initialize(float targetRadius, Color ringColor, float seconds)
            {
                radius = targetRadius;
                color = ringColor;
                lifetime = seconds;
                maxLifetime = seconds;
                line = gameObject.AddComponent<LineRenderer>();
                line.loop = true;
                line.positionCount = 48;
                line.startWidth = 0.045f;
                line.endWidth = 0.045f;
                line.material = CreateMaterial(color);
                line.useWorldSpace = false;
            }

            private void Update()
            {
                lifetime -= Time.deltaTime;
                var progress = 1f - Mathf.Clamp01(lifetime / Mathf.Max(0.01f, maxLifetime));
                var currentRadius = Mathf.Lerp(radius * 0.25f, radius, progress);
                for (var i = 0; i < line.positionCount; i++)
                {
                    var angle = Mathf.PI * 2f * i / line.positionCount;
                    line.SetPosition(i, new Vector3(Mathf.Cos(angle) * currentRadius, Mathf.Sin(angle) * currentRadius, 0f));
                }

                var faded = color;
                faded.a = Mathf.Clamp01(lifetime / Mathf.Max(0.01f, maxLifetime));
                line.material.color = faded;

                if (lifetime <= 0f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private sealed class SpriteBurst : MonoBehaviour
        {
            private SpriteRenderer spriteRenderer;
            private Color color;
            private float lifetime;
            private float maxLifetime;
            private Vector3 startScale;

            public void Initialize(float seconds, Color burstColor)
            {
                lifetime = Mathf.Max(0.05f, seconds);
                maxLifetime = lifetime;
                color = burstColor;
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                startScale = spriteRenderer != null ? spriteRenderer.transform.localScale : Vector3.one;
            }

            private void Update()
            {
                lifetime -= Time.deltaTime;
                var progress = 1f - Mathf.Clamp01(lifetime / Mathf.Max(0.01f, maxLifetime));

                if (spriteRenderer != null)
                {
                    spriteRenderer.transform.localScale = startScale * Mathf.Lerp(0.65f, 1.55f, progress);
                    var faded = Color.Lerp(color, Color.white, 0.25f);
                    faded.a = Mathf.Clamp01(lifetime / Mathf.Max(0.01f, maxLifetime));
                    spriteRenderer.color = faded;
                }

                if (lifetime <= 0f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
