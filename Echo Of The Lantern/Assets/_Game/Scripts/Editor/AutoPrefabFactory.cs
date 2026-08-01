#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoOfTheLantern.EditorTools
{
    public static class AutoPrefabFactory
    {
        private const string PlayerRoot = "Assets/_Game/Prefabs/Player";
        private const string EnvironmentRoot = "Assets/_Game/Prefabs/Environment";
        private const string InteractablesRoot = "Assets/_Game/Prefabs/Interactables";
        private const string UIRoot = "Assets/_Game/Prefabs/UI";
        private const string EffectsRoot = "Assets/_Game/Prefabs/Effects";

        private const string SpritesEnv = "Assets/_Game/Art/Sprites/Environment";
        private const string SpritesPlayer = "Assets/_Game/Art/Sprites/Player";
        private const string SpritesInteractables = "Assets/_Game/Art/Sprites/Interactables";
        private const string SpritesUI = "Assets/_Game/Art/Sprites/UI";
        private const string SpritesEffects = "Assets/_Game/Art/Sprites/Effects";
        private const string SpritesIcons = "Assets/_Game/Art/Sprites/Icons";

        private const string PlaceholderRoot = "Assets/_Game/_Placeholders/Sprites";

        private const string SessionQueued = "EchoOfTheLantern.AutoPrefabFactory.Queued";
        private const string SessionRunning = "EchoOfTheLantern.AutoPrefabFactory.Running";

        private readonly struct Spec
        {
            public readonly string PrefabPath;
            public readonly string SpriteName;
            public readonly Vector2 ColliderSize;
            public readonly bool IsTrigger;
            public readonly bool AddRigidbody2D;
            public readonly int SortingOrder;

            public Spec(string prefabPath, string spriteName, Vector2 colliderSize, bool isTrigger, bool addRigidbody2D, int sortingOrder)
            {
                PrefabPath = prefabPath;
                SpriteName = spriteName;
                ColliderSize = colliderSize;
                IsTrigger = isTrigger;
                AddRigidbody2D = addRigidbody2D;
                SortingOrder = sortingOrder;
            }
        }

        private static readonly Spec[] Specs =
        {
            new($"{EnvironmentRoot}/PFB_GroundTile.prefab", "SPR_Ground_Stone_Base.png", new Vector2(1f, 1f), false, false, -10),
            new($"{EnvironmentRoot}/PFB_WallTile.prefab", "SPR_Wall_Stone.png", new Vector2(1f, 1f), false, false, -5),
            new($"{EnvironmentRoot}/PFB_Pillar.prefab", "SPR_Pillar.png", new Vector2(0.7f, 1.0f), false, false, -2),
            new($"{EnvironmentRoot}/PFB_Pillar_Broken.prefab", "SPR_Pillar_Broken.png", new Vector2(0.7f, 1.0f), false, false, -2),
            new($"{EnvironmentRoot}/PFB_Rubble_Small.prefab", "SPR_Rubble_Small.png", new Vector2(0.7f, 0.5f), false, false, -2),
            new($"{EnvironmentRoot}/PFB_Rubble_Large.prefab", "SPR_Rubble_Large.png", new Vector2(1.0f, 0.8f), false, false, -2),
            new($"{EnvironmentRoot}/PFB_Statue.prefab", "SPR_Statue.png", new Vector2(0.9f, 1.1f), false, false, -2),

            new($"{PlayerRoot}/PFB_Player.prefab", "SPR_Player_Idle.png", new Vector2(0.55f, 0.85f), false, true, 0),

            new($"{InteractablesRoot}/PFB_Beacon.prefab", "SPR_Beacon_Off.png", new Vector2(0.85f, 0.85f), true, false, 3),
            new($"{InteractablesRoot}/PFB_Shrine.prefab", "SPR_Shrine.png", new Vector2(1.2f, 1.2f), true, false, 3),
            new($"{InteractablesRoot}/PFB_Refill.prefab", "SPR_Refill.png", new Vector2(0.8f, 0.8f), true, false, 3),
            new($"{InteractablesRoot}/PFB_Gate.prefab", "SPR_Gate_Closed.png", new Vector2(1.2f, 1.2f), true, false, 3),
            new($"{InteractablesRoot}/PFB_ShadowHazard.prefab", "SPR_Shadow_Hazard.png", new Vector2(1.0f, 1.0f), true, false, 3),
            new($"{InteractablesRoot}/PFB_RitualFragment.prefab", "SPR_Fragment.png", new Vector2(0.4f, 0.4f), true, false, 3),

            new($"{UIRoot}/PFB_UI_HUD.prefab", "SPR_UI_HUD.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_MenuBackground.prefab", "SPR_UI_MenuBackground.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_Win.prefab", "SPR_UI_Win.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_Lose.prefab", "SPR_UI_Lose.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_Button.prefab", "SPR_Button.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_ButtonHover.prefab", "SPR_Button_Hover.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_IconLantern.prefab", "ICO_Lantern.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_IconBeacon.prefab", "ICO_Beacon.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_IconShrine.prefab", "ICO_Shrine.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_IconWarning.prefab", "ICO_Warning.png", new Vector2(1f, 1f), false, false, 100),
            new($"{UIRoot}/PFB_UI_IconRestart.prefab", "ICO_Restart.png", new Vector2(1f, 1f), false, false, 100),

            new($"{EffectsRoot}/PFB_FX_LanternGlow.prefab", "FX_LanternGlow.png", new Vector2(1f, 1f), false, false, 0),
            new($"{EffectsRoot}/PFB_FX_BeaconGlow.prefab", "FX_BeaconGlow.png", new Vector2(1f, 1f), false, false, 0),
            new($"{EffectsRoot}/PFB_FX_Dust.prefab", "FX_Dust.png", new Vector2(1f, 1f), false, false, 0),
            new($"{EffectsRoot}/PFB_FX_Spark.prefab", "FX_Spark.png", new Vector2(1f, 1f), false, false, 0),
            new($"{EffectsRoot}/PFB_FX_ShadowPulse.prefab", "FX_ShadowPulse.png", new Vector2(1f, 1f), false, false, 0),
            new($"{EffectsRoot}/PFB_FX_Mist.prefab", "FX_Mist.png", new Vector2(1f, 1f), false, false, 0),
        };

        internal static void QueueRun()
        {
            if (SessionState.GetBool(SessionRunning, false))
                return;

            if (SessionState.GetBool(SessionQueued, false))
                return;

            SessionState.SetBool(SessionQueued, true);
            EditorApplication.delayCall -= ExecuteQueuedRun;
            EditorApplication.delayCall += ExecuteQueuedRun;
        }

        private static void ExecuteQueuedRun()
        {
            EditorApplication.delayCall -= ExecuteQueuedRun;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (!SessionState.GetBool(SessionQueued, false))
                return;

            SessionState.SetBool(SessionQueued, false);
            RunFactory();
        }

        private static void RunFactory()
        {
            if (SessionState.GetBool(SessionRunning, false))
                return;

            SessionState.SetBool(SessionRunning, true);

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (Spec spec in Specs)
                    CreateOrUpdatePrefab(spec);

                Debug.Log("[Echo of the Lantern] Prefab factory complete.");
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
                SessionState.SetBool(SessionRunning, false);
            }

            AutoSceneComposer.QueueRun();
        }

        private static void CreateOrUpdatePrefab(Spec spec)
        {
            Sprite sprite = LoadSprite(spec.SpriteName);
            if (sprite == null)
            {
                Debug.LogWarning($"Missing sprite for prefab {spec.PrefabPath}: {spec.SpriteName}");
                sprite = LoadPlaceholderSprite(spec.SpriteName);
            }

            if (sprite == null)
                return;

            GameObject temp = new GameObject(Path.GetFileNameWithoutExtension(spec.PrefabPath));
            SpriteRenderer renderer = temp.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = spec.SortingOrder;

            if (spec.AddRigidbody2D)
            {
                Rigidbody2D body = temp.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.linearDamping = 8f;
                body.angularDamping = 10f;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            BoxCollider2D collider = temp.AddComponent<BoxCollider2D>();
            collider.size = spec.ColliderSize;
            collider.isTrigger = spec.IsTrigger;

            string folder = Path.GetDirectoryName(spec.PrefabPath)?.Replace("\\", "/") ?? string.Empty;
            if (!AssetDatabase.IsValidFolder(folder))
            {
                Debug.LogWarning($"Prefab folder missing: {folder}");
                UnityEngine.Object.DestroyImmediate(temp);
                return;
            }

            PrefabUtility.SaveAsPrefabAsset(temp, spec.PrefabPath);
            UnityEngine.Object.DestroyImmediate(temp);
        }

        private static Sprite LoadSprite(string fileName)
        {
            string[] roots = { SpritesEnv, SpritesPlayer, SpritesInteractables, SpritesUI, SpritesEffects, SpritesIcons };
            foreach (string root in roots)
            {
                string path = FindAssetPathByFileName(root, fileName);
                if (!string.IsNullOrEmpty(path))
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null)
                        return sprite;
                }
            }

            return null;
        }

        private static Sprite LoadPlaceholderSprite(string fileName)
        {
            string path = FindAssetPathByFileName(PlaceholderRoot, fileName);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static string FindAssetPathByFileName(string folder, string fileName)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                return string.Empty;

            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName), new[] { folder });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileName(assetPath), fileName, StringComparison.OrdinalIgnoreCase))
                    return assetPath;
            }

            return string.Empty;
        }
    }
}
#endif