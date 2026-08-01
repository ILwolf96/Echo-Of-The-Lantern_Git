using UnityEngine;

namespace EchoOfTheLantern.Runtime.Interactions
{
    public sealed class BeaconInteractable : InteractableBase
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _inactiveSprite;
        [SerializeField] private Sprite _activeSprite;
        [SerializeField] private bool _isActivated;

        protected override void Awake()
        {
            base.Awake();

            if (_renderer == null)
            {
                _renderer = GetComponent<SpriteRenderer>();
            }

            ApplyVisualState();
        }

        public override bool CanInteract(PlayerInteractionController interactor)
        {
            return !_isActivated && interactor != null;
        }

        public override void Interact(PlayerInteractionController interactor)
        {
            if (_isActivated)
            {
                return;
            }

            _isActivated = true;
            ApplyVisualState();

            ObjectiveManager objectiveManager = ObjectiveManager.Resolve();
            if (objectiveManager != null)
            {
                objectiveManager.RegisterBeaconActivated();
            }
            else
            {
                Debug.LogError("[BeaconInteractable] ObjectiveManager is missing even after Resolve().", this);
            }

            UIManager ui = UIManager.Resolve();
            if (ui != null)
            {
                ui.FlashObjectiveProgress();
            }
            else
            {
                Debug.LogWarning("[BeaconInteractable] UIManager is missing even after Resolve().", this);
            }

            Debug.Log("[BeaconInteractable] Beacon activated.", this);
        }

        public override string GetPrompt()
        {
            return _isActivated ? string.Empty : "Activate Beacon (E)";
        }

        private void ApplyVisualState()
        {
            if (_renderer == null)
            {
                return;
            }

            if (_isActivated && _activeSprite != null)
            {
                _renderer.sprite = _activeSprite;
            }
            else if (!_isActivated && _inactiveSprite != null)
            {
                _renderer.sprite = _inactiveSprite;
            }
        }
    }
}