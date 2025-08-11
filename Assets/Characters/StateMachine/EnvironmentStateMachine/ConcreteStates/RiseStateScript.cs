using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine.ConcreteStates {
    public class RiseStateScript : EnvironmentInteractionStateScript {
        public RiseStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions estate) : 
            base(context, estate) 
        { }

        private float _stepHeight = 0.6f;
        private float _riseTime = 0.1f;
        private float _timer;
        private Vector3 groundPos;
        public override void EnterState()
        {
            Debug.Log("RISE State");
            _timer = 0f;
            Vector2 input = Context.InputScript.MoveInput;

            if (Context.CurrentStep == EnvironmentInteractionContextScript.EStep.Left) {
                Vector3 pivotedTarget = Context.LeftTargetOffsetPosition; // GetPivotedTarget(Context.CurrentIkTargetTransform, input, _stepHeight);
                pivotedTarget.y += _stepHeight;
                pivotedTarget.z += _stepHeight;
                Context.SetIkTargetLocalPosition(pivotedTarget);
            }
            else {
                Vector3 pivotedTarget = Context.RightTargetOffsetPosition; //GetPivotedTarget(Context.CurrentIkTargetTransform, input, _stepHeight);
                pivotedTarget.y += _stepHeight;
                pivotedTarget.z += _stepHeight;
                Context.SetIkTargetLocalPosition(pivotedTarget);
            }
        }
        
        public override void UpdateState()
        {
            _timer += Time.deltaTime;
        }

        public override void ExitState()
        {
            Debug.Log("EXIT RISE State");
        }


        public override EnvironmentInteractionStateMachineScript.EEnvironmentActions GetNextState()
        {
            if (_timer > _riseTime) return EnvironmentInteractionStateMachineScript.EEnvironmentActions.Touch;
            return StateKey;
        }


        public override void OnTriggerStay(Collider other) { }
        public override void OnTriggerExit(Collider other) { }
        public override void OnTriggerEnter(Collider other) { }
        
    }
}
