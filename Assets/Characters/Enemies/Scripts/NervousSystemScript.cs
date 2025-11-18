using System;
using System.Collections.Generic;
using System.Linq;
using Characters.ActiveRagdollSystem;
using Characters.LifeSupportSystem;
using Items.Scripts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Enemies.Scripts {
    public class NervousSystemScript : MonoBehaviour {
        protected List<TagHandle> TagHandles { get; } = new ();
        [Header("References")]
        [SerializeField] protected ActiveRagdollCoreScript arCoreScript;
        [Header("Settings")]
        [SerializeField] protected List<TagClass> tags = new ();
        public bool debug;

        private bool _triggered = false;
        private HurtingObjectScript _hurtingObjectScript;
        private AttackDamageSO _attackDamageSo;

        [System.Serializable]
        public class TagClass {
            public string TagString;
        }
            
        private void Awake() {
            arCoreScript = GetComponent<ActiveRagdollCoreScript>();
            if (tags == null || tags.Count == 0) return;

            foreach (var tag in tags) {
                TagHandle handle = TagHandle.GetExistingTag(tag.TagString);
                TagHandles.Add(handle);
            }
        }

        private void Start() {
            if (!arCoreScript) return;
            foreach (var bone in arCoreScript.boneMaps) {
                if (!bone.collider) continue;
                NerveScript script = bone.collider.gameObject.AddComponent<NerveScript>();
                script.SetTags(TagHandles);
                script.SetHost(this);
            }
        }

        #region  Public API
        public bool NervesTriggered => _triggered;
        public HurtingObjectScript HurtingScript => _hurtingObjectScript;
        public AttackDamageSO AttackDamageSO => _attackDamageSo;
        
        public void TriggerNerves(HurtingObjectScript hurtScript, AttackDamageSO attack) {
            _triggered = true;
            _hurtingObjectScript = hurtScript;
            _attackDamageSo = attack;
        }
        
        public void ResetNerves() {
            _triggered = false;
            _hurtingObjectScript = null;
            _attackDamageSo = null;
        }
        #endregion
    }
}
