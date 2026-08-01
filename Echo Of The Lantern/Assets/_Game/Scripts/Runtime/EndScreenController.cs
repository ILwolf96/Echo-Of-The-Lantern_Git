using UnityEngine;


namespace EchoOfTheLantern.Runtime
{
    public sealed class EndScreenController : MonoBehaviour
    {
        public void RestartGame()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.RestartGame();
            }
        }


        public void ReturnToMenu()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ReturnToMenu();
            }
        }
    }
}
