using UnityEngine;


namespace EchoOfTheLantern.Runtime.Interactions
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _prompt = "Interact";


        protected virtual void Awake()
        {
            Collider2D collider2D = GetComponent<Collider2D>();
            collider2D.isTrigger = true;
        }


        public virtual bool CanInteract(PlayerInteractionController interactor)
        {
            return interactor != null;
        }


        public virtual string GetPrompt()
        {
            return _prompt;
        }


        public abstract void Interact(PlayerInteractionController interactor);
    }
}
