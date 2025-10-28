using System;
using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class InfoScript : MonoBehaviour
    {
        private BranchDivisionScript branchDivision;
        public string estructuraID;
        public bool collision;
        private void Awake()
        {
            branchDivision = GetComponent<BranchDivisionScript>();
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
                branchDivision.AccionRama(false);
                if (!branchDivision.EntreRama.ContainsKey(estructuraID) && estructuraID != "")
                {
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                else
                {
                    estructuraID = Guid.NewGuid().ToString();
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
            }
            else if (gameObject.CompareTag("Room"))
            {
                branchDivision.AccionRama(false);
                if (!branchDivision.EntreRama.ContainsKey(estructuraID) && estructuraID != "")
                {
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
                else
                {
                    estructuraID = Guid.NewGuid().ToString();
                    branchDivision.EntreRama.Add(estructuraID, this);
                }
            }
        }
    }
}
