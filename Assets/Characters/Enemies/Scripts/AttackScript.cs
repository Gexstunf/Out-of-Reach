using System;
using Characters.Animation;
using Characters.Enemies.Scripts.Plant;
using Characters.LifeSupportSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Enemies.Scripts {
    public class AttackScript : MonoBehaviour {
        
        [Header("References")]
        // animator controller reference
        [SerializeField] private AnimationControllerManagerScript<PlantAnimController.EPlantStates> animatorManager;
        [SerializeField] private TargetingScript targetingScript;

        [Header("Settings")]
        public AttackDamageSO currentAttackSO;
        [SerializeField] private AttackDamageSO[] allAttacksSO;
        [SerializeField] private Collider[] limbColliders;
        public float attackCooldownTime = 2f;

        private float _cooldownTimer;

        private void Start() {
            targetingScript = GetComponent<TargetingScript>();
            animatorManager = GetComponent<AnimationControllerManagerScript<PlantAnimController.EPlantStates>>();
            if (animatorManager == null)
            {
                Debug.LogError("Missing an - AnimationControllerManagerScript<PlantAnimController.EPlantStates> - component on this GameObject!");
            }

            foreach (var col in limbColliders) {
                Debug.Log("Adding limb damage to: " + col.name);
                var limb = col.gameObject.AddComponent<LimbDamageScript>();
                limb.attackScript = this;
            }
        }

        private void Update() {
            _cooldownTimer -= Time.deltaTime;
            if (targetingScript.CurrentTargetTransform && _cooldownTimer <= 0f) {
                float distanceFromCurrentTarget = Vector3.Distance(transform.position, targetingScript.CurrentTargetTransform.position);

                if (distanceFromCurrentTarget < targetingScript.detectionRadius) {
                    Attack();
                    _cooldownTimer = attackCooldownTime;
                    Debug.Log("Attacking!");
                }
            }
        }

        private void Attack() {
            animatorManager.Trigger(PlantAnimController.EPlantStates.MediumAttack);
        }
    }
}
