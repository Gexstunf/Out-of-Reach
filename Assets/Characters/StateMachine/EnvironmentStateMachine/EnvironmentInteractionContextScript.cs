


using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Characters.StateMachine.EnvironmentStateMachine {
    public class EnvironmentInteractionContextScript {
        public enum EStep {
            Left,
            Right
        }
        
        private Transform _rootTransform;
        
        public EnvironmentInteractionContextScript( TwoBoneIKConstraint leftIkConstraint, TwoBoneIKConstraint rightIkConstraint,
            MultiRotationConstraint leftMultiRotationConstraint, MultiRotationConstraint rightMultiRotationConstraint,
            Transform rootTransform) 
        {
            _leftIkConstraint = leftIkConstraint;
            _rightIkConstraint = rightIkConstraint;
            _leftMultiRotationConstraint = leftMultiRotationConstraint;
            _rightMultiRotationConstraint = rightMultiRotationConstraint;
            _rootTransform = rootTransform;
        }

        
        [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
        [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
        [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
        [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
        
        public TwoBoneIKConstraint LeftIkConstraint => _leftIkConstraint;
        public TwoBoneIKConstraint RightIkConstraint => _rightIkConstraint;
        public MultiRotationConstraint LeftMultiRotationConstraint => _leftMultiRotationConstraint;
        public MultiRotationConstraint RightMultiRotationConstraint => _rightMultiRotationConstraint;
        
        public TwoBoneIKConstraint CurrentIkConstraint { get; private set; }
        public MultiRotationConstraint CurrentMultiRotationConstraint { get; private set; }
        public EStep CurrentStep {get; private set;}
        public Transform CurrentIkTargetTransform { get; private set; }
        public Transform CurrentLegShoulderTransform { get; private set; }
        public Transform RootTransform => _rootTransform;
        
        
        public void SetCurrentSide(Vector3 positionToCheck) {
            Vector3 leftLegShoulderPosition = _leftIkConstraint.data.root.transform.position;
            Vector3 rightLegShoulderPosition = _rightIkConstraint.data.root.transform.position;
            
            bool isLeftCloser = Vector3.Distance(positionToCheck, leftLegShoulderPosition) < Vector3.Distance(positionToCheck, rightLegShoulderPosition);

            if (isLeftCloser) {
                CurrentStep = EStep.Left;
                CurrentIkConstraint = _leftIkConstraint;
                CurrentMultiRotationConstraint = _leftMultiRotationConstraint;
            }
            else {
                CurrentStep = EStep.Right;
                CurrentIkConstraint = _rightIkConstraint;
                CurrentMultiRotationConstraint = _rightMultiRotationConstraint;
            }
            
            CurrentLegShoulderTransform = CurrentIkConstraint.data.root.transform;
            CurrentIkTargetTransform = CurrentIkConstraint.data.target.transform;
        }
    }
}
