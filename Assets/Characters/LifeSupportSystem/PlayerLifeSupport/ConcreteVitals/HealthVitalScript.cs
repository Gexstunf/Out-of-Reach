

using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals {
    public class HealthVitalScript : PlayerVitalScript
    {
        public HealthVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) : 
            base(context, vital)
        { }

        public override void SetupVital() {
            
        }
        public override void UpdateVital() {
            // Debug.Log("4");
        }
        public override void UpdateModifiers() {}
    }
}
