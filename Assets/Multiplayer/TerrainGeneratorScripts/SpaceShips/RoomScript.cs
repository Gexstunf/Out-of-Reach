using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public class RoomScript : MonoBehaviour
    {
        private static bool _reloadTriggered = false;

        [RuntimeInitializeOnLoadMethod]
        private static void ResetOnSceneLoad()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                _reloadTriggered = false;
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
                        ReloadManager.RemoveStructures();
                        ReloadManager.ReloadScene();
                        yield break;
                    }
                }
            }
        } 
    }
}