using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Central runtime authority for the game's lifecycle.
    /// 
    /// Responsibilities:
    /// - Track the current game state.
    /// - Expose events for UI, player, objective, and audio systems.
    /// - Handle pause, resume, win, lose, and restart.
    /// - Keep the code dynamic and decoupled through events rather than hard references.
    /// </summary>
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

        [SerializeField] private GameState _initialState = GameState.MainMenu;
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

        private void Start()
        {
            SetState(_initialState);
        }

        public void StartGame()
        {
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
            SetState(GameState.Playing);
            SceneManager.LoadScene(_gameSceneName);
            GameStarted?.Invoke();
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
