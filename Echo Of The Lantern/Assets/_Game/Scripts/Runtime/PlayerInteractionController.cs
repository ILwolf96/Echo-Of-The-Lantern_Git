// =========================================================
// FILE: PlayerInteractionController.cs
// PATH: Assets/_Game/Scripts/Runtime/PlayerInteractionController.cs
// =========================================================
using System.Collections.Generic;
using EchoOfTheLantern.Runtime.Interactions;
using UnityEngine;

namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Lets the player interact with the nearest valid interactable.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerInteractionController : MonoBehaviour
    {
        [Header("Interaction")]
        [SerializeField, Min(0.1f)] private float _interactionRadius = 1.2f;
        [SerializeField] private LayerMask _interactableLayers = ~0;
        [SerializeField] private KeyCode _interactionKey = KeyCode.E;

        [Header("References")]
        [SerializeField] private UIManager _uiManager;

        private readonly List<Collider2D> _results = new List<Collider2D>(16);
        private ContactFilter2D _contactFilter;
        private PlayerController _playerController;
        private IInteractable _currentInteractable;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();

            // Initialize the ContactFilter2D with layer mask settings
            _contactFilter = new ContactFilter2D();
            _contactFilter.useLayerMask = true;
            _contactFilter.layerMask = _interactableLayers;
            _contactFilter.useTriggers = true; // Ensures trigger colliders are detected
        }

        private void Start()
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
            _results.Clear();

            // Updated OverlapCircle supporting a List buffer via ContactFilter2D
            Physics2D.OverlapCircle(transform.position, _interactionRadius, _contactFilter, _results);

            IInteractable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < _results.Count; i++)
            {
                Collider2D hit = _results[i];
                if (hit == null)
                {
                    continue;
                }

                IInteractable interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract(this))
                {
                    continue;
                }

                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = interactable;
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