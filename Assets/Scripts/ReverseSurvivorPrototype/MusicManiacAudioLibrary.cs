using System.Collections.Generic;
using UnityEngine;

namespace ReverseSurvivorPrototype
{
    public enum MusicManiacAudioEvent
    {
        UiClick,
        UiConfirm,
        UiCancel,
        UiError,
        SelectMonster,
        SelectSkill,
        Summon,
        SummonInvalid,
        SkillWarning,
        SkillCast,
        SkillHit,
        SkillMiss,
        MonsterSpawn,
        MonsterAttack,
        MonsterHit,
        MonsterDeath,
        BossSpawn,
        BossDeath,
        HeroLevelUp,
        HeroDeath,
        BubbleVoice,
        RhythmHigh,
        RhythmMid,
        RhythmLow,
        RhythmWarning,
        RhythmEndLag,
        ProjectileFire,
        ProjectileHit,
        LightningBounce,
        SonicRelease,
        Victory,
        Defeat
    }

    public sealed class MusicManiacAudioSystem : MonoBehaviour
    {
        private static MusicManiacAudioSystem instance;

        private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, float> cooldowns = new Dictionary<string, float>();

        private AudioSource sfxSource;
        private AudioSource voiceSource;
        private AudioSource bgmSource;
        private AudioSource ambienceSource;
        private float lastBgmSwitchThreat = -1f;

        public static MusicManiacAudioSystem Instance
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

        public static MusicManiacAudioSystem Create()
        {
            if (instance != null)
            {
                return instance;
            }

            var audioObject = new GameObject("Music Maniac Audio System");
            instance = audioObject.AddComponent<MusicManiacAudioSystem>();
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
            sfxSource = CreateSource("SFX", 0.78f, false);
            voiceSource = CreateSource("Voice", 0.72f, false);
            bgmSource = CreateSource("BGM", 0.34f, true);
            ambienceSource = CreateSource("Ambience", 0.22f, true);
            PlayBgm("Audio/MusicManiac/BGM/bgm_battle_low_noise_walk");
            PlayAmbience("Audio/MusicManiac/Ambience/amb_livehouse_loop");
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
            var keys = new List<string>(cooldowns.Keys);
            foreach (var key in keys)
            {
                cooldowns[key] = Mathf.Max(0f, cooldowns[key] - Time.unscaledDeltaTime);
            }

            var director = GameDirector.Instance;
            if (director == null || director.Hero == null)
            {
                return;
            }

            var level = director.Hero.Level;
            var threat = director.Threat;
            var bucket = level >= 5 ? 1f : 0f;
            if (threat >= 0.7f)
            {
                bucket = 2f;
            }

            if (Mathf.Abs(bucket - lastBgmSwitchThreat) > 0.1f)
            {
                lastBgmSwitchThreat = bucket;
                if (bucket >= 2f)
                {
                    PlayBgm("Audio/MusicManiac/BGM/bgm_battle_high_rhythm_riot");
                }
                else if (bucket >= 1f)
                {
                    PlayBgm("Audio/MusicManiac/BGM/bgm_battle_mid_manic_beat");
                }
                else
                {
                    PlayBgm("Audio/MusicManiac/BGM/bgm_battle_low_noise_walk");
                }
            }
        }

        public void Play(MusicManiacAudioEvent eventId, Vector2 position, float volume = 1f)
        {
            var path = PathForEvent(eventId);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            PlayClip(path, SourceForEvent(eventId), volume * VolumeForEvent(eventId), CooldownForEvent(eventId), PitchMin(eventId), PitchMax(eventId));
        }

        public void PlayMonster(MonsterKind kind, string action, Vector2 position, float volume = 1f)
        {
            var path = MonsterPath(kind, action);
            PlayClip(path, sfxSource, volume * 0.82f, action == "move" ? 0.22f : 0.08f, 0.92f, 1.08f);
        }

        public void PlaySkill(CreatorSkillId skillId, string action, Vector2 position, float volume = 1f)
        {
            var path = SkillPath(skillId, action);
            PlayClip(path, sfxSource, volume * 0.92f, action == "warning" ? 0.55f : 0.12f, 0.96f, 1.04f);
        }

