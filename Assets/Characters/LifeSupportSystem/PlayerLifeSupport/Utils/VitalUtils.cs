using UnityEngine;

namespace Characters.LifeSupportSystem.PlayerLifeSupport.Utils {
    public class VitalUtils
    {
        public float BaseRegenRate { get; private set; }
        public float BaseRegenDelay { get; private set; }
        public float BaseUseRate { get; private set; }
        public float BaseUseCost {get; private set;}
        
        public float BaseMaxVital { get; private set; }
        
        public float RegenTimer { get; private set; }
        public float Timer { get; private set; }
        public float MinimumTime { get; private set; }
        
        public VitalUtils(float regenRate, float regenDelay, float baseUseRate, float minimumTime, float baseUseCost, float baseMaxVital) {
            BaseRegenDelay = regenDelay;
            BaseRegenRate = regenRate;
            BaseUseRate = baseUseRate;
            MinimumTime = minimumTime;
            BaseUseCost = baseUseCost;
            BaseMaxVital = baseMaxVital;
        }
        
        
        #region Helper functions
        public void ClampVital(ref float value) => value = Mathf.Clamp(value, 0, BaseMaxVital);
        
        public void DecreaseRegenTimer() {
            RegenTimer -= Time.deltaTime;
        }

        public void IncreaseRegenTimer() {
            RegenTimer += Time.deltaTime;
        }
        public void SetRegenTimer(float time) {
            RegenTimer = time;
        }

        public void IncreaseTimer() {
            Timer += Time.deltaTime;
        }
        
        public void DecreaseTimer() {
            Timer -= Time.deltaTime;
        }

        public void SetTimer(float time) {
            Timer = time;
        }
        #endregion
    }
}
