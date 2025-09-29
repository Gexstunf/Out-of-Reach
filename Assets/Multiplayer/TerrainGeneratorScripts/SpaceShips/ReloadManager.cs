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
        public static Dictionary<string, IdentificatorScript> AllStructures = new();
        private static bool _reloading = false;
        public static IdentificatorScript IdScript;
        
        [RuntimeInitializeOnLoadMethod]
        private static void ResetOnSceneLoad()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                _reloading = false;
            };
        }

        public static void RemoveStructures()
        {
            foreach (var structure in AllStructures.Values.Skip(1))
            {
                if (structure != null)
                {
                    Debug.Log("AAAAAA");
                    Object.Destroy(structure.gameObject);
                }
            }
            AllStructures.Clear();
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