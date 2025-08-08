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
        
        public EnvironmentInteractionContextScript Context { get; private set; }
        
        [Header("References")]
        [SerializeField] private PlayerStateMachineScript _playerStateMachine;
        [SerializeField] private CapsuleCollider _rootCollider;
        
        
        [Header("Environment interaction settings")]
        [SerializeField] private TwoBoneIKConstraint _leftIkConstraint;
        [SerializeField] private TwoBoneIKConstraint _rightIkConstraint;
        [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
        [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
        
        private void Awake() {
            _playerStateMachine = GetComponent<PlayerStateMachineScript>();
            _rootCollider = GetComponent<CapsuleCollider>();
            
            Context = new EnvironmentInteractionContextScript(_leftIkConstraint, _rightIkConstraint, 
                _leftMultiRotationConstraint, _rightMultiRotationConstraint, transform.root);
                
            ValidateConstraints();
            InitializeStates();
            ConstructTerrainDetectorCollider();
        }

        private void InitializeStates() {
            States.Add(EEnvironmentActions.Rise, new RiseStateScript(Context, EEnvironmentActions.Rise));
            States.Add(EEnvironmentActions.Touch, new TouchStateScript(Context, EEnvironmentActions.Touch));
        }
        
        private void ValidateConstraints() {
            Assert.IsNotNull(_leftIkConstraint, "Left IK constraint is not assigned!");
            Assert.IsNotNull(_rightIkConstraint, "Right IK constraint is not assigned!");
            Assert.IsNotNull(_leftMultiRotationConstraint, "Left Multi rotation constraint is not assigned!");
            Assert.IsNotNull(_rightMultiRotationConstraint, "Right Multi rotation constraint is not assigned!");
        }
        
        private void ConstructTerrainDetectorCollider() {
            float legSpan = _rootCollider.height / 2;

            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(legSpan + 0.4f, legSpan, legSpan + 0.4f);
            boxCollider.center = new Vector3(_rootCollider.center.x, _rootCollider.center.y - (legSpan/2f + 0.7f), _rootCollider.center.z);
            boxCollider.isTrigger = true;
        }
    }
}
