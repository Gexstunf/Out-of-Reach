using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class HandGrabberScript : MonoBehaviour
    {
        
        [Header("Settings")]
        public float grabDistance = 2f;      // how far you can grab
        public LayerMask itemMask;
        public bool debug;
        
        [Header("References")]
        public Rigidbody handRb;             // ragdoll hand rigidbody
        public Transform cameraTransform;    // reference to player's camera

        
        private GrabbableScript _current;
        
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
                Debug.Log("Checking item");
                if (item) {
                    Debug.Log("Grabbing");
                    item.Grab(handRb);
                    _current = item;
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!debug || cameraTransform == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * grabDistance);
        }
    }
}
