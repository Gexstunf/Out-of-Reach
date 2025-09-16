using TMPro;
using UnityEngine;

namespace UI.Scripts.TestingUI {
    public class UIManagerScript : MonoBehaviour
    {
        public TextMeshProUGUI staminaText;
        public TextMeshProUGUI healthText;
        
        public bool debug = false;

        public void DisplayStamina(float amount) {
            staminaText.text = amount.ToString();

            if (debug) {
                Debug.Log("Displaying stamina: " + amount);

            }
        }

        public void DisplayHealth(float amount) {
            healthText.text = amount.ToString();
            
            if (debug) {
                Debug.Log("Displaying health: " + amount);
            }
        }
    }
}
