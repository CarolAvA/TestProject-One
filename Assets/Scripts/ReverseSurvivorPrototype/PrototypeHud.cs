using System.Collections.Generic;
using System.Text;
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

        private GameDirector director;
        private Text heroText;
        private Text timerText;
        private Text threatText;
        private Text energyText;
        private Text healthValueText;
        private Text healthPercentText;
        private Text shieldPercentText;
        private Text objectiveText;
        private Text messageText;
        private Text aiStatusText;
        private Text rhythmText;
        private Text rhythmTracksText;
        private Text bdListText;
        private Text bdTooltipText;
        private Image healthFill;
        private Image shieldFill;
        private Image threatFill;
        private Image energyFill;
        private Image rhythmFill;
        private GameObject bdTooltipPanel;
        private GameObject mainMenuPanel;
        private GameObject resultPanel;
        private Text resultTitleText;
        private Text resultBodyText;
        private RuntimeUiConfig uiConfig;

        private readonly Dictionary<MonsterKind, Button> buttons = new Dictionary<MonsterKind, Button>();
        private readonly Dictionary<CreatorSkillId, Button> skillButtons = new Dictionary<CreatorSkillId, Button>();
        private readonly Dictionary<Button, Image> buttonImages = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Text> buttonLabels = new Dictionary<Button, Text>();
        private readonly Dictionary<Button, Text> buttonMetaLabels = new Dictionary<Button, Text>();
        private readonly Dictionary<Button, Text> buttonStatusLabels = new Dictionary<Button, Text>();
        private readonly Dictionary<Button, Image> buttonIcons = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonAccentBars = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonStatusBadges = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonDisabledOverlays = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonSelectionFrames = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> buttonIconSlots = new Dictionary<Button, Image>();
        private readonly List<Button> bdButtons = new List<Button>();
        private readonly List<AIBDDisplayData> bdDisplayCache = new List<AIBDDisplayData>();
        private int hoveredBdIndex = -1;
        private int pinnedBdIndex = -1;

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
            energyFill.fillAmount = Mathf.Clamp01(director.Energy / Mathf.Max(1f, director.MaxEnergy));

            heroText.text = $"Music Maniac  Lv.{hero.Level}\n{hero.BuildData.BuildName}";
            var shield01 = Mathf.Clamp01(hero.Shield / Mathf.Max(1f, hero.MaxHealth * 0.35f));
            healthValueText.text = $"HP {hero.Health:0}/{hero.MaxHealth:0}  Shield {hero.Shield:0}";
            healthPercentText.text = $"{hero.Health01:P0}";
            shieldPercentText.text = shield01 > 0.01f ? $"{shield01:P0}" : "Shield 0%";
            timerText.text = $"{Mathf.FloorToInt(director.RemainingTime / 60f):00}:{Mathf.FloorToInt(director.RemainingTime % 60f):00}";
            threatText.text = $"Threat {director.Threat:P0}";
            energyText.text = $"Soul {director.Energy:0}/{director.MaxEnergy:0}";

            var hpTag = hero.Health01 < 0.35f ? "Low HP. Push now." : "Defeat the Music Maniac.";
            objectiveText.text = $"目标\n{hpTag}\n\n倒计时\n剩余 {timerText.text}\n\n资源\n人口 {director.Monsters.Count}/15\n\n镜头\nWASD移动  Shift加速\nSpace回到AI角色";
            messageText.text = string.IsNullOrEmpty(director.Message) ? "Use the right deck to summon. Cast on the beat window." : director.Message;

            if (rhythm != null)
            {
                rhythmText.text = $"AI Attack: {rhythm.AttackName}    {rhythm.GetNextBeatText()}    {rhythm.AttackTime:0.0}s / {rhythm.AttackDuration:0.0}s";
                rhythmTracksText.text = rhythm.BuildTimelineText();
                rhythmFill.fillAmount = rhythm.Progress01;
                rhythmFill.color = GetRhythmProgressColor(rhythm);
                aiStatusText.text =
                    $"Build: {hero.BuildData.BuildName}\n" +
                    $"Attack: {rhythm.AttackName}\n" +
                    $"Danger: {rhythm.DangerLevel}  Tags: {rhythm.BuildTags}\n" +
                    $"Debuff: Heal {hero.AntiHealTime:0.0}s / Break {hero.ShieldBreakTime:0.0}s / Slow {hero.SkillSlowTime:0.0}s";
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
            BuildResultPanel(root);
        }

        private void BuildTopStatus(Transform root)
        {
            var top = CreateConfiguredPanel(root, "top_status_bar", "Top Status Bar", Anchor.TopStretch, new Vector2(0f, -8f), new Vector2(-28f, 74f), new Color(0.02f, 0.018f, 0.028f, 0.92f));
            ApplyPanelArt(top, "ui_panel");

            var heroPanel = CreatePanel(top.transform, "AI Character Info", Anchor.TopLeft, new Vector2(10f, -8f), new Vector2(690f, 58f), new Color(0.035f, 0.02f, 0.045f, 0.86f));
            ApplyPanelArt(heroPanel, "ui_bd_card");
            CreateHeader(heroPanel.transform, "AI Character Header", "AI角色信息", NeonCyan);

            var creatorPanel = CreatePanel(top.transform, "Creator Info", Anchor.TopRight, new Vector2(-10f, -8f), new Vector2(470f, 58f), new Color(0.04f, 0.027f, 0.018f, 0.86f));
            ApplyPanelArt(creatorPanel, "ui_bd_card");
            CreateHeader(creatorPanel.transform, "Creator Header", "造物主信息", NeonOrange);

            var centerPanel = CreatePanel(top.transform, "Battle State Info", Anchor.TopCenter, new Vector2(0f, -8f), new Vector2(430f, 58f), new Color(0.025f, 0.026f, 0.038f, 0.84f));
            ApplyPanelArt(centerPanel, "ui_bd_card");
            CreateHeader(centerPanel.transform, "Battle Header", "战斗状态", NeonPurple);

            var portrait = CreateConfiguredPanel(heroPanel.transform, "ai_portrait", "AI Portrait", Anchor.TopLeft, new Vector2(12f, -10f), new Vector2(42f, 42f), new Color(0.08f, 0.04f, 0.12f, 0.95f));
            MusicManiacArtLibrary.ApplySpriteToImage(portrait.GetComponent<Image>(), MusicManiacArtLibrary.HeroSprite, Color.white);

            heroText = CreateConfiguredText(heroPanel.transform, "ai_summary", "AI Summary", Anchor.TopLeft, new Vector2(62f, -9f), new Vector2(236f, 42f), 14, TextAnchor.UpperLeft);
            var healthBack = CreateConfiguredBar(heroPanel.transform, "ai_hp_bar", "AI HP", Anchor.TopLeft, new Vector2(314f, -15f), new Vector2(342f, 16f), new Color(0.12f, 0.07f, 0.12f, 0.96f), NeonPink, out healthFill);
            healthPercentText = CreateBarText(healthBack.transform, "AI HP Percent", "100%");
            var shieldBack = CreateBar(healthBack.transform, "AI Shield", Anchor.BottomStretch, new Vector2(0f, -8f), new Vector2(0f, 6f), new Color(0.02f, 0.08f, 0.11f, 0.88f), NeonCyan, out shieldFill);
            shieldPercentText = CreateBarText(shieldBack.transform, "AI Shield Percent", "Shield 0%");
            healthValueText = CreateText(heroPanel.transform, "AI HP Value", Anchor.TopLeft, new Vector2(314f, -38f), new Vector2(342f, 18f), 12, TextAnchor.MiddleCenter);

            timerText = CreateConfiguredText(centerPanel.transform, "timer_text", "Timer", Anchor.TopCenter, new Vector2(-128f, -20f), new Vector2(116f, 32f), 24, TextAnchor.MiddleCenter);
            var threatBack = CreateConfiguredBar(centerPanel.transform, "threat_bar", "Threat", Anchor.TopLeft, new Vector2(184f, -22f), new Vector2(190f, 14f), new Color(0.1f, 0.07f, 0.04f, 0.9f), NeonOrange, out threatFill);
            threatText = CreateText(threatBack.transform, "Threat Label", Anchor.TopCenter, new Vector2(0f, -19f), new Vector2(190f, 20f), 12, TextAnchor.MiddleCenter);

            var energyBack = CreateConfiguredBar(creatorPanel.transform, "energy_bar", "Soul Energy", Anchor.TopLeft, new Vector2(28f, -21f), new Vector2(250f, 16f), new Color(0.09f, 0.05f, 0.12f, 0.95f), NeonPurple, out energyFill);
            energyText = CreateText(energyBack.transform, "Soul Label", Anchor.TopCenter, new Vector2(0f, -20f), new Vector2(250f, 20f), 12, TextAnchor.MiddleCenter);
            CreateText(creatorPanel.transform, "Creator Hint", Anchor.TopRight, new Vector2(-18f, -20f), new Vector2(140f, 32f), 12, TextAnchor.MiddleRight).text = "召唤 / 技能\n资源与冷却";
        }

        private void BuildLeftInfo(Transform root)
        {
            var left = CreateConfiguredPanel(root, "left_info_panel", "Left Info Panel", Anchor.TopLeft, new Vector2(10f, -92f), new Vector2(250f, 272f), PanelColor);
            ApplyPanelArt(left, "ui_panel");
            objectiveText = CreateConfiguredText(left.transform, "objective_text", "Objective", Anchor.TopLeft, new Vector2(14f, -12f), new Vector2(222f, 160f), 15, TextAnchor.UpperLeft);

            var msg = CreateConfiguredPanel(left.transform, "message_tape", "Message Tape", Anchor.BottomStretch, new Vector2(0f, 12f), new Vector2(-24f, 78f), new Color(0.04f, 0.02f, 0.04f, 0.92f));
            ApplyPanelArt(msg, "ui_bubble");
            messageText = CreateConfiguredText(msg.transform, "message_text", "Message", Anchor.Stretch, new Vector2(10f, -6f), new Vector2(-20f, -12f), 13, TextAnchor.UpperLeft);
            messageText.color = new Color(0.1f, 0.08f, 0.11f, 1f);
        }

        private void BuildRightActionDeck(Transform root)
        {
            var right = CreateConfiguredPanel(root, "right_action_deck", "Right Action Deck", Anchor.TopRight, new Vector2(-10f, -92f), new Vector2(448f, 748f), new Color(0.023f, 0.017f, 0.033f, 0.93f));
            ApplyPanelArt(right, "ui_danger_frame");

            var aiPanel = CreateSection(right.transform, "AI State Section", "AI状态", new Vector2(0f, -10f), new Vector2(-20f, 86f), NeonCyan);
            aiStatusText = CreateText(aiPanel.transform, "AI Status", Anchor.TopLeft, new Vector2(12f, -30f), new Vector2(400f, 50f), 11, TextAnchor.UpperLeft);

            var summonPanel = CreateSection(right.transform, "Summon Section", "召唤怪物", new Vector2(0f, -104f), new Vector2(-20f, 342f), NeonPink);
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

            var summonGroup = GetButtonGroup("summon_icon_buttons", new Vector2(126f, 96f), new Vector2(78f, 96f), new Vector2(136f, -104f), 3, 11, 9, 8, new Vector2(0f, -3f), new Vector2(50f, 50f));
            for (var i = 0; i < order.Length; i++)
            {
                var kind = order[i];
                var config = director.GetMonsterConfig(kind);
                var button = CreateActionButton(summonPanel.transform, config.DisplayName, $"魂 {config.Cost:0}  {config.Tag}", "可用", ButtonPosition(summonGroup, i), summonGroup.buttonSize, NeonPink, summonGroup.labelFontSize, summonGroup.metaFontSize, summonGroup.statusFontSize);
                AddButtonIcon(button, MusicManiacArtLibrary.MonsterIcon(kind), summonGroup.iconPosition, summonGroup.iconSize);
                button.onClick.AddListener(() =>
                {
                    MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiClick, Vector2.zero, 0.7f);
                    director.SelectMonsterFromHud(kind);
                });
                buttons[kind] = button;
            }

            var skillPanel = CreateSection(right.transform, "Skill Section", "造物主技能", new Vector2(0f, -454f), new Vector2(-20f, 238f), NeonOrange);
            var skillOrder = new[]
            {
                CreatorSkillId.LightningStrike,
                CreatorSkillId.FrostField,
                CreatorSkillId.AntiHealCurse,
                CreatorSkillId.ShieldBrand,
                CreatorSkillId.BoneWall,
                CreatorSkillId.DemonHand
            };

            var skillGroup = GetButtonGroup("skill_icon_buttons", new Vector2(126f, 96f), new Vector2(78f, 50f), new Vector2(136f, -104f), 3, 11, 9, 8, new Vector2(0f, -3f), new Vector2(50f, 50f));
            for (var i = 0; i < skillOrder.Length; i++)
            {
                var skillId = skillOrder[i];
                var config = director.GetSkillConfig(skillId);
                var button = CreateActionButton(skillPanel.transform, config.DisplayName, $"魂 {config.Cost:0}  {config.Tag}", "可用", ButtonPosition(skillGroup, i), skillGroup.buttonSize, NeonOrange, skillGroup.labelFontSize, skillGroup.metaFontSize, skillGroup.statusFontSize);
                AddButtonIcon(button, MusicManiacArtLibrary.SkillIcon(skillId), skillGroup.iconPosition, skillGroup.iconSize);
                button.onClick.AddListener(() =>
                {
                    MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiClick, Vector2.zero, 0.7f);
                    director.SelectSkillFromHud(skillId);
                });
                skillButtons[skillId] = button;
            }

            var buildPanel = CreateSection(right.transform, "Build Pressure Section", "AI流派压力", new Vector2(0f, -692f), new Vector2(-20f, 50f), NeonPurple);
            CreateBuildButton(buildPanel.transform, "火力", 0, new Vector2(80f, -14f));
            CreateBuildButton(buildPanel.transform, "吸取", 1, new Vector2(214f, -14f));
            CreateBuildButton(buildPanel.transform, "护盾", 2, new Vector2(348f, -14f));
        }

        private void BuildBottomRhythmDeck(Transform root)
        {
            var bottom = CreateConfiguredPanel(root, "bottom_rhythm_deck", "Bottom Rhythm Deck", Anchor.BottomStretch, new Vector2(0f, 8f), new Vector2(-440f, 184f), new Color(0.018f, 0.017f, 0.024f, 0.94f));
            ApplyPanelArt(bottom, "ui_rhythm_bar");

            var rhythmPanel = CreateSection(bottom.transform, "Rhythm Timeline Section", "AI SFX Timeline", new Vector2(12f, -12f), new Vector2(1068f, 154f), NeonPink, Anchor.TopLeft);
            var attackBlock = CreatePanel(rhythmPanel.transform, "AI Attack Block", Anchor.TopLeft, new Vector2(14f, -30f), new Vector2(504f, 30f), new Color(0.12f, 0.035f, 0.09f, 0.82f));
            ApplyPanelArt(attackBlock, "ui_button");
            rhythmText = CreateConfiguredText(attackBlock.transform, "rhythm_header", "Rhythm Header", Anchor.Stretch, new Vector2(10f, -2f), new Vector2(-18f, -4f), 12, TextAnchor.MiddleLeft);

            var laneBlock = CreatePanel(rhythmPanel.transform, "AI Lane Block", Anchor.TopLeft, new Vector2(526f, -30f), new Vector2(526f, 30f), new Color(0.025f, 0.09f, 0.12f, 0.78f));
            ApplyPanelArt(laneBlock, "ui_button");
            CreateText(laneBlock.transform, "Lane Label", Anchor.Stretch, new Vector2(10f, -2f), new Vector2(-18f, -4f), 12, TextAnchor.MiddleLeft).text = "Beat Lane / SFX Sync";

            rhythmTracksText = CreateConfiguredText(rhythmPanel.transform, "rhythm_tracks", "Rhythm Tracks", Anchor.TopLeft, new Vector2(16f, -64f), new Vector2(1036f, 44f), 12, TextAnchor.UpperLeft);

            CreateTimelineZone(rhythmPanel.transform, "Windup Zone", "Windup", new Vector2(16f, -132f), new Vector2(220f, 16f), NeonCyan);
            CreateTimelineZone(rhythmPanel.transform, "Barrage Zone", "Barrage", new Vector2(240f, -132f), new Vector2(700f, 16f), NeonPink);
            CreateTimelineZone(rhythmPanel.transform, "Recovery Zone", "Recovery", new Vector2(944f, -132f), new Vector2(108f, 16f), new Color(0.35f, 0.95f, 0.52f, 0.95f));

            var progressBack = CreateBar(rhythmPanel.transform, "Rhythm Progress", Anchor.TopLeft, new Vector2(16f, -112f), new Vector2(1036f, 18f), new Color(0.08f, 0.06f, 0.1f, 0.78f), NeonPink, out rhythmFill);
            ApplyPanelArt(progressBack, "ui_button");

            var bdPanel = CreateSection(bottom.transform, "BD Strip Section", "Current BD", new Vector2(-12f, -12f), new Vector2(404f, 154f), NeonCyan, Anchor.TopRight);
            bdListText = CreateConfiguredText(bdPanel.transform, "bd_title", "BD Title", Anchor.TopLeft, new Vector2(12f, -30f), new Vector2(170f, 24f), 12, TextAnchor.MiddleLeft);
            var bdGroup = GetButtonGroup("bd_buttons", new Vector2(58f, 40f), new Vector2(48f, 24f), new Vector2(70f, -48f), 5, 10, 9, 8, Vector2.zero, Vector2.zero);
            for (var i = 0; i < 10; i++)
            {
                var index = i;
                var button = CreateButton(bdPanel.transform, "-", ButtonPosition(bdGroup, i), bdGroup.buttonSize, bdGroup.labelFontSize);
                button.onClick.AddListener(() => pinnedBdIndex = pinnedBdIndex == index ? -1 : index);
                var hover = button.gameObject.AddComponent<BDHoverTarget>();
                hover.Initialize(this, index);
                bdButtons.Add(button);
            }

            bdTooltipPanel = CreateConfiguredPanel(root, "bd_tooltip", "AI BD Tooltip", Anchor.BottomLeft, new Vector2(276f, 202f), new Vector2(384f, 168f), new Color(0.025f, 0.03f, 0.035f, 0.96f));
            ApplyPanelArt(bdTooltipPanel, "ui_panel");
            bdTooltipText = CreateConfiguredText(bdTooltipPanel.transform, "bd_tooltip_text", "BD Tooltip Text", Anchor.Stretch, new Vector2(10f, -8f), new Vector2(-20f, -16f), 12, TextAnchor.UpperLeft);
            bdTooltipPanel.SetActive(false);
        }

        private void BuildMainMenu(Transform root)
        {
            mainMenuPanel = CreateConfiguredPanel(root, "main_menu", "Main Menu", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.01f, 0.012f, 0.016f, 0.96f));
            var title = CreateConfiguredText(mainMenuPanel.transform, "main_title", "Main Menu Title", Anchor.TopCenter, new Vector2(0f, -230f), new Vector2(620f, 70f), 34, TextAnchor.MiddleCenter);
            title.text = "击败音乐疯子";
            title.color = new Color(1f, 0.86f, 0.42f, 1f);
            uiConfig?.ApplyText(title, "main_title");

            var subtitle = CreateConfiguredText(mainMenuPanel.transform, "main_subtitle", "Main Menu Subtitle", Anchor.TopCenter, new Vector2(0f, -310f), new Vector2(760f, 70f), 17, TextAnchor.MiddleCenter);
            subtitle.text = "作为造物主召唤怪物、释放技能，在1分钟内击败AI角色。";
            subtitle.color = new Color(0.78f, 0.88f, 0.92f, 1f);
            uiConfig?.ApplyText(subtitle, "main_subtitle");

            var startButton = CreateButton(mainMenuPanel.transform, "开始游戏", Vector2.zero, new Vector2(220f, 54f), 18);
            var startRect = startButton.GetComponent<RectTransform>();
            SetConfiguredRect(startButton.gameObject, "main_start_button", Anchor.TopCenter, new Vector2(0f, -410f), new Vector2(220f, 54f));
            startButton.onClick.AddListener(() =>
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiConfirm, Vector2.zero, 0.85f);
                director.StartGame();
                mainMenuPanel.SetActive(false);
            });

            var tip = CreateConfiguredText(mainMenuPanel.transform, "main_tip", "Main Menu Tip", Anchor.TopCenter, new Vector2(0f, -486f), new Vector2(760f, 48f), 13, TextAnchor.MiddleCenter);
            tip.text = "右侧选择怪物或技能，点击战场进行召唤或释放。WASD移动视野，Shift加速，Space回到AI角色。配置请在Unity顶部菜单 Tools/Defeat Music Maniac/配置编辑器 中修改。";
            tip.color = new Color(0.58f, 0.66f, 0.72f, 1f);
            uiConfig?.ApplyText(tip, "main_tip");
        }

        private void BuildResultPanel(Transform root)
        {
            resultPanel = CreateConfiguredPanel(root, "result_panel", "Result Panel", Anchor.Stretch, Vector2.zero, Vector2.zero, new Color(0.008f, 0.009f, 0.012f, 0.94f));
            resultTitleText = CreateConfiguredText(resultPanel.transform, "result_title", "Result Title", Anchor.TopCenter, new Vector2(0f, -260f), new Vector2(680f, 78f), 34, TextAnchor.MiddleCenter);
            resultBodyText = CreateConfiguredText(resultPanel.transform, "result_body", "Result Body", Anchor.TopCenter, new Vector2(0f, -340f), new Vector2(760f, 92f), 17, TextAnchor.MiddleCenter);

            var restartButton = CreateButton(resultPanel.transform, "重新开始", Vector2.zero, new Vector2(220f, 54f), 18);
            var restartRect = restartButton.GetComponent<RectTransform>();
            SetConfiguredRect(restartButton.gameObject, "result_restart_button", Anchor.TopCenter, new Vector2(0f, -456f), new Vector2(220f, 54f));
            restartButton.onClick.AddListener(() =>
            {
                MusicManiacAudioSystem.Instance.Play(MusicManiacAudioEvent.UiConfirm, Vector2.zero, 0.85f);
                director.RestartGame();
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
                ? $"音乐疯子已被击败。\n用时 {GameDirector.BattleDuration - director.RemainingTime:0.0} 秒。"
                : "1分钟倒计时结束，AI角色仍然存活。\n调整召唤节奏和技能释放，再来一次。";
            resultBodyText.color = new Color(0.86f, 0.9f, 0.92f, 1f);
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

                SetButtonMeta(pair.Value, $"魂 {config.Cost:0}  {config.Tag}", affordable);
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

                var state = cooldown > 0f ? $"CD{cooldown:0}s" : config.Cost > director.Energy ? "不足" : "可用";
                SetButtonMeta(pair.Value, $"魂 {config.Cost:0}  {config.Tag}", usable);
                SetButtonStatus(pair.Value, state, usable, selected);
                SetButtonVisual(pair.Value, usable, selected, new Color(0.72f, 0.18f, 0.32f), NeonOrange);
            }
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
                    ApplyPanelArt(badge.gameObject, status.StartsWith("CD") ? "ui_status_cd" : "ui_status_ready");
                }
                else
                {
                    badge.color = new Color(0.28f, 0.08f, 0.1f, 0.88f);
                    ApplyPanelArt(badge.gameObject, status.StartsWith("CD") ? "ui_status_cd" : "ui_status_low");
                }
            }
        }

        private void RefreshBDList(AIHeroBuildData buildData)
        {
            bdDisplayCache.Clear();
            bdDisplayCache.AddRange(buildData.GetDisplayCards());
            bdListText.text = $"Current BD  {bdDisplayCache.Count}/20";

            for (var i = 0; i < bdButtons.Count; i++)
            {
                var button = bdButtons[i];
                var label = button.GetComponentInChildren<Text>();
                var image = button.GetComponent<Image>();
                if (i >= bdDisplayCache.Count)
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                button.gameObject.SetActive(true);
                var data = bdDisplayCache[i];
                if (label != null)
                {
                    label.text = $"{data.IconText}\n{data.Level}";
                    label.fontSize = 10;
                }

                if (image != null)
                {
                    var color = data.Color;
                    if (data.IsNew)
                    {
                        color = Color.Lerp(color, Color.white, Mathf.PingPong(Time.time * 5f, 0.55f));
                    }

                    image.color = color;
                }
            }

            var tooltipIndex = pinnedBdIndex >= 0 ? pinnedBdIndex : hoveredBdIndex;
            var shouldShow = tooltipIndex >= 0 && tooltipIndex < bdDisplayCache.Count;
            bdTooltipPanel.SetActive(shouldShow);
            if (shouldShow)
            {
                bdTooltipText.text = BuildBDTooltip(bdDisplayCache[tooltipIndex]);
            }
        }

        private static string BuildBDTooltip(AIBDDisplayData data)
        {
            return $"{data.Name}   Lv.{data.Level}\n" +
                   $"{data.Category} / {data.Element}   {data.Rarity}\n\n" +
                   $"{data.Effect}\n\n" +
                   $"Audio: {data.Audio}\n" +
                   $"VFX: {data.Vfx}\n" +
                   $"Counter: {data.Counter}";
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
            ApplyPanelArt(titleBack, "ui_button");

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

        private static void CreateHeader(Transform parent, string name, string label, Color color)
        {
            var header = CreatePanel(parent, name, Anchor.TopStretch, new Vector2(0f, -2f), new Vector2(-10f, 18f), new Color(color.r * 0.18f, color.g * 0.18f, color.b * 0.18f, 0.72f));
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

            var titleText = CreateText(buttonObject.transform, "Label", Anchor.TopCenter, new Vector2(0f, -64f), new Vector2(size.x - 12f, 14f), labelFontSize, TextAnchor.MiddleCenter);
            titleText.text = title;
            titleText.color = Color.white;
            titleText.fontStyle = FontStyle.Bold;
            titleText.raycastTarget = false;

            var metaText = CreateText(buttonObject.transform, "Meta", Anchor.TopCenter, new Vector2(0f, -80f), new Vector2(size.x - 10f, 13f), metaFontSize, TextAnchor.MiddleCenter);
            metaText.text = meta;
            metaText.color = new Color(0.76f, 0.82f, 0.9f, 0.96f);
            metaText.raycastTarget = false;

            var badge = CreatePanel(buttonObject.transform, "Status Badge", Anchor.TopRight, new Vector2(-5f, -5f), new Vector2(42f, 16f), new Color(0.08f, 0.42f, 0.26f, 0.88f)).GetComponent<Image>();
            ApplyPanelArt(badge.gameObject, "ui_status_ready");
            badge.raycastTarget = false;
            var statusText = CreateText(badge.transform, "Status", Anchor.Stretch, new Vector2(2f, -1f), new Vector2(-4f, -2f), statusFontSize, TextAnchor.MiddleCenter);
            statusText.text = status;
            statusText.color = Color.white;
            statusText.raycastTarget = false;

            var selection = CreatePanel(buttonObject.transform, "Selection Frame", Anchor.Stretch, Vector2.zero, Vector2.zero, Color.Lerp(Color.white, accent, 0.25f)).GetComponent<Image>();
            ApplyPanelArt(selection.gameObject, "ui_danger_frame");
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
