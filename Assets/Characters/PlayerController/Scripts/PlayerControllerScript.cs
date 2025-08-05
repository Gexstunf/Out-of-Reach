using System;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
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
        [SerializeField] public CapsuleCollider playerCollider;
        
        [Header("State machine")]
        [SerializeField] private PlayerStateMachineScript _playerStateMachine;

        [Header("Movement Settings")] 
        public float moveForce = 30f;
        public float runForce = 10f;
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
        
        #endregion
        
        #region Startup logic
        private void Awake()
        {
            _inputScript = GetComponent<PlayerInputScript>();
            _rb = GetComponent<Rigidbody>();
            _rotator = gameObject.AddComponent<RotatorScript>();
            _cameraController = gameObject.AddComponent<CameraControllerScript>();
        }

        private void Start()
        {
            _rb.linearDamping = playerDrag;
            _rotator.Init(_lookSenseH, _lookSenseV, _lookLimitV);
            _cameraController.Init(_lookSenseH, _lookSenseV, _lookLimitV);
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
        
        #region Movement / state logic
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

        private Vector3 CalculateNewForce(Vector3 movementDirection)
        {
            Vector3 force = movementDirection * moveForce;
            // Vector3 currentDrag = force.normalized * drag;
            // Vector3 newForce = force.magnitude > playerDrag ? (force - currentDrag) : Vector3.zero;
            
            return force ;
        }

        private void HandleJumping(float force)
        {
            bool jumped = _inputScript.JumpPressed;
            bool canJump = !_playerStateMachine.Context.IsTired;
            
            if (jumped && isGrounded && canJump)
            {
                _rb.AddForce(Vector3.up * force, ForceMode.Impulse);
                _rb.AddForce(_rb.transform.forward * forwardJumpForce, ForceMode.Impulse);
                isGrounded = false;
                Debug.Log("Jumped");
            }
        }

        private void HandleGroundState() {
            isGrounded = IsGroundedWhileGrounded();
        }
        
        #endregion
        
        #region Helper funcs

        public void ResetVariables()
        {
            _rb.linearDamping = playerDrag;
        }

        private bool IsGroundedWhileGrounded() {
            float sphereRadius = 0.3f;
            float offset = 0.1f; // slightly below feet
            Vector3 spherePosition = transform.position + Vector3.down * (playerCollider.height / 2f - sphereRadius + offset);

            return Physics.CheckSphere(spherePosition, sphereRadius, groundLayer, QueryTriggerInteraction.Ignore);
        }

        private bool IsGroundedWhileAirborne()
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            float sphereRadius = 0.3f;
            float maxDistance = 0.3f; 
            return Physics.SphereCast(origin, sphereRadius, Vector3.down, out _, maxDistance, groundLayer, QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            float sphereRadius = 0.3f;
            float maxDistance = 0.3f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, sphereRadius); // start
            Gizmos.DrawLine(origin, origin + Vector3.down * maxDistance); // direction
            Gizmos.DrawWireSphere(origin + Vector3.down * maxDistance, sphereRadius); // end
        }
        
        #endregion
    
    }
}
