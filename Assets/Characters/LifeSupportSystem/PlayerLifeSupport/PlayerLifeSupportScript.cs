using Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals;
using Characters.PlayerController.Scripts.Input;
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
        [SerializeField] private PlayerInventoryPhoton _inventory;

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

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            Context = new PlayerLifeSupportContextScript(
                _rb, _maxHealth, _maxStamina, _staminaUseRate,
                _staminaRegenRate, _staminaRegenDelay, null, _playerInputScript
            );

            Debug.Log("[PlayerLifeSupportScript] Awake completed. UIManager null: " + (_uiManager == null));
        }

        private new void Start()
        {
            if (photonView.IsMine)
            {
                _uiManager = GetComponentInChildren<PlayerUIManager>(true);

                if (_uiManager != null)
                {
                    Debug.Log("[PlayerLifeSupportScript] UIManager found in Start(): " + _uiManager.name);
                    Context.SetUIManager(_uiManager);
                    _uiManager.SetTarget(Context, Vitals);
                    if (_inventory != null)
                        _uiManager.InitInventory(_inventory);
                }
                else
                {
                    Debug.LogWarning("[PlayerLifeSupportScript] UIManager no encontrado en Start()");
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

        private void Update()
        {
            if (!photonView.IsMine) return;

            foreach (var vital in Vitals.Values)
                vital.UpdateModifiers();

            foreach (var vital in Vitals.Values)
                vital.UpdateVital();

            if (_uiManager != null)
            {
                _uiManager.DisplayStamina(Context.Stamina);
                _uiManager.DisplayHealth(Context.Health);
            }
        }

        private void InitializeVitals()
        {
            Vitals.Add(EVitals.Weight, new WeightVitalScript(Context, EVitals.Weight));
            Vitals.Add(EVitals.Stamina, new StaminaVitalScript(Context, EVitals.Stamina));
            Vitals.Add(EVitals.Hunger, new HungerVitalScript(Context, EVitals.Hunger));
            Vitals.Add(EVitals.Health, new HealthVitalScript(Context, EVitals.Health));

            // ⚡ Setup inmediatamente
            foreach (var vital in Vitals.Values)
                vital.SetupVital();

            Debug.Log("[PlayerLifeSupportScript] Vitals initialized and setup: " + Vitals.Count);
        }

        private void ValidateReferences()
        {
            Debug.Assert(_rb != null, "Rigidbody is not assigned.");
            Debug.Assert(_uiManager != null, "UI Manager is not assigned.");
            Debug.Assert(_playerInputScript != null, "PlayerInputScript is not assigned.");

            if (_uiManager == null)
                Debug.LogError("❌ No se encontró PlayerUIManager/UIManagerScript en " + gameObject.name);
            else
                Debug.Log("✅ PlayerUIManager/UIManagerScript encontrado en " + gameObject.name);
        }
    }
}
