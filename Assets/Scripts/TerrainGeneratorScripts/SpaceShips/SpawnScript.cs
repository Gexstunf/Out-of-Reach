using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TerrainGeneratorScripts.SpaceShips
{
    public class SpawnScript : MonoBehaviour
    {
        public GameObject doorGameObject;
        public GameObject uniqueHall;
        public ExitScript exitScript; 
        public GameObject entrance;
        public GameObject intersection;
        public GameObject [] roomsPosibbles;
        public int rooms;
        public int quantityOfRooms;
        private int _roomHallwayIntersection;
        private readonly int _maxOfRoomSpawn = 9;
        private readonly int _minOfRoomSpawn = 5;
        private float _roomChance;
        private float _intersectionChance;
        
        
        private int maxIterations = 1000;

        void Start()
        {
            ExitScript.ClearAllExits();
            DoorScript.ClearAllDoors();
        }
        
        public void SecondStart(int doorNumber)
        {
            doorGameObject = GameObject.Find("Door" + doorNumber);
            if (doorGameObject != null)
            {
                DoorScript doorScript = doorGameObject.GetComponentInChildren<DoorScript>();
                if (doorScript != null && doorScript.isActive)
                {
                    Vector3 pos = doorGameObject.transform.position;
                    Quaternion rot = doorGameObject.transform.rotation;
                    uniqueHall = Instantiate(entrance, pos, rot);
                    exitScript = uniqueHall.GetComponentInChildren<ExitScript>();
                    ToSetExitNumber();
                }
            }
        }

        public void SpawnHallwayAndRooms()
        {
            // SecondStart() already handles initial entrance spawning, so we only need to continue generation
            SpawnHallWaysUntilRooms();
        }

        public void ToSetExitNumber()
        {
            if (exitScript != null)
            {
                exitScript.SetExitNumber();
            }
        }

        public void SpawnHallWaysUntilRooms()
        {
            rooms = 0;
            int iterations = 0;
            quantityOfRooms = Random.Range(_minOfRoomSpawn, _maxOfRoomSpawn);
            
            while (rooms < quantityOfRooms && iterations < maxIterations)
            {
                iterations++;
                
                var exitsCopy = new List<ExitScript>(ExitScript.allExits.Values);
                bool spawnedThisIteration = false;
                
                foreach (var exit in exitsCopy)
                {
                    if (exit != null && exit.isActive && rooms < quantityOfRooms)
                    {
                        _roomHallwayIntersection = Random.Range(1, 13);
                        
                        Vector3 pos = exit.transform.position;
                        Quaternion rot = exit.transform.rotation;
                        
                        if (_roomHallwayIntersection >= 1 && _roomHallwayIntersection <= 9) // Hallway
                        {
                            //Debug.Log("Spawning Hallway");
                            InstantiateFunction(entrance, pos, rot);
                            exit.DeactivateExit();
                            spawnedThisIteration = true;
                        }
                        else if (_roomHallwayIntersection >= 10 && _roomHallwayIntersection <= 11) // Intersection
                        {
                            //Debug.Log("Spawning Intersection");
                            InstantiateFunction(intersection, pos, rot);
                            exit.DeactivateExit();
                            spawnedThisIteration = true;
                        }
                        else if (_roomHallwayIntersection == 12) // Room
                        {
                            //Debug.Log("Spawning Room");
                            InstantiateFunction(roomsPosibbles[Random.Range(0,roomsPosibbles.Length)], pos, rot);
                            exit.DeactivateExit();
                            rooms++;
                            spawnedThisIteration = true;
                        }
                    }
                }
                
                if (!spawnedThisIteration || ExitScript.allExits.Count == 0)
                {
                    Debug.LogWarning("No more active exits available or no spawning occurred, stopping generation");
                    break;
                }
            }
            
            if (iterations >= maxIterations)
            {
                Debug.LogError("Hit maximum iterations in SpawnHallWaysUntilRooms - prevented infinite loop");
            }
        }

        public void InstantiateFunction(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab != null)
            {
                GameObject spawnedObject = Instantiate(prefab, position, rotation);
                
                // Find all ExitScript components in the newly spawned object and register them
                ExitScript[] newExits = spawnedObject.GetComponentsInChildren<ExitScript>();
                foreach (ExitScript newExit in newExits)
                {
                    if (newExit != null)
                    {
                        newExit.SetExitNumber();
                        //Debug.Log($"Registered new exit: {newExit.name}");
                    }
                }
            }
        }
    }
}
