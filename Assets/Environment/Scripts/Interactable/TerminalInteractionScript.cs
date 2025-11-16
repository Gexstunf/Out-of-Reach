using System;
using System.Collections;
using System.Numerics;
using Characters.PlayerController.Scripts;
using UI.Scripts;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Environment.Scripts.Interactable {
    public class TerminalInteractionScript : InteractionObjectScript
    {
    
        [Header("References")]
        public Camera mainCamera;
        public Transform screenTransform;
        public Transform targetTransform;
        public TerminalControllerScript terminalController;
        
        [Header("Settings")]
        public float transitionSpeed = 1f;
        
        private Transform _camTransform;
        private Vector3 _startCamPosition;
        private Quaternion _startCamRotation;

        private bool _movingToTarget;
        private bool _movingBack;
        private float _t;
        
        private InteractableControllerScript _interactableController;
        
        void Awake() {
            mainCamera = Camera.main;
            if (mainCamera) _camTransform = mainCamera.transform;
        }

        // Update is called once per frame
        void Update()
        {
            if (_movingToTarget)
                MoveCamera(_startCamPosition, targetTransform.position, _startCamRotation,
                    Quaternion.LookRotation(screenTransform.position - targetTransform.position));
            else if (_movingBack)
                MoveCamera(targetTransform.position, _startCamPosition,
                    Quaternion.LookRotation(screenTransform.position - targetTransform.position),
                    _startCamRotation);
        }
        
        public override void StartInteraction(InteractableControllerScript controller) {
            terminalController = FindFirstObjectByType<TerminalControllerScript>();
            _interactableController = controller;
            
            Debug.Log("Interacting with terminal!");
            if (!_camTransform) _camTransform = Camera.main.transform;
            _interactableController.SetActivePlayerControls(false);
            _startCamPosition = _camTransform.position;
            _startCamRotation = _camTransform.rotation;
            _t = 0f;
            
            terminalController.OpenTerminal();
            _movingToTarget = true;
        }
        
        /*
        private void SetActivePlayerControls(bool active) {
            playerInput.enabled = active;
            playerController.enabled = active;
            playerController.SetKinematic(!active);
        }
        */
        public override IEnumerator QuitInteraction() {
            _t = 0f;
            _movingToTarget = false;
            _movingBack = true;
            terminalController.CloseTerminal();

            yield return new WaitUntil(() => !_movingBack); // this should wait until _movingBack is false (means that camera finished coming back)
            _interactableController.SetActivePlayerControls(true);
        }
        
        private void MoveCamera(Vector3 startPos, Vector3 targetPos, Quaternion startRot, Quaternion targetRot)
        {
            _t += Time.deltaTime * transitionSpeed;

            _camTransform.position = Vector3.Lerp(startPos, targetPos, _t);
            _camTransform.rotation = Quaternion.Slerp(startRot, targetRot, _t);

            // Stop movement when done
            if (_t >= 1f)
            {
                _movingToTarget = false;
                _movingBack = false;
                _t = 0f;
            }
        }
    }
}
