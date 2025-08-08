using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using UnityEngine;

namespace Characters.StateMachine.PlayerStateMachine.ConcreteStates
{
    // IMPORTANT: PLAYER STATE SCRIPT, HELPS WITH CHOOSING LOGIC FOR STATES
    public class JumpingStateScript : PlayerStateScript
    {
        
        public JumpingStateScript(PlayerStateContextScript context, PlayerStateMachineScript.EPlayerStates estate) : 
            base(context, estate) 
        { }

        public override void EnterState() {
            Debug.Log("Entering Jumping State");
            Context.Rb.linearDamping = 0f;
            Context.Input.enabled = false;
            Context.Coordinator.OnTiredChanged += Context.HandleTiredChange;
        }

        public override void ExitState() {
            //Debug.Log("Exiting Jumping State");
            Context.PlayerController.ResetVariables();;
            Context.Input.enabled = true;
            Context.Coordinator.OnTiredChanged -= Context.HandleTiredChange;
        }

        public override void UpdateState() {
            //Vector3 oppositeForce = -Context.PlayerController.CurrentForce;
            //Context.Rb.AddForce(oppositeForce);
        }

        public override PlayerStateMachineScript.EPlayerStates GetNextState()
        {
            bool isJumping = IsJumping();
            bool isGrounded = Context.PlayerController.isGrounded;

            if (isGrounded) {
                return PlayerStateMachineScript.EPlayerStates.Walking;
            }
            
            if (!isJumping) {
                return PlayerStateMachineScript.EPlayerStates.Falling;
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