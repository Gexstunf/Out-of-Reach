

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine
{
    public abstract class PlayerStateScript : BaseStateScript<PlayerStateMachineScript.EPlayerStates>
    {
        protected PlayerStateContextScript Context;

        public PlayerStateScript(PlayerStateContextScript context, PlayerStateMachineScript.EPlayerStates state) : base(state)
        {
            Context = context;
        }
    }
}
