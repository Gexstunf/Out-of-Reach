using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport {
    public class PlayerLifeSupportContextScript
    {
        [SerializeField] private readonly PlayerStateMachineScript _stateMachine;
        [SerializeField] private readonly Rigidbody _rb;
        [SerializeField] private readonly float _maxHealth;
        [SerializeField] private readonly float _maxStamina;
        [SerializeField] private readonly float _currentHealth;
        [SerializeField] private readonly float _currentStamina;
        [SerializeField] private UIManagerScript _uiManager;


        public PlayerLifeSupportContextScript(PlayerStateMachineScript stateMachine, Rigidbody rb, 
            float maxHealth, float maxStamina, UIManagerScript uiManager) {
            _stateMachine = stateMachine;
            _rb = rb;
            _maxHealth = maxHealth;
            _maxStamina = maxStamina;
            _uiManager = uiManager;
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
