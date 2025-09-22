using System;
using System.Collections;
using Characters.PlayerController.Scripts.Input;
using Environment.Scripts;
using Items.Scripts;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

namespace Characters.PlayerController.Scripts {
    public class HandGrabberScript : MonoBehaviour {

        [Header("Settings")] 
        [SerializeField] private Transform leftGrabOrigin;
        [SerializeField] private Transform rightGrabOrigin;
        public float itemGrabDistance = 2f; // how far you can grab
        public float wallGrabDistance = 1f; // how far you can grab
        public float grabSpeed = 3f;
        public LayerMask grabbableMask;
        public bool debug;
        public bool useIndependentOrigins = false;

        [Header("References")] [SerializeField]
        private PlayerInputScript input;

        public Transform rightHandIKTarget;
        public Rigidbody rightHandRb;
        public Transform leftHandIKTarget;
        public Rigidbody leftHandRb;
        public Transform camTransform;


        [Header("Rig Settings")] 
        public Rig grabRig;
        public TwoBoneIKConstraint rightIKConstraint;
        public TwoBoneIKConstraint leftIKConstraint;

        
        private IGrabbableScript _leftWall;
        private IGrabbableScript _rightWall;
        private IGrabbableScript _currentItem;
        

        private Coroutine _grabCoroutine;
        private Vector3 _leftLocalHomeIKTargetPosition;
        private Vector3 _rightLocalHomeIKTargetPosition;
        
        private bool _prevLeftPressed = false;
        private bool _prevRightPressed = false;
        private bool _itemHeldByLeft = false;


        private void Start() {
            _leftLocalHomeIKTargetPosition = leftHandIKTarget.localPosition;
            _rightLocalHomeIKTargetPosition = rightHandIKTarget.localPosition;

            grabRig.weight = 1f;
            leftIKConstraint.weight = 0f;
            rightIKConstraint.weight = 0f;
        }

        void Update() {
            bool bothClicked = (input.LeftClickPressed && input.RightClickPressed);
            bool clicked = (input.LeftClickPressed || input.RightClickPressed);
            bool handAvailable = (_leftWall == null || _rightWall == null);

            // ===== ITEM GRAB / RELEASE =====
            if (clicked && _currentItem == null && handAvailable) {
                // try to grab an item if none is held
                OnGrabPressed(input.LeftClickPressed);
            }

            if (!clicked && _currentItem != null) {
                // release the current item when no grab input is held
                OnItemReleased();
            }

            // ===== WALL GRAB / RELEASE =====
            if (input.LeftClickPressed && _leftWall == null && _currentItem == null) {
                OnGrabPressed(true);
            }
            else if (!input.LeftClickPressed && _leftWall != null) {
                OnGrabReleased(true);  
            }

            if (input.RightClickPressed && _rightWall == null && _currentItem == null) {
                OnGrabPressed(false); 
            }
            else if (!input.RightClickPressed && _rightWall != null) {
                OnGrabReleased(false); 
            }

            // ===== IK WEIGHT RESET =====
            if (_currentItem == null && _leftWall == null) {
                leftIKConstraint.weight = Mathf.Lerp(leftIKConstraint.weight, 0f, Time.deltaTime * grabSpeed);
            }

            if (_currentItem == null && _rightWall == null) {
                rightIKConstraint.weight = Mathf.Lerp(rightIKConstraint.weight, 0f, Time.deltaTime * grabSpeed);
            }
        }

        /*void TryGrab(bool isLeftHand) {
            RaycastHit hit;
            Vector3 origin = camTransform.position;
            Vector3 direction = camTransform.forward;

            if (Physics.Raycast(origin, direction, out hit, itemGrabDistance, grabbableMask)) {
                GrabbableScript grabbable = hit.collider.GetComponent<GrabbableScript>();
                if (grabbable) {
                    ItemGrabTry(grabbable, hit.point, isLeftHand);

                    if (isLeftHand) {
                        _leftHandAvailable = false;
                    }
                    else {
                        _rightHandAvailable = false;
                    }
                    return;
                }
            }
            
            if (Physics.Raycast(origin, direction, out hit, wallGrabDistance)) {
                GrabbableScript grabbable = hit.collider.GetComponent<GrabbableScript>();
                if (hit.collider.gameObject.CompareTag("Surface")) {
                    SurfaceGrabTry(grabbable, hit.point, isLeftHand);
                    if (isLeftHand) {
                        _leftHandAvailable = false;
                    }
                    else {
                        _rightHandAvailable = false;
                    }
                    return;
                }
            }
        }*/

        // void ItemGrabTry(GrabbableScript item, Vector3 hitPoint, bool isLeftHand) {
        //     Debug.Log("Grabbing Item");
        //     _currentItem = item;
        //     if (_grabCoroutine != null) {
        //         StopCoroutine(_grabCoroutine);
        //     }
        //
        //     _grabCoroutine = StartCoroutine(GrabItemSequence(item, hitPoint, isLeftHand));
        // }

        // void SurfaceGrabTry(GrabbableScript grabbable, Vector3 hitPoint, bool isLeftHand) {
        //     Debug.Log("Grabbing Surface");
        //
        //     if (_grabCoroutine != null) {
        //         StopCoroutine(_grabCoroutine);
        //     }
        //     
        //     _grabCoroutine = StartCoroutine(GrabSurfaceSequence(grabbable, hitPoint, isLeftHand));
        // }

