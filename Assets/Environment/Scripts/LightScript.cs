using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Serialization;
using Photon.Pun;

namespace Environment.Scripts {
    public class LightScript : MonoBehaviourPun, IPunObservable {
        [Header("References")] 
        public GameObject lightObject;
        public GameObject vsfObject;
        [SerializeField] private AudioSource _lightAudioSource;
        [SerializeField] private AudioSource _failAudioSource;

        [Header("Settings")] public bool debug;
        public bool useSparkEffect = true;

        [Range(0, 1)] public float intensityScaleFactor = 0f;
        public float intensityOffsetMagnitude = 10f;
        public float flickerTime = 0.25f;
        public float blinkTime = 2f;
        public float minTime = 0.2f;
        public float maxTime = 2.25f;
        public float minIntensity = 0.6f;
        public float maxIntensity = 9f;
        [Range(0,1)] public float failureChance = 0.5f;
        [Range(0,1)] public float selfDestructChance = 0.15f;

        private ELightMode _lightMode;
        private bool _isOff;
        private bool _previouslyOn;
        private IEnumerator _flickeringRoutine;
        private IEnumerator _currentRoutine;

        private float _normalIntensity;
        
        private Light _lightComp;
        private ParticleSystem _particleSystem;


        private bool _doorFailed;
        
        public enum ELightMode {
            On,       
            Intermittent,
            Blinking,
            Off,  
            Discolored,
            Bright,
            Dim,
        }

        private void Start()
        {
            _lightComp = lightObject.GetComponent<Light>();
            if (vsfObject) _particleSystem = vsfObject.GetComponent<ParticleSystem>();
            _normalIntensity = _lightComp.intensity;

            // Solo el dueño (host o instancia principal) decide el modo inicial
            if (photonView.IsMine)
            {
                if (UnityEngine.Random.value < failureChance)
                {
                    _lightMode = (ELightMode)UnityEngine.Random.Range(1, Enum.GetValues(typeof(ELightMode)).Length);
                    _doorFailed = true;
                }
                else
                {
                    _lightMode = ELightMode.On;
                }

                if (UnityEngine.Random.value < selfDestructChance)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void Update()
        {
            // Solo el dueño ejecuta la lógica interna
            if (!photonView.IsMine) return;

            if (_currentRoutine != null) return;
            bool shouldTryEffect = false;

            switch (_lightMode)
            {
                case ELightMode.On:
                    break;
                case ELightMode.Intermittent:
                    shouldTryEffect = true;
                    StartLightRoutine(SwitchLightAfterTime(_isOff));
                    break;
                case ELightMode.Blinking:
                    shouldTryEffect = true;
                    StartLightRoutine(BlinkLight(_isOff));
                    break;
                case ELightMode.Off:
                    if (!_isOff) TurnOff();
                    break;
                case ELightMode.Discolored:
                    SetLightColor(Color.red);
                    break;
                case ELightMode.Bright:
                    maxIntensity += (intensityOffsetMagnitude * intensityScaleFactor);
                    break;
                case ELightMode.Dim:
                    maxIntensity -= (intensityOffsetMagnitude * intensityScaleFactor);
                    break;
            }

            if (!_isOff)
            {
                if (_flickeringRoutine == null)
                {
                    _flickeringRoutine = FlickerLight();
                    StartCoroutine(_flickeringRoutine);
                }

                if (!_previouslyOn && _lightAudioSource) _lightAudioSource.Play();
                _previouslyOn = true;
            }
            else
            {
                if (_previouslyOn && _doorFailed && _failAudioSource) _failAudioSource.Play();
                if (_previouslyOn && _lightAudioSource) _lightAudioSource.Stop();
                TurnOff();
                _previouslyOn = false;
            }

            if (useSparkEffect && shouldTryEffect) HandleSparkEffect();
        }

        private void StartLightRoutine(IEnumerator routine)
        {
            _currentRoutine = routine;
            StartCoroutine(routine);
        }

        private IEnumerator SwitchLightAfterTime(bool isOff)
        {
            float randomDelay = UnityEngine.Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(randomDelay);
            _currentRoutine = null;
            _isOff = !isOff;
        }

        private IEnumerator BlinkLight(bool isOff)
        {
            yield return new WaitForSeconds(blinkTime);
            _currentRoutine = null;
            _isOff = !isOff;
        }

        private IEnumerator FlickerLight()
        {
            float targetIntensity = GetRandomIntensity(minIntensity, maxIntensity);
            float startIntensity = _lightComp.intensity;
            float elapsed = 0f;

            while (elapsed < flickerTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flickerTime;
                _lightComp.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                yield return null;
            }

            _lightComp.intensity = targetIntensity;
            _flickeringRoutine = null;
        }

        private float GetRandomIntensity(float min, float max) => UnityEngine.Random.Range(min, max);
        private void SetLightColor(Color color) => _lightComp.color = color;

        private void HandleSparkEffect()
        {
            if (!_isOff || !_particleSystem) return;
            if (_particleSystem.isPlaying) _particleSystem.Stop();
            _particleSystem.Play();
        }

        #region Public API
        public void ChangeLightMode(ELightMode newMode)
        {
            _lightMode = newMode;
            _isOff = false;
        }

        public void TurnOff()
        {
            _lightComp.intensity = 0f;
            _isOff = true;
        }

        public void TurnOn()
        {
            _lightComp.intensity = _normalIntensity;
            _isOff = false;
        }
        #endregion

        #region Photon
        // Photon sincronización de estado
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext((int)_lightMode);
                stream.SendNext(_isOff);
                stream.SendNext(_lightComp.intensity);
                stream.SendNext(_lightComp.color);
            }
            else
            {
                _lightMode = (ELightMode)(int)stream.ReceiveNext();
                _isOff = (bool)stream.ReceiveNext();
                _lightComp.intensity = (float)stream.ReceiveNext();
                _lightComp.color = (Color)stream.ReceiveNext();
            }
        }
        #endregion
    }
}