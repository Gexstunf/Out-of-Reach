using UnityEngine;

namespace Characters.LifeSupportSystem.EnemyLifeSupport {
    public abstract class EnemyVitalScript : BaseVitalScript<EnemyLifeSupportScript.EVitals>
    {
        protected EnemyLifeSupportContextScript Context;

        public EnemyVitalScript(EnemyLifeSupportContextScript context, EnemyLifeSupportScript.EVitals vital) : base(vital) {
            Context = context;
        }
        
        public void ClampVital(ref float value, float max) => value = Mathf.Clamp(value, 0, max);
    }
}
