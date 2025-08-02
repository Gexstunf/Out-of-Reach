using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations.Utils;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport {
    public class PlayerLifeSupportContextScript : IMovementStates
    {
        [SerializeField] private readonly PlayerStateMachineScript _stateMachine;
        [SerializeField] private readonly Rigidbody _rb;
        [SerializeField] private readonly float _maxHealth;
        [SerializeField] private readonly float _maxStamina;
        [SerializeField] private readonly float _currentHealth;
        [SerializeField] private readonly float _currentStamina;
        [SerializeField] private UIManagerScript _uiManager;

        [Header("Movement states")] 
        public bool IsWalking { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsClimbing { get; private set; }
        public bool IsIdle { get; private set; }
        
        public PlayerLifeSupportContextScript(PlayerStateMachineScript stateMachine, Rigidbody rb, 
            float maxHealth, float maxStamina, UIManagerScript uiManager) {
            _stateMachine = stateMachine;
            _rb = rb;
            _maxHealth = maxHealth;
            _maxStamina = maxStamina;
            _uiManager = uiManager;
        }
        
        public void SetMovementStates(MovementStatesStructScript states) {
            IsWalking = states.IsWalking;
            IsJumping = states.IsJumping;
            IsClimbing = states.IsClimbing;
            IsRunning = states.IsRunning;
            IsIdle = states.IsIdle;
        }
        
        public PlayerStateMachineScript StateMachine => _stateMachine;
        public Rigidbody Rb => _rb;
        public UIManagerScript UIManager => _uiManager;
        
        public float MaxHealth => _maxHealth;
        public float MaxStamina => _maxStamina;
        public float Health => _currentHealth;
        public float Stamina => _currentStamina;
    }
}
