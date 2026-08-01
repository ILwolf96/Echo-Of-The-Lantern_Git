#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EchoOfTheLantern.EditorTools
{
    /// <summary>
    /// Enforces the project folder structure, creates fallback placeholder assets,
    /// and rehomes misplaced assets into the correct folders based on naming conventions.
    ///
    /// This script exists because the project must remain functional even if final art/audio
    /// has not yet been generated or imported.
    /// 
    /// Planned role in the pipeline:
    /// - Run first, or whenever the project structure needs repair.
    /// - Ensure the exact folder scaffold exists.
    /// - Create placeholder sprites, textures, and silent audio fallbacks.
    /// - Move misplaced assets into the correct canonical folders.
    /// - Then allow the asset bootstrapper / prefab factory / scene composer to run.
    /// </summary>
    public sealed class ProjectScaffoldWindow : EditorWindow
    {
        private const string GameRoot = "Assets/_Game";
        private const string ArtRoot = "Assets/_Game/Art";
        private const string ArtSpritesRoot = "Assets/_Game/Art/Sprites";
        private const string ArtAtlasesRoot = "Assets/_Game/Art/Atlases";
        private const string ArtMaterialsRoot = "Assets/_Game/Art/Materials";
        private const string ArtPalettesRoot = "Assets/_Game/Art/Palettes";
        private const string ArtFontsRoot = "Assets/_Game/Art/Fonts";
        private const string AudioRoot = "Assets/_Game/Audio";
        private const string PrefabsRoot = "Assets/_Game/Prefabs";
        private const string ScenesRoot = "Assets/_Game/Scenes";
        private const string ScriptableObjectsRoot = "Assets/_Game/ScriptableObjects";
        private const string ScriptsRoot = "Assets/_Game/Scripts";
        private const string AnimationRoot = "Assets/_Game/Animation";
        private const string SettingsRoot = "Assets/_Game/Settings";
        private const string IncomingRoot = "Assets/_Game/Incoming";
        private const string TempRoot = "Assets/_Game/_TempPlaceholders";

        private static readonly string[] RequiredFolders =
        {
            GameRoot,
            ArtRoot,
            ArtSpritesRoot,
            Path.Combine(ArtSpritesRoot, "Environment"),
            Path.Combine(ArtSpritesRoot, "Player"),
            Path.Combine(ArtSpritesRoot, "Interactables"),
            Path.Combine(ArtSpritesRoot, "UI"),
            Path.Combine(ArtSpritesRoot, "Effects"),
            Path.Combine(ArtSpritesRoot, "Icons"),
            ArtAtlasesRoot,
            ArtMaterialsRoot,
            ArtPalettesRoot,
            ArtFontsRoot,
            AudioRoot,
            Path.Combine(AudioRoot, "Ambient"),
            Path.Combine(AudioRoot, "Gameplay"),
            Path.Combine(AudioRoot, "UI"),
            Path.Combine(AudioRoot, "Effects"),
            PrefabsRoot,
            Path.Combine(PrefabsRoot, "Player"),
            Path.Combine(PrefabsRoot, "Environment"),
            Path.Combine(PrefabsRoot, "Interactables"),
            Path.Combine(PrefabsRoot, "UI"),
            Path.Combine(PrefabsRoot, "Effects"),
            ScenesRoot,
            ScriptableObjectsRoot,
            ScriptsRoot,
            Path.Combine(ScriptsRoot, "Editor"),
            AnimationRoot,
            SettingsRoot,
            IncomingRoot,
            TempRoot
        };

        private static readonly Dictionary<string, string> CanonicalFileTargets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Environment sprites
            ["SPR_Ground_Stone_Base.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Ground_Stone_Var01.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Ground_Stone_Var02.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Ground_Stone_Var03.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Wall_Stone.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Wall_Broken.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Pillar.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Pillar_Broken.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Rubble_Small.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Rubble_Large.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Statue.png"] = Path.Combine(ArtSpritesRoot, "Environment"),
            ["SPR_Background_Main.png"] = Path.Combine(ArtSpritesRoot, "Environment"),

            // Player
            ["SPR_Player_Idle.png"] = Path.Combine(ArtSpritesRoot, "Player"),
            ["SPR_Player_Walk.png"] = Path.Combine(ArtSpritesRoot, "Player"),
            ["SPR_Player_Hurt.png"] = Path.Combine(ArtSpritesRoot, "Player"),
            ["SPR_Player_Lantern.png"] = Path.Combine(ArtSpritesRoot, "Player"),
            ["SPR_Player_Shadow.png"] = Path.Combine(ArtSpritesRoot, "Player"),

            // Interactables
            ["SPR_Beacon_Off.png"] = Path.Combine(ArtSpritesRoot, "Interactables"),
            ["SPR_Beacon_On.png"] = Path.Combine(ArtSpritesRoot, "Interactables"),
            ["SPR_Shrine.png"] = Path.Combine(ArtSpritesRoot, "Interactables"),
            ["SPR_Refill.png"] = Path.Combine(ArtSpritesRoot, "Interactables"),
            ["SPR_Gate_Closed.png"] = Path.Combine(ArtSpritesRoot, "Interactables"),
            ["SPR_Gate_Open.png"] = Path.Combine(ArtSpritesRoot, "Interactables"),
            ["SPR_Fragment.png"] = Path.Combine(ArtSpritesRoot, "Interactables"),
            ["SPR_Shadow_Hazard.png"] = Path.Combine(ArtSpritesRoot, "Interactables"),

            // UI
            ["SPR_UI_MenuBackground.png"] = Path.Combine(ArtSpritesRoot, "UI"),
            ["SPR_UI_Win.png"] = Path.Combine(ArtSpritesRoot, "UI"),
            ["SPR_UI_Lose.png"] = Path.Combine(ArtSpritesRoot, "UI"),
            ["SPR_UI_HUD.png"] = Path.Combine(ArtSpritesRoot, "UI"),
            ["SPR_Button.png"] = Path.Combine(ArtSpritesRoot, "UI"),
            ["SPR_Button_Hover.png"] = Path.Combine(ArtSpritesRoot, "UI"),

            // Icons
            ["ICO_Lantern.png"] = Path.Combine(ArtSpritesRoot, "Icons"),
            ["ICO_Beacon.png"] = Path.Combine(ArtSpritesRoot, "Icons"),
            ["ICO_Shrine.png"] = Path.Combine(ArtSpritesRoot, "Icons"),
            ["ICO_Warning.png"] = Path.Combine(ArtSpritesRoot, "Icons"),
            ["ICO_Restart.png"] = Path.Combine(ArtSpritesRoot, "Icons"),

            // Effects
            ["FX_LanternGlow.png"] = Path.Combine(ArtSpritesRoot, "Effects"),
            ["FX_BeaconGlow.png"] = Path.Combine(ArtSpritesRoot, "Effects"),
            ["FX_Dust.png"] = Path.Combine(ArtSpritesRoot, "Effects"),
            ["FX_Spark.png"] = Path.Combine(ArtSpritesRoot, "Effects"),
            ["FX_ShadowPulse.png"] = Path.Combine(ArtSpritesRoot, "Effects"),
            ["FX_Mist.png"] = Path.Combine(ArtSpritesRoot, "Effects"),

            // Audio
            ["AMB_ShrineNight.wav"] = Path.Combine(AudioRoot, "Ambient"),
            ["SFX_Footstep_Stone.wav"] = Path.Combine(AudioRoot, "Gameplay"),
            ["SFX_Lantern.wav"] = Path.Combine(AudioRoot, "Gameplay"),
            ["SFX_BeaconActivate.wav"] = Path.Combine(AudioRoot, "Gameplay"),
            ["SFX_Collect.wav"] = Path.Combine(AudioRoot, "Gameplay"),
            ["SFX_Warning.wav"] = Path.Combine(AudioRoot, "Gameplay"),
            ["UI_Click.wav"] = Path.Combine(AudioRoot, "UI"),
            ["SFX_Win.wav"] = Path.Combine(AudioRoot, "Effects"),
            ["SFX_Lose.wav"] = Path.Combine(AudioRoot, "Effects"),

            // Materials
            ["MAT_Ground_Stone.mat"] = ArtMaterialsRoot,
            ["MAT_Wall_Stone.mat"] = ArtMaterialsRoot,
            ["MAT_Player.mat"] = ArtMaterialsRoot,
            ["MAT_Beacon.mat"] = ArtMaterialsRoot,
            ["MAT_Shrine.mat"] = ArtMaterialsRoot,
            ["MAT_Shadow.mat"] = ArtMaterialsRoot,
            ["MAT_UI.mat"] = ArtMaterialsRoot,
            ["MAT_Glow.mat"] = ArtMaterialsRoot,
            ["MAT_Mist.mat"] = ArtMaterialsRoot,

            // Atlases
            ["ATL_Environment.spriteatlas"] = ArtAtlasesRoot,
            ["ATL_Interactables.spriteatlas"] = ArtAtlasesRoot,
            ["ATL_UI.spriteatlas"] = ArtAtlasesRoot,
            ["ATL_Effects.spriteatlas"] = ArtAtlasesRoot,

            // Scriptable objects
            ["SO_GameConfig.asset"] = ScriptableObjectsRoot,
            ["SO_PlayerConfig.asset"] = ScriptableObjectsRoot,
            ["SO_LevelConfig.asset"] = ScriptableObjectsRoot,
            ["SO_UIConfig.asset"] = ScriptableObjectsRoot,
            ["SO_AudioConfig.asset"] = ScriptableObjectsRoot,
            ["SO_InteractionConfig.asset"] = ScriptableObjectsRoot,
            ["SO_BeaconConfig.asset"] = ScriptableObjectsRoot,
            ["SO_HazardConfig.asset"] = ScriptableObjectsRoot,
        };

        [MenuItem("Tools/Echo of the Lantern/Project Scaffold")]
        public static void Open()
        {
            GetWindow<ProjectScaffoldWindow>("Project Scaffold");
        }

        private Vector2 _scroll;
        private bool _createFolders = true;
        private bool _createPlaceholders = true;
        private bool _reorganizeAssets = true;
        private bool _revealReport = true;

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Echo of the Lantern", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Project structure, placeholders, and asset reorganization.", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(8);
            _createFolders = EditorGUILayout.ToggleLeft("Create missing folder scaffold", _createFolders);
            _createPlaceholders = EditorGUILayout.ToggleLeft("Create placeholder fallback assets", _createPlaceholders);
            _reorganizeAssets = EditorGUILayout.ToggleLeft("Move misplaced assets into correct folders", _reorganizeAssets);
            _revealReport = EditorGUILayout.ToggleLeft("Show summary report", _revealReport);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Run Project Scaffold", GUILayout.Height(36)))
            {
                RunScaffold();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This script is the safety net for the whole project. It creates the folder layout, keeps imported assets in the right place, and generates placeholders so the game can function before final art/audio is ready.",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        private void RunScaffold()
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                if (_createFolders)
                {
                    CreateFolderScaffold();
                }

                if (_createPlaceholders)
                {
                    CreatePlaceholderAssetsIfMissing();
                }

                int movedCount = 0;
                if (_reorganizeAssets)
                {
                    movedCount = ReorganizeProjectAssets();
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (_revealReport)
                {
                    EditorUtility.DisplayDialog(
                        "Project Scaffold Complete",
                        $"Folder scaffold verified. Placeholder assets ensured. Misplaced assets moved: {movedCount}",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Project Scaffold Failed", ex.Message, "OK");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            Debug.Log("[Echo of the Lantern] Project scaffold run finished.");
        }

        private static void CreateFolderScaffold()
        {
            foreach (string folder in RequiredFolders)
            {
                EnsureFolder(folder);
            }
        }

        private static void CreatePlaceholderAssetsIfMissing()
        {
            EnsurePlaceholderTexture(Path.Combine(ArtSpritesRoot, "Environment"), "SPR_Ground_Stone_Base.png", 512, 512, new Color32(96, 104, 118, 255), new Color32(134, 144, 160, 255), "ground");
            EnsurePlaceholderTexture(Path.Combine(ArtSpritesRoot, "Player"), "SPR_Player_Idle.png", 512, 512, new Color32(86, 92, 102, 255), new Color32(214, 182, 110, 255), "player");
            EnsurePlaceholderTexture(Path.Combine(ArtSpritesRoot, "Interactables"), "SPR_Beacon_Off.png", 512, 512, new Color32(100, 100, 110, 255), new Color32(220, 170, 85, 255), "beacon");
            EnsurePlaceholderTexture(Path.Combine(ArtSpritesRoot, "UI"), "SPR_UI_HUD.png", 1024, 256, new Color32(40, 46, 58, 255), new Color32(120, 130, 150, 255), "ui");
            EnsurePlaceholderTexture(Path.Combine(ArtSpritesRoot, "Effects"), "FX_LanternGlow.png", 512, 512, new Color32(0, 0, 0, 0), new Color32(255, 192, 96, 200), "glow");
            EnsurePlaceholderTexture(Path.Combine(ArtSpritesRoot, "Icons"), "ICO_Lantern.png", 128, 128, new Color32(54, 60, 70, 255), new Color32(255, 193, 96, 255), "icon");

            EnsurePlaceholderWav(Path.Combine(AudioRoot, "Ambient"), "AMB_ShrineNight.wav", 1.0f, 2.0f);
            EnsurePlaceholderWav(Path.Combine(AudioRoot, "Gameplay"), "SFX_BeaconActivate.wav", 0.25f, 2.0f);
            EnsurePlaceholderWav(Path.Combine(AudioRoot, "UI"), "UI_Click.wav", 0.1f, 2.0f);
            EnsurePlaceholderWav(Path.Combine(AudioRoot, "Effects"), "SFX_Win.wav", 0.3f, 2.0f);

            EnsurePlaceholderMaterial(Path.Combine(ArtMaterialsRoot, "MAT_Ground_Stone.mat"));
            EnsurePlaceholderMaterial(Path.Combine(ArtMaterialsRoot, "MAT_Player.mat"));
            EnsurePlaceholderMaterial(Path.Combine(ArtMaterialsRoot, "MAT_Beacon.mat"));
            EnsurePlaceholderMaterial(Path.Combine(ArtMaterialsRoot, "MAT_UI.mat"));
            EnsurePlaceholderMaterial(Path.Combine(ArtMaterialsRoot, "MAT_Glow.mat"));
        }

        private static int ReorganizeProjectAssets()
        {
            int moved = 0;
            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { GameRoot });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    continue;
                }

                string fileName = Path.GetFileName(assetPath);
                if (!CanonicalFileTargets.TryGetValue(fileName, out string targetFolder))
                {
                    continue;
                }

                string currentFolder = Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? string.Empty;
                if (string.Equals(currentFolder, targetFolder, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                EnsureFolder(targetFolder);
                string targetPath = Path.Combine(targetFolder, fileName).Replace('\\', '/');

                if (File.Exists(targetPath))
                {
                    continue;
                }

                string result = AssetDatabase.MoveAsset(assetPath, targetPath);
                if (string.IsNullOrEmpty(result))
                {
                    moved++;
                }
                else
                {
                    Debug.LogWarning($"Could not move {assetPath} to {targetPath}: {result}");
                }
            }

            return moved;
        }

        private static void EnsurePlaceholderTexture(string folderPath, string fileName, int width, int height, Color32 baseColor, Color32 accentColor, string label)
        {
            EnsureFolder(folderPath);
            string assetPath = Path.Combine(folderPath, fileName).Replace('\\', '/');
            if (File.Exists(assetPath))
            {
                return;
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float t = Mathf.InverseLerp(0f, height - 1, y);
                    Color32 c = Color32.Lerp(baseColor, accentColor, t);
                    if (label == "glow")
                    {
                        float dx = x - width * 0.5f;
                        float dy = y - height * 0.5f;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy) / (Mathf.Min(width, height) * 0.5f);
                        float alpha = Mathf.Clamp01(1f - dist);
                        c.a = (byte)(c.a * alpha);
                    }

                    texture.SetPixel(x, y, c);
                }
            }

            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            File.WriteAllBytes(assetPath, png);
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static void EnsurePlaceholderWav(string folderPath, string fileName, float seconds, float sampleRate)
        {
            EnsureFolder(folderPath);
            string assetPath = Path.Combine(folderPath, fileName).Replace('\\', '/');
            if (File.Exists(assetPath))
            {
                return;
            }

            int channels = 1;
            int bitsPerSample = 16;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(seconds * sampleRate));
            int byteRate = (int)(sampleRate * channels * bitsPerSample / 8);
            int blockAlign = channels * bitsPerSample / 8;
            int dataSize = sampleCount * blockAlign;
            int fileSize = 36 + dataSize;

            using FileStream stream = new FileStream(assetPath, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(fileSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write((int)sampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write((short)bitsPerSample);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            for (int i = 0; i < sampleCount; i++)
            {
                writer.Write((short)0);
            }
        }

        private static void EnsurePlaceholderMaterial(string materialPath)
        {
            EnsureFolder(Path.GetDirectoryName(materialPath)?.Replace('\\', '/') ?? ArtMaterialsRoot);
            if (File.Exists(materialPath))
            {
                return;
            }

            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Texture");
            Material material = new Material(shader)
            {
                name = Path.GetFileNameWithoutExtension(materialPath)
            };

            AssetDatabase.CreateAsset(material, materialPath);
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/') ?? string.Empty;
            string leaf = Path.GetFileName(folderPath);
            if (string.IsNullOrEmpty(parent) || parent == folderPath)
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
#endif
