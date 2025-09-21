using System;
using System.Collections;
using Characters.ActiveRagdollSystem;
using Characters.PlayerController.Scripts.Input;
using Characters.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations;
using Characters.Utils.ConfigurableJoints;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

namespace Characters.PlayerController.Scripts {
    public class ActiveRagdollControllerScript : MonoBehaviour {


        //[Header("Debug Settings")]
        //[Tooltip("Debug HAS to be on for these properties to take effect.")]
        //[SerializeField] private bool debug = false;
        [SerializeField] private bool alive = true;

        [Header("References")] 
        [SerializeField] private PlayerInputScript _playerInputScript;
        [SerializeField] private PlayerControllerScript _playerController;
        [SerializeField] private StateVitalsCoordinator _stateVitalsCoordinator;
        [SerializeField] private PlayerStateMachineScript _playerStateMachine;

        [Header("Physics settings")] 
        [SerializeField] private int _solverIterations = 12;
        [SerializeField] private int _solverVelIterations = 12;
        [SerializeField] private float _maxAngularVelocity = 20f;

        [Header("Configurable Joint Settings")] 
        [SerializeField] private float _deadAngularDrive;

        [SerializeField] private float _interpolationDuration;
        [SerializeField] private float _interpolationScaler;

        [Header("Revival settings")] 
        [SerializeField] private float _smoothLockDuration = 1.5f;
        [SerializeField] private float _lockSpring = 10000f;
        [SerializeField] private float _lockDamper = 50f;
        [SerializeField] private float _initialClearance = 0.5f;

        [Header("Jump Settings")]
        [SerializeField] private float _stabilizerPitch = 45f;
        [SerializeField] private float _rotationSpeed = 2f;
        [SerializeField] private float _jumpRigBlendSpeed = 1f;
        
        [Header("Rigs")]
        [SerializeField] private Rig _jumpRig;

        public BoneMap[] boneMaps;
        
        private BoneMap _stabilizerMap;
        private Vector3 _normalFixedPos;
        private Vector3 _normalLocalFixedPos;
        private Quaternion _cachedRot; // world
        private Quaternion _baseLocalRotation;

        private bool _hasJumped = false;
        private bool _hasCrouched = false;

        private bool _isInterpolating = false;
        private float _interpolationTime = 0f;
        private bool _targetAlive;
        private Coroutine _stabilizerCoroutine;
        private Coroutine _stabilizerCrouchCoroutine;

        
        [System.Serializable]
        public class BoneMap {
            public Quaternion initialLocalRotation = quaternion.identity;
            public Transform ghostBone;
            public ConfigurableJoint joint;
            public float angularDriveSpring;
            public bool isStabilizer;
            public Rigidbody rb;
            
            [HideInInspector] public float startSpring;
            [HideInInspector] public float targetSpring;
        }

        private void Start() {
            _playerInputScript = GetComponent<PlayerInputScript>();
            foreach (var bone in boneMaps) {
                // beware of those who dont have joints
                bone.rb.solverIterations = _solverIterations;
                bone.rb.solverVelocityIterations = _solverVelIterations;
                bone.rb.maxAngularVelocity = _maxAngularVelocity;
                
                bone.initialLocalRotation = bone.rb.transform.localRotation;

                if (bone.isStabilizer) {
                    _stabilizerMap = bone;
                    _normalLocalFixedPos = bone.rb.transform.localPosition;
                }
                
                if (!bone.joint) {
                    Debug.Log("Bone doesnt have joint: " + bone.rb.name);    
                }
                else {
                    bone.angularDriveSpring = bone.joint.angularXDrive.positionSpring;
                }
            }
            
            _cachedRot = _stabilizerMap.rb.transform.rotation;        
            _baseLocalRotation = _stabilizerMap.rb.transform.localRotation;

            _stateVitalsCoordinator.OnTiredChanged += HandleActiveRagdollState;
        }

