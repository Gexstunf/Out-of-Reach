using System;
using UnityEngine;

namespace Characters.EconomySystem {
    public class LiftZoneScript : MonoBehaviour
    {
        [Header("Settings")]
        public float liftForce = 7f;
        public float addedDrag = 5f;
        public float angularDrag = 1f;


        private void OnTriggerEnter(Collider other) {
            if (other.attachedRigidbody) {
                other.attachedRigidbody.linearDamping += addedDrag;
                other.attachedRigidbody.angularDamping += angularDrag;

                other.attachedRigidbody.useGravity = false;
            }
        }
        
        private void OnTriggerStay(Collider other) {
            if (other.attachedRigidbody) {
                other.attachedRigidbody.AddForce(Vector3.up * liftForce);
            }
        }
        
        private void OnTriggerExit(Collider other) {
            if (other.attachedRigidbody) {
                other.attachedRigidbody.linearDamping -= addedDrag;
                other.attachedRigidbody.angularDamping -= angularDrag;

                other.attachedRigidbody.useGravity = true;
            }
        }
    }
}