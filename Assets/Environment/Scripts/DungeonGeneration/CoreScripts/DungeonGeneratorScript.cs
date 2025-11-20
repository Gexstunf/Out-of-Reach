using System;
using System.Collections.Generic;
using Environment.Scripts.DungeonGeneration.Data;
using Environment.Scripts.DungeonGeneration.Utils;
using GlobalUtils;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Environment.Scripts.DungeonGeneration.CoreScripts {
    public class DungeonGeneratorScript : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        [SerializeField] private PrefabDatabaseScript prefabDb;
        [SerializeField] private PrefabPlacerScript prefabPlacer;
        [SerializeField] private Transform generationStartPoint;
        [SerializeField] private Transform motherPlacePoint;
        [SerializeField] private LoggerSO _logger;

        [Header("Relevant Structures")]
        [SerializeField] private StructurePrefabScript generationStartStructure;
        [SerializeField] private StructurePrefabScript motherStartStructure;
        [SerializeField] private StructurePrefabScript sealStructure;
        
        [Header("Settings")]
        public bool usePhoton;
        [SerializeField] private int minRoomCount =  10;
        [SerializeField] private AnimationCurve stopChanceCurve;
        [SerializeField] private int maxAttemptsPerSocket = 100;
        [SerializeField] private int maxAmountOfStructures = 200;
        [SerializeField] private float maxGenerationTime = 15f;
        [SerializeField] private string parentName = "STRUCTURES";

        [Header("Visualize")]
        [SerializeField] private int currentPlacedCount;
        [SerializeField] private int currentStructureSum;
        [SerializeField] private int currentRoomAmount;
        [SerializeField] private int currentIntersectionAmount;
        [SerializeField] private int currentHallwayAmount;
        
        public static DungeonGeneratorScript Instance;

        private List<StructureInstanceScript> _placed = new();
        private List<StructureSocketScript> _openSockets = new();
        private List<StructureSocketScript> _failedSockets = new();
        
        private bool _previouslyPlacedRoom = false;
        private int _roomCount = 0;
        private int _hallwayCount = 0;
        private int _intersectionCount = 0;
        private float _currentGenerationTime = 0f;
        #endregion

        #region Public API

        public bool FinishedGeneration { get; private set; } = false;

        #endregion
        
        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }
            
            Instance = this;
        }


        private void Start() {
            _logger = LoggerSO.Instance;
            _logger.Log("Starting Generation");
            
            if (usePhoton && !PhotonNetwork.IsMasterClient) return;
            Generate(generationStartStructure, generationStartPoint);
            SealExits();
            currentPlacedCount = _placed.Count;
            currentStructureSum = _hallwayCount + _intersectionCount + _roomCount; 
            FinishedGeneration = true;
            PlaceUnderParent(_placed);
            if (usePhoton) SyncStructuresToClients();
        }

        private void Generate(StructurePrefabScript startPrefab, Transform startPoint) {
            var motherInstance = prefabPlacer.PlaceInitial(motherStartStructure, motherPlacePoint.position);
            //var startInstance = prefabPlacer.PlaceInitial(startPrefab, startPoint.position);    
            _placed.Add(motherInstance);
            _openSockets.AddRange(motherInstance.GetExits());
            SetupShip(_openSockets, startPrefab);
            //_placed.Add(startInstance);
            //_openSockets.AddRange(startInstance.GetExits());

            //_logger.Log($"After placing the initial prefab, we have: {_openSockets.Count} open sockets.");
            float generationStartTime = Time.realtimeSinceStartup; // this is real time, Time.DeltaTime doesnt work because this is a single frame.
            
            while (_openSockets.Count > 0) {
                StructureSocketScript socket = RandomPickFromList(_openSockets);
                bool success = TryExpandFrom(socket);
                if (!success) {
                    _openSockets.Remove(socket); // remove, but add keep track of it so we can restore _openSocks and seal them later.
                    _failedSockets.Add(socket);
                    _logger.LogMinor($"FAILED placing in this socket: {socket}. \n Remaining open sockets: {_openSockets.Count}");
                }
            
                _currentGenerationTime = Time.realtimeSinceStartup - generationStartTime;
                if (ShouldTerminateGeneration()) break;
            }
        }

        private bool TryExpandFrom(StructureSocketScript socket, StructurePrefabScript p = null, bool ignoreOverlap = false) {
            for (int i = 0; i < maxAttemptsPerSocket; i++) {
                StructurePrefabScript prefab = p != null? p : prefabDb.GetWeightedRandom();

                if (_previouslyPlacedRoom && p == null) prefab = prefabDb.GetWeightedRandom(includeRooms: false);
                var result = prefabPlacer.TryPlacePrefab(prefab, socket, _placed, ignoreOverlap); // result is casted to a tuple (just in case you forget lil nigga)

                if (result.success) {
                    //_logger.LogMinor($"SUCCEEDED placing the prefab: {prefab.name}.");
                    _placed.Add(result.instance);
                    _openSockets.Remove(socket);
                    _openSockets.AddRange(result.instance.GetUnconnectedExits());
                    HandleStructure(prefab); // this just adds to some variables and checks some conditions e.g _prevPlacedRoom
                    
                    return true;
                }
            }
            return false;
        }

        private bool ShouldTerminateGeneration() {
            if (_roomCount >= minRoomCount) {
                float t = Mathf.InverseLerp(minRoomCount, minRoomCount * 2, _roomCount);
                float stopChance = stopChanceCurve.Evaluate(t);
                float random = Random.value;
                if (random < stopChance) {
                    Debug.Log("= = = = TERMINATING GENERATION due to STOP CHANCE = = = =");
                    Debug.Log($"The termination chance curve is currently at: {stopChance}.  T: {t} / Random: {random} ");
                    return true;
                }
            }

            if (_placed.Count >= maxAmountOfStructures) {
                Debug.Log("= = = = TERMINATING GENERATION due to HARD LIMIT = = = =");
                return true;
            }
            
            if (_currentGenerationTime > maxGenerationTime) {
                Debug.Log("= = = = TERMINATING GENERATION due to TIME LIMIT = = = =");
                Debug.Log($"The generation time is currently at: {_currentGenerationTime}.");
                return true;
            }

            return false;
        }
        
        private void SetupShip(List<StructureSocketScript> sockets, StructurePrefabScript setupPrefab) {
            var socketsCopy = new List<StructureSocketScript>(sockets);
            foreach (var socket in socketsCopy) {
                bool success = TryExpandFrom(socket, setupPrefab, ignoreOverlap: true);
                if (!success) {
                    _openSockets.Remove(socket); // remove, but add keep track of it so we can restore _openSocks and seal them later.
                    _failedSockets.Add(socket);
                    _logger.LogMinor($"FAILED SETUP placing in this socket: {socket}. \n Remaining open sockets: {_openSockets.Count}");
                }
            }
        }
            
        private void SealExits() {
            _openSockets.AddRange(_failedSockets); // restore the failed sockets to the open ones for sealing
            var socketsCopy = new List<StructureSocketScript>(_openSockets);

            foreach (var socket in socketsCopy) {
                bool success = TryExpandFrom(socket, sealStructure, true);
                if (!success) {
                    _openSockets.Remove(socket);
                    _logger.LogMinor($"FAILED placing SEAL in socket: {socket}");
                }
            }
        }
  
        private void HandleStructure(StructurePrefabScript prefab) {
            switch (prefab.structureType) {
                case StructureType.Room:
                    _roomCount++;
                    currentRoomAmount = _roomCount;
                    _previouslyPlacedRoom = true;
                    break;
                case StructureType.Hallway:
                    _hallwayCount++;
                    currentHallwayAmount = _hallwayCount;
                    _previouslyPlacedRoom = false;
                    break;
                case StructureType.Intersection:
                    _intersectionCount++;
                    currentIntersectionAmount = _intersectionCount;
                    _previouslyPlacedRoom = false;
                    break;
            }
        }
        
        public void SyncStructuresToClients() {
            foreach (StructureInstanceScript structure in _placed) {
                PhotonNetwork.Instantiate(
                    structure.definition.prefab.name,
                    structure.instance.transform.position,
                    structure.instance.transform.rotation
                );
            }
        }


        private void PlaceUnderParent(List<StructureInstanceScript> list) {
            var parent = new GameObject(parentName);
            foreach (var struInst in list) {
                struInst.instance.transform.SetParent(parent.transform);
            }
        }

        private T RandomPickFromList<T>(List<T> list) {
            if (list == null || list.Count == 0) return default;
            int index = Random.Range(0, list.Count);
            return list[index];
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            foreach (var socket in _openSockets) {
                Gizmos.DrawSphere(socket.transform.position, 1);
            }
        }
    }
}
