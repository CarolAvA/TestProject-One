using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace ReverseSurvivorPrototype
{
    public enum FeelImpactLevel
    {
        Micro,
        Light,
        Medium,
        Heavy,
        Ultimate
    }

    public enum FeelImpactEvent
    {
        Hit,
        CriticalHit,
        HeroHit,
        CreatorSkillCast,
        CreatorSkillHit,
        MonsterSpawn,
        MonsterDeath,
        BossSpawn,
        BossDeath,
        GameStart,
        GameEnd,
        LevelUp
    }

    public sealed class FeelImpactSystem : MonoBehaviour
    {
        private const int Channel = 0;
        private const int FlashId = 0;

        private static FeelImpactSystem instance;

        private readonly Dictionary<FeelImpactLevel, float> levelCooldowns = new Dictionary<FeelImpactLevel, float>();
        private readonly Dictionary<FeelImpactEvent, float> eventCooldowns = new Dictionary<FeelImpactEvent, float>();

        private Camera targetCamera;
        private Vector3 cameraBaseLocalPosition;
        private float shakeTimer;
        private float shakeDuration;
        private float shakeAmplitude;
        private float shakeFrequency;
        private float shakeX;
        private float shakeY;
        private float shakeZ;
        private float postTimer;
        private float postDuration;
        private float postIntensity;
        private float postLensIntensity;
        private float flashTimer;
        private float flashDuration;
        private float flashAlpha;
        private Image flashImage;
        private Bloom bloom;
        private ChromaticAberration chromaticAberration;
        private LensDistortion lensDistortion;
        private Vignette vignette;
        private float postProcessCooldown;
        private float flashCooldown;

        public static FeelImpactSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    Create(Camera.main);
                }

                return instance;
            }
        }

        public static FeelImpactSystem Create(Camera camera)
        {
            if (instance != null)
            {
                if (camera != null)
                {
                    instance.AttachToCamera(camera);
                }

                return instance;
            }

            var impactObject = new GameObject("Feel Impact System");
            instance = impactObject.AddComponent<FeelImpactSystem>();
            instance.AttachToCamera(camera);
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
            TickDictionary(levelCooldowns);
            TickDictionary(eventCooldowns);
            postProcessCooldown = Mathf.Max(0f, postProcessCooldown - Time.unscaledDeltaTime);
            flashCooldown = Mathf.Max(0f, flashCooldown - Time.unscaledDeltaTime);
            UpdateCameraShake();
            UpdateFlash();
            UpdatePostProcess();
        }

        public void Play(FeelImpactEvent impactEvent, FeelImpactLevel level, Vector2 worldPosition, Color accent)
        {
            var useLensDistortion = ShouldUseLensDistortion(impactEvent);
            if (!useLensDistortion)
            {
                ClearLensDistortion();
                DisableLensDistortionShakers();
            }

            if (!CanPlay(impactEvent, level))
            {
                return;
            }

            var profile = GetProfile(level);
            levelCooldowns[level] = profile.LevelCooldown;
            eventCooldowns[impactEvent] = profile.EventCooldown;

            TriggerCameraShake(profile);
            if (profile.FlashAlpha > 0f && flashCooldown <= 0f)
            {
                flashCooldown = profile.FlashCooldown;
                TriggerFlash(Color.Lerp(accent, Color.white, 0.42f), profile.FlashDuration, profile.FlashAlpha);
            }

            if (profile.PostIntensity > 0f && postProcessCooldown <= 0f)
            {
                postProcessCooldown = profile.PostCooldown;
                TriggerPostProcess(profile, useLensDistortion);
            }
            else if (!useLensDistortion)
            {
                ClearLensDistortion();
            }

            CreateImpactAccent(impactEvent, level, worldPosition, accent);
        }

        public void SetCameraBaseLocalPosition(Vector3 position)
        {
            cameraBaseLocalPosition = position;
            if (targetCamera != null && shakeTimer <= 0f)
            {
                targetCamera.transform.localPosition = position;
            }
        }

        public static FeelImpactLevel LevelForMonster(MonsterConfig config, bool isDeath)
        {
            if (config.Kind == MonsterKind.BoneKing)
            {
                return isDeath ? FeelImpactLevel.Ultimate : FeelImpactLevel.Heavy;
            }

            if (config.Kind == MonsterKind.Stoneguard || config.Kind == MonsterKind.Assassin || config.Kind == MonsterKind.Shieldbreaker)
            {
                return isDeath ? FeelImpactLevel.Medium : FeelImpactLevel.Light;
            }

            return isDeath ? FeelImpactLevel.Light : FeelImpactLevel.Micro;
        }

        public static FeelImpactLevel LevelForSkill(CreatorSkillConfig config, bool onHit)
        {
            if (config.Id == CreatorSkillId.DemonHand)
            {
                return onHit ? FeelImpactLevel.Ultimate : FeelImpactLevel.Heavy;
            }

            if (config.Danger >= 0.75f || config.Cost >= 140f)
            {
                return onHit ? FeelImpactLevel.Heavy : FeelImpactLevel.Medium;
            }

            return onHit ? FeelImpactLevel.Medium : FeelImpactLevel.Light;
        }

        private void AttachToCamera(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            targetCamera = camera;
            cameraBaseLocalPosition = camera.transform.localPosition;
            DisableBrokenCameraShaker(camera);
            EnsurePostProcessing(camera);
            EnsureFlashCanvas();
        }

        private static void DisableBrokenCameraShaker(Camera camera)
        {
            var shaker = camera.GetComponent<MMCameraShaker>();
            if (shaker != null)
            {
                shaker.enabled = false;
            }

            var wiggle = camera.GetComponent<MMWiggle>();
            if (wiggle != null)
            {
                wiggle.enabled = false;
            }
        }

        private void EnsurePostProcessing(Camera camera)
        {
            var additionalCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
            if (additionalCameraData == null)
            {
                additionalCameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            additionalCameraData.renderPostProcessing = true;

            var volumeObject = GameObject.Find("Runtime Feel URP Volume");
            if (volumeObject == null)
            {
                volumeObject = new GameObject("Runtime Feel URP Volume");
            }

            var volume = volumeObject.GetComponent<Volume>();
            if (volume == null)
            {
                volume = volumeObject.AddComponent<Volume>();
            }

            volume.isGlobal = true;
            volume.priority = 80f;
            if (volume.profile == null)
            {
                volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
                volume.profile.name = "Runtime Feel Volume Profile";
            }

            bloom = EnsureVolumeComponent<Bloom>(volume.profile);
            chromaticAberration = EnsureVolumeComponent<ChromaticAberration>(volume.profile);
            lensDistortion = EnsureVolumeComponent<LensDistortion>(volume.profile);
            vignette = EnsureVolumeComponent<Vignette>(volume.profile);

            bloom.intensity.Override(0.05f);
            chromaticAberration.intensity.Override(0f);
            lensDistortion.intensity.Override(0f);
            vignette.intensity.Override(0.12f);

            EnsureShaker<MMBloomShaker_URP>(volumeObject);
            EnsureShaker<MMChromaticAberrationShaker_URP>(volumeObject);
            EnsureShaker<MMVignetteShaker_URP>(volumeObject);
            DisableLensDistortionShakers();
        }

        private static T EnsureVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component))
            {
                component = profile.Add<T>(true);
            }

            component.active = true;
            return component;
        }

        private static void EnsureShaker<T>(GameObject owner) where T : MMShaker
        {
            var shaker = owner.GetComponent<T>();
            if (shaker == null)
            {
                shaker = owner.AddComponent<T>();
            }

            shaker.ChannelMode = MMChannelModes.Int;
            shaker.Channel = Channel;
            shaker.Interruptible = true;
            shaker.AlwaysResetTargetValuesAfterShake = true;
            shaker.CooldownBetweenShakes = 0f;
            if (!shaker.ListeningToEvents)
            {
                shaker.StartListening();
            }
        }

        private static void DisableLensDistortionShakers()
        {
            foreach (var shaker in FindObjectsByType<MMLensDistortionShaker_URP>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (shaker.ListeningToEvents)
                {
                    shaker.StopListening();
                }

                shaker.enabled = false;
                Destroy(shaker);
            }
        }

        private void EnsureFlashCanvas()
        {
            var existing = GameObject.Find("Runtime Feel Flash");
            if (existing != null)
            {
                flashImage = existing.GetComponent<Image>();
                return;
            }

            var canvasObject = new GameObject("Runtime Feel Flash Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 2500;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var flashObject = new GameObject("Runtime Feel Flash");
            flashObject.transform.SetParent(canvasObject.transform, false);
            var image = flashObject.AddComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = false;
            flashImage = image;
            var rect = flashObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var flash = flashObject.AddComponent<MMFlash>();
            flash.ChannelMode = MMChannelModes.Int;
            flash.Channel = Channel;
            flash.FlashID = FlashId;
            flash.Interruptable = true;
            flashObject.SetActive(true);
        }

        private bool CanPlay(FeelImpactEvent impactEvent, FeelImpactLevel level)
        {
            if (eventCooldowns.TryGetValue(impactEvent, out var eventCooldown) && eventCooldown > 0f)
            {
                return false;
            }

            if (levelCooldowns.TryGetValue(level, out var levelCooldown) && levelCooldown > 0f)
            {
                return false;
            }

            return true;
        }

        private static ImpactProfile GetProfile(FeelImpactLevel level)
        {
            switch (level)
            {
                case FeelImpactLevel.Micro:
                    return new ImpactProfile(0.045f, 0.035f, 13f, 0.018f, 0.014f, 0f, 0.02f, 0.08f, 0f, 0.08f, 0.18f, 0f, 0.08f);
                case FeelImpactLevel.Light:
                    return new ImpactProfile(0.075f, 0.07f, 15f, 0.035f, 0.027f, 0f, 0.055f, 0.11f, 0.08f, 0.2f, 0.26f, 0.25f, 0.16f);
                case FeelImpactLevel.Medium:
                    return new ImpactProfile(0.12f, 0.13f, 18f, 0.075f, 0.055f, 0f, 0.12f, 0.14f, 0.18f, 0.34f, 0.38f, 0.42f, 0.24f);
                case FeelImpactLevel.Heavy:
                    return new ImpactProfile(0.18f, 0.24f, 20f, 0.13f, 0.1f, 0.02f, 0.2f, 0.18f, 0.32f, 0.56f, 0.52f, 0.62f, 0.34f);
                case FeelImpactLevel.Ultimate:
                    return new ImpactProfile(0.28f, 0.38f, 24f, 0.22f, 0.17f, 0.035f, 0.28f, 0.24f, 0.5f, 0.85f, 0.75f, 0.9f, 0.48f);
                default:
                    return default;
            }
        }

        private void TriggerCameraShake(ImpactProfile profile)
        {
            if (profile.ShakeAmplitude <= 0f)
            {
                return;
            }

            shakeTimer = Mathf.Max(shakeTimer, profile.ShakeDuration);
            shakeDuration = Mathf.Max(0.01f, profile.ShakeDuration);
            shakeAmplitude = Mathf.Max(shakeAmplitude, profile.ShakeAmplitude);
            shakeFrequency = profile.ShakeFrequency;
            shakeX = profile.ShakeX;
            shakeY = profile.ShakeY;
            shakeZ = profile.ShakeZ;
        }

        private void TriggerFlash(Color color, float duration, float alpha)
        {
            flashTimer = Mathf.Max(flashTimer, duration);
            flashDuration = Mathf.Max(0.01f, duration);
            flashAlpha = Mathf.Max(flashAlpha, alpha);
            if (flashImage != null)
            {
                var flashColor = color;
                flashColor.a = alpha;
                flashImage.color = flashColor;
            }
        }

        private static bool ShouldUseLensDistortion(FeelImpactEvent impactEvent)
        {
            return impactEvent == FeelImpactEvent.GameStart || impactEvent == FeelImpactEvent.GameEnd;
        }

        private void TriggerPostProcess(ImpactProfile profile, bool useLensDistortion)
        {
            postTimer = Mathf.Max(postTimer, profile.PostDuration);
            postDuration = Mathf.Max(0.01f, profile.PostDuration);
            postIntensity = Mathf.Max(postIntensity, profile.PostIntensity);
            if (useLensDistortion)
            {
                postLensIntensity = Mathf.Max(postLensIntensity, profile.PostIntensity);
            }
            else
            {
                ClearLensDistortion();
            }

            var pulse = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.18f, 1f), new Keyframe(1f, 0f));

            try
            {
                MMChromaticAberrationShakeEvent_URP.Trigger(pulse, profile.PostDuration, 0f, profile.PostIntensity, false, 1f, ChannelData(), true, true, true, TimescaleModes.Unscaled);
                MMBloomShakeEvent_URP.Trigger(pulse, profile.PostDuration, 0f, profile.PostIntensity * 1.8f, pulse, 0f, -0.25f, true, 1f, ChannelData(), true, true, true, TimescaleModes.Unscaled);
                MMVignetteShakeEvent_URP.Trigger(pulse, profile.PostDuration, 0.12f, 0.12f + profile.PostIntensity * 0.24f, false, 1f, ChannelData(), true, true, true, TimescaleModes.Unscaled);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Feel URP shaker fallback active: {exception.Message}");
            }
        }

        private void UpdateCameraShake()
        {
            if (targetCamera == null)
            {
                return;
            }

            if (shakeTimer <= 0f)
            {
                targetCamera.transform.localPosition = Vector3.Lerp(targetCamera.transform.localPosition, cameraBaseLocalPosition, Time.unscaledDeltaTime * 18f);
                shakeAmplitude = 0f;
                return;
            }

            shakeTimer -= Time.unscaledDeltaTime;
            var progress = 1f - Mathf.Clamp01(shakeTimer / Mathf.Max(0.01f, shakeDuration));
            var falloff = 1f - progress;
            var time = Time.unscaledTime * shakeFrequency;
            var offset = new Vector3(
                Mathf.Sin(time * 1.37f) * (shakeX > 0f ? shakeX : shakeAmplitude),
                Mathf.Cos(time * 1.71f) * (shakeY > 0f ? shakeY : shakeAmplitude),
                Mathf.Sin(time * 2.11f) * shakeZ) * falloff;
            targetCamera.transform.localPosition = cameraBaseLocalPosition + offset;
        }

        private void UpdateFlash()
        {
            if (flashImage == null)
            {
                return;
            }

            if (flashTimer <= 0f)
            {
                var color = flashImage.color;
                color.a = 0f;
                flashImage.color = color;
                flashAlpha = 0f;
                return;
            }

            flashTimer -= Time.unscaledDeltaTime;
            var alpha = flashAlpha * Mathf.Clamp01(flashTimer / Mathf.Max(0.01f, flashDuration));
            var current = flashImage.color;
            current.a = alpha;
            flashImage.color = current;
        }

        private void UpdatePostProcess()
        {
            if (postTimer <= 0f)
            {
                ResetPostProcess();
                return;
            }

            postTimer -= Time.unscaledDeltaTime;
            var progress = 1f - Mathf.Clamp01(postTimer / Mathf.Max(0.01f, postDuration));
            var pulse = Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI);
            var wobble = Mathf.Sin(progress * Mathf.PI * 2.2f) * pulse;
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.Override(postIntensity * pulse);
            }

            if (lensDistortion != null)
            {
                lensDistortion.intensity.Override(postLensIntensity * 8f * wobble);
            }

            if (bloom != null)
            {
                bloom.intensity.Override(0.05f + postIntensity * 1.8f * pulse);
            }

            if (vignette != null)
            {
                vignette.intensity.Override(0.12f + postIntensity * 0.24f * pulse);
            }
        }

        private void ResetPostProcess()
        {
            postIntensity = 0f;
            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.Override(0f);
            }

            ClearLensDistortion();

            if (bloom != null)
            {
                bloom.intensity.Override(0.05f);
            }

            if (vignette != null)
            {
                vignette.intensity.Override(0.12f);
            }
        }

        private void ClearLensDistortion()
        {
            postLensIntensity = 0f;
            if (lensDistortion != null)
            {
                lensDistortion.intensity.Override(0f);
            }
        }

        private static void CreateImpactAccent(FeelImpactEvent impactEvent, FeelImpactLevel level, Vector2 worldPosition, Color accent)
        {
            if (level == FeelImpactLevel.Micro)
            {
                return;
            }

            var radius = level == FeelImpactLevel.Ultimate ? 1.65f : level == FeelImpactLevel.Heavy ? 1.2f : level == FeelImpactLevel.Medium ? 0.86f : 0.52f;
            var lifetime = level == FeelImpactLevel.Ultimate ? 0.46f : level == FeelImpactLevel.Heavy ? 0.34f : 0.24f;
            VisualFactory.CreatePulseRing(worldPosition, radius, Color.Lerp(accent, Color.white, 0.18f), lifetime);

            if (impactEvent == FeelImpactEvent.MonsterSpawn || impactEvent == FeelImpactEvent.CreatorSkillCast)
            {
                VisualFactory.CreatePulseRing(worldPosition, radius * 0.62f, accent, lifetime * 0.72f);
            }
        }

        private static MMChannelData ChannelData()
        {
            return new MMChannelData(MMChannelModes.Int, Channel, null);
        }

        private static void TickDictionary<T>(Dictionary<T, float> dictionary)
        {
            var keys = new List<T>(dictionary.Keys);
            foreach (var key in keys)
            {
                dictionary[key] = Mathf.Max(0f, dictionary[key] - Time.unscaledDeltaTime);
            }
        }

        private readonly struct ImpactProfile
        {
            public ImpactProfile(float shakeDuration, float shakeAmplitude, float shakeFrequency, float shakeX, float shakeY, float shakeZ, float flashAlpha, float flashDuration, float postIntensity, float postDuration, float levelCooldown, float postCooldown, float eventCooldown)
            {
                ShakeDuration = shakeDuration;
                ShakeAmplitude = shakeAmplitude;
                ShakeFrequency = shakeFrequency;
                ShakeX = shakeX;
                ShakeY = shakeY;
                ShakeZ = shakeZ;
                FlashAlpha = flashAlpha;
                FlashDuration = flashDuration;
                PostIntensity = postIntensity;
                PostDuration = postDuration;
                LevelCooldown = levelCooldown;
                PostCooldown = postCooldown;
                EventCooldown = eventCooldown;
                FlashCooldown = Mathf.Max(0.08f, flashDuration * 0.85f);
            }

            public float ShakeDuration { get; }
            public float ShakeAmplitude { get; }
            public float ShakeFrequency { get; }
            public float ShakeX { get; }
            public float ShakeY { get; }
            public float ShakeZ { get; }
            public float FlashAlpha { get; }
            public float FlashDuration { get; }
            public float PostIntensity { get; }
            public float PostDuration { get; }
            public float LevelCooldown { get; }
            public float PostCooldown { get; }
            public float EventCooldown { get; }
            public float FlashCooldown { get; }
        }
    }
}
