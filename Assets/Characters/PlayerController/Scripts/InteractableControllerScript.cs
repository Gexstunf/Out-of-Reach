using Environment.Scripts;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    [RequireComponent(typeof(WorldUIPlayerControllerScript))]
    public class InteractableControllerScript : MonoBehaviour {
        [Header("References")] 
        public WorldUIPlayerControllerScript worldUIController;
        
        private LayerMask _uiInteractionLayer;
        private Transform _uiInteractionOrigin;
        private float _uiInteractionDistance;
        
        private InteractableObjectScript _interactableObject;
        private bool _isInteracting;


        void Awake() {
            worldUIController = GetComponent<WorldUIPlayerControllerScript>();
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
            if (Physics.Raycast(_uiInteractionOrigin.position, _uiInteractionOrigin.forward, out RaycastHit hit, _uiInteractionDistance, _uiInteractionLayer)) {
                if (!_interactableObject) {
                    var interactable = hit.collider.gameObject.GetComponent<InteractableObjectScript>();
                    _interactableObject = interactable;
                }
                else if (!_isInteracting & wantsToInteract) {
                    _interactableObject.interactableObjectSO.Interact(_interactableObject.gameObject.transform);
                    _isInteracting = true;
                }
            }
            else {
                _interactableObject = null;
                _isInteracting = false;
            }
        }
    }
}
