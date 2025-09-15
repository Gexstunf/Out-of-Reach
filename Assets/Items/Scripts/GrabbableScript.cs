using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class GrabbableScript : MonoBehaviour
    {
        private ConfigurableJoint joint;
        private Rigidbody rb;

        [Header("Settings")]
        public Transform grabPoint;
        public float massScale = 1f;
        public float connectedMassScale = 1f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        public void Grab(Rigidbody handRb)
        {
            if (joint != null) return; // already grabbed

            // Move object so grabPoint aligns with hand
            if (grabPoint != null)
            {
                Vector3 offset = transform.position - grabPoint.position;
                transform.position = handRb.position + offset;
            }

            // Add joint on the OBJECT, connect to hand
            joint = gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = handRb;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            // tweak mass scaling for stability
            joint.massScale = massScale;
            joint.connectedMassScale = connectedMassScale;
        }

        public void Release()
        {
            if (joint != null)
            {
                Destroy(joint);
                joint = null;
            }
        }

    }
}
