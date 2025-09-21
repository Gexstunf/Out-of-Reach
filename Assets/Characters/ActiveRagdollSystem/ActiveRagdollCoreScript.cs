using System.Collections;
using Characters.Utils.ConfigurableJoints;
using UnityEngine;

namespace Characters.ActiveRagdollSystem {
    public class ActiveRagdollCoreScript : MonoBehaviour
    {
        [Header("Physics settings")] 
        [SerializeField] private int _solverIterations = 12;
        [SerializeField] private int _solverVelIterations = 12;
        [SerializeField] private float _maxAngularVelocity = 20f;

        [Header("Configurable Joint Settings")] 
        [SerializeField] private float _deadAngularDrive = 3.5f;
        [SerializeField] private float _interpolationDuration = 0.5f;
        [SerializeField] private float _interpolationScaler = 1f;
        [SerializeField] private float _stabilizerSpring = 60f;


        public BoneMap[] boneMaps;
        
        public enum StabilizerMode {
            Normal,
            Crouching,
            Standing,
            Dead,
            Reviving
        }

        private BoneMap _stabilizerMap;
        private bool _isInterpolating;
        private float _interpolationTime;
        private bool _targetAlive;
        
        private Coroutine _modeCoroutine;
        private StabilizerMode _currentMode;
        private Vector3 _normalLocalFixedPos;


        [System.Serializable]
        public class BoneMap {
            public Quaternion initialLocalRotation;
            public Transform ghostBone;
            public ConfigurableJoint joint;
            public float angularDriveSpring;
            public bool isStabilizer;
            public Rigidbody rb;
            [HideInInspector] public float startSpring;
            [HideInInspector] public float targetSpring;
        }

        void Start()
        {
            foreach (var bone in boneMaps)
            {
                bone.rb.solverIterations = _solverIterations;
                bone.rb.solverVelocityIterations = _solverVelIterations;
                bone.rb.maxAngularVelocity = _maxAngularVelocity;

                bone.initialLocalRotation = bone.rb.transform.localRotation;

                if (bone.isStabilizer) {
                    _stabilizerMap = bone;
                    _normalLocalFixedPos = bone.rb.transform.localPosition;
                }

                if (bone.joint == null) continue;

                if (bone.joint.angularXDrive.positionSpring != 0f) {
                    bone.angularDriveSpring = bone.joint.angularXDrive.positionSpring;
                    continue;
                }

                var drive = new JointDrive {
                    positionSpring = bone.angularDriveSpring,
                    positionDamper = 0f,
                    maximumForce = Mathf.Infinity
                };

                bone.joint.angularXDrive = drive;
                bone.joint.angularYZDrive = drive;
            }
        }

        void FixedUpdate()
        {
            if (_isInterpolating)
                UpdateInterpolation();

            SyncBoneRotations();
        }

        public void ApplyStabilizerPitch(float anglePitch, float rotationSpeed, bool hasToPitch)
        {
            if (_stabilizerMap == null) return;

            Quaternion currentLocal = _stabilizerMap.rb.transform.localRotation;
            Quaternion targetLocal = currentLocal;

            if (hasToPitch) {
                Quaternion pitchOffset = Quaternion.Euler(anglePitch, 0f, 0f);
                targetLocal = _stabilizerMap.initialLocalRotation * pitchOffset;
            }
            else {
                targetLocal = _stabilizerMap.initialLocalRotation;
            }
            

            _stabilizerMap.rb.transform.localRotation =
                Quaternion.Slerp(currentLocal, targetLocal, Time.fixedDeltaTime * rotationSpeed);
        }

        public void Kill()
        {
            SetAlive(false);
        }

        public void Revive()
        {
            SetAlive(true);
        }

        private void SetAlive(bool alive)
        {
            _targetAlive = alive;
            _interpolationTime = 0f;
            _isInterpolating = true;

            foreach (var bone in boneMaps)
            {
                if (!bone.joint) continue;

                bone.startSpring = bone.joint.angularXDrive.positionSpring;
                bone.targetSpring = alive ? bone.angularDriveSpring : _deadAngularDrive;
            }
        }

