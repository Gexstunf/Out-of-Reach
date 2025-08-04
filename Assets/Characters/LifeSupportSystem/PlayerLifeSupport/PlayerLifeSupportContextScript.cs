using Characters.PlayerController.Scripts;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations.Utils;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport {
    public class PlayerLifeSupportContextScript : IMovementStates
    {
        [SerializeField] private readonly Rigidbody _rb;
        [SerializeField] private UIManagerScript _uiManager;
        [SerializeField] private PlayerInputScript _playerInputScript;
        [SerializeField] private readonly float _maxHealth;
        [SerializeField] private readonly float _maxStamina;
        [SerializeField] private readonly float _currentHealth;
        [SerializeField] private readonly float _currentStamina;

        [Header("Movement states")] 
        public bool IsWalking { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsClimbing { get; private set; }
        public bool IsIdle { get; private set; }
        
        public bool IsMoving { get; private set; }
        
        public PlayerLifeSupportContextScript(Rigidbody rb, float maxHealth, float maxStamina, 
            UIManagerScript uiManager, PlayerInputScript playerInputScript) 
        {
            _rb = rb;
            _maxHealth = maxHealth;
            _maxStamina = maxStamina;
            _uiManager = uiManager;
            _playerInputScript = playerInputScript;
        }
        
        public void SetMovementStates(MovementStatesStructScript states) {
            IsWalking = states.IsWalking;
            IsJumping = states.IsJumping;
            IsClimbing = states.IsClimbing;
            IsRunning = states.IsRunning;
            IsIdle = states.IsIdle;
            IsMoving = states.IsMoving;
        }

        public bool IsStaminaRequired() => IsRunning || IsClimbing || IsJumping || IsWalking;
        public void ClampVital(ref float value, float max) => value = Mathf.Clamp(value, 0, max);
        public Rigidbody Rb => _rb;
        public UIManagerScript UIManager => _uiManager;
        public PlayerInputScript PlayerInputScript => _playerInputScript;
        
        public float MaxHealth => _maxHealth;
        public float MaxStamina => _maxStamina;
        public float Health => _currentHealth;
        public float Stamina => _currentStamina;
        

        public float StaminaUseRate => 5f;
        public float StaminaRegenRate => 2f;
        public float StaminaRegenDelay => 4f;
        
        public float JumpStaminaUse => 10f;
    }
}
