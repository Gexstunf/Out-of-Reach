using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Characters.Enemies.Scripts {
    public class EnemyAgentScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TargetingScript _targetingScript;
        [SerializeField] private NavMeshAgent _agent;
        
        public NavMeshAgent Agent => _agent;
        public bool IsChasing => _chasing;

        private bool _chasing;
        
        //[Header("AI Settings")]

        
        void Awake() {
            _targetingScript = GetComponent<TargetingScript>();
            _agent = GetComponent<NavMeshAgent>();

            if (_agent == null) Debug.LogWarning("No NavMeshAgent component attached to: " + gameObject.name);
            if (_targetingScript == null) Debug.LogWarning("No TargetingScript component attached to: " + gameObject.name);
        }

        void Update()
        {
            if (_targetingScript.CurrentTargetTransform && _agent.isActiveAndEnabled) {
                _agent.destination = _targetingScript.CurrentTargetTransform.position;
                _chasing = true;
            }
            else {
                _chasing = false;
            }
        }
    }
}
