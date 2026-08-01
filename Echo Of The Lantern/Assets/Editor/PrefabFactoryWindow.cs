#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace EchoOfTheLantern.EditorTools
{
    /// <summary>
    /// Creates prefabs from imported sprite assets using naming conventions.
    ///
    /// Expected pipeline:
    /// 1) Import raw AI-generated assets into Assets/_Game/Incoming or the prepared sprite folders.
    /// 2) Run the Asset Pipeline Bootstrapper to configure textures, materials, and atlases.
    /// 3) Run this Prefab Factory to generate usable prefabs with standard components.
    /// 4) Later scripts will compose scenes and connect gameplay systems automatically.
    ///
    /// This script intentionally uses only editor-time automation so the user does not need to
    /// manually create GameObjects, prefabs, components, or Inspector assignments.
    /// </summary>
    public sealed class PrefabFactoryWindow : EditorWindow
    {
        private const string PrefabsRoot = "Assets/_Game/Prefabs";
        private const string PlayerPrefabsRoot = "Assets/_Game/Prefabs/Player";
        private const string EnvironmentPrefabsRoot = "Assets/_Game/Prefabs/Environment";
        private const string InteractablePrefabsRoot = "Assets/_Game/Prefabs/Interactables";
        private const string UIPrefabsRoot = "Assets/_Game/Prefabs/UI";
        private const string EffectPrefabsRoot = "Assets/_Game/Prefabs/Effects";

        private const string SpritesEnvironmentRoot = "Assets/_Game/Art/Sprites/Environment";
        private const string SpritesPlayerRoot = "Assets/_Game/Art/Sprites/Player";
        private const string SpritesInteractablesRoot = "Assets/_Game/Art/Sprites/Interactables";
        private const string SpritesUIRoot = "Assets/_Game/Art/Sprites/UI";
        private const string SpritesEffectsRoot = "Assets/_Game/Art/Sprites/Effects";

        private Vector2 _scroll;
        private bool _createPlayer = true;
        private bool _createEnvironment = true;
        private bool _createInteractables = true;
        private bool _createUI = true;
        private bool _createEffects = true;

        [MenuItem("Tools/Echo of the Lantern/Prefab Factory")]
        public static void Open()
        {
            GetWindow<PrefabFactoryWindow>("Prefab Factory");
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Echo of the Lantern", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Generate prefabs from the prepared sprite library.", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(8);
            _createPlayer = EditorGUILayout.ToggleLeft("Create player prefabs", _createPlayer);
            _createEnvironment = EditorGUILayout.ToggleLeft("Create environment prefabs", _createEnvironment);
            _createInteractables = EditorGUILayout.ToggleLeft("Create interactable prefabs", _createInteractables);
            _createUI = EditorGUILayout.ToggleLeft("Create UI prefabs", _createUI);
            _createEffects = EditorGUILayout.ToggleLeft("Create effect prefabs", _createEffects);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Run Prefab Factory", GUILayout.Height(34)))
            {
                RunFactory();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Prefabs are created from sprite file names. The script expects the asset pipeline to have already imported and organized the source sprites.",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        private void RunFactory()
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                EnsurePrefabFolders();

                if (_createPlayer)
                {
                    CreatePlayerPrefab();
                }

                if (_createEnvironment)
                {
                    CreateEnvironmentPrefabs();
                }

                if (_createInteractables)
                {
                    CreateInteractablePrefabs();
                }

                if (_createUI)
                {
                    CreateUIPrefabs();
                }

                if (_createEffects)
                {
                    CreateEffectPrefabs();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Prefab Factory Failed", ex.Message, "OK");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log("[Echo of the Lantern] Prefab factory completed.");
            EditorUtility.DisplayDialog("Prefab Factory Complete", "Prefabs have been generated successfully.", "OK");
        }

        private static void EnsurePrefabFolders()
        {
            EnsureFolder(PrefabsRoot);
            EnsureFolder(PlayerPrefabsRoot);
            EnsureFolder(EnvironmentPrefabsRoot);
            EnsureFolder(InteractablePrefabsRoot);
            EnsureFolder(UIPrefabsRoot);
            EnsureFolder(EffectPrefabsRoot);
        }

        private void CreatePlayerPrefab()
        {
            string idleSpritePath = FindAssetPath("SPR_Player_Idle.png");
            string lanternSpritePath = FindAssetPath("SPR_Player_Lantern.png");

            GameObject root = CreateSpritePrefabBase("PFB_Player", idleSpritePath, PlayerPrefabsRoot);
            if (root == null)
            {
                return;
            }

            Configure2DRigidbody(root, freezeRotation: true);
            AddOrUpdateBoxCollider(root, new Vector2(0.55f, 0.85f), new Vector2(0f, -0.05f), isTrigger: false);

            if (!string.IsNullOrEmpty(lanternSpritePath))
            {
                GameObject lantern = CreateChildSprite(root.transform, "Lantern", lanternSpritePath, new Vector3(0.18f, 0.02f, -0.05f), 0.01f);
                SetRendererSorting(lantern, sortingOrder: 2);
            }

            SetRendererSorting(root, sortingOrder: 1);
            SavePrefab(root, Path.Combine(PlayerPrefabsRoot, "PFB_Player.prefab"));
            DestroyImmediate(root);
        }

        private void CreateEnvironmentPrefabs()
        {
            CreateSimpleSpritePrefab("PFB_Pillar", "SPR_Pillar.png", EnvironmentPrefabsRoot, new Vector2(0.55f, 1.0f), false);
            CreateSimpleSpritePrefab("PFB_Pillar_Broken", "SPR_Pillar_Broken.png", EnvironmentPrefabsRoot, new Vector2(0.55f, 1.0f), false);
            CreateSimpleSpritePrefab("PFB_Rubble_Small", "SPR_Rubble_Small.png", EnvironmentPrefabsRoot, new Vector2(0.7f, 0.45f), false);
            CreateSimpleSpritePrefab("PFB_Rubble_Large", "SPR_Rubble_Large.png", EnvironmentPrefabsRoot, new Vector2(1.0f, 0.7f), false);
            CreateSimpleSpritePrefab("PFB_Statue", "SPR_Statue.png", EnvironmentPrefabsRoot, new Vector2(0.8f, 1.1f), false);
        }

        private void CreateInteractablePrefabs()
        {
            CreateInteractablePrefab("PFB_Beacon", "SPR_Beacon_Off.png", InteractablePrefabsRoot, new Vector2(0.85f, 0.85f));
            CreateInteractablePrefab("PFB_Shrine", "SPR_Shrine.png", InteractablePrefabsRoot, new Vector2(1.4f, 1.4f));
            CreateInteractablePrefab("PFB_Refill", "SPR_Refill.png", InteractablePrefabsRoot, new Vector2(0.8f, 0.8f));
            CreateInteractablePrefab("PFB_Gate", "SPR_Gate_Closed.png", InteractablePrefabsRoot, new Vector2(1.4f, 1.4f));
            CreateInteractablePrefab("PFB_ShadowHazard", "SPR_Shadow_Hazard.png", InteractablePrefabsRoot, new Vector2(1.0f, 1.0f));
            CreateInteractablePrefab("PFB_RitualFragment", "SPR_Fragment.png", InteractablePrefabsRoot, new Vector2(0.4f, 0.4f));
        }

        private void CreateUIPrefabs()
        {
            CreateSimpleUIPrefab("PFB_UI_HUD", "SPR_UI_HUD.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_MenuBackground", "SPR_UI_MenuBackground.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_Win", "SPR_UI_Win.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_Lose", "SPR_UI_Lose.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_Button", "SPR_Button.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_ButtonHover", "SPR_Button_Hover.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_IconLantern", "ICO_Lantern.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_IconBeacon", "ICO_Beacon.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_IconShrine", "ICO_Shrine.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_IconWarning", "ICO_Warning.png", UIPrefabsRoot);
            CreateSimpleUIPrefab("PFB_UI_IconRestart", "ICO_Restart.png", UIPrefabsRoot);
        }

        private void CreateEffectPrefabs()
        {
            CreateSimpleSpritePrefab("PFB_FX_LanternGlow", "FX_LanternGlow.png", EffectPrefabsRoot, new Vector2(1.0f, 1.0f), true);
            CreateSimpleSpritePrefab("PFB_FX_BeaconGlow", "FX_BeaconGlow.png", EffectPrefabsRoot, new Vector2(1.2f, 1.2f), true);
            CreateSimpleSpritePrefab("PFB_FX_Dust", "FX_Dust.png", EffectPrefabsRoot, new Vector2(0.4f, 0.4f), true);
            CreateSimpleSpritePrefab("PFB_FX_Spark", "FX_Spark.png", EffectPrefabsRoot, new Vector2(0.4f, 0.4f), true);
            CreateSimpleSpritePrefab("PFB_FX_ShadowPulse", "FX_ShadowPulse.png", EffectPrefabsRoot, new Vector2(1.2f, 1.2f), true);
            CreateSimpleSpritePrefab("PFB_FX_Mist", "FX_Mist.png", EffectPrefabsRoot, new Vector2(2.0f, 2.0f), true);
        }

        private static void CreateSimpleUIPrefab(string prefabName, string spriteFileName, string folderPath)
        {
            GameObject root = CreateSpritePrefabBase(prefabName, FindAssetPath(spriteFileName), folderPath);
            if (root == null)
            {
                return;
            }

            SetRendererSorting(root, 100);
            SavePrefab(root, Path.Combine(folderPath, prefabName + ".prefab"));
            DestroyImmediate(root);
        }

        private static void CreateInteractablePrefab(string prefabName, string spriteFileName, string folderPath, Vector2 colliderSize)
        {
            GameObject root = CreateSpritePrefabBase(prefabName, FindAssetPath(spriteFileName), folderPath);
            if (root == null)
            {
                return;
            }

            AddOrUpdateBoxCollider(root, colliderSize, Vector2.zero, isTrigger: true);
            SetRendererSorting(root, 5);
            SavePrefab(root, Path.Combine(folderPath, prefabName + ".prefab"));
            DestroyImmediate(root);
        }

        private static void CreateSimpleSpritePrefab(string prefabName, string spriteFileName, string folderPath, Vector2 colliderSize, bool isTrigger)
        {
            GameObject root = CreateSpritePrefabBase(prefabName, FindAssetPath(spriteFileName), folderPath);
            if (root == null)
            {
                return;
            }

            AddOrUpdateBoxCollider(root, colliderSize, Vector2.zero, isTrigger);
            SetRendererSorting(root, 3);
            SavePrefab(root, Path.Combine(folderPath, prefabName + ".prefab"));
            DestroyImmediate(root);
        }

        private static GameObject CreateSpritePrefabBase(string objectName, string spritePath, string folderPath)
        {
            if (string.IsNullOrEmpty(spritePath))
            {
                Debug.LogWarning($"Sprite not found for prefab {objectName}. Skipping.");
                return null;
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                Debug.LogWarning($"Could not load sprite at {spritePath} for prefab {objectName}.");
                return null;
            }

            GameObject root = new GameObject(objectName);
            root.layer = LayerMask.NameToLayer("Default");

            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.sortingOrder = 0;

            return root;
        }

        private static GameObject CreateChildSprite(Transform parent, string objectName, string spritePath, Vector3 localPosition, float localScale)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                return null;
            }

            GameObject child = new GameObject(objectName);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localScale = Vector3.one * localScale;

            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 0;
            return child;
        }

        private static void Configure2DRigidbody(GameObject root, bool freezeRotation)
        {
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.linearDamping = 8f;
            body.angularDamping = 10f;
            body.bodyType = RigidbodyType2D.Dynamic;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.constraints = freezeRotation ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;
        }

        private static void AddOrUpdateBoxCollider(GameObject root, Vector2 size, Vector2 offset, bool isTrigger)
        {
            BoxCollider2D collider = root.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = root.AddComponent<BoxCollider2D>();
            }

            collider.size = size;
            collider.offset = offset;
            collider.isTrigger = isTrigger;
        }

        private static void SetRendererSorting(GameObject root, int sortingOrder)
        {
            SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }

            foreach (SpriteRenderer childRenderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                childRenderer.sortingOrder = sortingOrder;
            }
        }

        private static void SavePrefab(GameObject root, string prefabPath)
        {
            string folder = Path.GetDirectoryName(prefabPath)?.Replace('\\', '/') ?? string.Empty;
            EnsureFolder(folder);

            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            else
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }

            EditorUtility.SetDirty(root);
        }

        private static string FindAssetPath(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName));
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileName(path), fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return path;
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
