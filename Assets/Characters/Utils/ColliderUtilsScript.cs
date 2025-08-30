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

        public void IgnoreCollidersBetweenEachOther() {
            foreach (var currentCol in colliders) {
                foreach (var ignoreCol in colliders) {
                    if (currentCol != ignoreCol) {
                        Physics.IgnoreCollision(currentCol, ignoreCol);
                    }
                }
            }
        }
    }
}
