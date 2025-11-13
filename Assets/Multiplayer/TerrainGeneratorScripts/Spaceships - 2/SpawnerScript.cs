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
        
        private IEnumerator GenerateOld()
        {
            GameObject startObj = GameObject.Find("Exit_1");
            GameObject startObjParent = startObj.transform.parent.gameObject;
            if (!startObj || !startObjParent) yield break;
            
            int founderBranchID = branchDivision.CreateBranch(); // its the same as every branch, just cooler naming (it is the founder branch tho)
            SetObjBranchId(startObjParent, founderBranchID);
            SetupGenerationVariables(startObj); // maxRooms, etc...
            
            while (_currentRoots.Count > 0 && currentRoomCount < maxRooms)
            {
                List<Transform> nextRoots = new List<Transform>();

                foreach (var root in _currentRoots)
                {
                    Transform currentExit = root;
                    bool branchTerminated = false;
                    int hallwayChain = 0;

                    while (!branchTerminated && currentRoomCount < maxRooms) {
                        yield return null;
                        GameObject prefab = ChooseRandomPrefab();
                        GameObject spawned = CreateObjAndOrient(prefab, currentExit);
                        yield return new WaitForSecondsRealtime(0.01f);
                        if (!spawned) break;

                        List<Transform> foundExits = FindChildObjectsByName(spawned, "Exit");

                        HandleKilledBranch(foundExits);

                        int branchId = CreateAndAssignBranch(spawned);

                        if (spawned.CompareTag("Hallway"))
                        {
                            hallwayChain++;
                            if (hallwayChain > maxHallwayChain)
                            {
                                branchTerminated = true;
                                break;
                            }

                            currentExit = foundExits[0];
                        }
                        else
                        {
                            hallwayChain = 0;
                            nextRoots.AddRange(foundExits);
                            branchTerminated = true;
                        }
                    }
                }

                _currentRoots = nextRoots;
            }
        }
        
        private IEnumerator GenerateBranch(Transform root, List<Transform> nextRoots)
        {
            Transform currentExit = root;
            int hallwayChain = 0;
            int branchId = branchDivision.CreateBranch();
            int generatedAmount = 0;
            
            List<Transform> currentObjExits;

            while (generatedAmount < maxObjsPerBranch)
            {
                GameObject prefab = ChooseRandomPrefab();
                GameObject spawned = CreateObjAndOrient(prefab, currentExit);
                yield return new WaitForSecondsRealtime(0.01f);

                if (!spawned) {
                    Debug.Log($"Did not spawn anything! Branch ID: {branchId} had {generatedAmount} objects.");
                    yield break;
                };
                SetObjBranchId(spawned, branchId);

                generatedAmount++;
                currentObjExits = FindChildObjectsByName(spawned, EXIT_PREFIX);
                HandleKilledBranch(currentObjExits);
                
                if (generatedAmount == maxObjsPerBranch) {

                    if (!IsValidBranch(branchId)) {
                        branchDivision.KillBranch(branchId);
                        Debug.Log("Branch is NOT valid, killing Branch ID: " + branchId);
                        branchId = branchDivision.CreateBranch(branchId); // Once killed, re-make the branch
                        generatedAmount = 0;
                        continue;
                    };
                    
                    nextRoots.AddRange(currentObjExits);
                    Debug.Log("Completed branch with ID: " + branchId);
                    yield break;
                }
                

                if (spawned.CompareTag("Hallway"))
                {
                    hallwayChain++;
                    if (hallwayChain > maxHallwayChain)
                        yield break;
                }
                else
                {
                    hallwayChain = 0;
                }

                if (currentObjExits.Count < 0) {
                    Debug.Log($"Did not find any exits for this object: {spawned.name}, exiting!");
                    yield break;
                }
                
                currentExit = currentObjExits[0];
            }
        }

        private GameObject CreateObjAndOrient(GameObject prefab, Transform targetExit)
        {
            if (!prefab || !targetExit)
            {
                Debug.LogWarning($"CreateObjAndOrient() failed: prefab={prefab}, targetExit={targetExit}");
                return null;
            }

            GameObject newObj = Instantiate(prefab);
            Transform entry = GetEntryTransformFrom(newObj.GetComponentsInChildren<Transform>(true));
            
            if (!entry) {
                Destroy(newObj);
                Debug.LogWarning("Couldn't find entry");
                return null;
            }

            if (!newObj) {
                Debug.LogWarning("Something went wrong with creating the object.");
            }

            // just rotating it to face correctly
            try
            {
                newObj.transform.rotation = targetExit.rotation * Quaternion.Inverse(entry.localRotation);
                newObj.transform.position += (targetExit.position - entry.position);
                Physics.SyncTransforms();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CreateObjAndOrient] Transform error on '{prefab.name}': {ex}");
                Destroy(newObj);
                return null;
            }

            return newObj;
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

        private int CreateAndAssignBranch(GameObject spawned)
        {
            int id = branchDivision.CreateBranch();
            SetObjBranchId(spawned, id);
            return id;
        }

        private bool IsValidBranch(int id) {
            List<InfoScript> objects = branchDivision.Ramas[id];
            foreach (InfoScript obj in objects) {
                if (obj.IsOverlappingWithShip) {
                    Debug.Log("Overlapping!");
                    return false;
                }
            }
            return true;
        }
        
        public void AddToRoomCount(int amount = 1) {
            currentRoomCount += amount;
        }
    }
}
