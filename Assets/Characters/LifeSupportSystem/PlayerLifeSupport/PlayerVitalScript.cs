

using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport {
    
    public abstract class PlayerVitalScript : BaseVitalScript<PlayerLifeSupportScript.EVitals>
    {
        protected PlayerLifeSupportContextScript Context;

        public PlayerVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) : base(vital) {
            Context = context;
        }
        
        //public void ClampVital(ref float value, float max) => value = Mathf.Clamp(value, 0, max);
    }
}
