using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class ActiveRagdollControllerScript : MonoBehaviour
    {
        [System.Serializable]
        public struct BoneMap
        {
            public Transform ghostBone;
            public ConfigurableJoint joint;
            public Rigidbody rb;
        }

        public BoneMap[] boneMaps;

        void FixedUpdate()
        {
            foreach (var bone in boneMaps)
            {
                Vector3 targetPos = bone.joint.connectedBody.transform.InverseTransformPoint(bone.ghostBone.position);
                Quaternion targetRot = Quaternion.Inverse(bone.joint.connectedBody.transform.rotation) * bone.ghostBone.rotation;

                bone.joint.targetPosition = targetPos;
                bone.joint.targetRotation = targetRot;
            }
        }
    }
}
