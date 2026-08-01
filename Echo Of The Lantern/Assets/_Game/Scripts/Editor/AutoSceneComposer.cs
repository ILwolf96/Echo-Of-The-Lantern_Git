#if UNITY_EDITOR
using System;
using System.IO;
using EchoOfTheLantern.Runtime;
using EchoOfTheLantern.Runtime.Interactions;
using EchoOfTheLantern.Runtime.Services;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace EchoOfTheLantern.EditorTools
{
    /// <summary>
    /// Builds both canonical scenes automatically.
    /// Menu scene: title + play button.
    /// Game scene: world, player, objectives, hazards, UI, runtime bootstrap.
    /// </summary>
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
            BuildScenes();
        }


        private static void BuildScenes()
        {
            if (SessionState.GetBool(SessionRunning, false))
            {
                return;
            }


            SessionState.SetBool(SessionRunning, true);


            try
            {
                BuildMenuScene();
                BuildGameScene();
                AutoBuildSceneList.QueueRun();
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


            GameObject systemsRoot = new GameObject("GameSystems");
            SceneManager.MoveGameObjectToScene(systemsRoot, scene);
            systemsRoot.AddComponent<GameStateManager>();


            Canvas canvas = EnsureCanvas(scene, "Canvas_Menu");
            BuildMenuUI(canvas.transform);


            SpawnWorldSprite(scene, LoadPrefab("PFB_UI_MenuBackground.prefab"), Vector3.zero, Vector3.one * 10f, "MenuBackground", -100);


            EditorSceneManager.SaveScene(scene, MenuScenePath);
        }


        private static void BuildGameScene()
        {
            Scene scene = GetOrCreateScene(GameScenePath);
            ClearScene(scene);


            EnsureCamera(scene, false);
            EnsureEventSystem(scene);


            GameObject systemsRoot = new GameObject("GameSystems");
            SceneManager.MoveGameObjectToScene(systemsRoot, scene);


            systemsRoot.AddComponent<GameStateManager>();
            systemsRoot.AddComponent<ObjectiveManager>();
            systemsRoot.AddComponent<AssetRegistry>();
            systemsRoot.AddComponent<RuntimeGameBootstrapper>();


            Canvas canvas = EnsureCanvas(scene, "Canvas_Game");
            UIManager uiManager = systemsRoot.AddComponent<UIManager>();
            BuildGameUI(canvas.transform, uiManager);


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


        private static void BuildMenuUI(Transform canvasRoot)
        {
            CreateUIText(canvasRoot, "TitleText", new Vector2(0f, 160f), "Echo of the Lantern", TextAnchor.MiddleCenter, 52);
            GameObject playButton = CreateButton(canvasRoot, "PlayButton", "Play", new Vector2(0f, -20f));
            Button button = playButton.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.BeginGameplayFromMenu();
                }
                else
                {
                    EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
                }
            });
        }


        private static void BuildGameUI(Transform canvasRoot, UIManager uiManager)
        {
            GameObject objectiveTextObj = CreateUIText(canvasRoot, "ObjectiveText", new Vector2(20f, -20f), "Beacons: 0/3", TextAnchor.UpperLeft, 24);
            RectTransform objectiveRect = objectiveTextObj.GetComponent<RectTransform>();
            objectiveRect.anchorMin = new Vector2(0f, 1f);
            objectiveRect.anchorMax = new Vector2(0f, 1f);
            objectiveRect.pivot = new Vector2(0f, 1f);
            objectiveRect.anchoredPosition = new Vector2(20f, -20f);
            objectiveRect.sizeDelta = new Vector2(420f, 50f);


            GameObject promptTextObj = CreateUIText(canvasRoot, "PromptText", new Vector2(0f, 40f), "", TextAnchor.MiddleCenter, 24);
            RectTransform promptRect = promptTextObj.GetComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.5f, 0f);
            promptRect.anchorMax = new Vector2(0.5f, 0f);
            promptRect.pivot = new Vector2(0.5f, 0f);
            promptRect.anchoredPosition = new Vector2(0f, 30f);
            promptRect.sizeDelta = new Vector2(700f, 50f);


            GameObject winPanel = CreateEndPanel(canvasRoot, "WinPanel", "YOU WIN");
            GameObject losePanel = CreateEndPanel(canvasRoot, "LosePanel", "YOU LOSE");
            winPanel.SetActive(false);
            losePanel.SetActive(false);


            uiManager.Bind(
                objectiveTextObj.GetComponent<Text>(),
                promptTextObj.GetComponent<Text>(),
                winPanel,
                losePanel);
        }


        private static GameObject CreateUIText(Transform parent, string name, Vector2 anchoredPosition, string text, TextAnchor anchor, int fontSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);


            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(600f, 80f);


            Text uiText = go.AddComponent<Text>();
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontSize = fontSize;
            uiText.alignment = anchor;
            uiText.color = Color.white;
            uiText.text = text;
            uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;


            return go;
        }


        private static GameObject CreateEndPanel(Transform parent, string name, string message)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);


            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;


            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.72f);


            GameObject textObj = CreateUIText(panel.transform, $"{name}_Text", new Vector2(0f, 80f), message, TextAnchor.MiddleCenter, 56);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, 80f);
            textRect.sizeDelta = new Vector2(700f, 100f);


            GameObject buttonObj = CreateButton(panel.transform, $"{name}_RestartButton", "Restart", new Vector2(0f, -40f));
            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.RestartGame();
                }
            });


            return panel;
        }


        private static GameObject CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);


            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(240f, 60f);


            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.92f);


            Button button = buttonObj.AddComponent<Button>();


            GameObject labelObj = CreateUIText(buttonObj.transform, $"{name}_Label", Vector2.zero, label, TextAnchor.MiddleCenter, 28);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;


            return buttonObj;
        }


        private static void BuildGroundGrid(Scene scene, Transform parent)
        {
            GameObject groundPrefab = LoadPrefab("PFB_GroundTile.prefab");
            if (groundPrefab == null)
            {
                return;
            }


            for (int y = -4; y <= 4; y++)
            {
                for (int x = -5; x <= 5; x++)
                {
                    Vector3 pos = new Vector3(x, y, 0f);
                    Spawn(scene, groundPrefab, pos, parent, $"Ground_{x}_{y}");
                }
            }
        }


        private static void BuildBoundaryWalls(Scene scene, Transform parent)
        {
            GameObject wallPrefab = LoadPrefab("PFB_WallTile.prefab");
            if (wallPrefab == null)
            {
                Debug.LogWarning("Wall prefab missing. Cannot build perimeter walls.");
                return;
            }


            int width = 11;
            int height = 9;
            int startX = -(width / 2);
            int startY = -(height / 2);


            for (int x = 0; x < width; x++)
            {
                Spawn(scene, wallPrefab, new Vector3(startX + x, startY + height, 0f), parent, $"Wall_Top_{x}");
                Spawn(scene, wallPrefab, new Vector3(startX + x, startY - 1, 0f), parent, $"Wall_Bottom_{x}");
            }


            for (int y = 0; y < height; y++)
            {
                Spawn(scene, wallPrefab, new Vector3(startX - 1, startY + y, 0f), parent, $"Wall_Left_{y}");
                Spawn(scene, wallPrefab, new Vector3(startX + width, startY + y, 0f), parent, $"Wall_Right_{y}");
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
            string fullPath = GetFullPath(scenePath);
            if (File.Exists(fullPath))
            {
                return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }


            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }


        private static void ClearScene(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
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
            {
                cameraObject.AddComponent<AudioListener>();
            }
        }


        private static Canvas EnsureCanvas(Scene scene, string name)
        {
            GameObject canvasObject = new GameObject(name);
            SceneManager.MoveGameObjectToScene(canvasObject, scene);


            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;


            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }


        private static void EnsureEventSystem(Scene scene)
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }


            GameObject eventSystem = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }


        private static void SpawnWorldSprite(Scene scene, GameObject prefab, Vector3 position, Vector3 scale, string name, int sortingOffset)
        {
            if (prefab == null)
            {
                return;
            }


            GameObject instance = Spawn(scene, prefab, position, null, name);
            if (instance == null)
            {
                return;
            }


            instance.transform.localScale = scale;
            SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder += sortingOffset;
            }
        }


        private static GameObject Spawn(Scene scene, GameObject prefab, Vector3 position, Transform parent, string name)
        {
            if (prefab == null)
            {
                return null;
            }


            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            if (instance == null)
            {
                return null;
            }


            instance.name = name;
            instance.transform.position = position;


            if (parent != null)
            {
                instance.transform.SetParent(parent, true);
            }


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
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }


            return null;
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


        private static string GetFullPath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }
    }
}
#endif
