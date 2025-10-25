using System.Collections;
using Characters.PlayerController.Scripts.Input;
using Environment.Scripts;
using Environment.Scripts.Interactable;
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


        void Awake() {
            worldUIController = GetComponent<WorldUIPlayerControllerScript>();
            playerInput = GetComponent<PlayerInputScript>();
            playerController = GetComponent<PlayerControllerScript>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _uiInteractionDistance = worldUIController.uiInteractionDistance;
            _uiInteractionLayer = worldUIController.uiInteractionLayer;
            _uiInteractionOrigin = worldUIController.uiInteractionOrigin;
        }

        // Update is called once per frame
        void Update() {
            var wantsToInteract = UnityEngine.Input.GetKeyDown(KeyCode.Z);
            var wantsToQuitInteract = UnityEngine.Input.GetKeyDown(KeyCode.Escape);

            if (Physics.Raycast(_uiInteractionOrigin.position, _uiInteractionOrigin.forward, out RaycastHit hit, _uiInteractionDistance, _uiInteractionLayer)) {
                if (!_interactableObject) {
                    // cache object when detected
                    var interactable = hit.collider.gameObject.GetComponent<InteractionObjectScript>();
                    _interactableObject = interactable;
                }
                else if (!_isInteracting & wantsToInteract) {
                    // interact when Z, only called once
                    SetActivePlayerControls(false);
                    _interactableObject.StartInteraction();
                    _isInteracting = true;
                }
            }

            if (wantsToQuitInteract) {
                if (_isInteracting) {
                    StartCoroutine(HandleQuitInteraction(_interactableObject));
                }
                
                _interactableObject = null;
                _isInteracting = false;
            }
        }

        private void SetActivePlayerControls(bool active) {
            playerInput.enabled = active;
            playerController.enabled = active;
            playerController.SetKinematic(!active);
        }

        private IEnumerator HandleQuitInteraction(InteractionObjectScript obj) {
            yield return StartCoroutine(obj.QuitInteraction());
            SetActivePlayerControls(true);
        }
    }
}
