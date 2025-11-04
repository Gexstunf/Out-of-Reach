using System;
using Characters.ActiveRagdollSystem;
using Items.Scripts;
using UnityEngine;

namespace Characters.Enemies.Scripts {
    public class NervousSystemScript : MonoBehaviour {
        private TagHandle[] _tagHandles;
        public ActiveRagdollCoreScript arCoreScript;
        
        public bool NervesTriggered => _triggered;
        public HurtingObjectScript HurtingScript => _hurtingObjectScript;
        
        private bool _triggered = false;
        private HurtingObjectScript _hurtingObjectScript;
            
        private void Awake() {
            arCoreScript = GetComponent<ActiveRagdollCoreScript>();

            _tagHandles = new [] {
                TagHandle.GetExistingTag("Item"),
            };
            
            if (arCoreScript) {
                foreach (var bone in arCoreScript.boneMaps) {
                    if (!bone.collider) continue;
                    NerveScript script = bone.collider.gameObject.AddComponent<NerveScript>();
                    script.nervousSystemHostScript = this;
                    script.tags = _tagHandles;
                }
            }
        }

        public void TriggerNerves(HurtingObjectScript hurtScript) {
            _triggered = true;
            _hurtingObjectScript = hurtScript;
        }
        
        public void ResetNerves() {
            _triggered = false;
            _hurtingObjectScript = null;
        }
    }
}
