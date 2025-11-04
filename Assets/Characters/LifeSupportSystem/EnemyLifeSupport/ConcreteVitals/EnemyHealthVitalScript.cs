using Characters.Enemies.Scripts;
using Characters.LifeSupportSystem.PlayerLifeSupport;
using Items.Scripts;
using UnityEngine;

namespace Characters.LifeSupportSystem.EnemyLifeSupport.ConcreteVitals {
    public class EnemyHealthVitalScript : EnemyVitalScript
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public EnemyHealthVitalScript(EnemyLifeSupportContextScript context, EnemyLifeSupportScript.EVitals vital) : base(context, vital) { }

        private float _health;
        private bool IsUnconscious => _health <= 0f;

        private bool _wasUnconscious;

        public override void UpdateVital() {
            Context.SetHealth(_health);

            if (IsUnconscious) {
                Context.ArCoreScript.Kill();
                Context.DisableLivingFunctionalities();
                _wasUnconscious = true;
            }
            else if (_wasUnconscious) {
                Context.ArCoreScript.Revive();
                _wasUnconscious = false;
            }

            if (Context.NervousSystemScript.NervesTriggered) {
                var script = Context.NervousSystemScript.HurtingScript;
                if (script) {
                    Debug.Log("(enemy) I WAS HIT!");
                    DamageLife(script.damage);
                    Context.NervousSystemScript.ResetNerves();
                }
            }
        }

        public override void SetupVital() {
            _health = Context.MaxHealth;
        }

        public override void UpdateModifiers() {
            //throw new System.NotImplementedException();
        }

        public override void OnCollisionEnter(Collision other) {

        }

        public override void OnTriggerEnter(Collider other) {
            //throw new System.NotImplementedException();
        }

        public override void OnTriggerExit(Collider other) {
            //throw new System.NotImplementedException();
        }

        public override void OnTriggerStay(Collider other) {
            //throw new System.NotImplementedException();
        }
        
        private void DamageLife(float amount)
        {
            _health -= amount;
            ClampVital(ref _health, Context.MaxHealth);
        }
    }
}
