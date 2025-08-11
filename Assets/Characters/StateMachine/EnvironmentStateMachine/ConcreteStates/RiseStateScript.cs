using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine.ConcreteStates {
    public class RiseStateScript : EnvironmentInteractionStateScript {
        public RiseStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions estate) : 
            base(context, estate) 
        { }

        private float _stepHeight = 0.5f;
        private float _riseTime = 0.15f;
        private float _timer;
        private Vector3 groundPos;
        public override void EnterState()
        {
            Debug.Log("RISE State");
            _timer = 0f;
            
            if (Context.CurrentStep == EnvironmentInteractionContextScript.EStep.Left) {
                Vector3 newPos = Context.LeftTargetOffsetPosition;
                newPos.y += _stepHeight;
                Context.SetIkTargetLocalPosition(newPos);
            }
            else {
                Vector3 newPos = Context.RightTargetOffsetPosition;
                newPos.y += _stepHeight;
                Context.SetIkTargetLocalPosition(newPos);
            }
        }

        public override void UpdateState()
        {
            _timer += Time.deltaTime;
        }

        public override void ExitState()
        {
            Debug.Log("EXIT RISE State");
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
