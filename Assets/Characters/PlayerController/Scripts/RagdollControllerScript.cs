using System;
using Characters.Utils;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class RagdollControllerScript : MonoBehaviour {
        [Header("Flags Settings")] 
        public bool ignoreEachOtherColliders = true;
        public bool detectCollisionsRigidbodies = true;
        public bool useGravityRigidbodies = true;
        public bool rigidbodiesKinematic = false;
        public bool collidersAreTriggers = false;

        [Header("Utils")]
        public RigidbodyUtilsScript RbUtils;
        public ColliderUtilsScript ColUtils;
        
        [Header("Colliders to control")]
        public Collider[] colliders;
        
        [Header("Rigidbodies to control")]
        public Rigidbody[] rigidbodies;
        public Rigidbody mainRb;

        public void Awake() {
            RbUtils = new RigidbodyUtilsScript(rigidbodies, mainRb);
            ColUtils = new ColliderUtilsScript(colliders);
        }

        private void Start() {
            ApplyRagdollSettings(ignoreEachOtherColliders, collidersAreTriggers, detectCollisionsRigidbodies, useGravityRigidbodies, rigidbodiesKinematic);
        }

        public void ApplyRagdollSettings(bool ignoreColBetweenCol, bool colAreTriggers, bool rbDetectCollisions, bool useGravityRb, bool rbsKinematic) {
            RbUtils.SetDetectCollisions(rbDetectCollisions);
            RbUtils.SetUseGravity(useGravityRb);
            RbUtils.SetKinematicRigidbodies(rbsKinematic);
            ColUtils.SetIgnoreCollidersBetweenEachOther(ignoreColBetweenCol);
            ColUtils.SetCollidersToTriggers(colAreTriggers);
        }
    }
}
