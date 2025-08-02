
using Characters.PlayerController.Scripts.Input;
using Characters.SystemAdaptations.Utils;
using UnityEngine;

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine
{
    public class PlayerStateContextScript : IVitalStates 
    {
        [SerializeField] Rigidbody _rigidbody;
        [SerializeField] CapsuleCollider _collider;
        [SerializeField] PlayerInputScript _inputScript;
        [SerializeField] PlayerControllerScript _playerController;
        
        [Header("Vitals States")]
        public bool IsUnconscious { get; private set; }
        public bool IsTired { get; private set; }
        public bool IsHeavy { get; private set; }
        public bool IsStarved { get; private set; }
        
        private float _movementThreshold = 0.1f;

        public PlayerStateContextScript(Rigidbody rigidbody, CapsuleCollider collider, 
            PlayerInputScript inputScript, PlayerControllerScript playerController)
        {
            _rigidbody = rigidbody;
            _collider = collider;
            _inputScript = inputScript;
            _playerController = playerController;
        }
        
        public Rigidbody Rb => _rigidbody;
        public CapsuleCollider Collider => _collider;
        public PlayerInputScript Input => _inputScript;
        public PlayerControllerScript PlayerController => _playerController;
        
        public bool IsMovingLaterally() {
            Vector3 rawVelocity = _rigidbody.linearVelocity;
            Vector2 velocity = new Vector2(rawVelocity.x, rawVelocity.z);

            if (velocity.magnitude > _movementThreshold) {
                return true;
            }
            return false;
        }

        public void SetVitalStates(VitalsStructScript vitals) {
            IsUnconscious = vitals.IsUnconscious;
            IsTired = vitals.IsTired;
            IsHeavy = vitals.IsHeavy;
            IsStarved = vitals.IsStarved;
        }

        public bool IsJumping() {
            return _rigidbody.linearVelocity.y > 0f;
        }
    }
}
