using UnityEngine;

namespace Characters.IKSystem.Planners {
    public class StepPlannerScript
    {
        private enum StepState { Idle, Moving }
        private StepState _state = StepState.Idle;

        private Vector3 _currentTarget;
        private Vector3 _desiredTarget;

        public bool IsStepping => _state == StepState.Moving;

        public void UpdatePlan(Vector3 currentPos, Vector3 groundTarget, float maxStepDist) {
            if (_state == StepState.Idle) {
                float dist = Vector3.Distance(currentPos, groundTarget);
                if (dist > maxStepDist) {
                    _desiredTarget = groundTarget;
                    _state = StepState.Moving;
                }
            }
        }

        public Vector3 GetStepPosition(Vector3 currentPos, float speed, float stepHeight, ref float t) {
            if (_state == StepState.Moving) {
                t += Time.deltaTime * speed;
                Vector3 mid = (currentPos + _desiredTarget) / 2f + Vector3.up * stepHeight;
                Vector3 newPos = Vector3.Lerp(
                    Vector3.Lerp(currentPos, mid, t),
                    Vector3.Lerp(mid, _desiredTarget, t),
                    t
                );

                if (t >= 1f) {
                    _state = StepState.Idle;
                    t = 0f;
                    return _desiredTarget;
                }
                return newPos;
            }
            return currentPos;
        }
    }
}
