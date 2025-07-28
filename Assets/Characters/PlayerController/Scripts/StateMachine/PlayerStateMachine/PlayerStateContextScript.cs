


using UnityEngine;

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine
{
    public class PlayerStateContextScript
    {
        [SerializeField] Rigidbody _rigidbody;
        [SerializeField] CapsuleCollider _collider;

        public PlayerStateContextScript(Rigidbody rigidbody, CapsuleCollider collider)
        {
            _rigidbody = rigidbody;
            _collider = collider;
        }
        public Rigidbody Rb => _rigidbody;
        public CapsuleCollider Collider => _collider;
    }
}
