using UnityEngine;
using System.Collections.Generic;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class SalidaScript : MonoBehaviour
    {
        public static Dictionary<string, SalidaScript> MuchasSalidas = new();
        [SerializeField] SpawnerScript spawner;
        public string salidaID;
        public bool isActive = true;
        private static bool _firstTime = true;

        void Awake()
        {
            spawner = FindAnyObjectByType<SpawnerScript>();
            if (_firstTime)
            {
                spawner.maxRooms = Random.Range(7,10);
                _firstTime = false;
                Debug.Log(spawner.maxRooms);
            }
            if (!MuchasSalidas.ContainsKey(salidaID) && salidaID != "")
            {
                MuchasSalidas.Add(salidaID, this);
            }
            else
            {
                salidaID = System.Guid.NewGuid().ToString();
                MuchasSalidas.Add(salidaID, this);
            }

            if (isActive)
            {
                Debug.Log(gameObject.transform.position);
                spawner.SpawnStructure(gameObject.transform);
                isActive = false;
            }
        }
    }
}
