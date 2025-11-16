using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Environment.Scripts.DungeonGeneration.Data {
    
    [CreateAssetMenu(menuName = "Dungeon/PrefabDatabase")]
    public class PrefabDatabaseScript : ScriptableObject {
        
        [SerializeField] private List<StructurePrefabScript> hallways;
        [SerializeField] private List<StructurePrefabScript> rooms;
        [SerializeField] private List<StructurePrefabScript> intersections;

        #region Public API

        public StructurePrefabScript GetWeightedRandom(bool includeHallways = true, bool includeRooms = true, bool includeIntersections = true) {
            List<StructurePrefabScript> all = new();
            if (includeHallways) all.AddRange(hallways);
            if (includeRooms) all.AddRange(rooms);
            if (includeIntersections) all.AddRange(intersections);
            
            var prefab = ChoosePrefabByWeight(all);
            return prefab;
        }
        
        #endregion

        private StructurePrefabScript ChoosePrefabByWeight(List<StructurePrefabScript> all) {
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
