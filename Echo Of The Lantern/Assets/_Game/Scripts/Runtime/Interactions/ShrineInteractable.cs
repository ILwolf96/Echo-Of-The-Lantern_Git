using UnityEngine;

namespace EchoOfTheLantern.Runtime.Interactions
{
    public sealed class ShrineInteractable : InteractableBase
    {
        public override bool CanInteract(PlayerInteractionController interactor)
        {
            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();
            return objectiveManager != null && objectiveManager.AreAllBeaconsActivated;
        }

        public override void Interact(PlayerInteractionController interactor)
        {
            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();
            if (objectiveManager == null)
            {
                Debug.LogError("[ShrineInteractable] ObjectiveManager is missing even after Resolve().", this);
                return;
            }

            if (!objectiveManager.AreAllBeaconsActivated)
            {
                Debug.Log("[ShrineInteractable] Shrine is locked until all beacons are active.", this);
                return;
            }

            Debug.Log("[ShrineInteractable] Shrine used. Winning game.", this);

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.WinGame();
            }
        }

        public override string GetPrompt()
        {
            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();
            if (objectiveManager == null || !objectiveManager.AreAllBeaconsActivated)
            {
                return "Restore all beacons";
            }

            return "Return to Shrine (E)";
        }
    }
}