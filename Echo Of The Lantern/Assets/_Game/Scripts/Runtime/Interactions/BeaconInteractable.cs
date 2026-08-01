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


            if (ObjectiveManager.Instance != null)
            {
                ObjectiveManager.Instance.RegisterBeaconActivated();
            }


            if (UIManager.Instance != null)
            {
                UIManager.Instance.FlashObjectiveProgress();
            }
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