        private void UpdateInterpolation()
        {
            _interpolationTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(_interpolationTime / _interpolationDuration);

            foreach (var bone in boneMaps)
            {
                if (!bone.joint) continue;

                JointDrive driveX = bone.joint.angularXDrive;
                JointDrive driveYZ = bone.joint.angularYZDrive;

                float diff = Mathf.Abs(bone.startSpring - bone.targetSpring);
                float boneDuration = Mathf.Max(0.01f, _interpolationDuration * (_interpolationScaler / (diff + 1f)));
                float localT = Mathf.Clamp01(_interpolationTime / boneDuration);

                float lerped = Mathf.Lerp(bone.startSpring, bone.targetSpring, localT);

                driveX.positionSpring = lerped;
                driveYZ.positionSpring = lerped;

                bone.joint.angularXDrive = driveX;
                bone.joint.angularYZDrive = driveYZ;
            }

            if (t >= 1f)
                _isInterpolating = false;
        }

        private void SyncBoneRotations()
        {
            foreach (var bone in boneMaps)
            {
                if (bone.joint && bone.ghostBone) {
                    bone.joint.SetTargetRotationLocal(bone.ghostBone.localRotation, bone.initialLocalRotation);
                }
            }
        }

        public void SetStabilizerMode(StabilizerMode mode, object parameters) {
            if (_modeCoroutine != null && _currentMode != mode ) {
                StopCoroutine(_modeCoroutine);
                _modeCoroutine = null;
                _currentMode = mode;
            }
            
            switch (mode) {
                case StabilizerMode.Normal:
                    _stabilizerMap.joint.yMotion = ConfigurableJointMotion.Locked;
                    _stabilizerMap.joint.targetPosition = _normalLocalFixedPos;
                    break;

                case StabilizerMode.Crouching:
                    if (parameters is CrouchParams crouch) {
                        AllowLimitedYMovement(crouch.Height);
                    }                    
                    break;

                case StabilizerMode.Standing:
                    if (parameters is StandParams stand) {
                        _modeCoroutine = StartCoroutine(CrouchRoutine(stand.Duration));
                    }
                    break;

                case StabilizerMode.Dead:
                    if (parameters is DeathParams death) {
                        SetAlive(false);
                        if (death.AllowLimitedMovement) AllowLimitedConfigurableJointMovement(_stabilizerMap.joint);
                    }
                    break;

                case StabilizerMode.Reviving:
                    if (parameters is RevivalParams revive) {
                        SetAlive(true);
                        _modeCoroutine = StartCoroutine(SmoothLockStabilizer(_stabilizerMap.joint, revive));
                    }
                    break;
            }
        }

        #region Coroutine Logic
        
        private IEnumerator SmoothLockStabilizer(ConfigurableJoint joint, RevivalParams p) {
            float elapsed = 0f;
            var limit = joint.linearLimit;

            if (p.UseClearance) {
                limit.limit = p.StartClearance;
                joint.linearLimit = limit;
            }
            
            joint.yMotion = p.YMotionStart;


            // initialize drive properties once
            JointDrive drive = joint.yDrive;
            drive.positionSpring = p.StartSpring;
            drive.positionDamper = p.Damper;
            joint.yDrive = drive;


            while (elapsed < p.Duration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / p.Duration);

                if (p.UseClearance) {
                    limit.limit = Mathf.Lerp(p.StartClearance, p.EndClearance, t);
                    joint.linearLimit = limit;
                }
                drive.positionSpring = Mathf.Lerp(p.StartSpring, p.EndSpring, t);
                joint.yDrive = drive;
                
                yield return null;
            }

            // finally hard lock and restore limits
            joint.yMotion = p.YMotionEnd;
            joint.angularXMotion = p.AngularXEnd;
            joint.angularYMotion = p.AngularYEnd;
            joint.angularZMotion = p.AngularZEnd;
            
        }

        private IEnumerator CrouchRoutine(float duration) {
            _stabilizerMap.joint.yMotion = ConfigurableJointMotion.Limited;

            // cache start and end
            Vector3 startTarget = _stabilizerMap.joint.targetPosition;
            Vector3 endTarget   = _normalLocalFixedPos; // standing offset (local)

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // smooth interpolate back
                _stabilizerMap.joint.targetPosition = Vector3.Lerp(startTarget, endTarget, t);

                yield return null;
            }

            // snap to final position and lock again
            _stabilizerMap.joint.targetPosition = endTarget;
            _stabilizerMap.joint.yMotion = ConfigurableJointMotion.Locked;
            _currentMode = StabilizerMode.Normal;
        }

        #endregion
        
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

        void AllowLimitedYMovement(float height) {
            _stabilizerMap.joint.yMotion = ConfigurableJointMotion.Limited;
            var jointLim = new SoftJointLimit();
            jointLim.limit = height;
            _stabilizerMap.joint.linearLimit = jointLim;
            _stabilizerMap.joint.targetPosition = new Vector3(0f, height, 0f);
        }
    }
}
