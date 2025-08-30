using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;

public class PlayerUIManager : MonoBehaviourPun
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
    public GameObject[] slotUI; // Paneles o botones de cada slot
    public Image[] slotIcons;   // Imagen que muestra el icono del item
    private PlayerInventoryPhoton inventory; // referencia al inventario del player

    void Awake()
    {
        if (!photonView.IsMine) return; // Solo para jugador local

        // Stamina y Health
        if (staminaBar == null)
            staminaBar = transform.Find("Canvas/StaminaBar")?.GetComponent<Image>();
        if (healthContainer == null)
            healthContainer = transform.Find("Canvas/HealthContainer");

        healthTicks.Clear();
        foreach (Transform child in healthContainer)
        {
            Image img = child.GetComponent<Image>();
            if (img != null) healthTicks.Add(img);
        }

        DisplayHealth(maxHealth);
        DisplayStamina(maxStamina);

        // Inicializar botones de inventario
        for (int i = 0; i < slotUI.Length; i++)
        {
            int index = i; // necesario para closures
            Button btn = slotUI[i].GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSlotClicked(index));
            }
        }
    }

    // Conectar el PlayerInventoryPhoton
    public void InitInventory(PlayerInventoryPhoton inv)
    {
        inventory = inv;
        UpdateInventoryUI();
    }

    // Actualizar íconos de slots
    public void UpdateInventoryUI()
    {
        if (inventory == null) return;

        for (int i = 0; i < slotUI.Length; i++)
        {
            if (i < inventory.slots.Length && inventory.slots[i] != null)
            {
                slotIcons[i].sprite = inventory.slots[i].icon;
                slotIcons[i].enabled = true;
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].enabled = false;
            }
        }
    }

    // Cuando se hace click en un slot
    public void OnSlotClicked(int slotIndex)
    {
        if (inventory == null) return;
        inventory.EquipFromSlot(slotIndex);
        UpdateInventoryUI();
    }

    public void DisplayStamina(float amount)
    {
        if (staminaBar == null) return;
        staminaBar.fillAmount = Mathf.Clamp01(amount / maxStamina);
    }

    public void DisplayHealth(float amount)
    {
        if (healthTicks.Count == 0) return;

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
}
