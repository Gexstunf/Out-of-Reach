using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        public float doorCrackOffset = 0.05f;
        
        [Header("Settings Failure")] 
        public float doorFailureChance = 0.4f;
        public DoorFailureMode doorMode;
        
        [SerializeField] private float slamDistanceOpen = 2f; 
        [SerializeField] private float slamDistanceClosed = -1f;
        [SerializeField] private float slowDoorSpeed = 2f;

        [SerializeField] private float slamSpeed = 5f;       
        [SerializeField] private float minDelay = 0.3f;           // shortest pause
        [SerializeField] private float maxDelay = 1.5f;   

        private bool _isSlamming;
        private Vector3 _femaleTarget;
        private Vector3 _maleTarget;


        private bool _isOpen;
        private Vector3 _closedPosOffset;
        private Vector3 _initialFemalePosition;
        private Vector3 _initialMalePosition;
        
        public enum DoorFailureMode {
            None,       
            FailsToClose,
            FailsToOpen,  
            Slowed,
            JammedOpen,
        }
        
        void Start() {
            _initialFemalePosition = femaleDoor.localPosition;
            _initialMalePosition = maleDoor.localPosition;
            
            _initialFemalePosition = femaleDoor.localPosition;
            _initialMalePosition = maleDoor.localPosition;

            if (UnityEngine.Random.value < doorFailureChance) {
                // pick a random failure mode, but not "None"
                doorMode = (DoorFailureMode)UnityEngine.Random.Range(1, Enum.GetValues(typeof(DoorFailureMode)).Length);
            } else {
                doorMode = DoorFailureMode.None;
            }
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
                _isOpen = open;
            }


            if (doorMode != DoorFailureMode.None) {
                HandleDoorFailure();
                return;
            }


            if (_isOpen) {
                // initial pos is  open pos
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, (_initialFemalePosition ), doorSpeed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, (_initialMalePosition ), doorSpeed * Time.deltaTime);
            }
            else {
                _closedPosOffset = new Vector3(0f, 0f, doorCrackOffset);

                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, Vector3.zero + _closedPosOffset, doorSpeed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, Vector3.zero - _closedPosOffset, doorSpeed * Time.deltaTime);
            }
        }
        
        // private void BounceDoor(Vector3 femaleTarget, Vector3 maleTarget) {
        //     float bounce = Mathf.Sin(Time.time * bounceAgressiveness) * openGapFailure; // oscillation
        //     Vector3 femaleOffset = new Vector3(0f, 0f, bounce);
        //     Vector3 maleOffset   = new Vector3(0f, 0f, -bounce);
        //
        //     femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, femaleTarget + femaleOffset, doorSpeed * Time.deltaTime);
        //     maleDoor.localPosition   = Vector3.Lerp(maleDoor.localPosition, maleTarget   + maleOffset,   doorSpeed * Time.deltaTime);
        // }


        private void HandleDoorFailure() {
            switch (doorMode) {
                case DoorFailureMode.JammedOpen:
                    return;

                case DoorFailureMode.FailsToOpen:
                    SlamDoor(Vector3.zero + _closedPosOffset, Vector3.zero - _closedPosOffset, true);
                    break;

                case DoorFailureMode.FailsToClose:
                    SlamDoor(_initialFemalePosition, _initialMalePosition, false);
                    break;
                case DoorFailureMode.Slowed:
                    HandleDoor(slowDoorSpeed);
                    break;
            }   
        }
        
        private void OnDrawGizmos() {  
            if (!debug) return;
            Gizmos.color = _isOpen ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        private void HandleDoor(float speed) {
            if (_isOpen) {
                // initial pos is  open pos
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, (_initialFemalePosition ), speed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, (_initialMalePosition ), speed * Time.deltaTime);
            }
            else {
                _closedPosOffset = new Vector3(0f, 0f, doorCrackOffset);

                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, Vector3.zero + _closedPosOffset, speed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, Vector3.zero - _closedPosOffset, speed * Time.deltaTime);
            }
        }

        private void SlamDoor(Vector3 femaleTarget, Vector3 maleTarget, bool openSlam) {
            if (_isSlamming) return;
            StartCoroutine(SlamCycle(femaleTarget, maleTarget, openSlam));
        }

        private IEnumerator SlamCycle(Vector3 femaleTarget, Vector3 maleTarget, bool openSlam) {
            _isSlamming = true;
            float slamDistance = openSlam ? slamDistanceOpen : slamDistanceClosed;
            // overshoot target like it's trying to break through
            Vector3 femaleOvershoot = femaleTarget + new Vector3(0, 0, slamDistance);
            Vector3 maleOvershoot   = maleTarget   - new Vector3(0, 0, slamDistance);

            // slam forward
            float t = 0f;
            while (t < 1f) {
                t += Time.deltaTime * slamSpeed;
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, femaleOvershoot, t);
                maleDoor.localPosition   = Vector3.Lerp(maleDoor.localPosition,   maleOvershoot,   t);
                yield return null;
            }

            // snap back violently to original target
            t = 0f;
            while (t < 1f) {
                t += Time.deltaTime * slamSpeed;
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, femaleTarget, t);
                maleDoor.localPosition   = Vector3.Lerp(maleDoor.localPosition,   maleTarget,   t);
                yield return null;
            }
            
            float randomDelay = UnityEngine.Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(randomDelay);
            
            _isSlamming = false;
        }

    }
}
