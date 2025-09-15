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
            public float Time;                // 0..1 progress
            public float CooldownTimer;    // seconds left until allowed to step again
        }

        private struct CurrentContext {
            public FootState CurrentFoot;
            public FootState PreviousCurrentFoot;
            public Leg CurrentLeg;
            public Rigidbody RigidBody;
            public Vector3 CurrentFootPos;
            public readonly Transform RootTransform;
            public Vector3 PreviousRootPosition;
            public FootIKSettingsSO FootSettings;

            public CurrentContext(Vector3 currentFootPos, Leg currentLeg, FootState currentFoot, FootState previousCurrentFoot, Transform rootTransform, Vector3 previousRootPosition, FootIKSettingsSO footSettings, Rigidbody rigidBody) {
                CurrentFootPos = currentFootPos;
                CurrentLeg = currentLeg;
                CurrentFoot = currentFoot;
                RootTransform = rootTransform;
                PreviousRootPosition = previousRootPosition;
                FootSettings = footSettings;
                RigidBody = rigidBody;
                PreviousCurrentFoot = previousCurrentFoot;
            }
        }
        

        private readonly FootState _leftFoot = new FootState();
        private readonly FootState _rightFoot = new FootState();
        private CurrentContext _context;
        
        private int _stepTurnCounter = 0;
        private float _lastTurnSign = 0f;
        private float _startDegrees;
        private float _forwardOffset;
        
        // Public outputs (read after UpdateGait)
        public Vector3 LeftFootTargetPos  { get; private set; }
        public Vector3 RightFootTargetPos { get; private set; }
        public Quaternion LeftFootTargetRot  { get; private set; } = Quaternion.identity;
        public Quaternion RightFootTargetRot { get; private set; } = Quaternion.identity;


        public GaitPlannerScript(Vector3 initialLeftPos, Vector3 initialRightPos, Quaternion initialLeftRot, Quaternion initialRightRot, Transform rootTransform, FootIKSettingsSO footSettings, Rigidbody rigidBody)
        {
            _leftFoot.PlantedPos  = initialLeftPos;  _leftFoot.PlantedRot  = initialLeftRot;
            _rightFoot.PlantedPos = initialRightPos; _rightFoot.PlantedRot = initialRightRot;
            _context = new CurrentContext(initialLeftPos, Leg.Left, _leftFoot, _rightFoot, rootTransform, rootTransform.position, footSettings, rigidBody);
            LeftFootTargetPos = initialLeftPos;   LeftFootTargetRot = initialLeftRot;
            RightFootTargetPos = initialRightPos; RightFootTargetRot = initialRightRot;
        }

        public void UpdateGait(
            float deltaTime,
            GroundHit leftHit,
            GroundHit rightHit,
            Vector3 moveInput
        ) {
            var settings = _context.FootSettings;
            
            if (_context.CurrentFoot.Time >= settings.totalStepDuration) {
                FinishStep(_context.CurrentFoot);
                SwitchContext(_context.CurrentLeg);
                _startDegrees = _context.RootTransform.eulerAngles.y;
            }
            
            bool bothPlanted = (_leftFoot.State == StepState.Planted && _rightFoot.State == StepState.Planted);
            
            //if (ShouldStep(_context.CurrentFoot, rootTransform, settings.stepThreshold) && bothPlanted) Debug.Log("It should step once");
            
            if ((bothPlanted && ShouldStep(_context.PreviousRootPosition)) || (bothPlanted && RotationThresholdReached(_startDegrees))) {
                GroundHit currentHit = _context.CurrentLeg == Leg.Right ? rightHit : leftHit;
                Vector3 processedEndPos = currentHit.Position;
                SetDynamicForwardOffset(_context.RigidBody.linearVelocity);
                AdjustTargetWithMovement(moveInput, ref processedEndPos);
                BeginStep(_context.CurrentFoot, processedEndPos, quaternion.identity);
                _context.PreviousRootPosition = _context.RootTransform.position;
            }
            
            UpdateCurrentFoot(deltaTime);
        }

        private void BeginStep(FootState foot, Vector3 endPosWorld, Quaternion endRotWorld)
        {
            foot.State = StepState.Moving;
            foot.Time = 0f;
            foot.MoveStartPos = foot.PlantedPos;
            foot.MoveStartRot = foot.PlantedRot;
            foot.MoveEndPos = endPosWorld;
            foot.MoveEndRot = endRotWorld;
            _context.PreviousRootPosition = _context.RootTransform.position;
        }
        
        private void FinishStep(FootState foot) {
            foot.State = StepState.Planted;
            foot.Time = 0f;
            foot.PlantedPos = _context.CurrentFootPos;
            foot.PlantedRot = foot.MoveEndRot;
        }

        private void UpdateCurrentFoot(float time) {
            if (_context.CurrentFoot.State == StepState.Moving) {
                _context.CurrentFoot.Time += time;
                
                Vector3 targetPoint = _context.CurrentFoot.MoveEndPos;
                Vector3 startPos = _context.CurrentFoot.MoveStartPos;
                
                float processedTime = _context.CurrentFoot.Time / _context.FootSettings.totalStepDuration;  
                processedTime = Mathf.Clamp01(processedTime);
                Vector3 lerpPos = Vector3.Lerp(startPos, targetPoint, processedTime);
            
                lerpPos.y += Mathf.Sin(processedTime * Mathf.PI) * _context.FootSettings.stepHeight;
                _context.CurrentFootPos = lerpPos;
            
                if (_context.CurrentLeg == Leg.Left) {
                    LeftFootTargetPos = lerpPos;
                }
                else {
                    RightFootTargetPos = lerpPos;
                }
            }
        }
        
        private bool ShouldStep(Vector3 previousPos)
        {
            float distance = Vector3.Distance(previousPos, _context.RootTransform.position);
            if (_context.FootSettings.usePlanarDistance) {
                distance = PlanarDistance(previousPos, _context.RootTransform.position);
            } 
            return distance > _context.FootSettings.stepThreshold;
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
            
        private void SetDynamicForwardOffset(Vector3 speed) {
            float planeSpeed = new Vector2(speed.x, speed.z).magnitude;
            _forwardOffset =+ (planeSpeed * _context.FootSettings.offsetScaleFactor);
        }
        
        private void AdjustTargetWithMovement(Vector3 moveInput, ref Vector3 targetPos) {
            if (!(moveInput.sqrMagnitude > 0.01f)) return;
            float maxForward = _context.FootSettings.maxStepDistance;

            Vector3 localInput = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

            Vector3 movementDir = _context.RootTransform.TransformDirection(localInput);
            movementDir.y = 0f;

            Vector3 clampedTarget = targetPos + movementDir * Mathf.Clamp(_forwardOffset, -maxForward, maxForward);
            targetPos = clampedTarget;
        }
        
        private static Quaternion AimRot(Transform root, GroundHit hit, FootIKSettingsSO settings)
        {
            if (!settings.alignToSurface) return Quaternion.LookRotation(root.forward, Vector3.up);
            return FootPosSolverScript.RotationFromNormal(root.forward, hit.Normal);
        }

        private void SwitchContext(GaitPlannerScript.Leg currentLeg) {  
            // switch the data (from left foot --> to right foot)
            if (currentLeg == GaitPlannerScript.Leg.Left) {
                _context.CurrentLeg = GaitPlannerScript.Leg.Right;
                _context.CurrentFoot = _rightFoot;
                _context.PreviousCurrentFoot = _leftFoot;
            }
            else {
                _context.CurrentLeg = GaitPlannerScript.Leg.Left;
                _context.CurrentFoot = _leftFoot;
                _context.PreviousCurrentFoot = _rightFoot;
            }
        }
        
        private bool RotationThresholdReached(float startDeg) {
            float turnDiff = Mathf.DeltaAngle(startDeg, _context.RootTransform.rotation.eulerAngles.y);

            if (Mathf.Abs(turnDiff) < 20f)
                return false;

            float turnSign = Mathf.Sign(turnDiff);

            if (turnSign != _lastTurnSign) {
                _stepTurnCounter = 0;
                _lastTurnSign = turnSign;
            }

            _stepTurnCounter++;

            var dominantFoot = (turnSign < 0) 
                ? Leg.Right 
                : Leg.Left;

            bool useDominant = (_stepTurnCounter % 2 == 1);
            SwitchContext(useDominant ? dominantFoot : GetOppositeLeg(dominantFoot));
            return true;
        }

        private Leg GetOppositeLeg(Leg leg) {
            if (leg == Leg.Left) {
                return Leg.Right;
            }
            else {
                return Leg.Left;
            }
        }
    }
}
