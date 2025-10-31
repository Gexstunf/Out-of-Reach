using UnityEngine;
using System.Collections.Generic;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class BranchDivisionScript : MonoBehaviour
    {
        public Dictionary<string, InfoScript> Rama = new();
        public Dictionary<string, InfoScript> EntreRama = new();

        public void AccionRama(bool choco)
        {
            if (choco)
            {
                foreach (var estructura in Rama.Values)
                {
                    Destroy(estructura.gameObject);
                }
                Rama.Clear();
            }
            else
            {
                Rama.Clear();
            }
        }
    }
}


/* Me falta:
    Collisionador
    Detector de colisiones
*/