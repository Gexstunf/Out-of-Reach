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

        private void Awake()
        {
            GameObject start = GameObject.Find("Exit_1");
            if (start == null)
            {
                Debug.LogError("No se encontró Exit_1 en escena.");
                return;
            }
            primeraSalida = start.transform;

            maxRooms = Random.Range(7, 10);
            Debug.Log("Max Rooms: " + maxRooms);

            // Lista de raíces del nivel actual: cada raíz abre una rama que voy a desarrollar hasta su siguiente nodo (Room/Intersection)
            List<Transform> currentRoots = new List<Transform>() { primeraSalida };

            // Mientras haya raíces de ramas que procesar y no superemos maxRooms
            while (currentRoots.Count > 0 && _rooms < maxRooms)
            {
                List<Transform> nextRoots = new List<Transform>();

                // Procesar cada rama del nivel actual, UNA POR UNA, completa hasta su siguiente nodo
                foreach (var root in currentRoots)
                {
                    // Si llegamos al limite de rooms, salimos
                    if (_rooms >= maxRooms) break;

                    // Seguimos la rama desde esta root hasta encontrar Room/Intersection o hasta fallar
                    Transform currentExit = root;
                    bool branchTerminated = false;

                    while (!branchTerminated && _rooms < maxRooms)
                    {
                        // Spawn en la salida actual
                        GameObject prefabToSpawn;
                        int t = Random.Range(1, 16);

                        if (t <= 12) prefabToSpawn = hallWay;
                        else if (t <= 14) prefabToSpawn = interSectionPossible[Random.Range(0, interSectionPossible.Length)];
                        else { prefabToSpawn = roomPossible[Random.Range(0, roomPossible.Length)]; }

                        GameObject spawned = FuncionInstanciar(prefabToSpawn, currentExit);
                        if (spawned == null)
                        {
                            // Instanciación falló (p. ej. falta Entry o colisión detectada por otra lógica)
                            branchTerminated = true;
                            break;
                        }

                        // Si el prefab instanciado era Room, incrementamos contador de rooms
                        if (spawned.CompareTag("Room"))
                        {
                            _rooms++;
                        }

                        // Recolectamos las salidas de este prefab
                        List<Transform> foundExits = new List<Transform>();
                        foreach (Transform child in spawned.GetComponentsInChildren<Transform>())
                        {
                            if (child.name.StartsWith("Exit"))
                                foundExits.Add(child);
                        }

                        // Si el prefab es Hallway -> asumimos continuación (buscamos la salida que no es la entrada)
                        if (spawned.CompareTag("Hallway"))
                        {
                            // Si no hay exits, la rama termina aquí
                            if (foundExits.Count == 0)
                            {
                                branchTerminated = true;
                                break;
                            }

                            // Elegimos la salida que no coincide con la entrada (si hay más de 1, tomamos la primera por policy)
                            // Nota: en tu setup, lo típico es que un hallway tenga 1 Exit (la continuación).
                            // Tomamos la primera para seguir la rama.
                            currentExit = foundExits[0];
                            // Continuar en el while para expandir esa misma rama
                        }
                        else // si es Room o Intersection -> la rama alcanza un nodo importante => termina temporalmente
                        {
                            // Añadimos todas las salidas encontradas a nextRoots (serán las raíces del siguiente nivel de ramas)
                            foreach (var e in foundExits)
                            {
                                nextRoots.Add(e);
                            }

                            // Cerramos/terminamos esta rama por ahora
                            branchTerminated = true;
                            break;
                        }
                    } // end while rama

                    // Si la rama terminó sin llegar a nodo (por ejemplo instanciación fallida), no agregamos nada
                } // end foreach root

                // Avanzamos al siguiente nivel de ramas
                currentRoots = nextRoots;
            } // end while niveles

            Debug.Log("Generación terminada. Rooms generadas: " + _rooms);
        }

        GameObject FuncionInstanciar(GameObject prefab, Transform targetExit)
        {
            GameObject spawned = Instantiate(prefab);

            Transform entry = spawned.transform.Find("Entry");
            if (entry == null)
            {
                Debug.LogError(prefab.name + " no tiene Entry.");
                Destroy(spawned);
                return null;
            }

            // Alineo rotación y posición
            spawned.transform.rotation = targetExit.rotation * Quaternion.Inverse(entry.localRotation);
            spawned.transform.position += targetExit.position - entry.position;

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
