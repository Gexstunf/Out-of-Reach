using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public static class ReloadManager
    {
        
        private static bool _reloading = false;
        
        [RuntimeInitializeOnLoadMethod]
        private static void ResetOnSceneLoad()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                _reloading = false;
            };
        }

        public static void AddStructure()
        {
            
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