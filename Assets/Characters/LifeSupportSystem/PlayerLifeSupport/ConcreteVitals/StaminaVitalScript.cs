

using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using UnityEngine;
using UnityEngine.XR;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals {
    public class StaminaVitalScript : PlayerVitalScript {
        
        public StaminaVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) :
            base(context, vital)
        { }
        
        // base/setup variables  and optional values
        private float _baseStaminaRegenDelay = 5f;
        private float _baseStaminaRegenRate = 2f;
        private float _baseStaminaUseRate = 5f;
        private float _jumpStaminaUse = 15f;
        private bool _hasJumped = false;

        // dynamic delays
        private float _currentStaminaRegenDelay;
        
        // dynamic rates
        private float _currentStaminaUseRate;
        private float _currentStaminaRegenRate;
        
        // counters
        private float _regenCounter = 0f;

        
        private float _stamina;
        public bool IsTired { get; private set; }
        

        #region Modifier vars
        private readonly float _climbingModifier = 4f;
        private readonly float _runningModifier = 2f;
        private readonly float _jumpingModifier = 5f;
        private readonly float _tiredModifier = 3f;
        #endregion
        
        
        #region Main functions
        
        public override void SetupVital() {
            // setting up the base variables
            _stamina = Context.MaxStamina;
            _baseStaminaRegenDelay = Context.StaminaRegenDelay;
            _baseStaminaRegenRate = Context.StaminaRegenRate;
            _baseStaminaUseRate = Context.StaminaUseRate;
            _jumpStaminaUse = Context.JumpStaminaUse; 
            
            
            // using the base values
            _currentStaminaRegenDelay = _baseStaminaRegenDelay;
            _currentStaminaUseRate = _baseStaminaUseRate;
            _currentStaminaRegenRate = _baseStaminaRegenRate;
        }
        public override void UpdateModifiers() {
            //update stamina use rate based on the state
            _currentStaminaUseRate = _baseStaminaUseRate;
            
            if (Context.IsJumping) {
                _currentStaminaUseRate = _baseStaminaUseRate + _jumpingModifier;
            }

            if (Context.IsRunning) {
                _currentStaminaUseRate = _baseStaminaUseRate + _runningModifier;
            }
            
            if (Context.IsClimbing) {
                _currentStaminaUseRate = _baseStaminaUseRate + _climbingModifier;
            }
            
            if (IsTired) {
                
            }

        }
        public override void UpdateVital() {
            
            HandleWhenMovement(); // this handles the stamina logic when moving
            
            if (_regenCounter < 0f) {
                RegenStamina(_currentStaminaRegenRate);
            }
            
            DecreaseCounters();
            Context.UIManager.DisplayStamina(_stamina);
        }
        
        #endregion
        
        
        #region Logic
        
        private void HandleWhenMovement() {
            // checks if the movement should use stamina (checks if current state is a moving one)
            if (!Context.IsStaminaRequired()) return;
            
            if (HandleJump()) { // this handles jump stamina logic
                return;
            }
            
            UseStamina(_currentStaminaUseRate);
            Context.ClampVital(ref _stamina, Context.MaxStamina);
            _regenCounter = _currentStaminaRegenDelay;
        }
        private void UseStamina(float rate) {
            // if no stamina, make the regen delay greater
            if (_stamina <= 0f) {
                _currentStaminaRegenDelay += Time.deltaTime;
                IsTired = true;
                return;
            }
            
            // ensure regen delay is normal and subtract stamina
            _currentStaminaRegenDelay = _baseStaminaRegenDelay;
            IsTired = false;
            _stamina -= rate * Time.deltaTime;
        }
        private void RegenStamina(float rate) {
            if (_stamina < Context.MaxStamina) {
                _stamina += rate * Time.deltaTime;
            }
            Context.ClampVital(ref _stamina, Context.MaxStamina);
        }
        private bool HandleJump() {
            // only use stamina when you start a new jump (press jump), not <while> jumping
            if (Context.IsJumping && !_hasJumped) {
                _stamina -= _jumpStaminaUse;
                _hasJumped = true;
                Context.ClampVital(ref _stamina, Context.MaxStamina);
                return true;
            } 
            // the rest checks if stopped jumping, to restart the jump logic
            if (Context.IsJumping) {
                return true;
            } 
            
            _hasJumped = false;
            return false;
        }
        
        #endregion
        
        private void DecreaseCounters() {
            _regenCounter -= Time.deltaTime;
        }
    }
}






