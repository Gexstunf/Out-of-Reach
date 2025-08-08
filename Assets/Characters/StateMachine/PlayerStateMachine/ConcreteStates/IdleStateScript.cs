using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using UnityEngine;

namespace Characters.StateMachine.PlayerStateMachine.ConcreteStates
{
    public class IdleStateScript : PlayerStateScript {
        
        // IMPORTANT: PLAYER STATE SCRIPT, HELPS WITH CHOOSING LOGIC FOR STATES
        public IdleStateScript(PlayerStateContextScript context, PlayerStateMachineScript.EPlayerStates estate) : 
            base(context, estate)
        { }

        public override void EnterState()
        {
            Debug.Log("Entering idle state");
            Context.Coordinator.OnTiredChanged += Context.HandleTiredChange;
            Context.Coordinator.OnUnconsciousChanged += Context.HandleUnconsciousChange;
        }

        public override void ExitState()
        {
            //Debug.Log("Exiting idle state");
            Context.Coordinator.OnTiredChanged -= Context.HandleTiredChange;
            Context.Coordinator.OnUnconsciousChanged -= Context.HandleUnconsciousChange;
        }

        public override void UpdateState()
        {
        }

        public override PlayerStateMachineScript.EPlayerStates GetNextState() {
            bool isMoving = IsMovingLaterally();
            bool isRunning = IsRunning();
            bool isGrounded = Context.PlayerController.isGrounded;
            
            if (!isGrounded) {
                var state = HandleJumpState();
                return state;
            }
            
            if (isMoving) {
                return isRunning ? PlayerStateMachineScript.EPlayerStates.Running : 
                        PlayerStateMachineScript.EPlayerStates.Walking;
            }
            
            return StateKey;
        }

        public override void OnTriggerStay(Collider other) {
            
        }

        public override void OnTriggerExit(Collider other) {
            
        }

        public override void OnTriggerEnter(Collider other) {
            
        }
    }
}