using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment.Scripts {
    public class SuctionAreaScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SlidingDoorSpawnScript doorSpawnScript;
        [SerializeField] private SlidingDoorScript doorScript;
        [SerializeField] private Transform suctionPosition;
        
        [Header("Settings")]
        public float suctionForce = 40f;
        public float suctionRadius = 5f;
        public LayerMask suctionLayerMask;
        public bool useDoorScript = false;
        public bool useThisTransform = true;
        public bool useThisDoorScript = false;
        
        [Header("Debug")]
        public bool debug = false;
        private bool _isAttractingACharacter;
        
        private List<GameObject> _suckedCharacters = new List<GameObject>();
        
        
        private SlidingDoorSpawnScript DoorSpawnScript {
            get {
                if (!doorSpawnScript && useDoorScript && useThisDoorScript) {
                    doorSpawnScript = GetComponent<SlidingDoorSpawnScript>();
                    Debug.Log("Suction script cached Door SPAWN script");
                }
                return doorSpawnScript;
            }
        }
        
        private SlidingDoorScript DoorScript {
            get {
                if (doorSpawnScript && useDoorScript && useThisDoorScript && !doorScript) {
                    doorScript = doorSpawnScript.slidingDoorInstance.GetComponent<SlidingDoorScript>();
                    Debug.Log("Suction script cached Door script");
                }
                return doorScript;
            }
        }
        
        private void Start() {
            if (useThisTransform)
                suctionPosition = transform;
        }

        private void Update() {
            Collider[] hits = Physics.OverlapSphere(suctionPosition.position, suctionRadius, suctionLayerMask);
            if (hits == null || hits.Length == 0) {
                _isAttractingACharacter = false;
                return;
            }

            if (useDoorScript) {
                var doorSpawn = DoorSpawnScript;
                var door = DoorScript;
                if (!doorSpawn || !door.IsOpen) {
                    _isAttractingACharacter = false;
                    return;
                }
            }

            AttractCharacters(hits);
            _isAttractingACharacter = true;
            
        }

        private void AttractCharacters(Collider[] characters) {
            foreach (Collider col in characters) {

                if (DidFinishSuckingCharacter(col)) {
                    continue;
                }
                
                Vector3 cPos = col.transform.position;
                Vector3 vectorTowardsCharacter = (cPos - suctionPosition.position).normalized;
                col.attachedRigidbody.AddForce(-vectorTowardsCharacter * suctionForce); // the vector towards target, inversed = suction.
            }
        }

        private void OnTriggerEnter(Collider other) {
            _suckedCharacters.Add(other.gameObject);
            Debug.Log("Triggered with: " + other.gameObject.name + " and layer: " + other.gameObject.layer);
        }

        private bool DidFinishSuckingCharacter(Collider col) {
            foreach (var character in _suckedCharacters) {
                if (character == col.gameObject) {
                    return true;
                }
            }
            return false;
        }

        private void OnDrawGizmos() {
            if (!debug) return;
            
            Gizmos.color = _isAttractingACharacter ? Color.green : Color.red;

            if (useThisTransform)
                Gizmos.DrawWireSphere(transform.position, suctionRadius);
            else 
                Gizmos.DrawWireSphere(suctionPosition.position, suctionRadius);

        }
    }
}
