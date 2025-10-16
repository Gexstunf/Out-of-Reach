using System;
using System.Linq;
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
        [SerializeField]
        private AnimControllerManagerBaseScript animatorManager;

        [SerializeField] private TargetingScript targetingScript;

        [Header("Settings")] public AttackDamageSO currentAttackSO;
        [SerializeField] private AttackDamageSO[] allAttacksSO;
        [SerializeField] private Collider[] limbColliders;
        public bool randomizeBetweenValidAttacks = false;


        [Header("Attacks available")] [SerializeField]
        private AttackDefinitionScript[] attacks;

        private float _cooldownTimer;

        private void Reset() {
            animatorManager = GetComponent<AnimControllerManagerBaseScript>();
            targetingScript = GetComponent<TargetingScript>();
        }

        private void Start() {
            if (animatorManager == null) animatorManager = GetComponent<AnimControllerManagerBaseScript>();
            if (targetingScript == null) targetingScript = GetComponent<TargetingScript>();

            if (animatorManager == null)
                Debug.LogError("AttackScript requires an AnimationControllerManagerBase (concrete AnimationControllerManagerScript<T>) on this GameObject!!");

            foreach (var col in limbColliders ?? Array.Empty<Collider>()) {
                if (col == null) continue;
                var limb = col.gameObject.GetComponent<ILimbDamageScript>();
                if (limb == null) {
                    var added = col.gameObject.AddComponent<LimbDamageScript>();
                    added.SetHostAttackScript(this);
                }
                else {
                    // if someone implemented ILimbDamageScript manually, try to set host if possible:
                    if (limb is LimbDamageScript existingGeneric) {
                        existingGeneric.SetHostAttackScript(this);
                    }
                }
            }
        }

        private void Update() {
            _cooldownTimer -= Time.deltaTime;

            if (!targetingScript?.CurrentTargetTransform || _cooldownTimer > 0f) return;

            float distance = Vector3.Distance(transform.position, targetingScript.CurrentTargetTransform.position);

            var valid = attacks?.Where(a => distance <= a.range && a.selectionWeight > 0f).ToArray();
            if (valid == null || valid.Length == 0) return;

            AttackDefinitionScript chosen = null;
            if (randomizeBetweenValidAttacks)
                chosen = PickWeighted(valid);
            else
                chosen = PickByHighestRange(valid, distance); // prefer larger range that still fits

            if (chosen != null)
                DoAttack(chosen);
        }


        private AttackDefinitionScript PickByHighestRange(AttackDefinitionScript[] valid, float distance) {
            // prefer attack with smallest range >= distance (closest fit) or largest? this uses highest range as tie-breaker
            return valid.OrderBy(a => a.range).FirstOrDefault();
        }

        private AttackDefinitionScript PickWeighted(AttackDefinitionScript[] valid) {
            float total = valid.Sum(a => a.selectionWeight);
            if (total <= 0f) return valid[0];

            float r = UnityEngine.Random.Range(0f, total);
            float acc = 0f;
            foreach (var a in valid) {
                acc += a.selectionWeight;
                if (r <= acc) return a;
            }

            return valid[valid.Length - 1];
        }

        private void DoAttack(AttackDefinitionScript atk) {
            if (atk == null) return;
            _cooldownTimer = atk.cooldown;
            currentAttackSO = atk.damageSO;
            if (animatorManager != null) {
                // use string-based trigger so we stay enum-agnostic
                animatorManager.TriggerByName(atk.animationStateName);
            }
        }
    }
}
