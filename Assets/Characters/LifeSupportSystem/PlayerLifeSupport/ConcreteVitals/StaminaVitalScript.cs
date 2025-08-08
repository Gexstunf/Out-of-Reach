

using Characters.LifeSupportSystem.PlayerLifeSupport.Utils;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals {
    public class StaminaVitalScript : PlayerVitalScript {
        
        public StaminaVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) :
            base(context, vital)
        { }
        
        private VitalUtils VitalUtil { get; set; }
        
        // base/setup variables  and optional values
        private bool _hasJumped = false;
        
        // dynamic rates
        private float _currentStaminaUseRate;
        private float _currentStaminaRegenRate;
        
        private float _stamina;
        public bool HasStamina => _stamina > 0f;
        

        #region Modifier vars
        private readonly float _climbingModifier = 4f;
        private readonly float _runningModifier = 2f;
        private readonly float _jumpingModifier = 5f;
        #endregion
        
        
        #region Main functions
        
        public override void SetupVital() {
            // setting up the base variables
            VitalUtil = new VitalUtils(Context.StaminaRegenRate, Context.StaminaRegenDelay, 
                Context.StaminaUseRate, 2f, Context.JumpStaminaUse, Context.MaxStamina);
            
            _stamina = Context.MaxStamina;
            _currentStaminaUseRate = VitalUtil.BaseUseRate;
            _currentStaminaRegenRate = VitalUtil.BaseRegenRate;
        }
        public override void UpdateModifiers() {
            //update stamina use rate based on the state
            _currentStaminaUseRate = VitalUtil.BaseUseRate;
            
            if (Context.IsJumping) _currentStaminaUseRate = VitalUtil.BaseUseRate + _jumpingModifier;
            
            if (Context.IsRunning) _currentStaminaUseRate = VitalUtil.BaseUseRate + _runningModifier;
            
            if (Context.IsClimbing) _currentStaminaUseRate = VitalUtil.BaseUseRate + _climbingModifier;
            
            if (!HasStamina) {
                Context.SetTired(true);
            }
            else {
                Context.SetTired(false);
            };
        }
        
        public override void UpdateVital() {
            Context.UIManager.DisplayStamina(_stamina);

            if (HandleJump()) { // this handles jump stamina logic
                return;
            }
            
            UseStamina(_currentStaminaUseRate);

            if (VitalUtil.RegenTimer < 0f) {
                RegenStamina(_currentStaminaRegenRate);
            }
            
            VitalUtil.DecreaseRegenTimer();
        }
        
        #endregion
        
        
        #region Logic
        private void UseStamina(float rate) {
            
            if (!Context.IsStaminaRequired()) return;
            
            // if no stamina, make the regen delay greater
            if (_stamina <= 0f) {
                VitalUtil.IncreaseRegenTimer();
                return;
            }
            
            // ensure regen delay is normal and subtract stamina
            _stamina -= rate * Time.deltaTime;
            VitalUtil.SetRegenTimer(VitalUtil.BaseRegenDelay); // reset timer
            VitalUtil.ClampVital(ref _stamina);
        }
        
        private void RegenStamina(float rate) {
            if (_stamina < VitalUtil.BaseMaxVital) {
                _stamina += rate * Time.deltaTime;
                Context.SetTired(false);
            }
            VitalUtil.ClampVital(ref _stamina);
        }
        private bool HandleJump() {
            // only use stamina when you start a new jump (press jump), not <while> jumping
            if (Context.IsJumping && !_hasJumped) {
                _stamina -= VitalUtil.BaseUseCost;
                _hasJumped = true;
                VitalUtil.ClampVital(ref _stamina);
                return true;
            } 
            // the rest checks if stopped jumping, to restart the jump logic
            if (Context.IsJumping) {
                VitalUtil.SetRegenTimer(VitalUtil.BaseRegenDelay); // reset timer
                return true;
            }
            
            _hasJumped = false;
            return false;
        }
        
        #endregion
    }
}






