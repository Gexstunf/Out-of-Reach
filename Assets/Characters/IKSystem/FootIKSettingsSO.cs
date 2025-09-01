using UnityEngine;

namespace Characters.IKSystem {
    [CreateAssetMenu(fileName = "FootIKSettingsSO", menuName = "IK/FootSettings")]
    public class FootIKSettingsSO : ScriptableObject
    {
        [Header("Step Settings")]
        public float maxStepDistance = 0.5f;
        public float stepHeight = 0.2f;
        public float stepSpeed = 5f;

        [Header("Detection")]
        public LayerMask groundLayer;
        public float groundCheckDistance = 2f;
    }
}
