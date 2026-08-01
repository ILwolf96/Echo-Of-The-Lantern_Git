#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace EchoOfTheLantern.EditorTools
{
    /// <summary>
    /// Foundation editor tool for the AI-only pipeline.
    /// 
    /// Planned script architecture after this file:
    /// 1) AssetPipelineBootstrapperWindow  <- current file
    /// 2) PrefabFactory / PrefabAssembler
    /// 3) SceneComposer / LevelBuilder
    /// 4) GameState / Objective / Lose / Win systems
    /// 5) Player / Camera / Interaction systems
    /// 6) UI / Audio / VFX systems
    /// 
    /// This first foundation layer prepares imported raw assets so later scripts can build the game
    /// without manual slicing, assigning, material creation, or prefab assembly.
    /// </summary>
    public sealed class AssetPipelineBootstrapperWindow : EditorWindow
    {
        private const string GameRoot = "Assets/_Game";
        private const string IncomingRoot = "Assets/_Game/Incoming";
        private const string ArtRoot = "Assets/_Game/Art";
        private const string SpriteRoot = "Assets/_Game/Art/Sprites";
        private const string MaterialsRoot = "Assets/_Game/Art/Materials";
        private const string AtlasesRoot = "Assets/_Game/Art/Atlases";
        private const string AudioRoot = "Assets/_Game/Audio";
        private const string PrefabsRoot = "Assets/_Game/Prefabs";
        private const string ScriptableObjectsRoot = "Assets/_Game/ScriptableObjects";
        private const string ScriptsRoot = "Assets/_Game/Scripts";
        private const string ScenesRoot = "Assets/_Game/Scenes";
        private const string SettingsRoot = "Assets/_Game/Settings";

        private static readonly string[] RequiredFolders =
        {
            GameRoot,
            IncomingRoot,
            ArtRoot,
            SpriteRoot,
            Path.Combine(SpriteRoot, "Environment"),
            Path.Combine(SpriteRoot, "Player"),
            Path.Combine(SpriteRoot, "Interactables"),
            Path.Combine(SpriteRoot, "UI"),
            Path.Combine(SpriteRoot, "Effects"),
            Path.Combine(SpriteRoot, "Icons"),
            MaterialsRoot,
            AtlasesRoot,
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
            ScriptableObjectsRoot,
            ScriptsRoot,
            Path.Combine(ScriptsRoot, "Editor"),
            ScenesRoot,
            SettingsRoot
        };

        private const string AtlasEnvironment = "Assets/_Game/Art/Atlases/ATL_Environment.spriteatlas";
        private const string AtlasInteractables = "Assets/_Game/Art/Atlases/ATL_Interactables.spriteatlas";
        private const string AtlasUI = "Assets/_Game/Art/Atlases/ATL_UI.spriteatlas";
        private const string AtlasEffects = "Assets/_Game/Art/Atlases/ATL_Effects.spriteatlas";

        [MenuItem("Tools/Echo of the Lantern/Asset Bootstrapper")]
        public static void Open()
        {
            GetWindow<AssetPipelineBootstrapperWindow>("Asset Bootstrapper");
        }

        private string _sourceFolder = IncomingRoot;
        private Vector2 _scroll;
        private bool _configureTextures = true;
        private bool _createMaterials = true;
        private bool _createAtlases = true;

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Echo of the Lantern", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Foundation asset pipeline for imported raw assets.", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Source Folder", EditorStyles.boldLabel);
            _sourceFolder = EditorGUILayout.TextField("Incoming assets path", _sourceFolder);

            EditorGUILayout.Space(6);
            _configureTextures = EditorGUILayout.ToggleLeft("Configure texture import settings", _configureTextures);
            _createMaterials = EditorGUILayout.ToggleLeft("Create/update shared materials", _createMaterials);
            _createAtlases = EditorGUILayout.ToggleLeft("Create/update sprite atlases", _createAtlases);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Create Folder Scaffold", GUILayout.Height(28)))
            {
                CreateFolderScaffold();
            }

            if (GUILayout.Button("Run Full Bootstrap", GUILayout.Height(34)))
            {
                RunBootstrap();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Place all raw AI-generated assets into the Incoming folder first. This tool then prepares textures, materials, and atlases so later scripts can build prefabs and scenes automatically.",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        private void RunBootstrap()
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                CreateFolderScaffold();

                if (_configureTextures)
                {
                    ConfigureImportedTextures();
                }

                if (_createMaterials)
                {
                    CreateOrUpdateMaterials();
                }

                if (_createAtlases)
                {
                    CreateOrUpdateAtlases();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Asset Bootstrap Failed", ex.Message, "OK");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log("[Echo of the Lantern] Asset bootstrap completed.");
            EditorUtility.DisplayDialog("Asset Bootstrap Complete", "Imported assets have been prepared successfully.", "OK");
        }

        private static void CreateFolderScaffold()
        {
            foreach (string folder in RequiredFolders)
            {
                EnsureFolder(folder);
            }

            AssetDatabase.Refresh();
        }

        private void ConfigureImportedTextures()
        {
            IEnumerable<string> texturePaths = FindAssetsByExtensions(_sourceFolder, ".png", ".jpg", ".jpeg", ".tga", ".psd");

            foreach (string assetPath in texturePaths)
            {
                if (TryGetTextureImporter(assetPath, out TextureImporter importer))
                {
                    ConfigureTextureImporter(importer);
                    importer.SaveAndReimport();
                }
            }
        }

        private static void ConfigureTextureImporter(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = false;

            // Handle spriteMeshType via TextureImporterSettings
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }

        private void CreateOrUpdateMaterials()
        {
            CreateSpriteMaterial("MAT_Ground_Stone.mat", "SPR_Ground_Stone_Base.png", new Color(0.88f, 0.90f, 0.96f, 1f));
            CreateSpriteMaterial("MAT_Wall_Stone.mat", "SPR_Wall_Stone.png", new Color(0.80f, 0.83f, 0.90f, 1f));
            CreateSpriteMaterial("MAT_Player.mat", "SPR_Player_Idle.png", Color.white);
            CreateSpriteMaterial("MAT_Beacon.mat", "SPR_Beacon_On.png", Color.white);
            CreateSpriteMaterial("MAT_Shrine.mat", "SPR_Shrine.png", Color.white);
            CreateSpriteMaterial("MAT_Shadow.mat", "SPR_Shadow_Hazard.png", new Color(0.60f, 0.55f, 0.75f, 0.95f));
            CreateSpriteMaterial("MAT_UI.mat", "SPR_UI_HUD.png", Color.white);
            CreateSpriteMaterial("MAT_Glow.mat", "FX_LanternGlow.png", Color.white);
            CreateSpriteMaterial("MAT_Mist.mat", "FX_Mist.png", new Color(1f, 1f, 1f, 0.75f));
        }

        private static void CreateSpriteMaterial(string materialFileName, string representativeTextureFileName, Color tint)
        {
            string materialPath = Path.Combine(MaterialsRoot, materialFileName).Replace('\\', '/');
            string texturePath = FindAssetPathByFileName(representativeTextureFileName);

            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                throw new InvalidOperationException("Could not locate a usable shader for sprite materials.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = Path.GetFileNameWithoutExtension(materialFileName)
                };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            if (!string.IsNullOrEmpty(texturePath))
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture != null)
                {
                    material.mainTexture = texture;
                }
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", tint);
            }

            EditorUtility.SetDirty(material);
        }

        private void CreateOrUpdateAtlases()
        {
            CreateAtlas(AtlasEnvironment, new[] { Path.Combine(SpriteRoot, "Environment") });
            CreateAtlas(AtlasInteractables, new[] { Path.Combine(SpriteRoot, "Interactables"), Path.Combine(SpriteRoot, "Icons") });
            CreateAtlas(AtlasUI, new[] { Path.Combine(SpriteRoot, "UI") });
            CreateAtlas(AtlasEffects, new[] { Path.Combine(SpriteRoot, "Effects") });
        }

        private static void CreateAtlas(string atlasPath, IEnumerable<string> sourceFolders)
        {
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, atlasPath);
            }

            // Gather packable sprites from the selected folders.
            List<UnityEngine.Object> packables = new List<UnityEngine.Object>();
            foreach (string folder in sourceFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite != null)
                    {
                        packables.Add(sprite);
                    }
                }
            }

            if (packables.Count == 0)
            {
                return;
            }

            SpriteAtlasExtensions.Add(atlas, packables.ToArray());
            EditorUtility.SetDirty(atlas);
        }

        private static IEnumerable<string> FindAssetsByExtensions(string rootFolder, params string[] extensions)
        {
            if (!AssetDatabase.IsValidFolder(rootFolder))
            {
                yield break;
            }

            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { rootFolder });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string extension = Path.GetExtension(assetPath).ToLowerInvariant();
                if (extensions.Any(ext => string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase)))
                {
                    yield return assetPath;
                }
            }
        }

        private static bool TryGetTextureImporter(string assetPath, out TextureImporter importer)
        {
            importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            return importer != null;
        }

        private static string FindAssetPathByFileName(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName));
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileName(assetPath), fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return assetPath;
                }
            }

            return string.Empty;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parentFolder = Path.GetDirectoryName(folderPath)?.Replace('\\', '/') ?? string.Empty;
            string folderName = Path.GetFileName(folderPath);

            if (string.IsNullOrEmpty(parentFolder) || parentFolder == folderPath)
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parentFolder))
            {
                EnsureFolder(parentFolder);
            }

            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }
}
#endif
