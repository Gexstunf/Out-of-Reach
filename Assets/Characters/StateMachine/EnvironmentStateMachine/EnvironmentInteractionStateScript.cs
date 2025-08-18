using Characters.PlayerController.Scripts.StateMachine;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine {
    public abstract class EnvironmentInteractionStateScript : BaseStateScript<EnvironmentInteractionStateMachineScript.EEnvironmentActions>
    {
        protected EnvironmentInteractionContextScript Context;

        public EnvironmentInteractionStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions state) : base(state)
        {
            Context = context;
        }
        
        protected float StepThreshold { get; private set; }
    
        public Vector3 GetClosestPoint(Collider intersectingCollider, Vector3 positionToCheck) {
            return intersectingCollider.ClosestPoint(positionToCheck);
        }

        protected void StartIkTargetPositionTracking(Collider intersectingCollider) {
            if (intersectingCollider.gameObject.layer != LayerMask.NameToLayer("Terrain")) return;
            if (Context.CurrentInteractionCollider != null) return;
        }
        
        protected void UpdateIkTargetPositionTracking(Collider intersectingCollider) {
        }
        
        protected void ResetIkTargetPositionTracking(Collider intersectingCollider) {
            if (intersectingCollider == Context.CurrentInteractionCollider) {
                Context.CurrentInteractionCollider = null;
            };
        }

        protected void SetStepThreshold(float threshold) {
            StepThreshold = threshold;
        }
        
        protected Vector3 GetGroundPos() {
            Vector3 pos = Vector3.zero;
            if (Physics.Raycast(
                    Context.CurrentIkTargetTransform.position,
                    Vector3.down,
                    out RaycastHit hit,
                    maxDistance: Context.MaxGroundCheckDistance,
                    Context.GroundLayer)) 
            {
                pos = hit.point;
            }
            else {
                Debug.Log("No raycast hit");
                return  Context.CurrentIkTargetTransform.position;
            }
            return pos;
        }

        protected Vector3 ApplySideOffset(Vector3 position)
        {
            Vector3 rightDir = Context.RootTransform.right;
            float directionSign = (Context.CurrentStep == EnvironmentInteractionContextScript.EStep.Left) ? -1f : 1f;
            return position + rightDir * (Context.SideTargetOffset * directionSign);
        }
        
        protected bool MovementThresholdReached(Vector3 startPos) {
            Vector3 rootDelta = Context.RootTransform.position - startPos;
            float planarDistance = new Vector2(rootDelta.x, rootDelta.z).magnitude;
            return planarDistance >= StepThreshold;
        }
    }
}
