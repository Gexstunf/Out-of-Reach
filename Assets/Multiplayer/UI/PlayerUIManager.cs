using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Characters.LifeSupportSystem.PlayerLifeSupport;
using static Characters.LifeSupportSystem.PlayerLifeSupport.PlayerLifeSupportScript;
using Characters.LifeSupportSystem;
using Multiplayer.Inventory;

namespace UI
{
    public class PlayerUIManager : MonoBehaviour
    {
        [Header("Barra de Stamina")]
        public Image staminaBar;
        public float maxStamina = 100f;

        [Header("Vida en Ticks")]
        public Transform healthContainer;
        public Sprite tickOn;
        public Sprite tickYellow;
        public Sprite tickRed;
        public float maxHealth = 100f;
        public float healthPerTick = 10f;
        private List<Image> healthTicks = new List<Image>();

        [Header("Inventario UI")]
        public GameObject[] slotUI;
        public Image[] slotIcons;

        private InventoryControllerScript _inventoryController;
        private bool _initialized = false;

        [Header("Inventario Mochila")]
        public GameObject backpackPanel;
        public bool backpackOpen = false;
        public GameObject[] backpackSlotUI;
        public Image[] backpackSlotIcons;

        [Header("Debug")]
        public bool debug = true;

        private BackpackData currentBackpackWorld;

        // NUEVO: jugador objetivo
        private PlayerLifeSupportContextScript _context;
        private Dictionary<PlayerLifeSupportScript.EVitals, BaseVitalScript<PlayerLifeSupportScript.EVitals>> _vitals;

        public void Start()
        {
            backpackPanel.SetActive(false);
            Log("[UIManager] Panel de mochila inicializado como oculto.");
            Log($"[UIManager] Start en {gameObject.name}");
            InitUI();
        }

        private void Update()
        {
            if (!_initialized) return;

            UpdateUI();
        }

        public void InitUI()
        {
            if (_initialized)
            {
                Log("[UIManager] Ya estaba inicializado, se salta InitUI.");
                return;
            }

            if (staminaBar == null)
            {
                staminaBar = GameObject.Find("Canvas/StaminaBar")?.GetComponent<Image>();
                Log(staminaBar != null
                    ? "[UIManager] ✅ StaminaBar encontrado dinámicamente."
                    : "[UIManager] ⚠️ No se encontró StaminaBar dinámicamente.");
            }

            if (healthContainer == null)
            {
                healthContainer = GameObject.Find("Canvas/HealthContainer")?.transform;
                Log(healthContainer != null
                    ? "[UIManager] ✅ HealthContainer encontrado dinámicamente."
                    : "[UIManager] ⚠️ No se encontró HealthContainer dinámicamente.");
            }

            if (healthContainer != null)
            {
                healthTicks.Clear();
                foreach (Transform child in healthContainer)
                {
                    Image img = child.GetComponent<Image>();
                    if (img != null) healthTicks.Add(img);
                }
                Log($"[UIManager] Health ticks cargados: {healthTicks.Count}");
            }

            DisplayHealth(maxHealth);
            DisplayStamina(maxStamina);

            for (int i = 0; i < slotUI.Length; i++)
            {
                int index = i;
                Button btn = slotUI[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnSlotClicked(index));
                    Log($"[UIManager] Listener agregado a slot {i}");
                }
            }

            _initialized = true;
            Log("[UIManager] ✅ InitUI completado.");
        }

        public void InitInventory(InventoryControllerScript invController)
        {
            _inventoryController = invController;
            Log(_inventoryController != null
                ? "[UIManager] ✅ Inventario asignado correctamente."
                : "[UIManager] ⚠️ Inventario NULL al inicializar.");
            Log("[UIManager] Inventario inicializado");
            UpdateInventoryUI();
        }

        public void SetTarget(PlayerLifeSupportContextScript context,
                      Dictionary<PlayerLifeSupportScript.EVitals, BaseVitalScript<PlayerLifeSupportScript.EVitals>> vitals)
        {
            _context = context;
            _vitals = vitals;

            Log(_context != null
                ? "[UIManager] ✅ Context asignado correctamente."
                : "[UIManager] ⚠️ Context es NULL!");

            Log(_vitals != null
                ? $"[UIManager] ✅ Vitals asignados: {_vitals.Count}"
                : "[UIManager] ⚠️ Vitals es NULL!");
        }

        private void UpdateUI()
        {
            if (_context == null)
            {
                LogWarning("[UIManager] ⚠️ UpdateUI llamado pero Context es NULL.");
                return;
            }

            DisplayHealth(_context.Health);
            DisplayStamina(_context.Stamina);
            UpdateInventoryUI();
        }

