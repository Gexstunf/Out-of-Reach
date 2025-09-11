using System;
using System.Collections.Generic;
using Characters.Utils.ConfigurableJoints;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.PlayerController.Scripts {
    public class ActiveRagdollControllerScript : MonoBehaviour
    {
        
        [Header("Physics settings")]
        [SerializeField] private int _solverIterations = 12;
        [SerializeField] private int _solverVelIterations = 12;
        [SerializeField] private float _maxAngularVelocity = 20f;
        
        [System.Serializable]
        public class BoneMap {
            public Quaternion initialLocalRotation = quaternion.identity;
            public Transform ghostBone;
            public ConfigurableJoint joint;
            public Rigidbody rb;
        }

        private void Start() {
            foreach (var bone in boneMaps) {
                bone.rb.solverIterations = _solverIterations;
                bone.rb.solverVelocityIterations = _solverVelIterations;
                bone.rb.maxAngularVelocity = _maxAngularVelocity;
                
                bone.initialLocalRotation = bone.rb.transform.localRotation;

                if (!bone.joint) {
                    Debug.Log("Bone doesnt have joint: " + bone.rb.name);    
                }
            }
        }

        public BoneMap[] boneMaps;

        void FixedUpdate()
        {
            foreach (var bone in boneMaps)
            {
                if (bone.joint && bone.ghostBone) {
                    bone.joint.SetTargetRotationLocal(bone.ghostBone.localRotation, bone.initialLocalRotation);
                }
            }
        }
    }
}
