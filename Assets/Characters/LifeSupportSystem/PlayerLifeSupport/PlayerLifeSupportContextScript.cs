using Characters.PlayerController.Scripts;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations.Utils;
using UI.Scripts.TestingUI;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport {
    public class PlayerLifeSupportContextScript : IMovementStates
    {
        [SerializeField] private readonly Rigidbody _rb;
        [SerializeField] private UIManagerScript _uiManager;
        [SerializeField] private PlayerInputScript _playerInputScript;
        [SerializeField] private readonly float _maxHealth;
        [SerializeField] private readonly float _maxStamina;
        
        [SerializeField] private readonly float _staminaUseRate;
        [SerializeField] private readonly float _staminaRegenRate;
        [SerializeField] private readonly float _staminaRegenDelay;

        [SerializeField] private float _currentHealth;
        [SerializeField] private float _currentStamina;
        
        [Header("DEBUG ONLY LOGIC")]
        

        [Header("Movement states")] 
        public bool IsWalking { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsFalling { get; private set;}
        public bool IsClimbing { get; private set; }
        public bool IsIdle { get; private set; }
        
        public bool IsMoving { get; private set; }
        
        public PlayerLifeSupportContextScript(Rigidbody rb, float maxHealth, float maxStamina, float stamUseRate, float stamRegenRate, float stamRegenDelay, 
            UIManagerScript uiManager, PlayerInputScript playerInputScript) 
        {
            _rb = rb;
            _maxHealth = maxHealth;
            _maxStamina = maxStamina;
            _uiManager = uiManager;
            _playerInputScript = playerInputScript;
            _staminaUseRate = stamUseRate;
            _staminaRegenRate = stamRegenRate;
            _staminaRegenDelay = stamRegenDelay;
        }
        
        public void SetMovementStates(MovementStatesStructScript states) {
            IsWalking = states.IsWalking;
            IsJumping = states.IsJumping;
            IsClimbing = states.IsClimbing;
            IsRunning = states.IsRunning;
            IsIdle = states.IsIdle;
            IsMoving = states.IsMoving;
            IsFalling = states.IsFalling;
        }

        public bool IsStaminaRequired() => IsRunning || IsClimbing || IsJumping;
        public Rigidbody Rb => _rb;
        public UIManagerScript UIManager => _uiManager;
        public PlayerInputScript PlayerInputScript => _playerInputScript;
        
        public float MaxHealth => _maxHealth;
        public float MaxStamina => _maxStamina;
        public float Health => _currentHealth;
        public float Stamina => _currentStamina;
        

        public float StaminaUseRate => _staminaUseRate;
        public float StaminaRegenRate => _staminaRegenRate;
        public float StaminaRegenDelay => _staminaRegenDelay;
        
        public float HealthRegenRate => 2f;
        public float HealthRegenDelay => 4f;
        
        public float JumpStaminaUse => 10f;

        public bool IsTired { get; private set; }
        public bool IsHeavy { get; private set; }
        public bool IsStarved { get; private set; }
        public bool IsUnconscious { get; private set; }


        public void SetTired(bool value) {
            IsTired = value;
        }
        public void SetHeavy(bool value) {
            IsHeavy = value;
        }
        public void SetStarved(bool value) {
            IsStarved = value;
        }
        public void SetUnconscious(bool value) {
            IsUnconscious = value;
        }


        public void SetHealth(float value) {
            _currentHealth = value;
        }
        public void SetStamina(float value) {
            _currentStamina = value;
        }
    }
}