        private void OnDisable() {
            _stateVitalsCoordinator.OnTiredChanged -= HandleActiveRagdollState;
        }
        
        void FixedUpdate()
        {

            if (_playerStateMachine.IsJumping || _playerStateMachine.IsFalling) {
                if (!_hasJumped) {
                    _hasJumped = true;
                }

                Quaternion currentLocal = _stabilizerMap.rb.transform.localRotation;

                Quaternion pitchOffset = Quaternion.Euler(_stabilizerPitch, 0f, 0f);
                Quaternion targetLocal = _baseLocalRotation * pitchOffset;
                _stabilizerMap.rb.transform.localRotation =
                    Quaternion.Slerp(currentLocal, targetLocal, Time.fixedDeltaTime * _rotationSpeed);

                _jumpRig.weight = Mathf.Lerp(_jumpRig.weight, 1f, Time.fixedDeltaTime * _jumpRigBlendSpeed);
            }
            else if (_hasJumped) {
                Quaternion currentLocal = _stabilizerMap.rb.transform.localRotation;

                Quaternion targetLocal = _baseLocalRotation;

                _stabilizerMap.rb.transform.localRotation =
                    Quaternion.Slerp(currentLocal, targetLocal, Time.fixedDeltaTime * _rotationSpeed);

                _jumpRig.weight = Mathf.Lerp(_jumpRig.weight, 0f, Time.fixedDeltaTime * _jumpRigBlendSpeed);

                if (_jumpRig.weight < 0.05f) {
                    _hasJumped = false;
                }
            }


            if (_playerInputScript.CrouchPressed && _playerStateMachine.IsIdle) {
                
                _stabilizerMap.joint.yMotion = ConfigurableJointMotion.Limited;
                var jointLim = new SoftJointLimit();
                jointLim.limit = _playerController.crouchHeight;
                _stabilizerMap.joint.linearLimit = jointLim;
                
                _stabilizerMap.joint.targetPosition = new Vector3(0f, _playerController.crouchHeight, 0f);
                
                _hasCrouched = true;
            }
            else if (_hasCrouched) {
                
                if (_stabilizerCoroutine != null) {
                    StopCoroutine(_stabilizerCoroutine);
                }
                _stabilizerCrouchCoroutine = StartCoroutine(CrouchStabilizer());
                _hasCrouched = false;
            }
            
            if (_isInterpolating) {
                _interpolationTime += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(_interpolationTime / _interpolationDuration);

                InterpolateBones(t);
                
                if (t >= 1f) {
                    _isInterpolating = false;
                    alive = _targetAlive; // update state at the end
                    foreach (var bone in boneMaps) {
                        if (!bone.joint) continue;
                        JointDrive driveX = bone.joint.angularXDrive;
                        JointDrive driveYZ = bone.joint.angularYZDrive;

                        driveX.positionSpring = bone.targetSpring;
                        driveYZ.positionSpring = bone.targetSpring;

                        bone.joint.angularXDrive = driveX;
                        bone.joint.angularYZDrive = driveYZ;
                    }
                }
            }
            
            foreach (var bone in boneMaps)
            {
                if (bone.joint && bone.ghostBone) {
                    bone.joint.SetTargetRotationLocal(bone.ghostBone.localRotation, bone.initialLocalRotation);
                } 
            }
        }

        
        void SetActiveRagdollState(bool isAlive) {
            _targetAlive = isAlive;
            _interpolationTime = 0f;
            _isInterpolating = true;
            
            
            foreach (var bone in boneMaps) {
                if (!bone.joint) continue;

                bone.startSpring = bone.joint.angularXDrive.positionSpring;
                bone.targetSpring = isAlive ? bone.angularDriveSpring : _deadAngularDrive;
            }
        }

