using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;


namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class SpawnerScript : MonoBehaviour
    {
        public int rooms;
        public int maxRooms;
        public BranchDivisionScript branchDivision;
        public GameObject hallWay;
        public GameObject[] interSectionPossible;
        public GameObject[] roomPossible;
        public Transform primeraSalida;
        public bool needToAdd;


        private void Start()
        {
            needToAdd = false;
            StartCoroutine(Generate());
        }
        
        private IEnumerator Generate()
       {
           Debug.Log("[Spawner] Generate START");
           GameObject start = GameObject.Find("Exit_1");
           if (start == null)
           {
               Debug.LogError("[Spawner] No encontré Exit_1 -> yield break");
               yield break;
           }
      
           primeraSalida = start.transform;
           maxRooms = Random.Range(7, 10);
           Debug.Log($"[Spawner] Maximo de cuartos: {maxRooms}");
      
           List<Transform> currentRoots = new List<Transform>() { primeraSalida };
           int outerLoop = 0;
      
           while (currentRoots.Count > 0 && rooms < maxRooms)
           {
               outerLoop++;
               if (outerLoop % 10 == 0) Debug.Log($"[Spawner] Loop externo #{outerLoop} - currentRoots: {currentRoots.Count} rooms: {rooms}/{maxRooms}");
      
               List<Transform> nextRoots = new List<Transform>();
      
               foreach (var root in currentRoots)
               {
                   Debug.Log($"[Spawner] Procesando root: {root.name}");
                   Transform currentExit = root;
                   bool branchTerminated = false;
                   int hallwayChain = 0;
                   int innerLoop = 0;
      
                   while (!branchTerminated && rooms < maxRooms)
                   {
                       innerLoop++;
                       // LOG antes de yield para ver si la coroutine sigue
                       Debug.Log($"[Spawner]  -- inner #{innerLoop} rooms:{rooms} exit:{currentExit.name}");
                       yield return null;
      
                       GameObject prefabToSpawn;
                       int t = Random.Range(1, 11);
      
                       if (t <= 8) prefabToSpawn = hallWay;
                       else if (t <= 9) prefabToSpawn = interSectionPossible[Random.Range(0, interSectionPossible.Length)];
                       else { prefabToSpawn = roomPossible[Random.Range(0, roomPossible.Length)]; rooms++; }
      
                       Debug.Log($"[Spawner] Instanciando t={t} prefab={prefabToSpawn?.name}");
      
                       GameObject spawned = FuncionInstanciar(prefabToSpawn, currentExit);
                       yield return new WaitForSecondsRealtime(1.5f);
                       if (spawned == null)
                       {
                           Debug.LogWarning("[Spawner] Spawned NULL, rompiendo rama");
                           break;
                       }
      
                       List<Transform> foundExits = new List<Transform>();
                       foreach (Transform child in spawned.GetComponentsInChildren<Transform>())
                           if (child.name.StartsWith("Exit"))
                               foundExits.Add(child);
      
                       if (needToAdd && branchDivision != null && branchDivision.primerExit != null)
                       {
                           Debug.Log("[Spawner] Agregando primerExit de branchDivision");
                           foundExits.Add(branchDivision.primerExit);
                           needToAdd = false;
                       }
      
                       foundExits.RemoveAll(e => e == null);
                       Debug.Log($"[Spawner]    foundExits.count={foundExits.Count} (list: {string.Join(", ", foundExits.ConvertAll(f => f.name))})");
      
                       try
                       {
                           if (spawned.CompareTag("Hallway"))
                           {
                               hallwayChain++;
                               Debug.Log($"[Spawner]    Es Hallway, hallwayChain={hallwayChain}");
                               if (foundExits.Count == 0)
                               {
                                   Debug.LogWarning("[Spawner] Hallway SIN exits -> rompiendo rama");
                                   break;
                               }
      
                               if (hallwayChain > 30)
                               {
                                   Debug.LogWarning("[Spawner] Limite hallwayChain alcanzado -> forzando terminacion de rama");
                                   branchTerminated = true;
                                   break;
                               }
      
                               currentExit = foundExits[0];
                           }
                           else
                           {
                               hallwayChain = 0;
                               foreach (var e in foundExits)
                                   nextRoots.Add(e);
      
                               branchTerminated = true;
                           }
                       }
                       catch (System.Exception exInner)
                       {
                           Debug.LogError($"[Spawner] EXCEPTION dentro del manejo de spawned: {exInner}");
                           branchTerminated = true;
                       }
                   } // end inner while
               } // end foreach roots
      
               currentRoots = nextRoots;
               Debug.Log($"[Spawner] Fin iter. nextRoots: {currentRoots.Count} rooms: {rooms}");
           } // end outer while
      
           Debug.Log("[Spawner] Generate FINISHED");
       }
      


       GameObject FuncionInstanciar(GameObject prefab, Transform targetExit)
       {
           if (prefab == null)
           {
               Debug.LogError("[Spawner] Prefab es NULL");
               return null;
           }
      
           GameObject spawned = null;
           try
           {
               spawned = Instantiate(prefab);
           }
           catch (System.Exception ex)
           {
               Debug.LogError($"[Spawner] Exception en Instantiate: {ex}");
               if (spawned != null) Destroy(spawned);
               return null;
           }
      
           try
           {
               Transform entry = spawned.transform.Find("Entry");
               if (entry == null)
               {
                   Debug.LogWarning("[Spawner] Spawned NO tiene Entry -> destruyo");
                   Destroy(spawned);
                   return null;
               }
      
               spawned.transform.rotation = targetExit.rotation * Quaternion.Inverse(entry.localRotation);
               spawned.transform.position += targetExit.position - entry.position;
      
               Physics.SyncTransforms();
           }
           catch (System.Exception ex)
           {
               Debug.LogError($"[Spawner] Exception al posicionar spawned: {ex}");
               if (spawned != null) Destroy(spawned);
               return null;
           }
      
           return spawned;
       }
      
       private void OnDisable() => Debug.Log("[Spawner] OnDisable llamado");
       private void OnDestroy() => Debug.Log("[Spawner] OnDestroy llamado");
   }
}
//encontrar donde se esta parando;