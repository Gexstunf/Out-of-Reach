

using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals {
    public class WeightVitalScript : PlayerVitalScript
    {
        public WeightVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) : base(context, vital)
        { }


        public override void SetupVital() {
            
        }
        
        public override void UpdateVital() {
            Debug.Log("1");
        }
        public override void UpdateModifiers() {}
    }
}


