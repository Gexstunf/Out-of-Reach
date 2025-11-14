using System.Collections.Generic;
using Environment.Scripts.DungeonGeneration.Data;
using UnityEngine;

namespace Environment.Scripts.DungeonGeneration.Utils {
    public class PlacementValidatorScript : MonoBehaviour
    {
        public bool Overlaps(StructureInstanceScript candidate, List<StructureInstanceScript> existing) {
            foreach (var inst in existing) {
                if (inst.ShrunkBounds.Intersects(candidate.ShrunkBounds)) // we only compare the shrunk bounds, for no false overlaps
                    return true;
            }
            return false;
        }
    }
}
