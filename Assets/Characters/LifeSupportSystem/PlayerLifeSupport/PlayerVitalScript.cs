

namespace Characters.LifeSupportSystem.PlayerLifeSupport {
    
    public abstract class PlayerVitalScript : BaseVitalScript<PlayerLifeSupportScript.EVitals>
    {
        protected PlayerLifeSupportContextScript Context;

        public PlayerVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) : base(vital) {
            Context = context;
        }
    }
}
