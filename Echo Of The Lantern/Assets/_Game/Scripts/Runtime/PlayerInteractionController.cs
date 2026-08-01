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
        [SerializeField] private bool _enableDebugLogs = true;

        [Header("References")]
        [SerializeField] private UIManager _uiManager;

        private PlayerController _playerController;
        private IInteractable _currentInteractable;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
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

            if (Input.GetKeyDown(_interactionKey))
            {
                if (_currentInteractable != null)
                {
                    if (_currentInteractable.CanInteract(this))
                    {
                        if (_enableDebugLogs)
                        {
                            Debug.Log($"[PlayerInteractionController] Interacting with {_currentInteractable.GetType().Name}", this);
                        }

                        _currentInteractable.Interact(this);
                    }
                    else if (_enableDebugLogs)
                    {
                        Debug.Log($"[PlayerInteractionController] Interactable exists but CanInteract returned false: {_currentInteractable.GetType().Name}", this);
                    }
                }
                else if (_enableDebugLogs)
                {
                    Debug.Log("[PlayerInteractionController] No interactable in range.", this);
                }
            }
        }

        private void RefreshNearestInteractable()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _interactionRadius, _interactableLayers);

            IInteractable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
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

            if (_enableDebugLogs)
            {
                Debug.Log(_currentInteractable != null
                    ? $"[PlayerInteractionController] Current interactable: {_currentInteractable.GetType().Name}"
                    : "[PlayerInteractionController] No current interactable.", this);
            }
        }
    }
}