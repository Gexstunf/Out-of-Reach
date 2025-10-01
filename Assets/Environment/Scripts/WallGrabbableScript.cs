using Characters.PlayerController.Scripts;
using UnityEngine;

namespace Environment.Scripts {
    public class WallGrabbableScript : MonoBehaviour, IGrabbableScript
    {
        private ConfigurableJoint _joint;
        private GameObject _anchorObj;

        [Header("Settings for wall grabbing")] public float linearLimit = 2f;


        public Transform GrabHandle { get; set; }

        public void Grab(Rigidbody rb, Vector3 grabPoint) {

            _anchorObj = new GameObject("WallGrabAnchor");
            _anchorObj.transform.position = grabPoint;
            Rigidbody anchorRb = _anchorObj.AddComponent<Rigidbody>();
            anchorRb.isKinematic = true;

            _joint = rb.gameObject.AddComponent<ConfigurableJoint>();
            _joint.connectedBody = anchorRb;

            _joint.xMotion = ConfigurableJointMotion.Limited;
            _joint.yMotion = ConfigurableJointMotion.Limited;
            _joint.zMotion = ConfigurableJointMotion.Limited;

            _joint.angularXMotion = ConfigurableJointMotion.Free;
            _joint.angularYMotion = ConfigurableJointMotion.Free;
            _joint.angularZMotion = ConfigurableJointMotion.Free;

            SoftJointLimit newJointLimit = _joint.linearLimit;
            newJointLimit.limit = linearLimit;
            
            _joint.linearLimit = newJointLimit;
        }

        public void Release() {
            if (_joint) {
                Destroy(_joint);
                _joint = null;
            }
            if (_anchorObj) {
                Destroy(_anchorObj);
                _anchorObj = null;
            }
        }

        public bool IsItem { get; } = false;
    }
}
