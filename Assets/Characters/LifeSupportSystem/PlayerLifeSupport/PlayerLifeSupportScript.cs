using Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.Inventory;
using Multiplayer.UI;
using UI;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport
{
    [RequireComponent(typeof(PlayerInputScript))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerLifeSupportScript : LifeSupportManagerScript<PlayerLifeSupportScript.EVitals>
    {
        [Header("References")]
        [SerializeField] private PlayerUIManager _uiManager;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PlayerInputScript _playerInputScript;
        [SerializeField] private InventoryControllerScript _inventory;

        [Header("Life support settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _maxStamina = 100f;
        
        public float MaxHealth => _maxHealth;
        public float MaxStamina => _maxStamina;

        [Header("Stamina settings")]
        [SerializeField] private float _staminaUseRate = 5f;
        [SerializeField] private float _staminaRegenRate = 2f;
        [SerializeField] private float _staminaRegenDelay = 5f;

        private bool _isLocalPlayer;
        public PlayerLifeSupportContextScript Context { get; private set; }

        public enum EVitals
        {
            Weight,
            Health,
            Stamina,
            Hunger
        }

        private void Awake()
        {
            _rb =  GetComponent<Rigidbody>();
            _playerInputScript = GetComponent<PlayerInputScript>();
            _inventory = GetComponent<InventoryControllerScript>();
            _uiManager = GetComponent<PlayerUIManager>();

            Context = new PlayerLifeSupportContextScript(
                _rb, _maxHealth, _maxStamina, _staminaUseRate,
                _staminaRegenRate, _staminaRegenDelay, _uiManager, _playerInputScript
            );
        }

        public void Initialize(bool isLocalPlayer)
        {
            _isLocalPlayer = isLocalPlayer;

            if (isLocalPlayer)
            {
                _uiManager = GetComponent<PlayerUIManager>();
                if (_uiManager != null)
                {
                    Context.SetUIManager(_uiManager);
                    _uiManager.DisplayHealth(_maxHealth);
                    _uiManager.DisplayStamina(_maxStamina);
                }
            }
            else
            {
                var uiCanvas = GetComponentInChildren<Canvas>(true);
                if (uiCanvas != null)
                    uiCanvas.gameObject.SetActive(false);
            }

            InitializeVitals();
            ValidateReferences();
        }

        private void InitializeVitals()
        {
            Vitals.Add(EVitals.Weight, new WeightVitalScript(Context, EVitals.Weight));
            Vitals.Add(EVitals.Stamina, new StaminaVitalScript(Context, EVitals.Stamina));
            Vitals.Add(EVitals.Hunger, new HungerVitalScript(Context, EVitals.Hunger));
            Vitals.Add(EVitals.Health, new HealthVitalScript(Context, EVitals.Health));

            // Setup inmediatamente
            foreach (var vital in Vitals.Values)
                vital.SetupVital();

            Debug.Log("[PlayerLifeSupportScript] Vitals initialized and setup: " + Vitals.Count);
        }

        private void ValidateReferences()
        {
            Debug.Assert(_rb != null, "Rigidbody is not assigned. " + gameObject.name);
            Debug.Assert(_uiManager != null, "UI Manager is not assigned. " + gameObject.name);
            Debug.Assert(_playerInputScript != null, "PlayerInputScript is not assigned. " + gameObject.name);
        }
    }
}
