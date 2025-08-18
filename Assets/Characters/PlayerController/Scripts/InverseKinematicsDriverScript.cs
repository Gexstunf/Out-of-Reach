using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Characters.PlayerController.Scripts {
    public class InverseKinematicsDriverScript : MonoBehaviour
    {
        [Header("Bones & Targets")]
        public Transform hips;                 
        public Transform hipsParent;      
        [Tooltip("Source of OverrideTransform on the hips")]
        public Transform hipsOffsetTarget;     // source of OverrideTransform on pelvis
        public Rig hipsRig;                    
    
        [Header("Crouch")]
        [Range(0f, 1f)] public float crouch01 = 0f; 
        public float crouchHeight = 0.45f;
        public float smooth = 12f;
    
        private Vector3 _animatedHipsLocal;
        private Vector3 _baseHipsLocal;

        private void Start() {
            _baseHipsLocal = hips.localPosition;
        }

        void Reset()
        {
            if (hips != null) hipsParent = hips.parent;
        }

        void LateUpdate()
        {
            if (!hips || !hipsParent || !hipsOffsetTarget) return;

            _animatedHipsLocal = hips.localPosition;

            float desiredDrop = crouch01 * crouchHeight;
            Vector3 targetLocal = _baseHipsLocal + new Vector3(0f, -desiredDrop, 0f);

            MoveHipsLocally(targetLocal);
            
            float targetW = (crouch01 > 0f) ? 1f : 0f;
            hipsRig.weight = Mathf.MoveTowards(hipsRig.weight, targetW, Time.deltaTime * 8f);
        }

        private void MoveHipsLocally(Vector3 targetLocal) {
            Vector3 currentLocal = hipsParent.InverseTransformPoint(hipsOffsetTarget.position);
            Vector3 newLocal = Vector3.Lerp(currentLocal, targetLocal, Time.deltaTime * smooth);
            hipsOffsetTarget.position = hipsParent.TransformPoint(newLocal);
        }
    }
}
