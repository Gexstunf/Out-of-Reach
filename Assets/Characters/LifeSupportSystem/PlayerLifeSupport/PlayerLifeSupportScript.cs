using Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals;
using Characters.PlayerController.Scripts.Input;
using UI.Scripts.TestingUI;
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

        [Header("Life support settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _maxStamina = 100f;

        // Context accesible por otros sistemas (Coordinator, StateMachine, etc.)
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
            if (!photonView.IsMine) return;

            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_playerInputScript == null) _playerInputScript = GetComponent<PlayerInputScript>();
            if (_uiManager == null) _uiManager = GetComponentInChildren<PlayerUIManager>();

            Context = new PlayerLifeSupportContextScript(
                _rb, _maxHealth, _maxStamina, _uiManager, _playerInputScript
            );

            ValidateReferences();
            InitializeVitals();
        }

        private void Start()
        {
            // Inicializamos cada vital después de crearlos
            foreach (var vital in Vitals.Values)
            {
                vital.SetupVital();
            }
        }

        private void Update()
        {
            // Actualizamos modificadores primero (ej: running, jumping, falling)
            foreach (var vital in Vitals.Values)
            {
                vital.UpdateModifiers();
            }

            // Después actualizamos el valor real
            foreach (var vital in Vitals.Values)
            {
                vital.UpdateVital();
            }
        }

        private void InitializeVitals()
        {
            // El orden importa: algunos dependen de otros
            Vitals.Add(EVitals.Weight, new WeightVitalScript(Context, EVitals.Weight));
            Vitals.Add(EVitals.Stamina, new StaminaVitalScript(Context, EVitals.Stamina));
            Vitals.Add(EVitals.Hunger, new HungerVitalScript(Context, EVitals.Hunger));
            Vitals.Add(EVitals.Health, new HealthVitalScript(Context, EVitals.Health));
        }

        private void ValidateReferences()
        {
            Debug.Assert(_rb != null, "Rigidbody is not assigned.");
            Debug.Assert(_uiManager != null, "UIManager is not assigned.");
            Debug.Assert(_playerInputScript != null, "PlayerInputScript is not assigned.");

            if (_uiManager == null)
            {
                Debug.LogWarning("⚠ PlayerUIManager no asignado, la UI no se actualizará.");
            }
        }
    }
}
