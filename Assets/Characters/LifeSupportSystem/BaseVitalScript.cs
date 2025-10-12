using System;
using UnityEngine;

namespace Characters.LifeSupportSystem {
    public abstract class BaseVitalScript<EVitals>  where EVitals : Enum
    {
        public BaseVitalScript(EVitals vital) {
            VitalKey = vital;
        }
        
        public EVitals VitalKey { get; private set; }

        public abstract void UpdateVital();
        public abstract void SetupVital();
        public abstract void UpdateModifiers();
        
        public abstract void OnCollisionEnter(Collision other);

        public abstract void OnTriggerEnter(Collider other);
        public abstract void OnTriggerExit(Collider other);
        public abstract void OnTriggerStay(Collider other);
    }
}
