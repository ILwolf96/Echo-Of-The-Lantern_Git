using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoOfTheLantern.Runtime
{
    public sealed class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public enum GameState
        {
            Boot,
            MainMenu,
            Playing,
            Paused,
            Won,
            Lost
        }

        [SerializeField] private string _gameSceneName = "EchoOfTheLantern_Game";
        [SerializeField] private string _menuSceneName = "EchoOfTheLantern_Menu";

        public GameState CurrentState { get; private set; } = GameState.Boot;

        public event Action<GameState> StateChanged;
        public event Action GameStarted;
        public event Action GamePaused;
        public event Action GameResumed;
        public event Action GameWon;
        public event Action GameLost;
        public event Action GameRestarted;

        private int _lastInitializedSceneHandle = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            InitializeCurrentScene();
        }

        public void InitializeCurrentScene()
        {
            InitializeForScene(SceneManager.GetActiveScene());
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InitializeForScene(scene);
        }

        private void InitializeForScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                return;
            }

            if (scene.handle == _lastInitializedSceneHandle)
            {
                return;
            }

            _lastInitializedSceneHandle = scene.handle;

            Time.timeScale = 1f;

            UIManager ui = UIManager.Resolve();
            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();

            if (scene.name == _menuSceneName)
            {
                SetState(GameState.MainMenu);

                if (ui != null)
                {
                    ui.TryAutoBindFromScene();
                    ui.HideEndPanels();
                }

                return;
            }

            if (scene.name == _gameSceneName)
            {
                if (objectiveManager != null)
                {
                    objectiveManager.ResetObjectives();
                }

                if (ui != null)
                {
                    ui.TryAutoBindFromScene();
                    ui.HideEndPanels();

                    if (objectiveManager != null)
                    {
                        ui.SetBeaconProgress(objectiveManager.ActivatedBeacons, 3);
                    }
                }

                SetState(GameState.Playing);
                GameStarted?.Invoke();
            }
        }

        public void StartGame()
        {
            Time.timeScale = 1f;
            SetState(GameState.Playing);
            GameStarted?.Invoke();
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }

            Time.timeScale = 0f;
            SetState(GameState.Paused);
            GamePaused?.Invoke();
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
            {
                return;
            }

            Time.timeScale = 1f;
            SetState(GameState.Playing);
            GameResumed?.Invoke();
        }

        public void WinGame()
        {
            if (CurrentState == GameState.Won)
            {
                return;
            }

            Time.timeScale = 0f;
            SetState(GameState.Won);

            UIManager ui = UIManager.Resolve();
            if (ui != null)
            {
                ui.ShowWinPanel();
            }

            GameWon?.Invoke();
        }

        public void LoseGame()
        {
            if (CurrentState == GameState.Lost)
            {
                return;
            }

            Time.timeScale = 0f;
            SetState(GameState.Lost);

            UIManager ui = UIManager.Resolve();
            if (ui != null)
            {
                ui.ShowLosePanel();
            }

            GameLost?.Invoke();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SetState(GameState.Boot);
            GameRestarted?.Invoke();

            Scene active = SceneManager.GetActiveScene();
            if (active.name == _gameSceneName)
            {
                SceneManager.LoadScene(_gameSceneName);
            }
            else if (active.name == _menuSceneName)
            {
                SceneManager.LoadScene(_menuSceneName);
            }
            else
            {
                SceneManager.LoadScene(active.buildIndex);
            }
        }

        public void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(_menuSceneName);
        }

        public void BeginGameplayFromMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_gameSceneName);
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            CurrentState = newState;
            StateChanged?.Invoke(CurrentState);
        }

        public bool IsPlaying => CurrentState == GameState.Playing;
        public bool IsPaused => CurrentState == GameState.Paused;
        public bool HasEnded => CurrentState == GameState.Won || CurrentState == GameState.Lost;
    }
}