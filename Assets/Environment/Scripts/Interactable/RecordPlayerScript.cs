using System.Collections;
using Characters.PlayerController.Scripts;
using GlobalUtils;
using UI.Scripts;
using UnityEngine;

namespace Environment.Scripts.Interactable {
    public class RecordPlayerScript : InteractionObjectScript
    {
        [Header("References")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private UIInteractableScript _uiInteractable;
        [SerializeField] private Transform _disc;

        [Header("Settings")]
        public float spinSpeed = 0.3f;
        
        private LoggerSO _logger;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _uiInteractable = GetComponent<UIInteractableScript>();
            _logger = LoggerSO.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            if (_audioSource.isPlaying) {
                _disc.RotateAround(_disc.position, transform.up, Time.deltaTime * spinSpeed);
            }
        }

        public override void StartInteraction(InteractableControllerScript controller) {
            if (_audioSource.isPlaying) {
                _uiInteractable.SetText("Play");
                _audioSource.Stop();
            }
            else {
                _audioSource.Play();
                _uiInteractable.SetText("Stop");
            }

            controller.ResetInteraction(); // so, this can be constantly re-interacted with
        }

        public override IEnumerator QuitInteraction() {
            _audioSource.Stop();
            yield break;
        }
    }
}
