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
        public float playerDrag = 20f;
        
        [Header("Jump Settings")]
        public float jumpForce = 10f;
        public float forwardJumpForce = 5f;
        
        [Header("General settings")]
        public LayerMask groundLayer;
        public Vector3 groundCheckBoxSize = new Vector3(0.5f, 0.15f, 0.5f);

        [Header("Look Settings")]
        [SerializeField] private float _lookSenseH = 10f;
        [SerializeField] private float _lookSenseV = 10f;
        [SerializeField] private float _lookLimitV = 10f;
        [SerializeField] private Transform _eyesTransform;
        public Vector3 eyesOffset;

        [Header("Visualize Variables")] 
        public bool isGrounded = true;
        public Vector3 CurrentForce { get; private set; }
        public float gravity;
        
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
            _cameraController = new CameraControllerScript();
            _cameraController.TieToTransform(_eyesTransform, eyesOffset);
            gravity = Physics.gravity.y;
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

        private void Update() {

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
            
            float characterYaw = _rotator.GetYaw();

            _cameraController.UpdateCameraRotation(lookInput, _playerCamera, characterYaw);
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
            Vector3 pos = transform.TransformPoint(_playerCollider.center) - new Vector3(0f, _playerCollider.radius, 0f);
            
            bool hit = Physics.CheckBox(
                pos,
                groundCheckBoxSize,
                Quaternion.identity,
                groundLayer
            );

            return hit;
        }
        
        #endregion

        #region Gizmo draw

        private void OnDrawGizmosSelected() {
            if (_playerCollider == null) return;

            Vector3 pos = transform.TransformPoint(_playerCollider.center) - new Vector3(0f, _playerCollider.radius, 0f);
            bool grounded = IsGroundedWhileGrounded();

            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawCube(pos, groundCheckBoxSize * 2);
        }
        #endregion
    }
}
