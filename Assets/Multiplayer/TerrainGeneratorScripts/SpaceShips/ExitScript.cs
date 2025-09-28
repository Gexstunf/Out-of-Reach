using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Multiplayer.TerrainGeneratorScripts.SpaceShips
{
    public class ExitScript : MonoBehaviour
    {
        public static Dictionary<string, ExitScript> allExits = new();
        public string exitID;
        public bool isActive;
        public float activationChance;
        public GameObject activeObject;
        public GameObject inactiveVisual;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStatics()
        {
            allExits.Clear();
        }
        
        public void Awake()
        {
            isActive = Random.Range(0f, 1f) <= activationChance;
        }

        public void SetExitNumber()
        {
            if (isActive)
            {
                if (!allExits.ContainsKey(exitID))
                {
                    allExits.Add(exitID, this);
                }
                else
                {
                    exitID = System.Guid.NewGuid().ToString();
                    allExits.Add(exitID, this);
                }
            }
        }

        public void DeactivateExit()
        {
            isActive = false;
            // Remove from active exits dictionary when deactivated
            if (!string.IsNullOrEmpty(exitID) && allExits.ContainsKey(exitID))
            {
                allExits.Remove(exitID);
            }
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(exitID) && allExits.ContainsKey(exitID))
            {
                allExits.Remove(exitID);
            }
        }
        
        public void UpdateVisuals(bool active)
        {
            if (activeObject != null) activeObject.SetActive(active);
            if (inactiveVisual != null) inactiveVisual.SetActive(!active);
        }
    
        public static void ClearAllExits()
        {
            allExits.Clear();
        }
    }
}
