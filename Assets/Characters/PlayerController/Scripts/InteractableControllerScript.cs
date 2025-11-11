using System.Collections;
using Characters.PlayerController.Scripts.Input;
using Environment.Scripts;
using Environment.Scripts.Interactable;
using GlobalUtils;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    [RequireComponent(typeof(WorldUIPlayerControllerScript))]
    public class InteractableControllerScript : MonoBehaviour {
        
        [Header("References")] 
        public WorldUIPlayerControllerScript worldUIController;
        public PlayerInputScript playerInput;
        public PlayerControllerScript playerController;
        
        private LayerMask _uiInteractionLayer;
        private Transform _uiInteractionOrigin;
        private float _uiInteractionDistance;
        
        private InteractionObjectScript _interactableObject;
        private bool _isInteracting;

        private LoggerSO _logger;

        private void Awake() {
            worldUIController = GetComponent<WorldUIPlayerControllerScript>();
            playerInput = GetComponent<PlayerInputScript>();
            playerController = GetComponent<PlayerControllerScript>();

            _logger = LoggerSO.Instance;
        }
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start() {
            _uiInteractionDistance = worldUIController.uiInteractionDistance;
            _uiInteractionLayer = worldUIController.uiInteractionLayer;
            _uiInteractionOrigin = worldUIController.uiInteractionOrigin;
        }

        // Update is called once per frame
        private void Update() {
            var wantsToInteract = UnityEngine.Input.GetKeyDown(KeyCode.Z);
            var wantsToQuitInteract = UnityEngine.Input.GetKeyDown(KeyCode.Escape);
            
            if (!(wantsToInteract || wantsToQuitInteract)) return;

            if (Physics.Raycast(_uiInteractionOrigin.position, _uiInteractionOrigin.forward, out RaycastHit hit, _uiInteractionDistance, _uiInteractionLayer)) {
                if (!_interactableObject) {
                    // cache object when detected
                    _logger.LogMinor("Cached interactable");
                    var interactable = hit.collider.gameObject.GetComponent<InteractionObjectScript>();
                    _interactableObject = interactable;
                }
                
                if (!_isInteracting & wantsToInteract) {
                    // interact when Z, only called once
                    //SetActivePlayerControls(false);
                    _logger.Log("Starting interaction");
                    _isInteracting = true;
                    _interactableObject.StartInteraction(this);
                }
            }

            if (wantsToQuitInteract) {
                if (_isInteracting) {
                    StartCoroutine(HandleQuitInteraction(_interactableObject));
                }

                ResetInteraction();
            }
        }

        private IEnumerator HandleQuitInteraction(InteractionObjectScript obj) {
            yield return StartCoroutine(obj.QuitInteraction());
            //SetActivePlayerControls(true);
        }


        public void ResetInteraction() {
            _logger.Log("Reset interaction");
            _isInteracting = false;
            _interactableObject = null;
        }
        
        
        public void SetActivePlayerControls(bool active) {
            playerInput.enabled = active;
            playerController.enabled = active;
            playerController.SetKinematic(!active);
        }
    }
}
