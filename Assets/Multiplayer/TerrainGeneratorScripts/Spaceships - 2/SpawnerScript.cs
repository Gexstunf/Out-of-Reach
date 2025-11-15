using System.Collections;
using UnityEngine;
using System.Collections.Generic;

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
            rooms = 0;
            needToAdd = false;

            StartCoroutine(Generate());
        }

        private IEnumerator Generate()
        {
            GameObject start = GameObject.Find("Exit_1");
            if (start == null) {
                yield break;
                //return;
            }
            
            int nuev = branchDivision.CrearRama();
            foreach (var info in start.GetComponentsInChildren<InfoScript>())
                info.RamaID = nuev;

            primeraSalida = start.transform;
            maxRooms = Random.Range(7, 10);

            List<Transform> currentRoots = new List<Transform>() { primeraSalida };

            while (currentRoots.Count > 0 && rooms < maxRooms)
            {
                List<Transform> nextRoots = new List<Transform>();

                foreach (var root in currentRoots)
                {
                    Transform currentExit = root;
                    bool branchTerminated = false;
                    int hallwayChain = 0;

                    while (!branchTerminated && rooms < maxRooms)
                    {
                        yield return null;

                        GameObject prefabToSpawn;
                        int t = Random.Range(1, 10);

                        if (t <= 6) prefabToSpawn = hallWay;
                        else if (t <= 8) prefabToSpawn = interSectionPossible[Random.Range(0, interSectionPossible.Length)];
                        else prefabToSpawn = roomPossible[Random.Range(0, roomPossible.Length)];

                        GameObject spawned = FuncionInstanciar(prefabToSpawn, currentExit);
                        yield return new WaitForSecondsRealtime(0.1f);
                        if (spawned == null) break;

                        List<Transform> foundExits = new List<Transform>();
                        foreach (Transform child in spawned.GetComponentsInChildren<Transform>())
                            if (child.name.StartsWith("Exit"))
                                foundExits.Add(child);

                        if (needToAdd && branchDivision.primerExit != null)
                        {
                            foundExits.Add(branchDivision.primerExit);
                            needToAdd = false;
                        }

                        if (spawned.CompareTag("Hallway"))
                        {
                            hallwayChain++;
                            if (hallwayChain > 30)
                            {
                                branchTerminated = true;
                                int nueva = branchDivision.CrearRama();
                                foreach (var info in spawned.GetComponentsInChildren<InfoScript>())
                                    info.RamaID = nueva;

                                break;
                            }

                            currentExit = foundExits[0];
                        }
                        else
                        {
                            hallwayChain = 0;
                            nextRoots.AddRange(foundExits);

                            branchTerminated = true;
                            int nueva = branchDivision.CrearRama();
                            foreach (var info in spawned.GetComponentsInChildren<InfoScript>())
                                info.RamaID = nueva;

                        }
                    }
                }

                currentRoots = nextRoots;
            }
        }

        GameObject FuncionInstanciar(GameObject prefab, Transform targetExit)
        {
            if (prefab == null) return null;
            if (targetExit == null) return null;

            GameObject spawned = Instantiate(prefab);

            Transform entry = null;
            foreach (Transform t in spawned.GetComponentsInChildren<Transform>(true))
                if (t.name.Equals("Entry", System.StringComparison.OrdinalIgnoreCase))
                    entry = t;

            if (entry == null)
            {
                Destroy(spawned);
                return null;
            }

            spawned.transform.rotation = targetExit.rotation * Quaternion.Inverse(entry.localRotation);
            spawned.transform.position += (targetExit.position - entry.position);
            Physics.SyncTransforms();

            return spawned;
        }
    }
}
