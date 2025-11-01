using System;
using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class InfoScript : MonoBehaviour
    {
        [SerializeField] BranchDivisionScript branchDivision;
        public int ramaID = -1;

        private void Awake()
        {
            branchDivision = FindAnyObjectByType<BranchDivisionScript>();
        }

        private void Start()
        {
            if (ramaID == -1)
                ramaID = branchDivision.CrearRama();
            branchDivision.AgregarAHilo(ramaID, this);

            if (gameObject.CompareTag("Intersection"))
            {
                branchDivision.CerrarRama(ramaID, false);
            }
            else if (gameObject.CompareTag("Room"))
            {
                branchDivision.CerrarRama(ramaID, false);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            branchDivision.CerrarRama(ramaID, true);
        }

    }
}