// =========================================================
// FILE: AutoProjectBootstrapper.cs
// PATH: Assets/_Game/Scripts/Editor/AutoProjectBootstrapper.cs
// =========================================================
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EchoOfTheLantern.EditorTools
{
    [InitializeOnLoad]
    public static class AutoProjectBootstrapper
    {
        internal const string ProjectRoot = "Assets/_Game";
        internal const string ArtRoot = "Assets/_Game/Art";
        internal const string SpritesRoot = "Assets/_Game/Art/Sprites";
        internal const string AtlasesRoot = "Assets/_Game/Art/Atlases";
        internal const string MaterialsRoot = "Assets/_Game/Art/Materials";
        internal const string PalettesRoot = "Assets/_Game/Art/Palettes";
        internal const string FontsRoot = "Assets/_Game/Art/Fonts";
        internal const string AudioRoot = "Assets/_Game/Audio";
        internal const string PrefabsRoot = "Assets/_Game/Prefabs";
        internal const string ScenesRoot = "Assets/_Game/Scenes";
        internal const string ScriptableObjectsRoot = "Assets/_Game/ScriptableObjects";
        internal const string ScriptsRoot = "Assets/_Game/Scripts";
        internal const string AnimationRoot = "Assets/_Game/Animation";
        internal const string SettingsRoot = "Assets/_Game/Settings";
        internal const string IncomingRoot = "Assets/_Game/Incoming";
        internal const string PlaceholderRoot = "Assets/_Game/_Placeholders";

        private const string SessionKeyQueued = "EchoOfTheLantern.AutoBootstrap.Queued";
        private const string SessionKeyRunning = "EchoOfTheLantern.AutoBootstrap.Running";

        internal static bool IsRunning => SessionState.GetBool(SessionKeyRunning, false);

        private static readonly HashSet<string> RequiredFolders = new(StringComparer.OrdinalIgnoreCase)
        {
            ProjectRoot,
            ArtRoot,
            SpritesRoot,
            Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            Path.Combine(SpritesRoot, "Player").Replace("\\", "/"),
            Path.Combine(SpritesRoot, "Interactables").Replace("\\", "/"),
            Path.Combine(SpritesRoot, "UI").Replace("\\", "/"),
            Path.Combine(SpritesRoot, "Effects").Replace("\\", "/"),
            Path.Combine(SpritesRoot, "Icons").Replace("\\", "/"),
            AtlasesRoot,
            MaterialsRoot,
            PalettesRoot,
            FontsRoot,
            AudioRoot,
            Path.Combine(AudioRoot, "Ambient").Replace("\\", "/"),
            Path.Combine(AudioRoot, "Gameplay").Replace("\\", "/"),
            Path.Combine(AudioRoot, "UI").Replace("\\", "/"),
            Path.Combine(AudioRoot, "Effects").Replace("\\", "/"),
            PrefabsRoot,
            Path.Combine(PrefabsRoot, "Player").Replace("\\", "/"),
            Path.Combine(PrefabsRoot, "Environment").Replace("\\", "/"),
            Path.Combine(PrefabsRoot, "Interactables").Replace("\\", "/"),
            Path.Combine(PrefabsRoot, "UI").Replace("\\", "/"),
            Path.Combine(PrefabsRoot, "Effects").Replace("\\", "/"),
            ScenesRoot,
            ScriptableObjectsRoot,
            ScriptsRoot,
            Path.Combine(ScriptsRoot, "Editor").Replace("\\", "/"),
            AnimationRoot,
            SettingsRoot,
            IncomingRoot,
            PlaceholderRoot,
            Path.Combine(PlaceholderRoot, "Sprites").Replace("\\", "/"),
            Path.Combine(PlaceholderRoot, "Audio").Replace("\\", "/"),
            Path.Combine(PlaceholderRoot, "Materials").Replace("\\", "/"),
        };

        private static readonly Dictionary<string, string> CanonicalFileTargets = new(StringComparer.OrdinalIgnoreCase)
        {
            // Sprites
            ["SPR_Ground_Stone_Base.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Ground_Stone_Var01.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Ground_Stone_Var02.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Ground_Stone_Var03.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Wall_Stone.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Wall_Broken.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Pillar.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Pillar_Broken.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Rubble_Small.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Rubble_Large.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Statue.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),
            ["SPR_Background_Main.png"] = Path.Combine(SpritesRoot, "Environment").Replace("\\", "/"),

            ["SPR_Player_Idle.png"] = Path.Combine(SpritesRoot, "Player").Replace("\\", "/"),
            ["SPR_Player_Walk.png"] = Path.Combine(SpritesRoot, "Player").Replace("\\", "/"),
            ["SPR_Player_Hurt.png"] = Path.Combine(SpritesRoot, "Player").Replace("\\", "/"),
            ["SPR_Player_Lantern.png"] = Path.Combine(SpritesRoot, "Player").Replace("\\", "/"),
            ["SPR_Player_Shadow.png"] = Path.Combine(SpritesRoot, "Player").Replace("\\", "/"),

            ["SPR_Beacon_Off.png"] = Path.Combine(SpritesRoot, "Interactables").Replace("\\", "/"),
            ["SPR_Beacon_On.png"] = Path.Combine(SpritesRoot, "Interactables").Replace("\\", "/"),
            ["SPR_Shrine.png"] = Path.Combine(SpritesRoot, "Interactables").Replace("\\", "/"),
            ["SPR_Refill.png"] = Path.Combine(SpritesRoot, "Interactables").Replace("\\", "/"),
            ["SPR_Gate_Closed.png"] = Path.Combine(SpritesRoot, "Interactables").Replace("\\", "/"),
            ["SPR_Gate_Open.png"] = Path.Combine(SpritesRoot, "Interactables").Replace("\\", "/"),
            ["SPR_Fragment.png"] = Path.Combine(SpritesRoot, "Interactables").Replace("\\", "/"),
            ["SPR_Shadow_Hazard.png"] = Path.Combine(SpritesRoot, "Interactables").Replace("\\", "/"),

            ["SPR_UI_MenuBackground.png"] = Path.Combine(SpritesRoot, "UI").Replace("\\", "/"),
            ["SPR_UI_Win.png"] = Path.Combine(SpritesRoot, "UI").Replace("\\", "/"),
            ["SPR_UI_Lose.png"] = Path.Combine(SpritesRoot, "UI").Replace("\\", "/"),
            ["SPR_UI_HUD.png"] = Path.Combine(SpritesRoot, "UI").Replace("\\", "/"),
            ["SPR_Button.png"] = Path.Combine(SpritesRoot, "UI").Replace("\\", "/"),
            ["SPR_Button_Hover.png"] = Path.Combine(SpritesRoot, "UI").Replace("\\", "/"),

            ["ICO_Lantern.png"] = Path.Combine(SpritesRoot, "Icons").Replace("\\", "/"),
            ["ICO_Beacon.png"] = Path.Combine(SpritesRoot, "Icons").Replace("\\", "/"),
            ["ICO_Shrine.png"] = Path.Combine(SpritesRoot, "Icons").Replace("\\", "/"),
            ["ICO_Warning.png"] = Path.Combine(SpritesRoot, "Icons").Replace("\\", "/"),
            ["ICO_Restart.png"] = Path.Combine(SpritesRoot, "Icons").Replace("\\", "/"),

            ["FX_LanternGlow.png"] = Path.Combine(SpritesRoot, "Effects").Replace("\\", "/"),
            ["FX_BeaconGlow.png"] = Path.Combine(SpritesRoot, "Effects").Replace("\\", "/"),
            ["FX_Dust.png"] = Path.Combine(SpritesRoot, "Effects").Replace("\\", "/"),
            ["FX_Spark.png"] = Path.Combine(SpritesRoot, "Effects").Replace("\\", "/"),
            ["FX_ShadowPulse.png"] = Path.Combine(SpritesRoot, "Effects").Replace("\\", "/"),
            ["FX_Mist.png"] = Path.Combine(SpritesRoot, "Effects").Replace("\\", "/"),

            // Audio
            ["AMB_ShrineNight.wav"] = Path.Combine(AudioRoot, "Ambient").Replace("\\", "/"),
            ["SFX_Footstep_Stone.wav"] = Path.Combine(AudioRoot, "Gameplay").Replace("\\", "/"),
            ["SFX_Lantern.wav"] = Path.Combine(AudioRoot, "Gameplay").Replace("\\", "/"),
            ["SFX_BeaconActivate.wav"] = Path.Combine(AudioRoot, "Gameplay").Replace("\\", "/"),
            ["SFX_Collect.wav"] = Path.Combine(AudioRoot, "Gameplay").Replace("\\", "/"),
            ["SFX_Warning.wav"] = Path.Combine(AudioRoot, "Gameplay").Replace("\\", "/"),
            ["UI_Click.wav"] = Path.Combine(AudioRoot, "UI").Replace("\\", "/"),
            ["SFX_Win.wav"] = Path.Combine(AudioRoot, "Effects").Replace("\\", "/"),
            ["SFX_Lose.wav"] = Path.Combine(AudioRoot, "Effects").Replace("\\", "/"),

            // Materials
            ["MAT_Ground_Stone.mat"] = MaterialsRoot,
            ["MAT_Wall_Stone.mat"] = MaterialsRoot,
            ["MAT_Player.mat"] = MaterialsRoot,
            ["MAT_Beacon.mat"] = MaterialsRoot,
            ["MAT_Shrine.mat"] = MaterialsRoot,
            ["MAT_Shadow.mat"] = MaterialsRoot,
            ["MAT_UI.mat"] = MaterialsRoot,
            ["MAT_Glow.mat"] = MaterialsRoot,
            ["MAT_Mist.mat"] = MaterialsRoot,

            // Atlases
            ["ATL_Environment.spriteatlas"] = AtlasesRoot,
            ["ATL_Interactables.spriteatlas"] = AtlasesRoot,
            ["ATL_UI.spriteatlas"] = AtlasesRoot,
            ["ATL_Effects.spriteatlas"] = AtlasesRoot,

            // ScriptableObjects
            ["SO_GameConfig.asset"] = ScriptableObjectsRoot,
            ["SO_PlayerConfig.asset"] = ScriptableObjectsRoot,
            ["SO_LevelConfig.asset"] = ScriptableObjectsRoot,
            ["SO_UIConfig.asset"] = ScriptableObjectsRoot,
            ["SO_AudioConfig.asset"] = ScriptableObjectsRoot,
            ["SO_InteractionConfig.asset"] = ScriptableObjectsRoot,
            ["SO_BeaconConfig.asset"] = ScriptableObjectsRoot,
            ["SO_HazardConfig.asset"] = ScriptableObjectsRoot,
        };

        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            QueueRun();
        }

        internal static void QueueRun()
        {
            if (SessionState.GetBool(SessionKeyRunning, false) || SessionState.GetBool(SessionKeyQueued, false))
                return;

            SessionState.SetBool(SessionKeyQueued, true);
            EditorApplication.delayCall -= ExecuteQueuedRun;
            EditorApplication.delayCall += ExecuteQueuedRun;
        }

        private static void ExecuteQueuedRun()
        {
            EditorApplication.delayCall -= ExecuteQueuedRun;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (!SessionState.GetBool(SessionKeyQueued, false))
                return;

            SessionState.SetBool(SessionKeyQueued, false);
            RunBootstrap();
        }

        private static void RunBootstrap()
        {
            if (SessionState.GetBool(SessionKeyRunning, false))
                return;

            SessionState.SetBool(SessionKeyRunning, true);
            try
            {
                AssetDatabase.StartAssetEditing();
                EnsureFolderScaffold();
                CreatePlaceholderAssetsIfMissing();
                ReorganizeProjectAssets();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                SessionState.SetBool(SessionKeyRunning, false);
            }

            AutoPrefabFactory.QueueRun();
        }

        private static void EnsureFolderScaffold()
        {
            foreach (string folder in RequiredFolders)
                EnsureFolder(folder);
        }

        private static void CreatePlaceholderAssetsIfMissing()
        {
            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "Environment"), "SPR_Ground_Stone_Base.png", 512, 512, new Color32(96, 104, 118, 255), new Color32(134, 144, 160, 255), false, 512f);
            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "Player"), "SPR_Player_Idle.png", 512, 512, new Color32(86, 92, 102, 255), new Color32(214, 182, 110, 255), false, 512f);

            // Required beacon states and wall placeholders for the prefab factory
            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "Interactables"), "SPR_Beacon_Off.png", 512, 512, new Color32(100, 100, 110, 255), new Color32(220, 170, 85, 255), false, 512f);
            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "Interactables"), "SPR_Beacon_On.png", 512, 512, new Color32(120, 110, 80, 255), new Color32(255, 215, 120, 255), true, 512f);
            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "Environment"), "SPR_Wall_Stone.png", 512, 512, new Color32(92, 98, 108, 255), new Color32(126, 132, 144, 255), false, 512f);

            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "Interactables"), "SPR_Shrine.png", 1024, 1024, new Color32(90, 92, 100, 255), new Color32(220, 180, 90, 255), false, 1024f);
            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "UI"), "SPR_UI_HUD.png", 1024, 256, new Color32(40, 46, 58, 255), new Color32(120, 130, 150, 255), false, 100f);
            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "Effects"), "FX_LanternGlow.png", 512, 512, new Color32(0, 0, 0, 0), new Color32(255, 192, 96, 220), true, 512f);
            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "Icons"), "ICO_Lantern.png", 128, 128, new Color32(54, 60, 70, 255), new Color32(255, 193, 96, 255), false, 128f);

            // Added missing hazard visual placeholder texture
            EnsurePlaceholderTexture(Path.Combine(SpritesRoot, "Effects"), "SPR_Shadow_Hazard.png", 512, 512, new Color32(24, 24, 28, 255), new Color32(90, 40, 110, 255), true, 512f);

            EnsurePlaceholderAudio(Path.Combine(AudioRoot, "Ambient"), "AMB_ShrineNight.wav", 1f, 2f);
            EnsurePlaceholderAudio(Path.Combine(AudioRoot, "Gameplay"), "SFX_BeaconActivate.wav", 0.25f, 2f);
            EnsurePlaceholderAudio(Path.Combine(AudioRoot, "UI"), "UI_Click.wav", 0.1f, 2f);
            EnsurePlaceholderAudio(Path.Combine(AudioRoot, "Effects"), "SFX_Win.wav", 0.3f, 2f);

            EnsurePlaceholderMaterial(Path.Combine(MaterialsRoot, "MAT_Ground_Stone.mat"));
            EnsurePlaceholderMaterial(Path.Combine(MaterialsRoot, "MAT_Player.mat"));
            EnsurePlaceholderMaterial(Path.Combine(MaterialsRoot, "MAT_Beacon.mat"));
            EnsurePlaceholderMaterial(Path.Combine(MaterialsRoot, "MAT_UI.mat"));
            EnsurePlaceholderMaterial(Path.Combine(MaterialsRoot, "MAT_Glow.mat"));
        }

        private static void ReorganizeProjectAssets()
        {
            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { ProjectRoot });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(assetPath))
                    continue;

                string fileName = Path.GetFileName(assetPath);
                if (!CanonicalFileTargets.TryGetValue(fileName, out string targetFolder))
                    continue;

                string currentFolder = Path.GetDirectoryName(assetPath)?.Replace("\\", "/") ?? string.Empty;
                if (string.Equals(currentFolder, targetFolder, StringComparison.OrdinalIgnoreCase))
                    continue;

                EnsureFolder(targetFolder);
                string targetPath = Path.Combine(targetFolder, fileName).Replace("\\", "/");

                if (File.Exists(GetFullPath(targetPath)))
                    continue;

                string error = AssetDatabase.MoveAsset(assetPath, targetPath);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning($"Could not move {assetPath} -> {targetPath}: {error}");
                }
            }
        }

        private static void EnsurePlaceholderTexture(string folderPath, string fileName, int width, int height, Color32 baseColor, Color32 accentColor, bool radialAlpha, float pixelsPerUnit)
        {
            EnsureFolder(folderPath);
            string assetPath = Path.Combine(folderPath, fileName).Replace("\\", "/");
            string fullPath = GetFullPath(assetPath);

            if (File.Exists(fullPath))
                return;

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float blend = Mathf.InverseLerp(0f, height - 1, y);
                    Color32 c = Color32.Lerp(baseColor, accentColor, blend);

                    if (radialAlpha)
                    {
                        float dx = x - width * 0.5f;
                        float dy = y - height * 0.5f;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy) / (Mathf.Min(width, height) * 0.5f);
                        c.a = (byte)(c.a * Mathf.Clamp01(1f - dist));
                    }

                    texture.SetPixel(x, y, c);
                }

            texture.Apply();
            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            ImportAsSprite(assetPath, pixelsPerUnit);
        }

        private static void ImportAsSprite(string assetPath, float pixelsPerUnit)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        private static void EnsurePlaceholderAudio(string folderPath, string fileName, float seconds, float sampleRate)
        {
            EnsureFolder(folderPath);
            string assetPath = Path.Combine(folderPath, fileName).Replace("\\", "/");
            string fullPath = GetFullPath(assetPath);

            if (File.Exists(fullPath))
                return;

            int channels = 1;
            int bitsPerSample = 16;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(seconds * sampleRate));
            int byteRate = (int)(sampleRate * channels * bitsPerSample / 8);
            int blockAlign = channels * bitsPerSample / 8;
            int dataSize = sampleCount * blockAlign;
            int fileSize = 36 + dataSize;

            using FileStream stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.ASCII);

            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write((int)sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitsPerSample);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            for (int i = 0; i < sampleCount; i++)
                writer.Write((short)0);
        }

        private static void EnsurePlaceholderMaterial(string materialPath)
        {
            EnsureFolder(Path.GetDirectoryName(materialPath)?.Replace("\\", "/") ?? MaterialsRoot);
            if (File.Exists(GetFullPath(materialPath)))
                return;

            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Texture");
            Material material = new Material(shader) { name = Path.GetFileNameWithoutExtension(materialPath) };
            AssetDatabase.CreateAsset(material, materialPath);
        }

        private static void EnsureFolder(string folderPath)
        {
            folderPath = folderPath.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/") ?? string.Empty;
            string leaf = Path.GetFileName(folderPath);
            if (string.IsNullOrEmpty(parent) || parent == folderPath)
                return;

            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static string GetFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }
    }
}
#endif