using Characters.PlayerController.Scripts;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine;
using Characters.StateMachine.EnvironmentStateMachine;
using Characters.StateMachine.PlayerStateMachine.ConcreteStates;
using Characters.StateMachine.PlayerStateMachine.z;
using Characters.SystemAdaptations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Characters.StateMachine.PlayerStateMachine
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
        [SerializeField] private InverseKinematicsDriverScript _inverseKinematicsDriver;
        [SerializeField] private EnvironmentInteractionStateMachineScript _enviromentInteractionStateMachine;
        
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
            _enviromentInteractionStateMachine = GetComponent<EnvironmentInteractionStateMachineScript>();

            Context = new PlayerStateContextScript(_rb, _collider, _inputScript, _playerControllerScript, _coordinator, _enviromentInteractionStateMachine);
            ValidateReferences();
            InitializeStates();
        }
        
        private void ValidateReferences() {
            Assert.IsNotNull(_rb, "Rigidbody is not assigned!");
            Assert.IsNotNull(_collider, "Collider is not assigned!");
            Assert.IsNotNull(_inputScript, "Player-input-script is not assigned!");
            Assert.IsNotNull(_playerControllerScript, "Player-controller-script is not assigned!");
            Assert.IsNotNull(_coordinator, "Coordinator is not assigned!");
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
        
        public EPlayerStates StateKey => CurrentState.StateKey;
        
        # region  Exposed vars
        public bool IsFalling => StateKey == EPlayerStates.Falling;
        public bool IsJumping => StateKey == EPlayerStates.Jumping;
        public bool IsRunning => StateKey == EPlayerStates.Running;
        public bool IsWalking => StateKey == EPlayerStates.Walking;
        public bool IsIdle => StateKey == EPlayerStates.Idle;
        
        public bool IsGrounded => StateKey == EPlayerStates.Running || 
                                  StateKey == EPlayerStates.Walking ||
                                  StateKey == EPlayerStates.Idle;
        
        public bool IsMoving => StateKey == EPlayerStates.Running ||
                                StateKey == EPlayerStates.Walking ||
                                StateKey == EPlayerStates.Climbing || 
                                StateKey == EPlayerStates.Falling;
        
        # endregion
    }
}
