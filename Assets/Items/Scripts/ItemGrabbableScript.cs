using Characters.PlayerController.Scripts;
using Multiplayer.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items.Scripts {
    public class ItemGrabbableScript : MonoBehaviour, IGrabbableScript
    {
        [Header("Settings")]
        public Transform grabHandle;
        public ItemSO data;
        public float massScale = 1f;
        public float connectedMassScale = 1f;
        
        private ConfigurableJoint _joint;
        private Rigidbody _rb;

        
        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        public Transform GrabHandle { get; set; }

        public void Grab(Rigidbody rb, Vector3 grabPoint) {
            if (_joint) {
                Debug.Log("Couldnt attach, already has a joint");
                return; // already grabbed
            }
            

            // Move object so grabPoint aligns with hand

            if (grabHandle) {
                Vector3 offset = transform.position - grabHandle.position;
                transform.position = rb.position + offset;  
            }

            
            // Add joint on the OBJECT, connect to hand
            _joint = gameObject.AddComponent<ConfigurableJoint>();
            _joint.connectedBody = rb;

            _joint.xMotion = ConfigurableJointMotion.Locked;
            _joint.yMotion = ConfigurableJointMotion.Locked;
            _joint.zMotion = ConfigurableJointMotion.Locked;

            _joint.angularXMotion = ConfigurableJointMotion.Locked;
            _joint.angularYMotion = ConfigurableJointMotion.Locked;
            _joint.angularZMotion = ConfigurableJointMotion.Locked;

            // tweak mass scaling for stability
            _joint.massScale = massScale;
            _joint.connectedMassScale = connectedMassScale;
        }

        public void Release() {
            if (_joint)
            {
                Destroy(_joint);
                _joint = null;
            }
        }

        public bool IsItem { get; } = true;
    }
}
