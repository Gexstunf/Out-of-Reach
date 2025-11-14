using System.Collections.Generic;
using Environment.Scripts.DungeonGeneration.Data;
using UnityEngine;

namespace Environment.Scripts.DungeonGeneration.Utils {
    public class PlacementValidatorScript : MonoBehaviour
    {
        public bool Overlaps(StructureInstanceScript candidate, List<StructureInstanceScript> existing) {
            foreach (var inst in existing) {
                if (inst.ShrunkBounds.Intersects(candidate.Bounds)) // we compare a shrunk bound by a normal bound
                    return true;
            }
            return false;
        }
    }
}
