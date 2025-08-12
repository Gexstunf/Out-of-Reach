using TMPro;
using UnityEngine;
using UnityEngine.UI; // Para usar Image

public class UIManagerScript : MonoBehaviour
{
    [Header("Barra de Stamina")]
    public Image staminaBar;
    public float maxStamina;

    [Header("Vida en Ticks")]
    public Transform healthContainer;
    public Sprite tickOn;
    public Sprite tickOff;
    public float maxHealth = 100f;
    public float healthPerTick = 10f;

    private List<Image> healthTicks = new List<Image>();
    
    public void DisplayStamina(float amount){
        staminaBar.fillAmount = amount / maxStamina;
    }

    void Awake()
    {
        foreach (Transform child in healthContainer)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
                healthTicks.Add(img);
        }
    }

    public void DisplayHealth(float amount)
    {
        int ticksOn = Mathf.CeilToInt(amount / healthPerTick);

        for (int i = 0; i < healthTicks.Count; i++)
        {
            if (i < ticksOn)
                healthTicks[i].sprite = tickOn;
            else
                healthTicks[i].sprite = tickOff;
        }
    }
}
