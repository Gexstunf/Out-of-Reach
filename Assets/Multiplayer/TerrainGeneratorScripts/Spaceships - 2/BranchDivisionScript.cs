using UnityEngine;
using System.Collections.Generic;


namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class BranchDivisionScript : MonoBehaviour
    {
        public Dictionary<int, List<InfoScript>> Ramas = new();
        public Transform primerExit;
        public SpawnerScript spawner;
        private static int _nextRamaID = -1;
        public int CrearRama()
        {
            Ramas.Add(_nextRamaID, new List<InfoScript>());
            return _nextRamaID++;
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
                // Guardar el exit ANTES de destruir
                var first = Ramas[ramaID][0];
                var exit = first.transform.Find("Exit_1"); // o detectá cuál exit es el "padre"


                if (exit != null)
                {
                    primerExit = exit;
                    spawner.needToAdd = true;
                }


                // Ahora si destruis la rama
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