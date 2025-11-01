using System;
using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class InfoScript : MonoBehaviour
    {
        [SerializeField] BranchDivisionScript branchDivision;
        public string estructuraID;
        private void Start()
        {
            branchDivision = FindAnyObjectByType<BranchDivisionScript>();
            if (gameObject.CompareTag("Hallway"))
            {
                if (!branchDivision.Rama.ContainsKey(estructuraID) && estructuraID != "")
                {
                    branchDivision.Rama.Add(estructuraID, this);
                }
                else
                {
                    estructuraID = Guid.NewGuid().ToString();
                    branchDivision.Rama.Add(estructuraID, this);
                }
            }
            else if (gameObject.CompareTag("Intersection"))
            {
                if (!branchDivision.EntreRama.ContainsKey(estructuraID) && estructuraID != "")
                {
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                else
                {
                    estructuraID = Guid.NewGuid().ToString();
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                branchDivision.AccionRama(false);
            }
            else if (gameObject.CompareTag("Room"))
            {
                if (!branchDivision.EntreRama.ContainsKey(estructuraID) && estructuraID != "")
                {
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                else
                {
                    estructuraID = Guid.NewGuid().ToString();
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                branchDivision.AccionRama(false);
            }
        }
    }
}
