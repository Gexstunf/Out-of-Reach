using System;
using System.Collections.Generic;
using Characters.LifeSupportSystem;
using Items.Scripts;
using UnityEngine;

namespace Characters.Enemies.Scripts {
    public class NerveScript : MonoBehaviour {
        [SerializeField] private NervousSystemScript nervousSystemHostScript;
        private List<TagHandle> _tags;

        private void OnCollisionEnter(Collision other) {
            if (!CollidedWithTags(other)) return;

            HurtingObjectScript hurtScript = GetHurtingObject(other.gameObject);
            AttackDamageSO attackSo = GetAttackScript(other.gameObject);

            if (hurtScript == null && attackSo == null) return;
            if (nervousSystemHostScript.debug) Debug.Log($"Triggered nerve!");
            nervousSystemHostScript.TriggerNerves(hurtScript, attackSo);
        }

        private bool CollidedWithTags(Collision other) {
            if (nervousSystemHostScript.debug) Debug.Log($"Collision on bone: {name} with: {other.gameObject.name}");


            foreach (var t in _tags) {
                if (nervousSystemHostScript.debug) Debug.Log($"Comparing nerve tag: {t} with: {other.gameObject.tag}");
                if (other.gameObject.CompareTag(t)) // FIX HERE
                    return true;
            }

            return false;
        }

        public void SetTags(List<TagHandle> tagList) {
            _tags = tagList; // FIX HERE
        }

        public void SetHost(NervousSystemScript host) {
            nervousSystemHostScript = host;
        }

        private static HurtingObjectScript GetHurtingObject(GameObject obj) => obj.GetComponent<HurtingObjectScript>();

        private static AttackDamageSO GetAttackScript(GameObject obj) {
            var script = obj.GetComponent<ILimbDamageScript>();
            return script?.HostAttackScript?.enabled == true
                ? script.HostAttackScript.currentAttackSO
                : null;
        }
    }
}