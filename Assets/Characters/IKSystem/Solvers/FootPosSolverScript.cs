using UnityEngine;

namespace Characters.IKSystem.Solvers {
    public class FootPosSolverScript : GroundDetectorScript
    {
        [SerializeField] private FootIKSettingsSO settings;

        public override bool TryGetGroundPos(Vector3 origin, out Vector3 hitPos, out Vector3 normal) {
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, settings.groundCheckDistance, settings.groundLayer)) {
                hitPos = hit.point;
                normal = hit.normal;
                return true;
            }
            hitPos = origin;
            normal = Vector3.up;
            return false;
        }
    }
}
