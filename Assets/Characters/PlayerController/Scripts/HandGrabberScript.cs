using System;
using System.Collections;
using Characters.PlayerController.Scripts.Input;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.PlayerController.Scripts {
    public class HandGrabberScript : MonoBehaviour
    {
        
        [Header("Settings")]
        public float grabDistance = 2f;      // how far you can grab
        public float grabSpeed = 3f;
        public LayerMask itemMask;
        public bool debug;
        
        [Header("References")]
        [SerializeField] private PlayerInputScript input;
        public Transform rightHandIKTarget;
        public Rigidbody rightHandRb;            
        public Transform leftHandIKTarget;         
        public Rigidbody leftHandRb;             
        public Transform cameraTransform;  

        
        private GrabbableScript _current;
        private Coroutine _grabCoroutine;
        private Vector3 _leftLocalHomeIKTargetPosition;
        private Vector3 _rightLocalHomeIKTargetPosition;

        private void Start() {
            _leftLocalHomeIKTargetPosition = leftHandIKTarget.localPosition;
            _rightLocalHomeIKTargetPosition = rightHandIKTarget.localPosition;
        }

        void Update()
        {
            bool clicked = (input.LeftClickPressed || input.RightClickPressed);
            if (clicked && !_current) {
                TryGrab(input.LeftClickPressed);
            }
        }

        void TryGrab(bool isLeftClick)
        {
            RaycastHit hit;
            Vector3 origin = cameraTransform.position;
            Vector3 direction = cameraTransform.forward;

            if (Physics.Raycast(origin, direction, out hit, grabDistance, itemMask)) {
                GrabbableScript item = hit.collider.GetComponent<GrabbableScript>();
                if (item) {
                    Debug.Log("Grabbing");
                    _current = item;
                    if (_grabCoroutine != null) {
                        StopCoroutine(_grabCoroutine);
                    }
                    _grabCoroutine = StartCoroutine(GrabSequence(item, hit.point, isLeftClick));
                }
            }
        }

        private IEnumerator GrabSequence(GrabbableScript item, Vector3 hitPoint, bool isLeftClick) {
            Transform ikTarget = isLeftClick ? leftHandIKTarget : rightHandIKTarget;
            Rigidbody rb = isLeftClick ? leftHandRb : rightHandRb;
            Vector3 currentHome = isLeftClick ? _leftLocalHomeIKTargetPosition : _rightLocalHomeIKTargetPosition;
            
            Vector3 start = ikTarget.position;
            Vector3 target = item.grabPoint ? item.grabPoint.position : hitPoint;
            float elapsed = 0f;

            while (elapsed < 1f) {
                elapsed += Time.deltaTime * grabSpeed; 
                float progress = Mathf.Clamp01(elapsed);
                LerpTargetFromTo(start, target, progress, isLeftClick);
                yield return null;
            }

            if (isLeftClick) {
                leftHandIKTarget.position = target;
            }
            else {
                rightHandIKTarget.position = target;
            }
            
            item.Grab(rb);
            
            elapsed = 0f;
            Vector3 newPos = ikTarget.position;
            while (elapsed < 1f) {
                elapsed += Time.deltaTime * grabSpeed; 
                float progress = Mathf.Clamp01(elapsed);
                LerpTargetFromTo(newPos, ikTarget.parent.TransformPoint(currentHome), progress, isLeftClick);
                yield return null;
            }
        }

        private void LerpTargetFromTo(Vector3 start, Vector3 target, float progress, bool isLeftClick) {
            if (isLeftClick) {
                leftHandIKTarget.position = Vector3.Lerp(start, target, progress);
            }
            else {
                rightHandIKTarget.position = Vector3.Lerp(start, target, progress);
            }
        }
        
        void OnDrawGizmos()
        {
            if (!debug || cameraTransform == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * grabDistance);
        }
    }
}
