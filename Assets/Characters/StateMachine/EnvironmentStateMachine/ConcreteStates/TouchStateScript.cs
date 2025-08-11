using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine.ConcreteStates {
    public class TouchStateScript : EnvironmentInteractionStateScript {
        
        public TouchStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions estate) 
            : base(context, estate) { }

        private Vector3 groundPos;
        private Vector3 startPos;
        public override void EnterState() {
            Debug.Log("TOUCH State");
            Context.SetCurrentStep(GetOppositeStep(Context.CurrentStep));
            SetStepThreshold(0.5f);
            startPos = Context.RootTransform.position;
            
            if (Physics.Raycast(
                    Context.CurrentIkConstraint.data.root.position,
                    Vector3.down,
                    out RaycastHit hit,
                    maxDistance: 6f,
                    Context.GroundLayer)) 
            {
                groundPos = hit.point;
                Context.SetIkTargetWorldPosition(groundPos);
            }
        }


        public override void ExitState() {
            Debug.Log("EXIT TOUCH State");
        }

        public override void UpdateState() {
            
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
        
        
        private bool MovementThresholdReached()
        {
            Vector3 rootDelta = Context.RootTransform.position - startPos;
            float planarDistance = new Vector2(rootDelta.x, rootDelta.z).magnitude;
            return planarDistance >= StepThreshold;
        }
    }
}
