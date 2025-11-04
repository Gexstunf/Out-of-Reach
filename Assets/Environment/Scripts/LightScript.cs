using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;

namespace Environment.Scripts {
    public class LightScript : MonoBehaviour {
        [Header("References")] 
        public GameObject lightObject;
        private Light _lightComp;

        [Header("Settings")] 
        public float blinkTime = 2f;
        public float minTime = 0.2f;
        public float maxTime = 2.25f;
        public float minIntensity = 0.6f;
        public float maxIntensity = 50f;
        [Range(0,1)] public float failureChance = 0.5f;
        [Range(0,1)] public float selfDestructChance = 0.15f;

        private ELightMode _lightMode;
        private bool _isOff;
        private IEnumerator _currentRoutine;

        private float _normalIntensity;
        
        public enum ELightMode {
            On,       
            Intermittent,
            Blinking,
            Off,  
            Discolored,
            Bright,
            Dim,
        }

        private void Start() {
            _lightComp = lightObject.GetComponent<Light>();
            _normalIntensity = _lightComp.intensity;
            if (UnityEngine.Random.value < failureChance) {
                // pick a random failure mode, but not "On"
                _lightMode = (ELightMode)UnityEngine.Random.Range(1, Enum.GetValues(typeof(ELightMode)).Length);
            } else {
                _lightMode = ELightMode.On;
            }

            if (UnityEngine.Random.value < selfDestructChance) {
                Destroy(gameObject);
            }
        }

        private void Update() {
            if (_currentRoutine != null) return;
            
            switch (_lightMode) {
                case ELightMode.On:
                    break;
                case ELightMode.Intermittent:
                    StartLightRoutine(SwitchLightAfterTime(_isOff));
                    break;
                case ELightMode.Blinking:
                    StartLightRoutine(BlinkLight(_isOff));
                    break;
                case ELightMode.Off:
                    if (!_isOff) {
                        TurnOff();
                    }
                    break;
                case ELightMode.Discolored:
                    SetLightColor(Color.red);
                    break;
                case ELightMode.Bright:
                    SetLightIntensity(maxIntensity);
                    break;
                case ELightMode.Dim:
                    SetLightIntensity(minIntensity);
                    break;
                default:
                    break;
            }
        }

        private void StartLightRoutine(IEnumerator routine) {
                _currentRoutine = routine;
                StartCoroutine(routine);
        }

        private IEnumerator SwitchLightAfterTime(bool isOff) {
            float randomDelay = UnityEngine.Random.Range(minTime, maxTime);
            yield return StartCoroutine(SetLightIntensityAndWait(isOff, randomDelay, _normalIntensity));
            _currentRoutine = null;
            _isOff = !isOff;
        }
        
        private IEnumerator BlinkLight(bool isOff) {
            yield return StartCoroutine(SetLightIntensityAndWait(isOff, blinkTime, _normalIntensity));
            _currentRoutine = null;
            _isOff = !isOff;
        }

        private IEnumerator SetLightIntensityAndWait(bool isOff, float waitTime, float intensity) {
            _lightComp.intensity = isOff ? intensity : 0f;
            yield return new WaitForSeconds(waitTime);
        }

        private void SetLightIntensity(float intensity) {
            _lightComp.intensity = intensity;
        }
        
        private void SetLightColor(Color color) {
            _lightComp.color = color;
        }

        public void ChangeLightMode(ELightMode newMode) {
            _lightMode = newMode;
            _isOff = false;
        }

        public void TurnOff() {
            _lightComp.intensity = 0f;
            _isOff = true;
        }
        
        public void TurnOn() {
            _lightComp.intensity = _normalIntensity;
            _isOff = false;
        }
    }
}