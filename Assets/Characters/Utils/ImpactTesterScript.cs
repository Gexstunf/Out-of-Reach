using UnityEngine;

namespace Characters.Utils {
    public class ImpactTesterScript : MonoBehaviour {
        public float projectileSpeed = 50f;
        public float impactForce = 100f;
        public GameObject projectilePrefab;

        public bool shoot;

        void Update() {
            if (shoot) {
                // Instantiate the projectile
                GameObject projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.linearVelocity = transform.forward * projectileSpeed;
                }
                
                shoot = false;
            }
        }
    }
}

