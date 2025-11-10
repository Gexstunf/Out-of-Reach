using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class SpawnerScript : MonoBehaviour
    {
        private int _rooms;
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
            GameObject start = GameObject.Find("Exit_1");
            if (start == null) yield break;

            primeraSalida = start.transform;
            maxRooms = Random.Range(7, 10);

            List<Transform> currentRoots = new List<Transform>() { primeraSalida };

            while (currentRoots.Count > 0 && _rooms < maxRooms)
            {
                Debug.Log(_rooms);
                List<Transform> nextRoots = new List<Transform>();

                foreach (var root in currentRoots)
                {
                    Transform currentExit = root;
                    bool branchTerminated = false;

                    while (!branchTerminated && _rooms < maxRooms)
                    {
                        yield return null;

                        GameObject prefabToSpawn;
                        int t = Random.Range(1, 11);

                        if (t <= 8) prefabToSpawn = hallWay;
                        else if (t <= 9) prefabToSpawn = interSectionPossible[Random.Range(0, interSectionPossible.Length)];
                        else { prefabToSpawn = roomPossible[Random.Range(0, roomPossible.Length)]; _rooms++; }

                        GameObject spawned = FuncionInstanciar(prefabToSpawn, currentExit);
                        if (spawned == null)
                        {
                            Debug.LogWarning("Spawned null object");
                            break;
                        }

                        List<Transform> foundExits = new List<Transform>();

                        if (needToAdd)
                        {
                            foundExits.Add(branchDivision.primerEstructura.transform);
                        }
                        
                        foreach (Transform child in spawned.GetComponentsInChildren<Transform>())
                            if (child.name.StartsWith("Exit"))
                                foundExits.Add(child);

                        if (spawned.CompareTag("Hallway"))
                        {
                            if (foundExits.Count == 0)
                            {
                                Debug.LogWarning("Breaked");
                                break;
                            }
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
            Debug.Log("Terminó");
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
            
            spawned.transform.rotation = targetExit.rotation * Quaternion.Inverse(entry.localRotation);
            spawned.transform.position += targetExit.position - entry.position;
            
            Physics.SyncTransforms();
            
            return spawned;
        }
    }
}
//encontrar donde se esta parando;