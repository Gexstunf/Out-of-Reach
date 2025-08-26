using UnityEngine;

namespace Characters.Utils {
    public class ColliderUtilsScript : MonoBehaviour
    {
        [Header("Colliders to control")]
        public Collider[] colliders;

        public void SetCollidersTo(bool isEnabled) {
            foreach (Collider col in colliders) {
                col.enabled = isEnabled;
            }
        }
    }
}
