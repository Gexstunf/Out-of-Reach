using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Characters.LifeSupportSystem {
    public abstract class LifeSupportManagerScript<EVitals> : MonoBehaviourPun where EVitals : Enum {
        
        public Dictionary<EVitals, BaseVitalScript<EVitals>> Vitals = new Dictionary<EVitals, BaseVitalScript<EVitals>>();

        public void Start() {
            foreach (var vital in Vitals) {
                BaseVitalScript<EVitals> currentVital = vital.Value;
                currentVital.SetupVital();
            }
        }

        public void Update() {
            foreach (var vital in Vitals) {
                BaseVitalScript<EVitals> currentVital = vital.Value;
                currentVital.UpdateModifiers();
                currentVital.UpdateVital();
            }
        }
    }
}
