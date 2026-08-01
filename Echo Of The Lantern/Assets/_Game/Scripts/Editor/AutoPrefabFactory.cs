#if UNITY_EDITOR
using System;
using System.IO;
using EchoOfTheLantern.Runtime;
using EchoOfTheLantern.Runtime.Interactions;
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
            public readonly bool AddCollider;
            public readonly bool IsTrigger;
            public readonly bool AddRigidbody2D;
            public readonly int SortingOrder;
            public readonly Vector3 Scale;
            public readonly float ColliderScale;

            public Spec(string prefabPath, string spriteName, bool addCollider, bool isTrigger, bool addRigidbody2D, int sortingOrder, Vector3 scale, float colliderScale)
            {
                PrefabPath = prefabPath;
                SpriteName = spriteName;
                AddCollider = addCollider;
                IsTrigger = isTrigger;
                AddRigidbody2D = addRigidbody2D;
                SortingOrder = sortingOrder;
                Scale = scale;
                ColliderScale = colliderScale;
            }
        }

        private static readonly Spec[] Specs =
        {
            new($"{EnvironmentRoot}/PFB_GroundTile.prefab", "SPR_Ground_Stone_Base.png", false, false, false, -10, Vector3.one, 1f),
            new($"{EnvironmentRoot}/PFB_WallTile.prefab", "SPR_Wall_Stone.png", true, false, false, -5, Vector3.one, 1f),

            new($"{PlayerRoot}/PFB_Player.prefab", "SPR_Player_Idle.png", true, false, true, 0, Vector3.one * 0.72f, 0.78f),

            new($"{InteractablesRoot}/PFB_Beacon.prefab", "SPR_Beacon_Off.png", true, true, false, 3, Vector3.one * 1.08f, 0.95f),
            new($"{InteractablesRoot}/PFB_Shrine.prefab", "SPR_Shrine.png", true, true, false, 3, Vector3.one * 1.18f, 0.95f),
            new($"{InteractablesRoot}/PFB_Refill.prefab", "SPR_Refill.png", true, true, false, 3, Vector3.one * 0.9f, 0.9f),
            new($"{InteractablesRoot}/PFB_Gate.prefab", "SPR_Gate_Closed.png", true, true, false, 3, Vector3.one * 1.1f, 0.95f),
            new($"{InteractablesRoot}/PFB_ShadowHazard.prefab", "SPR_Shadow_Hazard.png", true, true, false, 3, Vector3.one, 1f),
            new($"{InteractablesRoot}/PFB_RitualFragment.prefab", "SPR_Fragment.png", true, true, false, 3, Vector3.one * 0.55f, 0.9f),

            new($"{UIRoot}/PFB_UI_HUD.prefab", "SPR_UI_HUD.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_MenuBackground.prefab", "SPR_UI_MenuBackground.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_Win.prefab", "SPR_UI_Win.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_Lose.prefab", "SPR_UI_Lose.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_Button.prefab", "SPR_Button.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_ButtonHover.prefab", "SPR_Button_Hover.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_IconLantern.prefab", "ICO_Lantern.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_IconBeacon.prefab", "ICO_Beacon.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_IconShrine.prefab", "ICO_Shrine.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_IconWarning.prefab", "ICO_Warning.png", false, false, false, 100, Vector3.one, 1f),
            new($"{UIRoot}/PFB_UI_IconRestart.prefab", "ICO_Restart.png", false, false, false, 100, Vector3.one, 1f),

            new($"{EffectsRoot}/PFB_FX_LanternGlow.prefab", "FX_LanternGlow.png", false, false, false, 0, Vector3.one, 1f),
            new($"{EffectsRoot}/PFB_FX_BeaconGlow.prefab", "FX_BeaconGlow.png", false, false, false, 0, Vector3.one * 1.15f, 1f),
            new($"{EffectsRoot}/PFB_FX_Dust.prefab", "FX_Dust.png", false, false, false, 0, Vector3.one, 1f),
            new($"{EffectsRoot}/PFB_FX_Spark.prefab", "FX_Spark.png", false, false, false, 0, Vector3.one, 1f),
            new($"{EffectsRoot}/PFB_FX_ShadowPulse.prefab", "FX_ShadowPulse.png", false, false, false, 0, Vector3.one, 1f),
            new($"{EffectsRoot}/PFB_FX_Mist.prefab", "FX_Mist.png", false, false, false, 0, Vector3.one * 2.2f, 1f),
        };

        internal static void QueueRun()
        {
            if (SessionState.GetBool(SessionRunning, false) || SessionState.GetBool(SessionQueued, false))
            {
                return;
            }

            SessionState.SetBool(SessionQueued, true);
            EditorApplication.delayCall -= ExecuteQueuedRun;
            EditorApplication.delayCall += ExecuteQueuedRun;
        }

        private static void ExecuteQueuedRun()
        {
            EditorApplication.delayCall -= ExecuteQueuedRun;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!SessionState.GetBool(SessionQueued, false))
            {
                return;
            }

            SessionState.SetBool(SessionQueued, false);
            RunFactory();
        }

        private static void RunFactory()
        {
            if (SessionState.GetBool(SessionRunning, false))
            {
                return;
            }

            SessionState.SetBool(SessionRunning, true);

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (Spec spec in Specs)
                {
                    CreateOrUpdatePrefab(spec);
                }

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
            Sprite sprite = LoadSprite(spec.SpriteName) ?? LoadPlaceholderSprite(spec.SpriteName);
            if (sprite == null)
            {
                Debug.LogWarning($"Missing sprite for prefab {spec.PrefabPath}: {spec.SpriteName}");
                return;
            }

            GameObject temp = new GameObject(Path.GetFileNameWithoutExtension(spec.PrefabPath));
            temp.transform.localScale = spec.Scale;

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
                body.bodyType = RigidbodyType2D.Dynamic;
            }

            if (spec.AddCollider)
            {
                BoxCollider2D collider = temp.AddComponent<BoxCollider2D>();
                collider.size = sprite.bounds.size * spec.ColliderScale;
                collider.isTrigger = spec.IsTrigger;
            }

            if (spec.PrefabPath.EndsWith("PFB_Player.prefab", StringComparison.OrdinalIgnoreCase))
            {
                temp.AddComponent<PlayerController>();
                temp.AddComponent<PlayerInteractionController>();
            }
            else if (spec.PrefabPath.EndsWith("PFB_Beacon.prefab", StringComparison.OrdinalIgnoreCase))
            {
                BeaconInteractable beacon = temp.AddComponent<BeaconInteractable>();
                ApplyBeaconSprites(beacon, "SPR_Beacon_Off.png", "SPR_Beacon_On.png");
            }
            else if (spec.PrefabPath.EndsWith("PFB_Shrine.prefab", StringComparison.OrdinalIgnoreCase))
            {
                temp.AddComponent<ShrineInteractable>();
            }
            else if (spec.PrefabPath.EndsWith("PFB_ShadowHazard.prefab", StringComparison.OrdinalIgnoreCase))
            {
                temp.AddComponent<HazardZoneController>();
            }

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

        private static void ApplyBeaconSprites(BeaconInteractable beacon, string inactiveSpriteName, string activeSpriteName)
        {
            if (beacon == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(beacon);
            SerializedProperty rendererProp = so.FindProperty("_renderer");
            SerializedProperty inactiveProp = so.FindProperty("_inactiveSprite");
            SerializedProperty activeProp = so.FindProperty("_activeSprite");

            if (rendererProp != null)
            {
                rendererProp.objectReferenceValue = beacon.GetComponent<SpriteRenderer>();
            }

            if (inactiveProp != null)
            {
                inactiveProp.objectReferenceValue = LoadSprite(inactiveSpriteName) ?? LoadPlaceholderSprite(inactiveSpriteName);
            }

            if (activeProp != null)
            {
                activeProp.objectReferenceValue = LoadSprite(activeSpriteName) ?? LoadPlaceholderSprite(activeSpriteName);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
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
                    {
                        return sprite;
                    }
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
            {
                return string.Empty;
            }

            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName), new[] { folder });
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
    }
}
#endif