// =========================================================
// FILE: EndScreenController.cs
// PATH: Assets/_Game/Scripts/Runtime/EndScreenController.cs
// =========================================================
using UnityEngine;

namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Simple restart helper for the end screens.
    /// </summary>
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
