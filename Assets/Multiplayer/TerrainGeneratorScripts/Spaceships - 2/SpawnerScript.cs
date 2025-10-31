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
        
        public void SpawnStructure(Transform target)
        { 
            if (_rooms < maxRooms)
            {
                _structureType = Random.Range(1, 16);

                if (_structureType is >= 1 and <= 11)
                {
                    FuncionInstanciar(hallWay, target);
                }
                else if (_structureType is >= 12 and <= 13)
                {
                    FuncionInstanciar(interSectionPossible[Random.Range(0,interSectionPossible.Length)], target);
                }
                else if (_structureType is >= 14 and <= 15)
                {
                    FuncionInstanciar(roomPossible[Random.Range(0,roomPossible.Length)], target);
                    _rooms++;
                }
            }
            else
            {
                Debug.Log("Maximum rooms reached");
            }
        }

        public void FuncionInstanciar(GameObject prefab, Transform targetExit)
        {
            if (prefab != null && targetExit != null)
            {
                GameObject spawnedObject = Instantiate(prefab);

                Transform entry = spawnedObject.transform.Find("Entry");
                if (entry == null)
                {
                    Debug.LogWarning(prefab.name + " no tiene un Entry definido.");
                    Destroy(spawnedObject);
                    return;
                }

                // Alinear la rotación
                spawnedObject.transform.rotation = targetExit.rotation * Quaternion.Inverse(entry.localRotation);

                // Alinear la posición
                Vector3 offset = spawnedObject.transform.position - entry.position;
                spawnedObject.transform.position = targetExit.position + offset;
            }
        }
    }
}
