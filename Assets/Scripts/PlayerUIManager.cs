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

    public void DisplayStamina(float amount)
    {
        if (!photonView.IsMine) return; 

        staminaBar.fillAmount = Mathf.Clamp01(amount / maxStamina);
    }

    void Awake()
    {
        // Inicializar ticks
        foreach (Transform child in healthContainer)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
                healthTicks.Add(img);
        }
    }

    public void DisplayHealth(float amount)
    {
        if (!photonView.IsMine) return;

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

                Color c = healthTicks[i].color;
                c.a = 1f;
                healthTicks[i].color = c;
            }
            else
            {
                Color c = healthTicks[i].color;
                c.a = 0f;
                healthTicks[i].color = c;
            }
        }
    }
}
