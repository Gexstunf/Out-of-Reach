using UnityEngine;

namespace Characters.ActiveRagdollSystem {
    public class RevivalParams {
        public float Damper = 0f;
        public float Duration = 1f;

        public float StartClearance = 0.5f;
        public float EndClearance = 0f;
        
        public bool UseClearance = true;
        
        public float StartSpring = 0f;
        public float EndSpring = 33f;

        public ConfigurableJointMotion YMotionStart = ConfigurableJointMotion.Limited;
        public ConfigurableJointMotion YMotionEnd = ConfigurableJointMotion.Locked;

        public ConfigurableJointMotion AngularXEnd = ConfigurableJointMotion.Limited;
        public ConfigurableJointMotion AngularYEnd = ConfigurableJointMotion.Locked;
        public ConfigurableJointMotion AngularZEnd = ConfigurableJointMotion.Limited;

        public AnimationCurve ClearanceCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve SpringCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        public ActiveRagdollCoreScript.StabilizerMode EndMode = ActiveRagdollCoreScript.StabilizerMode.Normal;
    }

    public struct StandParams {
        public float Duration;
    }

    public struct DeathParams {
        public bool AllowLimitedMovement;
    }
    
    public struct CrouchParams {
        public float Height;
    }
}