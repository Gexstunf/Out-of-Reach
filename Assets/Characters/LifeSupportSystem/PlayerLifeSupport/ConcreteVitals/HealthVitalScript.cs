using Characters.Enemies.Scripts;
using Characters.LifeSupportSystem.PlayerLifeSupport.Utils;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals
{
    public class HealthVitalScript : PlayerVitalScript
    {

        public HealthVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) :
            base(context, vital)
        { }

        private float _health;
        public bool IsUnconscious => _health <= 0f;
        public VitalUtils VitalUtil { get; private set; }

        private float _healthDamageMultiplier = 10f;
        private float _currentHealthRegenDelay = 0f;

        public override void SetupVital()
        {
            VitalUtil = new VitalUtils(Context.HealthRegenRate, Context.HealthRegenDelay,
                0f, 2f, 0f, Context.MaxHealth);

            _health = VitalUtil.BaseMaxVital;
            _currentHealthRegenDelay = VitalUtil.BaseRegenDelay;
            Debug.Log("Health setup: " + _health);
        }

        public override void UpdateModifiers()
        {
            if (Context.IsFalling)
            {
                VitalUtil.IncreaseTimer();
            }
            else if (VitalUtil.Timer < VitalUtil.MinimumTime)
            {
                VitalUtil.SetTimer(0f);
            }

            // Actualizamos solo si cambia
            Context.SetUnconscious(IsUnconscious);
        }

        public override void OnCollisionEnter(Collision other) {
            if (other.gameObject.CompareTag("Enemy")) {
                ILimbDamageScript limbScript = other.gameObject.GetComponent<ILimbDamageScript>();
                if (limbScript != null && limbScript.HostAttackScript && limbScript.HostAttackScript.enabled) {
                    AttackDamageSO damageSO = limbScript.HostAttackScript.currentAttackSO;
                    Debug.Log("WAS HIT!");
                    DamageLife(damageSO.damage);
                }
            }
        }

        public override void OnTriggerEnter(Collider other) {

        }

        public override void OnTriggerExit(Collider other) {
            //throw new System.NotImplementedException();
        }

        public override void OnTriggerStay(Collider other) {
            //throw new System.NotImplementedException();
        }

        public override void UpdateVital() {
            
            if (VitalUtil.RegenTimer < 0f) {
                //RegenerateLife(VitalUtil.BaseRegenRate);
            }

            if (!Context.IsFalling && VitalUtil.Timer > VitalUtil.MinimumTime)
            {
                DamageLife(VitalUtil.Timer * _healthDamageMultiplier);
                VitalUtil.SetTimer(0f);
                VitalUtil.SetRegenTimer(_currentHealthRegenDelay);
                Debug.Log("Applied damage!");
            }

            VitalUtil.DecreaseRegenTimer();
            Context.SetHealth(_health);
            Context.UIManager.DisplayHealth(_health);
        }

        private void DamageLife(float amount)
        {
            _health -= amount;
            VitalUtil.ClampVital(ref _health);
        }

        private void RegenerateLife(float rate)
        {
            if (_health < VitalUtil.BaseMaxVital && VitalUtil.RegenTimer < 0f)
            {
                _health += rate * Time.deltaTime;
                VitalUtil.ClampVital(ref _health);

                // Si revive, actualizamos el estado
                if (_health > 0f && Context.IsUnconscious)
                {
                    Context.SetUnconscious(false);
                }
            }
        }
    }
}
