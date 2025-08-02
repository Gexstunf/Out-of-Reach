     using System;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine.ConcreteStates;
using Characters.SystemAdaptations.Utils;
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
        
        public PlayerStateContextScript Context { get; private set; }
        
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

            Context = new PlayerStateContextScript(_rb, _collider, _inputScript, _playerControllerScript);
            ValidateReferences();
            InitializeStates();
        }

        private void InitializeStates()
        {
            States.Add(EPlayerStates.Falling, new FallingStateScript(Context, EPlayerStates.Falling));
            States.Add(EPlayerStates.Jumping, new JumpingStateScript(Context, EPlayerStates.Jumping));
            States.Add(EPlayerStates.Walking, new WalkingStateScript(Context, EPlayerStates.Walking));
            States.Add(EPlayerStates.Idle, new IdleStateScript(Context, EPlayerStates.Idle));
            
            CurrentState = States[EPlayerStates.Idle];
        }

        private void ValidateReferences() {
            Assert.IsNotNull(_rb, "Rigidbody is not assigned!");
            Assert.IsNotNull(_collider, "Collider is not assigned!");
            Assert.IsNotNull(_inputScript, "Player-input-script is not assigned!");
        }


        
        public EPlayerStates StateKey => CurrentState.StateKey;
        
        public bool IsFalling => CurrentState.StateKey == EPlayerStates.Falling;
        public bool IsJumping => CurrentState.StateKey == EPlayerStates.Jumping;
        public bool IsRunning => CurrentState.StateKey == EPlayerStates.Running;
        public bool IsWalking => CurrentState.StateKey == EPlayerStates.Walking;
        public bool IsIdle => CurrentState.StateKey == EPlayerStates.Idle;
    }
}
