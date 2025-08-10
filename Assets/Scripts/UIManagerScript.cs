using TMPro;
using UnityEngine;

namespace UI.Scripts.TestingUI {
    public class UIManagerScript : MonoBehaviour
    {
        public TextMeshProUGUI staminaText;
        public TextMeshProUGUI healthText;

        public void DisplayStamina(float amount) {
            staminaText.text = amount.ToString();
        }

        public void DisplayHealth(float amount) {
            healthText.text = amount.ToString();
        }
    }
}
