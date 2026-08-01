using UnityEngine;

namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Starts the actual playable session when the game scene loads.
    /// This is the missing piece that lets the minimal loop enter Playing state automatically.
    /// </summary>
    public sealed class RuntimeGameBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool _startGameOnAwake = true;

        private void Start()
        {
            if (!_startGameOnAwake)
            {
                return;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("[RuntimeGameBootstrapper] GameStateManager is missing.");
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideEndPanels();
            }
        }
    }
}
