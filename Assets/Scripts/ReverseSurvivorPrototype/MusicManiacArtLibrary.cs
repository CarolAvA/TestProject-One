using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReverseSurvivorPrototype
{
    public static class MusicManiacArtLibrary
    {
        private const float PixelArtPixelsPerUnit = 96f;
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        public static Sprite HeroSprite => LoadSprite("characters/hero_music_maniac");

        public static Sprite MonsterSprite(MonsterKind kind)
        {
            switch (kind)
            {
                case MonsterKind.Skeleton:
                    return LoadSprite("characters/monster_noise_blob");
                case MonsterKind.VenomBug:
                    return LoadSprite("characters/monster_venom_singer");
                case MonsterKind.Archer:
                    return LoadSprite("characters/monster_cassette_thrower");
                case MonsterKind.Stoneguard:
                    return LoadSprite("characters/monster_speaker_brute");
                case MonsterKind.HexPriest:
                    return LoadSprite("characters/monster_metronome_wizard");
                case MonsterKind.Shieldbreaker:
                    return LoadSprite("characters/monster_tuning_fork_breaker");
                case MonsterKind.Assassin:
                    return LoadSprite("characters/monster_cable_assassin");
                case MonsterKind.BoneKing:
                    return LoadSprite("characters/monster_distortion_king");
                default:
                    return LoadSprite("characters/monster_noise_blob");
            }
        }

        public static Sprite ProjectileSprite(ElementModule element)
        {
            switch (element)
            {
                case ElementModule.Lightning:
                    return LoadSprite("projectiles/projectile_lightning_note");
                case ElementModule.Ice:
                    return LoadSprite("projectiles/projectile_ice_note");
                case ElementModule.Fire:
                    return LoadSprite("projectiles/projectile_fire_note");
                case ElementModule.Poison:
                    return LoadSprite("projectiles/projectile_poison_note");
                default:
                    return LoadSprite("projectiles/projectile_basic_note");
            }
        }

        public static Sprite SkillIcon(CreatorSkillId skillId)
        {
            switch (skillId)
            {
                case CreatorSkillId.LightningStrike:
                    return LoadSprite("icons/skill_lightning");
                case CreatorSkillId.FrostField:
                    return LoadSprite("icons/skill_frost_field");
                case CreatorSkillId.AntiHealCurse:
                    return LoadSprite("icons/skill_anti_heal");
                case CreatorSkillId.ShieldBrand:
                    return LoadSprite("icons/skill_shield_brand");
                case CreatorSkillId.BoneWall:
                    return LoadSprite("icons/skill_bone_wall");
                case CreatorSkillId.DemonHand:
                    return LoadSprite("icons/skill_demon_hand");
                default:
                    return LoadSprite("icons/danger_warning");
            }
        }

        public static Sprite MonsterIcon(MonsterKind kind)
        {
            return MonsterSprite(kind);
        }

        public static Sprite Tile(string name)
        {
            return LoadSprite($"tiles/{name}");
        }

        public static Sprite Ui(string name)
        {
            return LoadSprite($"ui/{name}");
        }

        public static Sprite Vfx(string name)
        {
            return LoadSprite($"vfx/{name}");
        }

        public static Sprite Icon(string name)
        {
            return LoadSprite($"icons/{name}");
        }

        public static SpriteRenderer AttachSprite(GameObject owner, Sprite sprite, float worldHeight, int sortingOrder, string childName, Color tint)
        {
            if (owner == null || sprite == null)
            {
                return null;
            }

            var child = new GameObject(childName);
            child.transform.SetParent(owner.transform, false);
            child.transform.localPosition = Vector3.zero;
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingOrder = sortingOrder;

            var height = sprite.bounds.size.y;
            var scale = height <= 0f ? 1f : worldHeight / height;
            child.transform.localScale = Vector3.one * scale;
            return renderer;
        }

        public static GameObject CreateSpriteObject(string name, Sprite sprite, Vector2 position, float worldHeight, int sortingOrder, Color tint)
        {
            var obj = new GameObject(name);
            obj.transform.position = new Vector3(position.x, position.y, 0f);
            AttachSprite(obj, sprite, worldHeight, sortingOrder, "Sprite", tint);
            return obj;
        }

        public static void ApplySpriteToImage(Image image, Sprite sprite, Color tint)
        {
            if (image == null || sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.color = tint;
            image.preserveAspect = true;
        }

        public static Sprite LoadSprite(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }

            if (spriteCache.TryGetValue(resourcePath, out var cached))
            {
                return cached;
            }

            var texture = Resources.Load<Texture2D>($"ReverseSurvivorArt/{resourcePath}");
            if (texture == null)
            {
                Debug.LogWarning($"Missing music maniac art resource: {resourcePath}");
                return null;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), PixelArtPixelsPerUnit);
            spriteCache[resourcePath] = sprite;
            return sprite;
        }
    }
}
