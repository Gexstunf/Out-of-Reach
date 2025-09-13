using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Characters.LifeSupportSystem.PlayerLifeSupport;
using static Characters.LifeSupportSystem.PlayerLifeSupport.PlayerLifeSupportScript;
using Characters.LifeSupportSystem;

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

        private PlayerInventoryPhoton inventory;
        private bool _initialized = false;

        // NUEVO: jugador objetivo
        private PlayerLifeSupportContextScript _context;
        private Dictionary<PlayerLifeSupportScript.EVitals, BaseVitalScript<PlayerLifeSupportScript.EVitals>> _vitals;
        private PlayerInventoryPhoton _inventory;

        public void Start()
        {
            InitUI();
        }

        private void Update()
        {
            if (!_initialized) return;

            UpdateUI();
        }

        public void InitUI()
        {
            if (_initialized) return;

            if (staminaBar == null)
                staminaBar = GameObject.Find("Canvas/StaminaBar")?.GetComponent<Image>();
            if (healthContainer == null)
                healthContainer = GameObject.Find("Canvas/HealthContainer")?.transform;

            if (healthContainer != null)
            {
                healthTicks.Clear();
                foreach (Transform child in healthContainer)
                {
                    Image img = child.GetComponent<Image>();
                    if (img != null) healthTicks.Add(img);
                }
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
                }
            }

            _initialized = true;
        }

        public void InitInventory(PlayerInventoryPhoton inv)
        {
            inventory = inv;
            UpdateInventoryUI();
        }

        public void SetTarget(PlayerLifeSupportContextScript context,
                      Dictionary<PlayerLifeSupportScript.EVitals, BaseVitalScript<PlayerLifeSupportScript.EVitals>> vitals)
        {
            // Guardamos referencia al contexto y vitals para actualizar UI
            _context = context;
            _vitals = vitals;
        }

        private void UpdateUI()
        {
            if (_context == null) return;

            Debug.Log($"Actualizando UI - Health: {_context.Health}, Stamina: {_context.Stamina}");

            DisplayHealth(_context.Health);
            DisplayStamina(_context.Stamina);
            UpdateInventoryUI();
        }

        public void UpdateInventoryUI()
        {
            if (inventory == null) return;

            for (int i = 0; i < slotUI.Length; i++)
            {
                bool hasItem = (i < inventory.slots.Length && inventory.slots[i] != null);

                if (hasItem)
                {
                    slotIcons[i].sprite = inventory.slots[i].icon;
                    slotIcons[i].enabled = true;
                }
                else
                {
                    slotIcons[i].sprite = null;
                    slotIcons[i].enabled = false;
                }

                Image slotBg = slotUI[i].GetComponent<Image>();
                if (slotBg != null)
                {
                    slotBg.color = (i == inventory.activeSlot) ? Color.yellow : Color.white;
                }
            }
        }

        public void OnSlotClicked(int slotIndex)
        {
            if (inventory == null) return;
            inventory.EquipFromSlot(slotIndex);
            UpdateInventoryUI();
        }

        public void DisplayStamina(float amount)
        {
            if (staminaBar == null)
            {
                Debug.LogWarning("StaminaBar es NULL en " + gameObject.name);
                return;
            }

            float fill = Mathf.Clamp01(amount / maxStamina);
            Debug.Log($"[{gameObject.name}] Cambiando staminaBar.fillAmount a {fill}");
            staminaBar.fillAmount = fill;
        }

        public void DisplayHealth(float amount)
        {
            Debug.Log($"[{gameObject.name}] Actualizando vida: {amount}");

            if (healthTicks.Count == 0)
            {
                Debug.LogWarning("No hay healthTicks en " + gameObject.name);
                return;
            }

            int ticksOn = Mathf.CeilToInt(amount / healthPerTick);
            Debug.Log($"[{gameObject.name}] Ticks encendidos: {ticksOn}/{healthTicks.Count}");

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
    }
}
