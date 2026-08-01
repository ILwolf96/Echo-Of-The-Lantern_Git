// =========================================================
// FILE: CameraFollowController.cs
// PATH: Assets/_Game/Scripts/Runtime/CameraFollowController.cs
// =========================================================
using UnityEngine;

namespace EchoOfTheLantern.Runtime
{
    /// <summary>
    /// Keeps the camera centered on the player while remaining readable and stable.
    /// </summary>
    public sealed class CameraFollowController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField, Min(0f)] private float _followSpeed = 6f;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private bool _snapIfMissingTarget = true;

        private void Start()
        {
            if (_target == null)
            {
                PlayerController player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    _target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 desired = _target.position + _offset;
            if (_followSpeed <= 0f)
            {
                transform.position = desired;
                return;
            }

            transform.position = Vector3.Lerp(transform.position, desired, _followSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_snapIfMissingTarget && _target != null)
            {
                transform.position = _target.position + _offset;
            }
        }
    }
}
