

using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals {
    public class StaminaVitalScript : PlayerVitalScript {
        
        public StaminaVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) :
            base(context, vital)
        { }
        
        private float _regenRate = 5f;
        private float _regenDelay = 5f;
        private float _regenCounter = 0f;

        private float _stamina;
        private float _useRate = 5f;
        
        private float _jumpingStaminaDelay = 1f;
        private float _jumpingCounter = 0f;
        
        // helper vars
        
        public override void SetupVital() {
            _stamina = Context.MaxStamina;
        }
        public override void UpdateVital() {
            if (Context.IsRunning || Context.IsClimbing || Context.IsJumping) {
                if (Context.IsJumping && _jumpingCounter < 0f) {
                    UseStamina(_useRate);
                    _jumpingCounter = _jumpingStaminaDelay;
                }
                
                UseStamina(_useRate);
                _regenCounter = _regenDelay;
            }

            if (_regenCounter < 0f) {
                RegenStamina(_regenRate);
            }
            
            _regenCounter -= Time.deltaTime;
            _jumpingCounter -= Time.deltaTime;
            Context.UIManager.DisplayStamina(_stamina);
        }

        public override void UpdateModifiers() {
            if (Context.IsJumping) {
                _useRate = 10f;
            }
        }
        
        private void UseStamina(float amount) {
            _stamina -= amount * Time.deltaTime;
        }
        
        private void RegenStamina(float amount) {
            if (_stamina < Context.MaxStamina) {
                _stamina += amount * Time.deltaTime;
            }
        }
    }
}






