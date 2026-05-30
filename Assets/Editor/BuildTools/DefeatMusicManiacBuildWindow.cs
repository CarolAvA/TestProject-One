using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace ReverseSurvivorPrototype.EditorTools
{
    public sealed class DefeatMusicManiacBuildWindow : EditorWindow
    {
        private const string OutputRootPrefsKey = "DefeatMusicManiac.Build.OutputRoot";
        private const string ProductName = "DefeatMusicManiac";
        private const string DefaultOutputRoot = "Builds";

        private string outputRoot;
        private Vector2 scroll;

        [MenuItem("Tools/Defeat Music Maniac/Build Window")]
        public static void Open()
        {
            var window = GetWindow<DefeatMusicManiacBuildWindow>("Music Maniac Build");
            window.minSize = new Vector2(420f, 300f);
            window.Show();
        }

        [MenuItem("Tools/Defeat Music Maniac/Build/PC Windows")]
        public static void BuildWindowsPlayer()
        {
            BuildWindowsPlayer(GetOutputRoot());
        }

        [MenuItem("Tools/Defeat Music Maniac/Build/Android APK")]
        public static void BuildAndroidApk()
        {
            BuildAndroidApk(GetOutputRoot());
        }

        [MenuItem("Tools/Defeat Music Maniac/Build/Build All")]
        public static void BuildAll()
        {
            var root = GetOutputRoot();
            BuildWindowsPlayer(root);
            BuildAndroidApk(root);
        }

        public static void BuildWindowsFromCommandLine()
        {
            BuildWindowsPlayer(GetCommandLineOutputRoot());
        }

        public static void BuildAndroidFromCommandLine()
        {
            BuildAndroidApk(GetCommandLineOutputRoot());
        }

        public static void BuildAllFromCommandLine()
        {
            var root = GetCommandLineOutputRoot();
            BuildWindowsPlayer(root);
            BuildAndroidApk(root);
        }

        private void OnEnable()
        {
            outputRoot = GetOutputRoot();
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Defeat Music Maniac Build", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Builds use enabled scenes from Build Settings. Outputs default to Builds/PC and Builds/Android.", MessageType.Info);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Output Folder", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                outputRoot = EditorGUILayout.TextField(outputRoot);
                if (GUILayout.Button("Browse", GUILayout.Width(72f)))
                {
                    var selected = EditorUtility.OpenFolderPanel("Choose build output folder", FullProjectPath(outputRoot), string.Empty);
                    if (!string.IsNullOrEmpty(selected))
                    {
                        outputRoot = ProjectRelativePath(selected);
                        SaveOutputRoot(outputRoot);
                    }
                }
            }

            if (GUILayout.Button("Save Output Folder"))
            {
                SaveOutputRoot(outputRoot);
            }

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Build Buttons", EditorStyles.boldLabel);
            if (GUILayout.Button("Build PC Windows", GUILayout.Height(34f)))
            {
                SaveOutputRoot(outputRoot);
                BuildWindowsPlayer(outputRoot);
            }

            if (GUILayout.Button("Build Android APK", GUILayout.Height(34f)))
            {
                SaveOutputRoot(outputRoot);
                BuildAndroidApk(outputRoot);
            }

            if (GUILayout.Button("Build PC + Android", GUILayout.Height(38f)))
            {
                SaveOutputRoot(outputRoot);
                BuildWindowsPlayer(outputRoot);
                BuildAndroidApk(outputRoot);
            }

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Command Line", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(
                "-quit -batchmode -projectPath <project> -executeMethod ReverseSurvivorPrototype.EditorTools.DefeatMusicManiacBuildWindow.BuildAllFromCommandLine",
                EditorStyles.textField,
                GUILayout.Height(38f));
            EditorGUILayout.EndScrollView();
        }

        private static void BuildWindowsPlayer(string root)
        {
            var outputPath = FullProjectPath(Path.Combine(root, "PC", $"{ProductName}.exe"));
            Build(BuildTarget.StandaloneWindows64, outputPath);
        }

        private static void BuildAndroidApk(string root)
        {
            var outputPath = FullProjectPath(Path.Combine(root, "Android", $"{ProductName}.apk"));
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.reverseprototype.defeatmusicmaniac");
            PlayerSettings.Android.bundleVersionCode = Mathf.Max(1, PlayerSettings.Android.bundleVersionCode);
            EditorUserBuildSettings.buildAppBundle = false;
            Build(BuildTarget.Android, outputPath);
        }

        private static void Build(BuildTarget target, string outputPath)
        {
            EnsureRenderPipelineSettings();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            var scenes = GetEnabledScenes();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            Debug.Log($"Building {target} to {outputPath}");
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"{target} build failed: {report.summary.result}. Errors: {report.summary.totalErrors}");
            }

            Debug.Log($"{target} build succeeded: {outputPath} ({report.summary.totalSize} bytes)");
        }

        private static void EnsureRenderPipelineSettings()
        {
            const string universalRpPath = "Assets/Settings/UniversalRP.asset";
            var pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(universalRpPath);
            if (pipelineAsset == null)
            {
                Debug.LogWarning($"URP asset missing at {universalRpPath}. Build may render magenta if the active pipeline is invalid.");
                return;
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
            for (var i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipelineAsset;
            }

            var standaloneIndex = Mathf.Clamp(QualitySettings.names.Length - 1, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(standaloneIndex, false);
            EditorUtility.SetDirty(pipelineAsset);
            AssetDatabase.SaveAssets();
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && File.Exists(scene.path))
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length > 0)
            {
                return scenes;
            }

            const string fallbackScene = "Assets/Scenes/SampleScene.unity";
            if (File.Exists(fallbackScene))
            {
                return new[] { fallbackScene };
            }

            throw new BuildFailedException("No enabled scenes found in Build Settings.");
        }

        private static string GetOutputRoot()
        {
            return EditorPrefs.GetString(OutputRootPrefsKey, DefaultOutputRoot);
        }

        private static void SaveOutputRoot(string root)
        {
            EditorPrefs.SetString(OutputRootPrefsKey, string.IsNullOrWhiteSpace(root) ? DefaultOutputRoot : root);
        }

        private static string GetCommandLineOutputRoot()
        {
            var args = System.Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-buildOutput")
                {
                    return args[i + 1];
                }
            }

            return GetOutputRoot();
        }

        private static string FullProjectPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        }

        private static string ProjectRelativePath(string path)
        {
            var projectRoot = Directory.GetCurrentDirectory().Replace('\\', '/');
            var normalized = path.Replace('\\', '/');
            return normalized.StartsWith(projectRoot) ? normalized.Substring(projectRoot.Length).TrimStart('/') : path;
        }
    }
}
