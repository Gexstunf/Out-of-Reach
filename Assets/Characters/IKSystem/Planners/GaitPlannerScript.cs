using UnityEngine;
using Characters.IKSystem.Solvers;
using Unity.Mathematics;

namespace Characters.IKSystem.Planners
{
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

        private struct CurrentContext {
            public FootState CurrentFoot;
            public Leg CurrentLeg;
            public readonly Transform RootTransform;
            public Vector3 StartRootPos;
            public FootIKSettingsSO Settings;
            public Vector3 CurrentPos;
            public GroundHit CurrentGroundHit;

            public CurrentContext(Vector3 currentPos, Leg currentLeg, FootState currentFoot, GroundHit currentGroundHit, Transform rootTransform, Vector3 startRootPos, FootIKSettingsSO settings) {
                CurrentPos = currentPos;
                CurrentLeg = currentLeg;
                CurrentFoot = currentFoot;
                CurrentGroundHit = currentGroundHit;
                RootTransform = rootTransform;
                StartRootPos = startRootPos;
                Settings = settings;
            }
        }
        

        private readonly FootState _leftFoot = new FootState();
        private readonly FootState _rightFoot = new FootState();
        private CurrentContext _context;

        private Leg _nextLeg = Leg.Right; // start with right by default (pick whatever you prefer)

        // Public outputs (read after UpdateGait)
        public Vector3 LeftFootTargetPos  { get; private set; }
        public Vector3 RightFootTargetPos { get; private set; }
        public Quaternion LeftFootTargetRot  { get; private set; } = Quaternion.identity;
        public Quaternion RightFootTargetRot { get; private set; } = Quaternion.identity;
        public Leg CurrentLeg { get; private set; }
        float t = 0f;


        public GaitPlannerScript(Vector3 initialLeftPos, Vector3 initialRightPos, Quaternion initialLeftRot, 
            Quaternion initialRightRot, Transform rootTransform, FootIKSettingsSO settings)
        {
            _leftFoot.PlantedPos  = initialLeftPos;  _leftFoot.PlantedRot  = initialLeftRot;
            _rightFoot.PlantedPos = initialRightPos; _rightFoot.PlantedRot = initialRightRot;
            _context = new CurrentContext(initialLeftPos, Leg.Left, _leftFoot, new GroundHit(), rootTransform, rootTransform.position, settings);
            LeftFootTargetPos = initialLeftPos;   LeftFootTargetRot = initialLeftRot;
            RightFootTargetPos = initialRightPos; RightFootTargetRot = initialRightRot;
        }

        public void UpdateGait(
            float deltaTime,
            Transform rootTransform,
            GroundHit leftHit,
            GroundHit rightHit,
            bool hasToStep,
            FootIKSettingsSO settings) 
        {
            
            if (t >= settings.totalStepDuration) {
                FinishStep(_context.CurrentFoot);
                SwitchContext(_context.CurrentLeg);
                t = 0f;
            }
            t += deltaTime;

            GroundHit currentHit = _context.CurrentLeg == Leg.Right ? rightHit : leftHit;
            _context.CurrentGroundHit = currentHit;

            bool bothPlanted = (_leftFoot.State == StepState.Planted && _rightFoot.State == StepState.Planted);
            
            if (bothPlanted && ShouldStep(_context.CurrentFoot, _context.RootTransform)) {
                BeginStep(_context.CurrentFoot, currentHit.Position, quaternion.identity);
            }
            
            UpdateCurrentFoot(t);
        }

        private void BeginStep(FootState foot, Vector3 endPosWorld, Quaternion endRotWorld)
        {
            foot.State = StepState.Moving;
            foot.t = 0f;
            foot.MoveStartPos = foot.PlantedPos;
            foot.MoveStartRot = foot.PlantedRot;
            foot.MoveEndPos = endPosWorld;
            foot.MoveEndRot = endRotWorld;
            _context.StartRootPos = _context.RootTransform.position;
        }
        
        private void FinishStep(FootState foot) {
            foot.State = StepState.Planted;
            foot.t = 0f; 
            foot.PlantedPos = foot.MoveEndPos;
            foot.PlantedRot = foot.MoveEndRot;
        }

        private void UpdateCurrentFoot(float time) {
            bool bothPlanted = (_leftFoot.State == StepState.Planted && _rightFoot.State == StepState.Planted);

            if (!bothPlanted) {
                FootIKSettingsSO settings = _context.Settings;
                Vector3 targetPoint = _context.CurrentFoot.MoveEndPos;
                Vector3 startPos = _context.CurrentFoot.MoveStartPos;
                float processedTime = time / settings.totalStepDuration;
                processedTime = Mathf.Clamp01(processedTime);
                Vector3 lerpPos = Vector3.Lerp(startPos, targetPoint, processedTime);
        
                lerpPos.y += Mathf.Sin(processedTime * Mathf.PI) * settings.stepHeight;
                _context.CurrentPos = lerpPos;
        
                if (_context.CurrentLeg == Leg.Left) {
                    LeftFootTargetPos = lerpPos;
                }
                else {
                    RightFootTargetPos = lerpPos;
                }
            }
        }
        
        private bool ShouldStep(FootState foot, Transform rootTransform)
        {
            float distance = Vector3.Distance(foot.PlantedPos, rootTransform.position);
            if (_context.Settings.usePlanarDistance) {
                distance = PlanarDistance(_context.RootTransform.position, _context.StartRootPos);
            }
            return distance > _context.Settings.stepThreshold;
        }

        
        private void AddStepGap(ref Vector3 point) {
            point += Vector3.forward;
        }

        private static float PlanarDistance(Vector3 a, Vector3 b)
        {
            var pointA = new Vector2(a.x, a.z);
            var pointB = new Vector2(b.x, b.z);
            return Vector3.Distance(pointA, pointB);
        }

        private float PlanarRootDistance(Vector3 current, Vector3 start) {
            Vector3 rootDelta = current - start;
            float planarDistance = new Vector2(rootDelta.x, rootDelta.z).magnitude;
            return planarDistance;
        }

        private static Quaternion AimRot(Transform root, GroundHit hit, FootIKSettingsSO settings)
        {
            if (!settings.alignToSurface) return Quaternion.LookRotation(root.forward, Vector3.up);
            return FootPosSolverScript.RotationFromNormal(root.forward, hit.Normal);
        }

        private void SwitchContext(Leg currentLeg) {
            // switch the data (from left foot --> to right foot)
            if (currentLeg == Leg.Left) {
                _context.CurrentLeg = Leg.Right;
                _context.CurrentFoot = _rightFoot;
            }
            else {
                _context.CurrentLeg = Leg.Left;
                _context.CurrentFoot = _leftFoot;
            }
        }
    }
}
