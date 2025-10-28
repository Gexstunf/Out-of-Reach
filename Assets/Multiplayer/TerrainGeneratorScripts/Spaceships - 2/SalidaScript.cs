using UnityEngine;
using System.Collections.Generic;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class SalidaScript : MonoBehaviour
    {
        public static Dictionary<string, SalidaScript> MuchasSalidas = new();
        public SpawnerScript spawner;
        public string salidaID;
        public bool isActive = true;

        void Awake()
        {
            spawner = GetComponent<SpawnerScript>();
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
                spawner.SpawnStructure();
                isActive = false;
            }
        }
    }
}
