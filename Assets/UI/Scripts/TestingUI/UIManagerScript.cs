using TMPro;
using UnityEngine;

public class UIManagerScript : MonoBehaviour
{
    public TextMeshProUGUI staminaText;
    
    public void DisplayStamina(float amount) {
        staminaText.text = amount.ToString();
    }
}
