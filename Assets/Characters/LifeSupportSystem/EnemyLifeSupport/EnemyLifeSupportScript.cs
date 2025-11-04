using Characters.ActiveRagdollSystem;
using Characters.Enemies.Scripts;
using Characters.LifeSupportSystem.EnemyLifeSupport.ConcreteVitals;
using UnityEngine;

namespace Characters.LifeSupportSystem.EnemyLifeSupport {
    public class EnemyLifeSupportScript : LifeSupportManagerScript<EnemyLifeSupportScript.EVitals> {

        [Header("Life support settings")] 
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private ActiveRagdollCoreScript _arCoreScript;
        [SerializeField] private NervousSystemScript _nervousSystemScript;
        [SerializeField] private AttackScript _attackScript;
        [SerializeField] private TargetingScript _targetingScript;
        
        public EnemyLifeSupportContextScript Context { get; private set; }

        public enum EVitals
        {
            Health,
        }
        
        private void Awake() {
            _arCoreScript = GetComponent<ActiveRagdollCoreScript>();
            _nervousSystemScript = GetComponent<NervousSystemScript>();
            _attackScript = GetComponent<AttackScript>();
            _targetingScript = GetComponent<TargetingScript>();
            
            Context = new EnemyLifeSupportContextScript(_maxHealth, _arCoreScript, _nervousSystemScript, _targetingScript, _attackScript);
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
            
            if (!_nervousSystemScript) {
                Debug.Log("[EnemyLifeSupportScript] No NervousSystemScript found!");
            }
        }
    }
}
