using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class IgnoreCollisionScript : MonoBehaviour
    {
        [Header("Ignore Collider settings")]
        public Collider[] collidersToIgnore;
        public Collider thisCollider;

        private void Awake() {
            thisCollider = GetComponent<Collider>();
        }

        private void Start() {
            foreach (var otherCollider in collidersToIgnore) {
                Physics.IgnoreCollision(otherCollider, thisCollider, true);
            }
        }
    }
}
