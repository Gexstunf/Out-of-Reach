using Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals;
using Characters.PlayerController.Scripts.Input;
using UI.Scripts.TestingUI;
using UnityEngine;
using UnityEngine.Serialization;
using Assert = NUnit.Framework.Assert;

namespace Characters.LifeSupportSystem.PlayerLifeSupport {
    [RequireComponent(typeof(PlayerInputScript))]
    public class PlayerLifeSupportScript : LifeSupportManagerScript<PlayerLifeSupportScript.EVitals> {
        
        [Header("References")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private UIManagerScript _uiManager;
        [SerializeField] private PlayerInputScript _playerInputScript;
        
        [Header("Life support settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _maxStamina = 100f;
        
        [Header("Stamina settings")]
        [SerializeField] private float _staminaUseRate = 5f;
        [SerializeField] private float _staminaRegenRate = 2f;
        [SerializeField] private float _staminaRegenDelay = 5f;

        public PlayerLifeSupportContextScript Context { get; private set; }
        
        public enum EVitals
        {
            Weight,
            Health,
            Stamina,
            Hunger
        }
        
        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            Context = new PlayerLifeSupportContextScript(_rb, _maxHealth, _maxStamina, _staminaUseRate, 
                _staminaRegenRate, _staminaRegenDelay, _uiManager, _playerInputScript);
            
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
            Assert.IsNotNull(_rb, "Rigidbody is not assigned.");
            Assert.IsNotNull(_uiManager, "UIManager is not assigned.");
        }
    }
}