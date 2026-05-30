using System.Collections.Generic;
using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public sealed class UnitSpriteAnimator : MonoBehaviour
    {
        private const string ConfigResourcePath = "ReverseSurvivorConfig/MusicManiacConfigDatabase";

        private static MusicManiacConfigDatabase cachedDatabase;

        private readonly Dictionary<UnitAnimationAction, UnitActionAnimationConfig> actions = new Dictionary<UnitAnimationAction, UnitActionAnimationConfig>();

        private SpriteRenderer spriteRenderer;
        private UnitAnimationAction currentAction = UnitAnimationAction.Idle;
        private UnitActionAnimationConfig currentConfig;
        private int frameIndex;
        private float frameTimer;
        private float facing = 1f;
        private Vector2 fallbackSize = Vector2.one;
        private float sizeMultiplier = 1f;
        private bool initialized;

        public bool UseUnscaledTime { get; set; }

        public static UnitSpriteAnimator AttachHero(GameObject owner, SpriteRenderer renderer)
        {
            var animator = owner.GetComponent<UnitSpriteAnimator>();
            if (animator == null)
            {
                animator = owner.AddComponent<UnitSpriteAnimator>();
            }

            animator.Initialize(renderer, FindHeroConfig(), renderer != null ? renderer.sprite : null);
            animator.LoadGeneratedActions("hero_music_maniac", includeCast: true);
            animator.sizeMultiplier = 1.28f;
            animator.RestartIfReady();
            return animator;
        }

        public static UnitSpriteAnimator AttachMonster(GameObject owner, SpriteRenderer renderer, MonsterKind kind)
        {
            var animator = owner.GetComponent<UnitSpriteAnimator>();
            if (animator == null)
            {
                animator = owner.AddComponent<UnitSpriteAnimator>();
            }

            animator.Initialize(renderer, FindMonsterConfig(kind), renderer != null ? renderer.sprite : null);
            animator.LoadGeneratedActions(GeneratedMonsterPrefix(kind), includeCast: false);
            animator.sizeMultiplier = kind == MonsterKind.BoneKing ? 1.38f : 1.42f;
            animator.RestartIfReady();
            return animator;
        }

        public void Play(UnitAnimationAction action, bool restart = false)
        {
            if (!initialized || !actions.TryGetValue(action, out var config))
            {
                action = UnitAnimationAction.Idle;
                actions.TryGetValue(action, out config);
            }

            if (config == null || config.frames == null || config.frames.Count == 0)
            {
                return;
            }

            if (!restart && currentAction == action && currentConfig == config)
            {
                return;
            }

            currentAction = action;
            currentConfig = config;
            frameIndex = 0;
            frameTimer = 0f;
            ApplyFrame(CurrentFrame);
        }

        public void SetFacing(float horizontal)
        {
            if (horizontal < -0.05f)
            {
                facing = -1f;
            }
            else if (horizontal > 0.05f)
            {
                facing = 1f;
            }
        }

        public void Tick(Color tint, float pulse = 1f)
        {
            if (!initialized || spriteRenderer == null)
            {
                return;
            }

            if (currentConfig == null)
            {
                Play(UnitAnimationAction.Idle);
            }

            AdvanceFrame();
            spriteRenderer.color = tint;
            ApplyFrameTransform(CurrentFrame, pulse);
        }

        private void Initialize(SpriteRenderer renderer, CharacterAnimationConfig config, Sprite fallbackSprite)
        {
            spriteRenderer = renderer;
            actions.Clear();
            fallbackSize = EstimateSize(fallbackSprite);

            if (config != null && config.actions != null)
            {
                foreach (var action in config.actions)
                {
                    if (action != null && action.frames != null && action.frames.Count > 0)
                    {
                        actions[action.action] = action;
                    }
                }
            }

            if (fallbackSprite != null && !actions.ContainsKey(UnitAnimationAction.Idle))
            {
                actions[UnitAnimationAction.Idle] = BuildFallbackAction(fallbackSprite);
            }

            initialized = spriteRenderer != null && actions.Count > 0;
            RestartIfReady();
        }

        private void RestartIfReady()
        {
            initialized = spriteRenderer != null && actions.Count > 0;
            if (initialized)
            {
                Play(UnitAnimationAction.Idle, true);
            }
        }

        private void AdvanceFrame()
        {
            if (currentConfig == null || currentConfig.frames.Count <= 1)
            {
                return;
            }

            var frame = CurrentFrame;
            frameTimer += UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            var duration = frame != null && frame.duration > 0f
                ? frame.duration
                : 1f / Mathf.Max(1f, currentConfig.framesPerSecond);

            if (frameTimer < duration)
            {
                return;
            }

            frameTimer -= duration;
            var next = frameIndex + 1;
            if (next >= currentConfig.frames.Count)
            {
                next = currentConfig.loop ? 0 : currentConfig.frames.Count - 1;
            }

            if (next != frameIndex)
            {
                frameIndex = next;
                ApplyFrame(CurrentFrame);
            }
        }

        private UnitAnimationFrameConfig CurrentFrame
        {
            get
            {
                if (currentConfig == null || currentConfig.frames == null || currentConfig.frames.Count == 0)
                {
                    return null;
                }

                return currentConfig.frames[Mathf.Clamp(frameIndex, 0, currentConfig.frames.Count - 1)];
            }
        }

        private void ApplyFrame(UnitAnimationFrameConfig frame)
        {
            if (spriteRenderer == null || frame == null)
            {
                return;
            }

            if (frame.sprite != null)
            {
                spriteRenderer.sprite = frame.sprite;
            }

            ApplyFrameTransform(frame, 1f);
        }

        private void ApplyFrameTransform(UnitAnimationFrameConfig frame, float pulse)
        {
            if (spriteRenderer == null || frame == null)
            {
                return;
            }

            var size = frame.worldSize;
            if (size.x <= 0f || size.y <= 0f)
            {
                size = fallbackSize;
            }

            size *= sizeMultiplier;
            var spriteSize = spriteRenderer.sprite != null ? (Vector2)spriteRenderer.sprite.bounds.size : Vector2.one;
            var scaleX = spriteSize.x <= 0f ? size.x : size.x / spriteSize.x;
            var scaleY = spriteSize.y <= 0f ? size.y : size.y / spriteSize.y;
            spriteRenderer.transform.localScale = new Vector3(Mathf.Abs(scaleX) * facing, scaleY, 1f) * Mathf.Max(0.01f, pulse);

            var pivotOffset = new Vector2(0.5f - frame.pivot.x, 0.5f - frame.pivot.y);
            var localOffset = new Vector2(pivotOffset.x * size.x * facing, pivotOffset.y * size.y) + frame.offset;
            spriteRenderer.transform.localPosition = new Vector3(localOffset.x, localOffset.y, 0f);
        }

        private void LoadGeneratedActions(string prefix, bool includeCast)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return;
            }

            AddGeneratedAction(prefix, UnitAnimationAction.Idle, "Idle", "idle", 8, 8f, true, 0.11f);
            AddGeneratedAction(prefix, UnitAnimationAction.Move, "Move", "move", 8, 10f, true, 0.085f);
            AddGeneratedAction(prefix, UnitAnimationAction.Hit, "Hit", "hit", 4, 12f, false, 0.07f);
            AddGeneratedAction(prefix, UnitAnimationAction.Death, "Death", "death", 8, 8f, false, 0.13f);
            AddGeneratedAction(prefix, UnitAnimationAction.Attack, "Attack", "attack", 6, 12f, false, 0.075f);
            AddGeneratedAction(prefix, UnitAnimationAction.Spawn, "Spawn", "spawn", 6, 10f, false, 0.08f);
            if (includeCast)
            {
                AddGeneratedAction(prefix, UnitAnimationAction.Cast, "Cast", "cast", 6, 10f, false, 0.09f);
            }

            RestartIfReady();
        }

        private void AddGeneratedAction(string prefix, UnitAnimationAction actionType, string displayName, string key, int frameCount, float framesPerSecond, bool loop, float duration)
        {
            var action = new UnitActionAnimationConfig
            {
                action = actionType,
                displayName = displayName,
                framesPerSecond = framesPerSecond,
                loop = loop
            };

            for (var i = 0; i < frameCount; i++)
            {
                var sprite = MusicManiacArtLibrary.LoadSprite($"characters/{prefix}_{key}_{i:00}");
                if (sprite == null)
                {
                    return;
                }

                action.frames.Add(new UnitAnimationFrameConfig
                {
                    frameName = $"{displayName}{i + 1}",
                    sprite = sprite,
                    worldSize = EstimateSize(sprite),
                    pivot = new Vector2(0.5f, 0.5f),
                    duration = duration
                });
            }

            if (action.frames.Count > 0)
            {
                actions[actionType] = action;
            }
        }

        private static string GeneratedMonsterPrefix(MonsterKind kind)
        {
            switch (kind)
            {
                case MonsterKind.Skeleton:
                    return "monster_noise_blob";
                case MonsterKind.VenomBug:
                    return "monster_venom_singer";
                case MonsterKind.Archer:
                    return "monster_cassette_thrower";
                case MonsterKind.Stoneguard:
                    return "monster_speaker_brute";
                case MonsterKind.HexPriest:
                    return "monster_metronome_wizard";
                case MonsterKind.Shieldbreaker:
                    return "monster_tuning_fork_breaker";
                case MonsterKind.Assassin:
                    return "monster_cable_assassin";
                case MonsterKind.BoneKing:
                    return "monster_distortion_king";
                default:
                    return string.Empty;
            }
        }

        private static UnitActionAnimationConfig BuildFallbackAction(Sprite sprite)
        {
            var action = new UnitActionAnimationConfig
            {
                action = UnitAnimationAction.Idle,
                displayName = "Idle",
                framesPerSecond = 8f,
                loop = true
            };
            action.frames.Add(new UnitAnimationFrameConfig
            {
                frameName = "Default",
                sprite = sprite,
                worldSize = EstimateSize(sprite),
                pivot = new Vector2(0.5f, 0.5f),
                duration = 0.12f
            });
            return action;
        }

        private static Vector2 EstimateSize(Sprite sprite)
        {
            if (sprite == null)
            {
                return Vector2.one;
            }

            var size = sprite.bounds.size;
            return size.x > 0f && size.y > 0f ? (Vector2)size : Vector2.one;
        }

        private static CharacterAnimationConfig FindHeroConfig()
        {
            var database = Database;
            return database == null ? null : database.characterAnimations.Find(config => config != null && config.isHero);
        }

        private static CharacterAnimationConfig FindMonsterConfig(MonsterKind kind)
        {
            var database = Database;
            return database == null ? null : database.characterAnimations.Find(config => config != null && !config.isHero && config.monsterKind == kind);
        }

        private static MusicManiacConfigDatabase Database
        {
            get
            {
                if (cachedDatabase == null)
                {
                    cachedDatabase = Resources.Load<MusicManiacConfigDatabase>(ConfigResourcePath);
                }

                return cachedDatabase;
            }
        }
    }
}
