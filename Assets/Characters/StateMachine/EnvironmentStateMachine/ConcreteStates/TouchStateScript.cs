using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine.ConcreteStates {
    public class TouchStateScript : EnvironmentInteractionStateScript {
        
        public TouchStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions estate) 
            : base(context, estate) { }

        private Vector3 _currentFootGroundPos;
        private Vector3 _previousFootGroundPos;
        private Vector3 _startPos;

        private int stepCount = 0;
        
        private Transform _previousIkTargetTransform;
        public override void EnterState() {
            Debug.Log("TOUCH State");
            _startPos = Context.RootTransform.position;
            SetStepThreshold(0.75f);

            _previousFootGroundPos = GetGroundPos();
            Context.SetCurrentStep(Context.GetOppositeStep(Context.CurrentStep));
            _currentFootGroundPos = GetGroundPos();
            
        }

        public override void ExitState() {
            Debug.Log("EXIT TOUCH State");
            _previousIkTargetTransform = Context.CurrentIkTargetTransform;
        }

        public override void UpdateState() {
            Context.SetIkTargetWorldPosition(_currentFootGroundPos);
            Context.SetIkPreviousTargetWorldPosition(_previousFootGroundPos);
        }

        public override EnvironmentInteractionStateMachineScript.EEnvironmentActions GetNextState() {
            if (MovementThresholdReached()) {
                return EnvironmentInteractionStateMachineScript.EEnvironmentActions.Rise;
            }
            return StateKey;
        }
 
        public override void OnTriggerEnter(Collider other) {
            Debug.Log("TRIGGER ENTER");
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
    }
}
