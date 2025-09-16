using System;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.StateMachine.PlayerStateMachine;
using Characters.Utils;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;

namespace Characters.PlayerController.Scripts
{
    //[RequireComponent(typeof(Rigidbody))]
    //[RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(PlayerInputScript))]
    [RequireComponent(typeof(PlayerStateMachineScript))]
    public class PlayerControllerScript : MonoBehaviourPun
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
        public bool useCustomGravity;
        [Range(0f, 1f)] public float gravityScale = 1f;
        public float moveForce = 30f;
        public float runForce = 60f;
        public float playerDrag = 20f;
        
        [Header("Crouch Settings")]
        public float crouchHeight = 1.5f;
        
        [Header("Jump Settings")]
        public float jumpForce = 10f;
        public float forwardJumpForce = 5f;

        [Header("General settings")]
        public LayerMask groundLayer;
        public Vector3 groundCheckBoxSize = new Vector3(0.5f, 0.15f, 0.5f);
        public bool useOtherRb = true;

        [Header("Look Settings")]
        [SerializeField] private float _lookSenseH = 10f;
        [SerializeField] private float _lookSenseV = 10f;
        [SerializeField] private float _lookLimitV = 10f;
        [SerializeField] private Transform _eyesTransform;
        public Vector3 eyesOffset;

        [Header("Visualize Variables")]
        public bool isGrounded = true;
        public Vector3 CurrentForce { get; private set; }
        public float visualGravity;
        
        private float _groundCheckOffset;

        #endregion

        #region Startup logic

        private void Awake()
        {   
            if (!useOtherRb) _rb = GetComponent<Rigidbody>();
            _inputScript = GetComponent<PlayerInputScript>();
            _playerCollider = GetComponent<CapsuleCollider>();
            _playerStateMachine = GetComponent<PlayerStateMachineScript>();
            
            _rotator = gameObject.AddComponent<RotatorScript>();
            _cameraController = new CameraControllerScript();
            _cameraController.TieToTransform(_eyesTransform, eyesOffset);

            if (useCustomGravity) {
                _rb.useGravity = false;
            }   
            else {
                _rb.useGravity = true;
            }

            Validate();
        }

        private void Start()
        {
            if (!photonView.IsMine)
            {
                if (_playerCamera != null) _playerCamera.enabled = false;

                AudioListener listener = GetComponentInChildren<AudioListener>();
                if (listener != null) listener.enabled = false;

                _rb.isKinematic = true;
            }
            else
            {
                _rotator.Init(_lookSenseH, _lookSenseV, _lookLimitV);
                _cameraController.Init(_lookSenseH, _lookSenseV, _lookLimitV);

                _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            _rb.linearDamping = playerDrag;
            _groundCheckOffset = _playerCollider.radius + 0.1f;
        }

        #endregion

        #region Update logic

        private void Update()
        {
            if (!photonView.IsMine) return;

            HandleJumping(jumpForce);
            HandleGroundState();
        } 

        private void FixedUpdate()
        {
            if (!photonView.IsMine) return;

            if (useCustomGravity) {
                ApplyCustomGravity(gravityScale);
            }

            Vector3 movementDir = CalculateMovementDirection();
            Vector3 force = CalculateNewForce(movementDir);
            CurrentForce = force;

            _rb.AddForce(force, ForceMode.Force);

            float yRotation = _inputScript.LookInput.x * _lookSenseH;
            Quaternion deltaRotation = Quaternion.Euler(0f, yRotation, 0f);
            _rb.MoveRotation(_rb.rotation * deltaRotation);
        }

        #endregion

        #region Late-update logic

        private void LateUpdate()
        {
            if (!photonView.IsMine) return;
            
            visualGravity = Physics.gravity.y;
            if (useCustomGravity) visualGravity = Physics.gravity.y * gravityScale;


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

        private Vector3 CalculateNewForce(Vector3 movementDirection)
        {
            float currentForce = _playerStateMachine.IsRunning ? runForce : moveForce;
            Vector3 force = movementDirection * currentForce;
            return force;
        }

        private void HandleJumping(float force)
        {
            bool jumped = _inputScript.JumpPressed;
            bool tired = _playerStateMachine.Context.IsTired;

            if (jumped && isGrounded && !tired)
            {
                _rb.AddForce(Vector3.up * force, ForceMode.Impulse);
                _rb.AddForce(transform.forward * forwardJumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }

        private void HandleGroundState()
        {
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

        private void ApplyCustomGravity(float scale) {
            Vector3 grav = Physics.gravity * scale;
            _rb.AddForce(grav, ForceMode.Acceleration);
        }

        private void Validate() {
            if (_rb == null) Debug.LogWarning("Missing _rb: " + _rb);
            if (_inputScript == null) Debug.LogWarning("Missing _inputScript: " + _inputScript);
            if (_playerCamera == null) Debug.LogWarning("Missing _camera: " + _cameraController);
            if (_playerCollider == null) Debug.LogWarning("Missing _playerCollider: " + _playerCollider);
        }

        #endregion

        #region Gizmo draw

        private void OnDrawGizmosSelected()
        {
            if (_playerCollider == null) return;
            
            Vector3 pos = transform.TransformPoint(_playerCollider.center) - new Vector3(0f, _playerCollider.radius, 0f);
            bool grounded = IsGroundedWhileGrounded();

            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawCube(pos, groundCheckBoxSize * 2);
        }
        #endregion
    }
}
