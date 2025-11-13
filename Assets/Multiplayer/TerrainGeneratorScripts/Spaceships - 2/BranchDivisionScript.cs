using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class BranchDivisionScript : MonoBehaviour
    {
        public Dictionary<int, List<InfoScript>> Ramas = new();
        public Transform currentExit;
        public SpawnerScript spawner;
        private static int _nextRamaID = -1;
        public bool DidKilledBranch { get; private set; } = false;

        #region Public API

        public int CreateBranch(int id = -1)
        {
            if (id == -1) {
                _nextRamaID++; 
                Ramas.Add(_nextRamaID, new List<InfoScript>());
                return _nextRamaID;
            }
            
            Ramas.Add(id, new List<InfoScript>());
            return id;
        }

        public void AddObjToBranch(int ramaID, InfoScript obj)
        {
            Ramas[ramaID].Add(obj);
        }

        public void KillBranch(int ramaID)
        {
            if (!Ramas.ContainsKey(ramaID)) return;
            
            List<InfoScript> branch = Ramas[ramaID];
            
            var firstObj = branch[0];
            var exit = firstObj.transform.Find("Exit_1");
            
            ResetStartPoint(exit); // set the start point back to the original one starting one, so a new branch can be formed.
            ClearObjectsInBranch(branch);
            Ramas.Remove(ramaID);
        }
        
        public void ResetKilledState() {
            DidKilledBranch = false;
        }
        
        #endregion

        private void ClearObjectsInBranch(List<InfoScript> objs) {
            foreach (var obj in objs) {
                if (obj != null)
                    Destroy(obj.gameObject);
            }
        }

        private void ResetStartPoint(Transform exit) {
            if (exit != null)
            {
                GameObject dummy = new GameObject("RecoveredExit");
                dummy.transform.position = exit.position;
                dummy.transform.rotation = exit.rotation;
                currentExit = dummy.transform;
                DidKilledBranch = true;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (Ramas == null) return;

            int colorIndex = 0;

            foreach (var rama in Ramas)
            {
                Color c = Color.HSVToRGB((colorIndex * 0.15f) % 1f, 1f, 1f);
                Gizmos.color = c;
                colorIndex++;

                var objects = rama.Value;
                for (int i = 0; i < objects.Count - 1; i++)
                {
                    if (objects[i] != null && objects[i + 1] != null)
                        Gizmos.DrawLine(objects[i].transform.position, objects[i + 1].transform.position);
                }
            }
        }
    }
}