using Characters.ActiveRagdollSystem;
using UnityEngine;

namespace Characters.LifeSupportSystem.EnemyLifeSupport {
    public class EnemyLifeSupportContextScript {
        [SerializeField] private readonly float _maxHealth;
        [SerializeField] private float _currentHealth;
        [SerializeField] private ActiveRagdollCoreScript _arCoreScript;

        public EnemyLifeSupportContextScript(float maxHealth, ActiveRagdollCoreScript arCoreScript) 
        {
            _maxHealth = maxHealth;
            _arCoreScript = arCoreScript;
        }
        
        public ActiveRagdollCoreScript ArCoreScript => _arCoreScript;
        
        public float MaxHealth => _maxHealth;
        public float Health => _currentHealth;
        
        public void SetHealth(float value) {
            _currentHealth = value;
        }
    }
}