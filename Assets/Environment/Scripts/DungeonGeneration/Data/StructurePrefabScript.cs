using UnityEngine;

namespace Environment.Scripts.DungeonGeneration.Data {
    [CreateAssetMenu(menuName = "Dungeon/StructurePrefab")]
    public class StructurePrefabScript : ScriptableObject {
        public GameObject prefab;
        public StructureType structureType;
        [Range(1, 100)] public int weight = 1;
        public int Weight => weight;
    }

    public enum StructureType {
        Room,
        Hallway,
        Intersection,
    }
}