        public void PlayProjectile(ElementModule element, string action, Vector2 position, float volume = 1f)
        {
            var path = ProjectilePath(element, action);
            PlayClip(path, sfxSource, volume * 0.72f, action == "fire" ? 0.045f : 0.08f, 0.96f, 1.06f);
        }

        public void PlayHit(DamageFeedbackType type, bool strong, Vector2 position, float volume = 1f)
        {
            var path = HitPath(type, strong);
            PlayClip(path, sfxSource, volume * (strong ? 0.88f : 0.58f), strong ? 0.12f : 0.07f, 0.96f, 1.04f);
        }

        public void PlayRhythm(RhythmPitch pitch, float strength)
        {
            switch (pitch)
            {
                case RhythmPitch.High:
                    Play(MusicManiacAudioEvent.RhythmHigh, Vector2.zero, Mathf.Clamp01(0.55f + strength * 0.18f));
                    break;
                case RhythmPitch.Low:
                    Play(MusicManiacAudioEvent.RhythmLow, Vector2.zero, Mathf.Clamp01(0.7f + strength * 0.16f));
                    break;
                default:
                    Play(MusicManiacAudioEvent.RhythmMid, Vector2.zero, Mathf.Clamp01(0.55f + strength * 0.16f));
                    break;
            }
        }

        public void PlayResult(bool victory)
        {
            PlayBgm(victory ? "Audio/MusicManiac/BGM/bgm_result_victory_mocking_fanfare" : "Audio/MusicManiac/BGM/bgm_result_defeat_ai_laugh", false);
            Play(victory ? MusicManiacAudioEvent.Victory : MusicManiacAudioEvent.Defeat, Vector2.zero, 1f);
        }

        private AudioSource CreateSource(string sourceName, float volume, bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            source.loop = loop;
            return source;
        }

        private void PlayClip(string resourcesPath, AudioSource source, float volume, float cooldown, float pitchMin, float pitchMax)
        {
            if (source == null || string.IsNullOrEmpty(resourcesPath))
            {
                return;
            }

            if (cooldown > 0f && cooldowns.TryGetValue(resourcesPath, out var remaining) && remaining > 0f)
            {
                return;
            }

            var clip = LoadClip(resourcesPath);
            if (clip == null)
            {
                return;
            }

            cooldowns[resourcesPath] = cooldown;
            source.pitch = Random.Range(pitchMin, pitchMax);
            source.PlayOneShot(clip, volume);
        }

        private void PlayBgm(string resourcesPath, bool loop = true)
        {
            var clip = LoadClip(resourcesPath);
            if (clip == null || bgmSource == null || bgmSource.clip == clip)
            {
                return;
            }

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.pitch = 1f;
            bgmSource.Play();
        }

        private void PlayAmbience(string resourcesPath)
        {
            var clip = LoadClip(resourcesPath);
            if (clip == null || ambienceSource == null)
            {
                return;
            }

            ambienceSource.Stop();
            ambienceSource.clip = clip;
            ambienceSource.loop = true;
            ambienceSource.pitch = 1f;
            ambienceSource.Play();
        }

        private AudioClip LoadClip(string resourcesPath)
        {
            if (clipCache.TryGetValue(resourcesPath, out var cached))
            {
                return cached;
            }

            var clip = Resources.Load<AudioClip>(resourcesPath);
            if (clip == null)
            {
                Debug.LogWarning($"Missing Music Maniac audio resource: {resourcesPath}");
                return null;
            }

            clipCache[resourcesPath] = clip;
            return clip;
        }

        private static AudioSource SourceForEvent(MusicManiacAudioEvent eventId)
        {
            var system = Instance;
            switch (eventId)
            {
                case MusicManiacAudioEvent.BubbleVoice:
                case MusicManiacAudioEvent.HeroDeath:
                case MusicManiacAudioEvent.HeroLevelUp:
                    return system.voiceSource;
                default:
                    return system.sfxSource;
            }
        }

