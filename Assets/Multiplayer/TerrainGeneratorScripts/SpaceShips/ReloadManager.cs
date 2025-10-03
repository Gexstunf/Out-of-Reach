using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public static class ReloadManager
    {
        public static Dictionary<string, IdentificatorScript> AllStructures = new();
        public static SpawnScript Spawn;

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
        }
        
        public static void ReloadScene()
        {
            Debug.Log("Reloading scene");
            Spawn = Object.FindAnyObjectByType<SpawnScript>();
            Spawn.SecondStart();
        }
    }
}