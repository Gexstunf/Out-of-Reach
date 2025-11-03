using System;
using Characters.ActiveRagdollSystem;
using UnityEngine;

namespace Characters.Enemies.Scripts {
    public class NervousSystemScript : MonoBehaviour
    {
        public ActiveRagdollCoreScript arCoreScript;
            
        private void Awake() {
            arCoreScript = GetComponent<ActiveRagdollCoreScript>();
            if (arCoreScript) {
                foreach (var bone in arCoreScript.boneMaps) {
                    NerveScript script = bone.collider.gameObject.AddComponent<NerveScript>();
                }
            }
        }
    }
}
