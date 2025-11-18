using Characters.LifeSupportSystem.PlayerLifeSupport.Utils;
using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals
{
    public class StaminaVitalScript : PlayerVitalScript
    {

        public StaminaVitalScript(PlayerLifeSupportContextScript context, PlayerLifeSupportScript.EVitals vital) :
            base(context, vital)
        { }

        private VitalUtils VitalUtil { get; set; }

        private bool _hasJumped = false;

        private float _currentStaminaUseRate;
        private float _currentStaminaRegenRate;

        private float _stamina;
        public bool HasStamina => _stamina > 0f;

        #region Modifiers
        private readonly float _climbingModifier = 4f;
        private readonly float _runningModifier = 2f;
        private readonly float _jumpingModifier = 5f;
        #endregion

        public override void SetupVital()
        {
            VitalUtil = new VitalUtils(Context.StaminaRegenRate, Context.StaminaRegenDelay,
                Context.StaminaUseRate, 2f, Context.JumpStaminaUse, Context.MaxStamina);

            _stamina = Context.MaxStamina;
            _currentStaminaUseRate = VitalUtil.BaseUseRate;
            _currentStaminaRegenRate = VitalUtil.BaseRegenRate;
            //Debug.Log("Stamina setup: " + _stamina);
        }

        public override void UpdateModifiers()
        {
            _currentStaminaUseRate = VitalUtil.BaseUseRate;

            if (Context.IsJumping) _currentStaminaUseRate += _jumpingModifier;
            if (Context.IsRunning) _currentStaminaUseRate += _runningModifier;
            if (Context.IsClimbing) _currentStaminaUseRate += _climbingModifier;

            // Solo actualizamos el Context, el Coordinator decidir� si dispara evento
            Context.SetTired(!HasStamina);
        }

        public override void OnCollisionEnter(Collision other) {
        }

        public override void OnTriggerEnter(Collider other) {
        }

        public override void OnTriggerExit(Collider other) {
        }

        public override void OnTriggerStay(Collider other) {
        }

        public override void UpdateVital()
        {
            Context.UIManager.DisplayStamina(_stamina);

            if (HandleJump()) return;

            UseStamina(_currentStaminaUseRate);

            if (VitalUtil.RegenTimer < 0f)
            {
                RegenStamina(_currentStaminaRegenRate);
            }

            VitalUtil.DecreaseRegenTimer();
            Context.SetStamina(_stamina);
        }

        #region Logic
        private void UseStamina(float rate)
        {
            if (!Context.IsStaminaRequired()) return;

            if (_stamina <= 0f)
            {
                VitalUtil.IncreaseRegenTimer();
                return;
            }

            _stamina -= rate * Time.deltaTime;
            VitalUtil.SetRegenTimer(VitalUtil.BaseRegenDelay);
            VitalUtil.ClampVital(ref _stamina);
        }

        private void RegenStamina(float rate)
        {
            if (_stamina < VitalUtil.BaseMaxVital)
            {
                _stamina += rate * Time.deltaTime;
            }
            VitalUtil.ClampVital(ref _stamina);
        }

        private bool HandleJump()
        {
            if (Context.IsJumping && !_hasJumped)
            {
                _stamina -= VitalUtil.BaseUseCost;
                _hasJumped = true;
                VitalUtil.ClampVital(ref _stamina);
                return true;
            }
            if (Context.IsJumping)
            {
                VitalUtil.SetRegenTimer(VitalUtil.BaseRegenDelay);
                return true;
            }

            _hasJumped = false;
            return false;
        }
        #endregion
    }
}
