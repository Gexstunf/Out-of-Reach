using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
    public class SpawnerScript : MonoBehaviour
    {
        private const string ENTRY_NAME = "Entry";
        private const string EXIT_PREFIX = "Exit";
        
        [Header("Settings")]
        public int maxRoomCount = 10;
        public int minRoomCount = 7;
        public int maxHallwayChain = 15;
        public int maxObjsPerBranch = 7;
        
        [Header("Visualize current values")]
        public int currentRoomCount;
        public int maxRooms;
        public BranchDivisionScript branchDivision;
        public GameObject hallWay;
        public GameObject[] interSectionPossible;
        public GameObject[] roomPossible;
        private List<Transform> _currentRoots = new List<Transform>() { }; // this is a list of start points (exits basically)
        
        private Dictionary<int, Dictionary<int, List<InfoScript>>> _branchNetwork = new ();

        private void Start()
        {
            currentRoomCount = 0;
            //StartCoroutine(Generate());
            StartCoroutine(GenerateNew());
        }
        
        private IEnumerator GenerateNew()
        {
            GameObject startExit = GameObject.Find("Exit_1");
            if (!startExit || !startExit.transform.parent)
                yield break;

            int founderBranch = branchDivision.CreateBranch();
            SetObjBranchId(startExit.transform.parent.gameObject, founderBranch);

            SetupGenerationVariables(startExit);

            while (_currentRoots.Count > 0 && currentRoomCount < maxRooms)
            {
                List<Transform> nextRoots = new();

                foreach (var root in _currentRoots)
                    yield return StartCoroutine(GenerateBranch(root, nextRoots));

                _currentRoots = nextRoots;
            }
        }

        private IEnumerator GenerateBranch(Transform root, List<Transform> nextRoots) {
            Transform currentExit = root;
            int hallwayChain = 0;
            int branchId = branchDivision.CreateBranch();
            int generatedAmount = 0;
            _branchNetwork.Add(branchId, null);
            //_branchDivision.AddObjToBranch(RamaID, this);
            
            while (generatedAmount < maxObjsPerBranch)
            {
                GameObject prefab = ChooseRandomPrefab();
                (GameObject spawned, InfoScript spawnedInfoScript) = CreateObjAndOrient(prefab, currentExit);
                yield return new WaitForSecondsRealtime(0.01f);

                if (!spawned) {
                    Debug.Log($"Did not spawn anything! Branch ID: {branchId} had {generatedAmount} objects.");
                    yield break;
                };
                
                SetObjBranchId(spawned, branchId);
                branchDivision.AddObjToBranch(branchId, spawnedInfoScript);

                generatedAmount++;
                List<Transform> currentObjExits = FindChildObjectsByName(spawned, EXIT_PREFIX);
                
                if (generatedAmount == maxObjsPerBranch) {

                    if (!IsValidBranch(branchId)) {
                        Debug.Log("Branch is NOT valid, killing Branch ID: " + branchId);
                        branchDivision.KillBranch(branchId);
                        HandleKilledBranch(currentObjExits);
                        branchId = branchDivision.CreateBranch(branchId); // Once killed, re-make the branch
                        generatedAmount = 0;
                        continue;
                    };
                    
                    nextRoots.AddRange(currentObjExits);
                    Debug.Log("Completed branch with ID: " + branchId);
                    yield break;
                }

                bool continueGenerating = HandleGeneratedObject(spawned, nextRoots, ref hallwayChain, currentObjExits);
                if (!continueGenerating) yield break;

                if (currentObjExits.Count <= 0) {
                    Debug.Log($"Did not find any exits for this object: {spawned.name}, exiting!");
                    yield break;
                }
                
                currentExit = currentObjExits[0];
            }
        }

        private (GameObject, InfoScript) CreateObjAndOrient(GameObject prefab, Transform targetExit)
        {
            if (!prefab || !targetExit)
            {
                Debug.LogWarning($"CreateObjAndOrient() failed: prefab={prefab}, targetExit={targetExit}");
                return (null, null);
            }

            GameObject newObj = Instantiate(prefab);
            Transform entry = GetEntryTransformFrom(newObj.GetComponentsInChildren<Transform>(true));
            InfoScript script = newObj.GetComponentInChildren<InfoScript>();
            
            if (!entry) {
                Destroy(newObj);
                Debug.LogWarning("Couldn't find entry");
                return (null, null);
            }

            // just rotating it to face correctly

            newObj.transform.rotation = targetExit.rotation * Quaternion.Inverse(entry.localRotation);
            newObj.transform.position += (targetExit.position - entry.position);
            Physics.SyncTransforms();

            return (newObj, script);
        }

        private void SetObjBranchId(GameObject spawnedObj, int newId) {
            foreach (var info in spawnedObj.GetComponentsInChildren<InfoScript>())
                info.SetId(newId);
        }

        private Transform GetEntryTransformFrom(Transform[] allTransforms) {
            foreach (Transform t in allTransforms) {
                if (!(t.name.Equals(ENTRY_NAME, System.StringComparison.OrdinalIgnoreCase))) continue;
                return t; 
            }
            
            return null;
        }

        private void SetupGenerationVariables(GameObject startObj) {
            maxRooms = Random.Range(minRoomCount, maxRoomCount);
            _currentRoots.Add(startObj.transform); // again, this is an exit (they are a starting point for a branch) so basically, exits = roots
        }

        private GameObject ChooseRandomPrefab() { // this is just a weird "value" system. Basically it just chooses randomly, can be refactored.
            int t = Random.Range(1, 11);

            if (t <= 8) return hallWay;
            if (t <= 9) return interSectionPossible[Random.Range(0, interSectionPossible.Length)];
            return roomPossible[Random.Range(0, roomPossible.Length)];
        }
        
        private List<Transform> FindChildObjectsByName(GameObject obj, string prefix) {
            List<Transform> found = new();
            foreach (Transform child in obj.GetComponentsInChildren<Transform>())
                if (child.name.StartsWith(prefix))
                    found.Add(child);
            return found;
        }
        
        private void HandleKilledBranch(List<Transform> exits)
        {
            if (!branchDivision.DidKilledBranch || !branchDivision.currentExit)
                return;

            exits.Add(branchDivision.currentExit);
            branchDivision.ResetKilledState();
        }
        
        private bool IsValidBranch(int id) {
            List<InfoScript> objects = branchDivision.Ramas[id];
            Debug.Log($"There are {objects.Count} objects in Branch ID: {id}");
            foreach (InfoScript obj in objects) {
                if (obj.IsOverlapping()) {
                    Debug.Log("Invalid branch!");
                    return false;
                }
            }
            return true;
        }

        private bool HandleGeneratedObject(GameObject spawned, List<Transform> nextRoots, ref int hallwayChain, List<Transform> currentObjExits) {
            if (spawned.CompareTag("Hallway"))
            {
                hallwayChain++;
                if (hallwayChain > maxHallwayChain) return false;
            }
            else
            {
                hallwayChain = 0;
            }

            if (spawned.CompareTag("Intersection")) {
                var exits = new List<Transform>(currentObjExits);
                exits.RemoveAt(0);
                nextRoots.AddRange(exits);
            }
                
            if (spawned.CompareTag("Room")) currentRoomCount++;

            return true;
        }
    }
}
