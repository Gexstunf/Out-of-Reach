using System;
using System.Collections.Generic;
using Characters.Utils.ConfigurableJoints;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.PlayerController.Scripts {
    public class ActiveRagdollControllerScript : MonoBehaviour
    {
        
        
        [Header("Debug Settings")]
        [Tooltip("Debug HAS to be on for these properties to take effect.")]
        [SerializeField] private bool debug = false;
        //[SerializeField] private bool alive = true;
        
        [Header("Physics settings")]
        [SerializeField] private int _solverIterations = 12;
        [SerializeField] private int _solverVelIterations = 12;
        [SerializeField] private float _maxAngularVelocity = 20f;

        private ConfigurableJoint _stabilizerJoint;
        
        [System.Serializable]
        public class BoneMap {
            public Quaternion initialLocalRotation = quaternion.identity;
            public Transform ghostBone;
            public ConfigurableJoint joint;
            public float angularDriveSpring;
            public bool isStabilizer;
            public Rigidbody rb;
        }

        private void Start() {
            foreach (var bone in boneMaps) {
                // beware of those who dont have joints
                bone.rb.solverIterations = _solverIterations;
                bone.rb.solverVelocityIterations = _solverVelIterations;
                bone.rb.maxAngularVelocity = _maxAngularVelocity;
                
                bone.initialLocalRotation = bone.rb.transform.localRotation;

                if (bone.isStabilizer) {
                    _stabilizerJoint = bone.joint;
                }
                
                if (!bone.joint) {
                    Debug.Log("Bone doesnt have joint: " + bone.rb.name);    
                }
                else {
                    bone.angularDriveSpring = bone.joint.angularXDrive.positionSpring;
                }
            }
        }

        public BoneMap[] boneMaps;

        void FixedUpdate()
        {

            if (debug) {
                return;
            }
            
            foreach (var bone in boneMaps)
            {
                if (bone.joint && bone.ghostBone) {
                    bone.joint.SetTargetRotationLocal(bone.ghostBone.localRotation, bone.initialLocalRotation);
                } 
            }
        }

        void SwitchAngularDrivesToAlive(bool alive) {
            foreach (var bone in boneMaps)
            {
                if (!bone.joint) continue;
                
                JointDrive drive = bone.joint.angularXDrive;

                if (alive) {
                    drive.positionSpring = bone.angularDriveSpring;
                } else {
                    if (bone.isStabilizer) {
                        AllowLimitedConfigurableJointMovement(bone.joint);
                    }
                    drive.positionSpring = 5f;
                }
                
                bone.joint.angularXDrive = drive;
            }
        }

        void AllowLimitedConfigurableJointMovement(ConfigurableJoint joint) {
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.angularXMotion = ConfigurableJointMotion.Free;
        }


        [ContextMenu("Kill active ragdoll")]
        void KillActiveRagdoll() {
            SwitchAngularDrivesToAlive(false);
            AllowLimitedConfigurableJointMovement(_stabilizerJoint);
            debug = true;
        }
    }
}
