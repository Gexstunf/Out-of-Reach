using System;
using UnityEngine;

namespace Environment.Scripts {
    public class SlidingDoorScript : MonoBehaviour {

        [Header("References")] 
        public Transform femaleDoor;
        public Transform maleDoor;

        [Header("Settings")] 
        public float openOffset;
        public float detectionRadius = 4f;
        public LayerMask detectionLayerMask;
        public float doorSpeed = 0.2f;
        public bool open;
        public bool debug;
        public float doorCrackOffset = 0.9f;



        private bool _isOpen;
        private Vector3 _closedPosOffset;
        private Vector3 _initialFemalePosition;
        private Vector3 _initialMalePosition;
        
        void Start() {
            _initialFemalePosition = femaleDoor.localPosition;
            _initialMalePosition = maleDoor.localPosition;
        }

        private void Update() {
            if (!debug) {
                Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayerMask);
                foreach (Collider c in hits) {
                    if (c.CompareTag("Player")) {
                        _isOpen = true;
                        break;
                    }
                    _isOpen = false;
                }
            }
            else {
                _closedPosOffset = new Vector3(0f, 0f, doorCrackOffset);
                _isOpen = open;
            }

            if (_isOpen) {
                // initial pos is  open pos
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, (_initialFemalePosition ), doorSpeed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, (_initialMalePosition ), doorSpeed * Time.deltaTime);
            }
            else {
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, Vector3.zero + _closedPosOffset, doorSpeed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, Vector3.zero - _closedPosOffset, doorSpeed * Time.deltaTime);
            }
        }

        private void OnDrawGizmos() {
            if (!debug) return;
            Gizmos.color = _isOpen ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
