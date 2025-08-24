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

    void Awake()
    {
        if (!photonView.IsMine) return; // Solo inicializar UI para el jugador local

        // Buscar automáticamente StaminaBar y HealthContainer dentro del prefab del jugador
        if (staminaBar == null)
        {
            staminaBar = transform.Find("Canvas/StaminaBar")?.GetComponent<Image>();
            if (staminaBar == null) Debug.LogError("StaminaBar no encontrada dentro del prefab del jugador");
        }

        if (healthContainer == null)
        {
            healthContainer = transform.Find("Canvas/HealthContainer");
            if (healthContainer == null) Debug.LogError("HealthContainer no encontrado dentro del prefab del jugador");
        }

        // Guardar todos los ticks de vida
        healthTicks.Clear();
        foreach (Transform child in healthContainer)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
                healthTicks.Add(img);
        }

        // Mostrar valores iniciales
        DisplayHealth(maxHealth);
        DisplayStamina(maxStamina);
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
                // Colores según la cantidad de ticks
                if (ticksOn <= 3)
                    healthTicks[i].sprite = tickRed;
                else if (ticksOn <= 7)
                    healthTicks[i].sprite = tickYellow;
                else
                    healthTicks[i].sprite = tickOn;

                // Visible
                var c = healthTicks[i].color;
                c.a = 1f;
                healthTicks[i].color = c;
            }
            else
            {
                // Invisible
                var c = healthTicks[i].color;
                c.a = 0f;
                healthTicks[i].color = c;
            }
        }
    }
}
