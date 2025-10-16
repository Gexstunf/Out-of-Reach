using System;
using UnityEngine;

namespace Characters.Enemies.Scripts {
    public class TargetingScript : MonoBehaviour {
        
        [Header("General Settings")] 
        public float detectionRadius = 10f;
        [SerializeField] private LayerMask detectionLayerMask;
        [SerializeField] private float yOffset = 2f;
        [SerializeField] private bool stationary = true;
        
        [Header("AI Settings")] 
        
        
        [Header("Debug")] 
        public bool debug;
        
        
        private Transform _currentTargetTransform;
        public Transform CurrentTargetTransform => _currentTargetTransform;

        
        private Quaternion _initialRotation;
        
        
        void Start()
        {
            _initialRotation = transform.rotation;
        }

        // Update is called once per frame
        void Update() {
            HandleDetection();


            if (stationary) {
                if (_currentTargetTransform) {
                    RotateTowardsTarget();
                }
                else {
                    transform.rotation = Quaternion.Slerp(transform.rotation, _initialRotation, Time.deltaTime * 1.5f);
                }
            }
            else {
                // non stationary enemy logic
            }
        }

        void HandleDetection() {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayerMask);
            foreach (Collider c in hits) {
                if (c.CompareTag("Player")) {
                    
                    if (!_currentTargetTransform) {
                        _currentTargetTransform = c.transform;
                    }
                    
                    float distance = Vector3.Distance(transform.position, c.transform.position);
                    float distanceFromCurrentTarget = Vector3.Distance(transform.position, _currentTargetTransform.position);
                    
                    if (distance < distanceFromCurrentTarget) {
                        _currentTargetTransform = c.transform;
                    }
                }
            }
        }

        void RotateTowardsTarget() {
            Vector3 directionToTarget = _currentTargetTransform.position - transform.position;
            directionToTarget.y = 0;

            if (directionToTarget != Vector3.zero) {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = targetRotation;
            }
        }

        private void OnDrawGizmos() {
            if (!debug) return;
            
            Gizmos.color = _currentTargetTransform ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
