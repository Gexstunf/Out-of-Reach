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
            _nextRamaID++; 
            Ramas.Add(_nextRamaID, new List<InfoScript>());
            return _nextRamaID;
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
                var first = Ramas[ramaID][0];
                var exit = first.transform.Find("Exit_1");

                if (exit != null)
                {
                    GameObject dummy = new GameObject("RecoveredExit");
                    dummy.transform.position = exit.position;
                    dummy.transform.rotation = exit.rotation;
                    primerExit = dummy.transform;
                    spawner.needToAdd = true;
                }

                foreach (var estructura in Ramas[ramaID])
                {
                    if (estructura != null)
                        Destroy(estructura.gameObject);
                }
            }

            Ramas.Remove(ramaID);
        }
        
        private void OnDrawGizmos()
        {
            if (Ramas == null) return;

            int colorIndex = 0;

            foreach (var rama in Ramas)
            {
                Color c = Color.HSVToRGB((colorIndex * 0.15f) % 1f, 1f, 1f);
                Gizmos.color = c;
                colorIndex++;

                var piezas = rama.Value;
                for (int i = 0; i < piezas.Count - 1; i++)
                {
                    if (piezas[i] != null && piezas[i + 1] != null)
                        Gizmos.DrawLine(piezas[i].transform.position, piezas[i + 1].transform.position);
                }
            }
        }
    }
}