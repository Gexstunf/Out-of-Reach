using UnityEngine;

namespace Characters.IKSystem {
    public abstract class GroundDetectorScript : MonoBehaviour
    {
        public abstract bool TryGetGroundPos(Vector3 origin, out Vector3 hitPos, out Vector3 normal);
    }
}
 