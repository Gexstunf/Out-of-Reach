using Characters.PlayerController.Scripts.Input;
using Items.Scripts;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.XR;
using static UnityEditor.Progress;

namespace Characters.PlayerController.Scripts
{
    /// <summary>
    /// HandGrabber: clear, robust implementation for two hands.
    /// - global single item (_currentItem)
    /// - per-hand walls
    /// - managed coroutines (no self-stopping)
    /// - consistent IGrabbableScript API usage
    /// </summary>
    public class HandGrabberScript : MonoBehaviour
    {
        [Header("Settings")]
        public Transform leftGrabOrigin;
        public Transform rightGrabOrigin;
        [SerializeField] private Transform camTransform;
        [SerializeField] private bool useIndependentOrigins = false;
        [SerializeField] private LayerMask grabbableMask;
        [SerializeField] private float itemGrabDistance = 2f;
        [SerializeField] private float wallGrabDistance = 2f;
        [SerializeField] private float grabSpeed = 3f;
        [SerializeField] private float pushForce = 5f;                   
        [SerializeField] private bool debug;

        [Header("References")]
        [SerializeField] private PlayerInputScript input;
        [SerializeField] private Rig grabRig; // optional, to blend weights
        [SerializeField] private Rigidbody rb;
        [SerializeField] private PlayerInventoryPhoton inventory;

        [Header("Left Hand")]
        [SerializeField] private Transform leftHandIKTarget;
        [SerializeField] private Rigidbody leftHandRb;
        [SerializeField] private TwoBoneIKConstraint leftIKConstraint;

        [Header("Right Hand")]
        [SerializeField] private Transform rightHandIKTarget;
        [SerializeField] private Rigidbody rightHandRb;
        [SerializeField] private TwoBoneIKConstraint rightIKConstraint;

        // --- internal state ---
        public ItemGrabbableScript currentItem = null;     // only one item in the world can be held at once
        public HandData itemHoldingHand = null;         // which hand holds the item

        private HandData _leftHand;
        private HandData _rightHand;

        // --- HandData encapsulates per-hand state ---
        public class HandData
        {
            public Transform IKTarget;
            public Rigidbody Rb;
            public TwoBoneIKConstraint IKConstraint;
            public Vector3 LocalHomePosition; // local to IKTarget.parent
            public IGrabbableScript CurrentGrabbable; // wall grabbable when holding a wall
            public Coroutine ActiveCoroutine;
            public Vector3 CurrentTargetPos;
            public bool PrevPressed;

            public bool IsBusy => ActiveCoroutine != null;
            public bool HoldingItem; // true if this hand currently holds the global item

            public HandData(Transform ikTarget, Rigidbody rb, TwoBoneIKConstraint constraint, Vector3 homeLocal)
            {
                IKTarget = ikTarget;
                Rb = rb;
                IKConstraint = constraint;
                LocalHomePosition = homeLocal;
                CurrentTargetPos = ikTarget != null ? ikTarget.position : Vector3.zero;
            }
        }

        // -------------------------
        // Unity lifecycle
        // -------------------------
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            // initialize hand objects and home positions
            var leftHome = leftHandIKTarget.localPosition;
            var rightHome = rightHandIKTarget.localPosition;

            _leftHand = new HandData(leftHandIKTarget, leftHandRb, leftIKConstraint, leftHome);
            _rightHand = new HandData(rightHandIKTarget, rightHandRb, rightIKConstraint, rightHome);

            // initialize current target positions so we don't snap to Vector3.zero
            _leftHand.CurrentTargetPos = _leftHand.IKTarget.position;
            _rightHand.CurrentTargetPos = _rightHand.IKTarget.position;

            // ensure initial IK weights are correct
            if (grabRig != null) grabRig.weight = 1f;
            if (leftIKConstraint != null) leftIKConstraint.weight = 0f;
            if (rightIKConstraint != null) rightIKConstraint.weight = 0f;

            if (inventory == null)
                inventory = GetComponent<PlayerInventoryPhoton>();
        }

        private void OnDisable()
        {
            // cleanup any running coroutines and release any grabbed objects
            StopAndClearHand(_leftHand);
            StopAndClearHand(_rightHand);

            if (currentItem != null)
            {
                // find which hand held it and release cleanly
                if (itemHoldingHand != null)
                {
                    currentItem.Release();
                    itemHoldingHand.HoldingItem = false;
                }
                currentItem = null;
                itemHoldingHand = null;
            }
        }

