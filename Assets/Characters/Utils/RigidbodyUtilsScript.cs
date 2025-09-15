using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Characters.Utils {
    public class RigidbodyUtilsScript
    {
        [Header("Rigidbodies to control")]
        public Rigidbody[] Rigidbodies;
        public Rigidbody CurrentRb;

        public RigidbodyUtilsScript(Rigidbody[] rigidbodies, Rigidbody currentRb) {
            Rigidbodies = rigidbodies;
            CurrentRb = currentRb;
        }

        public void SetKinematicRigidbodies(bool isKinematic) {
            foreach (Rigidbody rb in Rigidbodies) {
                rb.isKinematic = isKinematic;
            }
        }

        public void SetDetectCollisions(bool detectCollisions) {
            foreach (var rb in Rigidbodies) {
                rb.detectCollisions = detectCollisions;
            }
        }

        public void SetUseGravity(bool useGrav) {
            foreach (var rb in Rigidbodies) {
                rb.useGravity = useGrav;
            }
        }
    }
}
