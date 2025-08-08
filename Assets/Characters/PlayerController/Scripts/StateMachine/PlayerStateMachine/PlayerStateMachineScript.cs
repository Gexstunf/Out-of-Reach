     using System;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine.ConcreteStates;
using Characters.SystemAdaptations;
using Characters.SystemAdaptations.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine
{
    [RequireComponent(typeof(PlayerInputScript))]
    [RequireComponent(typeof(StateVitalsCoordinator))]
    public class PlayerStateMachineScript : StateManagerScript<PlayerStateMachineScript.EPlayerStates>
    {

        [Header("Player State References")] 
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PlayerInputScript _inputScript;
        [SerializeField] private PlayerControllerScript _playerControllerScript;
        [SerializeField] private CapsuleCollider _collider;
        [SerializeField] private StateVitalsCoordinator _coordinator;
        public PlayerStateContextScript Context { get; private set; }
        
        public enum EPlayerStates
        {
            Falling,
            Jumping,
            Running,
            Walking,
            Idle,
            Climbing,
        }
        
        private void Awake() {
            _collider = GetComponent<CapsuleCollider>();
            _playerControllerScript = GetComponent<PlayerControllerScript>();
            _coordinator = GetComponent<StateVitalsCoordinator>();
            _inputScript = GetComponent<PlayerInputScript>();
            _rb = GetComponent<Rigidbody>();

            Context = new PlayerStateContextScript(_rb, _collider, _inputScript, _playerControllerScript, _coordinator);
            ValidateReferences();
            InitializeStates();
        }

        private void InitializeStates()
        {
            States.Add(EPlayerStates.Falling, new FallingStateScript(Context, EPlayerStates.Falling));
            States.Add(EPlayerStates.Jumping, new JumpingStateScript(Context, EPlayerStates.Jumping));
            States.Add(EPlayerStates.Walking, new WalkingStateScript(Context, EPlayerStates.Walking));
            States.Add(EPlayerStates.Idle, new IdleStateScript(Context, EPlayerStates.Idle));
            States.Add(EPlayerStates.Running, new RunningStateScript(Context, EPlayerStates.Running));
            
            CurrentState = States[EPlayerStates.Idle];
        }

        private void ValidateReferences() {
            Assert.IsNotNull(_rb, "Rigidbody is not assigned!");
            Assert.IsNotNull(_collider, "Collider is not assigned!");
            Assert.IsNotNull(_inputScript, "Player-input-script is not assigned!");
            Assert.IsNotNull(_playerControllerScript, "Player-controller-script is not assigned!");
            Assert.IsNotNull(_coordinator, "Coordinator is not assigned!");
        }


        
        public EPlayerStates StateKey => CurrentState.StateKey;
        
        public bool IsFalling => StateKey == EPlayerStates.Falling;
        public bool IsJumping => StateKey == EPlayerStates.Jumping;
        public bool IsRunning => StateKey == EPlayerStates.Running;
        public bool IsWalking => StateKey == EPlayerStates.Walking;
        public bool IsIdle => StateKey == EPlayerStates.Idle;
        
        public bool IsMoving => StateKey == EPlayerStates.Running ||
                                StateKey == EPlayerStates.Walking ||
                                StateKey == EPlayerStates.Climbing || 
                                StateKey == EPlayerStates.Falling;
    }
}