        private static string PathForEvent(MusicManiacAudioEvent eventId)
        {
            switch (eventId)
            {
                case MusicManiacAudioEvent.UiClick:
                    return "Audio/MusicManiac/SFX/UI/sfx_ui_click_01";
                case MusicManiacAudioEvent.UiConfirm:
                    return "Audio/MusicManiac/SFX/UI/sfx_ui_confirm_01";
                case MusicManiacAudioEvent.UiCancel:
                    return "Audio/MusicManiac/SFX/UI/sfx_ui_cancel_01";
                case MusicManiacAudioEvent.UiError:
                    return "Audio/MusicManiac/SFX/UI/sfx_ui_error_01";
                case MusicManiacAudioEvent.SelectMonster:
                    return "Audio/MusicManiac/SFX/UI/sfx_ui_select_monster_01";
                case MusicManiacAudioEvent.SelectSkill:
                    return "Audio/MusicManiac/SFX/Creator/sfx_creator_skill_select_01";
                case MusicManiacAudioEvent.Summon:
                    return "Audio/MusicManiac/SFX/UI/sfx_ui_summon_confirm_01";
                case MusicManiacAudioEvent.SummonInvalid:
                    return "Audio/MusicManiac/SFX/UI/sfx_ui_summon_invalid_01";
                case MusicManiacAudioEvent.SkillWarning:
                    return "Audio/MusicManiac/SFX/Creator/sfx_creator_skill_aim_01";
                case MusicManiacAudioEvent.SkillCast:
                    return "Audio/MusicManiac/SFX/Creator/sfx_creator_skill_select_01";
                case MusicManiacAudioEvent.SkillHit:
                    return "Audio/MusicManiac/SFX/Hit/sfx_hit_crit_01";
                case MusicManiacAudioEvent.SkillMiss:
                    return "Audio/MusicManiac/SFX/Hit/sfx_hit_dodge_01";
                case MusicManiacAudioEvent.MonsterSpawn:
                    return "Audio/MusicManiac/SFX/Monster/sfx_mon_noiseblob_spawn_01";
                case MusicManiacAudioEvent.MonsterAttack:
                    return "Audio/MusicManiac/SFX/Monster/sfx_mon_noiseblob_attack_01";
                case MusicManiacAudioEvent.MonsterHit:
                    return "Audio/MusicManiac/SFX/Monster/sfx_mon_noiseblob_hit_01";
                case MusicManiacAudioEvent.MonsterDeath:
                    return "Audio/MusicManiac/SFX/Monster/sfx_mon_noiseblob_death_01";
                case MusicManiacAudioEvent.BossSpawn:
                    return "Audio/MusicManiac/SFX/Boss/sfx_boss_distortionking_intro_01";
                case MusicManiacAudioEvent.BossDeath:
                    return "Audio/MusicManiac/SFX/Boss/sfx_boss_distortionking_death_01";
                case MusicManiacAudioEvent.HeroLevelUp:
                    return "Audio/MusicManiac/SFX/Voice/sfx_voice_ai_levelup_01";
                case MusicManiacAudioEvent.HeroDeath:
                    return "Audio/MusicManiac/SFX/Voice/sfx_voice_ai_deathvoice_01";
                case MusicManiacAudioEvent.BubbleVoice:
                    return "Audio/MusicManiac/SFX/Voice/sfx_voice_ai_taunt_01";
                case MusicManiacAudioEvent.RhythmHigh:
                    return "Audio/MusicManiac/SFX/AI/sfx_rhythm_high_tick_01";
                case MusicManiacAudioEvent.RhythmMid:
                    return "Audio/MusicManiac/SFX/AI/sfx_rhythm_mid_tap_01";
                case MusicManiacAudioEvent.RhythmLow:
                    return "Audio/MusicManiac/SFX/AI/sfx_rhythm_low_boom_01";
                case MusicManiacAudioEvent.RhythmWarning:
                    return "Audio/MusicManiac/SFX/AI/sfx_rhythm_warning_rise_01";
                case MusicManiacAudioEvent.RhythmEndLag:
                    return "Audio/MusicManiac/SFX/AI/sfx_rhythm_endlag_soft_01";
                case MusicManiacAudioEvent.ProjectileFire:
                    return "Audio/MusicManiac/SFX/AI/sfx_ai_bullet_base_fire_01";
                case MusicManiacAudioEvent.ProjectileHit:
                    return "Audio/MusicManiac/SFX/AI/sfx_ai_bullet_base_hit_01";
                case MusicManiacAudioEvent.LightningBounce:
                    return "Audio/MusicManiac/SFX/AI/sfx_ai_lightning_bounce_01";
                case MusicManiacAudioEvent.SonicRelease:
                    return "Audio/MusicManiac/SFX/AI/sfx_ai_sonic_release_01";
                case MusicManiacAudioEvent.Victory:
                    return "Audio/MusicManiac/SFX/Result/sfx_result_victory_01";
                case MusicManiacAudioEvent.Defeat:
                    return "Audio/MusicManiac/SFX/Result/sfx_result_defeat_01";
                default:
                    return string.Empty;
            }
        }

