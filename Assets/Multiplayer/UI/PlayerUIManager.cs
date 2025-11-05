using System.Collections.Generic;
using Characters.LifeSupportSystem.PlayerLifeSupport;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.Inventory;
using GlobalUtils;
using Multiplayer.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.UI {
    public class PlayerUIManager : MonoBehaviour
    {
        #region Variables
    
        [Header("References")]
        [SerializeField] private InventoryControllerScript _inventoryController;
        [SerializeField] private PlayerLifeSupportScript _playerLifeSupportScript;
        [SerializeField] private PlayerInputScript _playerInputScript;
    
        [Header("Vida")]
        public Sprite tickOn;
        public Sprite tickYellow;
        public Sprite tickRed;
        public float healthPerTick = 10f;
        private List<Image> _healthTicks = new List<Image>();

        [Header("UI Settings")] 
        public Transform healthContainer;
        public Image staminaBar;
        public UiSlot[] slots;

        [System.Serializable]
        public class UiSlot {
            public GameObject Obj;
            public Image BgImg;
            public Image Icon;
        
            public UiSlot(GameObject obj, Image bgImg, Image icon) {
                Obj = obj;
                BgImg = bgImg;
                Icon = icon;
            }
        }

        private float _maxStamina = 100f;
        private float _maxHealth = 100f;
        private LoggerSO _logger;
        #endregion

        #region Unity Methods
        private void Awake() {
            _inventoryController = GetComponent<InventoryControllerScript>();
            _playerLifeSupportScript = GetComponent<PlayerLifeSupportScript>();
            _playerInputScript = GetComponent<PlayerInputScript>();
        
            ValidateReferences();
        
            _maxHealth = _playerLifeSupportScript.MaxHealth;
            _maxStamina = _playerLifeSupportScript.MaxStamina;
        }
        private void Start() {
            _logger = LoggerSO.Instance;
            _logger.LogMinor($"[UIManager] Start en {gameObject.name}");
            InitUI();
        }
    
        #endregion
    
        #region Main Logic 
        private void InitUI()
        {
            _healthTicks.Clear();
            foreach (Transform child in healthContainer)
            {
                Image img = child.GetComponent<Image>();
                if (img != null) _healthTicks.Add(img);
            }
        
            DisplayHealth(_maxHealth);
            DisplayStamina(_maxStamina);
        }

        private void ValidateReferences() {
            if (_playerLifeSupportScript == null)
                _logger.LogWarning("[PlayerUIManager] Player life script is null.] ");
            if (_inventoryController == null)
                _logger.LogWarning("[PlayerUIManager] Player inventory script is null.] ");
            if (healthContainer == null)
                _logger.LogWarning("[PlayerUIManager] Health container is null.] ");
            if (staminaBar == null)
                _logger.LogWarning("[PlayerUIManager] Stamina bar is null.] ");
        }
        private void UpdateAllSlots(InventoryControllerScript.InventorySlot[] inv, PlayerInputScript input) {
            for (int i = 0; i < slots.Length; i++)
            {
                bool hasItem = (i < inv.Length && inv[i] != null);

                if (hasItem)
                {
                    ItemSO itemData = inv[i].itemData;
                    UpdateSlot(itemData, i);
                }
                else
                {
                    UpdateSlot(null, i);
                }

                Image slotBg = slots[i].BgImg;
            
                if (slotBg != null)
                    slotBg.color = (i == input.InventoryIndex) ? Color.yellow : Color.white;
            }
        }
        private void UpdateSlot(ItemSO itemData, int slot) {
            if (itemData != null)
            {
                slots[slot].Icon.sprite = itemData.icon;
                slots[slot].Icon.enabled = true;
            }
            else
            {
                slots[slot].Icon.sprite = null;
                slots[slot].Icon.enabled = false;
            }
        }
        #endregion
    
        #region Public API
    
        public void UpdateHotbarUI()
        {
            var inventory = _inventoryController.inventory;
            //var itemSOs = _inventoryController.itemSOs;
            UpdateAllSlots(inventory, _playerInputScript);
        }

        public void DisplayStamina(float amount)
        {
            float fill = Mathf.Clamp01(amount / _maxStamina);
            staminaBar.fillAmount = fill;
        }

        public void DisplayHealth(float amount)
        {
            if (_healthTicks.Count == 0)
            {
                return;
            }

            int ticksOn = Mathf.CeilToInt(amount / healthPerTick);

            for (int i = 0; i < _healthTicks.Count; i++)
            {
                if (i < ticksOn)
                {
                    if (ticksOn <= 3)
                        _healthTicks[i].sprite = tickRed;
                    else if (ticksOn <= 7)
                        _healthTicks[i].sprite = tickYellow;
                    else
                        _healthTicks[i].sprite = tickOn;

                    var c = _healthTicks[i].color;
                    c.a = 1f;
                    _healthTicks[i].color = c;
                }
                else
                {
                    var c = _healthTicks[i].color;
                    c.a = 0f;
                    _healthTicks[i].color = c;
                }
            }
        }
    
        #endregion
    }
}