        void InterpolateBones(float t) {
            foreach (var bone in boneMaps) {
                if (!bone.joint) continue;
                
                JointDrive driveX = bone.joint.angularXDrive;
                JointDrive driveYZ = bone.joint.angularYZDrive;

                float diff = Mathf.Abs(bone.startSpring - bone.targetSpring);

                // Scale duration inversely with diff
                float boneDuration = Mathf.Max(0.01f, _interpolationDuration * (_interpolationScaler / (diff + 1f)));
                float localT = Mathf.Clamp01(_interpolationTime / boneDuration);

                float lerped = Mathf.Lerp(bone.startSpring, bone.targetSpring, localT);

                driveX.positionSpring = lerped;
                driveYZ.positionSpring = lerped;

                bone.joint.angularXDrive = driveX;
                bone.joint.angularYZDrive = driveYZ;
            }
        }

        private IEnumerator SmoothLockStabilizer(ConfigurableJoint joint) {
            float elapsed = 0f;

            joint.yMotion = ConfigurableJointMotion.Limited;

            var limit = joint.linearLimit;
            float startLimit = _initialClearance;
            float endLimit = 0f; 
            limit.limit = startLimit;
            joint.linearLimit = limit;

            JointDrive drive = joint.yDrive;
            float startSpring = 0f;
            drive.positionSpring = startSpring;
            drive.positionDamper = _lockDamper;
            joint.yDrive = drive;

            while (elapsed < _smoothLockDuration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _smoothLockDuration);

                limit.limit = Mathf.Lerp(startLimit, endLimit, t);
                joint.linearLimit = limit;

                drive.positionSpring = Mathf.Lerp(startSpring, _lockSpring, t);
                joint.yDrive = drive;

                yield return null;
            }

            // finally hard lock and restore limits
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Limited;
        }

        private IEnumerator CrouchStabilizer() {
            _stabilizerMap.joint.yMotion = ConfigurableJointMotion.Limited;

            // Cache start and end
            Vector3 startTarget = _stabilizerMap.joint.targetPosition;
            Vector3 endTarget   = _normalLocalFixedPos; // standing offset (local)

            float duration = 0.3f; 
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Smooth interpolate back
                _stabilizerMap.joint.targetPosition = Vector3.Lerp(startTarget, endTarget, t);

                yield return null;
            }

            // Snap to final position and lock again
            _stabilizerMap.joint.targetPosition = endTarget;
            _stabilizerMap.joint.yMotion = ConfigurableJointMotion.Locked;
        }

        void AllowLimitedConfigurableJointMovement(ConfigurableJoint joint, bool allow = true) {
            if (allow) {
                joint.yMotion = ConfigurableJointMotion.Free;
                joint.angularXMotion = ConfigurableJointMotion.Free;
                joint.angularYMotion = ConfigurableJointMotion.Limited;
                joint.angularZMotion = ConfigurableJointMotion.Free;
            }
            else {
                joint.yMotion = ConfigurableJointMotion.Limited;
                joint.angularXMotion = ConfigurableJointMotion.Limited;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Limited;
            }
        }
        
        


        [ContextMenu("Kill active ragdoll")]
        void KillActiveRagdoll() {
            SetActiveRagdollState(false);
            AllowLimitedConfigurableJointMovement(_stabilizerMap.joint);
            
            if (_stabilizerCoroutine != null) {
                StopCoroutine(_stabilizerCoroutine);
                _stabilizerCoroutine = null;
            }
        }
        
        [ContextMenu("Revive active ragdoll")]
        void ReviveActiveRagdoll() {
            SetActiveRagdollState(true);
            SmoothLockStabilizer(_stabilizerMap.joint);
            
            if (_stabilizerCoroutine != null) {
                StopCoroutine(_stabilizerCoroutine);
            }
            _stabilizerCoroutine = StartCoroutine(SmoothLockStabilizer(_stabilizerMap.joint)); 
        }

        private void HandleActiveRagdollState(bool kill) {
            Debug.Log("HANDLING ACTIVE RAGDOLL");
            if (kill) {
                KillActiveRagdoll();
            }
            else {
                ReviveActiveRagdoll();
            }
        }
    }
}
