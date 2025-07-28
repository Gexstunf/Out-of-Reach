using UnityEngine;

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine.ConcreteStates
{
    public class WalkingStateScript : PlayerStateScript
    {
        
        public WalkingStateScript(PlayerStateContextScript context, PlayerStateMachineScript.EPlayerStates estate) : 
            base(context, estate)
        {
            PlayerStateContextScript Context = context;
        }
        
        public override void EnterState()
        { }

        public override void ExitState()
        { }

        public override void UpdateState()
        { }

        public override PlayerStateMachineScript.EPlayerStates GetNextState()
        {
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