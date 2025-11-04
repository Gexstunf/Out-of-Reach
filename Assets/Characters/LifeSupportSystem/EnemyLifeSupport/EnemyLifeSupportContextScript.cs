using Characters.ActiveRagdollSystem;
using Characters.Enemies.Scripts;
using UnityEngine;

namespace Characters.LifeSupportSystem.EnemyLifeSupport {
    public class EnemyLifeSupportContextScript {
        [SerializeField] private readonly float _maxHealth;
        [SerializeField] private float _currentHealth;
        [SerializeField] private ActiveRagdollCoreScript _arCoreScript;
        [SerializeField] private NervousSystemScript _nervousSystemScript;
        [SerializeField] private AttackScript _attackScript;
        [SerializeField] private TargetingScript _targetingScript;



        public EnemyLifeSupportContextScript(float maxHealth, ActiveRagdollCoreScript arCoreScript, NervousSystemScript nervousSystemScript, TargetingScript targetingScript, AttackScript attackScript) 
        {
            _maxHealth = maxHealth;
            _arCoreScript = arCoreScript;
            _nervousSystemScript = nervousSystemScript;
            _targetingScript = targetingScript;
            _attackScript = attackScript;
        }
        
        public ActiveRagdollCoreScript ArCoreScript => _arCoreScript;
        public NervousSystemScript NervousSystemScript => _nervousSystemScript;
        public AttackScript AttackScript => _attackScript;
        public TargetingScript TargetingScript => _targetingScript;
        
        public float MaxHealth => _maxHealth;
        public float Health => _currentHealth;
        
        public void SetHealth(float value) {
            _currentHealth = value;
        }

        public void DisableLivingFunctionalities() {
            _attackScript.enabled = false;
            _targetingScript.enabled = false;
        }
    }
}