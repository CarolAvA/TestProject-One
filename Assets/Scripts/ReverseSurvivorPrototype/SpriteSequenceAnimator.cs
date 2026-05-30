using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public sealed class SpriteSequenceAnimator : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private Sprite[] frames;
        private int frameIndex;
        private float frameTimer;
        private float frameDuration = 0.08f;
        private bool loop = true;
        private bool destroyOnComplete;

        public static SpriteSequenceAnimator Attach(SpriteRenderer renderer, string resourcePrefix, int frameCount, float framesPerSecond, bool loop = true, bool destroyOnComplete = false)
        {
            if (renderer == null || string.IsNullOrEmpty(resourcePrefix) || frameCount <= 0)
            {
                return null;
            }

            var loaded = new Sprite[frameCount];
            for (var i = 0; i < frameCount; i++)
            {
                var sprite = MusicManiacArtLibrary.LoadSprite($"{resourcePrefix}_anim_{i:00}");
                if (sprite == null)
                {
                    return null;
                }

                loaded[i] = sprite;
            }

            var animator = renderer.GetComponent<SpriteSequenceAnimator>();
            if (animator == null)
            {
                animator = renderer.gameObject.AddComponent<SpriteSequenceAnimator>();
            }

            animator.spriteRenderer = renderer;
            animator.frames = loaded;
            animator.frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
            animator.loop = loop;
            animator.destroyOnComplete = destroyOnComplete;
            animator.frameIndex = 0;
            animator.frameTimer = 0f;
            renderer.sprite = loaded[0];
            return animator;
        }

        private void Update()
        {
            if (spriteRenderer == null || frames == null || frames.Length <= 1)
            {
                return;
            }

            frameTimer += Time.deltaTime;
            if (frameTimer < frameDuration)
            {
                return;
            }

            frameTimer -= frameDuration;
            frameIndex++;
            if (frameIndex >= frames.Length)
            {
                if (destroyOnComplete)
                {
                    Destroy(transform.root.gameObject);
                    return;
                }

                frameIndex = loop ? 0 : frames.Length - 1;
            }

            spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
