using EchoOfTheLantern.Runtime.Interactions;
using UnityEngine;


namespace EchoOfTheLantern.Runtime
{
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerInteractionController : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField, Min(0.1f)] private float _interactionRadius = 1.2f;
        [SerializeField] private LayerMask _interactableLayers = ~0;
        [SerializeField] private KeyCode _interactionKey = KeyCode.E;


        [Header("References")]
        [SerializeField] private UIManager _uiManager;


        private readonly Collider2D[] _results = new Collider2D[16];
        private IInteractable _currentInteractable;


        private void Awake()
        {
            if (_uiManager == null)
            {
                _uiManager = FindFirstObjectByType<UIManager>();
            }
        }


        private void Update()
        {
            if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPlaying)
            {
                SetCurrentInteractable(null);
                return;
            }


            RefreshNearestInteractable();


            if (Input.GetKeyDown(_interactionKey) && _currentInteractable != null)
            {
                if (_currentInteractable.CanInteract(this))
                {
                    _currentInteractable.Interact(this);
                }
            }
        }


        private void RefreshNearestInteractable()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, _interactionRadius, _results, _interactableLayers);
            IInteractable best = null;
            float bestDistance = float.MaxValue;


            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _results[i];
                if (hit == null)
                {
                    continue;
                }


                InteractableBase interactableBase = hit.GetComponentInParent<InteractableBase>();
                if (interactableBase == null || !interactableBase.CanInteract(this))
                {
                    continue;
                }


                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = interactableBase;
                }
            }


            SetCurrentInteractable(best);
        }


        private void SetCurrentInteractable(IInteractable next)
        {
            if (ReferenceEquals(_currentInteractable, next))
            {
                return;
            }


            _currentInteractable = next;
            if (_uiManager != null)
            {
                _uiManager.SetInteractionPrompt(_currentInteractable != null ? _currentInteractable.GetPrompt() : string.Empty);
            }
        }
    }
}
