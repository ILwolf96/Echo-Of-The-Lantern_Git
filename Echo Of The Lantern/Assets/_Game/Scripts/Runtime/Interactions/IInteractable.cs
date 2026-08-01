// =========================================================
// FILE: IInteractable.cs
// PATH: Assets/_Game/Scripts/Runtime/Interactions/IInteractable.cs
// =========================================================
using UnityEngine;

namespace EchoOfTheLantern.Runtime.Interactions
{
    /// <summary>
    /// Contract for all gameplay interactables.
    /// </summary>
    public interface IInteractable
    {
        bool CanInteract(PlayerInteractionController interactor);
        void Interact(PlayerInteractionController interactor);
        string GetPrompt();
    }
}
