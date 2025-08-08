

using UnityEngine;

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine.ConcreteStates {
    public class RunningStateScript : PlayerStateScript {
        public RunningStateScript(PlayerStateContextScript context, PlayerStateMachineScript.EPlayerStates state) : base(context, state) 
        { }

        public override void EnterState() {
            Debug.Log("Entering running state");
            Context.Coordinator.OnTiredChanged += Context.HandleTiredChange;
        }

        public override void ExitState() {
            Context.Coordinator.OnTiredChanged -= Context.HandleTiredChange;
        }

        public override void UpdateState() {
            
        }

        public override PlayerStateMachineScript.EPlayerStates GetNextState() { 
            bool isMoving = Context.IsMovingLaterally();
            bool isRunning = Context.IsRunning();
            bool isGrounded = Context.PlayerController.isGrounded;
            
            if (!isGrounded) {
                var state = Context.HandleJumpState();
                return state;
            }

            if (!isMoving) {
                return PlayerStateMachineScript.EPlayerStates.Idle;
            }
            
            return isRunning && !Context.IsTired ? StateKey : PlayerStateMachineScript.EPlayerStates.Walking;
        }

        public override void OnTriggerStay(Collider other) {
            throw new System.NotImplementedException();
        }

        public override void OnTriggerExit(Collider other) {
            throw new System.NotImplementedException();
        }

        public override void OnTriggerEnter(Collider other) {
            throw new System.NotImplementedException();
        }
    }
}
