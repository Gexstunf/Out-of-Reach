using UnityEngine;

namespace Characters.Utils {
    public class ColliderUtilsScript
    {
        [Header("Colliders to control")]
        public Collider[] Colliders;

        public ColliderUtilsScript(Collider[] colliders) {
            Colliders = colliders;
        }

        public void SetCollidersToTriggers(bool isTrigger) {
            foreach (Collider col in Colliders) {
                col.isTrigger = isTrigger;
            }
        }

        public void IgnoreCollidersBetweenEachOther(bool ignore) {
            foreach (var currentCol in Colliders) {
                foreach (var ignoreCol in Colliders) {
                    if (currentCol != ignoreCol) {
                        Physics.IgnoreCollision(currentCol, ignoreCol, ignore);
                    } 
                }
            }
        }
    }
}