        private void Update()
        {
            // handle per-hand input edges
            ProcessHandInput(_leftHand, input.LeftClickPressed);
            ProcessHandInput(_rightHand, input.RightClickPressed);

            // global release: if no buttons pressed and an item is held, release item
            if (!input.LeftClickPressed && !input.RightClickPressed && currentItem != null)
            {
                OnItemReleased();
            }

            // update IK target transforms and weights (visual smoothing)
            UpdateIKWeightsAndTargets();
        }
        
        private void FixedUpdate() {
            // left or right hand holding a wall?
            bool holdingWall = (_leftHand.CurrentGrabbable != null || _rightHand.CurrentGrabbable != null);
            
            if (holdingWall && input.PropelPressed) {
                Vector3 pushDir = camTransform.forward;   
                Debug.Log("Adding focre");
                rb.AddForce(pushDir * pushForce, ForceMode.Acceleration);
            }
        }

        // -------------------------
        // Core input / per-hand logic
        // -------------------------
        private void ProcessHandInput(HandData hand, bool isPressed)
        {
            // capture previous pressed inside HandData to avoid external flags
            bool wasPressed = hand.PrevPressed;

            // handle press-down edge
            if (isPressed && !wasPressed)
            {
                // only allow grab presses if the hand is not busy
                if (!hand.IsBusy)
                {
                    TryGrab(hand);
                }
            }

            // handle release edge
            if (!isPressed && wasPressed)
            {
                // If this hand holds the global item -> release the item
                if (hand.HoldingItem && currentItem != null)
                {
                    OnItemReleased();
                }
                // Otherwise, if this hand currently has a wall grabbable -> release it
                else if (hand.CurrentGrabbable != null)
                {
                    ReleaseWall(hand);
                }
                // else, if the hand was moving back to home (coroutine), we may want to stop it and start return
                else
                {
                    // if nothing, ensure IK returns home
                    StartHandCoroutine(hand, ReachToHomeRoutine(hand));
                }
            }

            // update prev state
            hand.PrevPressed = isPressed;

            // always ensure visual IK target is set
            if (hand.IKTarget != null)
                hand.IKTarget.position = hand.CurrentTargetPos;
        }

        // -------------------------
        // TryGrab
        // -------------------------
        private void TryGrab(HandData hand)
        {
            Transform origin = useIndependentOrigins ? (hand == _leftHand ? leftGrabOrigin : rightGrabOrigin) : camTransform;
            if (origin == null)
            {
                Debug.LogWarning("HandGrabber: origin is null.");
                return;
            }

            // 1) Try item raycast (priority to items) using itemGrabDistance
            if (Physics.Raycast(origin.position, origin.forward, out var hitItem, itemGrabDistance, grabbableMask))
            {
                var grabbable = hitItem.collider.GetComponent<IGrabbableScript>();
                if (grabbable != null && IsItemGrabbable(grabbable))
                {
                    // only one item allowed
                    if (currentItem == null)
                    {
                        // stop any running hand coroutine and start grab flow
                        StopAndClearHand(hand);

                        hand.CurrentGrabbable = null; // clear wall slot for now
                        StartHandCoroutine(hand, ItemGrabFlow(grabbable, hitItem.point, hand));
                        // ItemGrabFlow will set _currentItem and hand.HoldingItem when grabbed
                    }
                    return;
                }
            }

            // 2) Try wall raycast (wallGrabDistance)
            if (Physics.Raycast(origin.position, origin.forward, out var hitWall, wallGrabDistance, grabbableMask))
            {
                var grabbable = hitWall.collider.GetComponent<IGrabbableScript>();
                if (grabbable != null && IsWallGrabbable(grabbable))
                {
                    // per-hand only if free
                    if (hand.CurrentGrabbable == null)
                    {
                        StopAndClearHand(hand);
                        StartHandCoroutine(hand, WallGrabFlow(grabbable, hitWall.point, hand));
                    }
                }
            }
        }

