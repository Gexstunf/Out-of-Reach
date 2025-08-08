using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine.ConcreteStates {
    public class TouchStateScript : EnvironmentInteractionStateScript
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public TouchStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions estate) : 
            base(context, estate) 
        { }

        public override void EnterState() {
            throw new System.NotImplementedException();
        }

        public override void ExitState() {
            throw new System.NotImplementedException();
        }

        public override void UpdateState() {
            throw new System.NotImplementedException();
        }

        public override EnvironmentInteractionStateMachineScript.EEnvironmentActions GetNextState() {
            throw new System.NotImplementedException();
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
