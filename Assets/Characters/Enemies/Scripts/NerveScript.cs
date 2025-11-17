using System;
using Items.Scripts;
using UnityEngine;

namespace Characters.Enemies.Scripts {
    public class NerveScript : NervousSystemScript {
        public NervousSystemScript nervousSystemHostScript;
        
        // Update is called once per frame
        private void OnCollisionEnter(Collision other) {
            if (CollidedWithTags(other)) {
                var script = GetHurtingObject(other.gameObject);
                if (!script) return;
                
                nervousSystemHostScript.TriggerNerves(script);
            }
        }

        private bool CollidedWithTags(Collision other) {
            foreach (var t in TagHandles) {
                if (other.gameObject.CompareTag(t)) {
                    return true;
                }
            }
            return false;
        }

        private static HurtingObjectScript GetHurtingObject(GameObject obj) {
            var script = obj.GetComponent<HurtingObjectScript>();
            return script;
        }
    }
}
