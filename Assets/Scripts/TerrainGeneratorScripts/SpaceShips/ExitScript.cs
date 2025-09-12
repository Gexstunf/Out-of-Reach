using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerrainGeneratorScripts.SpaceShips
{
    public class ExitScript : MonoBehaviour
    {
        public static Dictionary<string, ExitScript> allExits = new();
        public string exitID;
        public bool isActive;
        public float activationChance;
        
        [Header("Collision Detection")]
        [SerializeField] private LayerMask collisionLayers = -1;
        [SerializeField] private bool includeOwnCollider = false;
        private BoxCollider boxCollider;
        private MeshCollider meshCollider;
        private Collider collider;

        public void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
            meshCollider = GetComponent<MeshCollider>();

            for (int i = 0; i < 1; i++)
            {
                if (CheckForCollisions())
                {
                    DeactivateExit();
                    Destroy(gameObject);
                    return;
                }
            }
            
            // Original activation logic
            isActive = Random.Range(0f, 1f) <= activationChance;
            //Debug.Log($"Exit {name} isActive: {isActive}");
        }
        
        private bool CheckForCollisions()
        {
            if (boxCollider == null)
            {
                collider = meshCollider;
                return false;
            }
            else
            {
                collider = boxCollider;
            }

            // Get the bounds of the box collider in world space
            Bounds bounds = collider.bounds;
            Vector3 center = bounds.center;
            Vector3 halfExtents = bounds.extents;

            // Check for overlapping colliders
            Collider[] overlapping = Physics.OverlapBox(center, halfExtents, transform.rotation, collisionLayers);

            // Check if any overlapping colliders are found (excluding own collider if specified)
            bool hasOverlap = false;
            foreach (Collider col in overlapping)
            {
                if (!includeOwnCollider && col == collider)
                    continue;
                    
                hasOverlap = true;
                break;
            }

            return hasOverlap;
        }

        public void SetExitNumber()
        {
            if (isActive)
            {
                if (!allExits.ContainsKey(exitID))
                {
                    allExits.Add(exitID, this);
                }
                else
                {
                    // Generate new ID if collision occurs
                    exitID = System.Guid.NewGuid().ToString();
                    allExits.Add(exitID, this);
                }
            }
        }

        public void DeactivateExit()
        {
            isActive = false;
            // Remove from active exits dictionary when deactivated
            if (!string.IsNullOrEmpty(exitID) && allExits.ContainsKey(exitID))
            {
                allExits.Remove(exitID);
            }
            //Debug.Log($"Exit {name} deactivated and removed from active exits");
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(exitID) && allExits.ContainsKey(exitID))
            {
                allExits.Remove(exitID);
            }
        }
    
        public static void ClearAllExits()
        {
            allExits.Clear();
        }
    }
}
