using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals {
    public class HungerVitalScript : PlayerVitalScript
    {
        public HungerVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) : base(context, vital)
        { }
        
        public override void SetupVital() {
            
        }
        public override void UpdateVital() {
            Debug.Log("3");
        }
        public override void UpdateModifiers() {}
    }
}