        public void UpdateInventoryUI()
        {
            if (_inventoryController == null)
            {
                LogWarning("[UIManager] ⚠️ UpdateInventoryUI llamado pero Inventario es NULL.");
                return;
            }

            var inventory = _inventoryController.inventory;
            var itemSOs = _inventoryController.itemSOs;
            var input = _inventoryController.input;

            for (int i = 0; i < slotUI.Length; i++)
            {
                bool hasItem = (i < inventory.Length && inventory[i] != null);

                if (hasItem)
                {
                    ItemSO itemData = inventory[i].itemData;
                    if (itemData)
                    {
                        slotIcons[i].sprite = itemData.icon;
                        slotIcons[i].enabled = true;
                    }
                    else
                    {
                        slotIcons[i].sprite = null;
                        slotIcons[i].enabled = false;
                    }
                }
                else
                {
                    slotIcons[i].sprite = null;
                    slotIcons[i].enabled = false;
                }

                Image slotBg = slotUI[i].GetComponent<Image>();
                if (slotBg != null)
                {
                    slotBg.color = (i == input.InventoryIndex) ? Color.yellow : Color.white;
                }
            }
        }

        public void OnSlotClicked(int slotIndex)
        {
            if (_inventoryController == null)
            {
                LogWarning("[UIManager] ⚠️ OnSlotClicked llamado pero Inventario es NULL.");
                return;
            }

            _inventoryController.EquipItemFromSlot(slotIndex);
            UpdateInventoryUI();
        }

        public void ToggleBackpackInventory(BackpackData bd, PlayerInventoryPhoton inv)
        {
            if (backpackPanel.activeSelf)
            {
                HideBackpackInventory();
            }
            else
            {
                ShowBackpackInventory(bd, inv);
                currentBackpackWorld = bd;
                //currentInv = inv;
            }
        }

        public void ShowBackpackInventory(BackpackData bd, PlayerInventoryPhoton inv)
        {
            Debug.Log("[UIManager] Abriendo inventario de mochila.");
            backpackPanel.SetActive(true);

            for (int i = 0; i < backpackSlotUI.Length; i++)
            {
                if (i >= bd.internalSlots.Length) continue;

                ItemSO item = bd.internalSlots[i];
                backpackSlotIcons[i].sprite = item != null ? item.icon : null;
                backpackSlotIcons[i].enabled = item != null;

                int slotIndex = i;
                Button btn = backpackSlotUI[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();

                    if (item != null)
                    {
                        btn.onClick.AddListener(() =>
                        {
                            Debug.Log($"[UIManager] Sacando item de slot {slotIndex}");
                            UpdateBackpackUI(bd.internalSlots);
                        });
                    }
                    else
                    {
                        btn.onClick.AddListener(() =>
                        {
                            if (inv.tempItemData != null)
                            {
                                Debug.Log($"[UIManager] Guardando item en slot {slotIndex}");
                                UpdateBackpackUI(bd.internalSlots);
                            }
                        });
                    }
                }
            }
        }

        public void HideBackpackInventory()
        {
            if (backpackPanel.activeSelf)
            {
                Debug.Log("[UIManager] Cerrando inventario de mochila.");
                backpackPanel.SetActive(false);
            }
        }

        public void UpdateBackpackUI(ItemSO[] internalSlots)
        {
            for (int i = 0; i < backpackSlotUI.Length; i++)
            {
                if (i >= internalSlots.Length) continue;

                ItemSO item = internalSlots[i];
                backpackSlotIcons[i].sprite = item != null ? item.icon : null;
                backpackSlotIcons[i].enabled = item != null;
            }

            Debug.Log("[UIManager] Mochila UI actualizada.");
        }

        public void CloseBackpack()
        {
            backpackPanel.SetActive(false);
            Debug.Log("[UIManager] Mochila cerrada.");
        }

        public void DisplayStamina(float amount)
        {
            if (staminaBar == null)
            {
                Debug.LogWarning("[UIManager] ⚠️ StaminaBar es NULL.");
                return;
            }

            float fill = Mathf.Clamp01(amount / maxStamina);
            staminaBar.fillAmount = fill;
        }

        public void DisplayHealth(float amount)
        {
            if (healthTicks.Count == 0)
            {
                return;
            }

            int ticksOn = Mathf.CeilToInt(amount / healthPerTick);

            for (int i = 0; i < healthTicks.Count; i++)
            {
                if (i < ticksOn)
                {
                    if (ticksOn <= 3)
                        healthTicks[i].sprite = tickRed;
                    else if (ticksOn <= 7)
                        healthTicks[i].sprite = tickYellow;
                    else
                        healthTicks[i].sprite = tickOn;

                    var c = healthTicks[i].color;
                    c.a = 1f;
                    healthTicks[i].color = c;
                }
                else
                {
                    var c = healthTicks[i].color;
                    c.a = 0f;
                    healthTicks[i].color = c;
                }
            }
        }

        private void Log(string message)
        {
            if (debug) Debug.Log(message);
        }

        private void LogWarning(string message)
        {
            if (debug) Debug.LogWarning(message);
        }
    }
}
