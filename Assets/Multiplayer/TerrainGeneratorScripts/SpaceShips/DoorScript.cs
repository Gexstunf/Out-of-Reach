using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public class DoorScript : MonoBehaviour
    {
        public static Dictionary<string, DoorScript> allDoors = new();
        public string doorID;
        public bool isActive;
        public float activationChance; 
        public SpawnScript spawn;

        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStatics()
        {
            allDoors.Clear();
        }
        void Awake()
        {
            spawn = FindAnyObjectByType<SpawnScript>();

            if (string.IsNullOrEmpty(doorID))
                doorID = System.Guid.NewGuid().ToString();

            if (!allDoors.ContainsKey(doorID))
            {
                allDoors.Add(doorID, this);
            }

            RandomizeActivation();
            spawn.SecondStart();
            spawn.SpawnHallWaysUntilRooms();
        }

        void OnDestroy()
        {
            if (allDoors.ContainsKey(doorID))
                allDoors.Remove(doorID);
        }

        public void RandomizeActivation()
        {
            isActive = Random.Range(0f, 1f) <= activationChance;
        }
        
        public static void ClearAllDoors()
        {
            allDoors.Clear();
        }
    }
}
