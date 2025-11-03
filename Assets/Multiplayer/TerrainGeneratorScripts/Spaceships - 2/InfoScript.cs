using System;
using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class InfoScript : MonoBehaviour
    {
        [SerializeField] BranchDivisionScript branchDivision;
        public static int RamaID = -1;
        public LayerMask layerMask = LayerMask.GetMask("Terrain");

        private void Awake()
        {
            branchDivision = FindAnyObjectByType<BranchDivisionScript>();
        }
        
        private void Start()
        {
            Collider myCol = GetComponent<Collider>();
            
            if (gameObject.CompareTag("Intersection") && !IsOverlapping(layerMask, myCol.bounds.extents))
            {
                branchDivision.CerrarRama(RamaID, false);
                RamaID = branchDivision.CrearRama();
                branchDivision.AgregarAHilo(RamaID, this);
            }
            else if (gameObject.CompareTag("Room") && !IsOverlapping(layerMask, myCol.bounds.extents))
            {
                branchDivision.CerrarRama(RamaID, false);
                RamaID = branchDivision.CrearRama();
                branchDivision.AgregarAHilo(RamaID, this);
            }
            else if (IsOverlapping(layerMask, myCol.bounds.extents))
            {
                Debug.Log("Ta overlappeando");
                branchDivision.CerrarRama(RamaID, true);
            }
        }
        private bool IsOverlapping(LayerMask layer, Vector3 halfExtents)
        {
            Collider[] hits = Physics.OverlapBox(
                transform.position,
                halfExtents,
                transform.rotation,
                layer
            );
            
            Debug.Log(hits.Length);
            
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject)
                    continue;
                
                if (hit.CompareTag("Room") || hit.CompareTag("Hallway") || hit.CompareTag("Intersection"))
                {
                    return true;
                }
            }
            return hits.Length > 0;
        }
    }
}