        // -------------------------
        // Grab flows / coroutines
        // -------------------------
        // Managed Start wrapper - ensures ActiveCoroutine cleared at the end
        private Coroutine StartHandCoroutine(HandData hand, IEnumerator routine)
        {
            // stop existing coroutine first
            StopAndClearHand(hand);

            IEnumerator Managed()
            {
                hand.ActiveCoroutine = StartCoroutine(routine);
                // Wait until inner coroutine completes
                yield return hand.ActiveCoroutine;
                // clear reference (safe cleanup)
                hand.ActiveCoroutine = null;
            }

            // Start the manager coroutine (so hand.ActiveCoroutine is set inside Managed)
            hand.ActiveCoroutine = StartCoroutine(Managed());
            return hand.ActiveCoroutine;
        }

        private void StopAndClearHand(HandData hand)
        {
            if (hand.ActiveCoroutine != null)
            {
                StopCoroutine(hand.ActiveCoroutine);
                hand.ActiveCoroutine = null;
            }
        }

        // Item flow: reach -> attach object to hand (object side joint) -> return hand to home and release visual weight
        private IEnumerator ItemGrabFlow(IGrabbableScript item, Vector3 hitPoint, HandData hand)
        {
            // reach to the item's handle or hit point
            Vector3 grabPoint = item.GrabHandle != null ? item.GrabHandle.position : hitPoint;
            yield return ReachToPointRoutine(grabPoint, hand, targetWeight: 1f);

            item.Grab(hand.Rb, grabPoint);

            // register global item owner             
            // call grabbable API - connect object to hand
            RegisterAndGrabItem(item as ItemGrabbableScript, hand, grabPoint);

            // return hand to its home and reduce IK weight
            yield return ReachToHomeRoutine(hand);

            // Leave hand.HoldingItem true until OnItemReleased is called externally
        }

        public void RegisterAndGrabItem(ItemGrabbableScript item, HandData hand, Vector3 grabPoint)
        {
            item.Grab(hand.Rb, grabPoint);
            RegisterItemGrab(item as ItemGrabbableScript, hand);
        }

        private void RegisterItemGrab(ItemGrabbableScript item, HandData hand)
        {
            currentItem = item as ItemGrabbableScript;
            itemHoldingHand = hand;
            hand.HoldingItem = true;
        }

        // Wall flow: reach -> call grabbable grab (likely creates joint on hand/world) -> keep IK weight
        private IEnumerator WallGrabFlow(IGrabbableScript wall, Vector3 hitPoint, HandData hand)
        {
            yield return ReachToPointRoutine(hitPoint, hand, targetWeight: 1f);

            // attach wall - the grabbable decides how it anchors (it should create/destroy its own joint)
            wall.Grab(rb, hitPoint);

            // store per-hand grabbable so release knows which to call
            hand.CurrentGrabbable = wall;

            // keep hand IK weight at 1 while held; coroutine ends here (hand.ActiveCoroutine cleared by StartHandCoroutine manager)
        }

        // Reach helper (smoothly move IK target and IK weight)
        private IEnumerator ReachToPointRoutine(Vector3 targetPoint, HandData hand, float targetWeight)
        {
            Vector3 startPos = hand.IKTarget.position;
            float startWeight = hand.IKConstraint != null ? hand.IKConstraint.weight : 0f;
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * grabSpeed;
                float t = Mathf.Clamp01(elapsed);
                hand.IKTarget.position = Vector3.Lerp(startPos, targetPoint, t);
                if (hand.IKConstraint != null)
                    hand.IKConstraint.weight = Mathf.Lerp(startWeight, targetWeight, t);
                hand.CurrentTargetPos = hand.IKTarget.position;
                yield return null;
            }

