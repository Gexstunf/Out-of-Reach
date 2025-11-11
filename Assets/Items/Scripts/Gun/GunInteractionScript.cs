using System.Collections.Generic;
using Multiplayer.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items.Scripts.Gun {
    public class GunInteractionScript : ItemInteractionScript
    {
        [Header("References")]
        public GameObject projectilePrefab;
        public List<GameObject> projectileList;
        
        [Header("Settings")]
        public float projectileForce = 1f;
        public float projectileRange = 10f;
        public float projectileDamage = 35f;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public override void Interact() {
            var projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            
            rb.AddForce(transform.forward * projectileForce, ForceMode.Impulse);

            projectileList.Add(projectile); // track instantiations
        }
    }
}
