using UnityEngine;

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine.ConcreteStates
{
    public class WalkingStateScript : PlayerStateScript {
        
        public WalkingStateScript(PlayerStateContextScript context, PlayerStateMachineScript.EPlayerStates estate) : 
            base(context, estate) 
        { }

        public override void EnterState() {
            Debug.Log("Entering Walking State");
            Context.Coordinator.OnTiredChanged += Context.HandleTiredChange;
            Context.Coordinator.OnUnconsciousChanged += Context.HandleUnconsciousChange;
        }

        public override void ExitState() {
            //Debug.Log("Exiting Walking State");
            Context.Coordinator.OnTiredChanged -= Context.HandleTiredChange;
            Context.Coordinator.OnUnconsciousChanged -= Context.HandleUnconsciousChange;
        }

        public override void UpdateState() {
            
        }

        public override PlayerStateMachineScript.EPlayerStates GetNextState()
        {
            bool isMoving = Context.IsMovingLaterally();
            bool isGrounded = Context.PlayerController.isGrounded;

            if (!isGrounded) {
                bool isJumping = Context.IsJumping();
                return isJumping ? PlayerStateMachineScript.EPlayerStates.Jumping :
                    PlayerStateMachineScript.EPlayerStates.Falling;
            }

            if (!isMoving) {
                return PlayerStateMachineScript.EPlayerStates.Idle;
            }

            return StateKey;
        }

        public override void OnTriggerStay(Collider other)
        { }

        public override void OnTriggerExit(Collider other)
        { }

        public override void OnTriggerEnter(Collider other)
        { }
    }
}