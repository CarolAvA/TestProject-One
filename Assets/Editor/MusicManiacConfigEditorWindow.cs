using System.Collections.Generic;
using System.IO;
using ReverseSurvivorPrototype;
using UnityEditor;
using UnityEngine;

namespace ReverseSurvivorPrototype.EditorTools
{
    public sealed class MusicManiacConfigEditorWindow : EditorWindow
    {
        private const string DatabasePath = "Assets/Resources/ReverseSurvivorConfig/MusicManiacConfigDatabase.asset";
        private const string MenuRoot = "Tools/Defeat Music Maniac/配置编辑器";

        private static readonly string[] Tabs =
        {
            "序列帧",
            "弹体图片",
            "伤害表现",
            "AOE特效",
            "怪物数值",
            "技能数值",
            "UI配置"
        };

        private MusicManiacConfigDatabase database;
        private SerializedObject serializedDatabase;
        private Vector2 scroll;
        private int tabIndex;

        [MenuItem(MenuRoot)]
        public static void Open()
        {
            var window = GetWindow<MusicManiacConfigEditorWindow>("音乐疯子配置");
            window.minSize = new Vector2(760f, 560f);
            window.Show();
        }

        [MenuItem(MenuRoot + "/序列帧配置")]
        public static void OpenAnimationTab()
        {
            OpenTab(0);
        }

        [MenuItem(MenuRoot + "/弹体图片配置")]
        public static void OpenProjectileTab()
        {
            OpenTab(1);
        }

        [MenuItem(MenuRoot + "/伤害表现配置")]
        public static void OpenDamageTab()
        {
            OpenTab(2);
        }

        [MenuItem(MenuRoot + "/AOE特效配置")]
        public static void OpenAoeTab()
        {
            OpenTab(3);
        }

        [MenuItem(MenuRoot + "/怪物数值配置")]
        public static void OpenMonsterTab()
        {
            OpenTab(4);
        }

        [MenuItem(MenuRoot + "/技能数值配置")]
        public static void OpenSkillTab()
        {
            OpenTab(5);
        }

        [MenuItem(MenuRoot + "/UI配置")]
        public static void OpenUiTab()
        {
            OpenTab(6);
        }

        private static void OpenTab(int index)
        {
            Open();
            var window = GetWindow<MusicManiacConfigEditorWindow>();
            window.tabIndex = index;
        }

        private void OnEnable()
        {
            LoadOrCreateDatabase();
        }

