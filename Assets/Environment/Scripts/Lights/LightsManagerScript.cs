using System.Collections;
using Environment.Scripts.DungeonGeneration.CoreScripts;
using GlobalUtils;
using UnityEngine;

namespace Environment.Scripts.Lights {
    public class LightsManagerScript : MonoBehaviour {

        [Header("References")] 
        [SerializeField] private LoggerSO _logger;
        [SerializeField] private DungeonGeneratorScript _dungeonGenerator;
        private LightScript[] _lights;

        public static LightsManagerScript Instance;

        [Header("Settings")] 
        public bool usePhoton;

        [Header("Visualize")] 
        public int lightCount;

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }
            
            Instance = this;
        }

        private IEnumerator Start() {
            _dungeonGenerator = DungeonGeneratorScript.Instance;
            yield return new WaitUntil(() => _dungeonGenerator.FinishedGeneration);
            _lights = FindObjectsByType<LightScript>(FindObjectsSortMode.None);

            foreach (var l in _lights) {
                l.StartLight();
                lightCount++;
            }
        }
    }
}
