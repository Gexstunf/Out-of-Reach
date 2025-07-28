using System;
using Characters.PlayerController.Scripts.Input;
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

        [Header("Movement Settings")] 
        public float moveForce = 30f;
        public float runForce = 10f;
        public float playerDrag = 5f;
        public float jumpForce = 10f;

        [Header("Look Settings")]
        [SerializeField] private float _lookSenseH = 10f;
        [SerializeField] private float _lookSenseV = 10f;
        [SerializeField] private float _lookLimitV = 10f;
        
        private PlayerInputScript _locomotionScript;
        private Rigidbody _rb;
        
        #endregion
        
        #region Startup logic
        private void Awake()
        {
            _locomotionScript = GetComponent<PlayerInputScript>();
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
        }

        private void FixedUpdate()
        {
            Vector3 movementDir = CalculateMovementDirection();
            Vector3 force = CalculateNewForce(movementDir);
            
            _rb.AddForce(force);
        }
        
        #endregion
        
        #region Late-update logic
        private void LateUpdate()
        {
            Vector2 lookInput = _locomotionScript.LookInput;
            _rotator.RotateTransform(lookInput);
            _cameraController.UpdateCameraRotation(lookInput, _playerCamera);
        }
        
        #endregion
        
        #region Movement
        private Vector3 CalculateMovementDirection()
        {
            Vector3 forwardCamTransform = _playerCamera.transform.forward;
            Vector3 rightCamTransform = _playerCamera.transform.right;
            
            Vector3 cameraForwardXZ = new Vector3(forwardCamTransform.x, 0f, forwardCamTransform.z).normalized;
            Vector3 cameraRightXZ = new Vector3(rightCamTransform.x, 0f, rightCamTransform.z).normalized;

            Vector3 movementDirection = cameraRightXZ * _locomotionScript.MoveInput.x +
                                        cameraForwardXZ * _locomotionScript.MoveInput.y;
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
            bool jumped = _locomotionScript.JumpPressed;
            
            if (jumped)
            {
                _rb.AddForce(Vector3.up * force, ForceMode.Impulse);
                _rb.linearDamping = 0f;
            }
        }
        
        #endregion
        
        #region Helper funcs

        private void ResetVariables()
        {
            _rb.linearDamping = playerDrag;
        }
        #endregion
    
    }
}
