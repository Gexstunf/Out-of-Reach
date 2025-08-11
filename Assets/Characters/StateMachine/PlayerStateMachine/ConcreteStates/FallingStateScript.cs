using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using UnityEngine;

namespace Characters.StateMachine.PlayerStateMachine.ConcreteStates
{
    public class FallingStateScript : PlayerStateScript
    {
        
        public FallingStateScript(PlayerStateContextScript context, PlayerStateMachineScript.EPlayerStates estate) : 
            base(context, estate)
        { }

        public override void EnterState() {
            //Debug.Log("Entering Falling state");
            Context.Input.enabled = false;
            Context.Rb.linearDamping = 0f;
            Context.Coordinator.OnUnconsciousChanged += Context.HandleUnconsciousChange;
        }

        public override void ExitState() {
            //Debug.Log("Exiting Falling state");
            Context.PlayerController.ResetVariables();
            Context.Input.enabled = true;
            Context.Coordinator.OnUnconsciousChanged -= Context.HandleUnconsciousChange;
        }

        public override void UpdateState() {
            //Vector3 oppositeForce = -Context.PlayerController.CurrentForce;
            //Context.Rb.AddForce(oppositeForce);
        }

        public override PlayerStateMachineScript.EPlayerStates GetNextState() {
            bool isGrounded = Context.PlayerController.isGrounded;
            
            if (isGrounded) {
                Debug.Log("Now grounded!!!");
                return PlayerStateMachineScript.EPlayerStates.Walking;
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
