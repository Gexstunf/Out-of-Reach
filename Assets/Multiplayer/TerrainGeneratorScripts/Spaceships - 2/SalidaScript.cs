using UnityEngine;
using System.Collections.Generic;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class SalidaScript : MonoBehaviour
    {
        public static Dictionary<string, SalidaScript> MuchasSalidas = new();
        public string salidaID;

        void Awake()
        {
            if (!MuchasSalidas.ContainsKey(salidaID) && salidaID != "")
            {
                MuchasSalidas.Add(salidaID, this);
            }
            else
            {
                salidaID = System.Guid.NewGuid().ToString();
                MuchasSalidas.Add(salidaID, this);
            }
        }
    }
}