        private IEnumerator GrabSurfaceSequence(GrabbableScript grabbble, Vector3 hitPoint, bool isLeftHand) {
            Transform ikTarget = isLeftHand ? leftHandIKTarget : rightHandIKTarget;
            Rigidbody rb = isLeftHand ? leftHandRb : rightHandRb;
            Vector3 currentHome = isLeftHand ? _leftLocalHomeIKTargetPosition : _rightLocalHomeIKTargetPosition;
            TwoBoneIKConstraint currentConstraint = isLeftHand ? leftIKConstraint : rightIKConstraint;
            
            Vector3 start = ikTarget.position;
            Vector3 target = hitPoint;
            float elapsed = 0f;
            
            while (elapsed < 1f) {
                elapsed += Time.deltaTime * grabSpeed; 
                float progress = Mathf.Clamp01(elapsed);
                LerpTargetFromTo(start, target, progress, isLeftHand);
                currentConstraint.weight = Mathf.Lerp(currentConstraint.weight, 1f, Time.deltaTime * grabSpeed);
                yield return null;
            }
 
            if (isLeftHand) {
                leftHandIKTarget.position = target;
            }
            else {
                rightHandIKTarget.position = target;
            }
            
            grabbble.Grab(rb, target);
        }

        private IEnumerator GrabItemSequence(GrabbableScript grabbable, Vector3 hitPoint, bool isLeftHand) {
            Transform ikTarget = isLeftHand ? leftHandIKTarget : rightHandIKTarget;
            Rigidbody rb = isLeftHand ? leftHandRb : rightHandRb;
            Vector3 currentHome = isLeftHand ? _leftLocalHomeIKTargetPosition : _rightLocalHomeIKTargetPosition;
            TwoBoneIKConstraint currentConstraint = isLeftHand ? leftIKConstraint : rightIKConstraint;
            
            //reach out
            Vector3 start = ikTarget.position;
            Vector3 target = grabbable.grabPoint ? grabbable.grabPoint.position : hitPoint;
            float elapsed = 0f;
            while (elapsed < 1f) {
                elapsed += Time.deltaTime * grabSpeed; 
                float progress = Mathf.Clamp01(elapsed);
                LerpTargetFromTo(start, target, progress, isLeftHand);
                currentConstraint.weight = Mathf.Lerp(currentConstraint.weight, 1f, Time.deltaTime * grabSpeed);
                yield return null;
            }
            
            // clean the position
            if (isLeftHand) {
                leftHandIKTarget.position = target;
            }
            else {
                rightHandIKTarget.position = target;
            }
            
            grabbable.Grab(rb, Vector3.zero);
            
            // return to position
            elapsed = 0f;
            Vector3 newPos = ikTarget.position;
            while (elapsed < 1f) {
                elapsed += Time.deltaTime * grabSpeed; 
                float progress = Mathf.Clamp01(elapsed);
                LerpTargetFromTo(newPos, ikTarget.parent.TransformPoint(currentHome), progress, isLeftHand);
                currentConstraint.weight = Mathf.Lerp(currentConstraint.weight, 0f, Time.deltaTime * grabSpeed);
                yield return null;
            }
        }
        
        #region Public API

        public void OnGrabPressed(bool isLeftHand)
        {
            Transform origin = useIndependentOrigins ? 
                (isLeftHand ? leftGrabOrigin : rightGrabOrigin) : camTransform;
            if (isLeftHand)
                TryGrab(leftHandRb, origin, ref _leftWall);
            else
                TryGrab(rightHandRb, origin, ref _rightWall);
        }

        public void OnGrabReleased(bool isLeftHand)
        {
            if (isLeftHand)
                ReleaseGrab(ref _leftWall, leftHandRb);
            else
                ReleaseGrab(ref _rightWall, rightHandRb);
        }

        public void OnItemReleased()
        {
            if (_currentItem != null)
            {
                _currentItem.Release(null); // releasing from item
                _currentItem = null;
            }
        }
        
        #endregion
        
        
        private void TryGrab(Rigidbody handRb, Transform origin, ref IGrabbableScript wallSlot)
        {
            if (Physics.Raycast(origin.position, origin.forward, out var hit, itemGrabDistance, grabbableMask))
            {
                var grabbable = hit.collider.GetComponent<IGrabbableScript>();
                if (grabbable == null) return;

                // items (only 1 globally allowed)
                if (grabbable is ItemGrabbableScript) {
                    if (_currentItem == null) {
                        grabbable.Grab(handRb, hit.point);
                        _currentItem = grabbable;
                    }
                }
                else if (grabbable is WallGrabbableScript) {
                    if (wallSlot == null) {
                        grabbable.Grab(handRb, hit.point);
                        wallSlot = grabbable;
                    }
                }
            }
        }
        
        private void ReleaseGrab(ref IGrabbableScript wallSlot, Rigidbody handRb)
        {
            if (wallSlot != null)
            {
                //wallSlot.Release(handRb);
                wallSlot = null;
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
            if (!debug || camTransform == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(camTransform.position, camTransform.forward * itemGrabDistance);
            Gizmos.DrawRay(rightGrabOrigin.position, rightGrabOrigin.forward * itemGrabDistance);
            Gizmos.DrawRay(leftGrabOrigin.position, leftGrabOrigin.forward * itemGrabDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(camTransform.position, camTransform.forward * wallGrabDistance);

        }
    }
}
