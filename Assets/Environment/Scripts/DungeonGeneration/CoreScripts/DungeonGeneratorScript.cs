using System;
using System.Collections.Generic;
using Environment.Scripts.DungeonGeneration.Data;
using Environment.Scripts.DungeonGeneration.Utils;
using GlobalUtils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Environment.Scripts.DungeonGeneration.CoreScripts {
    public class DungeonGeneratorScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PrefabDatabaseScript prefabDb;
        [SerializeField] private PrefabPlacerScript prefabPlacer;
        [SerializeField] private Transform generationStartPoint;
        [SerializeField] private StructurePrefabScript generationStartStructure;
        [SerializeField] private LoggerSO _logger;
        
        [Header("Settings")]
        [SerializeField] private int minRoomCount =  10;
        [SerializeField] private AnimationCurve stopChanceCurve;
        [SerializeField] private int maxAttemptsPerSocket = 100;

        private List<StructureInstanceScript> _placed = new();
        private List<StructureSocketScript> _openSockets = new();
        private int _roomCount = 0;

        private void Awake() {
            _logger = LoggerSO.Instance;
        }

        public void Start() {
            _logger.Log("Starting Generation");
            Generate(generationStartStructure, generationStartPoint);
        }

        public void Generate(StructurePrefabScript startPrefab, Transform startPoint) {
            var startInstance = prefabPlacer.PlaceInitial(startPrefab, startPoint.position);
            _placed.Add(startInstance);
            _openSockets.AddRange(startInstance.GetExits());
            
            startInstance.UpdateBounds();
            var drawer = startInstance.instance.AddComponent<BoundsDrawerScript>();
            drawer.SetBounds(startInstance.Bounds, 0);
            
            _logger.Log($"After placing the initial prefab, we have: {_openSockets.Count} open sockets.");

            while (_openSockets.Count > 0) {
                StructureSocketScript socket = RandomPickFromList(_openSockets);
                bool success = TryExpandFrom(socket);
                if (!success) {
                    _openSockets.Remove(socket);
                    _logger.LogMinor($"FAILED placing in this socket: {socket}. \n Remaining open sockets: {_openSockets.Count}");
                }

                if (_roomCount >= minRoomCount) {
                    float t = Mathf.InverseLerp(minRoomCount, minRoomCount * 2, _roomCount);
                    float stopChance = stopChanceCurve.Evaluate(t);
                    if (!(Random.value < stopChance)) break;
                }
            }
        }

        public bool TryExpandFrom(StructureSocketScript socket) {
            for (int i = 0; i < maxAttemptsPerSocket; i++) {
                StructurePrefabScript prefab = prefabDb.GetWeightedRandom();
                var result = prefabPlacer.TryPlacePrefab(prefab, socket, _placed); // result is casted to a tuple (just in case you forget lil nigga)

                if (result.success) {
                    _logger.LogMinor($"SUCCEEDED placing the prefab: {prefab.name}.");
                    _placed.Add(result.instance);
                    _openSockets.Remove(socket);
                    _openSockets.AddRange(result.instance.GetUnconnectedExits());
                    if (prefab.structureType == StructureType.Room)
                        _roomCount++;
                    return true;
                }
            }
            return false;
        }

        private T RandomPickFromList<T>(List<T> list) {
            if (list == null || list.Count == 0) return default;
            int index = Random.Range(0, list.Count);
            return list[index];
        }
    }
}
