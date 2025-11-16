using GlobalUtils;
using UnityEngine;

namespace Environment.Scripts.Lights {
    public class LightsManagerScript : MonoBehaviour {

        [Header("References")] 
        [SerializeField] private LoggerSO _logger;
        public static LightsManagerScript Instance;

        [Header("Settings")] 
        public bool usePhoton;

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }
            
            Instance = this;
        }
    }
}
