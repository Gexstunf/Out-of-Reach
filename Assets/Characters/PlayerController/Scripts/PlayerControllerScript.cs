using System;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.StateMachine.PlayerStateMachine;
using Characters.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.PlayerController.Scripts
{
    public class PlayerControllerScript : MonoBehaviour
    {
        #region Variables
        
        [Header("References")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private RotatorScript _rotator;
        [SerializeField] private CameraControllerScript _cameraController;
        [SerializeField] private PlayerInputScript _inputScript;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] public CapsuleCollider _playerCollider;
        
        [Header("State machine")]
        [SerializeField] private PlayerStateMachineScript _playerStateMachine;

        [Header("Movement Settings")] 
        public float moveForce = 30f;
        public float runForce = 60f;
        public float playerDrag = 5f;
        
        [Header("Jump Settings")]
        public float jumpForce = 10f;
        public float forwardJumpForce = 5f;
        
        [Header("General settings")]
        public LayerMask groundLayer;

        [Header("Look Settings")]
        [SerializeField] private float _lookSenseH = 10f;
        [SerializeField] private float _lookSenseV = 10f;
        [SerializeField] private float _lookLimitV = 10f;

        [Header("Visualize Variables")] 
        public bool isGrounded = true;
        public Vector3 CurrentForce { get; private set; }
        public float Gravity;
        
        private float _groundCheckOffset;

        #endregion
        
        #region Startup logic
        private void Awake()
        {
            _inputScript = GetComponent<PlayerInputScript>();
            _rb = GetComponent<Rigidbody>();
            _playerCollider = GetComponent<CapsuleCollider>();
            _playerStateMachine = GetComponent<PlayerStateMachineScript>();
            
            _rotator = gameObject.AddComponent<RotatorScript>();
            _cameraController = gameObject.AddComponent<CameraControllerScript>();

            Gravity = Physics.gravity.y;
        }

        private void Start()
        {
            _rb.linearDamping = playerDrag;
            _rotator.Init(_lookSenseH, _lookSenseV, _lookLimitV);
            _cameraController.Init(_lookSenseH, _lookSenseV, _lookLimitV);

            _groundCheckOffset = _playerCollider.radius + 0.1f;
        }
        
        #endregion

        #region Update logic

        private void Update()
        {
            HandleJumping(jumpForce);
            HandleGroundState();
        }

        private void FixedUpdate()
        {
            // this is where physics should occurr
            Vector3 movementDir = CalculateMovementDirection();
            Vector3 force = CalculateNewForce(movementDir);
            CurrentForce = force;
            _rb.AddForce(force);
        }
        
        #endregion
        
        #region Late-update logic
        private void LateUpdate()
        {
            Vector2 lookInput = _inputScript.LookInput;
            _rotator.RotateTransform(lookInput);
            _cameraController.UpdateCameraRotation(lookInput, _playerCamera);
        }
        
        #endregion
        
        #region Movement & ground state logic
        private Vector3 CalculateMovementDirection()
        {
            Vector3 forwardCamTransform = _playerCamera.transform.forward;
            Vector3 rightCamTransform = _playerCamera.transform.right;
            
            Vector3 cameraForwardXZ = new Vector3(forwardCamTransform.x, 0f, forwardCamTransform.z).normalized;
            Vector3 cameraRightXZ = new Vector3(rightCamTransform.x, 0f, rightCamTransform.z).normalized;

            Vector3 movementDirection = cameraRightXZ * _inputScript.MoveInput.x +
                                        cameraForwardXZ * _inputScript.MoveInput.y;
            return movementDirection;
        }

        private Vector3 CalculateNewForce(Vector3 movementDirection) {
            float currentForce = _playerStateMachine.IsRunning ? runForce : moveForce;
            Vector3 force = movementDirection * currentForce;
            return force ;
        }

        private void HandleJumping(float force)
        {
            bool jumped = _inputScript.JumpPressed;
            bool tired = _playerStateMachine.Context.IsTired;
            
            if (jumped && isGrounded && !tired)
            {
                _rb.AddForce(Vector3.up * force, ForceMode.Impulse);
                _rb.AddForce(_rb.transform.forward * forwardJumpForce, ForceMode.Impulse);
                isGrounded = false;
                Debug.Log("Jumped");
            }
        }

        private void HandleGroundState() {
            // maybe more logic
            isGrounded = IsGroundedWhileGrounded();
        }
        
        #endregion
        
        #region Helper funcs

        public void ResetVariables()
        {
            _rb.linearDamping = playerDrag;
        }

        private bool IsGroundedWhileGrounded() {
            float sphereRadius = _playerCollider.radius;
            Vector3 spherePosition = transform.position + Vector3.down * (_playerCollider.height / 2f - sphereRadius + _groundCheckOffset);
            bool hit = Physics.CheckSphere(spherePosition, sphereRadius, groundLayer, QueryTriggerInteraction.Ignore);
            return hit;
        }

        private bool IsGroundedWhileAirborne()
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            float sphereRadius = _playerCollider.radius;
            float maxDistance = 0.3f; 
            bool hit = Physics.SphereCast(origin, sphereRadius, Vector3.down, out _, maxDistance, groundLayer, QueryTriggerInteraction.Ignore);
            return hit;
        }
        
        #endregion

        #region Gizmo draw

        private void OnDrawGizmosSelected()
                {
                    if (_playerCollider == null) return;
        
                    // Draw the original capsule collider (yellow)
                    Gizmos.color = Color.yellow;
                    float radius = _playerCollider.radius;
                    float height = _playerCollider.height;
                    Vector3 center = _playerCollider.center;
        
                    Vector3 top = transform.position + center + Vector3.up * (height / 2 - radius);
                    Vector3 bottom = transform.position + center + Vector3.down * (height / 2 - radius);
        
                    // Draw spheres at the ends
                    Gizmos.DrawWireSphere(top, radius);
                    Gizmos.DrawWireSphere(bottom, radius);
        
                    // Draw lines connecting spheres (body of capsule)
                    Gizmos.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
                    Gizmos.DrawLine(top + Vector3.back * radius, bottom + Vector3.back * radius);
                    Gizmos.DrawLine(top + Vector3.left * radius, bottom + Vector3.left * radius);
                    Gizmos.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
        
                    // Draw the ground check sphere (green if grounded, red if not)
                    Vector3 spherePosition = transform.position + Vector3.down * (height / 2f - radius + _groundCheckOffset);
                    bool grounded = IsGroundedWhileGrounded();
            
                    Gizmos.color = grounded ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(spherePosition, radius);
            
                    // Draw a line from the bottom of the collider to the check sphere
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(bottom, spherePosition);
                }

        #endregion
        
        
    
    }
}
