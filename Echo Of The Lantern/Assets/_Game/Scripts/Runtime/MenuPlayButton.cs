using UnityEngine;

namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Scene-bound Play button target so the menu click is persistent after the scene is saved.
    /// </summary>
    public sealed class MenuPlayButton : MonoBehaviour
    {
        public void PlayGame()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.BeginGameplayFromMenu();
            }
            else
            {
                Debug.LogError("[MenuPlayButton] GameStateManager is missing.");
            }
        }
    }
}