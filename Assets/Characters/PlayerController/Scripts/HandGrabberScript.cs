using System.Collections;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class HandGrabberScript : MonoBehaviour
    {
        
        [Header("Settings")]
        public float grabDistance = 2f;      // how far you can grab
        public float grabSpeed = 3f;
        public LayerMask itemMask;
        public bool debug;
        
        [Header("References")]
        public Transform handIKTarget;         // IK target Transform
        public Rigidbody handRb;             // ragdoll hand rigidbody
        public Transform cameraTransform;    // reference to player's camera

        
        private GrabbableScript _current;
        private Coroutine _grabCoroutine;
        
        void Update()
        {
            if (!_current)
            {
                TryGrab();
            }
        }

        void TryGrab()
        {
            RaycastHit hit;
            Vector3 origin = cameraTransform.position;
            Vector3 direction = cameraTransform.forward;

            if (Physics.Raycast(origin, direction, out hit, grabDistance, itemMask)) {
                GrabbableScript item = hit.collider.GetComponent<GrabbableScript>();
                if (item) {
                    Debug.Log("Grabbing");
                    _current = item;
                    if (_grabCoroutine != null) {
                        StopCoroutine(_grabCoroutine);
                    }
                    _grabCoroutine = StartCoroutine(GrabSequence(item, hit.point));
                }
            }
        }

        private IEnumerator GrabSequence(GrabbableScript item, Vector3 hitPoint) {
            Vector3 start = handIKTarget.position;
            Vector3 target = item.grabPoint ? item.grabPoint.position : hitPoint;
            float elapsed = 0f;

            while (elapsed < 1f) {
                elapsed += Time.deltaTime * grabSpeed; 
                float progress = Mathf.Clamp01(elapsed);
                
                handIKTarget.position = Vector3.Lerp(start, target, progress);
                yield return null;
            }
            
            handIKTarget.position = target;
            item.Grab(handRb);
        }

        void OnDrawGizmos()
        {
            if (!debug || cameraTransform == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * grabDistance);
        }
    }
}
