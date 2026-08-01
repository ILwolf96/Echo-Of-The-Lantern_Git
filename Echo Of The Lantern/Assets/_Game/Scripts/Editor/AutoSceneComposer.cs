#if UNITY_EDITOR
using EchoOfTheLantern.Runtime;
using EchoOfTheLantern.Runtime.Services;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoOfTheLantern.EditorTools
{
    public static class AutoSceneComposer
    {
        private const string GameScenePath = "Assets/_Game/Scenes/EchoOfTheLantern_Game.unity";
        private const string MenuScenePath = "Assets/_Game/Scenes/EchoOfTheLantern_Menu.unity";

        private const string PrefabsEnvironmentRoot = "Assets/_Game/Prefabs/Environment";
        private const string PrefabsPlayerRoot = "Assets/_Game/Prefabs/Player";
        private const string PrefabsInteractablesRoot = "Assets/_Game/Prefabs/Interactables";
        private const string PrefabsUIRoot = "Assets/_Game/Prefabs/UI";
        private const string PrefabsEffectsRoot = "Assets/_Game/Prefabs/Effects";

        private const string SessionQueued = "EchoOfTheLantern.AutoSceneComposer.Queued";
        private const string SessionRunning = "EchoOfTheLantern.AutoSceneComposer.Running";

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
            BuildScenes();
        }

        private static void BuildScenes()
        {
            if (SessionState.GetBool(SessionRunning, false))
                return;

            SessionState.SetBool(SessionRunning, true);

            try
            {
                BuildMenuScene();
                BuildGameScene();
                Debug.Log("[Echo of the Lantern] Scene composer complete.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                SessionState.SetBool(SessionRunning, false);
            }
        }

        private static void BuildMenuScene()
        {
            Scene scene = GetOrCreateScene(MenuScenePath);
            ClearScene(scene);

            EnsureCamera(scene, true);
            EnsureEventSystem(scene);
            EnsureCanvas(scene, "Canvas_Menu");

            SpawnWorldSprite(scene, LoadPrefab("PFB_UI_MenuBackground.prefab"), Vector3.zero, Vector3.one * 10f, "MenuBackground", -100);
            SpawnWorldSprite(scene, LoadPrefab("PFB_UI_Win.prefab"), new Vector3(0f, 2.5f, 0f), Vector3.one * 1.2f, "TitlePlaceholder", 10);

            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }

        private static void BuildGameScene()
        {
            Scene scene = GetOrCreateScene(GameScenePath);
            ClearScene(scene);

            EnsureCamera(scene, false);
            EnsureEventSystem(scene);
            EnsureCanvas(scene, "Canvas_Game");
            EnsureGameSystems(scene);

            GameObject sceneRoot = new GameObject("SceneRoot");
            SceneManager.MoveGameObjectToScene(sceneRoot, scene);

            GameObject envRoot = new GameObject("Environment");
            envRoot.transform.SetParent(sceneRoot.transform, false);

            GameObject gameplayRoot = new GameObject("Gameplay");
            gameplayRoot.transform.SetParent(sceneRoot.transform, false);

            GameObject effectsRoot = new GameObject("Effects");
            effectsRoot.transform.SetParent(sceneRoot.transform, false);

            GameObject markersRoot = new GameObject("Markers");
            markersRoot.transform.SetParent(sceneRoot.transform, false);

            BuildGroundGrid(scene, envRoot.transform);
            BuildBoundaryWalls(scene, envRoot.transform);
            BuildProps(scene, envRoot.transform);

            Spawn(scene, LoadPrefab("PFB_Player.prefab"), new Vector3(-4.5f, -3.75f, 0f), gameplayRoot.transform, "Player");
            Spawn(scene, LoadPrefab("PFB_Beacon.prefab"), new Vector3(-3.5f, 0f, 0f), gameplayRoot.transform, "Beacon_A");
            Spawn(scene, LoadPrefab("PFB_Beacon.prefab"), new Vector3(0f, 1.75f, 0f), gameplayRoot.transform, "Beacon_B");
            Spawn(scene, LoadPrefab("PFB_Beacon.prefab"), new Vector3(3.5f, -0.5f, 0f), gameplayRoot.transform, "Beacon_C");
            Spawn(scene, LoadPrefab("PFB_Shrine.prefab"), new Vector3(0f, -3.25f, 0f), gameplayRoot.transform, "Shrine");
            Spawn(scene, LoadPrefab("PFB_Refill.prefab"), new Vector3(-0.25f, 3.0f, 0f), gameplayRoot.transform, "Refill");
            Spawn(scene, LoadPrefab("PFB_Gate.prefab"), new Vector3(5.25f, -3.25f, 0f), gameplayRoot.transform, "Gate");

            Spawn(scene, LoadPrefab("PFB_ShadowHazard.prefab"), new Vector3(-1.75f, -0.5f, 0f), gameplayRoot.transform, "Shadow_01");
            Spawn(scene, LoadPrefab("PFB_ShadowHazard.prefab"), new Vector3(1.75f, -1.0f, 0f), gameplayRoot.transform, "Shadow_02");
            Spawn(scene, LoadPrefab("PFB_ShadowHazard.prefab"), new Vector3(0.5f, 2.75f, 0f), gameplayRoot.transform, "Shadow_03");

            Spawn(scene, LoadPrefab("PFB_RitualFragment.prefab"), new Vector3(-2.2f, 1.9f, 0f), gameplayRoot.transform, "Fragment_A");
            Spawn(scene, LoadPrefab("PFB_RitualFragment.prefab"), new Vector3(2.1f, 2.1f, 0f), gameplayRoot.transform, "Fragment_B");

            Spawn(scene, LoadPrefab("PFB_FX_Mist.prefab"), Vector3.zero, effectsRoot.transform, "Mist");
            CreateMarker(markersRoot.transform, "Spawn_Player", new Vector3(-4.5f, -3.75f, 0f));
            CreateMarker(markersRoot.transform, "Spawn_Shrine", new Vector3(0f, -3.25f, 0f));

            EditorSceneManager.SaveScene(scene, GameScenePath);
        }

        private static void EnsureGameSystems(Scene scene)
        {
            GameObject systemsRoot = GameObject.Find("GameSystems");
            if (systemsRoot != null)
                return;

            systemsRoot = new GameObject("GameSystems");
            SceneManager.MoveGameObjectToScene(systemsRoot, scene);

            systemsRoot.AddComponent<GameStateManager>();
            systemsRoot.AddComponent<ObjectiveManager>();
            systemsRoot.AddComponent<AssetRegistry>();
        }

        private static void BuildGroundGrid(Scene scene, Transform parent)
        {
            GameObject groundPrefab = LoadPrefab("PFB_GroundTile.prefab");
            if (groundPrefab == null) return;

            for (int y = -4; y <= 4; y++)
                for (int x = -5; x <= 5; x++)
                {
                    Vector3 pos = new Vector3(x, y, 0f);
                    Spawn(scene, groundPrefab, pos, parent, $"Ground_{x}_{y}");
                }
        }

        private static void BuildBoundaryWalls(Scene scene, Transform parent)
        {
            GameObject wallPrefab = LoadPrefab("PFB_WallTile.prefab");
            if (wallPrefab == null) return;

            for (int x = -6; x <= 6; x++)
            {
                Spawn(scene, wallPrefab, new Vector3(x, 5.5f, 0f), parent, $"Wall_Top_{x}");
                Spawn(scene, wallPrefab, new Vector3(x, -5.5f, 0f), parent, $"Wall_Bottom_{x}");
            }

            for (int y = -5; y <= 5; y++)
            {
                Spawn(scene, wallPrefab, new Vector3(-6.5f, y, 0f), parent, $"Wall_Left_{y}");
                Spawn(scene, wallPrefab, new Vector3(6.5f, y, 0f), parent, $"Wall_Right_{y}");
            }
        }

        private static void BuildProps(Scene scene, Transform parent)
        {
            Spawn(scene, LoadPrefab("PFB_Pillar.prefab"), new Vector3(-3.5f, 2.5f, 0f), parent, "Pillar_A");
            Spawn(scene, LoadPrefab("PFB_Pillar_Broken.prefab"), new Vector3(3.25f, 1.75f, 0f), parent, "Pillar_B");
            Spawn(scene, LoadPrefab("PFB_Rubble_Large.prefab"), new Vector3(-2.75f, -1.5f, 0f), parent, "Rubble_Large");
            Spawn(scene, LoadPrefab("PFB_Rubble_Small.prefab"), new Vector3(2.5f, -2.2f, 0f), parent, "Rubble_Small");
            Spawn(scene, LoadPrefab("PFB_Statue.prefab"), new Vector3(0f, 3.4f, 0f), parent, "Statue");
        }

        private static Scene GetOrCreateScene(string scenePath)
        {
            if (File.Exists(GetFullPath(scenePath)))
                return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void ClearScene(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                UnityEngine.Object.DestroyImmediate(root);
        }

        private static void EnsureCamera(Scene scene, bool isMenu)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = isMenu ? 5.5f : 6.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.05f, 0.08f, 1f);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            if (cameraObject.GetComponent<AudioListener>() == null)
                cameraObject.AddComponent<AudioListener>();
        }

        private static void EnsureCanvas(Scene scene, string name)
        {
            GameObject canvasObject = new GameObject(name);
            SceneManager.MoveGameObjectToScene(canvasObject, scene);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        private static void EnsureEventSystem(Scene scene)
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystem = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void SpawnWorldSprite(Scene scene, GameObject prefab, Vector3 position, Vector3 scale, string name, int sortingOffset)
        {
            if (prefab == null) return;

            GameObject instance = Spawn(scene, prefab, position, null, name);
            if (instance == null) return;

            instance.transform.localScale = scale;
            SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.sortingOrder += sortingOffset;
        }

        private static GameObject Spawn(Scene scene, GameObject prefab, Vector3 position, Transform parent, string name)
        {
            if (prefab == null) return null;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            if (instance == null) return null;

            instance.name = name;
            instance.transform.position = position;

            if (parent != null)
                instance.transform.SetParent(parent, true);

            return instance;
        }

        private static void CreateMarker(Transform parent, string name, Vector3 localPosition)
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = localPosition;
        }

        private static GameObject LoadPrefab(string fileName)
        {
            string[] roots =
            {
                PrefabsEnvironmentRoot,
                PrefabsPlayerRoot,
                PrefabsInteractablesRoot,
                PrefabsUIRoot,
                PrefabsEffectsRoot
            };

            foreach (string root in roots)
            {
                string path = FindAssetPathByFileName(root, fileName);
                if (!string.IsNullOrEmpty(path))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            return null;
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

        private static string GetFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }
    }
}
#endif