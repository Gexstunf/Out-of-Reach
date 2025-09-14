using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerrainGeneratorScripts.SpaceShips
{
    public class ExitScript : MonoBehaviour
    {
        public static Dictionary<string, ExitScript> allExits = new();
        public string exitID;
        public bool isActive;
        public float activationChance;
        

        public void Awake()
        {
            
            // Original activation logic
            isActive = Random.Range(0f, 1f) <= activationChance;
            //Debug.Log($"Exit {name} isActive: {isActive}");
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
                    // Generate new ID if collision occurs
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
            //Debug.Log($"Exit {name} deactivated and removed from active exits");
        }

        private void OnDestroy()
        {
            if (!string.IsNullOrEmpty(exitID) && allExits.ContainsKey(exitID))
            {
                allExits.Remove(exitID);
            }
        }
    
        public static void ClearAllExits()
        {
            allExits.Clear();
        }
    }
}
