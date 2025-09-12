using System;
using System.Collections;
using Characters.SystemAdaptations;
using Characters.Utils.ConfigurableJoints;
using Unity.Mathematics;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class ActiveRagdollControllerScript : MonoBehaviour
    {
        
        
        //[Header("Debug Settings")]
        //[Tooltip("Debug HAS to be on for these properties to take effect.")]
        //[SerializeField] private bool debug = false;
        [SerializeField] private bool alive = true;
        
        [Header("Settings")]
        [SerializeField] private StateVitalsCoordinator _stateVitalsCoordinator;
        
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

        private ConfigurableJoint _stabilizerJoint;
        private bool _isInterpolating = false;
        private float _interpolationTime = 0f;
        private bool _targetAlive;
        private Coroutine _stabilizerCoroutine;

        
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
            foreach (var bone in boneMaps) {
                // beware of those who dont have joints
                bone.rb.solverIterations = _solverIterations;
                bone.rb.solverVelocityIterations = _solverVelIterations;
                bone.rb.maxAngularVelocity = _maxAngularVelocity;
                
                bone.initialLocalRotation = bone.rb.transform.localRotation;

                if (bone.isStabilizer) {
                    _stabilizerJoint = bone.joint;
                }
                
                if (!bone.joint) {
                    Debug.Log("Bone doesnt have joint: " + bone.rb.name);    
                }
                else {
                    bone.angularDriveSpring = bone.joint.angularXDrive.positionSpring;
                }
            }

            _stateVitalsCoordinator.OnTiredChanged += HandleActiveRagdollState;
        }

        private void OnDisable() {
            _stateVitalsCoordinator.OnTiredChanged -= HandleActiveRagdollState;
        }

        public BoneMap[] boneMaps;

        void FixedUpdate()
        {

            // if (debug || !alive) {
            //     return;
            // }

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
            AllowLimitedConfigurableJointMovement(_stabilizerJoint);
            
            if (_stabilizerCoroutine != null) {
                StopCoroutine(_stabilizerCoroutine);
                _stabilizerCoroutine = null;
            }
        }
        
        [ContextMenu("Revive active ragdoll")]
        void ReviveActiveRagdoll() {
            SetActiveRagdollState(isAlive: true);
            SmoothLockStabilizer(_stabilizerJoint);
            
            if (_stabilizerCoroutine != null) {
                StopCoroutine(_stabilizerCoroutine);
            }
            _stabilizerCoroutine = StartCoroutine(SmoothLockStabilizer(_stabilizerJoint)); 
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
