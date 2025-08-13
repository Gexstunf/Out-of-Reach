


using Characters.PlayerController.Scripts.Input;
using Characters.StateMachine.PlayerStateMachine;
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
            Transform rootTransform, PlayerInputScript inputScript, Camera playerCamera, LayerMask groundLayer, PlayerStateMachineScript stateMachine,
            Rigidbody rigidBody) 
        {
            _leftIkConstraint = leftIkConstraint;
            _rightIkConstraint = rightIkConstraint;
            _leftMultiRotationConstraint = leftMultiRotationConstraint;
            _rightMultiRotationConstraint = rightMultiRotationConstraint;
            _rootTransform = rootTransform;
            _rigidBody = rigidBody;
            _inputScript = inputScript;
            _playerCamera = playerCamera;
            _groundLayerMask = groundLayer;
            _playerStateMachine = stateMachine;
        }

        [SerializeField] private LayerMask _groundLayerMask;
        [SerializeField] private Transform _rootTransform;
        [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
        [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
        [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
        [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
        [SerializeField] private PlayerInputScript _inputScript;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private PlayerStateMachineScript _playerStateMachine;
        [SerializeField] private Rigidbody _rigidBody;
        
        public TwoBoneIKConstraint LeftIkConstraint => _leftIkConstraint;
        public TwoBoneIKConstraint RightIkConstraint => _rightIkConstraint;
        public MultiRotationConstraint LeftMultiRotationConstraint => _leftMultiRotationConstraint;
        public MultiRotationConstraint RightMultiRotationConstraint => _rightMultiRotationConstraint;
        
        public PlayerStateMachineScript StateMachine => _playerStateMachine;
        
        public Collider CurrentInteractionCollider { get; set; }
        
        public TwoBoneIKConstraint PreviousIkConstraint { get; private set; }
        public TwoBoneIKConstraint CurrentIkConstraint { get; private set; }
        
        public MultiRotationConstraint PreviousMultiRotationConstraint { get; private set; }
        public MultiRotationConstraint CurrentMultiRotationConstraint { get; private set; }
        
        public EStep PreviousStep { get; private set; }
        public EStep CurrentStep {get; private set;}
        
        
        public Transform PreviousIkTargetTransform {get; private set;}
        public Transform CurrentIkTargetTransform { get; private set; }
        public Transform CurrentLegShoulderTransform { get; private set; }
        public Transform RootTransform => _rootTransform;
        public Vector3 RightTargetOffsetPosition { get; private set; }
        public Vector3 LeftTargetOffsetPosition { get; private set; }
        public float SideTargetOffset = 0.3f;


        public Vector3 ClosestPointOnColliderFromLegShoulderTransform { get; set; }
        public float MaxGroundCheckDistance => 6f;
        
        public PlayerInputScript InputScript => _inputScript;
        public Camera Camera => _playerCamera;
        public LayerMask GroundLayer => _groundLayerMask;
        public Rigidbody Rigidbody => _rigidBody;

        public void SetCurrentStep(EStep step) {
            CurrentStep = step;
            PreviousStep = GetOppositeStep(CurrentStep);
            
            if (step == EStep.Left) {
                
                PreviousIkConstraint = _rightIkConstraint;
                CurrentIkConstraint = _leftIkConstraint;
                
                PreviousMultiRotationConstraint = _rightMultiRotationConstraint;
                CurrentMultiRotationConstraint = _leftMultiRotationConstraint;
            }
            else {

                PreviousIkConstraint = _leftIkConstraint;
                CurrentIkConstraint = _rightIkConstraint;
                
                PreviousMultiRotationConstraint = _leftMultiRotationConstraint;
                CurrentMultiRotationConstraint = _rightMultiRotationConstraint;
            }
            // no need for previous in this one
            CurrentLegShoulderTransform = CurrentIkConstraint.data.root.transform;
            
            PreviousIkTargetTransform = PreviousIkConstraint.data.target.transform;
            CurrentIkTargetTransform = CurrentIkConstraint.data.target.transform;
        }
        
        public EStep GetOppositeStep(EStep step) 
        {
            return step == EStep.Left ? EStep.Right : EStep.Left;
        }

        public void SetIkTargetLocalPosition(Vector3 position) {
            CurrentIkTargetTransform.localPosition = position;
        }

        public void SetIkPreviousTargetWorldPosition(Vector3 position) {
            PreviousIkTargetTransform.position = position;
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
