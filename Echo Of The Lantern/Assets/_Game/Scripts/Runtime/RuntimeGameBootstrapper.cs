using System.Collections;
using UnityEngine;

namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Rebinds the scene after load and starts the game one frame later so the new scene objects exist.
    /// This is important after restart and when entering the game scene directly.
    /// </summary>
    public sealed class RuntimeGameBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool _startGameOnAwake = true;
        [SerializeField] private bool _autoBindUiFromScene = true;
        [SerializeField] private bool _debugLogs = true;

        private Coroutine _bootstrapRoutine;

        private void Start()
        {
            if (_bootstrapRoutine != null)
            {
                StopCoroutine(_bootstrapRoutine);
            }

            _bootstrapRoutine = StartCoroutine(BootstrapNextFrame());
        }

        private IEnumerator BootstrapNextFrame()
        {
            yield return null;

            GameStateManager gameState = GameStateManager.Instance != null
                ? GameStateManager.Instance
                : FindFirstObjectByType<GameStateManager>(FindObjectsInactive.Include);

            if (gameState == null)
            {
                GameObject gs = new GameObject("GameStateManager");
                gameState = gs.AddComponent<GameStateManager>();
                DontDestroyOnLoad(gs);
            }

            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();
            UIManager ui = UIManager.Resolve();

            if (_autoBindUiFromScene && ui != null)
            {
                ui.TryAutoBindFromScene();
                ui.HideEndPanels();

                if (objectiveManager != null)
                {
                    ui.SetBeaconProgress(objectiveManager.ActivatedBeacons, 3);
                }
            }

            if (_startGameOnAwake && gameState != null)
            {
                if (_debugLogs)
                {
                    Debug.Log("[RuntimeGameBootstrapper] Starting gameplay bootstrap.", this);
                }

                gameState.StartGame();
            }

            _bootstrapRoutine = null;
        }
    }
}