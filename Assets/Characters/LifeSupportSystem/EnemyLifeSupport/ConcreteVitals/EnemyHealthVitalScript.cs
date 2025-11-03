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
                _wasUnconscious = true;
            }
            else if (_wasUnconscious) {
                Context.ArCoreScript.Revive();
                _wasUnconscious = false;
            }

            foreach (var bone in Context.ArCoreScript.boneMaps) {
                /*
                if (bone.collider has collided with a gameobject with tag "item") {
                    HurtingObjectScript hurtScript = other.gameObject.GetComponent<HurtingObjectScript>();
                    if (!hurtScript) {
                        Debug.Log("(enemy) I WAS HIT!");
                        DamageLife(hurtScript.damage);
                    }
                } 
                */
            }
        }

        public override void SetupVital() {
            _health = Context.MaxHealth;
        }

        public override void UpdateModifiers() {
            //throw new System.NotImplementedException();
        }

        public override void OnCollisionEnter(Collision other) {
            if (other.gameObject.CompareTag("Item")) {
                HurtingObjectScript hurtScript = other.gameObject.GetComponent<HurtingObjectScript>();
                if (!hurtScript) {
                    Debug.Log("(enemy) I WAS HIT!");
                    DamageLife(hurtScript.damage);
                }
            }
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
