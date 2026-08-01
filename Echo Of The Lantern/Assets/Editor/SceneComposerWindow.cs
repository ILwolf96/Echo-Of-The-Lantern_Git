#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoOfTheLantern.EditorTools
{
    /// <summary>
    /// Creates a complete starting scene from prefab assets without manual placement.
    ///
    /// This script is intentionally conservative:
    /// - It assumes the Prefab Factory has already generated the prefabs.
    /// - It generates a playable top-down 2D layout.
    /// - It creates the most important scene objects automatically.
    /// - It can be rerun to rebuild the scene from scratch.
    ///
    /// The design goal is to keep the project AI-only and avoid manual GameObject creation,
    /// manual component assignment, or manual Inspector setup.
    /// </summary>
    public sealed class SceneComposerWindow : EditorWindow
    {
        private const string ScenesRoot = "Assets/_Game/Scenes";
        private const string GameScenePath = "Assets/_Game/Scenes/EchoOfTheLantern_Game.unity";
        private const string MenuScenePath = "Assets/_Game/Scenes/EchoOfTheLantern_Menu.unity";

        private const string PrefabsEnvironmentRoot = "Assets/_Game/Prefabs/Environment";
        private const string PrefabsInteractablesRoot = "Assets/_Game/Prefabs/Interactables";
        private const string PrefabsPlayerRoot = "Assets/_Game/Prefabs/Player";
        private const string PrefabsUIRoot = "Assets/_Game/Prefabs/UI";
        private const string PrefabsEffectsRoot = "Assets/_Game/Prefabs/Effects";

        private Vector2 _scroll;
        private bool _createMenuScene = true;
        private bool _createGameScene = true;
        private bool _rebuildFromScratch = true;

        [MenuItem("Tools/Echo of the Lantern/Scene Composer")]
        public static void Open()
        {
            GetWindow<SceneComposerWindow>("Scene Composer");
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Echo of the Lantern", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Build scenes automatically from generated prefabs.", EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space(8);
            _createMenuScene = EditorGUILayout.ToggleLeft("Create menu scene", _createMenuScene);
            _createGameScene = EditorGUILayout.ToggleLeft("Create game scene", _createGameScene);
            _rebuildFromScratch = EditorGUILayout.ToggleLeft("Rebuild scenes from scratch", _rebuildFromScratch);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Run Scene Composer", GUILayout.Height(34)))
            {
                RunComposer();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "This tool builds the playable scene layout using prefab assets and standard scene objects. It also creates a menu scene scaffold if enabled.",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        private void RunComposer()
        {
            try
            {
                EnsureSceneFolders();

                if (_createMenuScene)
                {
                    CreateMenuScene();
                }

                if (_createGameScene)
                {
                    CreateGameScene();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Scene Composer Failed", ex.Message, "OK");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Echo of the Lantern] Scene composition completed.");
            EditorUtility.DisplayDialog("Scene Composer Complete", "Scenes have been generated successfully.", "OK");
        }

        private static void EnsureSceneFolders()
        {
            EnsureFolder("Assets/_Game");
            EnsureFolder(ScenesRoot);
        }

        private void CreateMenuScene()
        {
            Scene scene = GetOrCreateScene(MenuScenePath, _rebuildFromScratch);
            if (!scene.IsValid())
            {
                return;
            }

            if (_rebuildFromScratch)
            {
                ClearScene(scene);
            }

            SceneManager.SetActiveScene(scene);

            EnsureCamera(scene, true);
            EnsureEventSystem(scene);
            EnsureCanvas(scene, "Canvas_Menu", RenderMode.ScreenSpaceOverlay);
            EnsureMenuBackground(scene);
            EnsureMenuTitle(scene);
            EnsureMenuButtons(scene);
            EnsureAmbientLight(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        private void CreateGameScene()
        {
            Scene scene = GetOrCreateScene(GameScenePath, _rebuildFromScratch);
            if (!scene.IsValid())
            {
                return;
            }

            if (_rebuildFromScratch)
            {
                ClearScene(scene);
            }

            SceneManager.SetActiveScene(scene);

            EnsureCamera(scene, false);
            EnsureAmbientLight(scene);
            EnsureCanvas(scene, "Canvas_Game", RenderMode.ScreenSpaceOverlay);
            EnsureEventSystem(scene);

            SpawnEnvironment(scene);
            SpawnPlayer(scene);
            SpawnInteractables(scene);
            SpawnEffects(scene);
            CreateSceneMarkers(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScenePath);
        }

        private static Scene GetOrCreateScene(string scenePath, bool createFromScratch)
        {
            Scene scene;

            if (File.Exists(scenePath) && !createFromScratch)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                SceneAsset asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (asset == null)
                {
                    EnsureFolder(Path.GetDirectoryName(scenePath)?.Replace('\\', '/') ?? ScenesRoot);
                }
            }

            return scene;
        }

        private static void ClearScene(Scene scene)
        {
            List<GameObject> roots = new List<GameObject>(scene.GetRootGameObjects());
            foreach (GameObject root in roots)
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void EnsureCamera(Scene scene, bool isMenu)
        {
            Camera camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            camera.orthographic = true;
            camera.orthographicSize = isMenu ? 5.5f : 6.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.05f, 0.08f, 1f);
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.rotation = Quaternion.identity;

            if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }
        }

        private static void EnsureAmbientLight(Scene scene)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.20f, 0.26f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.05f, 0.07f, 0.10f, 1f);
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.025f;
        }

        private static void EnsureEventSystem(Scene scene)
        {
            if (UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static void EnsureCanvas(Scene scene, string canvasName, RenderMode renderMode)
        {
            GameObject existing = GameObject.Find(canvasName);
            if (existing != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject(canvasName);
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = renderMode;
            canvas.sortingOrder = 100;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        private static void EnsureMenuBackground(Scene scene)
        {
            GameObject go = new GameObject("MenuBackground");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.position = Vector3.zero;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite("SPR_UI_MenuBackground.png");
            renderer.sortingOrder = -100;
            go.transform.localScale = new Vector3(10f, 10f, 1f);
        }

        private static void EnsureMenuTitle(Scene scene)
        {
            GameObject title = new GameObject("MenuTitle");
            SceneManager.MoveGameObjectToScene(title, scene);
            title.transform.position = new Vector3(0f, 2.75f, 0f);
            var sr = title.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("SPR_UI_Win.png");
            sr.color = new Color(1f, 1f, 1f, 0.0f);
            sr.sortingOrder = 10;
        }

        private static void EnsureMenuButtons(Scene scene)
        {
            CreateUIPlaceholder(scene, "Button_Play", new Vector3(0f, 0.5f, 0f), "SPR_Button.png");
            CreateUIPlaceholder(scene, "Button_Quit", new Vector3(0f, -0.2f, 0f), "SPR_Button.png");
        }

        private static void CreateUIPlaceholder(Scene scene, string objectName, Vector3 position, string spriteName)
        {
            GameObject go = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.position = position;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spriteName);
            sr.sortingOrder = 5;
        }

        private static void SpawnEnvironment(Scene scene)
        {
            CreateTiledGround(scene);
            CreateBoundaryWalls(scene);
            CreateEnvironmentalProps(scene);
        }

        private static void CreateTiledGround(Scene scene)
        {
            string[] tiles =
            {
                "SPR_Ground_Stone_Base.png",
                "SPR_Ground_Stone_Var01.png",
                "SPR_Ground_Stone_Var02.png",
                "SPR_Ground_Stone_Var03.png"
            };

            int index = 0;
            for (int y = -4; y <= 4; y++)
            {
                for (int x = -5; x <= 5; x++)
                {
                    GameObject tile = new GameObject($"Ground_{x}_{y}");
                    SceneManager.MoveGameObjectToScene(tile, scene);
                    tile.transform.position = new Vector3(x, y, 0f);
                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = LoadSprite(tiles[index % tiles.Length]);
                    sr.sortingOrder = -10;
                    index++;
                }
            }
        }

        private static void CreateBoundaryWalls(Scene scene)
        {
            for (int x = -6; x <= 6; x++)
            {
                CreateWallPiece(scene, new Vector3(x, 5.5f, 0f), "SPR_Wall_Stone.png");
                CreateWallPiece(scene, new Vector3(x, -5.5f, 0f), "SPR_Wall_Stone.png");
            }

            for (int y = -5; y <= 5; y++)
            {
                CreateWallPiece(scene, new Vector3(-6.5f, y, 0f), "SPR_Wall_Stone.png");
                CreateWallPiece(scene, new Vector3(6.5f, y, 0f), "SPR_Wall_Stone.png");
            }
        }

        private static void CreateWallPiece(Scene scene, Vector3 position, string spriteName)
        {
            GameObject wall = new GameObject("Wall");
            SceneManager.MoveGameObjectToScene(wall, scene);
            wall.transform.position = position;
            SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spriteName);
            sr.sortingOrder = -5;
        }

        private static void CreateEnvironmentalProps(Scene scene)
        {
            SpawnPrefabIfExists(scene, "PFB_Pillar.prefab", new Vector3(-3.5f, 2.5f, 0f));
            SpawnPrefabIfExists(scene, "PFB_Pillar_Broken.prefab", new Vector3(3.25f, 1.75f, 0f));
            SpawnPrefabIfExists(scene, "PFB_Rubble_Large.prefab", new Vector3(-2.75f, -1.5f, 0f));
            SpawnPrefabIfExists(scene, "PFB_Rubble_Small.prefab", new Vector3(2.5f, -2.2f, 0f));
            SpawnPrefabIfExists(scene, "PFB_Statue.prefab", new Vector3(0f, 3.4f, 0f));
        }

        private static void SpawnPlayer(Scene scene)
        {
            SpawnPrefabIfExists(scene, "PFB_Player.prefab", new Vector3(-4.5f, -3.75f, 0f));
        }

        private static void SpawnInteractables(Scene scene)
        {
            SpawnPrefabIfExists(scene, "PFB_Beacon.prefab", new Vector3(-3.5f, 0f, 0f));
            SpawnPrefabIfExists(scene, "PFB_Beacon.prefab", new Vector3(0f, 1.75f, 0f));
            SpawnPrefabIfExists(scene, "PFB_Beacon.prefab", new Vector3(3.5f, -0.5f, 0f));

            SpawnPrefabIfExists(scene, "PFB_Shrine.prefab", new Vector3(0f, -3.25f, 0f));
            SpawnPrefabIfExists(scene, "PFB_Refill.prefab", new Vector3(-0.25f, 3.0f, 0f));
            SpawnPrefabIfExists(scene, "PFB_Gate.prefab", new Vector3(5.25f, -3.25f, 0f));

            SpawnPrefabIfExists(scene, "PFB_ShadowHazard.prefab", new Vector3(-1.75f, -0.5f, 0f));
            SpawnPrefabIfExists(scene, "PFB_ShadowHazard.prefab", new Vector3(1.75f, -1.0f, 0f));
            SpawnPrefabIfExists(scene, "PFB_ShadowHazard.prefab", new Vector3(0.5f, 2.75f, 0f));

            SpawnPrefabIfExists(scene, "PFB_RitualFragment.prefab", new Vector3(-2.2f, 1.9f, 0f));
            SpawnPrefabIfExists(scene, "PFB_RitualFragment.prefab", new Vector3(2.1f, 2.1f, 0f));
        }

        private static void SpawnEffects(Scene scene)
        {
            SpawnPrefabIfExists(scene, "PFB_FX_Mist.prefab", new Vector3(0f, 0f, 0f));
        }

        private static void CreateSceneMarkers(Scene scene)
        {
            GameObject root = new GameObject("SceneMarkers");
            SceneManager.MoveGameObjectToScene(root, scene);

            CreateMarker(root.transform, "SpawnPoint_Player", new Vector3(-4.5f, -3.75f, 0f));
            CreateMarker(root.transform, "SpawnPoint_Beacon_01", new Vector3(-3.5f, 0f, 0f));
            CreateMarker(root.transform, "SpawnPoint_Beacon_02", new Vector3(0f, 1.75f, 0f));
            CreateMarker(root.transform, "SpawnPoint_Beacon_03", new Vector3(3.5f, -0.5f, 0f));
            CreateMarker(root.transform, "SpawnPoint_Shrine", new Vector3(0f, -3.25f, 0f));
        }

        private static void CreateMarker(Transform parent, string name, Vector3 position)
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = position;
        }

        private static void SpawnPrefabIfExists(Scene scene, string prefabFileName, Vector3 position)
        {
            string prefabPath = FindPrefabPath(prefabFileName);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogWarning($"Prefab not found: {prefabFileName}. The scene will still be created.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.position = position;
        }

        private static string FindPrefabPath(string prefabFileName)
        {
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(prefabFileName), new[]
            {
                PrefabsPlayerRoot,
                PrefabsEnvironmentRoot,
                PrefabsInteractablesRoot,
                PrefabsUIRoot,
                PrefabsEffectsRoot
            });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileName(path), prefabFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private static Sprite LoadSprite(string spriteFileName)
        {
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(spriteFileName), new[]
            {
                "Assets/_Game/Art/Sprites/Environment",
                "Assets/_Game/Art/Sprites/Player",
                "Assets/_Game/Art/Sprites/Interactables",
                "Assets/_Game/Art/Sprites/UI",
                "Assets/_Game/Art/Sprites/Effects",
                "Assets/_Game/Art/Sprites/Icons"
            });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(Path.GetFileName(path), spriteFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
            }

            return null;
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
