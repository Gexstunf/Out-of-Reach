using Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals;
using Characters.PlayerController.Scripts.Input;
using UI.Scripts.TestingUI;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport {
    [RequireComponent(typeof(PlayerInputScript))]
    public class PlayerLifeSupportScript : LifeSupportManagerScript<PlayerLifeSupportScript.EVitals> {
        
        [Header("References")]
        [SerializeField] private PlayerUIManager _uiManager;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PlayerInputScript _playerInputScript;
        
        [Header("Life support settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _maxStamina = 100f;
        
        public PlayerLifeSupportContextScript Context { get; private set; }
        
        public enum EVitals {
            Weight,
            Health,
            Stamina,
            Hunger
        }
        
        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            Context = new PlayerLifeSupportContextScript(_rb, _maxHealth, _maxStamina, _uiManager, _playerInputScript);
            
            ValidateReferences();
            InitializeVitals();
        }

        private void InitializeVitals() {
            // order *should* matter, modifiers will probably depend on each vital
            Vitals.Add(EVitals.Weight, new WeightVitalScript(Context, EVitals.Weight));
            Vitals.Add(EVitals.Stamina, new StaminaVitalScript(Context, EVitals.Stamina));
            Vitals.Add(EVitals.Hunger, new HungerVitalScript(Context, EVitals.Hunger));
            Vitals.Add(EVitals.Health, new HealthVitalScript(Context, EVitals.Health));
        }

        private void ValidateReferences() {
            Debug.Assert(_rb != null, "Rigidbody is not assigned.");
            Debug.Assert(_uiManager != null, "UIManager is not assigned.");
            Debug.Assert(_playerInputScript != null, "PlayerInputScript is not assigned.");
        }
    }
}
