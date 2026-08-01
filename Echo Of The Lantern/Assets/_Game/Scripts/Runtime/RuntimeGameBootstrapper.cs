using UnityEngine;


namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Starts the playable run automatically when the game scene loads.
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
