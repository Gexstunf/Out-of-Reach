using Characters.PlayerController.Scripts;
using Characters.PlayerController.Scripts.Input;
using Characters.StateMachine.EnvironmentStateMachine;
using Characters.SystemAdaptations;
using Characters.SystemAdaptations.Utils;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Characters.StateMachine.PlayerStateMachine {
    public class PlayerStateContextScript : IVitalStates {

        [SerializeField] Rigidbody _rigidbody;
        [SerializeField] CapsuleCollider _collider;
        [SerializeField] PlayerInputScript _inputScript;
        [SerializeField] PlayerControllerScript _playerController;
        [SerializeField] StateVitalsCoordinator _coordinator;
        [SerializeField] EnvironmentInteractionStateMachineScript _envInteractionStateMachine;

        [Header("Vitals States")] public bool IsUnconscious { get; private set; }
        public bool IsTired { get; private set; }
        public bool IsHeavy { get; private set; }
        public bool IsStarved { get; private set; }

        
        private float _movementThreshold = 0.1f;

        public PlayerStateContextScript(Rigidbody rigidbody, CapsuleCollider collider,
            PlayerInputScript inputScript, PlayerControllerScript playerController,
            StateVitalsCoordinator coordinator, EnvironmentInteractionStateMachineScript envStateMachine
        ) {
            _rigidbody = rigidbody;
            _collider = collider;
            _inputScript = inputScript;
            _playerController = playerController;
            _coordinator = coordinator;
            _envInteractionStateMachine = envStateMachine;
        }

        public Rigidbody Rb => _rigidbody;
        public CapsuleCollider Collider => _collider;
        public PlayerInputScript Input => _inputScript;
        public PlayerControllerScript PlayerController => _playerController;
        public StateVitalsCoordinator Coordinator => _coordinator;
        public EnvironmentInteractionStateMachineScript EnvironmentInteractionStateMachine => _envInteractionStateMachine;
        public float MovementThreshold => _movementThreshold;

        public void HandleTiredChange(bool isTired) {
            IsTired = isTired;
            Debug.Log("Changing tired state to: " + isTired);
        }
        
        public void HandleUnconsciousChange(bool isUnconscious) {
            IsUnconscious = isUnconscious;
            _inputScript.enabled = !isUnconscious;
            Debug.Log("Changing unconscious state to: " + isUnconscious);
        }
    }
}
