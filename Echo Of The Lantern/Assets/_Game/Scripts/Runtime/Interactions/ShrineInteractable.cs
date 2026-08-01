using UnityEngine;


namespace EchoOfTheLantern.Runtime.Interactions
{
    public sealed class ShrineInteractable : InteractableBase
    {
        public override bool CanInteract(PlayerInteractionController interactor)
        {
            return ObjectiveManager.Instance != null && ObjectiveManager.Instance.AreAllBeaconsActivated;
        }


        public override void Interact(PlayerInteractionController interactor)
        {
            if (CanInteract(interactor) && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.WinGame();
            }
        }


        public override string GetPrompt()
        {
            if (ObjectiveManager.Instance == null || !ObjectiveManager.Instance.AreAllBeaconsActivated)
            {
                return "Restore all beacons";
            }


            return "Return to Shrine (E)";
        }
    }
}
