using Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals;
using Characters.PlayerController.Scripts.Input;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

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

        public PlayerLifeSupportContextScript Context { get; private set; }

        public enum EVitals { Weight, Health, Stamina, Hunger }

        private PhotonView photonView;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _playerInputScript = GetComponent<PlayerInputScript>();
            _uiManager = GetComponentInChildren<PlayerUIManager>();

            if (!photonView.IsMine)
            {
                // Jugadores remotos
                if (_rb != null) _rb.isKinematic = true;
                if (_uiManager != null) _uiManager.gameObject.SetActive(false);
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) cam.gameObject.SetActive(false);
                return;
            }

            // Jugador local
            if (_uiManager != null) _uiManager.gameObject.SetActive(true);
        }

        private void Start()
        {
            if (!photonView.IsMine) return;

            // Inicializar Context y vitals solo para jugador local
            Context = new PlayerLifeSupportContextScript(_rb, _maxHealth, _maxStamina, _uiManager, _playerInputScript);
            InitializeVitals();

            if (_uiManager != null)
            {
                PlayerInventoryPhoton inventory = GetComponent<PlayerInventoryPhoton>();
                if (inventory != null) _uiManager.InitInventory(inventory);
            }
        }

        private void InitializeVitals()
        {
            Vitals.Add(EVitals.Weight, new WeightVitalScript(Context, EVitals.Weight));
            Vitals.Add(EVitals.Stamina, new StaminaVitalScript(Context, EVitals.Stamina));
            Vitals.Add(EVitals.Hunger, new HungerVitalScript(Context, EVitals.Hunger));
            Vitals.Add(EVitals.Health, new HealthVitalScript(Context, EVitals.Health));
        }
    }
}
