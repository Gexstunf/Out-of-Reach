

using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals {
    public class HealthVitalScript : PlayerVitalScript {
        
        public HealthVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) : 
            base(context, vital)
        { }

        public bool IsUnconscious { get; private set; }
        private float _health;

        private float _baseHealthRegenDelay;
        private float _healthRegenCounter = 0f;
        
        private float _baseHealthRegenRate;

        public override void SetupVital() {
            _health = Context.MaxHealth;
            _baseHealthRegenDelay = Context.HealthRegenDelay;
            _baseHealthRegenRate = Context.HealthRegenRate;
        }

        public override void UpdateVital() {
            if (_healthRegenCounter < 0f) {
                RegenerateLife(_baseHealthRegenRate);
            }

            DecreaseCounters();
        }
        public override void UpdateModifiers() { }

        private void DamageLife(float amount) {
            if (_health < 0f) {
                IsUnconscious = true;
            }
            
            IsUnconscious = false;
            _health -= amount;
        }

        private void DecreaseCounters() {
            _healthRegenCounter -= Time.deltaTime;
        }

        private void RegenerateLife(float rate) {
            _health += rate * Time.deltaTime;
            IsUnconscious = false;
        }
    }
}
