using Characters.PlayerController.Scripts;
using UnityEngine;

namespace Environment.Scripts {
    public class WallGrabbableScript : MonoBehaviour, IGrabbableScript
    {
        private ConfigurableJoint _joint;
        private Rigidbody _anchorRb;

        public void Grab(Rigidbody handRb, Vector3 grabPoint) {

            GameObject anchorObj = new GameObject("WallGrabAnchor");
            anchorObj.transform.position = grabPoint;
            _anchorRb = anchorObj.AddComponent<Rigidbody>();
            _anchorRb.isKinematic = true;

            _joint = handRb.gameObject.AddComponent<ConfigurableJoint>();
            _joint.connectedBody = _anchorRb;

            _joint.xMotion = ConfigurableJointMotion.Locked;
            _joint.yMotion = ConfigurableJointMotion.Locked;
            _joint.zMotion = ConfigurableJointMotion.Locked;

            _joint.angularXMotion = ConfigurableJointMotion.Free;
            _joint.angularYMotion = ConfigurableJointMotion.Free;
            _joint.angularZMotion = ConfigurableJointMotion.Free;
        }

        public void Release(Rigidbody handRb) {
            if (_joint) {
                Destroy(_joint);
                _joint = null;
            }
            if (_anchorRb) {
                Destroy(_anchorRb.gameObject);
                _anchorRb = null;
            }
        }
    }
}
