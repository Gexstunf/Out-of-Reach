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
            Debug.Log("Added "+ structureID);
            ReloadManager.AllStructures.Add(structureID, this);
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
            ReloadManager.AllStructures.Remove(structureID);
        }
    }
}
