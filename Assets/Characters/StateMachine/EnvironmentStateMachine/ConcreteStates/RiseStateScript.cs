using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine.ConcreteStates {
    public class RiseStateScript : EnvironmentInteractionStateScript {
        public RiseStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions estate) : 
            base(context, estate) 
        { }

        private readonly float _stepHeight = 0.6f;
        private readonly float _riseTime = 0.2f;

        private float _timer;
        private Vector3 _startPos;
        private Vector3 _riseTarget;

        public override void EnterState()
        {
            Debug.Log("RISE State");
            
            _timer = 0f;
            _startPos = Context.CurrentIkConstraint.data.target.localPosition;

            _riseTarget = Context.CurrentStep == EnvironmentInteractionContextScript.EStep.Left
                ? Context.LeftTargetOffsetPosition
                : Context.RightTargetOffsetPosition;

            _riseTarget.y += _stepHeight;
        }
        
        public override void UpdateState()
        {
            _timer += Time.deltaTime;
            float t = Mathf.Clamp01(_timer / _riseTime);

            Vector3 currentPos = Vector3.Lerp(_startPos, _riseTarget, t);
            Context.SetIkTargetLocalPosition(currentPos);
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
