using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerrainGeneratorScripts.SpaceShips
{
    public class DoorScript : MonoBehaviour
    {
        public static Dictionary<string, DoorScript> allDoors = new();
        public string doorID;
        public bool isActive;
        public float activationChance;
        public GameObject activeVisual;
        public GameObject inactiveVisual;
        public SpawnScript entrance;

        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStatics()
        {
            allDoors.Clear();
        }
        void Awake()
        {
            entrance = FindAnyObjectByType<SpawnScript>();

            if (string.IsNullOrEmpty(doorID))
                doorID = System.Guid.NewGuid().ToString();

            if (!allDoors.ContainsKey(doorID))
            {
                allDoors.Add(doorID, this);
            }

            RandomizeActivation();
            UpdateVisuals();
            entrance.SecondStart();
            entrance.SpawnHallWaysUntilRooms();
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

        public void SetActivation(bool active)
        {
            isActive = active;
            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            if (activeVisual != null)
                activeVisual.SetActive(isActive);

            if (inactiveVisual != null)
                inactiveVisual.SetActive(!isActive);
        }
        
        public static void ClearAllDoors()
        {
            allDoors.Clear();
        }
    }
}
