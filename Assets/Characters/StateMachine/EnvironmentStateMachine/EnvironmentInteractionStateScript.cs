using Characters.PlayerController.Scripts.StateMachine;
using UnityEngine;

namespace Characters.StateMachine.EnvironmentStateMachine {
    public abstract class EnvironmentInteractionStateScript : BaseStateScript<EnvironmentInteractionStateMachineScript.EEnvironmentActions>
    {
        protected EnvironmentInteractionContextScript Context;

        public EnvironmentInteractionStateScript(EnvironmentInteractionContextScript context, EnvironmentInteractionStateMachineScript.EEnvironmentActions state) : base(state)
        {
            Context = context;
        }
    
        public Vector3 GetClosestPoint(Collider intersectingCollider, Vector3 positionToCheck) {
            return intersectingCollider.ClosestPoint(positionToCheck);
        }

        protected void StartIkTargetPositionTracking(Collider intersectingCollider) {
            Vector3 closestPointFromRoot = GetClosestPoint(intersectingCollider, Context.RootTransform.position);
            Context.SetCurrentSide(closestPointFromRoot);
        }
        
        protected void UpdateIkTargetPositionTracking(Collider intersectingCollider) {
            
        }
        
        protected void ResetIkTargetPositionTracking(Collider intersectingCollider) {
            
        }
    }
}
