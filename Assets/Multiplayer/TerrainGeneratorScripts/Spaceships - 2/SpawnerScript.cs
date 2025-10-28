using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class SpawnerScript : MonoBehaviour
    {
        private int _structureType;
        public GameObject hallWay;
        public GameObject [] interSectionPossible;
        public GameObject [] roomPossible;
        public void SpawnStructure()
        {
            _structureType = Random.Range(1, 16);

            if (_structureType is >= 1 and <= 12)
            {
                FuncionInstanciar(hallWay, gameObject.transform);
            }
            else if (_structureType is >= 13 and <= 14)
            {
                FuncionInstanciar(interSectionPossible[Random.Range(0,interSectionPossible.Length)], gameObject.transform);
            }
            else if (_structureType is >= 14 and <= 15)
            {
                FuncionInstanciar(roomPossible[Random.Range(0,roomPossible.Length)], gameObject.transform);
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
                    Debug.LogError(prefab.name + " no tiene un Entry definido.");
                    Destroy(spawnedObject);
                    return;
                }

                Quaternion rotationOffset = targetExit.rotation * Quaternion.Inverse(entry.rotation);
                spawnedObject.transform.rotation = rotationOffset * spawnedObject.transform.rotation;

                Vector3 positionOffset = targetExit.position - entry.position;
                spawnedObject.transform.position += positionOffset;
            }
        }
    }
}
