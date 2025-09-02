using UnityEngine;
using Characters.IKSystem.Solvers;

namespace Characters.IKSystem.Planners
{
    /// <summary>
    /// Alternates steps between left and right foot.
    /// Maintains per-foot state and outputs the desired world positions/rotations for IK targets.
    /// </summary>
    public class GaitPlannerScript
    {
        private enum StepState { Planted, Moving }
        public enum Leg { Left, Right }

        private class FootState
        {
            public StepState State = StepState.Planted;
            public Vector3 PlantedPos;     // all world
            public Quaternion PlantedRot;  
            public Vector3 MoveStartPos;   
            public Quaternion MoveStartRot;
            public Vector3 MoveEndPos;     
            public Quaternion MoveEndRot;  
            public float t;                // 0..1 progress
            public float CooldownTimer;    // seconds left until allowed to step again
        }

        private readonly FootState _leftFoot = new FootState();
        private readonly FootState _rightFoot = new FootState();

        private Leg _nextLeg = Leg.Right; // start with right by default (pick whatever you prefer)

        // Public outputs (read after UpdateGait)
        public Vector3 LeftFootTargetPos  { get; private set; }
        public Vector3 RightFootTargetPos { get; private set; }
        public Quaternion LeftFootTargetRot  { get; private set; } = Quaternion.identity;
        public Quaternion RightFootTargetRot { get; private set; } = Quaternion.identity;
        
        public Vector3 CurrentFootTargetPos { get; private set; }
        public Vector3 CurrentFootTargetRot { get; private set; }
        public Vector3 PreviousFootTargetPos { get; private set; }
        public Vector3 PreviousFootTargetRot { get; private set; }
        public Leg CurrentLeg { get; private set; }
        float t = 0f;


        public GaitPlannerScript(Vector3 initialLeftPos, Vector3 initialRightPos, Quaternion initialLeftRot, Quaternion initialRightRot)
        {
            _leftFoot.PlantedPos  = initialLeftPos;  _leftFoot.PlantedRot  = initialLeftRot;
            _rightFoot.PlantedPos = initialRightPos; _rightFoot.PlantedRot = initialRightRot;
            LeftFootTargetPos = initialLeftPos;   LeftFootTargetRot = initialLeftRot;
            RightFootTargetPos = initialRightPos; RightFootTargetRot = initialRightRot;
        }

        /// <summary>
        /// Call once per frame from your driver.
        /// </summary>
        public void UpdateGait(
            float deltaTime,
            Transform rootTransform,
            GroundHit leftHit,
            GroundHit rightHit,
            FootIKSettingsSO settings) {
            
            GroundHit currentHit = CurrentLeg == Leg.Right ? rightHit : leftHit;
            UpdateCurrentFoot(currentHit, settings, t);
            t += deltaTime;

            if (t >= 1f) {
                SwitchLegs(CurrentLeg);
                t = 0f;
            }
            
            // UpdateFoot(_leftFoot,  deltaTime, rootTransform, settings, out var lPos, out var lRot);
            // UpdateFoot(_rightFoot, deltaTime, rootTransform, settings, out var rPos, out var rRot);
            //
            // LeftFootTargetPos = lPos;
            // LeftFootTargetRot = lRot;
            // RightFootTargetPos = rPos;
            // RightFootTargetRot = rRot;
        }

        private void BeginStep(FootState foot, Vector3 endPosWorld, Quaternion endRotWorld)
        {
            foot.State = StepState.Moving;
            foot.t = 0f;
            foot.MoveStartPos = foot.PlantedPos;
            foot.MoveStartRot = foot.PlantedRot;
            foot.MoveEndPos = endPosWorld;
            foot.MoveEndRot = endRotWorld;
        }

        private void FinishStep(FootState foot, FootIKSettingsSO settings,
            out Vector3 outPos, out Quaternion outRot)
        {
            foot.State = StepState.Planted;
            foot.PlantedPos = foot.MoveEndPos;
            foot.PlantedRot = foot.MoveEndRot;
            foot.CooldownTimer = settings.stepCooldown;

            outPos = foot.PlantedPos;
            outRot = foot.PlantedRot;

            _nextLeg = (_nextLeg == Leg.Left) ? Leg.Right : Leg.Left;
        }

        private void UpdateFoot(
            FootState foot,
            float dTime,
            Transform root,
            FootIKSettingsSO settings,
            out Vector3 outPos,
            out Quaternion outRot)
        {
            if (foot.State == StepState.Moving)
            {
                float distanceBetweenPos = Vector3.Distance(foot.MoveStartPos, foot.MoveEndPos);
                float totalDist = Mathf.Max(0.05f, distanceBetweenPos);
                float stepTime = totalDist / Mathf.Max(0.01f, settings.stepSpeed);
                foot.t += dTime / stepTime;
                float t = Mathf.Clamp01(foot.t);

                Vector3 pos = Vector3.Lerp(foot.MoveStartPos, foot.MoveEndPos, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * settings.stepHeight;

                float rotTime = Mathf.Clamp01(settings.footRotLerpSpeed * t * dTime + t);
                Quaternion rot = Quaternion.Slerp(foot.MoveStartRot, foot.MoveEndRot, rotTime);

                outPos = pos;
                outRot = rot;

                if (t >= 1f)
                {
                    FinishStep(foot, settings, out outPos, out outRot);
                }
            }
            else
            {
                outPos = foot.PlantedPos;
                outRot = foot.PlantedRot;
            }
        }

        private void UpdateCurrentFoot(GroundHit hit, FootIKSettingsSO settings, float time) {
            Vector3 targetPoint = hit.Position;
            AddStepGap(ref targetPoint);
            Vector3 pos = Vector3.Lerp(hit.Position, targetPoint, time);
            pos.y += Mathf.Sin(time * Mathf.PI) * settings.stepHeight;
            CurrentFootTargetPos = pos;
        }

        private void UpdatePreviousFoot() {
            
        }
        
        private void AddStepGap(ref Vector3 point) {
            point += Vector3.forward;
        }

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f; b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private static Quaternion AimRot(Transform root, GroundHit hit, FootIKSettingsSO settings)
        {
            if (!settings.alignToSurface) return Quaternion.LookRotation(root.forward, Vector3.up);
            return FootPosSolverScript.RotationFromNormal(root.forward, hit.Normal);
        }

        private void SwitchLegs(Leg currentLeg) {
            CurrentLeg = currentLeg == Leg.Left ? Leg.Right : Leg.Left;
        }
    }
}
