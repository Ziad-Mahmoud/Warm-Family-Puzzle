using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WarmFamilyPuzzle.EditorTools
{
    // One-shot project setup + build menu. ConfigureBatch runs from the CLI
    // on first import; the menu items are for everyday use afterwards.
    public static class ProjectConfigurator
    {
        const string ScenePath = "Assets/Scenes/Main.unity";
        const string AppId = "com.zizoelzoz.warmfamilypuzzle";

        public static void ConfigureBatch()
        {
            Configure();
        }

        [MenuItem("Warm Family Puzzle/Configure Project")]
        public static void Configure()
        {
            PlayerSettings.companyName = "ZizoElZoz";
            PlayerSettings.productName = "Warm Family Puzzle";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AppId);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, AppId);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 480;
            PlayerSettings.defaultScreenHeight = 920;
            PlayerSettings.resizableWindow = true;

            PlayerSettings.WebGL.decompressionFallback = true;

            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.showUnityLogo = false;

            EnsureScene();
            ConfigurePhotoImporters();
            ApplyAppIcon();

            AssetDatabase.SaveAssets();
            Debug.Log("[WarmFamilyPuzzle] Project configured.");
        }

        static void EnsureScene()
        {
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        static void ConfigurePhotoImporters()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Photos" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is TextureImporter importer)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.mipmapEnabled = false;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.maxTextureSize = 2048;
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.SaveAndReimport();
                }
            }
        }

        static void ApplyAppIcon()
        {
            const string iconPath = "Assets/Icons/app_icon.png";
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (icon == null)
            {
                AssetDatabase.ImportAsset(iconPath);
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            }

            if (icon != null)
            {
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            }
        }

        // --- builds ----------------------------------------------------------

        [MenuItem("Warm Family Puzzle/Build/Windows x64")]
        public static void BuildWindows()
        {
            Build(BuildTarget.StandaloneWindows64, "Builds/Windows/WarmFamilyPuzzle.exe");
        }

        [MenuItem("Warm Family Puzzle/Build/WebGL")]
        public static void BuildWebGL()
        {
            Build(BuildTarget.WebGL, "Builds/WebGL");
        }

        [MenuItem("Warm Family Puzzle/Build/Android APK")]
        public static void BuildAndroid()
        {
            Build(BuildTarget.Android, "Builds/Android/WarmFamilyPuzzle.apk");
        }

        [MenuItem("Warm Family Puzzle/Build/iOS (Xcode project)")]
        public static void BuildIos()
        {
            Build(BuildTarget.iOS, "Builds/iOS");
        }

        static void Build(BuildTarget target, string location)
        {
            Configure();
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                target = target,
                locationPathName = location,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log("[WarmFamilyPuzzle] Build " + target + ": " + report.summary.result +
                      " → " + location);
        }
    }
}
