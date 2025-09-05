using UnityEngine;

namespace Characters.IKSystem {
    [CreateAssetMenu(menuName = "IK/Foot IK Settings", fileName = "FootIKSettings")]
    public class FootIKSettingsSO : ScriptableObject
    {
        [Header("Detection")]
        public LayerMask groundLayer;
        [Tooltip("Extra height above foot to start the raycast.")]
        public float raycastVerticalOffset = 0.3f;
        [Tooltip("How far down search for ground from the ray origin.")]
        public float groundCheckDistance = 2.0f;
        [Tooltip("Extra lift so the foot doesnt clip the ground after hit.")]
        public float footPlantOffsetY = 0.02f;

        [Header("Stepping")]
        [Tooltip("Should use planar distance?")]
        public bool usePlanarDistance = true;
        [Tooltip("Min planar distance from current planted pos to new ground needed to trigger a step.")]
        public float maxStepDistance = 0.5f;
        [Tooltip("Vertical arc height for the swing phase.")]
        public float stepHeight = 0.2f;
        [Tooltip("How fast the foot moves toward the new target (units/sec).")]
        public float stepSpeed = 5.0f;
        [Tooltip("Minimum time between steps of the SAME foot.")]
        public float stepCooldown = 0.12f;
        [Tooltip("Base step length")] 
        public float stepLength = 0.3f;
        [Tooltip("Duration of step")]
        public float totalStepDuration = 0.3f;
        public float downTimeStep = 0.15f;
        
        [Header("Threshold")]
        public float stepThreshold = 0.1f;

        [Header("Orientation")]
        [Tooltip("Align foot up to ground normal.")]
        public bool alignToSurface = true;
        [Tooltip("How fast to slerp toward the desired rotation.")]
        public float footRotLerpSpeed = 12.0f;

        [Header("Debug")]
        public bool drawDebug = false;
    }
}
