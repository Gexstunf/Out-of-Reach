using System;
using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public class IdentificatorScript : MonoBehaviour
    {
        public string structureID;
        private void Awake()
        {
            if (string.IsNullOrEmpty(structureID))
                structureID = Guid.NewGuid().ToString();
        }

        void Start()
        {
            ReloadManager.AllStructures.Add(structureID, this);
        }
    }
}
