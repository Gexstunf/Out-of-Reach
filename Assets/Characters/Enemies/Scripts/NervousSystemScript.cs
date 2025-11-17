using System;
using System.Linq;
using Characters.ActiveRagdollSystem;
using Items.Scripts;
using Unity.VisualScripting;
using UnityEngine;

namespace Characters.Enemies.Scripts {
    public class NervousSystemScript : MonoBehaviour {
        public TagHandle[] TagHandles;
        [SerializeField] private TagClass[] _tags;
        [SerializeField] private ActiveRagdollCoreScript arCoreScript;
        
        public bool NervesTriggered => _triggered;
        public HurtingObjectScript HurtingScript => _hurtingObjectScript;
        
        private bool _triggered = false;
        private HurtingObjectScript _hurtingObjectScript;
        
        [System.Serializable]
        public class TagClass {
            public string TagString = "Item";
        }
            
        private void Awake() {
            arCoreScript = GetComponent<ActiveRagdollCoreScript>();
        }

        private void Start() {
            
            TagHandles = new TagHandle[_tags.Length];
            for (int i = 0; i < _tags.Length; i++) {
                TagHandles[i] = TagHandle.GetExistingTag(_tags[i].TagString);
                Debug.Log( "Nervous system tag: " + TagHandles[i]);
            }

            if (!arCoreScript) return;
            foreach (var bone in arCoreScript.boneMaps) {
                if (!bone.collider && bone.collider.gameObject) continue;
                NerveScript script = bone.collider.gameObject.AddComponent<NerveScript>();
                script.nervousSystemHostScript = this;
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