            // ensure final values
            hand.IKTarget.position = targetPoint;
            if (hand.IKConstraint != null) hand.IKConstraint.weight = targetWeight;
            hand.CurrentTargetPos = targetPoint;
        }

        private IEnumerator ReachToHomeRoutine(HandData hand)
        {
            Vector3 homeWorld = hand.IKTarget.parent.TransformPoint(hand.LocalHomePosition);
            yield return ReachToPointRoutine(homeWorld, hand, targetWeight: 0f);
        }

        // -------------------------
        // Release helpers
        // -------------------------
        private void ReleaseWall(HandData hand)
        {
            // Stop any reach coroutines then release
            StopAndClearHand(hand);

            if (hand.CurrentGrabbable != null)
            {
                hand.CurrentGrabbable.Release();
                hand.CurrentGrabbable = null;
            }

            // send hand back home visually
            StartHandCoroutine(hand, ReachToHomeRoutine(hand));
        }

        public void OnItemReleased()
        {
            if (currentItem == null) return;

            // find the hand that holds it
            if (itemHoldingHand != null)
            {
                // let grabbable handle release
                currentItem.Release();

                // clear hand state
                itemHoldingHand.HoldingItem = false;
                itemHoldingHand = null;
            }

            currentItem = null;
        }

        /// <summary>
        /// Pide al HandGrabber que agarre una instancia ya existente en escena (por ejemplo, creada por Photon).
        /// Usa la mano más cercana al objeto (o la derecha por defecto) y lanza el flujo de ItemGrabFlow
        /// para que la mano haga el reach & grab exactamente como si el jugador la hubiera tomado desde el mundo.
        /// </summary>
        public void GrabNetworkedObject(GameObject networkedObject)
        {
            if (networkedObject == null) return;
            var grabbable = networkedObject.GetComponent<IGrabbableScript>();
            if (grabbable == null) return;

            // decidir mano: preferencia por la mano que esté libre y más cercana al objeto
            HandData chosen = null;
            float leftDist = (_leftHand != null && _leftHand.IKTarget != null) ? Vector3.Distance(_leftHand.IKTarget.position, networkedObject.transform.position) : float.MaxValue;
            float rightDist = (_rightHand != null && _rightHand.IKTarget != null) ? Vector3.Distance(_rightHand.IKTarget.position, networkedObject.transform.position) : float.MaxValue;

            // preferir mano más cercana si está libre, si no, la otra
            if (_rightHand != null && !_rightHand.IsBusy && !_rightHand.HoldingItem) chosen = _rightHand;
            if (chosen == null && _leftHand != null && !_leftHand.IsBusy && !_leftHand.HoldingItem) chosen = _leftHand;

            // si ambas ocupadas, escoger la más cercana
            if (chosen == null)
            {
                chosen = (leftDist <= rightDist) ? _leftHand : _rightHand;
            }

            // Si sigue nulo, fallback a derecha
            if (chosen == null) chosen = _rightHand ?? _leftHand;

            // Asegurarse de parar cualquier cosa y arrancar el flujo como si el jugador hubiera reachado al objeto
            StopAndClearHand(chosen);
            StartHandCoroutine(chosen, ItemGrabFlow(grabbable, networkedObject.transform.position, chosen));
        }


        // -------------------------
        // IK smoothing / visuals
        // -------------------------
        private void UpdateIKWeightsAndTargets()
        {
            // left
            if (_leftHand.CurrentGrabbable == null && !_leftHand.HoldingItem && _leftHand.ActiveCoroutine == null)
            {
                if (_leftHand.IKConstraint != null)
                    _leftHand.IKConstraint.weight = Mathf.Lerp(_leftHand.IKConstraint.weight, 0f, Time.deltaTime * grabSpeed);
            }

            // right
            if (_rightHand.CurrentGrabbable == null && !_rightHand.HoldingItem && _rightHand.ActiveCoroutine == null)
            {
                if (_rightHand.IKConstraint != null)
                    _rightHand.IKConstraint.weight = Mathf.Lerp(_rightHand.IKConstraint.weight, 0f, Time.deltaTime * grabSpeed);
            }
        }

        // -------------------------
        // Utilities
        // -------------------------
        private bool IsItemGrabbable(IGrabbableScript g)
        {
            return g is Items.Scripts.ItemGrabbableScript || g.IsItem; // try both patterns; if your IGrabbable has IsItem, use it
        }

        private bool IsWallGrabbable(IGrabbableScript g)
        {
            return g is Environment.Scripts.WallGrabbableScript || !IsItemGrabbable(g);
        }

        // OnDrawGizmos for debugging
        private void OnDrawGizmos()
        {
            if (!debug) return;

            if (camTransform != null)
            {
                Debug.DrawRay(camTransform.position, camTransform.forward * itemGrabDistance, Color.yellow);
                Debug.DrawRay(camTransform.position, camTransform.forward * wallGrabDistance, Color.red);
            }

            if (leftGrabOrigin != null)
            {
                Debug.DrawRay(leftGrabOrigin.position, leftGrabOrigin.forward * itemGrabDistance, Color.cyan);
            }

            if (rightGrabOrigin != null)
            {
                Debug.DrawRay(rightGrabOrigin.position, rightGrabOrigin.forward * itemGrabDistance, Color.cyan);
            }
        }
    }
}
