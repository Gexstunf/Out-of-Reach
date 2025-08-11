


using Characters.PlayerController.Scripts.Input;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Characters.StateMachine.EnvironmentStateMachine {
    public class EnvironmentInteractionContextScript {
        public enum EStep {
            Left,
            Right
        }
        
        
        public EnvironmentInteractionContextScript( TwoBoneIKConstraint leftIkConstraint, TwoBoneIKConstraint rightIkConstraint,
            MultiRotationConstraint leftMultiRotationConstraint, MultiRotationConstraint rightMultiRotationConstraint,
            Transform rootTransform, PlayerInputScript inputScript, Camera playerCamera, LayerMask groundLayer) 
        {
            _leftIkConstraint = leftIkConstraint;
            _rightIkConstraint = rightIkConstraint;
            _leftMultiRotationConstraint = leftMultiRotationConstraint;
            _rightMultiRotationConstraint = rightMultiRotationConstraint;
            _rootTransform = rootTransform;
            _inputScript = inputScript;
            _playerCamera = playerCamera;
            _groundLayerMask = groundLayer;
        }

        [SerializeField] private LayerMask _groundLayerMask;
        [SerializeField] private Transform _rootTransform;
        [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
        [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
        [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
        [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
        [SerializeField] private PlayerInputScript _inputScript;
        [SerializeField] private Camera _playerCamera;
        
        public TwoBoneIKConstraint LeftIkConstraint => _leftIkConstraint;
        public TwoBoneIKConstraint RightIkConstraint => _rightIkConstraint;
        public MultiRotationConstraint LeftMultiRotationConstraint => _leftMultiRotationConstraint;
        public MultiRotationConstraint RightMultiRotationConstraint => _rightMultiRotationConstraint;
        
        public Collider CurrentInteractionCollider { get; set; }
        
        public TwoBoneIKConstraint CurrentIkConstraint { get; private set; }
        public MultiRotationConstraint CurrentMultiRotationConstraint { get; private set; }
        public EStep CurrentStep {get; private set;}
        
        public Transform CurrentIkTargetTransform { get; private set; }
        public Transform CurrentLegShoulderTransform { get; private set; }
        public Transform RootTransform => _rootTransform;
        public Vector3 RightTargetOffsetPosition { get; private set; }
        public Vector3 LeftTargetOffsetPosition { get; private set; }


        public Vector3 ClosestPointOnColliderFromLegShoulderTransform { get; set; }
        
        public PlayerInputScript InputScript => _inputScript;
        public Camera Camera => _playerCamera;
        public LayerMask GroundLayer => _groundLayerMask;

        public void SetCurrentStep(EStep step) {
            CurrentStep = step;
            if (step == EStep.Left) {
                CurrentIkConstraint = _leftIkConstraint;
                CurrentMultiRotationConstraint = _leftMultiRotationConstraint;
            }
            else {
                CurrentIkConstraint = _rightIkConstraint;
                CurrentMultiRotationConstraint = _rightMultiRotationConstraint;
            }
            
            CurrentLegShoulderTransform = CurrentIkConstraint.data.root.transform;
            CurrentIkTargetTransform = CurrentIkConstraint.data.target.transform;
        }

        public void SetIkTargetLocalPosition(Vector3 position) {
            CurrentIkTargetTransform.localPosition = position;
        }

        public void SetIkTargetWorldPosition(Vector3 position) {
            CurrentIkTargetTransform.position = position;
        }

        public void SetTargetOffset(Vector3 leftOffset, Vector3 rightOffset) {
            RightTargetOffsetPosition = rightOffset;
            LeftTargetOffsetPosition = leftOffset;
        }
    }
}
