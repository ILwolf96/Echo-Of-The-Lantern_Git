using UnityEngine;


namespace EchoOfTheLantern.Runtime.Interactions
{
    public interface IInteractable
    {
        bool CanInteract(PlayerInteractionController interactor);
        void Interact(PlayerInteractionController interactor);
        string GetPrompt();
    }
}
