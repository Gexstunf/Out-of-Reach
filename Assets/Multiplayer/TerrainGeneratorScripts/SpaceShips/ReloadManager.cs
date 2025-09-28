using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public static class ReloadManager
    {
        public static Dictionary<string, GameObject> AllStructures = new();
        private static bool _reloading = false;
        
        [RuntimeInitializeOnLoadMethod]
        private static void ResetOnSceneLoad()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                _reloading = false;
            };
        }

        public static void AddStructure(GameObject structure)
        {
            Debug.Log(structure.name + " has been added");
            AllStructures.Add(structure.name, structure);
        }

        public static void RemoveStructures()
        {
            foreach (var structure in AllStructures.Skip(1))
            {
                Debug.Log(structure.Key + " has been removed");
                Object.Destroy(structure.Value);
                AllStructures.Remove(structure.Key);
            }
        }
        
        public static void ReloadScene(string sceneName)
        {
            if (_reloading) return;
            _reloading = true;
            Debug.Log(sceneName);
            SceneManager.LoadScene(sceneName);
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void ResetReloadFlag()
        {
            _reloading = false;
        }
    }
}