using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class InfoScript : MonoBehaviour
    {
        [SerializeField] BranchDivisionScript branchDivision;
        public static int RamaID = -1;

        private void Awake()
        {
            branchDivision = FindAnyObjectByType<BranchDivisionScript>();
        }

        private void Start()
        {
            if (gameObject.CompareTag("Intersection"))
            {
                branchDivision.CerrarRama(RamaID, false);
                RamaID = branchDivision.CrearRama();
                branchDivision.AgregarAHilo(RamaID, this);
            }
            else if (gameObject.CompareTag("Room"))
            {
                branchDivision.CerrarRama(RamaID, false);
                RamaID = branchDivision.CrearRama();
                branchDivision.AgregarAHilo(RamaID, this);
            }
            Debug.Log(RamaID);
        }
    }
}