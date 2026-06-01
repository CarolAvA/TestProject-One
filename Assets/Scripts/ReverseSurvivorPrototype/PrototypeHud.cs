using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReverseSurvivorPrototype
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.025f, 0.024f, 0.034f, 0.88f);
        private static readonly Color NeonPink = new Color(1f, 0.16f, 0.58f, 0.95f);
        private static readonly Color NeonCyan = new Color(0.12f, 0.82f, 1f, 0.95f);
        private static readonly Color NeonPurple = new Color(0.58f, 0.22f, 0.95f, 0.95f);
        private static readonly Color NeonOrange = new Color(1f, 0.5f, 0.16f, 0.95f);
        private const float RhythmTrackX = 112f;
        private const float RhythmTrackWidth = 886f;
        private const float RhythmLaneHeight = 14f;
        private const float RhythmLaneSpacing = 18f;
        private const int RhythmBeatMarkerCapacity = 36;

        private GameDirector director;
        private Text heroText;
        private Text timerText;
        private Text threatText;
        private Text summonEnergyText;
        private Text skillEnergyText;
        private Text healthValueText;
        private Text healthPercentText;
        private Text shieldPercentText;
        private Text objectiveText;
        private Text messageText;
        private Text aiStatusText;
        private Text rhythmText;
        private Text rhythmTracksText;
        private Text rhythmPhaseText;
        private Text rhythmCueText;
        private Text rhythmSyncText;
        private Text rhythmBeatCountText;
        private Text bdListText;
        private Text bdTooltipTitleText;
        private Text bdTooltipMetaText;
        private Text bdTooltipText;
        private Text summonEnergyTopText;
        private Text skillEnergyTopText;
        private Image bdTooltipAccent;
        private Image healthFill;
        private Image shieldFill;
        private Image threatFill;
        private Image summonEnergyTopFill;
        private Image skillEnergyTopFill;
        private Image summonEnergyFill;
        private Image skillEnergyFill;
        private Image rhythmFill;
        private Image rhythmCursor;
        private GameObject bdTooltipPanel;
        private GameObject mainMenuPanel;
        private GameObject settingsPanel;
        private GameObject resultPanel;
        private Image resultPromoImage;
        private Text resultTitleText;
        private Text resultBodyText;
        private RuntimeUiConfig uiConfig;

        private readonly Dictionary<MonsterKind, Button> buttons = new Dictionary<MonsterKind, Button>();
        private readonly Dictionary<CreatorSkillId, Button> skillButtons = new Dictionary<CreatorSkillId, Button>();
        private readonly Dictionary<Button, Image> buttonImages = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Text> buttonLabels = new Dictionary<Button, Text>();
        private readonly Dictionary<Button, Text> buttonMetaLabels = new Dictionary<Button, Text>();
        private readonly Dictionary<Button, Text> buttonStatusLabels = new Dictionary<Button, Text>();
        private readonly Dictionary<Button, Text> buttonShortcutLabels = new Dictionary<Button, Text>();
        private readonly Dictionary<Button, Image> buttonShortcutBadges = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonIcons = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonAccentBars = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonStatusBadges = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonDisabledOverlays = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonSelectionFrames = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonIconSlots = new Dictionary<Button, Image>();
        private readonly List<Button> bdButtons = new List<Button>();
        private readonly List<BDCardView> bdCardViews = new List<BDCardView>();
        private readonly List<AIBDDisplayData> bdDisplayCache = new List<AIBDDisplayData>();
        private readonly List<Image> rhythmLaneFills = new List<Image>();
        private readonly List<Image> rhythmBeatMarkers = new List<Image>();
        private readonly List<RhythmBeatUiData> rhythmBeatCache = new List<RhythmBeatUiData>();
        private int hoveredBdIndex = -1;
        private int pinnedBdIndex = -1;

        private sealed class BDCardView
        {
            public Button Button;
            public Image Background;
            public Image IconBack;
            public Text IconText;
            public Text NameText;
            public Text MetaText;
            public Text LevelText;
            public Image Accent;
            public Image NewBadge;
            public Image Selection;
        }

        public static PrototypeHud Create(GameDirector director)
        {
            var canvasObject = new GameObject("Prototype HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var hud = canvasObject.AddComponent<PrototypeHud>();
            hud.director = director;
            hud.Build(canvasObject.transform);
            return hud;
        }

        private void Update()
        {
            if (director == null || director.Hero == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R) && director.IsFinished)
            {
                director.RestartGame();
                return;
            }

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(!director.IsGameStarted);
            }

            RefreshResultPanel();

            var hero = director.Hero;
            var rhythm = hero.Rhythm;
            healthFill.fillAmount = hero.Health01;
            shieldFill.fillAmount = Mathf.Clamp01(hero.Shield / Mathf.Max(1f, hero.MaxHealth * 0.35f));
            threatFill.fillAmount = director.Threat;
            var summonEnergy01 = Mathf.Clamp01(director.SummonEnergy / Mathf.Max(1f, director.MaxSummonEnergy));
            var skillEnergy01 = Mathf.Clamp01(director.SkillEnergy / Mathf.Max(1f, director.MaxSkillEnergy));
            if (summonEnergyFill != null)
            {
                summonEnergyFill.fillAmount = summonEnergy01;
            }

            if (skillEnergyFill != null)
            {
                skillEnergyFill.fillAmount = skillEnergy01;
            }

            if (summonEnergyTopFill != null)
            {
                summonEnergyTopFill.fillAmount = summonEnergy01;
            }

            if (skillEnergyTopFill != null)
            {
                skillEnergyTopFill.fillAmount = skillEnergy01;
            }

            heroText.text = $"音乐疯子  等级{hero.Level}\n{hero.BuildData.BuildName}";
            var shield01 = Mathf.Clamp01(hero.Shield / Mathf.Max(1f, hero.MaxHealth * 0.35f));
            healthValueText.text = $"生命 {hero.Health:0}/{hero.MaxHealth:0}  护盾 {hero.Shield:0}";
            healthPercentText.text = $"{hero.Health01:P0}";
            shieldPercentText.text = shield01 > 0.01f ? $"护盾 {shield01:P0}" : "护盾 0%";
            timerText.text = $"{Mathf.FloorToInt(director.RemainingTime / 60f):00}:{Mathf.FloorToInt(director.RemainingTime % 60f):00}";
            threatText.text = $"威胁 {director.Threat:P0}";
            var summonEnergyLabel = $"召唤能量 {director.SummonEnergy:0}/{director.MaxSummonEnergy:0}";
            var skillEnergyLabel = $"技能能量 {director.SkillEnergy:0}/{director.MaxSkillEnergy:0}";
            if (summonEnergyText != null)
            {
                summonEnergyText.text = summonEnergyLabel;
            }

            if (skillEnergyText != null)
            {
                skillEnergyText.text = skillEnergyLabel;
            }

            if (summonEnergyTopText != null)
            {
                summonEnergyTopText.text = summonEnergyLabel;
            }

            if (skillEnergyTopText != null)
            {
                skillEnergyTopText.text = skillEnergyLabel;
            }

            var hpTag = hero.Health01 < 0.35f ? "血量较低，集中压制。" : $"{GameDirector.FormatDuration(director.BattleDuration)}内击败音乐疯子。";
            objectiveText.text = $"作战目标\n{hpTag}\n倒计时  {timerText.text}\n人口  {director.Monsters.Count}/{GameDirector.MonsterPopulationCap}\n镜头  方向键位 / 加速 / 回中";
            messageText.text = string.IsNullOrEmpty(director.Message) ? "右侧选择召唤或技能，点击战场释放。" : director.Message;

            if (rhythm != null)
            {
                RefreshRhythmPanel(rhythm);
                aiStatusText.text =
                    $"流派：{hero.BuildData.BuildName}\n" +
                    $"攻击：{rhythm.AttackName}   危险：{rhythm.DangerLevel}\n" +
                    $"标签：{rhythm.BuildTags}\n" +
                    $"减益：禁疗 {hero.AntiHealTime:0.0}秒 / 破盾 {hero.ShieldBreakTime:0.0}秒 / 减速 {hero.SkillSlowTime:0.0}秒";
            }

            RefreshBDList(hero.BuildData);
            RefreshButtonStates();
        }

        private void Build(Transform root)
        {
            uiConfig = new RuntimeUiConfig();
            BuildTopStatus(root);
            BuildLeftInfo(root);
            BuildRightActionDeck(root);
            BuildBottomRhythmDeck(root);
            BuildMainMenu(root);
            BuildSettingsPanel(root);
            BuildResultPanel(root);
        }

        private void BuildTopStatus(Transform root)
        {
            var top = CreateConfiguredPanel(root, "top_status_bar", "Top Status Bar", Anchor.TopStretch, new Vector2(0f, -8f), new Vector2(-28f, 66f), new Color(0.02f, 0.018f, 0.028f, 0.88f));
            ApplyPanelArt(top, "ui_panel_battle");

            var heroPanel = CreatePanel(top.transform, "AI Character Info", Anchor.TopLeft, new Vector2(10f, -7f), new Vector2(690f, 52f), new Color(0.035f, 0.02f, 0.045f, 0.84f));
            ApplyPanelArt(heroPanel, "ui_panel_ai");
            CreateHeader(heroPanel.transform, "AI Character Header", "角色信息", NeonCyan);

            var creatorPanel = CreatePanel(top.transform, "Creator Info", Anchor.TopRight, new Vector2(-10f, -7f), new Vector2(470f, 52f), new Color(0.04f, 0.027f, 0.018f, 0.84f));
            ApplyPanelArt(creatorPanel, "ui_panel_creator");
            CreateHeader(creatorPanel.transform, "Creator Header", "造物主信息", NeonOrange);

            var centerPanel = CreatePanel(top.transform, "Battle State Info", Anchor.TopCenter, new Vector2(0f, -7f), new Vector2(430f, 52f), new Color(0.025f, 0.026f, 0.038f, 0.82f));
            ApplyPanelArt(centerPanel, "ui_panel_battle");
            CreateHeader(centerPanel.transform, "Battle Header", "战斗状态", NeonPurple);

            var portrait = CreateConfiguredPanel(heroPanel.transform, "ai_portrait", "AI Portrait", Anchor.TopLeft, new Vector2(12f, -8f), new Vector2(38f, 38f), new Color(0.08f, 0.04f, 0.12f, 0.95f));
            MusicManiacArtLibrary.ApplySpriteToImage(portrait.GetComponent<Image>(), MusicManiacArtLibrary.HeroSprite, Color.white);

            heroText = CreateConfiguredText(heroPanel.transform, "ai_summary", "AI Summary", Anchor.TopLeft, new Vector2(58f, -8f), new Vector2(238f, 38f), 13, TextAnchor.UpperLeft);
            var healthBack = CreateConfiguredBar(heroPanel.transform, "ai_hp_bar", "AI HP", Anchor.TopLeft, new Vector2(314f, -13f), new Vector2(342f, 15f), new Color(0.12f, 0.07f, 0.12f, 0.96f), NeonPink, out healthFill);
            healthPercentText = CreateBarText(healthBack.transform, "AI HP Percent", "100%");
            var shieldBack = CreateBar(healthBack.transform, "AI Shield", Anchor.BottomStretch, new Vector2(0f, -8f), new Vector2(0f, 6f), new Color(0.02f, 0.08f, 0.11f, 0.88f), NeonCyan, out shieldFill);
            shieldPercentText = CreateBarText(shieldBack.transform, "AI Shield Percent", "护盾 0%");
            healthValueText = CreateText(heroPanel.transform, "AI HP Value", Anchor.TopLeft, new Vector2(314f, -34f), new Vector2(342f, 16f), 11, TextAnchor.MiddleCenter);

            timerText = CreateConfiguredText(centerPanel.transform, "timer_text", "Timer", Anchor.TopCenter, new Vector2(-130f, -17f), new Vector2(118f, 30f), 23, TextAnchor.MiddleCenter);
            var threatBack = CreateConfiguredBar(centerPanel.transform, "threat_bar", "Threat", Anchor.TopLeft, new Vector2(184f, -19f), new Vector2(190f, 13f), new Color(0.1f, 0.07f, 0.04f, 0.9f), NeonOrange, out threatFill);
            threatText = CreateText(threatBack.transform, "Threat Label", Anchor.TopCenter, new Vector2(0f, -17f), new Vector2(190f, 18f), 11, TextAnchor.MiddleCenter);

            var summonEnergyBack = CreateConfiguredBar(creatorPanel.transform, "summon_energy_bar_top", "Summon Energy Top", Anchor.TopLeft, new Vector2(28f, -17f), new Vector2(188f, 13f), new Color(0.1f, 0.04f, 0.09f, 0.95f), NeonPink, out summonEnergyTopFill);
            summonEnergyTopText = CreateText(summonEnergyBack.transform, "Summon Energy Top Label", Anchor.TopCenter, new Vector2(0f, -15f), new Vector2(188f, 17f), 10, TextAnchor.MiddleCenter);
            var skillEnergyBack = CreateConfiguredBar(creatorPanel.transform, "skill_energy_bar_top", "Skill Energy Top", Anchor.TopLeft, new Vector2(244f, -17f), new Vector2(188f, 13f), new Color(0.1f, 0.065f, 0.035f, 0.95f), NeonOrange, out skillEnergyTopFill);
            skillEnergyTopText = CreateText(skillEnergyBack.transform, "Skill Energy Top Label", Anchor.TopCenter, new Vector2(0f, -15f), new Vector2(188f, 17f), 10, TextAnchor.MiddleCenter);
        }

        private void BuildLeftInfo(Transform root)
        {
            var left = CreateConfiguredPanel(root, "left_info_panel", "Left Info Panel", Anchor.TopLeft, new Vector2(10f, -82f), new Vector2(238f, 206f), PanelColor);
            ApplyPanelArt(left, "ui_panel_objective");
            CreateHeader(left.transform, "Objective Header", "战斗简报", NeonCyan);
            objectiveText = CreateConfiguredText(left.transform, "objective_text", "Objective", Anchor.TopLeft, new Vector2(14f, -30f), new Vector2(210f, 96f), 13, TextAnchor.UpperLeft);

            var msg = CreateConfiguredPanel(left.transform, "message_tape", "Message Tape", Anchor.BottomStretch, new Vector2(0f, 10f), new Vector2(-24f, 62f), new Color(0.04f, 0.02f, 0.04f, 0.9f));
            ApplyPanelArt(msg, "ui_bubble");
            messageText = CreateConfiguredText(msg.transform, "message_text", "Message", Anchor.Stretch, new Vector2(10f, -6f), new Vector2(-20f, -12f), 12, TextAnchor.UpperLeft);
            messageText.color = new Color(0.1f, 0.08f, 0.11f, 1f);
        }

        private void BuildRightActionDeck(Transform root)
        {
            var right = CreateConfiguredPanel(root, "right_action_deck", "Right Action Deck", Anchor.TopRight, new Vector2(-10f, -82f), new Vector2(432f, 812f), new Color(0.023f, 0.017f, 0.033f, 0.9f));
            ApplyPanelArt(right, "ui_panel_action");

            var aiPanel = CreateSection(right.transform, "AI State Section", "角色状态", new Vector2(0f, -10f), new Vector2(-18f, 78f), NeonCyan);
            aiStatusText = CreateText(aiPanel.transform, "AI Status", Anchor.TopLeft, new Vector2(12f, -28f), new Vector2(388f, 44f), 10, TextAnchor.UpperLeft);

            var summonPanel = CreateSection(right.transform, "Summon Section", "召唤怪物", new Vector2(0f, -96f), new Vector2(-18f, 366f), NeonPink);
            var summonEnergyBack = CreateBar(summonPanel.transform, "Summon Energy", Anchor.TopStretch, new Vector2(0f, -34f), new Vector2(-24f, 18f), new Color(0.1f, 0.04f, 0.09f, 0.95f), NeonPink, out summonEnergyFill);
            ApplyPanelArt(summonEnergyBack, "ui_rhythm_lane");
            summonEnergyText = CreateBarText(summonEnergyBack.transform, "Summon Energy Label", "召唤能量 0/0");
            summonEnergyText.fontSize = 12;
            var order = new[]
            {
                MonsterKind.Skeleton,
                MonsterKind.VenomBug,
                MonsterKind.Archer,
                MonsterKind.Stoneguard,
                MonsterKind.HexPriest,
                MonsterKind.Shieldbreaker,
                MonsterKind.Assassin,
                MonsterKind.BoneKing
            };

            var summonGroup = GetButtonGroup("summon_icon_buttons", new Vector2(120f, 96f), new Vector2(74f, 68f), new Vector2(130f, -100f), 3, 12, 10, 9, new Vector2(0f, -3f), new Vector2(50f, 50f));
            for (var i = 0; i < order.Length; i++)
            {
                var kind = order[i];
                var config = director.GetMonsterConfig(kind);
                var button = CreateActionButton(summonPanel.transform, config.DisplayName, $"召唤 {config.Cost:0}  {config.Tag}", "可用", ButtonPosition(summonGroup, i), summonGroup.buttonSize, NeonPink, summonGroup.labelFontSize, summonGroup.metaFontSize, summonGroup.statusFontSize);
                AddButtonIcon(button, MusicManiacArtLibrary.MonsterIcon(kind), summonGroup.iconPosition, summonGroup.iconSize);
                button.onClick.AddListener(() =>
                {
                    MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiClick, Vector2.zero, 0.7f);
                    director.SelectMonsterFromHud(kind);
                });
                buttons[kind] = button;
            }

            var skillPanel = CreateSection(right.transform, "Skill Section", "造物主技能", new Vector2(0f, -468f), new Vector2(-18f, 288f), NeonOrange);
            var skillEnergyBack = CreateBar(skillPanel.transform, "Skill Energy", Anchor.TopStretch, new Vector2(0f, -34f), new Vector2(-24f, 18f), new Color(0.1f, 0.065f, 0.035f, 0.95f), NeonOrange, out skillEnergyFill);
            ApplyPanelArt(skillEnergyBack, "ui_rhythm_lane");
            skillEnergyText = CreateBarText(skillEnergyBack.transform, "Skill Energy Label", "技能能量 0/0");
            skillEnergyText.fontSize = 12;
            var skillOrder = new[]
            {
                CreatorSkillId.LightningStrike,
                CreatorSkillId.FrostField,
                CreatorSkillId.AntiHealCurse,
                CreatorSkillId.ShieldBrand,
                CreatorSkillId.BoneWall,
                CreatorSkillId.DemonHand
            };

            var skillGroup = GetButtonGroup("skill_icon_buttons", new Vector2(120f, 96f), new Vector2(74f, 34f), new Vector2(130f, -100f), 3, 12, 10, 9, new Vector2(0f, -3f), new Vector2(50f, 50f));
            for (var i = 0; i < skillOrder.Length; i++)
            {
                var skillId = skillOrder[i];
                var config = director.GetSkillConfig(skillId);
                var button = CreateActionButton(skillPanel.transform, config.DisplayName, $"技能 {config.Cost:0}  {config.Tag}", "可用", ButtonPosition(skillGroup, i), skillGroup.buttonSize, NeonOrange, skillGroup.labelFontSize, skillGroup.metaFontSize, skillGroup.statusFontSize);
                AddButtonIcon(button, MusicManiacArtLibrary.SkillIcon(skillId), skillGroup.iconPosition, skillGroup.iconSize);
                SetButtonShortcut(button, (i + 1).ToString(), NeonOrange);
                button.onClick.AddListener(() =>
                {
                    MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiClick, Vector2.zero, 0.7f);
                    director.SelectSkillFromHud(skillId);
                });
                skillButtons[skillId] = button;
            }

            var buildPanel = CreateSection(right.transform, "Build Pressure Section", "角色流派压力", new Vector2(0f, -762f), new Vector2(-18f, 44f), NeonPurple);
            CreateBuildButton(buildPanel.transform, "火力", 0, new Vector2(78f, -13f));
            CreateBuildButton(buildPanel.transform, "吸取", 1, new Vector2(208f, -13f));
            CreateBuildButton(buildPanel.transform, "护盾", 2, new Vector2(338f, -13f));

            var settingsButton = CreateButton(right.transform, "暂停", new Vector2(358f, -790f), new Vector2(72f, 32f), 11);
            settingsButton.onClick.AddListener(() =>
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiClick, Vector2.zero, 0.7f);
                OpenSettings();
            });
        }

        private void BuildBottomRhythmDeck(Transform root)
        {
            var bottom = CreateConfiguredPanel(root, "bottom_rhythm_deck", "Bottom Rhythm Deck", Anchor.BottomStretch, new Vector2(-26f, 8f), new Vector2(-500f, 166f), new Color(0.018f, 0.017f, 0.024f, 0.9f));
            ApplyPanelArt(bottom, "ui_rhythm_bar");

            rhythmLaneFills.Clear();
            rhythmBeatMarkers.Clear();

            var rhythmPanel = CreateSection(bottom.transform, "Rhythm Timeline Section", "角色音轨节奏", new Vector2(12f, -10f), new Vector2(1034f, 140f), NeonPink, Anchor.TopLeft);
            var attackBlock = CreatePanel(rhythmPanel.transform, "AI Attack Block", Anchor.TopLeft, new Vector2(14f, -28f), new Vector2(408f, 26f), new Color(0.12f, 0.035f, 0.09f, 0.8f));
            ApplyPanelArt(attackBlock, "ui_rhythm_lane");
            rhythmText = CreateConfiguredText(attackBlock.transform, "rhythm_header", "Rhythm Header", Anchor.Stretch, new Vector2(10f, -2f), new Vector2(-18f, -4f), 11, TextAnchor.MiddleLeft);

            var phaseBlock = CreatePanel(rhythmPanel.transform, "AI Rhythm Phase Block", Anchor.TopLeft, new Vector2(430f, -28f), new Vector2(130f, 26f), new Color(0.025f, 0.09f, 0.12f, 0.76f));
            ApplyPanelArt(phaseBlock, "ui_rhythm_lane");
            rhythmPhaseText = CreateText(phaseBlock.transform, "Phase Label", Anchor.Stretch, new Vector2(10f, -2f), new Vector2(-18f, -4f), 11, TextAnchor.MiddleLeft);
            rhythmPhaseText.text = "阶段：准备";

            var cueBlock = CreatePanel(rhythmPanel.transform, "AI Rhythm Cue Block", Anchor.TopLeft, new Vector2(568f, -28f), new Vector2(286f, 26f), new Color(0.08f, 0.055f, 0.11f, 0.78f));
            ApplyPanelArt(cueBlock, "ui_rhythm_lane");
            rhythmCueText = CreateText(cueBlock.transform, "Cue Label", Anchor.Stretch, new Vector2(10f, -2f), new Vector2(-18f, -4f), 11, TextAnchor.MiddleLeft);
            rhythmCueText.text = "下一拍：等待";

            var syncBlock = CreatePanel(rhythmPanel.transform, "AI Rhythm Sync Block", Anchor.TopLeft, new Vector2(862f, -28f), new Vector2(156f, 26f), new Color(0.035f, 0.055f, 0.11f, 0.78f));
            ApplyPanelArt(syncBlock, "ui_rhythm_lane");
            rhythmSyncText = CreateText(syncBlock.transform, "Sync Label", Anchor.TopLeft, new Vector2(8f, -2f), new Vector2(66f, 12f), 10, TextAnchor.MiddleLeft);
            rhythmSyncText.text = "同步 0%";
            rhythmBeatCountText = CreateText(syncBlock.transform, "Beat Count Label", Anchor.TopRight, new Vector2(-8f, -2f), new Vector2(74f, 12f), 10, TextAnchor.MiddleRight);
            rhythmBeatCountText.text = "节拍 0/0";

            CreateRhythmLane(rhythmPanel.transform, RhythmPitch.High, "track_high", "高音", "连奏", -60f, new Color(1f, 0.92f, 0.25f, 0.95f));
            CreateRhythmLane(rhythmPanel.transform, RhythmPitch.Mid, "track_mid", "中音", "直射", -80f, new Color(0.35f, 0.78f, 1f, 0.95f));
            CreateRhythmLane(rhythmPanel.transform, RhythmPitch.Low, "track_low", "低音", "冲击", -100f, new Color(1f, 0.24f, 0.12f, 0.95f));

            rhythmTracksText = CreateConfiguredText(rhythmPanel.transform, "rhythm_tracks", "Rhythm Tracks", Anchor.TopLeft, new Vector2(16f, -120f), new Vector2(88f, 16f), 10, TextAnchor.MiddleLeft);
            rhythmTracksText.text = "音效同步";

            var progressBack = CreateBar(rhythmPanel.transform, "Rhythm Progress", Anchor.TopLeft, new Vector2(RhythmTrackX, -120f), new Vector2(RhythmTrackWidth, 16f), new Color(0.08f, 0.06f, 0.1f, 0.78f), NeonPink, out rhythmFill);
            ApplyPanelArt(progressBack, "ui_rhythm_lane");
            CreateProgressSegment(progressBack.transform, "Windup Zone", "预备", 0f, RhythmTrackWidth * 0.2f, NeonCyan);
            CreateProgressSegment(progressBack.transform, "Barrage Zone", "弹幕", RhythmTrackWidth * 0.2f, RhythmTrackWidth * 0.66f, NeonPink);
            CreateProgressSegment(progressBack.transform, "Recovery Zone", "收招", RhythmTrackWidth * 0.86f, RhythmTrackWidth * 0.14f, new Color(0.35f, 0.95f, 0.52f, 0.95f));

            for (var i = 0; i < RhythmBeatMarkerCapacity; i++)
            {
                var marker = CreatePanel(rhythmPanel.transform, $"Rhythm Beat Marker {i:00}", Anchor.TopLeft, Vector2.zero, new Vector2(8f, 14f), Color.white).GetComponent<Image>();
                marker.raycastTarget = false;
                marker.gameObject.SetActive(false);
                rhythmBeatMarkers.Add(marker);
            }

            rhythmCursor = CreatePanel(rhythmPanel.transform, "Rhythm Cursor", Anchor.TopLeft, new Vector2(RhythmTrackX, -58f), new Vector2(4f, 62f), new Color(1f, 1f, 1f, 0.88f)).GetComponent<Image>();
            rhythmCursor.raycastTarget = false;

            var bdPanel = CreateSection(bottom.transform, "BD Strip Section", "当前构筑", new Vector2(-12f, -10f), new Vector2(382f, 140f), NeonCyan, Anchor.TopRight);
            bdListText = CreateConfiguredText(bdPanel.transform, "bd_title", "BD Title", Anchor.TopLeft, new Vector2(12f, -28f), new Vector2(350f, 18f), 10, TextAnchor.MiddleLeft);
            var bdGroup = GetButtonGroup("bd_buttons", new Vector2(68f, 43f), new Vector2(42f, 18f), new Vector2(73f, -48f), 5, 9, 8, 8, Vector2.zero, Vector2.zero);
            for (var i = 0; i < 10; i++)
            {
                var index = i;
                var view = CreateBDCard(bdPanel.transform, ButtonPosition(bdGroup, i), bdGroup.buttonSize);
                var button = view.Button;
                button.onClick.AddListener(() => pinnedBdIndex = pinnedBdIndex == index ? -1 : index);
                var hover = button.gameObject.AddComponent<BDHoverTarget>();
                hover.Initialize(this, index);
                bdButtons.Add(button);
                bdCardViews.Add(view);
            }

            bdTooltipPanel = CreateConfiguredPanel(root, "bd_tooltip", "AI BD Tooltip", Anchor.BottomLeft, new Vector2(264f, 186f), new Vector2(462f, 244f), new Color(0.025f, 0.03f, 0.035f, 0.96f));
            ApplyPanelArt(bdTooltipPanel, "ui_panel");
            bdTooltipAccent = CreatePanel(bdTooltipPanel.transform, "BD Tooltip Accent", Anchor.TopStretch, new Vector2(0f, -5f), new Vector2(-12f, 5f), NeonCyan).GetComponent<Image>();
            bdTooltipAccent.raycastTarget = false;
            bdTooltipTitleText = CreateText(bdTooltipPanel.transform, "BD Tooltip Title", Anchor.TopLeft, new Vector2(16f, -18f), new Vector2(300f, 24f), 16, TextAnchor.MiddleLeft);
            bdTooltipTitleText.fontStyle = FontStyle.Bold;
            bdTooltipMetaText = CreateText(bdTooltipPanel.transform, "BD Tooltip Meta", Anchor.TopRight, new Vector2(-16f, -20f), new Vector2(180f, 20f), 11, TextAnchor.MiddleRight);
            bdTooltipText = CreateConfiguredText(bdTooltipPanel.transform, "bd_tooltip_text", "BD Tooltip Text", Anchor.TopLeft, new Vector2(16f, -52f), new Vector2(430f, 176f), 11, TextAnchor.UpperLeft);
            bdTooltipPanel.SetActive(false);
        }

        private void BuildMainMenu(Transform root)
        {
            mainMenuPanel = CreateConfiguredPanel(root, "main_menu", "Main Menu", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.01f, 0.012f, 0.016f, 0.96f));

            var promo = CreateConfiguredPanel(mainMenuPanel.transform, "main_promo_image", "Main Promo Image", Anchor.TopRight, new Vector2(-96f, -116f), new Vector2(980f, 552f), new Color(1f, 1f, 1f, 1f));
            MusicManiacArtLibrary.ApplySpriteToImage(promo.GetComponent<Image>(), MusicManiacArtLibrary.Ui("promo_main"), Color.white);

            var menuCard = CreateConfiguredPanel(mainMenuPanel.transform, "main_menu_card", "Main Menu Card", Anchor.TopLeft, new Vector2(110f, -128f), new Vector2(560f, 604f), new Color(0.025f, 0.021f, 0.034f, 0.94f));
            ApplyPanelArt(menuCard, "ui_panel");
            CreateHeader(menuCard.transform, "Main Menu Header", "挑战选择", NeonOrange);

            var title = CreateConfiguredText(menuCard.transform, "main_title", "Main Menu Title", Anchor.TopLeft, new Vector2(32f, -56f), new Vector2(496f, 72f), 38, TextAnchor.MiddleLeft);
            title.text = "击败音乐疯子";
            title.color = new Color(1f, 0.86f, 0.42f, 1f);
            uiConfig?.ApplyText(title, "main_title");

            var subtitle = CreateConfiguredText(menuCard.transform, "main_subtitle", "Main Menu Subtitle", Anchor.TopLeft, new Vector2(32f, -136f), new Vector2(496f, 86f), 17, TextAnchor.UpperLeft);
            subtitle.text = "选择挑战模式。召唤怪物、释放造物主技能，在倒计时结束前击败音乐疯子。";
            subtitle.color = new Color(0.78f, 0.88f, 0.92f, 1f);
            uiConfig?.ApplyText(subtitle, "main_subtitle");

            var easyButton = CreateModeButton(menuCard.transform, "简单模式", "1分钟挑战 / 当前血量", new Vector2(0f, -280f), NeonCyan);
            easyButton.onClick.AddListener(() =>
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiConfirm, Vector2.zero, 0.85f);
                director.StartGame(GameMode.Easy);
                CloseSettings(false);
                mainMenuPanel.SetActive(false);
            });

            var hardButton = CreateModeButton(menuCard.transform, "困难模式", "5分钟挑战 / 角色血量 x5", new Vector2(0f, -376f), NeonOrange);
            hardButton.onClick.AddListener(() =>
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiConfirm, Vector2.zero, 0.85f);
                director.StartGame(GameMode.Hard);
                CloseSettings(false);
                mainMenuPanel.SetActive(false);
            });

            var tipPanel = CreatePanel(menuCard.transform, "Main Tip Panel", Anchor.TopLeft, new Vector2(32f, -484f), new Vector2(496f, 82f), new Color(0.05f, 0.045f, 0.065f, 0.86f));
            ApplyPanelArt(tipPanel, "ui_bubble");
            var tip = CreateConfiguredText(tipPanel.transform, "main_tip", "Main Menu Tip", Anchor.Stretch, new Vector2(16f, -10f), new Vector2(-26f, -18f), 13, TextAnchor.UpperLeft);
            tip.text = "简单模式适合快速测试；困难模式时间更长、血量更高。进入战斗后在右侧选择怪物或技能，点击战场释放。";
            tip.color = new Color(0.58f, 0.66f, 0.72f, 1f);
            uiConfig?.ApplyText(tip, "main_tip");
        }

        private void BuildSettingsPanel(Transform root)
        {
            settingsPanel = CreateConfiguredPanel(root, "settings_panel", "Settings Panel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.005f, 0.006f, 0.009f, 0.94f));
            var promo = CreateConfiguredPanel(settingsPanel.transform, "settings_promo_image", "Settings Promo Image", Anchor.TopRight, new Vector2(-116f, -170f), new Vector2(820f, 462f), Color.white);
            MusicManiacArtLibrary.ApplySpriteToImage(promo.GetComponent<Image>(), MusicManiacArtLibrary.Ui("promo_settings"), Color.white);

            var pauseCard = CreateConfiguredPanel(settingsPanel.transform, "settings_menu_card", "Settings Menu Card", Anchor.TopLeft, new Vector2(168f, -206f), new Vector2(500f, 432f), new Color(0.025f, 0.024f, 0.034f, 0.96f));
            ApplyPanelArt(pauseCard, "ui_panel");
            CreateHeader(pauseCard.transform, "Settings Header", "暂停菜单", NeonCyan);

            var title = CreateText(pauseCard.transform, "Settings Title", Anchor.TopLeft, new Vector2(32f, -58f), new Vector2(430f, 52f), 32, TextAnchor.MiddleLeft);
            title.text = "游戏已暂停";
            title.color = new Color(1f, 0.86f, 0.42f, 1f);

            var body = CreateText(pauseCard.transform, "Settings Body", Anchor.TopLeft, new Vector2(32f, -122f), new Vector2(430f, 86f), 15, TextAnchor.UpperLeft);
            body.text = "可以继续当前战斗，也可以返回主界面重新选择简单模式或困难模式。返回主界面会清空当前战场。";
            body.color = new Color(0.78f, 0.88f, 0.92f, 1f);

            var resumeButton = CreateButton(pauseCard.transform, "继续游戏", new Vector2(0f, -246f), new Vector2(380f, 58f), 18);
            SetConfiguredRect(resumeButton.gameObject, "settings_resume_button", Anchor.TopCenter, new Vector2(0f, -246f), new Vector2(380f, 58f));
            resumeButton.onClick.AddListener(() =>
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiConfirm, Vector2.zero, 0.85f);
                CloseSettings(true);
            });

            var mainButton = CreateButton(pauseCard.transform, "返回主界面", new Vector2(0f, -322f), new Vector2(380f, 58f), 18);
            SetConfiguredRect(mainButton.gameObject, "settings_main_button", Anchor.TopCenter, new Vector2(0f, -322f), new Vector2(380f, 58f));
            mainButton.onClick.AddListener(() =>
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiConfirm, Vector2.zero, 0.85f);
                director.ReturnToMainMenu();
            });

            settingsPanel.SetActive(false);
        }

        private Button CreateModeButton(Transform parent, string title, string subtitle, Vector2 position, Color accent)
        {
            var buttonObject = new GameObject($"{title} Button");
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.08f, 0.07f, 0.11f, 0.98f);
            MusicManiacArtLibrary.ApplySpriteToImage(image, MusicManiacArtLibrary.Ui("ui_button"), image.color);
            image.preserveAspect = false;

            var button = buttonObject.AddComponent<Button>();
            var configId = title.Contains("困难") ? "main_hard_button" : "main_easy_button";
            SetConfiguredRect(buttonObject, configId, Anchor.TopCenter, position, new Vector2(496f, 82f));
            var buttonSize = buttonObject.GetComponent<RectTransform>().sizeDelta;
            var contentWidth = Mathf.Max(220f, buttonSize.x - 34f);

            var accentLine = CreatePanel(buttonObject.transform, "Accent", Anchor.TopStretch, new Vector2(0f, -5f), new Vector2(-18f, 5f), accent).GetComponent<Image>();
            accentLine.raycastTarget = false;

            var titleText = CreateText(buttonObject.transform, "Title", Anchor.TopCenter, new Vector2(0f, -15f), new Vector2(contentWidth, 30f), 20, TextAnchor.MiddleCenter);
            titleText.text = title;
            titleText.color = Color.Lerp(Color.white, accent, 0.18f);
            titleText.fontStyle = FontStyle.Bold;
            titleText.raycastTarget = false;

            var subtitleText = CreateText(buttonObject.transform, "Subtitle", Anchor.TopCenter, new Vector2(0f, -47f), new Vector2(contentWidth, 22f), 13, TextAnchor.MiddleCenter);
            subtitleText.text = subtitle;
            subtitleText.color = new Color(0.78f, 0.84f, 0.9f, 0.96f);
            subtitleText.raycastTarget = false;

            buttonImages[button] = image;
            buttonLabels[button] = titleText;
            return button;
        }

        private void OpenSettings()
        {
            if (settingsPanel == null || !director.IsGameStarted || director.IsFinished)
            {
                return;
            }

            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling();
            Time.timeScale = 0f;
        }

        private void CloseSettings(bool resumeGame)
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (resumeGame && director.IsGameStarted && !director.IsFinished)
            {
                Time.timeScale = 1f;
            }
        }

        private void BuildResultPanel(Transform root)
        {
            resultPanel = CreateConfiguredPanel(root, "result_panel", "Result Panel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.008f, 0.009f, 0.012f, 0.94f));
            var promo = CreateConfiguredPanel(resultPanel.transform, "result_promo_image", "Result Promo Image", Anchor.TopRight, new Vector2(-96f, -116f), new Vector2(980f, 552f), Color.white);
            resultPromoImage = promo.GetComponent<Image>();
            MusicManiacArtLibrary.ApplySpriteToImage(resultPromoImage, MusicManiacArtLibrary.Ui("promo_victory"), Color.white);

            var resultCard = CreateConfiguredPanel(resultPanel.transform, "result_menu_card", "Result Menu Card", Anchor.TopLeft, new Vector2(120f, -166f), new Vector2(540f, 500f), new Color(0.025f, 0.022f, 0.033f, 0.96f));
            ApplyPanelArt(resultCard, "ui_panel");
            CreateHeader(resultCard.transform, "Result Header", "战斗结算", NeonOrange);

            resultTitleText = CreateConfiguredText(resultCard.transform, "result_title", "Result Title", Anchor.TopLeft, new Vector2(34f, -66f), new Vector2(468f, 78f), 40, TextAnchor.MiddleLeft);
            resultBodyText = CreateConfiguredText(resultCard.transform, "result_body", "Result Body", Anchor.TopLeft, new Vector2(34f, -156f), new Vector2(468f, 150f), 17, TextAnchor.UpperLeft);

            var restartButton = CreateButton(resultCard.transform, "重新开始", Vector2.zero, new Vector2(400f, 58f), 18);
            SetConfiguredRect(restartButton.gameObject, "result_restart_button", Anchor.TopCenter, new Vector2(0f, -366f), new Vector2(400f, 58f));
            restartButton.onClick.AddListener(() =>
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiConfirm, Vector2.zero, 0.85f);
                director.ReturnToMainMenu();
            });
            resultPanel.SetActive(false);
        }

        private void RefreshResultPanel()
        {
            if (resultPanel == null)
            {
                return;
            }

            resultPanel.SetActive(director.IsFinished);
            if (!director.IsFinished)
            {
                return;
            }

            resultTitleText.text = director.IsVictory ? "胜利" : "失败";
            resultTitleText.color = director.IsVictory ? new Color(1f, 0.86f, 0.32f, 1f) : new Color(1f, 0.28f, 0.32f, 1f);
            resultBodyText.text = director.IsVictory
                ? $"造物主胜利！音乐疯子已被击败。\n\n模式：{director.CurrentModeName}\n用时：{director.BattleTimer:0.0} 秒\n\n点击重新开始返回主界面，重新选择挑战模式。"
                : $"挑战失败。{GameDirector.FormatDuration(director.BattleDuration)}倒计时结束，音乐疯子仍然存活。\n\n模式：{director.CurrentModeName}\n\n点击重新开始返回主界面，调整节奏后再来一局。";
            resultBodyText.color = new Color(0.86f, 0.9f, 0.92f, 1f);
            MusicManiacArtLibrary.ApplySpriteToImage(resultPromoImage, MusicManiacArtLibrary.Ui(director.IsVictory ? "promo_victory" : "promo_defeat"), Color.white);
        }

        private void CreateBuildButton(Transform root, string label, int buildIndex, Vector2 position)
        {
            var button = CreateButton(root, label, position, new Vector2(84f, 38f), 10);
            button.onClick.AddListener(() =>
            {
                director.ForceHeroBuild(buildIndex);
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiConfirm, Vector2.zero, 0.65f);
            });
        }

        public void SetHoveredBD(int index, bool hovering)
        {
            if (hovering)
            {
                hoveredBdIndex = index;
            }
            else if (hoveredBdIndex == index)
            {
                hoveredBdIndex = -1;
            }
        }

        private void RefreshButtonStates()
        {
            foreach (var pair in buttons)
            {
                var config = director.GetMonsterConfig(pair.Key);
                var affordable = director.CanAffordMonster(pair.Key);
                var selected = pair.Key == director.SelectedMonster && !director.IsAimingSkill;
                var label = GetButtonLabel(pair.Value);
                if (label != null)
                {
                    label.text = config.DisplayName;
                    label.color = affordable ? Color.white : new Color(0.62f, 0.62f, 0.68f);
                }

                SetButtonMeta(pair.Value, $"召唤 {config.Cost:0}  {config.Tag}", affordable);
                SetButtonStatus(pair.Value, affordable ? "可用" : "不足", affordable, selected);
                SetButtonVisual(pair.Value, affordable, selected, new Color(0.75f, 0.28f, 0.62f), NeonPink);
            }

            foreach (var pair in skillButtons)
            {
                var config = director.GetSkillConfig(pair.Key);
                var cooldown = director.GetSkillCooldownRemaining(pair.Key);
                var usable = director.CanPrepareSkill(pair.Key);
                var selected = pair.Key == director.SelectedSkill && director.IsAimingSkill;
                var buttonText = GetButtonLabel(pair.Value);
                if (buttonText != null)
                {
                    buttonText.text = config.DisplayName;
                    buttonText.color = usable ? Color.white : new Color(0.62f, 0.62f, 0.68f);
                }

                var state = cooldown > 0f ? $"冷却{cooldown:0}秒" : config.Cost > director.SkillEnergy ? "不足" : "可用";
                SetButtonMeta(pair.Value, $"技能 {config.Cost:0}  {config.Tag}", usable);
                SetButtonStatus(pair.Value, state, usable, selected);
                SetButtonVisual(pair.Value, usable, selected, new Color(0.72f, 0.18f, 0.32f), NeonOrange);
            }
        }

        private void RefreshRhythmPanel(AIRhythmController rhythm)
        {
            var progress = rhythm.Progress01;
            var progressColor = GetRhythmProgressColor(rhythm);
            rhythmText.text = $"当前攻击：{rhythm.AttackName}   类型：{rhythm.AttackType}   危险 {rhythm.DangerLevel}";
            rhythmPhaseText.text = $"阶段：{rhythm.PhaseName}";
            rhythmCueText.text = rhythm.IsAttacking
                ? $"下一拍：{rhythm.NextBeatPitchName}  {rhythm.NextBeatRemaining:0.0}秒后触发"
                : $"下一拍：{rhythm.GetNextBeatText()}";
            rhythmSyncText.text = $"同步 {progress:P0}";
            rhythmBeatCountText.text = $"节拍 {rhythm.TriggeredBeatCount}/{rhythm.TotalBeatCount}";
            rhythmTracksText.text = rhythm.IsInEndLag ? $"收招 {rhythm.EndLagRemaining:0.0}秒" : "音效同步";

            rhythmFill.fillAmount = progress;
            rhythmFill.color = progressColor;
            UpdateRhythmCursor(progress, progressColor);

            rhythm.FillTimelineBeats(rhythmBeatCache);
            UpdateRhythmLaneFills(rhythm, progress);
            UpdateRhythmBeatMarkers();
        }

        private void UpdateRhythmCursor(float progress, Color color)
        {
            if (rhythmCursor == null)
            {
                return;
            }

            var rect = rhythmCursor.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(RhythmTrackX + RhythmTrackWidth * Mathf.Clamp01(progress), -58f);
            rhythmCursor.color = new Color(color.r, color.g, color.b, 0.92f);
        }

        private void UpdateRhythmLaneFills(AIRhythmController rhythm, float progress)
        {
            for (var i = 0; i < rhythmLaneFills.Count; i++)
            {
                var fill = rhythmLaneFills[i];
                if (fill == null)
                {
                    continue;
                }

                var pitch = GetLanePitch(i);
                var active = rhythm.IsAttacking && (rhythm.NextBeatPitch == pitch || CountBeatsForPitch(pitch) > 0);
                fill.fillAmount = rhythm.IsAttacking ? progress : 0f;
                fill.color = active ? Color.Lerp(GetPitchColor(pitch), Color.white, 0.12f) : new Color(0.16f, 0.17f, 0.2f, 0.42f);
            }
        }

        private void UpdateRhythmBeatMarkers()
        {
            for (var i = 0; i < rhythmBeatMarkers.Count; i++)
            {
                var marker = rhythmBeatMarkers[i];
                if (marker == null)
                {
                    continue;
                }

                if (i >= rhythmBeatCache.Count)
                {
                    marker.gameObject.SetActive(false);
                    continue;
                }

                var beat = rhythmBeatCache[i];
                marker.gameObject.SetActive(true);
                var rect = marker.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(RhythmTrackX + RhythmTrackWidth * beat.Progress, GetPitchMarkerY(beat.Pitch));
                rect.sizeDelta = beat.Next ? new Vector2(12f, 18f) : beat.Strength >= 1.6f ? new Vector2(10f, 16f) : new Vector2(7f, 12f);
                var color = GetPitchColor(beat.Pitch);
                if (beat.Triggered)
                {
                    color = Color.Lerp(color, Color.white, 0.72f);
                    color.a = 0.48f;
                }
                else if (beat.Next)
                {
                    color = Color.Lerp(color, Color.white, Mathf.PingPong(Time.time * 4f, 0.45f));
                    color.a = 1f;
                }
                else
                {
                    color.a = 0.9f;
                }

                marker.color = color;
            }
        }

        private int CountBeatsForPitch(RhythmPitch pitch)
        {
            var count = 0;
            for (var i = 0; i < rhythmBeatCache.Count; i++)
            {
                if (rhythmBeatCache[i].Pitch == pitch)
                {
                    count++;
                }
            }

            return count;
        }

        private void SetButtonVisual(Button button, bool usable, bool selected, Color selectedColor, Color accentColor)
        {
            var baseColor = selected ? selectedColor : usable ? new Color(0.16f, 0.12f, 0.24f, 0.98f) : new Color(0.06f, 0.06f, 0.075f, 0.82f);
            var highlighted = usable ? new Color(0.36f, 0.2f, 0.5f, 0.98f) : new Color(0.09f, 0.09f, 0.11f, 0.82f);
            var pressed = usable ? new Color(0.9f, 0.28f, 0.42f, 0.98f) : new Color(0.12f, 0.08f, 0.1f, 0.86f);

            var colors = button.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = highlighted;
            colors.selectedColor = selected ? selectedColor : baseColor;
            colors.pressedColor = pressed;
            colors.disabledColor = new Color(0.04f, 0.04f, 0.05f, 0.72f);
            button.colors = colors;

            var image = GetButtonImage(button);
            if (image != null)
            {
                image.color = baseColor;
            }

            if (buttonIcons.TryGetValue(button, out var icon) && icon != null)
            {
                icon.color = usable ? Color.white : new Color(0.42f, 0.42f, 0.48f, 0.72f);
            }

            if (buttonIconSlots.TryGetValue(button, out var iconSlot) && iconSlot != null)
            {
                iconSlot.color = usable ? new Color(0.92f, 0.92f, 1f, 1f) : new Color(0.28f, 0.28f, 0.34f, 0.82f);
            }

            if (buttonAccentBars.TryGetValue(button, out var accent) && accent != null)
            {
                accent.color = usable ? accentColor : new Color(0.2f, 0.2f, 0.24f, 0.78f);
            }

            if (buttonDisabledOverlays.TryGetValue(button, out var overlay) && overlay != null)
            {
                overlay.gameObject.SetActive(!usable);
            }

            if (buttonSelectionFrames.TryGetValue(button, out var frame) && frame != null)
            {
                frame.gameObject.SetActive(selected);
                frame.color = Color.Lerp(Color.white, accentColor, 0.25f);
            }

            if (buttonShortcutBadges.TryGetValue(button, out var shortcutBadge) && shortcutBadge != null)
            {
                shortcutBadge.color = selected
                    ? Color.Lerp(accentColor, Color.white, 0.18f)
                    : usable
                        ? new Color(accentColor.r * 0.22f, accentColor.g * 0.22f, accentColor.b * 0.22f, 0.94f)
                        : new Color(0.12f, 0.12f, 0.14f, 0.78f);
            }

            if (buttonShortcutLabels.TryGetValue(button, out var shortcutLabel) && shortcutLabel != null)
            {
                shortcutLabel.color = selected || usable
                    ? Color.Lerp(Color.white, accentColor, selected ? 0.06f : 0.2f)
                    : new Color(0.46f, 0.46f, 0.52f, 0.9f);
            }
        }

        private void SetButtonMeta(Button button, string text, bool usable)
        {
            if (buttonMetaLabels.TryGetValue(button, out var label) && label != null)
            {
                label.text = text;
                label.color = usable ? new Color(0.76f, 0.82f, 0.9f, 0.96f) : new Color(0.42f, 0.42f, 0.48f, 0.88f);
            }
        }

        private void SetButtonStatus(Button button, string status, bool usable, bool selected)
        {
            if (buttonStatusLabels.TryGetValue(button, out var label) && label != null)
            {
                label.text = selected ? "已选" : status;
                label.color = usable || selected ? Color.white : new Color(0.58f, 0.58f, 0.64f, 0.92f);
            }

            if (buttonStatusBadges.TryGetValue(button, out var badge) && badge != null)
            {
                if (selected)
                {
                    badge.color = new Color(1f, 0.76f, 0.22f, 0.92f);
                    ApplyPanelArt(badge.gameObject, "ui_status_armed");
                }
                else if (usable)
                {
                    badge.color = new Color(0.08f, 0.42f, 0.26f, 0.88f);
                    ApplyPanelArt(badge.gameObject, status.StartsWith("冷却") ? "ui_status_cd" : "ui_status_ready");
                }
                else
                {
                    badge.color = new Color(0.28f, 0.08f, 0.1f, 0.88f);
                    ApplyPanelArt(badge.gameObject, status.StartsWith("冷却") ? "ui_status_cd" : "ui_status_low");
                }
            }
        }

        private void SetButtonShortcut(Button button, string shortcut, Color accent)
        {
            if (button == null || string.IsNullOrEmpty(shortcut))
            {
                return;
            }

            if (!buttonShortcutBadges.TryGetValue(button, out var badge) || badge == null)
            {
                badge = CreatePanel(button.transform, "Shortcut Badge", Anchor.BottomStretch, new Vector2(0f, -18f), new Vector2(-20f, 16f), new Color(accent.r * 0.18f, accent.g * 0.18f, accent.b * 0.18f, 0.94f)).GetComponent<Image>();
                ApplyPanelArt(badge.gameObject, "ui_status_ready");
                badge.raycastTarget = false;
                buttonShortcutBadges[button] = badge;

                var label = CreateText(badge.transform, "Shortcut Label", Anchor.Stretch, new Vector2(4f, -1f), new Vector2(-8f, -2f), 9, TextAnchor.MiddleCenter);
                label.fontStyle = FontStyle.Bold;
                label.raycastTarget = false;
                buttonShortcutLabels[button] = label;
            }

            badge.color = new Color(accent.r * 0.2f, accent.g * 0.2f, accent.b * 0.2f, 0.94f);
            if (buttonShortcutLabels.TryGetValue(button, out var shortcutLabel) && shortcutLabel != null)
            {
                shortcutLabel.text = $"快捷键 {shortcut}";
                shortcutLabel.color = Color.Lerp(Color.white, accent, 0.2f);
            }
        }

        private void RefreshBDList(AIHeroBuildData buildData)
        {
            bdDisplayCache.Clear();
            bdDisplayCache.AddRange(buildData.GetDisplayCards());
            bdListText.text = $"当前构筑  {buildData.BuildName}   {bdDisplayCache.Count}/20   标签：{buildData.ElementTags}";

            for (var i = 0; i < bdCardViews.Count; i++)
            {
                var view = bdCardViews[i];
                if (i >= bdDisplayCache.Count)
                {
                    view.Button.gameObject.SetActive(false);
                    continue;
                }

                view.Button.gameObject.SetActive(true);
                var data = bdDisplayCache[i];
                RefreshBDCard(view, data, i == pinnedBdIndex || i == hoveredBdIndex);
            }

            var tooltipIndex = pinnedBdIndex >= 0 ? pinnedBdIndex : hoveredBdIndex;
            var shouldShow = tooltipIndex >= 0 && tooltipIndex < bdDisplayCache.Count;
            bdTooltipPanel.SetActive(shouldShow);
            if (shouldShow)
            {
                RefreshBDTooltip(bdDisplayCache[tooltipIndex]);
            }
        }

        private void RefreshBDCard(BDCardView view, AIBDDisplayData data, bool selected)
        {
            var color = data.Color;
            var pulse = data.IsNew ? Mathf.PingPong(Time.time * 4.5f, 0.5f) : 0f;
            var backColor = Color.Lerp(new Color(0.055f, 0.05f, 0.075f, 0.96f), new Color(color.r * 0.34f, color.g * 0.34f, color.b * 0.34f, 0.98f), data.IsCore ? 0.82f : 0.48f);
            if (data.IsNew)
            {
                backColor = Color.Lerp(backColor, Color.white, pulse * 0.22f);
            }

            view.Background.color = backColor;
            view.IconBack.color = Color.Lerp(new Color(0.06f, 0.06f, 0.08f, 0.96f), color, 0.6f + pulse * 0.25f);
            view.IconText.text = data.IconText;
            view.IconText.color = data.Element == ElementModule.None ? Color.white : new Color(0.05f, 0.05f, 0.07f, 1f);
            view.NameText.text = data.Name;
            view.NameText.color = data.IsCore ? new Color(1f, 0.92f, 0.58f, 1f) : Color.white;
            view.MetaText.text = $"{data.CategoryName} · {data.RarityName}";
            view.MetaText.color = Color.Lerp(Color.white, color, 0.35f);
            view.LevelText.text = $"Lv.{data.Level}";
            view.LevelText.color = data.Level >= 3 ? new Color(1f, 0.88f, 0.35f, 1f) : new Color(0.82f, 0.9f, 1f, 1f);
            view.Accent.color = data.IsNew ? Color.Lerp(color, Color.white, pulse) : color;
            view.NewBadge.gameObject.SetActive(data.IsNew);
            view.NewBadge.color = Color.Lerp(new Color(1f, 0.76f, 0.18f, 0.9f), Color.white, pulse);
            view.Selection.gameObject.SetActive(selected);
            view.Selection.color = Color.Lerp(Color.white, color, 0.22f);
        }

        private void RefreshBDTooltip(AIBDDisplayData data)
        {
            bdTooltipAccent.color = data.Color;
            bdTooltipTitleText.text = $"{data.Name}  Lv.{data.Level}";
            bdTooltipTitleText.color = data.IsCore ? new Color(1f, 0.9f, 0.46f, 1f) : Color.Lerp(Color.white, data.Color, 0.2f);
            bdTooltipMetaText.text = $"{data.CategoryName} / {data.ElementName} / {data.RarityName}";
            bdTooltipMetaText.color = Color.Lerp(Color.white, data.Color, 0.35f);
            bdTooltipText.text = BuildBDTooltip(data);
        }

        private static string BuildBDTooltip(AIBDDisplayData data)
        {
            return $"当前效果\n{data.Effect}\n\n" +
                   $"音效表现\n{data.Audio}\n\n" +
                   $"视觉表现\n{data.Vfx}\n\n" +
                   $"下级成长\n{data.NextLevel}\n\n" +
                   $"造物主应对\n{data.Counter}";
        }

        private static Color GetRhythmProgressColor(AIRhythmController rhythm)
        {
            if (rhythm.IsInEndLag)
            {
                return new Color(0.35f, 0.95f, 0.52f, 0.98f);
            }

            if (rhythm.Progress01 < 0.22f)
            {
                return NeonCyan;
            }

            if (rhythm.Progress01 > 0.9f)
            {
                return NeonOrange;
            }

            return NeonPink;
        }

        private static Color GetPitchColor(RhythmPitch pitch)
        {
            switch (pitch)
            {
                case RhythmPitch.Low:
                    return new Color(1f, 0.24f, 0.12f, 0.96f);
                case RhythmPitch.Mid:
                    return new Color(0.35f, 0.78f, 1f, 0.96f);
                case RhythmPitch.High:
                    return new Color(1f, 0.92f, 0.25f, 0.96f);
                default:
                    return Color.white;
            }
        }

        private static float GetPitchMarkerY(RhythmPitch pitch)
        {
            switch (pitch)
            {
                case RhythmPitch.High:
                    return -60f - 2f;
                case RhythmPitch.Mid:
                    return -80f - 2f;
                case RhythmPitch.Low:
                    return -100f - 2f;
                default:
                    return -80f;
            }
        }

        private static RhythmPitch GetLanePitch(int index)
        {
            switch (index)
            {
                case 0:
                    return RhythmPitch.High;
                case 1:
                    return RhythmPitch.Mid;
                case 2:
                    return RhythmPitch.Low;
                default:
                    return RhythmPitch.Mid;
            }
        }

        private static GameObject CreatePanel(Transform parent, string name, Anchor anchor, Vector2 position, Vector2 size, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = color;
            var rect = panel.GetComponent<RectTransform>();
            SetAnchor(rect, anchor);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return panel;
        }

        private GameObject CreateConfiguredPanel(Transform parent, string configId, string name, Anchor anchor, Vector2 position, Vector2 size, Color color)
        {
            var layout = uiConfig?.Layout(configId);
            var panel = CreatePanel(
                parent,
                name,
                layout != null ? uiConfig.ToHudAnchor(layout.anchor) : anchor,
                layout != null ? layout.position : position,
                layout != null ? layout.size : size,
                layout != null ? layout.backgroundColor : color);
            if (layout != null)
            {
                panel.SetActive(layout.visible);
            }

            return panel;
        }

        private static GameObject CreateSection(Transform parent, string name, string title, Vector2 position, Vector2 size, Color accent, Anchor anchor = Anchor.TopStretch)
        {
            var section = CreatePanel(parent, name, anchor, position, size, new Color(0.035f, 0.028f, 0.046f, 0.9f));
            ApplyPanelArt(section, "ui_bd_card");

            var titleBack = CreatePanel(section.transform, $"{name} Title Back", Anchor.TopStretch, new Vector2(0f, -4f), new Vector2(-12f, 22f), new Color(accent.r * 0.22f, accent.g * 0.22f, accent.b * 0.22f, 0.82f));
            ApplyPanelArt(titleBack, "ui_section_header");

            var accentStrip = CreatePanel(section.transform, $"{name} Accent", Anchor.TopLeft, new Vector2(8f, -7f), new Vector2(4f, 16f), accent);
            accentStrip.GetComponent<Image>().raycastTarget = false;

            var label = CreateText(section.transform, $"{name} Title", Anchor.TopLeft, new Vector2(18f, -4f), new Vector2(220f, 22f), 12, TextAnchor.MiddleLeft);
            label.text = title;
            label.color = Color.Lerp(Color.white, accent, 0.25f);
            return section;
        }

        private static void CreateTimelineZone(Transform parent, string name, string label, Vector2 position, Vector2 size, Color color)
        {
            var zone = CreatePanel(parent, name, Anchor.TopLeft, position, size, new Color(color.r * 0.22f, color.g * 0.22f, color.b * 0.22f, 0.68f));
            zone.GetComponent<Image>().raycastTarget = false;

            var zoneLabel = CreateText(zone.transform, $"{name} Label", Anchor.Stretch, new Vector2(4f, -1f), new Vector2(-8f, -2f), 9, TextAnchor.MiddleCenter);
            zoneLabel.text = label;
            zoneLabel.color = Color.Lerp(Color.white, color, 0.35f);
            zoneLabel.raycastTarget = false;
        }

        private void CreateRhythmLane(Transform parent, RhythmPitch pitch, string iconName, string label, string hint, float y, Color color)
        {
            var iconPanel = CreatePanel(parent, $"{label} Icon Panel", Anchor.TopLeft, new Vector2(18f, y + 4f), new Vector2(78f, 20f), new Color(color.r * 0.14f, color.g * 0.14f, color.b * 0.14f, 0.88f));
            ApplyPanelArt(iconPanel, "ui_rhythm_lane");

            var iconObject = new GameObject($"{label} Icon");
            iconObject.transform.SetParent(iconPanel.transform, false);
            var icon = iconObject.AddComponent<Image>();
            MusicManiacArtLibrary.ApplySpriteToImage(icon, MusicManiacArtLibrary.Icon(iconName), Color.white);
            icon.raycastTarget = false;
            var iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(5f, 0f);
            iconRect.sizeDelta = new Vector2(18f, 18f);

            var laneLabel = CreateText(iconPanel.transform, $"{label} Text", Anchor.Stretch, new Vector2(28f, -2f), new Vector2(-6f, -4f), 10, TextAnchor.MiddleLeft);
            laneLabel.text = $"{label} {hint}";
            laneLabel.color = Color.Lerp(Color.white, color, 0.28f);
            laneLabel.raycastTarget = false;

            var laneBack = CreatePanel(parent, $"{label} Lane Back", Anchor.TopLeft, new Vector2(RhythmTrackX, y), new Vector2(RhythmTrackWidth, RhythmLaneHeight), new Color(0.035f, 0.038f, 0.052f, 0.84f));
            ApplyPanelArt(laneBack, "ui_rhythm_lane");
            laneBack.GetComponent<Image>().raycastTarget = false;

            var laneFillObject = CreatePanel(laneBack.transform, $"{label} Lane Fill", Anchor.Stretch, Vector2.zero, Vector2.zero, color);
            var laneFill = laneFillObject.GetComponent<Image>();
            laneFill.type = Image.Type.Filled;
            laneFill.fillMethod = Image.FillMethod.Horizontal;
            laneFill.fillAmount = 0f;
            laneFill.color = new Color(color.r, color.g, color.b, 0.72f);
            laneFill.raycastTarget = false;
            rhythmLaneFills.Add(laneFill);

            for (var i = 1; i < 8; i++)
            {
                var tick = CreatePanel(parent, $"{label} Beat Tick {i}", Anchor.TopLeft, new Vector2(RhythmTrackX + RhythmTrackWidth * i / 8f, y + 1f), new Vector2(2f, RhythmLaneHeight - 2f), new Color(1f, 1f, 1f, 0.08f));
                tick.GetComponent<Image>().raycastTarget = false;
            }
        }

        private static void CreateProgressSegment(Transform parent, string name, string label, float x, float width, Color color)
        {
            var segment = CreatePanel(parent, name, Anchor.TopLeft, new Vector2(x, -1f), new Vector2(width, 14f), new Color(color.r * 0.18f, color.g * 0.18f, color.b * 0.18f, 0.38f));
            segment.GetComponent<Image>().raycastTarget = false;
            var segmentLabel = CreateText(segment.transform, $"{name} Label", Anchor.Stretch, new Vector2(2f, -1f), new Vector2(-4f, -2f), 9, TextAnchor.MiddleCenter);
            segmentLabel.text = label;
            segmentLabel.color = Color.Lerp(Color.white, color, 0.32f);
            segmentLabel.raycastTarget = false;
        }

        private static void CreateHeader(Transform parent, string name, string label, Color color)
        {
            var header = CreatePanel(parent, name, Anchor.TopStretch, new Vector2(0f, -2f), new Vector2(-10f, 18f), new Color(color.r * 0.18f, color.g * 0.18f, color.b * 0.18f, 0.72f));
            ApplyPanelArt(header, "ui_section_header");
            header.GetComponent<Image>().raycastTarget = false;
            var text = CreateText(header.transform, $"{name} Text", Anchor.Stretch, new Vector2(8f, -1f), new Vector2(-12f, -2f), 11, TextAnchor.MiddleLeft);
            text.text = label;
            text.color = Color.Lerp(Color.white, color, 0.32f);
            text.raycastTarget = false;
        }

        private static GameObject CreateBar(Transform parent, string name, Anchor anchor, Vector2 position, Vector2 size, Color backColor, Color fillColor, out Image fill)
        {
            var back = CreatePanel(parent, $"{name} Back", anchor, position, size, backColor);
            var fillObject = CreatePanel(back.transform, $"{name} Fill", Anchor.Stretch, Vector2.zero, Vector2.zero, fillColor);
            fill = fillObject.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            return back;
        }

        private static Text CreateBarText(Transform parent, string name, string defaultText)
        {
            var text = CreateText(parent, name, Anchor.Stretch, new Vector2(4f, -1f), new Vector2(-8f, -2f), 11, TextAnchor.MiddleCenter);
            text.text = defaultText;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            return text;
        }

        private GameObject CreateConfiguredBar(Transform parent, string configId, string name, Anchor anchor, Vector2 position, Vector2 size, Color backColor, Color fillColor, out Image fill)
        {
            var layout = uiConfig?.Layout(configId);
            var back = CreateBar(
                parent,
                name,
                layout != null ? uiConfig.ToHudAnchor(layout.anchor) : anchor,
                layout != null ? layout.position : position,
                layout != null ? layout.size : size,
                layout != null ? layout.backgroundColor : backColor,
                fillColor,
                out fill);
            if (layout != null)
            {
                back.SetActive(layout.visible);
            }

            return back;
        }

        private static Text CreateText(Transform parent, string name, Anchor anchor, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            text.fontSize = fontSize;
            text.color = new Color(0.93f, 0.94f, 0.92f);
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            var rect = text.GetComponent<RectTransform>();
            SetAnchor(rect, anchor);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            if (anchor == Anchor.Stretch)
            {
                rect.offsetMin = new Vector2(position.x, -position.y + Mathf.Min(0f, size.y));
                rect.offsetMax = new Vector2(size.x, size.y);
            }

            return text;
        }

        private Text CreateConfiguredText(Transform parent, string configId, string name, Anchor anchor, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var layout = uiConfig?.Layout(configId);
            var textConfig = uiConfig?.Text(configId);
            var text = CreateText(
                parent,
                name,
                layout != null ? uiConfig.ToHudAnchor(layout.anchor) : anchor,
                layout != null ? layout.position : position,
                layout != null ? layout.size : size,
                textConfig != null ? Mathf.Max(1, textConfig.fontSize) : fontSize,
                textConfig != null ? textConfig.alignment : alignment);
            if (layout != null)
            {
                text.gameObject.SetActive(layout.visible);
            }

            if (textConfig != null)
            {
                text.color = textConfig.color;
                text.gameObject.SetActive(textConfig.visible);
                if (!string.IsNullOrEmpty(textConfig.overrideText))
                {
                    text.text = textConfig.overrideText;
                }
            }

            return text;
        }

        private void SetConfiguredRect(GameObject target, string configId, Anchor fallbackAnchor, Vector2 fallbackPosition, Vector2 fallbackSize)
        {
            var layout = uiConfig?.Layout(configId);
            var rect = target.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            if (layout != null)
            {
                ApplyAnchor(rect, uiConfig.ToHudAnchor(layout.anchor));
                rect.anchoredPosition = layout.position;
                rect.sizeDelta = layout.size;
                var image = target.GetComponent<Image>();
                if (image != null)
                {
                    image.color = layout.backgroundColor;
                }

                target.SetActive(layout.visible);
                return;
            }

            ApplyAnchor(rect, fallbackAnchor);
            rect.anchoredPosition = fallbackPosition;
            rect.sizeDelta = fallbackSize;
        }

        private UiButtonGroupConfig GetButtonGroup(string id, Vector2 buttonSize, Vector2 firstPosition, Vector2 spacing, int columns, int labelFontSize, int metaFontSize, int statusFontSize, Vector2 iconPosition, Vector2 iconSize)
        {
            return uiConfig?.ButtonGroup(id) ?? new UiButtonGroupConfig
            {
                id = id,
                buttonSize = buttonSize,
                firstPosition = firstPosition,
                spacing = spacing,
                columns = columns,
                labelFontSize = labelFontSize,
                metaFontSize = metaFontSize,
                statusFontSize = statusFontSize,
                iconPosition = iconPosition,
                iconSize = iconSize
            };
        }

        private static Vector2 ButtonPosition(UiButtonGroupConfig config, int index)
        {
            var columns = Mathf.Max(1, config.columns);
            var col = index % columns;
            var row = index / columns;
            return config.firstPosition + new Vector2(config.spacing.x * col, config.spacing.y * row);
        }

        private static Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, int fontSize)
        {
            var buttonObject = new GameObject(label);
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.15f, 0.13f, 0.18f, 0.96f);
            MusicManiacArtLibrary.ApplySpriteToImage(image, MusicManiacArtLibrary.Ui("ui_button"), image.color);
            image.preserveAspect = false;

            var button = buttonObject.AddComponent<Button>();
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = CreateText(buttonObject.transform, "Label", Anchor.Stretch, new Vector2(6f, -3f), new Vector2(-10f, -6f), fontSize, TextAnchor.MiddleCenter);
            text.text = label;
            text.color = Color.white;
            var hud = parent.GetComponentInParent<PrototypeHud>();
            if (hud != null)
            {
                hud.buttonImages[button] = image;
                hud.buttonLabels[button] = text;
            }

            return button;
        }

        private static Button CreateActionButton(Transform parent, string title, string meta, string status, Vector2 position, Vector2 size, Color accent, int labelFontSize = 10, int metaFontSize = 9, int statusFontSize = 8)
        {
            var buttonObject = new GameObject(title);
            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.15f, 0.13f, 0.18f, 0.96f);
            MusicManiacArtLibrary.ApplySpriteToImage(image, MusicManiacArtLibrary.Ui("ui_icon_button"), image.color);
            image.preserveAspect = false;

            var button = buttonObject.AddComponent<Button>();
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var accentBar = CreatePanel(buttonObject.transform, "Accent", Anchor.Stretch, new Vector2(0f, 0f), Vector2.zero, accent).GetComponent<Image>();
            var accentRect = accentBar.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(1f, 0f);
            accentRect.pivot = new Vector2(0.5f, 0f);
            accentRect.offsetMin = new Vector2(8f, 3f);
            accentRect.offsetMax = new Vector2(-8f, 7f);
            accentBar.raycastTarget = false;

            var titleText = CreateText(buttonObject.transform, "Label", Anchor.TopCenter, new Vector2(0f, -64f), new Vector2(size.x - 12f, 18f), labelFontSize, TextAnchor.MiddleCenter);
            titleText.text = title;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyle.Bold;
            titleText.raycastTarget = false;

            var metaText = CreateText(buttonObject.transform, "Meta", Anchor.TopCenter, new Vector2(0f, -82f), new Vector2(size.x - 10f, 15f), metaFontSize, TextAnchor.MiddleCenter);
            metaText.text = meta;
            metaText.color = new Color(0.76f, 0.82f, 0.9f, 0.96f);
            metaText.raycastTarget = false;

            var badge = CreatePanel(buttonObject.transform, "Status Badge", Anchor.TopRight, new Vector2(-5f, -5f), new Vector2(48f, 18f), new Color(0.08f, 0.42f, 0.26f, 0.88f)).GetComponent<Image>();
            ApplyPanelArt(badge.gameObject, "ui_status_ready");
            badge.raycastTarget = false;
            var statusText = CreateText(badge.transform, "Status", Anchor.Stretch, new Vector2(2f, -1f), new Vector2(-4f, -2f), statusFontSize, TextAnchor.MiddleCenter);
            statusText.text = status;
            statusText.color = Color.white;
            statusText.raycastTarget = false;

            var selection = CreatePanel(buttonObject.transform, "Selection Frame", Anchor.Stretch, Vector2.zero, Vector2.zero, Color.Lerp(Color.white, accent, 0.25f)).GetComponent<Image>();
            ApplyPanelArt(selection.gameObject, "ui_selected_frame");
            selection.raycastTarget = false;
            selection.gameObject.SetActive(false);

            var disabled = CreatePanel(buttonObject.transform, "Disabled Overlay", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.42f)).GetComponent<Image>();
            disabled.raycastTarget = false;
            disabled.gameObject.SetActive(false);

            var hud = parent.GetComponentInParent<PrototypeHud>();
            if (hud != null)
            {
                hud.buttonImages[button] = image;
                hud.buttonLabels[button] = titleText;
                hud.buttonMetaLabels[button] = metaText;
                hud.buttonStatusLabels[button] = statusText;
                hud.buttonAccentBars[button] = accentBar;
                hud.buttonStatusBadges[button] = badge;
                hud.buttonDisabledOverlays[button] = disabled;
                hud.buttonSelectionFrames[button] = selection;
            }

            return button;
        }

        private static BDCardView CreateBDCard(Transform parent, Vector2 position, Vector2 size)
        {
            var buttonObject = new GameObject("BD Card");
            buttonObject.transform.SetParent(parent, false);

            var background = buttonObject.AddComponent<Image>();
            background.color = new Color(0.055f, 0.05f, 0.075f, 0.96f);
            MusicManiacArtLibrary.ApplySpriteToImage(background, MusicManiacArtLibrary.Ui("ui_bd_card"), background.color);
            background.preserveAspect = false;

            var button = buttonObject.AddComponent<Button>();
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var accent = CreatePanel(buttonObject.transform, "Accent", Anchor.Stretch, Vector2.zero, Vector2.zero, NeonCyan).GetComponent<Image>();
            var accentRect = accent.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(1f, 0f);
            accentRect.pivot = new Vector2(0.5f, 0f);
            accentRect.offsetMin = new Vector2(5f, 2f);
            accentRect.offsetMax = new Vector2(-5f, 5f);
            accent.raycastTarget = false;

            var iconBack = CreatePanel(buttonObject.transform, "Icon Back", Anchor.TopLeft, new Vector2(5f, -5f), new Vector2(25f, 25f), new Color(0.12f, 0.14f, 0.18f, 0.96f)).GetComponent<Image>();
            ApplyPanelArt(iconBack.gameObject, "ui_icon_slot");
            iconBack.raycastTarget = false;

            var iconText = CreateText(iconBack.transform, "Icon Text", Anchor.Stretch, new Vector2(1f, -1f), new Vector2(-2f, -2f), 11, TextAnchor.MiddleCenter);
            iconText.fontStyle = FontStyle.Bold;
            iconText.raycastTarget = false;

            var levelText = CreateText(buttonObject.transform, "Level", Anchor.TopRight, new Vector2(-5f, -6f), new Vector2(31f, 12f), 9, TextAnchor.MiddleRight);
            levelText.fontStyle = FontStyle.Bold;
            levelText.raycastTarget = false;

            var nameText = CreateText(buttonObject.transform, "Name", Anchor.TopLeft, new Vector2(5f, -29f), new Vector2(size.x - 10f, 11f), 8, TextAnchor.MiddleCenter);
            nameText.fontStyle = FontStyle.Bold;
            nameText.raycastTarget = false;

            var metaText = CreateText(buttonObject.transform, "Meta", Anchor.TopLeft, new Vector2(5f, -38f), new Vector2(size.x - 10f, 9f), 7, TextAnchor.MiddleCenter);
            metaText.raycastTarget = false;

            var newBadge = CreatePanel(buttonObject.transform, "New Badge", Anchor.TopRight, new Vector2(-4f, -4f), new Vector2(10f, 10f), new Color(1f, 0.76f, 0.18f, 0.9f)).GetComponent<Image>();
            newBadge.raycastTarget = false;
            newBadge.gameObject.SetActive(false);

            var selection = CreatePanel(buttonObject.transform, "Selection", Anchor.Stretch, Vector2.zero, Vector2.zero, Color.white).GetComponent<Image>();
            ApplyPanelArt(selection.gameObject, "ui_selected_frame");
            selection.raycastTarget = false;
            selection.gameObject.SetActive(false);

            var hud = parent.GetComponentInParent<PrototypeHud>();
            if (hud != null)
            {
                hud.buttonImages[button] = background;
            }

            return new BDCardView
            {
                Button = button,
                Background = background,
                IconBack = iconBack,
                IconText = iconText,
                NameText = nameText,
                MetaText = metaText,
                LevelText = levelText,
                Accent = accent,
                NewBadge = newBadge,
                Selection = selection
            };
        }

        private static void ApplyPanelArt(GameObject panel, string artName)
        {
            if (panel == null)
            {
                return;
            }

            var image = panel.GetComponent<Image>();
            if (image != null)
            {
                MusicManiacArtLibrary.ApplySpriteToImage(image, MusicManiacArtLibrary.Ui(artName), image.color);
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }
        }

        private static void AddButtonIcon(Button button, Sprite sprite, Vector2 position, Vector2 size)
        {
            if (button == null || sprite == null)
            {
                return;
            }

            var slotObject = CreatePanel(button.transform, "Icon Slot", Anchor.TopCenter, new Vector2(position.x, -3f), new Vector2(size.x + 10f, size.y + 10f), Color.white);
            ApplyPanelArt(slotObject, "ui_icon_slot");
            slotObject.transform.SetSiblingIndex(1);
            var slotImage = slotObject.GetComponent<Image>();
            slotImage.raycastTarget = false;

            var iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(slotObject.transform, false);
            var icon = iconObject.AddComponent<Image>();
            MusicManiacArtLibrary.ApplySpriteToImage(icon, sprite, Color.white);
            var rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            icon.raycastTarget = false;

            var hud = button.GetComponentInParent<PrototypeHud>();
            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                var labelRect = label.GetComponent<RectTransform>();
                if (hud == null || !hud.buttonMetaLabels.ContainsKey(button))
                {
                    labelRect.offsetMin = new Vector2(38f, 3f);
                }
            }

            if (hud != null)
            {
                hud.buttonIcons[button] = icon;
                hud.buttonIconSlots[button] = slotImage;
            }
        }

        private Image GetButtonImage(Button button)
        {
            if (buttonImages.TryGetValue(button, out var image))
            {
                return image;
            }

            image = button.GetComponent<Image>();
            buttonImages[button] = image;
            return image;
        }

        private Text GetButtonLabel(Button button)
        {
            if (buttonLabels.TryGetValue(button, out var label))
            {
                return label;
            }

            label = button.GetComponentInChildren<Text>();
            buttonLabels[button] = label;
            return label;
        }

        public static void ApplyAnchor(RectTransform rect, Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.TopLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    break;
                case Anchor.TopRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    break;
                case Anchor.BottomLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                    rect.pivot = new Vector2(0f, 0f);
                    break;
                case Anchor.TopCenter:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    break;
                case Anchor.BottomStretch:
                    rect.anchorMin = new Vector2(0f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    break;
                case Anchor.TopStretch:
                    rect.anchorMin = new Vector2(0f, 1f);
                    rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    break;
                case Anchor.Stretch:
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    break;
            }
        }

        private static void SetAnchor(RectTransform rect, Anchor anchor)
        {
            ApplyAnchor(rect, anchor);
        }

        public enum Anchor
        {
            TopLeft,
            TopRight,
            BottomLeft,
            TopCenter,
            BottomStretch,
            TopStretch,
            Stretch
        }
    }
}
