

// =========================================================

// FILE: PlayerController.cs

// PATH: Assets/_Game/Scripts/Runtime/PlayerController.cs

// =========================================================

using UnityEngine;



namespace EchoOfTheLantern.Runtime

{

    /// <summary>

    /// Handles player movement for the 2D top-down game.

    /// 

    /// Design goals:

    /// - Keep movement readable and responsive.

    /// - Avoid hardcoded scene dependencies.

    /// - Allow future swap-in of a dedicated input provider.

    /// - Remain safe if the Input System setup changes later.

    /// </summary>

    [RequireComponent(typeof(Rigidbody2D))]

    public sealed class PlayerController : MonoBehaviour

    {

        [Header("Movement")]

        [SerializeField, Min(0f)] private float _moveSpeed = 4.5f;

        [SerializeField, Min(0f)] private float _acceleration = 18f;

        [SerializeField, Min(0f)] private float _deceleration = 22f;



        [Header("Input")]

        [SerializeField] private bool _useLegacyInputFallback = true;



        private Rigidbody2D _body;

        private Vector2 _moveInput;

        private Vector2 _currentVelocity;



        public Vector2 MoveInput => _moveInput;

        public Vector2 CurrentVelocity => _currentVelocity;



        private void Awake()

        {

            _body = GetComponent<Rigidbody2D>();

            _body.gravityScale = 0f;

            _body.constraints = RigidbodyConstraints2D.FreezeRotation;

        }



        private void Update()

        {

            if (_useLegacyInputFallback)

            {

                ReadLegacyMovementInput();

            }

        }



        private void FixedUpdate()

        {

            Vector2 targetVelocity = _moveInput.normalized * _moveSpeed;

            float rate = _moveInput.sqrMagnitude > 0.001f ? _acceleration : _deceleration;



            _currentVelocity = Vector2.MoveTowards(_currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);

            _body.linearVelocity = _currentVelocity;

        }



        /// <summary>

        /// Call this from a future input adapter if you want to use the new Input System cleanly.

        /// </summary>

        public void SetMoveInput(Vector2 moveInput)

        {

            _moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        }



        public void StopMovement()

        {

            _moveInput = Vector2.zero;

            _currentVelocity = Vector2.zero;

            _body.linearVelocity = Vector2.zero;

        }



        private void ReadLegacyMovementInput()

        {

            float x = Input.GetAxisRaw("Horizontal");

            float y = Input.GetAxisRaw("Vertical");

            SetMoveInput(new Vector2(x, y));

        }

    }

}