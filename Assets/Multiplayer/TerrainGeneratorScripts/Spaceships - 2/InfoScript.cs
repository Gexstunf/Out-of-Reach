using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class InfoScript : MonoBehaviour
    {
        [SerializeField] BranchDivisionScript branchDivision;
        [SerializeField] SpawnerScript spawner;
        public int RamaID = -1;

        private void Awake()
        {
            branchDivision = FindAnyObjectByType<BranchDivisionScript>();
            spawner = FindAnyObjectByType<SpawnerScript>();
        }

        private void Start()
        {
            bool overlap = IsOverlapping();
            
            if (overlap)
            {
                branchDivision.CerrarRama(RamaID, true);
                Destroy(gameObject);
                return;
            }
            
            if (gameObject.CompareTag("Room"))
            {
                spawner.rooms++;
            }
            branchDivision.AgregarAHilo(RamaID, this);
        }

        private bool IsOverlapping()
        {
            Physics.SyncTransforms();

            Collider myCol = GetComponent<Collider>();
            if (myCol == null) return false;
            
            Collider[] hits = Physics.OverlapBox(
                myCol.bounds.center,
                myCol.bounds.extents,
                transform.rotation
            );

            foreach (var other in hits)
            {
                if (other == myCol) continue;
                if (other.transform.root == transform.root) continue;
                
                if (Physics.ComputePenetration(
                        myCol, transform.position, transform.rotation,
                        other, other.transform.position, other.transform.rotation,
                        out Vector3 direction, out float distance))
                {
                    if (distance < 0.05f) // contacto leve → OK
                        continue;
                    if (other.CompareTag("Room") || other.CompareTag("Hallway") || other.CompareTag("Intersection"))
                        return true;
                }
            }

            return false;
        }

    }
}