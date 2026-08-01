// =========================================================
// FILE: HazardZoneController.cs
// PATH: Assets/_Game/Scripts/Runtime/Interactions/HazardZoneController.cs
// =========================================================
using UnityEngine;

namespace EchoOfTheLantern.Runtime.Interactions
{
    /// <summary>
    /// Simple lose-condition hazard.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class HazardZoneController : MonoBehaviour
    {
        [SerializeField] private bool _loseOnTouch = true;
        [SerializeField] private bool _disableAfterHit = true;
        [SerializeField] private GameObject _visualRoot;

        private bool _triggered;

        private void Awake()
        {
            Collider2D collider2D = GetComponent<Collider2D>();
            collider2D.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered)
            {
                return;
            }

            if (!other.TryGetComponent<PlayerController>(out _))
            {
                return;
            }

            _triggered = true;

            if (_loseOnTouch && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.LoseGame();
            }

            if (_disableAfterHit && _visualRoot != null)
            {
                _visualRoot.SetActive(false);
            }
        }
    }
}
