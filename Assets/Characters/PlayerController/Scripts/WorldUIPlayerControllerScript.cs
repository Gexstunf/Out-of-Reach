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
                   
        
        private void Start() {
            handGrabberScript = GetComponent<HandGrabberScript>();
            
            if (handGrabberScript) {
                uiInteractionDistance = handGrabberScript.itemGrabDistance;
                uiInteractionLayer = handGrabberScript.grabbableMask;
            }
        }

        public void Update() {
            if (Physics.Raycast(uiInteractionOrigin.position, uiInteractionOrigin.forward, out RaycastHit hit, uiInteractionDistance, uiInteractionLayer)) {
                HandleGrabbable(hit);
                HandleInteractable(hit);
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
        
        private void HandleGrabbable(RaycastHit hit) {
            if (_grabbableCache == null) {
                var grabbable = hit.collider.gameObject.GetComponent<IGrabbableScript>();
                _grabbableCache = grabbable;
            }

            if (_grabbableCache != null) {
                if (handGrabberScript.IsItemGrabbable(_grabbableCache) && !_openedUi) {
                    var item = hit.collider.gameObject.GetComponent<ItemGrabbableScript>();
                    _openedUi = true;
                    worldUIManagerScript.ShowPrice(hit.collider.transform, item.data.value);
                }
            }
        }
        
        private void HandleInteractable(RaycastHit hit) {

            if (!_interactableCache) {
                var interactable = hit.collider.gameObject.GetComponent<UIInteractableScript>();
                _interactableCache = interactable;
            }

            if (_interactableCache && !_openedUi) {
                _openedUi = true;
                worldUIManagerScript.ShowInteractable(_interactableCache.anchorTransform, _interactableCache.interactText);
            }
        }
    }
}
 