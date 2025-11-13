using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class InfoScript : MonoBehaviour
    {
        private BranchDivisionScript _branchDivision;
        private SpawnerScript _spawner;
        
        public int RamaID { set; get;}
        public int visualizeId;
        public Collider overlapCheckCollider;

        public float drawSeconds = 10f;
        private bool _draw = false;
        
        private bool _overlapped = false;
        
        public bool IsOverlappingWithShip => _overlapped;

        private void Awake()
        {
            _branchDivision = FindAnyObjectByType<BranchDivisionScript>();
            _spawner = FindAnyObjectByType<SpawnerScript>();
        }

        private void Start()
        {
            // bool overlap = IsOverlapping();
            //
            // if (overlap)
            // {
            //     branchDivision.KillBranch(RamaID);
            //     //Destroy(gameObject);
            //     return;
            // }
            
        }

        public void SetId(int id) {
            RamaID = id;
            visualizeId = RamaID;
        }

        public bool IsOverlapping() {
            var wastrigger = overlapCheckCollider.isTrigger;
            overlapCheckCollider.isTrigger = false; // temporarily make it solid
            Physics.SyncTransforms();

            if (!overlapCheckCollider) return false;
            Vector3 worldCenter = overlapCheckCollider.transform.position;
            Quaternion worldRotation = overlapCheckCollider.transform.rotation;
            Collider[] hits = Physics.OverlapBox(worldCenter, overlapCheckCollider.bounds.size, worldRotation);

            foreach (var other in hits)
            {
                if (other == overlapCheckCollider) continue;
                if (other.transform.root == transform.root) continue;

                bool isShip = other.CompareTag("Room") ||
                              other.CompareTag("Hallway") ||
                              other.CompareTag("Intersection");

                if (isShip)
                {
                    Debug.Log($"{other.name} is overlapping.");
                    return true;
                }
            }
            Debug.Log($"{hits.Length} overlapped objects.");
            overlapCheckCollider.isTrigger = wastrigger; // reset
            return false;
        }

        private void OnTriggerStay(Collider other) {
            _overlapped = true;
        }

        private void OnDrawGizmos() {
            if (drawSeconds < 0 && _draw) return;
            
            Vector3 worldCenter = overlapCheckCollider.transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(worldCenter, overlapCheckCollider.bounds.size);
            drawSeconds -= Time.deltaTime;
        }   
    }
}