using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace TerrainGeneratorScripts.SpaceShips
{
    public class RoomScript : MonoBehaviour
    {
        private static bool reloadTriggered = false;

        [RuntimeInitializeOnLoadMethod]
        private static void ResetOnSceneLoad()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                reloadTriggered = false; // reset for the new scene
            };
        }

        IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();

            if (reloadTriggered) yield break;

            Collider myCol = GetComponent<Collider>();
            if (myCol == null) yield break;

            Collider[] others = Physics.OverlapBox(
                myCol.bounds.center,
                myCol.bounds.extents,
                transform.rotation
            );

            foreach (Collider other in others)
            {
                // 🚫 Ignore my own colliders (root + children)
                if (other == myCol || other.transform.IsChildOf(transform))
                    continue;

                Vector3 dir;
                float distance;

                if (Physics.ComputePenetration(
                        myCol, transform.position, transform.rotation,
                        other, other.transform.position, other.transform.rotation,
                        out dir, out distance))
                {
                    if (other.CompareTag("Indestructible") && distance > 0.1f)
                    {
                        reloadTriggered = true;
                        Debug.LogWarning($"Reloading due to overlap: {name} with {other.name}");
                        SceneManager.LoadScene("Test-004 (Generation)");
                        yield break;
                    }
                }
            }
        } 
    }
}