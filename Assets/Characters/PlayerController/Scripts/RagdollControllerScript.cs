using System;
using Characters.Utils;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class RagdollControllerScript : MonoBehaviour
    {
        [Header("Ragdoll Settings")]
        public GameObject ragdollPrefab;
        public GameObject fpcControllerPrefab;
        public static GameObject FpcController;
        
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
            FpcController = gameObject;
        }

        public void SpawnRagdoll(Vector3 position, Quaternion rotation) {
            GameObject ragdoll = Instantiate(ragdollPrefab, position, rotation);
            MatchRagdollLimbsToBones(ragdoll, FpcController);
            Destroy(FpcController);
        }

        public void SpawnFirstPersonController(Vector3 position, Quaternion rotation) {
            GameObject fpc = Instantiate(fpcControllerPrefab, position, rotation);
            FpcController = fpc;
        } 

        public void MatchRagdollLimbsToBones(GameObject ragdoll, GameObject controller) {
            GameObject model = controller.transform.Find("Model").gameObject; 
        }
    }
}