        private static string MonsterPath(MonsterKind kind, string action)
        {
            var prefix = "sfx_mon_noiseblob";
            switch (kind)
            {
                case MonsterKind.VenomBug:
                    prefix = "sfx_mon_poisonsinger";
                    break;
                case MonsterKind.Archer:
                    prefix = "sfx_mon_cassette";
                    break;
                case MonsterKind.Stoneguard:
                    prefix = "sfx_mon_speakerbrute";
                    break;
                case MonsterKind.HexPriest:
                    prefix = "sfx_mon_metromage";
                    break;
                case MonsterKind.Shieldbreaker:
                    prefix = "sfx_mon_tuninghammer";
                    break;
                case MonsterKind.Assassin:
                    prefix = "sfx_mon_cablestalker";
                    break;
                case MonsterKind.BoneKing:
                    if (action == "spawn") return "Audio/MusicManiac/SFX/Boss/sfx_boss_distortionking_intro_01";
                    if (action == "death") return "Audio/MusicManiac/SFX/Boss/sfx_boss_distortionking_death_01";
                    if (action == "attack") return "Audio/MusicManiac/SFX/Boss/sfx_boss_distortionking_slam_01";
                    return "Audio/MusicManiac/SFX/Boss/sfx_boss_distortionking_scream_01";
            }

            var suffix = action == "spawn" ? "spawn" : action == "death" ? "death" : action == "hit" ? "hit" : action == "move" ? "move" : "attack";
            if (kind == MonsterKind.Stoneguard && action == "move") suffix = "step";
            if (kind == MonsterKind.VenomBug && action == "move") suffix = "idle";
            if (kind == MonsterKind.HexPriest && action == "move") suffix = "idle_tick";
            if (kind == MonsterKind.HexPriest && action == "attack") suffix = "cast";
            return $"Audio/MusicManiac/SFX/Monster/{prefix}_{suffix}_01";
        }

        private static string SkillPath(CreatorSkillId skillId, string action)
        {
            switch (skillId)
            {
                case CreatorSkillId.LightningStrike:
                    return $"Audio/MusicManiac/SFX/Creator/sfx_creator_thunder_{SkillSuffix(action)}_01";
                case CreatorSkillId.FrostField:
                    return $"Audio/MusicManiac/SFX/Creator/sfx_creator_icecircle_{SkillSuffix(action)}_01";
                case CreatorSkillId.AntiHealCurse:
                    return $"Audio/MusicManiac/SFX/Creator/sfx_creator_healblock_{SkillSuffix(action)}_01";
                case CreatorSkillId.ShieldBrand:
                    return $"Audio/MusicManiac/SFX/Creator/sfx_creator_shieldbreak_{SkillSuffix(action)}_01";
                case CreatorSkillId.BoneWall:
                    return $"Audio/MusicManiac/SFX/Creator/sfx_creator_spiketrap_{SkillSuffix(action)}_01";
                case CreatorSkillId.DemonHand:
                    return action == "warning"
                        ? "Audio/MusicManiac/SFX/Creator/sfx_creator_noiselock_warning_01"
                        : "Audio/MusicManiac/SFX/Boss/sfx_boss_djtyrant_drop_01";
                default:
                    return "Audio/MusicManiac/SFX/Creator/sfx_creator_skill_select_01";
            }
        }

        private static string SkillSuffix(string action)
        {
            if (action == "hit") return "hit";
            if (action == "warning") return "warning";
            return "cast";
        }

