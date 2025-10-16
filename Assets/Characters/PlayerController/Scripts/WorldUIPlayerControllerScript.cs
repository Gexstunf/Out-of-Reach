using System;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class WorldUIPlayerControllerScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HandGrabberScript handGrabberScript;
        [SerializeField] private Transform uiInteractionOrigin;
        
        [Header("Settings")]
        [SerializeField] private float uiInteractionDistance = 5f;
        [SerializeField] private LayerMask uiInteractionLayer;

        private void Start() {
            handGrabberScript = GetComponent<HandGrabberScript>();
            
            if (handGrabberScript) {
                uiInteractionDistance = handGrabberScript.itemGrabDistance;
                uiInteractionLayer = handGrabberScript.grabbableMask;
            }
        }

        public void Update() {
            if (Physics.Raycast(uiInteractionOrigin.position, uiInteractionOrigin.forward, out RaycastHit hit, uiInteractionDistance, uiInteractionLayer)) 
            {
                // if (handGrabberScript.IsItemGrabbable()) {
                //     
                // }
            }
        }
    }
}
 