#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoOfTheLantern.EditorTools
{
    [InitializeOnLoad]
    public static class AutoSpriteImportRules
    {
        private const string SessionReimported = "EchoOfTheLantern.SpriteReimportedOnce";

        static AutoSpriteImportRules()
        {
            EditorApplication.delayCall += ReimportExistingSpritesOnce;
        }

        internal static void ApplyRules(TextureImporter importer, string assetPath)
        {
            string normalizedPath = assetPath.Replace("\\", "/");

            if (!IsRelevantTexture(normalizedPath))
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            importer.spritePixelsPerUnit = ResolvePixelsPerUnit(normalizedPath);
        }

        private static float ResolvePixelsPerUnit(string assetPath)
        {
            string path = assetPath.ToLowerInvariant();
            string file = Path.GetFileNameWithoutExtension(path);

            if (path.Contains("/ui/") || path.Contains("/icons/"))
            {
                return 100f;
            }

            if (file.Contains("background") || file.Contains("menu") || file.Contains("win") || file.Contains("lose") || file.Contains("hud"))
            {
                return 100f;
            }

            if (file.Contains("shrine"))
            {
                return 1024f;
            }

            if (file.Contains("player"))
            {
                return 512f;
            }

            if (file.Contains("beacon"))
            {
                return 512f;
            }

            if (file.Contains("gate"))
            {
                return 512f;
            }

            if (file.Contains("refill"))
            {
                return 512f;
            }

            if (file.Contains("shadow") || file.Contains("mist") || file.Contains("glow") || file.Contains("spark") || file.Contains("dust"))
            {
                return 512f;
            }

            return 512f;
        }

        private static bool IsRelevantTexture(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            return extension is ".png" or ".jpg" or ".jpeg" or ".tga" or ".psd";
        }

        private static void ReimportExistingSpritesOnce()
        {
            if (SessionState.GetBool(SessionReimported, false))
            {
                return;
            }

            SessionState.SetBool(SessionReimported, true);

            string[] roots =
            {
                "Assets/_Game/Art/Sprites/Environment",
                "Assets/_Game/Art/Sprites/Player",
                "Assets/_Game/Art/Sprites/Interactables",
                "Assets/_Game/Art/Sprites/UI",
                "Assets/_Game/Art/Sprites/Effects",
                "Assets/_Game/Art/Sprites/Icons",
                "Assets/_Game/_Placeholders/Sprites",
                "Assets/_Game/Incoming"
            };

            foreach (string root in roots)
            {
                if (!AssetDatabase.IsValidFolder(root))
                {
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { root });
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!IsRelevantTexture(assetPath))
                    {
                        continue;
                    }

                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                }
            }

            AssetDatabase.Refresh();
        }
    }

    public sealed class AutoSpriteImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            AutoSpriteImportRules.ApplyRules((TextureImporter)assetImporter, assetPath);
        }
    }
}
#endif