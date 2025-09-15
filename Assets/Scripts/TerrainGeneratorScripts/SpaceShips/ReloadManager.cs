using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerrainGeneratorScripts.SpaceShips
{
    public static class ReloadManager
    {
        private static bool _reloading = false;
        
        [RuntimeInitializeOnLoadMethod]
        private static void ResetOnSceneLoad()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                _reloading = false; // reset for the new scene
            };
        }

        // Call this instead of SceneManager.LoadScene
        public static void ReloadScene(string sceneName)
        {
            if (_reloading) return; // prevent multiple reloads
            _reloading = true;
            SceneManager.LoadScene(sceneName);
        }

        // Optional: reset reload flag when new scene is fully loaded
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ResetReloadFlag()
        {
            _reloading = false;
        }
    }
}