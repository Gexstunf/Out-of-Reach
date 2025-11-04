using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class InfoScript : MonoBehaviour
    {
        [SerializeField] BranchDivisionScript branchDivision;
        public static int RamaID = -1;
        public LayerMask layerMask = LayerMask.GetMask("Structure"); 

        private void Awake()
        {
            branchDivision = FindAnyObjectByType<BranchDivisionScript>();
        }
        
        private void Start()
        {
            if (gameObject.CompareTag("Intersection") && !IsOverlapping())
            {
                Debug.Log("No ta overlappeando");
                branchDivision.CerrarRama(RamaID, false);
                RamaID = branchDivision.CrearRama();
                branchDivision.AgregarAHilo(RamaID, this);
            }
            else if (gameObject.CompareTag("Room") && !IsOverlapping())
            {
                Debug.Log("No ta overlappeando");
                branchDivision.CerrarRama(RamaID, false);
                RamaID = branchDivision.CrearRama();
                branchDivision.AgregarAHilo(RamaID, this);
            }
            else if (IsOverlapping())
            {
                Debug.Log("Ta overlappeando");
                branchDivision.CerrarRama(RamaID, true);
            }
        }
        private bool IsOverlapping()
        {
            Collider myCol = GetComponent<Collider>();
            
            Collider[] others = Physics.OverlapBox(
                myCol.bounds.center,
                myCol.bounds.extents,
                transform.rotation
            );
            
            foreach (var other in others)
            {
                if (other != myCol)
                {
                    Vector3 dir;
                    float distance;
                    if (other.CompareTag("Room") || other.CompareTag("Hallway") || other.CompareTag("Intersection"))
                    {
                        if (Physics.ComputePenetration(
                                myCol, transform.position, transform.rotation,
                                other, other.transform.position, other.transform.rotation,
                                out dir, out distance))
                        {
                            if (distance < 0.1f)
                            {
                                return true;
                            }
                            //Cambiar los prefabs para que queden perfectos
                        }
                    }
                }
            }
            return false;
        }
    }
}