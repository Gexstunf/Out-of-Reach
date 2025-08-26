using UnityEngine;

namespace Characters.Utils {
    public class RigidbodyUtilsScript : MonoBehaviour
    {
        [Header("Rigidbodies to control")]
        public Rigidbody[] rigidbodies;

        public void SetKinematicRigidbodies(bool isKinematic) {
            foreach (Rigidbody rb in rigidbodies) {
                rb.isKinematic = isKinematic;
            }
        }
    }
}
