using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine.ConcreteStates {
    public class TouchStateScript : EnvironmentInteractionStateScript {
        
        public TouchStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions estate) 
            : base(context, estate) { }

        private Vector3 _currentFootGroundPos;
        private Vector3 _previousFootGroundPos;
        private Vector3 _previousFootAirPos;
        private Vector3 _startPos;

        private float _turnDegrees;
        private float _startDegrees;
        private float _timer;
        private float _downTime = 0.15f;

        private float _lastTurnSign = 0f;
        private int _stepTurnCounter = 0;
        
        private Transform _previousIkTargetTransform;
        public override void EnterState() {
            Debug.Log("TOUCH State");
            _startPos = Context.RootTransform.position;
            _startDegrees = Context.RootTransform.eulerAngles.y;
            _turnDegrees = _startDegrees;
            _timer = 0f;
            
            SetStepThreshold(0.5f);
            
            _previousFootAirPos = Context.CurrentIkConstraint.data.target.position;
            _previousFootGroundPos = GetGroundPos();
            _previousFootGroundPos = ApplySideOffset(_previousFootGroundPos);
            Context.SetCurrentStep(Context.GetOppositeStep(Context.CurrentStep));
            _currentFootGroundPos = GetGroundPos();
            
        }

        public override void ExitState() {
            Debug.Log("EXIT TOUCH State");
            _previousIkTargetTransform = Context.CurrentIkTargetTransform;
        }

        public override void UpdateState() {
            _timer += Time.deltaTime;
            _turnDegrees = Context.RootTransform.rotation.eulerAngles.y;
            
            Context.SetIkTargetWorldPosition(_currentFootGroundPos);
            
            float t = Mathf.Clamp01(_timer / _downTime);
            Vector3 newPreviousPos = Vector3.Lerp(_previousFootAirPos, _previousFootGroundPos, t);
            Context.SetIkPreviousTargetWorldPosition(newPreviousPos);
        }

        public override EnvironmentInteractionStateMachineScript.EEnvironmentActions GetNextState() {
            
            if (Context.StateMachine.IsIdle) {
                if (RotationThresholdReached()) {
                    return EnvironmentInteractionStateMachineScript.EEnvironmentActions.Rise;
                }
            }
            
            if (MovementThresholdReached()) {
                return EnvironmentInteractionStateMachineScript.EEnvironmentActions.Rise;
            }
            return StateKey;
        }
 
        public override void OnTriggerEnter(Collider other) {
            //StartIkTargetPositionTracking(other);
        }
        public override void OnTriggerStay(Collider other) {
            //UpdateIkTargetPositionTracking(other);
        }
        public override void OnTriggerExit(Collider other) {
            //ResetIkTargetPositionTracking(other);
        }
        
        private bool MovementThresholdReached() {
            Vector3 rootDelta = Context.RootTransform.position - _startPos;
            float planarDistance = new Vector2(rootDelta.x, rootDelta.z).magnitude;
            return planarDistance >= StepThreshold;
        }

        private bool RotationThresholdReached() {
            float turnDiff = Mathf.DeltaAngle(_startDegrees, _turnDegrees);
            float absDiff = Mathf.Abs(turnDiff);

            if (absDiff < 50f)
                return false;

            float turnSign = Mathf.Sign(turnDiff);

            if (turnSign != _lastTurnSign) {
                _stepTurnCounter = 0;
                _lastTurnSign = turnSign;
            }

            _stepTurnCounter++;

            var dominantFoot = (turnSign > 0) 
                ? EnvironmentInteractionContextScript.EStep.Right 
                : EnvironmentInteractionContextScript.EStep.Left;

            bool useDominant = (_stepTurnCounter % 2 == 1);
            Context.SetCurrentStep(useDominant ? dominantFoot : Context.GetOppositeStep(dominantFoot));

            return true;
        }
    }
}
