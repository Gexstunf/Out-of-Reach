using UnityEngine;

namespace Characters.Utils {
    public class ColliderUtilsScript : MonoBehaviour
    {
        [Header("Colliders to control")]
        public Collider[] colliders;

        public void SetCollidersToTriggers(bool isTrigger) {
            foreach (Collider col in colliders) {
                col.isTrigger = isTrigger;
            }
        }
    }
}
