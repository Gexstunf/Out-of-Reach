using Characters.ActiveRagdollSystem;
using Characters.LifeSupportSystem.EnemyLifeSupport.ConcreteVitals;
using UnityEngine;

namespace Characters.LifeSupportSystem.EnemyLifeSupport {
    public class EnemyLifeSupportScript : LifeSupportManagerScript<EnemyLifeSupportScript.EVitals> {

        [Header("Life support settings")] 
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private ActiveRagdollCoreScript _arCoreScript;
        
        
        public EnemyLifeSupportContextScript Context { get; private set; }

        public enum EVitals
        {
            Health,
        }
        
        private void Awake() {
            _arCoreScript = GetComponent<ActiveRagdollCoreScript>();
            Context = new EnemyLifeSupportContextScript(_maxHealth, _arCoreScript);
            ValidateReferences();
            InitializeVitals();
        }
        
        private void InitializeVitals()
        {
            // El orden importa: algunos dependen de otros
            Vitals.Add(EVitals.Health, new EnemyHealthVitalScript(Context, EVitals.Health));
        }

        private void ValidateReferences()
        {
            if (!_arCoreScript) {
                Debug.Log("[EnemyLifeSupportScript] No ActiveRagdollCoreScript found!");
            }
        }
    }
}
