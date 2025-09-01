using UnityEngine;

namespace Characters.IKSystem.Planners {
    public class GaitPlannerScript {
        public enum StepLeg { Left, Right }

        private StepLeg _currentStep = StepLeg.Right;
        private readonly StepPlannerScript _leftFootPlanner = new StepPlannerScript();
        private readonly StepPlannerScript _rightFootPlanner = new StepPlannerScript();
                                                    
        public Transform LeftFootTarget;
        public Transform RightFootTarget;

        public GaitPlannerScript(Transform leftFootTarget, Transform rightFootTarget) {
            LeftFootTarget = leftFootTarget;
            RightFootTarget = rightFootTarget;
        }

        public void UpdateGait(Vector3 rootPos, Vector3 leftGround, Vector3 rightGround, FootIKSettingsSO settings) {
            if (!_leftFootPlanner.IsStepping && !_rightFootPlanner.IsStepping) {
                // pick foot based on currentStep
                if (_currentStep == StepLeg.Left) _leftFootPlanner.UpdatePlan(LeftFootTarget.position, leftGround, settings.maxStepDistance);
                else _rightFootPlanner.UpdatePlan(RightFootTarget.position, rightGround, settings.maxStepDistance);
            }

            float leftT = 0f, rightT = 0f;
            Vector3 leftPos = _leftFootPlanner.GetStepPosition(LeftFootTarget.position, settings.stepSpeed, settings.stepHeight, ref leftT);
            Vector3 rightPos = _rightFootPlanner.GetStepPosition(RightFootTarget.position, settings.stepSpeed, settings.stepHeight, ref rightT);

            LeftFootTarget.position = leftPos;
            RightFootTarget.position = rightPos;

            // Alternate step if current foot finished moving
            if (!_leftFootPlanner.IsStepping && _currentStep == StepLeg.Left) _currentStep = StepLeg.Right;
            if (!_rightFootPlanner.IsStepping && _currentStep == StepLeg.Right) _currentStep = StepLeg.Left;
        }
    }
}
