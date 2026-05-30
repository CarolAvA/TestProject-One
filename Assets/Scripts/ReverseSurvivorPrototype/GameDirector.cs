using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReverseSurvivorPrototype
{
    public enum MonsterKind
    {
        Skeleton,
        VenomBug,
        Archer,
        Stoneguard,
        HexPriest,
        Shieldbreaker,
        Assassin,
        BoneKing
    }

    public enum HeroBuild
    {
        FlameAura,
        Lifesteal,
        ShieldWall
    }

    public sealed class GameDirector : MonoBehaviour
    {
        public static GameDirector Instance { get; private set; }

        public const float ArenaHalfWidth = 30f;
        public const float ArenaHalfHeight = 16f;
        public const float BattleDuration = 60f;
        private const float CameraViewSize = 5.2f;
        private const float CameraPanSpeed = 8.5f;
        private const float CameraFastPanMultiplier = 1.85f;
        private const float CameraFollowLerp = 5.5f;
        private const float CameraEdgeOverscroll = 3.2f;
        private const float MonsterDamageMultiplier = 3.2f;

        private readonly List<MonsterUnit> monsters = new List<MonsterUnit>();
        private readonly List<ExperienceOrb> experienceOrbs = new List<ExperienceOrb>();
        private readonly List<HazardZone> hazards = new List<HazardZone>();
        private readonly List<SkillWarning> skillWarnings = new List<SkillWarning>();
        private readonly List<TemporaryWall> temporaryWalls = new List<TemporaryWall>();
        private readonly Dictionary<MonsterKind, MonsterConfig> monsterConfigs = new Dictionary<MonsterKind, MonsterConfig>();
        private readonly Dictionary<CreatorSkillId, CreatorSkillConfig> skillConfigs = new Dictionary<CreatorSkillId, CreatorSkillConfig>();
        private readonly Dictionary<CreatorSkillId, float> skillCooldowns = new Dictionary<CreatorSkillId, float>();

        private HeroController hero;
        private PrototypeHud hud;
        private Camera mainCamera;
        private MonsterKind selectedMonster = MonsterKind.Skeleton;
        private CreatorSkillId selectedSkill = CreatorSkillId.LightningStrike;
        private bool aimingSkill;
        private float energy = 85f;
        private float maxEnergy = 220f;
        private float battleTimer;
        private float threat;
        private float messageTimer;
        private string message = "Select a monster, then click the arena to summon.";
        private bool finished;
        private bool gameStarted;
        private bool victory;
        private bool restarting;
        private Vector2 cameraTarget;
        private Vector2 cameraViewCenter;
        private HeroBuild forcedNextBuild = HeroBuild.FlameAura;

        public IReadOnlyList<MonsterUnit> Monsters => monsters;
        public IReadOnlyList<ExperienceOrb> ExperienceOrbs => experienceOrbs;
        public IReadOnlyList<HazardZone> Hazards => hazards;
        public IReadOnlyList<SkillWarning> SkillWarnings => skillWarnings;
        public IReadOnlyList<TemporaryWall> TemporaryWalls => temporaryWalls;
        public HeroController Hero => hero;
        public MonsterKind SelectedMonster => selectedMonster;
        public CreatorSkillId SelectedSkill => selectedSkill;
        public bool IsAimingSkill => aimingSkill;
        public float Energy => energy;
        public float MaxEnergy => maxEnergy;
        public float BattleTimer => battleTimer;
        public float RemainingTime => Mathf.Max(0f, BattleDuration - battleTimer);
        public float Threat => threat;
        public string Message => message;
        public bool IsGameStarted => gameStarted;
        public bool IsFinished => finished;
        public bool IsVictory => victory;
        public IEnumerable<MonsterKind> ConfiguredMonsterKinds => monsterConfigs.Keys;
        public IEnumerable<CreatorSkillId> ConfiguredSkillIds => skillConfigs.Keys;

        private void Awake()
        {
            Time.timeScale = 1f;
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildConfigs();
            DamageFeedbackSystem.Create();
            MusicManiacAudioSystem.Create();
            Time.timeScale = 0f;
            BuildScene();
        }

        private void Update()
        {
            if (!gameStarted)
            {
                return;
            }

            if (finished)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartGame();
                }

                return;
            }

            if (Time.timeScale <= 0f)
            {
                return;
            }

            battleTimer += Time.deltaTime;
            threat = Mathf.Clamp01((hero.Level - 1) * 0.09f + (1f - hero.Health01) * 0.22f + battleTimer / 420f);
            energy = Mathf.Min(maxEnergy, energy + (8f + threat * 15f) * Time.deltaTime);
            TickSkillCooldowns();

            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
            }
            else if (!string.IsNullOrEmpty(message) && battleTimer > 4f)
            {
                message = string.Empty;
            }

            HandleSummonInput();
            HandleSkillHotkeys();
            HandleCameraInput();
            CleanupLists();

            if (hero.Health <= 0f)
            {
                hero.PlayDeathAnimation();
                Finish(true, "造物主胜利！音乐疯子已被击败。");
            }
            else if (battleTimer >= BattleDuration)
            {
                Finish(false, "挑战失败！1分钟内没有击败音乐疯子。");
            }
        }

        public void StartGame()
        {
            if (gameStarted)
            {
                return;
            }

            gameStarted = true;
            Time.timeScale = 1f;
            FeelImpactSystem.Instance.Play(FeelImpactEvent.GameStart, FeelImpactLevel.Heavy, hero != null ? hero.Position : Vector2.zero, new Color(1f, 0.86f, 0.42f));
            SetMessage("战斗开始！在1分钟内击败音乐疯子。", 3f);
        }

        public void RestartGame()
        {
            if (restarting)
            {
                return;
            }

            restarting = true;
            Time.timeScale = 1f;
            CleanupRuntimeSystemsForReload();
            if (Instance == this)
            {
                Instance = null;
            }

            var activeScene = SceneManager.GetActiveScene();
            var sceneIndex = activeScene.buildIndex >= 0 ? activeScene.buildIndex : 0;
            SceneManager.LoadScene(sceneIndex, LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SelectMonster(MonsterKind kind)
        {
            selectedMonster = kind;
            aimingSkill = false;
            var config = GetMonsterConfig(kind);
            MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.SelectMonster, Vector2.zero, 0.9f);
            SetMessage($"{config.DisplayName} selected. Cost {config.Cost:0} energy.", 2.5f);
        }

        public void SelectSkill(CreatorSkillId skillId)
        {
            selectedSkill = skillId;
            aimingSkill = true;
            var config = GetSkillConfig(skillId);
            MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.SelectSkill, Vector2.zero, 0.9f);
            SetMessage($"{config.DisplayName} aiming. Click the arena to warn and cast.", 3f);
        }

        public MonsterConfig GetMonsterConfig(MonsterKind kind)
        {
            return monsterConfigs[kind];
        }

        public CreatorSkillConfig GetSkillConfig(CreatorSkillId skillId)
        {
            return skillConfigs[skillId];
        }

        public bool CanAffordMonster(MonsterKind kind)
        {
            return energy >= GetMonsterConfig(kind).Cost;
        }

        public bool CanPrepareSkill(CreatorSkillId skillId)
        {
            var config = GetSkillConfig(skillId);
            return energy >= config.Cost && GetSkillCooldownRemaining(skillId) <= 0f;
        }

        public void SelectMonsterFromHud(MonsterKind kind)
        {
            var config = GetMonsterConfig(kind);
            if (energy < config.Cost)
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiError, Vector2.zero, 1f);
                SetMessage($"Not enough soul for {config.DisplayName}. Need {config.Cost:0}.", 1.8f);
                return;
            }

            SelectMonster(kind);
        }

        public void SelectSkillFromHud(CreatorSkillId skillId)
        {
            var config = GetSkillConfig(skillId);
            var cooldown = GetSkillCooldownRemaining(skillId);
            if (energy < config.Cost)
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiError, Vector2.zero, 1f);
                SetMessage($"Not enough soul for {config.DisplayName}. Need {config.Cost:0}.", 1.8f);
                return;
            }

            if (cooldown > 0f)
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiError, Vector2.zero, 1f);
                SetMessage($"{config.DisplayName} cooling down: {cooldown:0.0}s.", 1.8f);
                return;
            }

            SelectSkill(skillId);
        }

        public float GetSkillCooldownRemaining(CreatorSkillId skillId)
        {
            return skillCooldowns.TryGetValue(skillId, out var cooldown) ? cooldown : 0f;
        }

        public void RegisterMonster(MonsterUnit monster)
        {
            monsters.Add(monster);
        }

        public void RegisterExperience(ExperienceOrb orb)
        {
            experienceOrbs.Add(orb);
        }

        public void UnregisterExperience(ExperienceOrb orb)
        {
            experienceOrbs.Remove(orb);
        }

        public void AwardDamage(float damage)
        {
            energy = Mathf.Min(maxEnergy, energy + damage * 0.08f);
        }

        public void DropExperience(Vector2 position, float value)
        {
            var orbObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orbObject.name = "Experience Orb";
            orbObject.transform.position = new Vector3(position.x, position.y, 0f);
            orbObject.transform.localScale = Vector3.one * 0.18f;
            Destroy(orbObject.GetComponent<SphereCollider>());

            var renderer = orbObject.GetComponent<MeshRenderer>();
            renderer.material = VisualFactory.CreateMaterial(new Color(0.45f, 0.95f, 1f));
            renderer.enabled = false;
            MusicManiacArtLibrary.AttachSprite(orbObject, MusicManiacArtLibrary.Icon("resource_energy"), 0.34f, 6, "Energy Pixel Art", Color.white);

            var orb = orbObject.AddComponent<ExperienceOrb>();
            orb.Initialize(value);
            RegisterExperience(orb);
        }

        public void ForceHeroBuild(int buildIndex)
        {
            forcedNextBuild = (HeroBuild)Mathf.Clamp(buildIndex, 0, 2);
            hero.OverrideNextBuild(forcedNextBuild);
            SetMessage($"Hero next build pressure: {forcedNextBuild}", 3f);
        }

        public void ResolveSkillImpact(CreatorSkillConfig config, Vector2 position)
        {
            skillWarnings.RemoveAll(warning => warning == null);

            if (config.Id == CreatorSkillId.BoneWall)
            {
                CreateBoneWall(position);
                SetMessage("Bone wall rises. The hero will try to route around it.", 2.5f);
                return;
            }

            if (config.Duration > 0f)
            {
                SkillEffectZone.Create(config, position);
            }

            var distance = Vector2.Distance(hero.Position, position);
            if (distance <= config.Radius)
            {
                var beforeHealth = hero.Health;
                hero.TakeSkillDamage(config.Damage, config.AntiHealSeconds > 0f, config.ShieldBreakSeconds > 0f);
                var dealt = Mathf.Max(0f, beforeHealth - hero.Health);

                if (config.SlowMultiplier < 1f)
                {
                    hero.ApplySkillSlow(config.SlowMultiplier, Mathf.Max(1.2f, config.Duration));
                }

                if (config.AntiHealSeconds > 0f)
                {
                    hero.ApplyAntiHeal(config.AntiHealSeconds);
                }

                if (config.ShieldBreakSeconds > 0f)
                {
                    hero.ApplyShieldBreak(config.ShieldBreakSeconds);
                }

                DamageFeedbackSystem.Instance.ReportHeroDamage(
                    hero,
                    Mathf.Max(config.Damage, dealt),
                    FeedbackTypeForSkill(config),
                    position,
                    true,
                    config.SlowMultiplier < 1f,
                    config.ShieldBreakSeconds > 0f,
                    hero.Health01 < 0.35f,
                    config.Danger >= 0.75f || config.Damage >= 90f);
                FeelImpactSystem.Instance.Play(FeelImpactEvent.CreatorSkillHit, FeelImpactSystem.LevelForSkill(config, true), position, config.EffectColor);
                MusicManiacAudioSystem.Instance.PlaySkill(config.Id, "hit", position, 1f);
                SetMessage($"{config.DisplayName} hit the hero.", 2f);
            }
            else
            {
                DamageFeedbackSystem.Instance.ReportSpecialText(position, "MISS", DamageFeedbackType.Physical, false);
                DamageFeedbackSystem.Instance.ReportHeroBubble(hero, BubbleTalkEvent.DodgedSkill, false);
                MusicManiacAudioSystem.Instance.PlaySkill(config.Id, "miss", position, 0.85f);
                SetMessage($"{config.DisplayName} missed. The warning gave the hero room.", 2.4f);
            }
        }

        private static DamageFeedbackType FeedbackTypeForSkill(CreatorSkillConfig config)
        {
            switch (config.Id)
            {
                case CreatorSkillId.LightningStrike:
                    return DamageFeedbackType.Lightning;
                case CreatorSkillId.FrostField:
                    return DamageFeedbackType.Ice;
                case CreatorSkillId.AntiHealCurse:
                case CreatorSkillId.DemonHand:
                    return DamageFeedbackType.Shadow;
                case CreatorSkillId.ShieldBrand:
                    return DamageFeedbackType.ShieldBreak;
                default:
                    return DamageFeedbackType.Physical;
            }
        }

        private void BuildConfigs()
        {
            monsterConfigs[MonsterKind.Skeleton] = new MonsterConfig(MonsterKind.Skeleton, "Skeleton", 10f, 36f, 6f * MonsterDamageMultiplier, 1.55f, 0.95f, 0.65f, 0f, new Color(0.82f, 0.86f, 0.86f), "Cheap swarm");
            monsterConfigs[MonsterKind.VenomBug] = new MonsterConfig(MonsterKind.VenomBug, "Venom Bug", 15f, 26f, 3f * MonsterDamageMultiplier, 1.95f, 0.9f, 0.5f, 4f * MonsterDamageMultiplier, new Color(0.36f, 0.92f, 0.32f), "Poison");
            monsterConfigs[MonsterKind.Archer] = new MonsterConfig(MonsterKind.Archer, "Archer", 25f, 24f, 7f * MonsterDamageMultiplier, 1.1f, 6.2f, 1.25f, 0f, new Color(0.95f, 0.74f, 0.36f), "Ranged");
            monsterConfigs[MonsterKind.Stoneguard] = new MonsterConfig(MonsterKind.Stoneguard, "Stoneguard", 40f, 115f, 8f * MonsterDamageMultiplier, 0.75f, 1.1f, 0.9f, 0f, new Color(0.42f, 0.48f, 0.52f), "Tank");
            monsterConfigs[MonsterKind.HexPriest] = new MonsterConfig(MonsterKind.HexPriest, "Hex Priest", 60f, 42f, 4f * MonsterDamageMultiplier, 1f, 5.1f, 1.4f, 0f, new Color(0.62f, 0.44f, 0.95f), "Anti-heal");
            monsterConfigs[MonsterKind.Shieldbreaker] = new MonsterConfig(MonsterKind.Shieldbreaker, "Shieldbreaker", 70f, 56f, 10f * MonsterDamageMultiplier, 1.35f, 1.45f, 1.05f, 0f, new Color(0.2f, 0.72f, 0.95f), "Break shield");
            monsterConfigs[MonsterKind.Assassin] = new MonsterConfig(MonsterKind.Assassin, "Assassin", 80f, 34f, 22f * MonsterDamageMultiplier, 2.55f, 0.95f, 1.25f, 0f, new Color(0.18f, 0.15f, 0.2f), "Burst");
            monsterConfigs[MonsterKind.BoneKing] = new MonsterConfig(MonsterKind.BoneKing, "Bone King", 160f, 420f, 20f * MonsterDamageMultiplier, 0.85f, 1.65f, 0.75f, 0f, new Color(0.9f, 0.88f, 0.68f), "Boss");

            skillConfigs[CreatorSkillId.LightningStrike] = new CreatorSkillConfig(CreatorSkillId.LightningStrike, "Lightning", CreatorSkillType.Damage, 50f, 6f, 0.6f, 0.9f, 0f, 95f, 0f, 1f, 0f, 0f, 0.55f, new Color(1f, 0.2f, 0.12f, 0.66f), new Color(1f, 0.95f, 0.52f, 0.72f), "Burst");
            skillConfigs[CreatorSkillId.FrostField] = new CreatorSkillConfig(CreatorSkillId.FrostField, "Frost Field", CreatorSkillType.Control, 120f, 20f, 1.2f, 1.85f, 4f, 18f, 4f, 0.48f, 0f, 0f, 0.72f, new Color(0.95f, 0.16f, 0.14f, 0.58f), new Color(0.25f, 0.62f, 1f, 0.62f), "Slow");
            skillConfigs[CreatorSkillId.AntiHealCurse] = new CreatorSkillConfig(CreatorSkillId.AntiHealCurse, "Anti-Heal", CreatorSkillType.Curse, 120f, 18f, 1.2f, 1.35f, 0f, 28f, 0f, 1f, 6f, 0f, 0.78f, new Color(0.95f, 0.08f, 0.18f, 0.6f), new Color(0.72f, 0.28f, 0.92f, 0.62f), "Anti-heal");
            skillConfigs[CreatorSkillId.ShieldBrand] = new CreatorSkillConfig(CreatorSkillId.ShieldBrand, "Shield Brand", CreatorSkillType.Curse, 110f, 16f, 1f, 1.35f, 0f, 20f, 0f, 1f, 0f, 6f, 0.74f, new Color(0.95f, 0.12f, 0.12f, 0.6f), new Color(0.12f, 0.82f, 1f, 0.62f), "Break shield");
            skillConfigs[CreatorSkillId.BoneWall] = new CreatorSkillConfig(CreatorSkillId.BoneWall, "Bone Wall", CreatorSkillType.Terrain, 150f, 28f, 1f, 1.3f, 6f, 0f, 0f, 1f, 0f, 0f, 0.68f, new Color(0.92f, 0.18f, 0.12f, 0.58f), new Color(0.78f, 0.76f, 0.62f, 1f), "Terrain");
            skillConfigs[CreatorSkillId.DemonHand] = new CreatorSkillConfig(CreatorSkillId.DemonHand, "Demon Hand", CreatorSkillType.Finisher, 500f, 60f, 2.5f, 2.55f, 0f, 330f, 0f, 1f, 0f, 0f, 1f, new Color(0.32f, 0f, 0f, 0.78f), new Color(0.95f, 0.1f, 0.05f, 0.82f), "Finisher");

            foreach (var skillId in skillConfigs.Keys)
            {
                skillCooldowns[skillId] = 0f;
            }
        }

        private void BuildScene()
        {
            SetupCamera();
            SetupEventSystem();
            CreateArena();
            CreateHazards();
            CreateHero();
            hud = PrototypeHud.Create(this);
        }

        private void SetupCamera()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = CameraViewSize;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            cameraTarget = Vector2.zero;
            cameraViewCenter = Vector2.zero;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.06f, 0.075f, 0.085f);
            FeelImpactSystem.Create(mainCamera);
            FeelImpactSystem.Instance.SetCameraBaseLocalPosition(mainCamera.transform.localPosition);
        }

        private static void SetupEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void CreateArena()
        {
            CreateStageTileBackdrop();

            for (var x = Mathf.CeilToInt(-ArenaHalfWidth); x <= ArenaHalfWidth; x += 2)
            {
                VisualFactory.CreateLine(new Vector2(x, -ArenaHalfHeight), new Vector2(x, ArenaHalfHeight), new Color(0.19f, 0.25f, 0.22f), 0.015f, "Grid");
            }

            for (var y = Mathf.CeilToInt(-ArenaHalfHeight); y <= ArenaHalfHeight; y += 2)
            {
                VisualFactory.CreateLine(new Vector2(-ArenaHalfWidth, y), new Vector2(ArenaHalfWidth, y), new Color(0.19f, 0.25f, 0.22f), 0.015f, "Grid");
            }

            var borderGuide = new Color(0.22f, 0.32f, 0.38f, 0.62f);
            VisualFactory.CreateLine(new Vector2(-ArenaHalfWidth, -ArenaHalfHeight), new Vector2(ArenaHalfWidth, -ArenaHalfHeight), borderGuide, 0.025f, "Arena Border Guide");
            VisualFactory.CreateLine(new Vector2(-ArenaHalfWidth, ArenaHalfHeight), new Vector2(ArenaHalfWidth, ArenaHalfHeight), borderGuide, 0.025f, "Arena Border Guide");
            VisualFactory.CreateLine(new Vector2(-ArenaHalfWidth, -ArenaHalfHeight), new Vector2(-ArenaHalfWidth, ArenaHalfHeight), borderGuide, 0.025f, "Arena Border Guide");
            VisualFactory.CreateLine(new Vector2(ArenaHalfWidth, -ArenaHalfHeight), new Vector2(ArenaHalfWidth, ArenaHalfHeight), borderGuide, 0.025f, "Arena Border Guide");
            CreateStageDecor();
        }

        private static void CreateStageTileBackdrop()
        {
            var centerTiles = new[]
            {
                "tile_stage_floor",
                "tile_stage_floor_a",
                "tile_stage_floor_b",
                "tile_stage_floor_c",
                "tile_soundwave_floor",
                "tile_dark_brick",
                "tile_asphalt_wave",
                "tile_concrete_roof"
            };

            var minX = Mathf.FloorToInt(-ArenaHalfWidth);
            var maxX = Mathf.CeilToInt(ArenaHalfWidth);
            var minY = Mathf.FloorToInt(-ArenaHalfHeight);
            var maxY = Mathf.CeilToInt(ArenaHalfHeight);
            var outerMinX = minX - 1;
            var outerMaxX = maxX + 1;
            var outerMinY = minY - 1;
            var outerMaxY = maxY + 1;
            for (var x = outerMinX; x <= outerMaxX; x++)
            {
                for (var y = outerMinY; y <= outerMaxY; y++)
                {
                    var tileName = SelectStageTile(x, y, minX, maxX, minY, maxY, centerTiles);
                    var tile = MusicManiacArtLibrary.CreateSpriteObject(
                        $"Stage Tile {x},{y}",
                        MusicManiacArtLibrary.Tile(tileName),
                        new Vector2(x + 0.5f, y + 0.5f),
                        1f,
                        -25,
                        Color.white);
                    tile.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, 0.92f);
                }
            }
        }

        private static string SelectStageTile(int x, int y, int minX, int maxX, int minY, int maxY, string[] centerTiles)
        {
            var outsideLeft = x < minX;
            var outsideRight = x > maxX;
            var outsideBottom = y < minY;
            var outsideTop = y > maxY;
            if (outsideLeft && outsideTop) return "tile_border_corner_tl";
            if (outsideRight && outsideTop) return "tile_border_corner_tr";
            if (outsideLeft && outsideBottom) return "tile_border_corner_bl";
            if (outsideRight && outsideBottom) return "tile_border_corner_br";
            if (outsideTop) return "tile_border_top";
            if (outsideBottom) return "tile_border_bottom";
            if (outsideLeft) return "tile_border_left";
            if (outsideRight) return "tile_border_right";

            var edgeDistance = Mathf.Min(x - minX, maxX - x, y - minY, maxY - y);
            if (edgeDistance <= 1)
            {
                var edgeTiles = new[] { "tile_stage_floor_b", "tile_dark_brick", "tile_asphalt_wave" };
                return edgeTiles[Mathf.Abs((x * 11 + y * 5) % edgeTiles.Length)];
            }

            return centerTiles[Mathf.Abs((x * 7 + y * 3) % centerTiles.Length)];
        }

        private static void CreateStageDecor()
        {
            CreateDecor("speaker", new Vector2(-ArenaHalfWidth + 1.4f, ArenaHalfHeight - 1.1f), 1.25f, 18f);
            CreateDecor("speaker", new Vector2(ArenaHalfWidth - 1.4f, ArenaHalfHeight - 1.1f), 1.25f, -18f);
            CreateDecor("dj_table", new Vector2(0f, ArenaHalfHeight - 0.9f), 1.15f, 0f);
            CreateDecor("neon_sign", new Vector2(-ArenaHalfWidth + 3.2f, -ArenaHalfHeight + 0.9f), 1.0f, 0f);
            CreateDecor("poster", new Vector2(ArenaHalfWidth - 3.2f, -ArenaHalfHeight + 0.9f), 1.0f, 0f);
            CreateDecor("cable", new Vector2(-5.4f, -ArenaHalfHeight + 0.8f), 0.9f, 0f);
            CreateDecor("mic_stand", new Vector2(7.5f, -ArenaHalfHeight + 1.1f), 1.0f, 0f);
            CreateDecor("light_rig", new Vector2(-9.5f, ArenaHalfHeight - 0.9f), 1.0f, 0f);
            CreateDecor("barrier", new Vector2(12.5f, ArenaHalfHeight - 1.0f), 1.0f, 0f);
            CreateDecor("barrier", new Vector2(-12.5f, -ArenaHalfHeight + 1.0f), 1.0f, 180f);
            CreateDecor("crate", new Vector2(-ArenaHalfWidth + 1.5f, 2.8f), 1.0f, -8f);
            CreateDecor("crate", new Vector2(ArenaHalfWidth - 1.8f, -4.8f), 1.0f, 9f);
            CreateDecor("road_sign", new Vector2(-2.4f, -ArenaHalfHeight + 1.2f), 0.95f, -3f);
        }

        private static void CreateDecor(string assetName, Vector2 position, float height, float zRotation)
        {
            var decor = MusicManiacArtLibrary.CreateSpriteObject(
                $"Stage Decor - {assetName}",
                MusicManiacArtLibrary.LoadSprite($"decor/{assetName}"),
                position,
                height,
                -10,
                Color.white);
            decor.transform.position = new Vector3(position.x, position.y, -0.05f);
            decor.transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
        }

        private void CreateHazards()
        {
            CreateHazard("Poison Pool", new Vector2(-10.5f, 4.2f), 1.55f, 6f, 0.72f, new Color(0.28f, 0.7f, 0.22f, 0.62f));
            CreateHazard("Slow Sigil", new Vector2(8.4f, -3.6f), 1.85f, 1.8f, 0.42f, new Color(0.2f, 0.55f, 0.95f, 0.56f));
            CreateHazard("Poison Pool", new Vector2(15.2f, 7.4f), 1.3f, 5f, 0.8f, new Color(0.28f, 0.7f, 0.22f, 0.62f));
            CreateHazard("Slow Sigil", new Vector2(-18.5f, -7.2f), 1.45f, 1.4f, 0.55f, new Color(0.2f, 0.55f, 0.95f, 0.48f));
        }

        private void CreateHazard(string name, Vector2 position, float radius, float damage, float speedMultiplier, Color color)
        {
            var hazardObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hazardObject.name = name;
            hazardObject.transform.position = new Vector3(position.x, position.y, 0.25f);
            hazardObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            hazardObject.transform.localScale = new Vector3(radius * 2f, 0.05f, radius * 2f);
            Destroy(hazardObject.GetComponent<CapsuleCollider>());
            hazardObject.GetComponent<MeshRenderer>().material = VisualFactory.CreateMaterial(color);
            MusicManiacArtLibrary.AttachSprite(
                hazardObject,
                color.g > color.b ? MusicManiacArtLibrary.Tile("tile_poison") : MusicManiacArtLibrary.Tile("tile_ice"),
                radius * 2.25f,
                -8,
                "Hazard Pixel Art",
                new Color(1f, 1f, 1f, 0.88f));

            var hazard = hazardObject.AddComponent<HazardZone>();
            hazard.Initialize(radius, damage, speedMultiplier);
            hazards.Add(hazard);
        }

        private void CreateHero()
        {
            var heroObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            heroObject.name = "AI Hero";
            heroObject.transform.position = Vector3.zero;
            heroObject.transform.localScale = new Vector3(1.08f, 1.08f, 1.08f);
            Destroy(heroObject.GetComponent<CapsuleCollider>());
            heroObject.GetComponent<MeshRenderer>().material = VisualFactory.CreateMaterial(new Color(0.92f, 0.94f, 1f));
            MusicManiacArtLibrary.AttachSprite(heroObject, MusicManiacArtLibrary.HeroSprite, 2.15f, 10, "Music Maniac Sprite", Color.white);

            hero = heroObject.AddComponent<HeroController>();
            hero.Initialize(forcedNextBuild);
        }

        private void HandleSummonInput()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            var world = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var target = new Vector2(world.x, world.y);
            if (aimingSkill)
            {
                TryCastSkill(selectedSkill, target);
            }
            else
            {
                TrySummon(selectedMonster, target);
            }
        }

        private void TryCastSkill(CreatorSkillId skillId, Vector2 position)
        {
            if (Mathf.Abs(position.x) > ArenaHalfWidth || Mathf.Abs(position.y) > ArenaHalfHeight)
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.SummonInvalid, position, 1f);
                SetMessage("Cast inside the arena.", 2f);
                return;
            }

            var config = GetSkillConfig(skillId);
            if (energy < config.Cost)
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiError, position, 1f);
                SetMessage($"Not enough energy for {config.DisplayName}.", 1.8f);
                return;
            }

            if (GetSkillCooldownRemaining(skillId) > 0f)
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiError, position, 1f);
                SetMessage($"{config.DisplayName} is cooling down.", 1.8f);
                return;
            }

            energy -= config.Cost;
            skillCooldowns[skillId] = config.Cooldown;
            var warning = SkillWarning.Create(config, position);
            skillWarnings.Add(warning);
            MusicManiacAudioSystem.Instance.PlaySkill(config.Id, "warning", position, 1f);
            FeelImpactSystem.Instance.Play(FeelImpactEvent.CreatorSkillCast, FeelImpactSystem.LevelForSkill(config, false), position, config.WarningColor);
            SetMessage($"{config.DisplayName} warning active. Hero is reacting.", config.WarningTime);
        }

        private void HandleSkillHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSkill(CreatorSkillId.LightningStrike);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSkill(CreatorSkillId.FrostField);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSkill(CreatorSkillId.AntiHealCurse);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSkill(CreatorSkillId.ShieldBrand);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSkill(CreatorSkillId.BoneWall);
            if (Input.GetKeyDown(KeyCode.Alpha6)) SelectSkill(CreatorSkillId.DemonHand);
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                aimingSkill = false;
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiCancel, Vector2.zero, 0.85f);
                SetMessage("Skill cast canceled.", 1.2f);
            }
        }

        private void HandleCameraInput()
        {
            if (mainCamera == null)
            {
                return;
            }

            var input = Vector2.zero;
            if (Input.GetKey(KeyCode.A))
            {
                input.x -= 1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                input.x += 1f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                input.y -= 1f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                input.y += 1f;
            }

            if (Input.GetKeyDown(KeyCode.Space) && hero != null)
            {
                cameraTarget = hero.Position;
            }

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            var speed = CameraPanSpeed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                speed *= CameraFastPanMultiplier;
            }

            cameraTarget += input * speed * Time.deltaTime;
            cameraTarget = ClampCameraTarget(cameraTarget);

            cameraViewCenter = Vector2.Lerp(cameraViewCenter, cameraTarget, Time.deltaTime * CameraFollowLerp);
            cameraViewCenter = ClampCameraTarget(cameraViewCenter);
            var desired = new Vector3(cameraViewCenter.x, cameraViewCenter.y, -10f);
            FeelImpactSystem.Instance.SetCameraBaseLocalPosition(desired);
        }

        private Vector2 ClampCameraTarget(Vector2 target)
        {
            if (mainCamera == null)
            {
                return target;
            }

            var vertical = mainCamera.orthographicSize;
            var horizontal = vertical * mainCamera.aspect;
            var maxX = Mathf.Max(0f, ArenaHalfWidth - horizontal + CameraEdgeOverscroll);
            var maxY = Mathf.Max(0f, ArenaHalfHeight - vertical + CameraEdgeOverscroll);
            target.x = Mathf.Clamp(target.x, -maxX, maxX);
            target.y = Mathf.Clamp(target.y, -maxY, maxY);
            return target;
        }

        private void TickSkillCooldowns()
        {
            var keys = new List<CreatorSkillId>(skillCooldowns.Keys);
            foreach (var key in keys)
            {
                skillCooldowns[key] = Mathf.Max(0f, skillCooldowns[key] - Time.deltaTime);
            }
        }

        private void TrySummon(MonsterKind kind, Vector2 position)
        {
            if (Mathf.Abs(position.x) > ArenaHalfWidth || Mathf.Abs(position.y) > ArenaHalfHeight)
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.SummonInvalid, position, 1f);
                SetMessage("Summon inside the arena.", 2f);
                return;
            }

            if (Vector2.Distance(position, hero.Position) < 1.25f)
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.SummonInvalid, position, 1f);
                SetMessage("Too close to the hero.", 2f);
                return;
            }

            var config = GetMonsterConfig(kind);
            if (energy < config.Cost)
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiError, position, 1f);
                SetMessage("Not enough energy.", 1.6f);
                return;
            }

            energy -= config.Cost;
            MonsterUnit.Create(config, position);
            MusicManiacAudioSystem.Instance.PlayMonster(config.Kind, "spawn", position, 1f);
            MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.Summon, position, 0.85f);
            VisualFactory.CreateAnimatedSpriteBurst(
                position,
                "vfx/vfx_spawn_smoke",
                config.Kind == MonsterKind.BoneKing ? 1.8f : 0.95f,
                config.Color,
                0.45f,
                18,
                8,
                16f);
            FeelImpactSystem.Instance.Play(config.Kind == MonsterKind.BoneKing ? FeelImpactEvent.BossSpawn : FeelImpactEvent.MonsterSpawn, FeelImpactSystem.LevelForMonster(config, false), position, config.Color);
        }

        private void CleanupLists()
        {
            monsters.RemoveAll(monster => monster == null);
            experienceOrbs.RemoveAll(orb => orb == null);
            skillWarnings.RemoveAll(warning => warning == null);
            temporaryWalls.RemoveAll(wall => wall == null);
        }

        private void CreateBoneWall(Vector2 position)
        {
            var size = Mathf.Abs(hero.Position.x - position.x) > Mathf.Abs(hero.Position.y - position.y)
                ? new Vector2(0.55f, 3.5f)
                : new Vector2(3.5f, 0.55f);
            var wall = TemporaryWall.Create(position, size, GetSkillConfig(CreatorSkillId.BoneWall).Duration);
            temporaryWalls.Add(wall);
        }

        private void Finish(bool didWin, string result)
        {
            if (finished)
            {
                return;
            }

            finished = true;
            victory = didWin;
            Time.timeScale = 0f;
            MusicManiacAudioSystem.Instance.PlayResult(didWin);
            FeelImpactSystem.Instance.Play(FeelImpactEvent.GameEnd, didWin ? FeelImpactLevel.Ultimate : FeelImpactLevel.Heavy, hero != null ? hero.Position : Vector2.zero, didWin ? new Color(1f, 0.72f, 0.28f) : new Color(0.45f, 0.65f, 1f));
            SetMessage($"{result} 按R重新开始。", 999f);
        }

        private void SetMessage(string text, float seconds)
        {
            message = text;
            messageTimer = seconds;
        }

        private static void CleanupRuntimeSystemsForReload()
        {
            DamageFeedbackSystem.ResetInstance();
            MusicManiacAudioSystem.ResetInstance();
            FeelImpactSystem.ResetInstance();
        }
    }

    public readonly struct MonsterConfig
    {
        public MonsterConfig(MonsterKind kind, string displayName, float cost, float health, float damage, float moveSpeed, float attackRange, float attackCooldown, float poisonDamage, Color color, string tag)
        {
            Kind = kind;
            DisplayName = displayName;
            Cost = cost;
            Health = health;
            Damage = damage;
            MoveSpeed = moveSpeed;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
            PoisonDamage = poisonDamage;
            Color = color;
            Tag = tag;
        }

        public MonsterKind Kind { get; }
        public string DisplayName { get; }
        public float Cost { get; }
        public float Health { get; }
        public float Damage { get; }
        public float MoveSpeed { get; }
        public float AttackRange { get; }
        public float AttackCooldown { get; }
        public float PoisonDamage { get; }
        public Color Color { get; }
        public string Tag { get; }
    }
}
