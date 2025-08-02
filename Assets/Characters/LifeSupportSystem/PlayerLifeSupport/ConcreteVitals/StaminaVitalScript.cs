

using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals {
    public class StaminaVitalScript : PlayerVitalScript {
        
        public StaminaVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) :
            base(context, vital)
        { }
        
        private float _regenRate = 5f;
        private float _regenDelay = 5f;
        private float _stamina;
        private float _useRate = 5f;
        
        private PlayerStateMachineScript _stateMachine;
        
        // helper vars
        
        public override void SetupVital() {
            _stamina = Context.MaxStamina;
            _stateMachine = Context.StateMachine;
        }
        public override void UpdateVital() {
            if (_stateMachine.IsWalking) {
                UseStamina(_useRate);
                Context.UIManager.DisplayStamina(_stamina);
            }
        }

        public override void UpdateModifiers() {
        }
        
        private void UseStamina(float amount) {
            _stamina -= amount * Time.deltaTime;
        }
    }
}






