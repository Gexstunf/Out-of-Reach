using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class SpawnerScript : MonoBehaviour
    {
        private int _structureType;
        private int _rooms;
        public int maxRooms;
        public GameObject hallWay;
        public GameObject [] interSectionPossible;
        public GameObject [] roomPossible;
        public GameObject primeraSalida;

        private void Awake()
        {
            primeraSalida = GameObject.Find("Exit_1");
            maxRooms = Random.Range(7,10);
            Debug.Log(maxRooms);
            SpawnStructure(primeraSalida.transform);
        }
        
        public void SpawnStructure(Transform targetExit)
        {
            if (_rooms >= maxRooms)
                return;

            GameObject prefabToSpawn;

            int t = Random.Range(1, 16);
            if (t <= 12) prefabToSpawn = hallWay;
            else if (t <= 14) prefabToSpawn = interSectionPossible[Random.Range(0, interSectionPossible.Length)];
            else { prefabToSpawn = roomPossible[Random.Range(0, roomPossible.Length)]; _rooms++; }

            GameObject spawned = FuncionInstanciar(prefabToSpawn, targetExit);
            if (spawned == null) return;
            
            Transform[] exits = spawned.GetComponentsInChildren<Transform>();
            foreach (Transform exit in exits)
            {
                if (exit.name.StartsWith("Exit"))
                {
                    SpawnStructure(exit);
                }
            }
        }
        
        public GameObject FuncionInstanciar(GameObject prefab, Transform targetExit)
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
            return spawned;
        }

    }
}
