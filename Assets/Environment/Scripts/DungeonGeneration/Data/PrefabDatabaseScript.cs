using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Environment.Scripts.DungeonGeneration.Data {
    
    [CreateAssetMenu(menuName = "Dungeon/PrefabDatabase")]
    public class PrefabDatabaseScript : ScriptableObject {
        
        [SerializeField] private List<StructurePrefabScript> hallways;
        [SerializeField] private List<StructurePrefabScript> rooms;
        [SerializeField] private List<StructurePrefabScript> intersections;

        public StructurePrefabScript GetWeightedRandom() {
            List<StructurePrefabScript> all = new();
            all.AddRange(hallways);
            all.AddRange(rooms);
            all.AddRange(intersections);
            
            int total = all.Sum(p => p.Weight);
            int pick = Random.Range(0, total);
            int current = 0;

            foreach (var prefab in all) {
                current += prefab.Weight;
                if (pick < current) {
                    return prefab;
                }
            }
            
            return all[0]; // fallback
        }
    }
}
