using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Characters.Utils {
    public class RigidbodyUtilsScript : MonoBehaviour
    {
        [Header("Rigidbodies to control")]
        public Rigidbody[] rigidbodies;
        public Rigidbody currentRb;

        public void Start() {
            currentRb = GetComponent<Rigidbody>();
        }

        public void SetKinematicRigidbodies(bool isKinematic) {
            foreach (Rigidbody rb in rigidbodies) {
                rb.isKinematic = isKinematic;
            }
            currentRb.isKinematic = !isKinematic;
        }

        public void SetDetectCollisions(bool detectCollisions) {
            foreach (var rb in rigidbodies) {
                rb.detectCollisions = detectCollisions;
            }
        }

        public void SetUseGravity(bool useGrav) {
            foreach (var rb in rigidbodies) {
                rb.useGravity = useGrav;
            }
        }
    }
}
