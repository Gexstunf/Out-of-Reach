

using Characters.LifeSupportSystem.PlayerLifeSupport.Utils;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals {
    public class HealthVitalScript : PlayerVitalScript {
        
        public HealthVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) : 
            base(context, vital)
        { }

        public bool IsUnconscious => _health <= 0f;
        private float _health;
        public VitalUtils VitalUtil { get; private set; }

        private float _healthDamageMultiplier = 10f;
        private float _currentHealthRegenDelay = 0f;
        
        public override void SetupVital() {
            VitalUtil = new VitalUtils(Context.HealthRegenRate, Context.HealthRegenDelay, 
                0f, 2f, 0f, Context.MaxHealth);
            
            _health = VitalUtil.BaseMaxVital;
            _currentHealthRegenDelay = VitalUtil.BaseRegenDelay;
        }

        public override void UpdateModifiers() {
            // if falling, track the fall time
            if (Context.IsFalling) {
                VitalUtil.IncreaseTimer();
            } 
            else if (VitalUtil.Timer < VitalUtil.MinimumTime) { // if not, check if it is not the minimum and reset the counter
                VitalUtil.SetTimer(0f);
            }
            
            if (IsUnconscious) Context.SetUnconscious(IsUnconscious);
        }
        
        public override void UpdateVital() {
            
            if (VitalUtil.RegenTimer < 0f) {
                RegenerateLife(VitalUtil.BaseRegenRate);
            }

            if (!Context.IsFalling && VitalUtil.Timer > VitalUtil.MinimumTime) {
                DamageLife(VitalUtil.Timer * _healthDamageMultiplier);
                VitalUtil.SetTimer(0f);
                VitalUtil.SetRegenTimer(_currentHealthRegenDelay);
                Debug.Log("Applied damage!");
            }
            
            VitalUtil.DecreaseRegenTimer();
            Context.SetHealth(_health);
            Context.UIManager.DisplayHealth(_health);
        }

        private void DamageLife(float amount) {
            _health -= amount;
            VitalUtil.ClampVital(ref _health);
        }

        private void RegenerateLife(float rate) {
            if (_health < VitalUtil.BaseMaxVital && VitalUtil.RegenTimer < 0f) {
                _health += rate * Time.deltaTime;
                Context.SetUnconscious(false);
            }
        }
    }
}
