using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Environment.Scripts;
using Environment.Scripts.DungeonGeneration.CoreScripts;
using UnityEngine;

namespace Characters.Enemies.Scripts {
    public class EnemiesManagerScript : MonoBehaviour
    {
        [Header("References")]
        public DungeonGeneratorScript dungeonGeneratorScript;
        public AINavMeshScript navMeshScript;
        public static EnemiesManagerScript Instance;
        
        private List<EnemyAgentScript> _agents;
        
        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }

            Instance = this;
        }

        private IEnumerator Start() {
            yield return new WaitUntil(() => dungeonGeneratorScript.FinishedGeneration);
            navMeshScript = FindFirstObjectByType<AINavMeshScript>();
            navMeshScript?.BuildNavMesh();
            _agents = FindObjectsByType<EnemyAgentScript>(FindObjectsSortMode.None).ToList();

            foreach (var agent in _agents) {
                agent.SetAIActive(true);
            }
        }
    }
}
