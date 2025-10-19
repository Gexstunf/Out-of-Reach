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
        [SerializeField] private Transform uiInteractionOrigin;
        
        [Header("Settings")]
        [SerializeField] private float uiInteractionDistance = 5f;
        [SerializeField] private LayerMask uiInteractionLayer;

        private bool _openedUi;
        private void Start() {
            handGrabberScript = GetComponent<HandGrabberScript>();
            
            if (handGrabberScript) {
                uiInteractionDistance = handGrabberScript.itemGrabDistance;
                uiInteractionLayer = handGrabberScript.grabbableMask;
            }
        }

        public void Update() {
            if (Physics.Raycast(uiInteractionOrigin.position, uiInteractionOrigin.forward, out RaycastHit hit, uiInteractionDistance, uiInteractionLayer)) {
                var grabbable = hit.collider.gameObject.GetComponent<IGrabbableScript>();
                if (grabbable != null) {
                    if (handGrabberScript.IsItemGrabbable(grabbable)) {
                        var item = hit.collider.gameObject.GetComponent<ItemGrabbableScript>();
                        _openedUi = true;
                        worldUIManagerScript.ShowPrice(hit.collider.transform, item.data.value);
                    }
                }
            }
            else {
                if (_openedUi) {
                    _openedUi = false;
                    worldUIManagerScript.HidePrice();
                }
            }
        }
    }
}
 