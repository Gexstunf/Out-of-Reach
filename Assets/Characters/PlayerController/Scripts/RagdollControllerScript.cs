using System;
using System.Collections.Generic;
using Characters.ActiveRagdollSystem;
using Characters.Utils;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class RagdollControllerScript : MonoBehaviour {
        [Header("References")] 
        public ActiveRagdollCoreScript activeRagdoll;
        
        [Header("Flags Settings")] 
        public bool modifyRbs = true;
        public bool modifyCols = true;
        public bool ignoreEachOtherColliders = true;
        public bool detectCollisionsRigidbodies = true;
        public bool useGravityRigidbodies = true;
        public bool rigidbodiesKinematic = false;
        public bool collidersAreTriggers = false;
        public LayerMask ignoreLayer = 0;

        [Header("Utils")]
        public RigidbodyUtilsScript RbUtils;
        public ColliderUtilsScript ColUtils;
        
        [Header("Colliders to control")]
        public List<Collider> colliders;
        
        [Header("Rigidbodies to control")]
        public List<Rigidbody> rigidbodies;
        public Rigidbody mainRb;

        private void Awake() {
            activeRagdoll = GetComponent<ActiveRagdollCoreScript>();
        }

        private void Start() {
            foreach (var bone in activeRagdoll.boneMaps) {
                if (bone.collider) colliders.Add(bone.collider);
                if (bone.rb) rigidbodies.Add(bone.rb);
            }
            
            RbUtils = new RigidbodyUtilsScript(rigidbodies.ToArray(), mainRb);
            ColUtils = new ColliderUtilsScript(colliders.ToArray());
            ApplyRagdollSettings(ignoreEachOtherColliders, collidersAreTriggers, detectCollisionsRigidbodies, useGravityRigidbodies, rigidbodiesKinematic, ignoreLayer);
        }

        private void ApplyRagdollSettings(bool ignoreColBetweenCol, bool colAreTriggers, bool rbDetectCollisions, bool useGravityRb, bool rbsKinematic, LayerMask ignoreLayers) {
            if (modifyRbs) {
                RbUtils.SetDetectCollisions(rbDetectCollisions);
                RbUtils.SetUseGravity(useGravityRb);
                RbUtils.SetKinematicRigidbodies(rbsKinematic);
            }

            if (modifyCols) {
                ColUtils.SetIgnoreCollidersBetweenEachOther(ignoreColBetweenCol);
                ColUtils.SetCollidersToTriggers(colAreTriggers);
                ColUtils.SetExcludeLayer(ignoreLayers);
            }
        }

        public void IgnoreInternalCollisions(bool ignore) {
            ColUtils.SetIgnoreCollidersBetweenEachOther(ignore);
            ignoreEachOtherColliders = ignore;
        }
    }
}
