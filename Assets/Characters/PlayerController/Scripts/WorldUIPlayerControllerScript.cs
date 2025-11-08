using System;
using Items.Scripts;
using UI.Scripts;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class WorldUIPlayerControllerScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HandGrabberScript handGrabberScript;
        [SerializeField] private WorldUIManagerScript worldUIManagerScript;
        public Transform uiInteractionOrigin;
        
        [Header("Settings")]
        public float uiInteractionDistance = 5f;
        public LayerMask uiInteractionLayer;

        private bool _openedUi;
        private IGrabbableScript _grabbableCache;
        private UIInteractableScript _interactableCache;


        private string _prevInteractableText;
        
        private void Start() {
            handGrabberScript = GetComponent<HandGrabberScript>();
            
            if (handGrabberScript) {
                uiInteractionDistance = handGrabberScript.itemGrabDistance;
                uiInteractionLayer = handGrabberScript.grabbableMask;
            }
        }

        public void Update() {
            if (Physics.Raycast(uiInteractionOrigin.position, uiInteractionOrigin.forward, out RaycastHit hit, uiInteractionDistance, uiInteractionLayer)) {
                
                if (!_interactableCache) {
                    var interactable = hit.collider.gameObject.GetComponent<UIInteractableScript>();
                    _interactableCache = interactable;
                    if (_interactableCache) HandleAInteractable();
                }
                
                if (_grabbableCache == null) {
                    var grabbable = hit.collider.gameObject.GetComponent<IGrabbableScript>();
                    _grabbableCache = grabbable;
                    if (_grabbableCache != null) HandleAGrabbable(hit);
                }
            }
            else {
                if (_openedUi) {
                    _openedUi = false;
                    HideUIAndCleanCache();
                }
            }
        }

        private void HideUIAndCleanCache() {
            worldUIManagerScript.HidePrice();
            worldUIManagerScript.HideInteractable();
            _grabbableCache = null;
            _interactableCache = null;
        }
        
        private void HandleAGrabbable(RaycastHit hit) {
            if (_grabbableCache != null) {
                if (handGrabberScript.IsItemGrabbable(_grabbableCache) && !_openedUi) {
                    var item = hit.collider.gameObject.GetComponent<ItemGrabbableScript>();
                    _openedUi = true;
                    worldUIManagerScript.ShowPrice(hit.collider.transform, item.data.value);
                }
            }
        }
        
        private void HandleAInteractable() {
            
            var currentTxt = _interactableCache.DisplayText;

            if ((_interactableCache && !_openedUi) || _prevInteractableText != currentTxt) {
                _openedUi = true;
                _prevInteractableText = currentTxt;
                worldUIManagerScript.ShowInteractable(_interactableCache.anchorTransform, currentTxt);
            }
        }
    }
}
 