using Characters.IKSystem.Planners;
using Characters.IKSystem.Solvers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.IKSystem.RigDrivers
{
    /// <summary>
    /// Updates the IK targets in local space based on the foot solver and gait planner.
    /// </summary>
    public class LegIKInteractionScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FootIKSettingsSO settings;
        [SerializeField] private FootPosSolverScript solver;

        [Header("Transforms")]
        [SerializeField] private Transform ghostRigRoot;
        [SerializeField] private Transform leftLegTarget;
        [SerializeField] private Transform rightLegTarget;

        [Header("Character root for movement")]
        [SerializeField] private Transform rootTransform;
        
        [Header("Flags")]
        [SerializeField] private bool useLocalConversion;

        private GaitPlannerScript gaitPlanner;
        private Vector3 _characterPosition;
        
        private GroundHit _leftHit;
        private GroundHit _rightHit;
        private int _stepCounter;

        void Start()
        {
            _leftHit  = solver.TryGetGround(leftLegTarget.position);
            _rightHit = solver.TryGetGround(rightLegTarget.position);
            
            Vector3 leftInitPos = _leftHit.Position;
            Vector3 rightInitPos = _rightHit.Position;
            Quaternion leftInitRot = leftLegTarget.rotation;
            Quaternion rightInitRot = rightLegTarget.rotation;
            
            _characterPosition = rootTransform.position;
            gaitPlanner = new GaitPlannerScript(leftInitPos, rightInitPos, leftInitRot, rightInitRot);

        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            
            if (MovementThresholdReached(_characterPosition, transform.position)) {
                _characterPosition = rootTransform.position;
                _leftHit  = solver.TryGetGround(leftLegTarget.position);
                _rightHit = solver.TryGetGround(rightLegTarget.position);
                _stepCounter += 1;
                Debug.Log("Taking a step! nº: " + _stepCounter);
            }

            gaitPlanner.UpdateGait(deltaTime, rootTransform, _leftHit, _rightHit, settings);
            
            // convert planner world-space outputs to GhostRig local-space
            if (useLocalConversion) {
               LocalFootTarget leftLocal  = new LocalFootTarget(gaitPlanner.LeftFootTargetPos,  gaitPlanner.LeftFootTargetRot,  ghostRigRoot);
               LocalFootTarget rightLocal = new LocalFootTarget(gaitPlanner.RightFootTargetPos, gaitPlanner.RightFootTargetRot, ghostRigRoot);
   
               leftLegTarget.localPosition  = leftLocal.Position;
               leftLegTarget.localRotation  = leftLocal.Rotation;
               rightLegTarget.localPosition = rightLocal.Position;
               rightLegTarget.localRotation = rightLocal.Rotation; 
            }
            else {
                leftLegTarget.position = gaitPlanner.LeftFootTargetPos;
                rightLegTarget.position = gaitPlanner.RightFootTargetPos;
            }
        }

        /// <summary>
        /// Converts world-space position/rotation into local space relative to a root transform.
        /// </summary>
        private struct LocalFootTarget
        {
            public Vector3 Position;
            public Quaternion Rotation;

            public LocalFootTarget(Vector3 worldPos, Quaternion worldRot, Transform rigRoot)
            {
                Position = rigRoot.InverseTransformPoint(worldPos);
                Rotation = Quaternion.Inverse(rigRoot.rotation) * worldRot;
            }
        }

        private bool MovementThresholdReached(Vector3 a, Vector3 b) {
            a.y = 0f;
            b.y = 0f;
            float dist = Vector3.Distance(a, b);
            return dist > settings.stepThreshold;
        }
    }
}
