using System;
using Characters.PlayerController.Scripts;
using Characters.PlayerController.Scripts.Input;
using Characters.PlayerController.Scripts.StateMachine;
using Characters.StateMachine.EnvironmentStateMachine.ConcreteStates;
using Characters.StateMachine.PlayerStateMachine;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Characters.StateMachine.EnvironmentStateMachine {
    public class EnvironmentInteractionStateMachineScript : StateManagerScript<EnvironmentInteractionStateMachineScript.EEnvironmentActions> {
        public enum EEnvironmentActions {
            Rise,
            Touch
        }

        private EnvironmentInteractionContextScript _context;
        
        [Header("References")]
        [SerializeField] private PlayerStateMachineScript _playerStateMachine;
        [SerializeField] private PlayerInputScript _inputScript;
        [SerializeField] private CapsuleCollider _rootCollider;
        [SerializeField] private Camera _camera;
        
        
        [Header("Environment interaction settings")]
        [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
        [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
        [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
        [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private Rigidbody _rigidBody;
        private void Awake() {
            _playerStateMachine = GetComponent<PlayerStateMachineScript>();
            _rootCollider = GetComponent<CapsuleCollider>();
            _inputScript = GetComponent<PlayerInputScript>();
            _rigidBody = GetComponent<Rigidbody>();

            _context = new EnvironmentInteractionContextScript(_leftIkConstraint, _rightIkConstraint,
                _leftMultiRotationConstraint, _rightMultiRotationConstraint, transform.root, _inputScript, _camera,
                _groundLayer, _playerStateMachine, _rigidBody);
            
            ValidateConstraints();
            InitializeStates();
            //ConstructTerrainDetectorCollider();

            _context.SetCurrentStep(EnvironmentInteractionContextScript.EStep.Right);            
            _context.SetTargetOffset(_leftIkConstraint.data.target.localPosition, _rightIkConstraint.data.target.localPosition);
        }
        
        private void InitializeStates() {
            States.Add(EEnvironmentActions.Touch, new TouchStateScript(_context, EEnvironmentActions.Touch));
            States.Add(EEnvironmentActions.Rise, new RiseStateScript(_context, EEnvironmentActions.Rise));

            CurrentState = States[EEnvironmentActions.Touch];
        }
        
        private void ValidateConstraints() {
            Assert.IsNotNull(_leftIkConstraint, "Left IK constraint is not assigned!");
            Assert.IsNotNull(_rightIkConstraint, "Right IK constraint is not assigned!");
            Assert.IsNotNull(_leftMultiRotationConstraint, "Left Multi rotation constraint is not assigned!");
            Assert.IsNotNull(_rightMultiRotationConstraint, "Right Multi rotation constraint is not assigned!");
            Assert.IsNotNull(_groundLayer, "Ground layer is not assigned!");
        }
        
        private void ConstructTerrainDetectorCollider() {
            float legSpan = _rootCollider.height / 2;

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(legSpan + 0.4f, legSpan, legSpan + 0.4f);
            boxCollider.center = new Vector3(_rootCollider.center.x, _rootCollider.center.y - (legSpan/2f + 0.7f), _rootCollider.center.z);
            boxCollider.isTrigger = true;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            if (_context != null && _context.ClosestPointOnColliderFromLegShoulderTransform != null) {
                Gizmos.DrawSphere(_context.ClosestPointOnColliderFromLegShoulderTransform, 0.3f);
            }
        }
    }
}
