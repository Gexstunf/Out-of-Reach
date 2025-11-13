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
            
            if (gameObject.CompareTag("Room"))
            {
                _spawner.AddToRoomCount(1);
            }
            _branchDivision.AddObjToBranch(RamaID, this);
        }

        public void SetId(int id) {
            RamaID = id;
            visualizeId = RamaID;
        }

        public bool IsOverlapping()
        {
            Physics.SyncTransforms();

            if (!overlapCheckCollider) return false;
            
            Collider[] hits = Physics.OverlapBox(
                overlapCheckCollider.bounds.center,
                overlapCheckCollider.bounds.extents,
                transform.rotation
            );

            foreach (var other in hits)
            {
                if (other == overlapCheckCollider) continue;
                if (other.transform.root == transform.root) continue;
                
                bool isShip = (other.CompareTag("Room") || other.CompareTag("Hallway") || other.CompareTag("Intersection"));
                
                if (Physics.ComputePenetration(
                        overlapCheckCollider, transform.position, transform.rotation,
                        other, other.transform.position, other.transform.rotation,
                        out Vector3 direction, out float distance))
                {
                    if (distance > 0.05f && isShip) {
                        Debug.Log(other.name + " is overlapping.");
                        return true;
                    } 
                    _draw = true;
                }
            }

            return false;
        }

        private void OnTriggerStay(Collider other) {
            _overlapped = true;
        }

        private void OnDrawGizmos() {
            if (drawSeconds < 0 && _draw) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(overlapCheckCollider.bounds.center, overlapCheckCollider.bounds.extents);
            drawSeconds -= Time.deltaTime;
        }
    }
}