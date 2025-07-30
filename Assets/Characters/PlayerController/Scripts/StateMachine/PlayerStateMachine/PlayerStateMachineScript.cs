using System;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine.ConcreteStates;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine
{
    public class PlayerStateMachineScript : StateManagerScript<PlayerStateMachineScript.EPlayerStates>
    {

        [Header("Player State References")] 
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PlayerInputScript _inputScript;
        [SerializeField] private PlayerControllerScript _playerControllerScript;
        [SerializeField] private CapsuleCollider _collider;
        
        
        private PlayerStateContextScript _context;
        
        public enum EPlayerStates
        {
            Falling,
            Jumping,
            Running,
            Walking,
            Idle,
        }
        
        private void Awake() {
            _collider = GetComponent<CapsuleCollider>();
            _playerControllerScript = GetComponent<PlayerControllerScript>();

            _context = new PlayerStateContextScript(_rb, _collider, _inputScript, _playerControllerScript);
            ValidateReferences();
            InitializeStates();
        }

        private void InitializeStates()
        {
            States.Add(EPlayerStates.Falling, new FallingStateScript(_context, EPlayerStates.Falling));
            States.Add(EPlayerStates.Jumping, new JumpingStateScript(_context, EPlayerStates.Jumping));
            States.Add(EPlayerStates.Walking, new WalkingStateScript(_context, EPlayerStates.Walking));
            States.Add(EPlayerStates.Idle, new IdleStateScript(_context, EPlayerStates.Idle));
            
            CurrentState = States[EPlayerStates.Idle];
        }

        private void ValidateReferences() {
            Assert.IsNotNull(_rb, "Rigidbody is not assigned!");
            Assert.IsNotNull(_collider, "Collider is not assigned!");
            Assert.IsNotNull(_inputScript, "Player-input-script is not assigned!");
        }
    }
}
