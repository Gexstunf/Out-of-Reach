using UnityEngine;

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine.ConcreteStates.Utils {
    public class PlayerStateUtilsScript {
        
        // prolly not gon use this
        public bool IsMovingLaterally(Rigidbody rb) {
            Vector3 velocity = rb.linearVelocity;

            if (velocity != Vector3.zero) {
                return true;
            }
            
            return false;
        }
    }
}
