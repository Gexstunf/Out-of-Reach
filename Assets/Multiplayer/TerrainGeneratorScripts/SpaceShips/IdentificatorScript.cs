using System;
using UnityEngine;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public class IdentificatorScript : MonoBehaviour
    {
        public static float PriorityNum = 0;
        public float uniquePriority;
        public string structureID;
        private void Awake()
        {
            if (string.IsNullOrEmpty(structureID))
                structureID = Guid.NewGuid().ToString();
            PriorityNum += 0.1f;
            uniquePriority = PriorityNum;
            Debug.Log(uniquePriority);
        }

        void Start()
        {
            ReloadManager.AllStructures.Add(structureID, this);
        }
    }
}
