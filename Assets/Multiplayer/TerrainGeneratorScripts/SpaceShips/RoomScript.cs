using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace TerrainGeneratorScripts.SpaceShips
{
    public class RoomScript : MonoBehaviour
    {
        private static bool _reloadTriggered = false;
        [SerializeField] private static string _sceneName = "RoomGeneration";

        [RuntimeInitializeOnLoadMethod]
        private static void ResetOnSceneLoad()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                _reloadTriggered = false; // reset for the new scene
            };
        }

        IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();

            if (_reloadTriggered) yield break;

            Collider myCol = GetComponent<Collider>();
            if (myCol == null) yield break;

            Collider[] others = Physics.OverlapBox(
                myCol.bounds.center,
                myCol.bounds.extents,
                transform.rotation
            );

            foreach (Collider other in others)
            {
                // Ignore my own colliders (root + children)
                if (other == myCol || other.transform.IsChildOf(transform))
                    continue;

                Vector3 dir;
                float distance;

                if (Physics.ComputePenetration(
                        myCol, transform.position, transform.rotation,
                        other, other.transform.position, other.transform.rotation,
                        out dir, out distance))
                {
                    if (other.CompareTag("Indestructible") && distance > 0.01f)
                    {
                        _reloadTriggered = true;
                        Debug.LogWarning($"Reloading due to overlap: {name} with {other.name}!!");
                        ReloadManager.ReloadScene("RoomGeneration");
                        yield break;
                    }
                }
            }
        } 
    }
}