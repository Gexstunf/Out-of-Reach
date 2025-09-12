using System.Collections.Generic;
using Unity.Mathematics;
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
        public GameObject exitOut;
        public GameObject [] intersectionsPossibles;
        public GameObject [] roomsPossibles;
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
        
        public void SecondStart()
        {
            doorGameObject = GameObject.Find("Door1");
            if (doorGameObject != null)
            {
                DoorScript doorScript = doorGameObject.GetComponentInChildren<DoorScript>();
                if (doorScript != null && doorScript.isActive)
                {
                    Vector3 pos = doorGameObject.transform.position;
                    Quaternion rot = doorGameObject.transform.rotation;
                    pos.x += 4;
                    uniqueHall = Instantiate(entrance, pos, rot);
                    exitScript = uniqueHall.GetComponentInChildren<ExitScript>();
                    ToSetExitNumber();
                }
            }
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
                        _roomHallwayIntersection = Random.Range(1, 12);

                        if (_roomHallwayIntersection is >= 1 and <= 7) // Hallway
                        {
                            InstantiateFunction(entrance, exit.transform);
                            exit.DeactivateExit();
                            spawnedThisIteration = true;
                        }
                        else if (_roomHallwayIntersection is >= 8 and <= 9) // Intersection
                        {
                            InstantiateFunction(intersectionsPossibles[Random.Range(0,intersectionsPossibles.Length)], exit.transform);
                            exit.DeactivateExit();
                            spawnedThisIteration = true;
                        }
                        else if (_roomHallwayIntersection == 10 && rooms > 2) // Exit
                        {
                            InstantiateFunction(exitOut, exit.transform);
                            exit.DeactivateExit();
                            spawnedThisIteration = true;
                        }
                        else if (_roomHallwayIntersection == 11) // Room
                        {
                            InstantiateFunction(roomsPossibles[Random.Range(0,roomsPossibles.Length)], exit.transform);
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


        public void InstantiateFunction(GameObject prefab, Transform targetExit)
        {
            if (prefab != null && targetExit != null)
            {
                // 1. Instanciamos en la escena
                GameObject spawnedObject = Instantiate(prefab);

                // 2. Buscamos el EntryPoint del prefab
                Transform entryPoint = spawnedObject.transform.Find("EntryPoint");
                if (entryPoint == null)
                {
                    Debug.LogError(prefab.name + " no tiene un EntryPoint definido.");
                    Destroy(spawnedObject);
                    return;
                }

                // 3. Alinear rotación
                Quaternion rotationOffset = targetExit.rotation * Quaternion.Inverse(entryPoint.rotation);
                spawnedObject.transform.rotation = rotationOffset * spawnedObject.transform.rotation;

                // 4. Alinear posición
                Vector3 positionOffset = targetExit.position - entryPoint.position;
                spawnedObject.transform.position += positionOffset;

                // 5. Registrar nuevas salidas
                ExitScript[] newExits = spawnedObject.GetComponentsInChildren<ExitScript>();
                foreach (ExitScript newExit in newExits)
                {
                    if (newExit != null)
                    {
                        newExit.SetExitNumber();
                    }
                }
            }
        }

    }
}
