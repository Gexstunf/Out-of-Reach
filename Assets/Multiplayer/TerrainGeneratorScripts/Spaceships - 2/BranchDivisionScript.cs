using UnityEngine;
using System.Collections.Generic;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class BranchDivisionScript : MonoBehaviour
    {
        public Dictionary<int, List<InfoScript>> Ramas = new();

        private int nextRamaID = 0;

        public int CrearRama()
        {
            Ramas.Add(nextRamaID, new List<InfoScript>());
            return nextRamaID++;
        }

        public void AgregarAHilo(int ramaID, InfoScript estructura)
        {
            Ramas[ramaID].Add(estructura);
        }

        public void CerrarRama(int ramaID, bool choco)
        {
            if (!Ramas.ContainsKey(ramaID)) return;

            if (choco)
            {
                foreach (var estructura in Ramas[ramaID])
                {
                    if (estructura != null)
                        Destroy(estructura.gameObject);
                }
            }

            Ramas.Remove(ramaID);
        }
    }
}