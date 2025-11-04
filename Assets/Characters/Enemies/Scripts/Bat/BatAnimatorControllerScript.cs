using System;
using Characters.Animation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Enemies.Scripts.Bat {
    public class BatAnimatorControllerScript : AnimationControllerManagerScript<BatAnimatorControllerScript.EBatStates>
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Header("References")]
        [SerializeField] private Animator batAnimator;
        [SerializeField] private EnemyAgentScript _agentScript;
        [SerializeField] private TargetingScript _targetingScript;

        
        private readonly int _speedHash = Animator.StringToHash("Speed");
        private readonly int _sleepingHash = Animator.StringToHash("Sleeping");
        
        public enum EBatStates
        {
            SpinAttack,
            GlideAttack,
            HeavyAttack,
            Alive,
            Dead,
            Rest,
            Sleep
        }

        private void Awake() {
            _agentScript = GetComponent<EnemyAgentScript>();
        }

        private void Update() {
            batAnimator.SetFloat(_speedHash, _agentScript.Agent.velocity.magnitude);
            batAnimator.SetBool(_sleepingHash, !_agentScript.IsChasing);
        }

        protected override void SetAnimatorBool(EBatStates batState, bool value) {
            batAnimator.SetBool(batState.ToString(), value);
        }

        protected override void SetAnimatorTrigger(EBatStates batState) {
            batAnimator.SetTrigger(batState.ToString());
        }

        protected override void SetAnimatorFloat(EBatStates batState, float value) {
            batAnimator.SetFloat(batState.ToString(), value);
        }
    }
}
