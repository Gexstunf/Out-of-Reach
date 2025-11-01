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

        private List<Transform> pendingExits = new();

        private void Awake()
        {
            primeraSalida = GameObject.Find("Exit_1").transform;
            maxRooms = Random.Range(7, 10);
            Debug.Log("Max Rooms: " + maxRooms);
            
            SpawnStructure(primeraSalida);
            
            while (pendingExits.Count > 0)
            {
                Transform exit = pendingExits[0];
                pendingExits.RemoveAt(0);
                SpawnStructure(exit);
            }
        }

        void SpawnStructure(Transform targetExit)
        {
            if (_rooms >= maxRooms) return;

            GameObject prefabToSpawn;
            int t = Random.Range(1, 16);

            if (t <= 12) prefabToSpawn = hallWay;
            else if (t <= 14) prefabToSpawn = interSectionPossible[Random.Range(0, interSectionPossible.Length)];
            else { prefabToSpawn = roomPossible[Random.Range(0, roomPossible.Length)]; _rooms++; }

            GameObject spawned = FuncionInstanciar(prefabToSpawn, targetExit);
            if (spawned == null) return;
            
            foreach (Transform child in spawned.GetComponentsInChildren<Transform>())
            {
                if (child.name.StartsWith("Exit"))
                {
                    pendingExits.Add(child);
                }
            }
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

            spawned.transform.rotation = targetExit.rotation * Quaternion.Inverse(entry.localRotation);
            spawned.transform.position += targetExit.position - entry.position;

            // ✅ Transferir la rama del padre al hijo
            InfoScript padreInfo = targetExit.GetComponentInParent<InfoScript>();
            InfoScript hijoInfo = spawned.GetComponent<InfoScript>();

            if (hijoInfo != null && padreInfo != null)
                hijoInfo.ramaID = padreInfo.ramaID;

            return spawned;
        }

    }
}
