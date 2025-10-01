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
        [SerializeField] private GrabbableType grabbableType = GrabbableType.Item;

        private enum GrabbableType
        {
            Item = 1,
            Wall = 2
        }

        void Awake()
        {
            //if (grabbableType == GrabbableType.Item) {
                rb = GetComponent<Rigidbody>();
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            //}
        }

        public void Grab(Rigidbody handRb, Vector3 grabPoint)
        {
            switch (grabbableType) {
                case  GrabbableType.Item: 
                    GrabAnItem(handRb);
                    break;
                case GrabbableType.Wall:
                    GrabAWall(handRb, grabPoint);
                    break;
            }

        }

        void GrabAnItem(Rigidbody handRb) {
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
        
        void GrabAWall(Rigidbody handRb, Vector3 anchorPoint) {
            ConfigurableJoint newJoint = handRb.gameObject.AddComponent<ConfigurableJoint>();

            newJoint.yMotion = ConfigurableJointMotion.Locked;
            newJoint.xMotion = ConfigurableJointMotion.Locked;
            newJoint.zMotion = ConfigurableJointMotion.Locked;
            
            newJoint.angularXMotion = ConfigurableJointMotion.Free;
            newJoint.angularYMotion = ConfigurableJointMotion.Free;
            newJoint.angularZMotion = ConfigurableJointMotion.Free;
            
            newJoint.anchor = anchorPoint;
        }

        public void ReleaseItem()
        {
            if (joint != null)
            {
                Destroy(joint);
                joint = null;
            }
        }
        
        public void ReleaseWall(Rigidbody handRb)
        {
        }

    }
}