        private static string ProjectilePath(ElementModule element, string action)
        {
            var suffix = action == "hit" ? "hit" : "fire";
            switch (element)
            {
                case ElementModule.Lightning:
                    return $"Audio/MusicManiac/SFX/AI/sfx_ai_lightning_{suffix}_01";
                case ElementModule.Ice:
                    return suffix == "hit" ? "Audio/MusicManiac/SFX/AI/sfx_ai_ice_hit_crack_01" : "Audio/MusicManiac/SFX/AI/sfx_ai_ice_fire_01";
                case ElementModule.Fire:
                    return suffix == "hit" ? "Audio/MusicManiac/SFX/AI/sfx_ai_fireball_hit_01" : "Audio/MusicManiac/SFX/AI/sfx_ai_fireball_fire_01";
                case ElementModule.Poison:
                    return $"Audio/MusicManiac/SFX/AI/sfx_ai_poison_{suffix}_01";
                default:
                    return $"Audio/MusicManiac/SFX/AI/sfx_ai_bullet_base_{suffix}_01";
            }
        }

        private static string HitPath(DamageFeedbackType type, bool strong)
        {
            switch (type)
            {
                case DamageFeedbackType.Fire:
                    return strong ? "Audio/MusicManiac/SFX/Hit/sfx_hit_fire_heavy_01" : "Audio/MusicManiac/SFX/Hit/sfx_hit_fire_light_01";
                case DamageFeedbackType.Ice:
                    return strong ? "Audio/MusicManiac/SFX/Hit/sfx_hit_ice_heavy_01" : "Audio/MusicManiac/SFX/Hit/sfx_hit_ice_light_01";
                case DamageFeedbackType.Lightning:
                    return strong ? "Audio/MusicManiac/SFX/Hit/sfx_hit_lightning_heavy_01" : "Audio/MusicManiac/SFX/Hit/sfx_hit_lightning_light_01";
                case DamageFeedbackType.Poison:
                    return strong ? "Audio/MusicManiac/SFX/Hit/sfx_hit_poison_heavy_01" : "Audio/MusicManiac/SFX/Hit/sfx_hit_poison_light_01";
                case DamageFeedbackType.Sonic:
                    return strong ? "Audio/MusicManiac/SFX/Hit/sfx_hit_sonic_heavy_01" : "Audio/MusicManiac/SFX/Hit/sfx_hit_sonic_light_01";
                case DamageFeedbackType.ShieldBreak:
                    return "Audio/MusicManiac/SFX/Hit/sfx_hit_shieldbreak_01";
                default:
                    return strong ? "Audio/MusicManiac/SFX/Hit/sfx_hit_physical_heavy_01" : "Audio/MusicManiac/SFX/Hit/sfx_hit_physical_light_01";
            }
        }

        private static float VolumeForEvent(MusicManiacAudioEvent eventId)
        {
            switch (eventId)
            {
                case MusicManiacAudioEvent.RhythmHigh:
                case MusicManiacAudioEvent.RhythmMid:
                    return 0.32f;
                case MusicManiacAudioEvent.RhythmLow:
                case MusicManiacAudioEvent.SonicRelease:
                    return 0.7f;
                case MusicManiacAudioEvent.BubbleVoice:
                    return 0.48f;
                default:
                    return 0.72f;
            }
        }

        private static float CooldownForEvent(MusicManiacAudioEvent eventId)
        {
            switch (eventId)
            {
                case MusicManiacAudioEvent.RhythmHigh:
                case MusicManiacAudioEvent.ProjectileFire:
                    return 0.035f;
                case MusicManiacAudioEvent.RhythmMid:
                    return 0.05f;
                case MusicManiacAudioEvent.RhythmLow:
                    return 0.16f;
                case MusicManiacAudioEvent.BubbleVoice:
                    return 1.5f;
                default:
                    return 0.08f;
            }
        }

        private static float PitchMin(MusicManiacAudioEvent eventId)
        {
            return eventId == MusicManiacAudioEvent.RhythmLow ? 0.98f : 0.94f;
        }

        private static float PitchMax(MusicManiacAudioEvent eventId)
        {
            return eventId == MusicManiacAudioEvent.RhythmLow ? 1.02f : 1.08f;
        }
    }
}

