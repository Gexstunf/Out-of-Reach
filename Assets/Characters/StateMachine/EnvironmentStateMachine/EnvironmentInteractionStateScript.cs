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
        
        protected Vector3 GetPivotedTarget(Transform origin, Vector2 movement, float distance) {
            Vector3 localMove = new Vector3(movement.x, 0f, movement.y);

            if (localMove.sqrMagnitude > 1f)
                localMove.Normalize();

            localMove *= distance;

            Vector3 worldMove = origin.rotation * localMove;
            return worldMove;
        }
        
        protected Vector3 GetGroundPos() {
            Vector3 pos = Vector3.zero;
            if (Physics.Raycast(
                    Context.CurrentIkTargetTransform.position,
                    Vector3.down,
                    out RaycastHit hit,
                    maxDistance: 3f,
                    Context.GroundLayer)) 
            {
                pos = hit.point;
            }
            else {
                Debug.Log("Raycast failed!");
                return Context.CurrentIkTargetTransform.position;
            }
            return pos;
        }
    }
}
