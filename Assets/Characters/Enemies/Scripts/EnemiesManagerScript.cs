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
        
        #region Public API
        public bool AgentsCanInteract { get; private set; } = false;
        #endregion
        
        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }
            
            navMeshScript = FindFirstObjectByType<AINavMeshScript>();
            Instance = this;
        }

        private IEnumerator Start() {
            dungeonGeneratorScript = DungeonGeneratorScript.Instance;
            yield return new WaitUntil(() => dungeonGeneratorScript.FinishedGeneration);
            navMeshScript?.BuildNavMesh();
            _agents = FindObjectsByType<EnemyAgentScript>(FindObjectsSortMode.None).ToList();

            foreach (var agent in _agents) {
                agent.SetAIActive(true);
            }
            AgentsCanInteract = true;
        }
    }
}
