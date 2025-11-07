using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class SpawnerScript : MonoBehaviour
    {
        private int _rooms;
        public int maxRooms;
        public GameObject hallWay;
        public GameObject[] interSectionPossible;
        public GameObject[] roomPossible;
        public Transform primeraSalida;

        private void Start()
        {
            StartCoroutine(Generate());
        }

        private IEnumerator Generate()
        {
            GameObject start = GameObject.Find("Exit_1");
            if (start == null) yield break;

            primeraSalida = start.transform;
            maxRooms = Random.Range(7, 10);

            List<Transform> currentRoots = new List<Transform>() { primeraSalida };

            while (currentRoots.Count > 0 && _rooms < maxRooms)
            {
                List<Transform> nextRoots = new List<Transform>();

                foreach (var root in currentRoots)
                {
                    Transform currentExit = root;
                    bool branchTerminated = false;

                    while (!branchTerminated && _rooms < maxRooms)
                    {
                        yield return null; // ← ***esta línea permite física y Start()***

                        GameObject prefabToSpawn;
                        int t = Random.Range(1, 11);

                        if (t <= 8) prefabToSpawn = hallWay;
                        else if (t <= 9) prefabToSpawn = interSectionPossible[Random.Range(0, interSectionPossible.Length)];
                        else { prefabToSpawn = roomPossible[Random.Range(0, roomPossible.Length)]; _rooms++; }

                        GameObject spawned = FuncionInstanciar(prefabToSpawn, currentExit);
                        if (spawned == null) break;

                        List<Transform> foundExits = new List<Transform>();
                        foreach (Transform child in spawned.GetComponentsInChildren<Transform>())
                            if (child.name.StartsWith("Exit"))
                                foundExits.Add(child);

                        if (spawned.CompareTag("Hallway"))
                        {
                            if (foundExits.Count == 0) break;
                            currentExit = foundExits[0];
                        }
                        else
                        {
                            foreach (var e in foundExits)
                                nextRoots.Add(e);

                            branchTerminated = true;
                        }
                    }
                }

                currentRoots = nextRoots;
            }
        }


        GameObject FuncionInstanciar(GameObject prefab, Transform targetExit)
        {
            GameObject spawned = Instantiate(prefab);

            Transform entry = spawned.transform.Find("Entry");
            if (entry == null)
            {
                Destroy(spawned);
                return null;
            }

            // Alineo rotación y posición
            spawned.transform.rotation = targetExit.rotation * Quaternion.Inverse(entry.localRotation);
            spawned.transform.position += targetExit.position - entry.position;
            
            Physics.SyncTransforms();

            // Aquí podrías obtener InfoScript y setear RamaID actual si vas a usar BranchDivision
            // var info = spawned.GetComponent<InfoScript>();
            // if (info != null) info.RamaID = currentRama; // (si implementás el tracking de rama)
            
            return spawned;
        }
    }
}

/*
 Colisiones / destrucción: si FuncionInstanciar devuelve null (p. ej. por falta de Entry o 
 porque decidiste destruirla al detectar overlap desde otro script), la rama termina silenciosamente
 y no agrega salidas. Si querés comportamiento distinto (ej: reintentar o marcar rama como fallida),
 implementalo en FuncionInstanciar o antes de añadir nextRoots.
 */
