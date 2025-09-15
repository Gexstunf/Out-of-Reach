using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine.ConcreteStates {
    public class RiseStateScript : EnvironmentInteractionStateScript {
        public RiseStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions estate) : 
            base(context, estate) 
        { }

        private readonly float _stepHeight = 0.5f;
        private readonly float _riseTime = 0.2f;

        private float _timer;
        private float _forwardOffset; 
        private Vector3 _startPos;
        private Vector3 _riseTarget;
        private Vector3 _currentTargetOffsetPosition;

        public override void EnterState()
        {
            Debug.Log("Enter R State");

            _timer = 0f;
            _forwardOffset = 0.25f;
            _startPos = Context.CurrentIkConstraint.data.target.localPosition;
            Vector2 moveInput = Context.InputScript.MoveInput;

            _currentTargetOffsetPosition = Context.CurrentStep == EnvironmentInteractionContextScript.EStep.Left
                ? Context.LeftTargetOffsetPosition
                : Context.RightTargetOffsetPosition;
            
            _riseTarget = _currentTargetOffsetPosition;

            SetDynamicForwardOffset(Context.Rigidbody.linearVelocity);
            AdjustTargetWithMovement(moveInput);
            _riseTarget.y += _stepHeight;
        }
        
        public override void UpdateState()
        {

            _timer += Time.deltaTime;
            float t = Mathf.Clamp01(_timer / _riseTime);

            Vector3 currentPos = Vector3.Lerp(_startPos, _riseTarget, t);
            // only update if grounded
            if (Context.StateMachine.IsGrounded) Context.SetIkTargetLocalPosition(currentPos);
        }

        public override void ExitState()
        {
            Debug.Log("Exit R State");
        }


        public override EnvironmentInteractionStateMachineScript.EEnvironmentActions GetNextState()
        {
            if (_timer > _riseTime) return EnvironmentInteractionStateMachineScript.EEnvironmentActions.Touch;
            return StateKey;
        }


        public override void OnTriggerStay(Collider other) { }
        public override void OnTriggerExit(Collider other) { }
        public override void OnTriggerEnter(Collider other) { }

        private void SetDynamicForwardOffset(Vector3 speed) {
            float planeSpeed = new Vector2(speed.x, speed.z).magnitude;
            _forwardOffset =+ (planeSpeed * 0.2f);
        }
        
        private void AdjustTargetWithMovement(Vector3 moveInput) {
            if (!(moveInput.sqrMagnitude > 0.01f)) return;
            
            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            
            float maxForward = 0.8f; 
            Vector3 clampedTarget = _currentTargetOffsetPosition + moveDir * Mathf.Clamp(_forwardOffset, -maxForward, maxForward);
            _riseTarget = clampedTarget;
        }
    }
}
