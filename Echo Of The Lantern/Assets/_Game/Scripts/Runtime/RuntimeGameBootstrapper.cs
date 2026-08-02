using UnityEngine;

namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Safety bootstrap only. Scene state is now driven by GameStateManager scene-loaded handling.
    /// </summary>
    public sealed class RuntimeGameBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool _bindUiOnStart = true;
        [SerializeField] private bool _debugLogs = true;

        private void Start()
        {
            GameStateManager gsm = GameStateManager.Instance != null
                ? GameStateManager.Instance
                : FindFirstObjectByType<GameStateManager>(FindObjectsInactive.Include);

            if (gsm == null)
            {
                GameObject go = new GameObject("GameStateManager");
                gsm = go.AddComponent<GameStateManager>();
            }

            gsm.InitializeCurrentScene();

            if (_bindUiOnStart)
            {
                UIManager ui = UIManager.Resolve();
                if (ui != null)
                {
                    ui.TryAutoBindFromScene();
                    ui.HideEndPanels();
                }
            }

            if (_debugLogs)
            {
                Debug.Log("[RuntimeGameBootstrapper] Scene bootstrap complete.", this);
            }
        }
    }
}