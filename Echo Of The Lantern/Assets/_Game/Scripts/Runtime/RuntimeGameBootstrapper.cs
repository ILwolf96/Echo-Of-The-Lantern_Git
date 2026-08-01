using UnityEngine;

namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Ensures the runtime managers exist and binds the scene UI after the game scene loads.
    /// </summary>
    public sealed class RuntimeGameBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool _startGameOnAwake = true;
        [SerializeField] private bool _autoBindUiFromScene = true;

        private void Start()
        {
            GameStateManager gameState = GameStateManager.Instance != null
                ? GameStateManager.Instance
                : FindFirstObjectByType<GameStateManager>(FindObjectsInactive.Include);

            if (gameState == null)
            {
                GameObject gs = new GameObject("GameStateManager");
                gameState = gs.AddComponent<GameStateManager>();
            }

            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();
            UIManager ui = UIManager.Resolve();

            if (_autoBindUiFromScene && ui != null)
            {
                ui.TryAutoBindFromScene();
                ui.HideEndPanels();
                ui.FlashObjectiveProgress();
            }

            if (_startGameOnAwake && gameState != null)
            {
                gameState.StartGame();
            }
        }
    }
}