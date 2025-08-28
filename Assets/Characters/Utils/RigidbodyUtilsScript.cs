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
    }
}