        private void OnGUI()
        {
            LoadOrCreateDatabase();
            if (database == null || serializedDatabase == null)
            {
                EditorGUILayout.HelpBox("配置数据库加载失败。", MessageType.Error);
                return;
            }

            serializedDatabase.Update();
            DrawToolbar();
            EditorGUILayout.Space(6f);
            tabIndex = GUILayout.Toolbar(tabIndex, Tabs, GUILayout.Height(30f));
            EditorGUILayout.Space(8f);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            switch (tabIndex)
            {
                case 0:
                    DrawAnimationTab();
                    break;
                case 1:
                    DrawProjectileTab();
                    break;
                case 2:
                    DrawDamageTab();
                    break;
                case 3:
                    DrawAoeTab();
                    break;
                case 4:
                    DrawMonsterTab();
                    break;
                case 5:
                    DrawSkillTab();
                    break;
                case 6:
                    DrawUiTab();
                    break;
            }

            EditorGUILayout.EndScrollView();

            if (serializedDatabase.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(database);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("击败音乐疯子 - 编辑器配置工具", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("选中配置资产", EditorStyles.toolbarButton, GUILayout.Width(94f)))
                {
                    Selection.activeObject = database;
                    EditorGUIUtility.PingObject(database);
                }

                if (GUILayout.Button("补齐默认配置", EditorStyles.toolbarButton, GUILayout.Width(94f)))
                {
                    FillDefaults();
                }

                if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(54f)))
                {
                    SaveDatabase();
                }
            }
        }

        private void DrawAnimationTab()
        {
            EditorGUILayout.HelpBox("单位动画按“单位 -> 动作 -> 每帧”配置。每个动作可单独设置FPS和循环；每一帧都能单独配置图片、世界尺寸、中心点、偏移和帧时长。", MessageType.Info);
            var list = serializedDatabase.FindProperty("characterAnimations");
            DrawList(list, "角色/怪物序列帧配置", DrawAnimationItem, AddAnimationItem);
        }

        private void DrawProjectileTab()
        {
            EditorGUILayout.HelpBox("配置所有子弹、弹体、拖尾和命中特效图片。这里是编辑器配置，不会在游戏运行时打开。", MessageType.Info);
            var list = serializedDatabase.FindProperty("projectileVisuals");
            DrawList(list, "弹体图片配置", DrawProjectileItem, AddProjectileItem);
        }

        private void DrawDamageTab()
        {
            EditorGUILayout.HelpBox("配置伤害跳字、颜色、字号、上飘、合并规则、命中特效和音效。", MessageType.Info);
            var list = serializedDatabase.FindProperty("damageVisuals");
            DrawList(list, "伤害表现配置", DrawDamageItem, AddDamageItem);
        }

        private void DrawAoeTab()
        {
            EditorGUILayout.HelpBox("配置AOE、预警圆、出生、死亡、区域贴图等特效。怪物死亡默认禁用镜头畸变。", MessageType.Info);
            var list = serializedDatabase.FindProperty("aoeVfx");
            DrawList(list, "AOE特效配置", DrawAoeItem, AddAoeItem);
        }

        private void DrawMonsterTab()
        {
            EditorGUILayout.HelpBox("配置怪物数值。当前运行时代码仍有内置默认值；这里先作为正式数据入口保存到项目资产。", MessageType.Warning);
            var list = serializedDatabase.FindProperty("monsterValues");
            DrawList(list, "怪物数值配置", DrawMonsterItem, AddMonsterItem);
        }

        private void DrawSkillTab()
        {
            EditorGUILayout.HelpBox("配置造物主技能数值、预警范围、预警颜色和效果颜色。预警圆半径应和真实伤害半径一致。", MessageType.Warning);
            var list = serializedDatabase.FindProperty("skillValues");
            DrawList(list, "技能数值配置", DrawSkillItem, AddSkillItem);
        }

        private void DrawUiTab()
        {
            EditorGUILayout.HelpBox("配置游戏内HUD、主菜单、结算界面的大小、位置、颜色、文字、字体和按钮组布局。修改后保存，重新运行游戏即可应用。", MessageType.Info);
            DrawList(serializedDatabase.FindProperty("uiLayouts"), "UI框体/控件布局", DrawUiLayoutItem, AddUiLayoutItem);
            EditorGUILayout.Space(8f);
            DrawList(serializedDatabase.FindProperty("uiTexts"), "UI文字配置", DrawUiTextItem, AddUiTextItem);
            EditorGUILayout.Space(8f);
            DrawList(serializedDatabase.FindProperty("uiButtonGroups"), "UI按钮组配置", DrawUiButtonGroupItem, AddUiButtonGroupItem);
        }

        private static void DrawList(SerializedProperty list, string title, System.Action<SerializedProperty> drawItem, System.Action<SerializedProperty> addItem)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("新增", GUILayout.Width(70f)))
                    {
                        list.arraySize++;
                        addItem(list.GetArrayElementAtIndex(list.arraySize - 1));
                    }
                }

                EditorGUILayout.Space(4f);
                for (var i = 0; i < list.arraySize; i++)
                {
                    var item = list.GetArrayElementAtIndex(i);
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            item.isExpanded = EditorGUILayout.Foldout(item.isExpanded, $"#{i + 1}  {DisplayName(item)}", true);
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("删除", GUILayout.Width(60f)))
                            {
                                list.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }

                        if (item.isExpanded)
                        {
                            EditorGUI.indentLevel++;
                            drawItem(item);
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }
        }

        private static void DrawAnimationItem(SerializedProperty item)
        {
            Field(item, "displayName", "显示名称");
            Field(item, "assetId", "资源ID/路径");
            Field(item, "isHero", "是否AI角色");
            Field(item, "monsterKind", "怪物类型");
            Field(item, "worldHeight", "世界高度");
            DrawActionAnimationList(item.FindPropertyRelative("actions"));
        }

        private static void DrawActionAnimationList(SerializedProperty actions)
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("动作配置", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("补齐标准动作", GUILayout.Width(96f)))
                    {
                        EnsureStandardActions(actions);
                    }

                    if (GUILayout.Button("新增动作", GUILayout.Width(78f)))
                    {
                        actions.arraySize++;
                        SetupAction(actions.GetArrayElementAtIndex(actions.arraySize - 1), UnitAnimationAction.Idle, "新动作", null, true);
                    }
                }

                for (var i = 0; i < actions.arraySize; i++)
                {
                    var action = actions.GetArrayElementAtIndex(i);
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            action.isExpanded = EditorGUILayout.Foldout(action.isExpanded, $"动作 {i + 1}：{ActionTitle(action)}", true);
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("删除动作", GUILayout.Width(78f)))
                            {
                                actions.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }

                        if (action.isExpanded)
                        {
                            EditorGUI.indentLevel++;
                            Field(action, "action", "动作类型");
                            Field(action, "displayName", "动作名称");
                            Field(action, "framesPerSecond", "动作FPS");
                            Field(action, "loop", "是否循环");
                            DrawFrameList(action.FindPropertyRelative("frames"));
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }
        }

        private static void DrawFrameList(SerializedProperty frames)
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("逐帧图片配置", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("新增帧", GUILayout.Width(70f)))
                    {
                        frames.arraySize++;
                        SetupFrame(frames.GetArrayElementAtIndex(frames.arraySize - 1), $"第{frames.arraySize}帧", null);
                    }
                }

                if (frames.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("这个动作还没有帧。点击“新增帧”后逐帧配置图片、尺寸和中心点。", MessageType.Warning);
                }

                for (var i = 0; i < frames.arraySize; i++)
                {
                    var frame = frames.GetArrayElementAtIndex(i);
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            frame.isExpanded = EditorGUILayout.Foldout(frame.isExpanded, $"帧 {i + 1}：{FrameTitle(frame)}", true);
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("上移", GUILayout.Width(48f)) && i > 0)
                            {
                                frames.MoveArrayElement(i, i - 1);
                            }

                            if (GUILayout.Button("下移", GUILayout.Width(48f)) && i < frames.arraySize - 1)
                            {
                                frames.MoveArrayElement(i, i + 1);
                            }

                            if (GUILayout.Button("复制", GUILayout.Width(48f)))
                            {
                                frames.InsertArrayElementAtIndex(i);
                                frames.MoveArrayElement(i, i + 1);
                                var copy = frames.GetArrayElementAtIndex(i + 1);
                                copy.FindPropertyRelative("frameName").stringValue = FrameTitle(frame) + " 副本";
                            }

                            if (GUILayout.Button("删除", GUILayout.Width(48f)))
                            {
                                frames.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }

                        if (frame.isExpanded)
                        {
                            EditorGUI.indentLevel++;
                            Field(frame, "frameName", "帧名称");
                            Field(frame, "sprite", "帧图片");
                            Field(frame, "worldSize", "图片世界尺寸");
                            Field(frame, "pivot", "中心点(0-1)");
                            Field(frame, "offset", "位置偏移");
                            Field(frame, "duration", "单帧时长");
                            var pivot = frame.FindPropertyRelative("pivot").vector2Value;
                            if (pivot.x < 0f || pivot.x > 1f || pivot.y < 0f || pivot.y > 1f)
                            {
                                EditorGUILayout.HelpBox("中心点建议保持在0到1之间，例如(0.5, 0.5)代表图片中心。", MessageType.Warning);
                            }

                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }
        }

        private static void DrawProjectileItem(SerializedProperty item)
        {
            Field(item, "displayName", "显示名称");
            Field(item, "element", "元素类型");
            Field(item, "projectileSprite", "弹体图片");
            Field(item, "sizeMultiplier", "尺寸倍率");
            Field(item, "rotateToDirection", "朝向跟随方向");
            Field(item, "trailSprite", "拖尾图片");
            Field(item, "hitVfx", "命中特效");
            Field(item, "tint", "染色");
        }

        private static void DrawDamageItem(SerializedProperty item)
        {
            Field(item, "displayName", "显示名称");
            Field(item, "damageType", "伤害类型");
            Field(item, "textColor", "跳字颜色");
            Field(item, "fontSize", "字号");
            Field(item, "floatHeight", "上飘距离");
            Field(item, "lifetime", "生命周期");
            Field(item, "mergeFrequentNumbers", "合并高频跳字");
            Field(item, "showKillPrefix", "击杀显示KILL");
            Field(item, "hitVfx", "命中特效");
            Field(item, "hitSfx", "命中音效");
        }

        private static void DrawAoeItem(SerializedProperty item)
        {
            Field(item, "displayName", "显示名称");
            Field(item, "effectId", "特效ID");
            Field(item, "sprite", "特效图片");
            Field(item, "radiusMultiplier", "半径倍率");
            Field(item, "duration", "持续时间");
            Field(item, "tint", "颜色");
            Field(item, "loop", "是否循环");
            Field(item, "sortingOrder", "排序层级");
            Field(item, "matchDamageRadius", "匹配伤害范围");
            Field(item, "disableLensDistortion", "禁用镜头畸变");
        }

        private static void DrawMonsterItem(SerializedProperty item)
        {
            Field(item, "kind", "怪物类型");
            Field(item, "displayName", "显示名称");
            Field(item, "cost", "消耗");
            Field(item, "health", "生命");
            Field(item, "damage", "伤害");
            Field(item, "moveSpeed", "移动速度");
            Field(item, "attackRange", "攻击范围");
            Field(item, "attackCooldown", "攻击冷却");
            Field(item, "poisonDamage", "毒伤DPS");
            Field(item, "color", "代表颜色");
            Field(item, "tag", "标签");
        }

        private static void DrawSkillItem(SerializedProperty item)
        {
            Field(item, "id", "技能ID");
            Field(item, "displayName", "显示名称");
            Field(item, "type", "技能类型");
            Field(item, "cost", "消耗");
            Field(item, "cooldown", "冷却");
            Field(item, "warningTime", "预警时间");
            Field(item, "radius", "范围半径");
            Field(item, "duration", "持续时间");
            Field(item, "damage", "伤害");
            Field(item, "tickDamage", "持续伤害");
            Field(item, "slowMultiplier", "减速倍率");
            Field(item, "antiHealSeconds", "禁疗时间");
            Field(item, "shieldBreakSeconds", "破盾时间");
            Field(item, "danger", "危险度");
            Field(item, "warningColor", "预警颜色");
            Field(item, "effectColor", "效果颜色");
            Field(item, "tag", "标签");
        }

        private static void DrawUiLayoutItem(SerializedProperty item)
        {
            Field(item, "id", "配置ID");
            Field(item, "displayName", "显示名称");
            Field(item, "anchor", "锚点");
            Field(item, "position", "位置");
            Field(item, "size", "大小");
            Field(item, "backgroundColor", "背景颜色");
            Field(item, "visible", "是否显示");
        }

        private static void DrawUiTextItem(SerializedProperty item)
        {
            Field(item, "id", "配置ID");
            Field(item, "displayName", "显示名称");
            Field(item, "overrideText", "覆盖文字");
            Field(item, "fontSize", "字体大小");
            Field(item, "color", "文字颜色");
            Field(item, "alignment", "对齐方式");
            Field(item, "visible", "是否显示");
        }

        private static void DrawUiButtonGroupItem(SerializedProperty item)
        {
            Field(item, "id", "配置ID");
            Field(item, "displayName", "显示名称");
            Field(item, "buttonSize", "按钮大小");
            Field(item, "firstPosition", "第一个按钮位置");
            Field(item, "spacing", "按钮间距");
            Field(item, "columns", "每行列数");
            Field(item, "labelFontSize", "标题字号");
            Field(item, "metaFontSize", "描述字号");
            Field(item, "statusFontSize", "状态字号");
            Field(item, "iconPosition", "图标位置");
            Field(item, "iconSize", "图标大小");
        }

        private static void Field(SerializedProperty item, string propertyName, string label)
        {
            EditorGUILayout.PropertyField(item.FindPropertyRelative(propertyName), new GUIContent(label), true);
        }

        private static string DisplayName(SerializedProperty item)
        {
            var name = item.FindPropertyRelative("displayName");
            if (name != null && !string.IsNullOrEmpty(name.stringValue))
            {
                return name.stringValue;
            }

            var assetId = item.FindPropertyRelative("assetId");
            if (assetId != null && !string.IsNullOrEmpty(assetId.stringValue))
            {
                return assetId.stringValue;
            }

            var effectId = item.FindPropertyRelative("effectId");
            if (effectId != null && !string.IsNullOrEmpty(effectId.stringValue))
            {
                return effectId.stringValue;
            }

            return "未命名配置";
        }

        private static string ActionTitle(SerializedProperty action)
        {
            var displayName = action.FindPropertyRelative("displayName");
            if (displayName != null && !string.IsNullOrEmpty(displayName.stringValue))
            {
                return displayName.stringValue;
            }

            return action.FindPropertyRelative("action").enumDisplayNames[action.FindPropertyRelative("action").enumValueIndex];
        }

        private static string FrameTitle(SerializedProperty frame)
        {
            var frameName = frame.FindPropertyRelative("frameName");
            if (frameName != null && !string.IsNullOrEmpty(frameName.stringValue))
            {
                return frameName.stringValue;
            }

            var sprite = frame.FindPropertyRelative("sprite").objectReferenceValue;
            return sprite != null ? sprite.name : "未命名帧";
        }

        private static void EnsureStandardActions(SerializedProperty actions)
        {
            AddActionIfMissing(actions, UnitAnimationAction.Idle, "待机", true);
            AddActionIfMissing(actions, UnitAnimationAction.Move, "移动", true);
            AddActionIfMissing(actions, UnitAnimationAction.Attack, "攻击", false);
            AddActionIfMissing(actions, UnitAnimationAction.Hit, "受击", false);
            AddActionIfMissing(actions, UnitAnimationAction.Death, "死亡", false);
            AddActionIfMissing(actions, UnitAnimationAction.Spawn, "出生", false);
            AddActionIfMissing(actions, UnitAnimationAction.Cast, "施法", false);
        }

        private static void AddActionIfMissing(SerializedProperty actions, UnitAnimationAction actionType, string displayName, bool loop)
        {
            for (var i = 0; i < actions.arraySize; i++)
            {
                var action = actions.GetArrayElementAtIndex(i);
                if (action.FindPropertyRelative("action").enumValueIndex == (int)actionType)
                {
                    return;
                }
            }

            actions.arraySize++;
            SetupAction(actions.GetArrayElementAtIndex(actions.arraySize - 1), actionType, displayName, null, loop);
        }

        private static void SetupAction(SerializedProperty action, UnitAnimationAction actionType, string displayName, Sprite defaultSprite, bool loop)
        {
            action.FindPropertyRelative("action").enumValueIndex = (int)actionType;
            action.FindPropertyRelative("displayName").stringValue = displayName;
            action.FindPropertyRelative("framesPerSecond").floatValue = 8f;
            action.FindPropertyRelative("loop").boolValue = loop;
            var frames = action.FindPropertyRelative("frames");
            if (frames.arraySize == 0)
            {
                frames.arraySize = 1;
                SetupFrame(frames.GetArrayElementAtIndex(0), "第1帧", defaultSprite);
            }
        }

        private static void SetupFrame(SerializedProperty frame, string frameName, Sprite sprite)
        {
            frame.FindPropertyRelative("frameName").stringValue = frameName;
            frame.FindPropertyRelative("sprite").objectReferenceValue = sprite;
            frame.FindPropertyRelative("worldSize").vector2Value = EstimateWorldSize(sprite);
            frame.FindPropertyRelative("pivot").vector2Value = new Vector2(0.5f, 0.5f);
            frame.FindPropertyRelative("offset").vector2Value = Vector2.zero;
            frame.FindPropertyRelative("duration").floatValue = 0.12f;
            frame.isExpanded = true;
        }

        private static Vector2 EstimateWorldSize(Sprite sprite)
        {
            if (sprite == null)
            {
                return Vector2.one;
            }

            var bounds = sprite.bounds.size;
            if (bounds.x <= 0f || bounds.y <= 0f)
            {
                return Vector2.one;
            }

            return new Vector2(bounds.x, bounds.y);
        }

        private void LoadOrCreateDatabase()
        {
            if (database != null && serializedDatabase != null)
            {
                return;
            }

            database = AssetDatabase.LoadAssetAtPath<MusicManiacConfigDatabase>(DatabasePath);
            if (database == null)
            {
                var folder = Path.GetDirectoryName(DatabasePath)?.Replace("\\", "/");
                if (!string.IsNullOrEmpty(folder) && !AssetDatabase.IsValidFolder(folder))
                {
                    EnsureFolder(folder);
                }

                database = CreateInstance<MusicManiacConfigDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
                FillDefaults();
                AssetDatabase.SaveAssets();
            }

            serializedDatabase = new SerializedObject(database);
            if (NeedsAnimationMigration(database))
            {
                FillDefaults();
                AssetDatabase.SaveAssets();
            }
        }

        private static void EnsureFolder(string folder)
        {
            var parts = folder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private void SaveDatabase()
        {
            serializedDatabase.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ShowNotification(new GUIContent("配置已保存"));
        }

        private static bool NeedsAnimationMigration(MusicManiacConfigDatabase db)
        {
            if (db.characterAnimations.Count == 0)
            {
                return true;
            }

            foreach (var animation in db.characterAnimations)
            {
                if (animation.actions == null || animation.actions.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void FillDefaults()
        {
            Undo.RecordObject(database, "补齐默认配置");
            FillAnimationDefaults(database);
            FillProjectileDefaults(database);
            FillDamageDefaults(database);
            FillAoeDefaults(database);
            FillMonsterDefaults(database);
            FillSkillDefaults(database);
            FillUiDefaults(database);
            EditorUtility.SetDirty(database);
            serializedDatabase = new SerializedObject(database);
        }

        private static void AddAnimationItem(SerializedProperty item)
        {
            item.FindPropertyRelative("displayName").stringValue = "新序列帧";
            item.FindPropertyRelative("assetId").stringValue = "characters/new_animation";
            item.FindPropertyRelative("worldHeight").floatValue = 1.25f;
            EnsureStandardActions(item.FindPropertyRelative("actions"));
        }

        private static void AddProjectileItem(SerializedProperty item)
        {
            item.FindPropertyRelative("displayName").stringValue = "新弹体";
            item.FindPropertyRelative("sizeMultiplier").floatValue = 1.25f;
            item.FindPropertyRelative("rotateToDirection").boolValue = true;
            item.FindPropertyRelative("tint").colorValue = Color.white;
        }

        private static void AddDamageItem(SerializedProperty item)
        {
            item.FindPropertyRelative("displayName").stringValue = "新伤害表现";
            item.FindPropertyRelative("textColor").colorValue = Color.white;
            item.FindPropertyRelative("fontSize").intValue = 18;
            item.FindPropertyRelative("floatHeight").floatValue = 0.92f;
            item.FindPropertyRelative("lifetime").floatValue = 0.78f;
            item.FindPropertyRelative("mergeFrequentNumbers").boolValue = true;
            item.FindPropertyRelative("showKillPrefix").boolValue = true;
        }

        private static void AddAoeItem(SerializedProperty item)
        {
            item.FindPropertyRelative("displayName").stringValue = "新AOE特效";
            item.FindPropertyRelative("effectId").stringValue = "vfx/new_effect";
            item.FindPropertyRelative("radiusMultiplier").floatValue = 1f;
            item.FindPropertyRelative("duration").floatValue = 0.35f;
            item.FindPropertyRelative("tint").colorValue = Color.white;
            item.FindPropertyRelative("sortingOrder").intValue = 16;
            item.FindPropertyRelative("matchDamageRadius").boolValue = true;
            item.FindPropertyRelative("disableLensDistortion").boolValue = true;
        }

        private static void AddMonsterItem(SerializedProperty item)
        {
            item.FindPropertyRelative("displayName").stringValue = "新怪物";
            item.FindPropertyRelative("cost").floatValue = 10f;
            item.FindPropertyRelative("health").floatValue = 30f;
            item.FindPropertyRelative("damage").floatValue = 5f;
            item.FindPropertyRelative("moveSpeed").floatValue = 1f;
            item.FindPropertyRelative("attackRange").floatValue = 0.6f;
            item.FindPropertyRelative("attackCooldown").floatValue = 1f;
            item.FindPropertyRelative("color").colorValue = Color.white;
            item.FindPropertyRelative("tag").stringValue = "标签";
        }

        private static void AddSkillItem(SerializedProperty item)
        {
            item.FindPropertyRelative("displayName").stringValue = "新技能";
            item.FindPropertyRelative("cost").floatValue = 50f;
            item.FindPropertyRelative("cooldown").floatValue = 6f;
            item.FindPropertyRelative("warningTime").floatValue = 0.8f;
            item.FindPropertyRelative("radius").floatValue = 1f;
            item.FindPropertyRelative("slowMultiplier").floatValue = 1f;
            item.FindPropertyRelative("warningColor").colorValue = new Color(1f, 0.2f, 0.12f, 0.66f);
            item.FindPropertyRelative("effectColor").colorValue = Color.white;
            item.FindPropertyRelative("tag").stringValue = "标签";
        }

        private static void AddUiLayoutItem(SerializedProperty item)
        {
            item.FindPropertyRelative("id").stringValue = "new_ui_layout";
            item.FindPropertyRelative("displayName").stringValue = "新UI布局";
            item.FindPropertyRelative("anchor").enumValueIndex = (int)UiAnchorPreset.TopLeft;
            item.FindPropertyRelative("position").vector2Value = Vector2.zero;
            item.FindPropertyRelative("size").vector2Value = new Vector2(200f, 80f);
            item.FindPropertyRelative("backgroundColor").colorValue = Color.white;
            item.FindPropertyRelative("visible").boolValue = true;
        }

        private static void AddUiTextItem(SerializedProperty item)
        {
            item.FindPropertyRelative("id").stringValue = "new_ui_text";
            item.FindPropertyRelative("displayName").stringValue = "新UI文字";
            item.FindPropertyRelative("overrideText").stringValue = string.Empty;
            item.FindPropertyRelative("fontSize").intValue = 14;
            item.FindPropertyRelative("color").colorValue = Color.white;
            item.FindPropertyRelative("alignment").enumValueIndex = (int)TextAnchor.MiddleCenter;
            item.FindPropertyRelative("visible").boolValue = true;
        }

        private static void AddUiButtonGroupItem(SerializedProperty item)
        {
            item.FindPropertyRelative("id").stringValue = "new_button_group";
            item.FindPropertyRelative("displayName").stringValue = "新按钮组";
            item.FindPropertyRelative("buttonSize").vector2Value = new Vector2(132f, 50f);
            item.FindPropertyRelative("firstPosition").vector2Value = Vector2.zero;
            item.FindPropertyRelative("spacing").vector2Value = new Vector2(144f, -56f);
            item.FindPropertyRelative("columns").intValue = 2;
            item.FindPropertyRelative("labelFontSize").intValue = 10;
            item.FindPropertyRelative("metaFontSize").intValue = 9;
            item.FindPropertyRelative("statusFontSize").intValue = 8;
            item.FindPropertyRelative("iconPosition").vector2Value = new Vector2(22f, 0f);
            item.FindPropertyRelative("iconSize").vector2Value = new Vector2(28f, 28f);
        }

        private static void FillAnimationDefaults(MusicManiacConfigDatabase db)
        {
            AddOrUpdateAnimation(db, true, MonsterKind.Skeleton, "AI角色", "characters/hero_music_maniac", 1.25f);
            AddOrUpdateAnimation(db, false, MonsterKind.Skeleton, "噪点小怪", "characters/monster_noise_blob", 0.9f);
            AddOrUpdateAnimation(db, false, MonsterKind.VenomBug, "毒液歌者", "characters/monster_venom_singer", 0.85f);
            AddOrUpdateAnimation(db, false, MonsterKind.Archer, "磁带射手", "characters/monster_cassette_thrower", 0.9f);
            AddOrUpdateAnimation(db, false, MonsterKind.Stoneguard, "音箱重卫", "characters/monster_speaker_brute", 1.05f);
            AddOrUpdateAnimation(db, false, MonsterKind.HexPriest, "节拍巫师", "characters/monster_metronome_wizard", 0.95f);
            AddOrUpdateAnimation(db, false, MonsterKind.Shieldbreaker, "调音叉破盾者", "characters/monster_tuning_fork_breaker", 0.95f);
            AddOrUpdateAnimation(db, false, MonsterKind.Assassin, "线缆刺客", "characters/monster_cable_assassin", 0.88f);
            AddOrUpdateAnimation(db, false, MonsterKind.BoneKing, "失真之王", "characters/monster_distortion_king", 1.45f);
        }

        private static void FillProjectileDefaults(MusicManiacConfigDatabase db)
        {
            AddOrUpdateProjectile(db, ElementModule.None, "基础音符弹", "projectiles/projectile_basic_note", 1.2f);
            AddOrUpdateProjectile(db, ElementModule.Lightning, "闪电音符弹", "projectiles/projectile_lightning_note", 1.28f);
            AddOrUpdateProjectile(db, ElementModule.Ice, "冰冻音符弹", "projectiles/projectile_ice_note", 1.28f);
            AddOrUpdateProjectile(db, ElementModule.Fire, "火焰音符弹", "projectiles/projectile_fire_note", 1.34f);
            AddOrUpdateProjectile(db, ElementModule.Poison, "毒气音符弹", "projectiles/projectile_poison_note", 1.28f);
        }

        private static void FillDamageDefaults(MusicManiacConfigDatabase db)
        {
            AddOrUpdateDamage(db, DamageFeedbackType.Physical, "普通伤害", new Color(0.94f, 0.96f, 0.92f), 18);
            AddOrUpdateDamage(db, DamageFeedbackType.Fire, "火焰伤害", new Color(1f, 0.34f, 0.08f), 19);
            AddOrUpdateDamage(db, DamageFeedbackType.Ice, "冰冻伤害", new Color(0.58f, 0.9f, 1f), 18);
            AddOrUpdateDamage(db, DamageFeedbackType.Lightning, "闪电暴击", new Color(0.45f, 0.68f, 1f), 22);
            AddOrUpdateDamage(db, DamageFeedbackType.Poison, "毒气DOT", new Color(0.36f, 0.95f, 0.24f), 15);
            AddOrUpdateDamage(db, DamageFeedbackType.Sonic, "音波AOE", new Color(0.88f, 0.46f, 1f), 20);
            AddOrUpdateDamage(db, DamageFeedbackType.ShieldBreak, "破盾伤害", new Color(0.22f, 0.86f, 1f), 22);
        }

        private static void FillAoeDefaults(MusicManiacConfigDatabase db)
        {
            AddOrUpdateAoe(db, "warning_ring", "技能预警圆", "vfx/vfx_warning_ring", 1f, 0.8f, new Color(1f, 0.25f, 0.15f, 0.82f), false, true, false);
            AddOrUpdateAoe(db, "sonic_ring", "音波冲击环", "vfx/vfx_sonic_ring", 1f, 0.35f, new Color(0.88f, 0.46f, 1f), false, true, false);
            AddOrUpdateAoe(db, "hit_spark", "命中火花", "vfx/vfx_hit_spark", 1f, 0.22f, Color.white, false, false, false);
            AddOrUpdateAoe(db, "death_noise", "死亡噪波", "vfx/vfx_death_noise", 1f, 0.42f, new Color(0.95f, 0.48f, 1f), false, false, true);
            AddOrUpdateAoe(db, "spawn_smoke", "出生烟雾", "vfx/vfx_spawn_smoke", 1f, 0.45f, Color.white, false, false, false);
            AddOrUpdateAoe(db, "poison_area", "毒气区域", "tiles/tile_poison", 1f, 4f, new Color(0.36f, 0.95f, 0.24f, 0.66f), true, true, false);
            AddOrUpdateAoe(db, "ice_area", "冰冻区域", "tiles/tile_ice", 1f, 4f, new Color(0.58f, 0.9f, 1f, 0.66f), true, true, false);
            AddOrUpdateAoe(db, "fire_area", "火焰区域", "tiles/tile_fire", 1f, 4f, new Color(1f, 0.34f, 0.08f, 0.66f), true, true, false);
        }

        private static void FillMonsterDefaults(MusicManiacConfigDatabase db)
        {
            AddOrUpdateMonster(db, MonsterKind.Skeleton, "Skeleton", 10f, 36f, 6f, 1.55f, 0.55f, 0.65f, 0f, new Color(0.82f, 0.86f, 0.86f), "Cheap swarm");
            AddOrUpdateMonster(db, MonsterKind.VenomBug, "Venom Bug", 15f, 26f, 3f, 1.95f, 0.45f, 0.5f, 4f, new Color(0.36f, 0.92f, 0.32f), "Poison");
            AddOrUpdateMonster(db, MonsterKind.Archer, "Archer", 25f, 24f, 7f, 1.1f, 5.8f, 1.25f, 0f, new Color(0.95f, 0.74f, 0.36f), "Ranged");
            AddOrUpdateMonster(db, MonsterKind.Stoneguard, "Stoneguard", 40f, 115f, 8f, 0.75f, 0.7f, 0.9f, 0f, new Color(0.42f, 0.48f, 0.52f), "Tank");
            AddOrUpdateMonster(db, MonsterKind.HexPriest, "Hex Priest", 60f, 42f, 4f, 1f, 4.7f, 1.4f, 0f, new Color(0.62f, 0.44f, 0.95f), "Anti-heal");
            AddOrUpdateMonster(db, MonsterKind.Shieldbreaker, "Shieldbreaker", 70f, 56f, 10f, 1.35f, 1.2f, 1.05f, 0f, new Color(0.2f, 0.72f, 0.95f), "Break shield");
            AddOrUpdateMonster(db, MonsterKind.Assassin, "Assassin", 80f, 34f, 22f, 2.55f, 0.55f, 1.25f, 0f, new Color(0.18f, 0.15f, 0.2f), "Burst");
            AddOrUpdateMonster(db, MonsterKind.BoneKing, "Bone King", 160f, 420f, 20f, 0.85f, 1.15f, 0.75f, 0f, new Color(0.9f, 0.88f, 0.68f), "Boss");
        }

        private static void FillSkillDefaults(MusicManiacConfigDatabase db)
        {
            AddOrUpdateSkill(db, CreatorSkillId.LightningStrike, "Lightning", CreatorSkillType.Damage, 50f, 6f, 0.6f, 0.9f, 0f, 95f, 0f, 1f, 0f, 0f, 0.55f, new Color(1f, 0.2f, 0.12f, 0.66f), new Color(1f, 0.95f, 0.52f, 0.72f), "Burst");
            AddOrUpdateSkill(db, CreatorSkillId.FrostField, "Frost Field", CreatorSkillType.Control, 120f, 20f, 1.2f, 1.85f, 4f, 18f, 4f, 0.48f, 0f, 0f, 0.72f, new Color(0.95f, 0.16f, 0.14f, 0.58f), new Color(0.25f, 0.62f, 1f, 0.62f), "Slow");
            AddOrUpdateSkill(db, CreatorSkillId.AntiHealCurse, "Anti-Heal", CreatorSkillType.Curse, 120f, 18f, 1.2f, 1.35f, 0f, 28f, 0f, 1f, 6f, 0f, 0.78f, new Color(0.95f, 0.08f, 0.18f, 0.6f), new Color(0.72f, 0.28f, 0.92f, 0.62f), "Anti-heal");
            AddOrUpdateSkill(db, CreatorSkillId.ShieldBrand, "Shield Brand", CreatorSkillType.Curse, 110f, 16f, 1f, 1.35f, 0f, 20f, 0f, 1f, 0f, 6f, 0.74f, new Color(0.95f, 0.12f, 0.12f, 0.6f), new Color(0.12f, 0.82f, 1f, 0.62f), "Break shield");
            AddOrUpdateSkill(db, CreatorSkillId.BoneWall, "Bone Wall", CreatorSkillType.Terrain, 150f, 28f, 1f, 1.3f, 6f, 0f, 0f, 1f, 0f, 0f, 0.68f, new Color(0.92f, 0.18f, 0.12f, 0.58f), new Color(0.78f, 0.76f, 0.62f, 1f), "Terrain");
            AddOrUpdateSkill(db, CreatorSkillId.DemonHand, "Demon Hand", CreatorSkillType.Finisher, 500f, 60f, 2.5f, 2.55f, 0f, 330f, 0f, 1f, 0f, 0f, 1f, new Color(0.32f, 0f, 0f, 0.78f), new Color(0.95f, 0.1f, 0.05f, 0.82f), "Finisher");
        }

        private static void FillUiDefaults(MusicManiacConfigDatabase db)
        {
            AddOrUpdateUiLayout(db, "top_status_bar", "顶部状态栏", UiAnchorPreset.TopStretch, new Vector2(0f, -8f), new Vector2(-28f, 58f), new Color(0.02f, 0.018f, 0.028f, 0.92f));
            AddOrUpdateUiLayout(db, "ai_portrait", "AI头像", UiAnchorPreset.TopLeft, new Vector2(14f, -9f), new Vector2(44f, 44f), new Color(0.08f, 0.04f, 0.12f, 0.95f));
            AddOrUpdateUiLayout(db, "ai_summary", "AI概要文字", UiAnchorPreset.TopLeft, new Vector2(66f, -8f), new Vector2(270f, 42f), Color.white);
            AddOrUpdateUiLayout(db, "ai_hp_bar", "AI血条", UiAnchorPreset.TopLeft, new Vector2(346f, -13f), new Vector2(330f, 14f), new Color(0.12f, 0.07f, 0.12f, 0.96f));
            AddOrUpdateUiLayout(db, "timer_text", "倒计时", UiAnchorPreset.TopCenter, new Vector2(-30f, -13f), new Vector2(110f, 36f), Color.white);
            AddOrUpdateUiLayout(db, "threat_bar", "威胁条", UiAnchorPreset.TopCenter, new Vector2(140f, -15f), new Vector2(190f, 12f), new Color(0.1f, 0.07f, 0.04f, 0.9f));
            AddOrUpdateUiLayout(db, "energy_bar", "能量条", UiAnchorPreset.TopRight, new Vector2(-250f, -15f), new Vector2(190f, 14f), new Color(0.09f, 0.05f, 0.12f, 0.95f));
            AddOrUpdateUiLayout(db, "left_info_panel", "左侧目标面板", UiAnchorPreset.TopLeft, new Vector2(10f, -76f), new Vector2(250f, 272f), new Color(0.025f, 0.024f, 0.034f, 0.88f));
            AddOrUpdateUiLayout(db, "message_tape", "提示信息框", UiAnchorPreset.BottomStretch, new Vector2(0f, 12f), new Vector2(-24f, 78f), new Color(0.04f, 0.02f, 0.04f, 0.92f));
            AddOrUpdateUiLayout(db, "right_action_deck", "右侧行动牌组", UiAnchorPreset.TopRight, new Vector2(-10f, -76f), new Vector2(316f, 706f), new Color(0.023f, 0.017f, 0.033f, 0.93f));
            AddOrUpdateUiLayout(db, "bottom_rhythm_deck", "底部节奏面板", UiAnchorPreset.BottomStretch, new Vector2(0f, 8f), new Vector2(-440f, 184f), new Color(0.018f, 0.017f, 0.024f, 0.94f));
            AddOrUpdateUiLayout(db, "bd_tooltip", "BD提示框", UiAnchorPreset.BottomLeft, new Vector2(276f, 202f), new Vector2(384f, 168f), new Color(0.025f, 0.03f, 0.035f, 0.96f));
            AddOrUpdateUiLayout(db, "main_menu", "主界面蒙版", UiAnchorPreset.Stretch, Vector2.zero, Vector2.zero, new Color(0.01f, 0.012f, 0.016f, 0.96f));
            AddOrUpdateUiLayout(db, "main_title", "主界面标题", UiAnchorPreset.TopCenter, new Vector2(0f, -230f), new Vector2(620f, 70f), Color.white);
            AddOrUpdateUiLayout(db, "main_subtitle", "主界面说明", UiAnchorPreset.TopCenter, new Vector2(0f, -310f), new Vector2(760f, 70f), Color.white);
            AddOrUpdateUiLayout(db, "main_start_button", "开始游戏按钮", UiAnchorPreset.TopCenter, new Vector2(0f, -410f), new Vector2(220f, 54f), new Color(0.15f, 0.13f, 0.18f, 0.96f));
            AddOrUpdateUiLayout(db, "main_tip", "主界面提示", UiAnchorPreset.TopCenter, new Vector2(0f, -486f), new Vector2(760f, 48f), Color.white);
            AddOrUpdateUiLayout(db, "result_panel", "结算界面蒙版", UiAnchorPreset.Stretch, Vector2.zero, Vector2.zero, new Color(0.008f, 0.009f, 0.012f, 0.94f));
            AddOrUpdateUiLayout(db, "result_title", "结算标题", UiAnchorPreset.TopCenter, new Vector2(0f, -260f), new Vector2(680f, 78f), Color.white);
            AddOrUpdateUiLayout(db, "result_body", "结算正文", UiAnchorPreset.TopCenter, new Vector2(0f, -340f), new Vector2(760f, 92f), Color.white);
            AddOrUpdateUiLayout(db, "result_restart_button", "重新开始按钮", UiAnchorPreset.TopCenter, new Vector2(0f, -456f), new Vector2(220f, 54f), new Color(0.15f, 0.13f, 0.18f, 0.96f));

            AddOrUpdateUiText(db, "ai_summary", "AI概要文字", string.Empty, 14, new Color(0.93f, 0.94f, 0.92f), TextAnchor.UpperLeft);
            AddOrUpdateUiText(db, "timer_text", "倒计时文字", string.Empty, 24, new Color(0.93f, 0.94f, 0.92f), TextAnchor.MiddleCenter);
            AddOrUpdateUiText(db, "objective_text", "目标文字", string.Empty, 15, new Color(0.93f, 0.94f, 0.92f), TextAnchor.UpperLeft);
            AddOrUpdateUiText(db, "message_text", "提示文字", string.Empty, 13, new Color(0.1f, 0.08f, 0.11f, 1f), TextAnchor.UpperLeft);
            AddOrUpdateUiText(db, "rhythm_header", "节奏标题", string.Empty, 12, new Color(0.93f, 0.94f, 0.92f), TextAnchor.MiddleLeft);
            AddOrUpdateUiText(db, "rhythm_tracks", "节奏轨道文字", string.Empty, 12, new Color(0.93f, 0.94f, 0.92f), TextAnchor.UpperLeft);
            AddOrUpdateUiText(db, "bd_title", "BD标题", string.Empty, 12, new Color(0.93f, 0.94f, 0.92f), TextAnchor.MiddleLeft);
            AddOrUpdateUiText(db, "bd_tooltip_text", "BD提示文字", string.Empty, 12, new Color(0.93f, 0.94f, 0.92f), TextAnchor.UpperLeft);
            AddOrUpdateUiText(db, "main_title", "主界面标题文字", "击败音乐疯子", 34, new Color(1f, 0.86f, 0.42f, 1f), TextAnchor.MiddleCenter);
            AddOrUpdateUiText(db, "main_subtitle", "主界面说明文字", "作为造物主召唤怪物、释放技能，在1分钟内击败AI角色。", 17, new Color(0.78f, 0.88f, 0.92f, 1f), TextAnchor.MiddleCenter);
            AddOrUpdateUiText(db, "main_tip", "主界面提示文字", "右侧选择怪物或技能，点击战场进行召唤或释放。配置请在Unity顶部菜单 Tools/Defeat Music Maniac/配置编辑器 中修改。", 13, new Color(0.58f, 0.66f, 0.72f, 1f), TextAnchor.MiddleCenter);
            AddOrUpdateUiText(db, "result_title", "结算标题文字", string.Empty, 34, Color.white, TextAnchor.MiddleCenter);
            AddOrUpdateUiText(db, "result_body", "结算正文文字", string.Empty, 17, new Color(0.78f, 0.88f, 0.92f, 1f), TextAnchor.MiddleCenter);

            AddOrUpdateButtonGroup(db, "summon_buttons", "召唤怪物按钮组", new Vector2(132f, 50f), new Vector2(76f, 70f), new Vector2(144f, -56f), 2, 10, 9, 8, new Vector2(22f, 0f), new Vector2(28f, 28f));
            AddOrUpdateButtonGroup(db, "skill_buttons", "造物主技能按钮组", new Vector2(132f, 50f), new Vector2(76f, 46f), new Vector2(144f, -56f), 2, 10, 9, 8, new Vector2(22f, 0f), new Vector2(28f, 28f));
            AddOrUpdateButtonGroup(db, "bd_buttons", "BD按钮组", new Vector2(58f, 40f), new Vector2(48f, 24f), new Vector2(70f, -48f), 5, 10, 9, 8, Vector2.zero, Vector2.zero);
        }

        private static void AddOrUpdateAnimation(MusicManiacConfigDatabase db, bool isHero, MonsterKind kind, string name, string path, float worldHeight)
        {
            var item = db.characterAnimations.Find(x => x.isHero == isHero && (!isHero && x.monsterKind == kind || isHero));
            if (item == null)
            {
                item = new CharacterAnimationConfig();
                db.characterAnimations.Add(item);
            }

            item.isHero = isHero;
            item.monsterKind = kind;
            item.displayName = name;
            item.assetId = path;
            item.worldHeight = worldHeight;
            EnsureRuntimeStandardActions(item, LoadSprite(path), worldHeight);
        }

        private static void EnsureRuntimeStandardActions(CharacterAnimationConfig item, Sprite defaultSprite, float worldHeight)
        {
            AddRuntimeActionIfMissing(item, UnitAnimationAction.Idle, "待机", true, defaultSprite, worldHeight);
            AddRuntimeActionIfMissing(item, UnitAnimationAction.Move, "移动", true, defaultSprite, worldHeight);
            AddRuntimeActionIfMissing(item, UnitAnimationAction.Attack, "攻击", false, defaultSprite, worldHeight);
            AddRuntimeActionIfMissing(item, UnitAnimationAction.Hit, "受击", false, defaultSprite, worldHeight);
            AddRuntimeActionIfMissing(item, UnitAnimationAction.Death, "死亡", false, defaultSprite, worldHeight);
            AddRuntimeActionIfMissing(item, UnitAnimationAction.Spawn, "出生", false, defaultSprite, worldHeight);
            AddRuntimeActionIfMissing(item, UnitAnimationAction.Cast, "施法", false, defaultSprite, worldHeight);
        }

        private static void AddRuntimeActionIfMissing(CharacterAnimationConfig item, UnitAnimationAction actionType, string displayName, bool loop, Sprite defaultSprite, float worldHeight)
        {
            var action = item.actions.Find(x => x.action == actionType);
            if (action == null)
            {
                action = new UnitActionAnimationConfig();
                item.actions.Add(action);
            }

            action.action = actionType;
            action.displayName = displayName;
            action.loop = loop;
            if (action.frames.Count == 0)
            {
                action.frames.Add(new UnitAnimationFrameConfig());
            }

            var frame = action.frames[0];
            if (string.IsNullOrEmpty(frame.frameName))
            {
                frame.frameName = "第1帧";
            }

            if (frame.sprite == null)
            {
                frame.sprite = defaultSprite;
            }

            if (frame.worldSize.x <= 0f || frame.worldSize.y <= 0f || frame.worldSize == Vector2.one)
            {
                frame.worldSize = EstimateWorldSize(defaultSprite, worldHeight);
            }

            if (frame.pivot == Vector2.zero)
            {
                frame.pivot = new Vector2(0.5f, 0.5f);
            }

            if (frame.duration <= 0f)
            {
                frame.duration = 0.12f;
            }
        }

        private static Vector2 EstimateWorldSize(Sprite sprite, float worldHeight)
        {
            if (sprite == null || sprite.bounds.size.y <= 0f)
            {
                return new Vector2(worldHeight, worldHeight);
            }

            var aspect = sprite.bounds.size.x / sprite.bounds.size.y;
            return new Vector2(worldHeight * aspect, worldHeight);
        }

        private static void AddOrUpdateProjectile(MusicManiacConfigDatabase db, ElementModule element, string name, string path, float size)
        {
            var item = db.projectileVisuals.Find(x => x.element == element);
            if (item == null)
            {
                item = new ProjectileVisualConfig();
                db.projectileVisuals.Add(item);
            }

            item.element = element;
            item.displayName = name;
            item.projectileSprite = LoadSprite(path);
            item.sizeMultiplier = size;
            item.hitVfx = LoadSprite("vfx/vfx_hit_spark");
        }

        private static void AddOrUpdateDamage(MusicManiacConfigDatabase db, DamageFeedbackType type, string name, Color color, int fontSize)
        {
            var item = db.damageVisuals.Find(x => x.damageType == type);
            if (item == null)
            {
                item = new DamageVisualConfig();
                db.damageVisuals.Add(item);
            }

            item.damageType = type;
            item.displayName = name;
            item.textColor = color;
            item.fontSize = fontSize;
            item.hitVfx = LoadSprite(type == DamageFeedbackType.Sonic ? "vfx/vfx_sonic_ring" : "vfx/vfx_hit_spark");
        }

        private static void AddOrUpdateAoe(MusicManiacConfigDatabase db, string id, string name, string path, float radius, float duration, Color tint, bool loop, bool matchRadius, bool disableLens)
        {
            var item = db.aoeVfx.Find(x => x.effectId == id);
            if (item == null)
            {
                item = new AoeVfxConfig();
                db.aoeVfx.Add(item);
            }

            item.effectId = id;
            item.displayName = name;
            item.sprite = LoadSprite(path);
            item.radiusMultiplier = radius;
            item.duration = duration;
            item.tint = tint;
            item.loop = loop;
            item.matchDamageRadius = matchRadius;
            item.disableLensDistortion = disableLens;
        }

        private static void AddOrUpdateMonster(MusicManiacConfigDatabase db, MonsterKind kind, string name, float cost, float hp, float damage, float speed, float range, float cooldown, float poison, Color color, string tag)
        {
            var item = db.monsterValues.Find(x => x.kind == kind);
            if (item == null)
            {
                item = new MonsterBalanceConfig();
                db.monsterValues.Add(item);
            }

            item.kind = kind;
            item.displayName = name;
            item.cost = cost;
            item.health = hp;
            item.damage = damage;
            item.moveSpeed = speed;
            item.attackRange = range;
            item.attackCooldown = cooldown;
            item.poisonDamage = poison;
            item.color = color;
            item.tag = tag;
        }

        private static void AddOrUpdateSkill(MusicManiacConfigDatabase db, CreatorSkillId id, string name, CreatorSkillType type, float cost, float cooldown, float warning, float radius, float duration, float damage, float tick, float slow, float antiHeal, float shieldBreak, float danger, Color warningColor, Color effectColor, string tag)
        {
            var item = db.skillValues.Find(x => x.id == id);
            if (item == null)
            {
                item = new CreatorSkillValueConfig();
                db.skillValues.Add(item);
            }

            item.id = id;
            item.displayName = name;
            item.type = type;
            item.cost = cost;
            item.cooldown = cooldown;
            item.warningTime = warning;
            item.radius = radius;
            item.duration = duration;
            item.damage = damage;
            item.tickDamage = tick;
            item.slowMultiplier = slow;
            item.antiHealSeconds = antiHeal;
            item.shieldBreakSeconds = shieldBreak;
            item.danger = danger;
            item.warningColor = warningColor;
            item.effectColor = effectColor;
            item.tag = tag;
        }

        private static void AddOrUpdateUiLayout(MusicManiacConfigDatabase db, string id, string name, UiAnchorPreset anchor, Vector2 position, Vector2 size, Color color)
        {
            var item = db.uiLayouts.Find(x => x.id == id);
            if (item == null)
            {
                item = new UiLayoutConfig();
                db.uiLayouts.Add(item);
            }

            item.id = id;
            item.displayName = name;
            item.anchor = anchor;
            item.position = position;
            item.size = size;
            item.backgroundColor = color;
            item.visible = true;
        }

        private static void AddOrUpdateUiText(MusicManiacConfigDatabase db, string id, string name, string text, int fontSize, Color color, TextAnchor alignment)
        {
            var item = db.uiTexts.Find(x => x.id == id);
            if (item == null)
            {
                item = new UiTextConfig();
                db.uiTexts.Add(item);
            }

            item.id = id;
            item.displayName = name;
            item.overrideText = text;
            item.fontSize = fontSize;
            item.color = color;
            item.alignment = alignment;
            item.visible = true;
        }

        private static void AddOrUpdateButtonGroup(MusicManiacConfigDatabase db, string id, string name, Vector2 buttonSize, Vector2 firstPosition, Vector2 spacing, int columns, int labelFontSize, int metaFontSize, int statusFontSize, Vector2 iconPosition, Vector2 iconSize)
        {
            var item = db.uiButtonGroups.Find(x => x.id == id);
            if (item == null)
            {
                item = new UiButtonGroupConfig();
                db.uiButtonGroups.Add(item);
            }

            item.id = id;
            item.displayName = name;
            item.buttonSize = buttonSize;
            item.firstPosition = firstPosition;
            item.spacing = spacing;
            item.columns = columns;
            item.labelFontSize = labelFontSize;
            item.metaFontSize = metaFontSize;
            item.statusFontSize = statusFontSize;
            item.iconPosition = iconPosition;
            item.iconSize = iconSize;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/ReverseSurvivorArt/{path}.png");
        }
    }
}
