using Characters.IKSystem.Planners;
using Characters.IKSystem.Solvers;
using UnityEngine;

namespace Characters.IKSystem.RigDrivers {
    public class LegIKInteractionScript : MonoBehaviour
    {
        [SerializeField] private FootIKSettingsSO settings;
        [SerializeField] private FootPosSolverScript solver;
        [Header("Transforms")]
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Transform leftFootIKTarget;
        [SerializeField] private Transform rightFootIKTarget;

        private GaitPlannerScript gaitPlanner;
        void Start() {
            gaitPlanner = new GaitPlannerScript(leftFootIKTarget, rightFootIKTarget);
        }

        void Update() {
            // 1. Detect ground for each foot
            Vector3 leftGround = solver.TryGetGroundPos(leftFootTarget.position);
            Vector3 rightGround = solver.TryGetGroundPos(rightFootIKTarget.position);

            // 2. Update gait planner
            gaitPlanner.UpdateGait(rootTransform.position, leftGround, rightGround, settings);

            // 3. The foot targets are already updated by the planner
            //    -> Animator RigBuilder constraints read these targets
        }
    }
}
