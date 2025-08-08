using Characters.PlayerController.Scripts.StateMachine;
using UnityEngine;

namespace Characters.StateMachine.PlayerStateMachine
{
    public abstract class PlayerStateScript : BaseStateScript<PlayerStateMachineScript.EPlayerStates>
    {
        protected PlayerStateContextScript Context;

        public PlayerStateScript(PlayerStateContextScript context, PlayerStateMachineScript.EPlayerStates state) : base(state)
        {
            Context = context;
        }
        
        public bool IsMovingLaterally() {
            Vector3 rawVelocity = Context.Rb.linearVelocity;
            Vector2 velocity2d = new Vector2(rawVelocity.x, rawVelocity.z);

            if (velocity2d.magnitude > 0.1f) {
                return true;
            }
            return false;
        }
        
        public PlayerStateMachineScript.EPlayerStates HandleJumpState() {
            bool jumping = IsJumping();
            return jumping ? PlayerStateMachineScript.EPlayerStates.Jumping :
                PlayerStateMachineScript.EPlayerStates.Falling;
        }

        public bool IsJumping() {
            return  Context.Rb.linearVelocity.y > 0f;
        }

        public bool IsRunning() {
            return Context.Input.RunningPressed;
        }
    }
}
