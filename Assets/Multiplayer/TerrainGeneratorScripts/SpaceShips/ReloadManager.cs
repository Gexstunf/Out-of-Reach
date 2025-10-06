using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public static class ReloadManager
    {
        public static Dictionary<string, IdentificatorScript> AllStructures = new();
        public static DoorScript Door;

        public static void RemoveStructures()
        {
            foreach (var structure in AllStructures.Values)
            {
                if (structure != null)
                {
                    Object.Destroy(structure.gameObject);
                }
            }
            AllStructures.Clear();
            ExitScript.ClearAllExits();
            DoorScript.ClearAllDoors();
        }
        
        public static void ReloadScene()
        {
            Debug.Log("Reloading scene");
            Door = Object.FindAnyObjectByType<DoorScript>();
            Door.Awake();
        }
    }
}