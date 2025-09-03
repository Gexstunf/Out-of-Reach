using UnityEngine;
using Characters.IKSystem;
using UnityEngine.ProBuilder.Shapes;

namespace Characters.IKSystem.Solvers
{
    /// <summary>
    /// Result of a ground probe.
    /// </summary>
    
    public struct GroundHit
    {
        public bool Valid;
        public Vector3 Position; // world position (already includes footPlantOffsetY)
        public Vector3 Normal;   // world normal

        public static GroundHit Invalid => new GroundHit { Valid = false, Position = Vector3.zero, Normal = Vector3.up };
    }

    /// <summary>
    /// raycast solver. Operates in WORLD space.
    /// </summary>
    
    public class FootPosSolverScript : MonoBehaviour
    {
        [SerializeField] private FootIKSettingsSO settings;
        
        [Header("Debug Settings")]
        [SerializeField] private bool debug;
        [SerializeField] private Transform leftHomePoint;
        [SerializeField] private Transform rightHomePoint;
        
        public GroundHit TryGetGround(Vector3 originWorld)
        {
            var rayOrigin = originWorld + Vector3.up * settings.raycastVerticalOffset;

            if (Physics.Raycast(rayOrigin, Vector3.down, out var hit,
                    settings.groundCheckDistance + settings.raycastVerticalOffset,
                    settings.groundLayer, QueryTriggerInteraction.Ignore))
            {
                var p = hit.point + Vector3.up * settings.footPlantOffsetY;
                var n = hit.normal;
                if (settings.drawDebug)
                {
                    DrawDebug(rayOrigin, hit, p, n);
                }
                return new GroundHit { Valid = true, Position = p, Normal = n };
            }

            if (settings.drawDebug) Debug.DrawLine(rayOrigin, rayOrigin + Vector3.down * settings.groundCheckDistance, Color.red);
            return GroundHit.Invalid;
        }

        public void OnDrawGizmos() {
            if (debug) {
                Debug.DrawLine(leftHomePoint.position, leftHomePoint.position + Vector3.down * settings.groundCheckDistance, Color.green);
                Debug.DrawLine(rightHomePoint.position, rightHomePoint.position + Vector3.down * settings.groundCheckDistance, Color.green);
            }
        }
        /// <summary>
        /// A helper to build a foot rotation aligned to the surface.
        /// </summary>
        public static Quaternion RotationFromNormal(Vector3 forwardRef, Vector3 groundNormal)
        {
            var projFwd = Vector3.ProjectOnPlane(forwardRef, groundNormal);
            if (projFwd.sqrMagnitude < 1e-4f) projFwd = Vector3.forward; // fallback
            return Quaternion.LookRotation(projFwd.normalized, groundNormal);
        }
        
        private void DrawDebug(Vector3 rayOrigin, RaycastHit hit, Vector3 p, Vector3 n) {
            Debug.DrawLine(rayOrigin, hit.point, Color.green);
            //Gizmos.DrawSphere(hit.point ,0.15f);
            Debug.DrawRay(p, n * 0.15f, Color.cyan);
        }
    }
}
